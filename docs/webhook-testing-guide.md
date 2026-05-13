# Webhook Testing Guide

This guide explains how to test Stripe webhooks locally using the Stripe CLI.

## Prerequisites

- Stripe CLI installed ([Download](https://stripe.com/docs/stripe-cli))
- Backend API running locally
- Stripe account with test mode enabled

## Setup Stripe CLI

### 1. Install Stripe CLI

**Windows (using Scoop):**
```powershell
scoop bucket add stripe https://github.com/stripe/scoop-stripe-cli.git
scoop install stripe
```

**Mac (using Homebrew):**
```bash
brew install stripe/stripe-cli/stripe
```

**Linux:**
```bash
wget https://github.com/stripe/stripe-cli/releases/latest/download/stripe_linux_x86_64.tar.gz
tar -xvf stripe_linux_x86_64.tar.gz
sudo mv stripe /usr/local/bin/
```

### 2. Login to Stripe

```bash
stripe login
```

This will open your browser for authentication. Press Enter after authorizing.

## Local Webhook Forwarding

### 1. Start Your Backend API

```powershell
cd backend/src/WahadiniCryptoQuest.API
dotnet run
```

Verify the API is running at `http://localhost:5000`

### 2. Forward Webhooks to Local Endpoint

```bash
stripe listen --forward-to http://localhost:5000/api/webhooks/stripe
```

**Output Example:**
```
> Ready! You are using Stripe API Version [2023-10-16]. Your webhook signing secret is whsec_xxxxx
```

**Important:** Copy the webhook signing secret (`whsec_xxxxx`)

### 3. Update appsettings.json

Edit `backend/src/WahadiniCryptoQuest.API/appsettings.json`:

```json
{
  "Stripe": {
    "WebhookSecret": "whsec_xxxxx_FROM_STRIPE_CLI"
  }
}
```

**Note:** For production, use the webhook secret from Stripe Dashboard instead.

## Testing Webhook Events

### Test Checkout Session Completed

```bash
stripe trigger checkout.session.completed
```

**Expected Response:**
- Backend logs: "Received Stripe webhook: checkout.session.completed"
- Database: New record in `webhook_events` table
- Status: `Processed` if successful

### Test Invoice Payment Succeeded

```bash
stripe trigger invoice.payment_succeeded
```

**What This Tests:**
- Subscription renewal
- Activation from incomplete/past_due status
- SubscriptionHistory creation

### Test Invoice Payment Failed

```bash
stripe trigger invoice.payment_failed
```

**What This Tests:**
- Subscription marked as `PastDue`
- Grace period activated
- User still has premium access for 3 days

### Test Subscription Deleted

```bash
stripe trigger customer.subscription.deleted
```

**What This Tests:**
- Subscription marked as `Expired`
- User loses premium access
- SubscriptionHistory records expiration

## Manual Webhook Testing

### Create Real Checkout Session

1. Navigate to `http://localhost:5173/pricing`
2. Click "Get Started" on any plan
3. Use test card: `4242 4242 4242 4242`
4. Complete checkout

**Webhooks Triggered:**
1. `checkout.session.completed` - Creates subscription
2. `customer.subscription.created` - (optionally handled)
3. `invoice.payment_succeeded` - Activates subscription

### Verify in Database

```sql
-- Check webhook events
SELECT stripe_event_id, event_type, status, retry_count, created_at
FROM webhook_events
ORDER BY created_at DESC
LIMIT 10;

-- Check subscription created
SELECT u.email, s.tier, s.status, s.has_premium_access, s.current_period_end
FROM subscriptions s
JOIN users u ON s.user_id = u.id
WHERE s.is_deleted = false
ORDER BY s.created_at DESC
LIMIT 5;

-- Check subscription history
SELECT sh.change_type, sh.new_tier, sh.new_status, sh.triggered_by, sh.changed_at
FROM subscription_histories sh
ORDER BY sh.changed_at DESC
LIMIT 10;
```

## Monitoring Webhooks

### View Stripe CLI Logs

The Stripe CLI shows real-time webhook events:

```
2025-12-15 14:32:01   --> checkout.session.completed [evt_1234]
2025-12-15 14:32:01   <--  [200] POST http://localhost:5000/api/webhooks/stripe
2025-12-15 14:32:05   --> invoice.payment_succeeded [evt_5678]
2025-12-15 14:32:05   <--  [200] POST http://localhost:5000/api/webhooks/stripe
```

### Check Application Logs

Backend logs show webhook processing:

```
info: WahadiniCryptoQuest.API.Controllers.WebhooksController[0]
      Received Stripe webhook: checkout.session.completed (evt_1234)
info: WahadiniCryptoQuest.Service.Handlers.Subscription.ProcessWebhookEventHandler[0]
      Created subscription abc-123 for user xyz-789 from checkout session
```

## Troubleshooting

### Issue: "Invalid signature" Error

**Cause:** Webhook secret mismatch

**Solution:**
1. Get secret from Stripe CLI output: `stripe listen --print-secret`
2. Update `appsettings.json` with correct secret
3. Restart backend API

### Issue: Webhook Event Not Processed

**Check:**
1. Stripe CLI is forwarding to correct URL
2. Backend API is running and accessible
3. Webhook endpoint returns 200 (check logs)

**Query Failed Events:**
```sql
SELECT event_type, status, error_message, retry_count, created_at
FROM webhook_events
WHERE status = 2 -- Failed
ORDER BY created_at DESC;
```

### Issue: Duplicate Event Processing

**Expected Behavior:** Idempotency check prevents duplicates

**Verify:**
```sql
SELECT stripe_event_id, COUNT(*) as count
FROM webhook_events
GROUP BY stripe_event_id
HAVING COUNT(*) > 1;
```

Should return 0 rows (each event ID is unique).

### Issue: Webhook Times Out

**Cause:** Processing takes > 10 seconds

**Solution:** Return 200 immediately, process asynchronously
- WebhooksController already returns 200 before processing
- Background job queue implementation coming in Phase 9

## Production Webhook Setup

### 1. Create Webhook Endpoint in Stripe Dashboard

1. Go to [Stripe Dashboard > Developers > Webhooks](https://dashboard.stripe.com/webhooks)
2. Click **Add endpoint**
3. **Endpoint URL:** `https://yourdomain.com/api/webhooks/stripe`
4. **Events to listen for:**
   - `checkout.session.completed`
   - `invoice.payment_succeeded`
   - `invoice.payment_failed`
   - `customer.subscription.updated`
   - `customer.subscription.deleted`
5. Click **Add endpoint**

### 2. Copy Production Webhook Secret

1. Click on your webhook endpoint
2. Copy **Signing secret** (starts with `whsec_`)
3. Store in Azure Key Vault or environment variable

**Azure App Service:**
```bash
az webapp config appsettings set \
  --resource-group YourResourceGroup \
  --name YourAppName \
  --settings Stripe__WebhookSecret=whsec_production_secret
```

### 3. Test Production Webhooks

Use Stripe Dashboard > Webhooks > [Your Endpoint] > "Send test webhook"

## Webhook Event Reference

| Event | Trigger | Action |
|-------|---------|--------|
| `checkout.session.completed` | User completes checkout | Create subscription record |
| `invoice.payment_succeeded` | Payment successful | Activate/renew subscription |
| `invoice.payment_failed` | Payment fails | Mark subscription as past due |
| `customer.subscription.updated` | Subscription changed | Sync status from Stripe |
| `customer.subscription.deleted` | Subscription cancelled | Expire subscription |

## Retry Logic

Stripe automatically retries failed webhooks:
- First retry: After 1 hour
- Subsequent retries: Up to 3 days
- Total attempts: ~20 attempts

Your webhook endpoint should:
- ✅ Return 200 OK immediately
- ✅ Use idempotency keys (StripeEventId)
- ✅ Log failures for manual review
- ✅ Track retry count in database

## Security Best Practices

✅ **Implemented:**
- Signature verification using Stripe SDK
- Timestamp validation (built into Stripe SDK)
- Idempotency check prevents duplicate processing

⚠️ **Recommended:**
- Use HTTPS in production (required by Stripe)
- Rate limit webhook endpoint (Phase 9)
- Monitor for unusual patterns
- Set up alerts for failed webhooks

## Next Steps

After verifying webhooks work locally:
1. Test full checkout → activation flow
2. Test subscription renewal (simulate invoice.payment_succeeded)
3. Test payment failure → grace period flow
4. Deploy to staging and test with Stripe test mode
5. Configure production webhooks before going live
