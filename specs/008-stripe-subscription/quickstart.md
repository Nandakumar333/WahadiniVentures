# Quickstart Guide: Stripe Subscription Management

**Feature**: 008-stripe-subscription  
**Date**: 2025-12-09  
**Estimated Setup Time**: 30-45 minutes

## Prerequisites

### Required Accounts
- [x] Stripe Account (test mode) - [Sign up at stripe.com](https://stripe.com)
- [x] Active WahadiniCryptoQuest development environment
- [x] PostgreSQL database running

### Required Tools
- .NET 8 SDK
- Node.js 18+ and npm
- Stripe CLI (for webhook testing)
- Git

---

## Step 1: Stripe Dashboard Configuration (10 minutes)

### 1.1 Create Stripe Account
1. Sign up at https://stripe.com
2. Verify your email address
3. Switch to **Test Mode** (toggle in top-right corner)

### 1.2 Create Products and Prices

**Product 1: Monthly Premium**
1. Navigate to **Products** → **Add Product**
2. Name: `Monthly Premium`
3. Description: `WahadiniCryptoQuest Monthly Premium Subscription`
4. Pricing:
   - **USD**: $9.99/month (Recurring)
   - **INR**: ₹750/month (Recurring)
   - **EUR**: €8.99/month (Recurring)
5. Copy each Price ID (format: `price_xxxxx`)

**Product 2: Yearly Premium**
1. **Add Product** again
2. Name: `Yearly Premium`
3. Description: `WahadiniCryptoQuest Yearly Premium Subscription (Save 17%)`
4. Pricing:
   - **USD**: $99/year (Recurring)
   - **INR**: ₹7500/year (Recurring)
   - **EUR**: €89/year (Recurring)
5. Copy each Price ID

**Save these Price IDs - you'll need them for database seeding!**

### 1.3 Get API Keys
1. Navigate to **Developers** → **API Keys**
2. Copy **Publishable Key** (starts with `pk_test_`)
3. Copy **Secret Key** (starts with `sk_test_`)
4. **Never commit these to version control!**

### 1.4 Configure Webhook Endpoint
1. Navigate to **Developers** → **Webhooks**
2. Click **Add Endpoint**
3. Endpoint URL: `https://your-domain.com/api/webhooks/stripe` (or use ngrok for local dev)
4. Select events to listen for:
   - `checkout.session.completed`
   - `customer.subscription.created`
   - `customer.subscription.updated`
   - `customer.subscription.deleted`
   - `invoice.payment_succeeded`
   - `invoice.payment_failed`
5. Copy **Signing Secret** (starts with `whsec_`)

---

## Step 2: Backend Setup (15 minutes)

### 2.1 Install Dependencies

```bash
cd backend
dotnet add src/WahadiniCryptoQuest.API/Wah adiniCryptoQuest.API.csproj package Stripe.net --version 43.0.0
```

### 2.2 Configure Environment Variables

**File**: `backend/src/WahadiniCryptoQuest.API/appsettings.Development.json`

```json
{
  "Stripe": {
    "SecretKey": "sk_test_YOUR_SECRET_KEY_HERE",
    "PublishableKey": "pk_test_YOUR_PUBLISHABLE_KEY_HERE",
    "WebhookSecret": "whsec_YOUR_WEBHOOK_SECRET_HERE"
  },
  "App": {
    "ClientUrl": "http://localhost:5173",
    "ServerUrl": "http://localhost:5000"
  }
}
```

**Important**: Add `appsettings.Development.json` to `.gitignore` if not already there!

### 2.3 Run Database Migrations

```bash
cd backend

# Create migration
dotnet ef migrations add AddStripeSubscription --project src/WahadiniCryptoQuest.DAL --startup-project src/WahadiniCryptoQuest.API

# Apply migration
dotnet ef database update --project src/WahadiniCryptoQuest.DAL --startup-project src/WahadiniCryptoQuest.API
```

### 2.4 Seed Currency Pricing Data

**File**: `backend/scripts/seed-currency-pricing.sql`

```sql
-- Seed currency pricing with Stripe Price IDs
INSERT INTO CurrencyPricing (CurrencyCode, CurrencySymbol, CurrencyName, 
    StripePriceIdMonthly, StripePriceIdYearly, MonthlyPrice, YearlyPrice, 
    LocaleCode, DecimalPlaces, IsActive, IsDefault, CreatedAt)
VALUES 
    -- USD (Default)
    ('USD', '$', 'US Dollar', 
     'price_YOUR_USD_MONTHLY_ID', 'price_YOUR_USD_YEARLY_ID', 
     9.99, 99.00, 'en-US', 2, TRUE, TRUE, NOW()),
    
    -- INR
    ('INR', '₹', 'Indian Rupee', 
     'price_YOUR_INR_MONTHLY_ID', 'price_YOUR_INR_YEARLY_ID', 
     750.00, 7500.00, 'hi-IN', 2, TRUE, FALSE, NOW()),
    
    -- EUR
    ('EUR', '€', 'Euro', 
     'price_YOUR_EUR_MONTHLY_ID', 'price_YOUR_EUR_YEARLY_ID', 
     8.99, 89.00, 'en-EU', 2, TRUE, FALSE, NOW());
```

**Replace** `price_YOUR_xxx_ID` with actual Price IDs from Step 1.2!

Run the seed script:
```bash
psql -U postgres -d wahadini_crypto_quest -f scripts/seed-currency-pricing.sql
```

### 2.5 Start Backend Server

```bash
cd backend
dotnet run --project src/WahadiniCryptoQuest.API
```

Verify it's running: `http://localhost:5000/swagger`

---

## Step 3: Frontend Setup (10 minutes)

### 3.1 Install Dependencies

```bash
cd frontend
npm install @stripe/stripe-js
```

### 3.2 Configure Environment Variables

**File**: `frontend/.env.development.local`

```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_STRIPE_PUBLISHABLE_KEY=pk_test_YOUR_PUBLISHABLE_KEY_HERE
```

**Important**: Add `.env.development.local` to `.gitignore`!

### 3.3 Start Frontend Server

```bash
cd frontend
npm run dev
```

Verify it's running: `http://localhost:5173`

---

## Step 4: Stripe CLI Setup (for Local Webhook Testing) (5 minutes)

### 4.1 Install Stripe CLI

**Windows (PowerShell)**:
```powershell
choco install stripe-cli
```

**macOS**:
```bash
brew install stripe/stripe-cli/stripe
```

**Linux**:
```bash
# Download from https://github.com/stripe/stripe-cli/releases/latest
```

### 4.2 Login to Stripe CLI

```bash
stripe login
```

This will open your browser to authorize the CLI.

### 4.3 Forward Webhooks to Local Server

```bash
stripe listen --forward-to localhost:5000/api/webhooks/stripe
```

**Copy the webhook signing secret** displayed (starts with `whsec_`) and update your `appsettings.Development.json`.

Keep this terminal window open while developing!

---

## Step 5: Verify Setup (5 minutes)

### 5.1 Test Currency Pricing Endpoint

```bash
curl http://localhost:5000/api/subscriptions/plans
```

Expected: JSON response with three plans (Free, Monthly, Yearly) in USD.

### 5.2 Test Currency Detection

```bash
curl http://localhost:5000/api/subscriptions/plans?currency=INR
```

Expected: Same plans but with INR pricing (₹750/month, ₹7500/year).

### 5.3 Test Checkout Flow (End-to-End)

1. **Login to frontend**: `http://localhost:5173/login`
2. **Navigate to pricing**: Click "Upgrade" or visit `/pricing`
3. **Select plan**: Click "Subscribe Monthly" (USD)
4. **Apply discount** (optional): If you have a reward code
5. **Redirect to Stripe**: Should redirect to Stripe Checkout
6. **Use test card**: `4242 4242 4242 4242`, any future date, any CVC
7. **Complete payment**: Should redirect back to success page
8. **Verify subscription**: Check `/profile` - should show "Premium" badge

### 5.4 Test Webhook Processing

1. Complete checkout (Step 5.3)
2. Check Stripe CLI terminal - should show webhook event received
3. Check backend logs - should show event processing
4. Verify database: 
```sql
SELECT * FROM Subscriptions WHERE UserId = YOUR_USER_ID;
```

Expected: One record with Status = 'Active'

---

## Quick Reference

### Stripe Test Cards

| Card Number | Scenario |
|-------------|----------|
| 4242 4242 4242 4242 | Successful payment |
| 4000 0000 0000 0002 | Payment declined |
| 4000 0000 0000 9995 | Insufficient funds |
| 4000 0025 0000 3155 | 3D Secure authentication required |

### Useful Commands

```bash
# View Stripe logs
stripe logs tail

# Trigger webhook manually
stripe trigger checkout.session.completed

# List recent events
stripe events list --limit 10

# View customer details
stripe customers list

# View subscriptions
stripe subscriptions list
```

### API Endpoints Quick Reference

```bash
# Get plans
GET /api/subscriptions/plans?currency=USD

# Get subscription status
GET /api/subscriptions/status
Authorization: Bearer {token}

# Create checkout
POST /api/subscriptions/checkout
Authorization: Bearer {token}
Body: { "priceId": "price_xxx", "currency": "USD" }

# Create portal session
POST /api/subscriptions/portal
Authorization: Bearer {token}
Body: { "returnUrl": "http://localhost:5173/profile" }

# Cancel subscription
POST /api/subscriptions/cancel
Authorization: Bearer {token}
Body: { "reason": "Testing" }
```

---

## Troubleshooting

### Issue: "Stripe signature verification failed"

**Solution**: 
- Ensure Stripe CLI is running with correct forward URL
- Update `WebhookSecret` in `appsettings.Development.json` with CLI-provided secret
- Restart backend server after updating config

### Issue: "Currency 'XXX' not found"

**Solution**:
- Verify currency pricing seed script ran successfully
- Check database: `SELECT * FROM CurrencyPricing;`
- Ensure `IsActive = TRUE` for the currency

### Issue: "Price ID not found in Stripe"

**Solution**:
- Verify Price IDs in database match Stripe Dashboard
- Check you're using TEST mode Price IDs (start with `price_test_`)
- Recreate products if necessary and update database

### Issue: "CORS error" from frontend

**Solution**:
- Verify `App:ClientUrl` in `appsettings.Development.json` matches frontend URL
- Check CORS configuration in `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### Issue: Database migration fails

**Solution**:
- Check PostgreSQL is running: `psql -U postgres -l`
- Verify connection string in `appsettings.Development.json`
- Drop and recreate database if needed (development only!):
```sql
DROP DATABASE wahadini_crypto_quest;
CREATE DATABASE wahadini_crypto_quest;
```

---

## Next Steps

After successful setup:

1. **Implement remaining features**:
   - Discount code integration with Rewards system
   - Admin currency management UI
   - Subscription analytics dashboard

2. **Write tests**:
   - Unit tests for domain entities
   - Integration tests for API endpoints
   - End-to-end tests for checkout flow

3. **Production preparation**:
   - Replace test API keys with live keys
   - Configure production webhook endpoint
   - Set up monitoring and alerting
   - Review Stripe Dashboard settings

4. **Security review**:
   - Ensure no API keys in version control
   - Verify webhook signature validation
   - Test rate limiting
   - Review CORS configuration

---

## Support Resources

- **Stripe Documentation**: https://stripe.com/docs
- **Stripe API Reference**: https://stripe.com/docs/api
- **Stripe Testing**: https://stripe.com/docs/testing
- **Stripe CLI Docs**: https://stripe.com/docs/stripe-cli
- **Project Documentation**: `/docs/subscription-feature.md`

---

## Checklist

Before proceeding to development, ensure:

- [ ] Stripe account created and verified
- [ ] Products and prices configured in Stripe Dashboard
- [ ] API keys copied and stored securely
- [ ] Webhook endpoint configured
- [ ] Backend dependencies installed
- [ ] Database migrations applied successfully
- [ ] Currency pricing data seeded
- [ ] Frontend dependencies installed
- [ ] Environment variables configured
- [ ] Stripe CLI installed and authenticated
- [ ] All verification tests passed
- [ ] Webhook forwarding working

**Setup Complete! Ready for feature implementation.** 🎉
