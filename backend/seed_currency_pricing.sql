-- Seed initial currency pricing data for 5 supported currencies
-- This data should be updated with actual Stripe Price IDs after Stripe Dashboard setup

-- USD (United States Dollar)
INSERT INTO "CurrencyPricings" 
    ("Id", "CurrencyCode", "StripePriceId", "MonthlyPrice", "YearlyPrice", "YearlySavingsPercent", 
     "CurrencySymbol", "ShowDecimal", "DecimalPlaces", "IsActive", 
     "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted")
VALUES 
    (gen_random_uuid(), 'USD', 'price_USD_monthly_placeholder', 9.99, 99.99, 16.68, 
     '$', true, 2, true, 
     CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'System', 'System', false);

-- INR (Indian Rupee)
INSERT INTO "CurrencyPricings" 
    ("Id", "CurrencyCode", "StripePriceId", "MonthlyPrice", "YearlyPrice", "YearlySavingsPercent", 
     "CurrencySymbol", "ShowDecimal", "DecimalPlaces", "IsActive", 
     "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted")
VALUES 
    (gen_random_uuid(), 'INR', 'price_INR_monthly_placeholder', 799.00, 7999.00, 16.68, 
     '₹', true, 2, true, 
     CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'System', 'System', false);

-- EUR (Euro)
INSERT INTO "CurrencyPricings" 
    ("Id", "CurrencyCode", "StripePriceId", "MonthlyPrice", "YearlyPrice", "YearlySavingsPercent", 
     "CurrencySymbol", "ShowDecimal", "DecimalPlaces", "IsActive", 
     "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted")
VALUES 
    (gen_random_uuid(), 'EUR', 'price_EUR_monthly_placeholder', 8.99, 89.99, 16.68, 
     '€', true, 2, true, 
     CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'System', 'System', false);

-- JPY (Japanese Yen - no decimals)
INSERT INTO "CurrencyPricings" 
    ("Id", "CurrencyCode", "StripePriceId", "MonthlyPrice", "YearlyPrice", "YearlySavingsPercent", 
     "CurrencySymbol", "ShowDecimal", "DecimalPlaces", "IsActive", 
     "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted")
VALUES 
    (gen_random_uuid(), 'JPY', 'price_JPY_monthly_placeholder', 1200.00, 12000.00, 16.67, 
     '¥', false, 0, true, 
     CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'System', 'System', false);

-- GBP (British Pound)
INSERT INTO "CurrencyPricings" 
    ("Id", "CurrencyCode", "StripePriceId", "MonthlyPrice", "YearlyPrice", "YearlySavingsPercent", 
     "CurrencySymbol", "ShowDecimal", "DecimalPlaces", "IsActive", 
     "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted")
VALUES 
    (gen_random_uuid(), 'GBP', 'price_GBP_monthly_placeholder', 7.99, 79.99, 16.68, 
     '£', true, 2, true, 
     CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'System', 'System', false);

-- Note: After setting up Stripe Dashboard:
-- 1. Create products for MonthlyPremium and YearlyPremium
-- 2. Create prices for each currency
-- 3. Update StripePriceId values in this data
-- 4. Update appsettings.json Stripe:Prices configuration
