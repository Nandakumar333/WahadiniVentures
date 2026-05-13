-- Seed data for discount codes
-- Sample discounts for subscription purchases

DO $$
DECLARE
    discount_id1 UUID;
    discount_id2 UUID;
    discount_id3 UUID;
BEGIN
    -- Generate UUIDs for the discount codes
    discount_id1 := gen_random_uuid();
    discount_id2 := gen_random_uuid();
    discount_id3 := gen_random_uuid();

    -- Insert sample discount codes
    INSERT INTO discount_codes (id, code, discount_percentage, required_points, max_redemptions, current_redemptions, expiry_date, is_active, created_at, updated_at)
    VALUES
        (discount_id1, 'SAVE10', 10, 500, 100, 0, (CURRENT_TIMESTAMP + INTERVAL '90 days'), true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
        (discount_id2, 'SAVE20', 20, 1000, 50, 0, (CURRENT_TIMESTAMP + INTERVAL '90 days'), true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
        (discount_id3, 'SAVE50', 50, 5000, 10, 0, (CURRENT_TIMESTAMP + INTERVAL '90 days'), true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

    RAISE NOTICE 'Discount codes seeded successfully:';
    RAISE NOTICE '- SAVE10 (10%% off, 500 points, 100 redemptions available)';
    RAISE NOTICE '- SAVE20 (20%% off, 1000 points, 50 redemptions available)';
    RAISE NOTICE '- SAVE50 (50%% off, 5000 points, 10 redemptions available)';
END $$;
