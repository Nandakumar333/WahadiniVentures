# Stripe Subscription Setup Guide

This guide walks you through setting up the Stripe subscription system for WahadiniCryptoQuest.

## Prerequisites

- Stripe account (create at [stripe.com](https://stripe.com))
- .NET 8 SDK installed
- Node.js 18+ installed
- PostgreSQL 15+ running

## 1. Stripe Dashboard Configuration

### 1.1 Create Stripe Account & Get API Keys

1. Go to [Stripe Dashboard](https://dashboard.stripe.com)
2. Navigate to **Developers > API Keys**
3. Copy your **Publishable key** (starts with `pk_test_`)
4. Copy your **Secret key** (starts with `sk_test_`)
5. Keep these secure - you'll need them in the next steps

### 1.2 Create Products and Prices

1. Navigate to **Products** in Stripe Dashboard
2. Click **Add product**

**Monthly Premium Product:**
- Name: `Monthly Premium`
- Description: `Premium subscription with monthly billing`
- Click **Save product**
- Under **Pricing**, click **Add another price**:
  - USD: $9.99/month (recurring)
  - EUR: €8.99/month (recurring)
  - GBP: £7.99/month (recurring)
  - INR: ₹799/month (recurring)
  - JPY: ¥999/month (recurring)

**Yearly Premium Product:**
- Name: `Yearly Premium`
- Description: `Premium subscription with annual billing - Save 17%!`
- Click **Save product**
- Under **Pricing**, click **Add another price**:
  - USD: $99.99/year (recurring)
  - EUR: €89.99/year (recurring)
  - GBP: £79.99/year (recurring)
  - INR: ₹7,999/year (recurring)
  - JPY: ¥9,999/year (recurring)

**Important:** Copy the Price IDs (e.g., `price_1234...`) for each currency/tier combination. You'll need these in `appsettings.json`.

### 1.3 Configure Webhook Endpoint

1. Navigate to **Developers > Webhooks**
2. Click **Add endpoint**
3. **Endpoint URL**: `https://yourdomain.com/api/webhooks/stripe` (use ngrok for local testing)
4. **Events to listen for**:
   - `checkout.session.completed`
   - `invoice.payment_succeeded`
   - `invoice.payment_failed`
   - `customer.subscription.updated`
   - `customer.subscription.deleted`
5. Click **Add endpoint**
6. Copy the **Signing secret** (starts with `whsec_`)

## 2. Backend Configuration

### 2.1 Update appsettings.json

Edit `backend/src/WahadiniCryptoQuest.API/appsettings.json`:

```json
{
  "Stripe": {
    "SecretKey": "sk_test_your_secret_key_here",
    "WebhookSecret": "whsec_your_webhook_secret_here",
    "MonthlyPrices": {
      "USD": "price_1234567890_monthly_usd",
      "EUR": "price_1234567890_monthly_eur",
      "GBP": "price_1234567890_monthly_gbp",
      "INR": "price_1234567890_monthly_inr",
      "JPY": "price_1234567890_monthly_jpy"
    },
    "YearlyPrices": {
      "USD": "price_0987654321_yearly_usd",
      "EUR": "price_0987654321_yearly_eur",
      "GBP": "price_0987654321_yearly_gbp",
      "INR": "price_0987654321_yearly_inr",
      "JPY": "price_0987654321_yearly_jpy"
    },
    "SuccessUrl": "http://localhost:5173/subscription/success?session_id={CHECKOUT_SESSION_ID}",
    "CancelUrl": "http://localhost:5173/subscription/cancel"
  }
}
```

**Production:** Use environment variables or Azure Key Vault instead of hardcoding secrets.

### 2.2 Apply Database Migration

```bash
cd backend
dotnet ef database update --project src/WahadiniCryptoQuest.DAL --startup-project src/WahadiniCryptoQuest.API
```

### 2.3 Seed Currency Pricing Data

Run the seed script to populate currency pricing:

```bash
# Windows (PowerShell)
cd backend
Get-Content seed_currency_pricing.sql | psql -U postgres -d wahadinicryptoquest

# Linux/Mac
cd backend
psql -U postgres -d wahadinicryptoquest -f seed_currency_pricing.sql
```

This creates 5 currency pricings (USD, EUR, GBP, INR, JPY) with proper formatting rules.

## 3. Frontend Configuration

### 3.1 Create .env File

Copy `.env.example` to `.env`:

```bash
cd frontend
cp .env.example .env
```

Edit `.env`:

```env
VITE_API_BASE_URL=http://localhost:5000
VITE_STRIPE_PUBLISHABLE_KEY=pk_test_your_publishable_key_here
```

### 3.2 Install Dependencies

```bash
cd frontend
npm install
```

## 4. Testing Locally

### 4.1 Set Up Stripe CLI (for webhook testing)

1. Install [Stripe CLI](https://stripe.com/docs/stripe-cli)
2. Login: `stripe login`
3. Forward webhooks to local API:

```bash
stripe listen --forward-to http://localhost:5000/api/webhooks/stripe
```

4. Copy the webhook signing secret from output and update `appsettings.json`

### 4.2 Start Backend

```bash
cd backend/src/WahadiniCryptoQuest.API
dotnet run
```

API should be running at `http://localhost:5000`

### 4.3 Start Frontend

```bash
cd frontend
npm run dev
```

Frontend should be running at `http://localhost:5173`

### 4.4 Test Checkout Flow

1. Navigate to `http://localhost:5173/pricing`
2. Select a plan (Monthly or Yearly)
3. Use Stripe test card: `4242 4242 4242 4242`
   - Expiry: Any future date
   - CVC: Any 3 digits
   - ZIP: Any 5 digits
4. Complete checkout
5. Verify redirect to success page
6. Check database: `Subscriptions` table should have new record

### 4.5 Test Webhook Processing

1. Ensure Stripe CLI is forwarding webhooks
2. Complete a checkout
3. Check terminal logs for webhook events
4. Verify `WebhookEvents` table has processed events
5. Verify `SubscriptionHistory` has audit records

## 5. Stripe Test Cards

| Card Number | Scenario |
|-------------|----------|
| 4242 4242 4242 4242 | Successful payment |
| 4000 0025 0000 3155 | Requires authentication (3D Secure) |
| 4000 0000 0000 9995 | Payment declined |
| 4000 0000 0000 0341 | Charge succeeds but card issuer declines future payments |

See [Stripe Testing Guide](https://stripe.com/docs/testing) for more test cards.

## 6. Monitoring & Debugging

### 6.1 Check Subscription Status

Query database:

```sql
SELECT u.email, s.tier, s.status, s.current_period_end, s.has_premium_access
FROM subscriptions s
JOIN users u ON s.user_id = u.id
WHERE s.is_deleted = false
ORDER BY s.created_at DESC;
```

### 6.2 View Webhook Events

```sql
SELECT stripe_event_id, event_type, status, retry_count, created_at
FROM webhook_events
ORDER BY created_at DESC
LIMIT 20;
```

### 6.3 Check Subscription History

```sql
SELECT sh.change_type, sh.old_tier, sh.new_tier, sh.old_status, sh.new_status, sh.changed_at
FROM subscription_histories sh
JOIN subscriptions s ON sh.subscription_id = s.id
WHERE s.id = 'your-subscription-id'
ORDER BY sh.changed_at DESC;
```

## 7. Production Deployment

### 7.1 Update URLs

1. Update `Stripe.SuccessUrl` and `Stripe.CancelUrl` in production `appsettings.json`
2. Update webhook endpoint in Stripe Dashboard to production URL
3. Use production API keys (starts with `pk_live_` and `sk_live_`)

### 7.2 Security Checklist

- ✅ Store Stripe keys in environment variables or Azure Key Vault
- ✅ Enable HTTPS for all endpoints
- ✅ Verify webhook signatures (already implemented)
- ✅ Rate limit subscription endpoints (Phase 9)
- ✅ Monitor for unusual activity
- ✅ Set up alerts for failed payments
- ✅ Implement retry logic for webhook processing (already implemented)

### 7.3 Enable Stripe Radar

1. Navigate to **Radar** in Stripe Dashboard
2. Enable fraud protection rules
3. Review and block high-risk payments

## 8. Troubleshooting

### Issue: "Invalid API Key"

- Verify `Stripe.SecretKey` in `appsettings.json` matches your Stripe account
- Check if you're using test keys in test mode and live keys in production

### Issue: Webhooks Not Received

- Verify webhook endpoint is publicly accessible (use ngrok for local testing)
- Check webhook signing secret matches `Stripe.WebhookSecret`
- View webhook attempts in Stripe Dashboard > Developers > Webhooks

### Issue: Checkout Session Fails

- Check Stripe Dashboard > Logs for error details
- Verify Price IDs in `appsettings.json` match your Stripe products
- Ensure `SuccessUrl` and `CancelUrl` are valid URLs

### Issue: User Already Has Subscription

- This is expected behavior - users can only have one active subscription
- To test, cancel existing subscription first or use a different user account

## 9. Next Steps

Once basic subscription is working:

1. **Phase 4**: Implement webhook lifecycle automation (already in progress)
2. **Phase 5**: Add discount code redemption
3. **Phase 6**: Implement subscription management (cancel, reactivate, billing portal)
4. **Phase 7**: Build admin currency configuration UI
5. **Phase 8**: Enhance plan comparison display
6. **Phase 9**: Production hardening (rate limiting, monitoring, security)

## Support

For issues or questions:
- Stripe Support: [stripe.com/support](https://support.stripe.com)
- Stripe Documentation: [stripe.com/docs](https://stripe.com/docs)
- Project Issues: [GitHub Issues](https://github.com/your-repo/issues)
