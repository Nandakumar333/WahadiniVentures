DO $$
DECLARE
    v_user_id UUID;
    v_lesson_id UUID;
    v_current_balance INT := 0;
    v_now TIMESTAMP := NOW();
BEGIN
    -- 1. Get Test User ID
    SELECT "Id" INTO v_user_id FROM "users" WHERE "Email" = 'test@wahadinicryptoquest.com';

    IF v_user_id IS NOT NULL THEN
        RAISE NOTICE 'Found Test User ID: %', v_user_id;

        -- Calculate current balance to ensure consistency
        -- (Optional: logic to sum existing transactions if needed, but here we just append)
        
        -- 2. Insert Reward Transactions
        
        -- Welcome Bonus (100)
        IF NOT EXISTS (SELECT 1 FROM "reward_transactions" WHERE "UserId" = v_user_id AND "TransactionType" = 'AdminBonus' AND "Description" = 'Welcome Bonus') THEN
            INSERT INTO "reward_transactions" (
                "Id", "UserId", "Amount", "TransactionType", "Description", "BalanceAfter", "CreatedAt", "UpdatedAt", "IsDeleted"
            ) VALUES (
                gen_random_uuid(), v_user_id, 100, 'AdminBonus', 'Welcome Bonus', 100, v_now, v_now, false
            );
            v_current_balance := v_current_balance + 100;
        END IF;

        -- Lesson Completion (50)
        IF NOT EXISTS (SELECT 1 FROM "reward_transactions" WHERE "UserId" = v_user_id AND "TransactionType" = 'LessonCompletion') THEN
            INSERT INTO "reward_transactions" (
                "Id", "UserId", "Amount", "TransactionType", "Description", "BalanceAfter", "ReferenceType", "ReferenceId", "CreatedAt", "UpdatedAt", "IsDeleted"
            ) VALUES (
                gen_random_uuid(), v_user_id, 50, 'LessonCompletion', 'Completed lesson: What are Crypto Airdrops?', v_current_balance + 50, 'Lesson', gen_random_uuid()::text, v_now, v_now, false
            );
            v_current_balance := v_current_balance + 50;
        END IF;

        -- Daily Streak (10)
        IF NOT EXISTS (SELECT 1 FROM "reward_transactions" WHERE "UserId" = v_user_id AND "TransactionType" = 'DailyStreak') THEN
            INSERT INTO "reward_transactions" (
                "Id", "UserId", "Amount", "TransactionType", "Description", "BalanceAfter", "ReferenceType", "ReferenceId", "CreatedAt", "UpdatedAt", "IsDeleted"
            ) VALUES (
                gen_random_uuid(), v_user_id, 10, 'DailyStreak', 'Daily Login Streak (Day 1)', v_current_balance + 10, 'Streak', to_char(v_now, 'YYYY-MM-DD'), v_now, v_now, false
            );
            v_current_balance := v_current_balance + 10;
        END IF;

        -- 3. Update User Points
        -- Note: We update both TotalPointsEarned and CurrentPoints
        UPDATE "users"
        SET "TotalPointsEarned" = v_current_balance,
            "CurrentPoints" = v_current_balance,
            "UpdatedAt" = v_now
        WHERE "Id" = v_user_id;

        -- 4. Insert Achievements
        -- Note: Assuming table name is "UserAchievements" (PascalCase) as it lacks explicit ToTable configuration
        -- If this fails, try "user_achievements" or "UserAchievement"
        
        -- First Steps (Unlocked)
        IF NOT EXISTS (SELECT 1 FROM "UserAchievements" WHERE "UserId" = v_user_id AND "AchievementId" = 'first-steps') THEN
            INSERT INTO "UserAchievements" (
                "Id", "UserId", "AchievementId", "UnlockedAt", "IsUnlocked", "Progress", "Notified", "CreatedAt", "UpdatedAt", "IsDeleted"
            ) VALUES (
                gen_random_uuid(), v_user_id, 'first-steps', v_now - INTERVAL '1 day', true, 100, true, v_now - INTERVAL '1 day', v_now - INTERVAL '1 day', false
            );
        END IF;

        -- Course Champion (In Progress)
        IF NOT EXISTS (SELECT 1 FROM "UserAchievements" WHERE "UserId" = v_user_id AND "AchievementId" = 'course-champion') THEN
            INSERT INTO "UserAchievements" (
                "Id", "UserId", "AchievementId", "UnlockedAt", "IsUnlocked", "Progress", "Notified", "CreatedAt", "UpdatedAt", "IsDeleted"
            ) VALUES (
                gen_random_uuid(), v_user_id, 'course-champion', '0001-01-01 00:00:00', false, 50, false, v_now, v_now, false
            );
        END IF;

    ELSE
        RAISE NOTICE 'Test user not found. Skipping user-related inserts.';
    END IF;

    -- 5. Insert Learning Task (Quiz)
    SELECT "Id" INTO v_lesson_id FROM "lessons" WHERE "Title" = 'What are Crypto Airdrops?' LIMIT 1;

    IF v_lesson_id IS NOT NULL THEN
        RAISE NOTICE 'Found Lesson ID: %', v_lesson_id;

        IF NOT EXISTS (SELECT 1 FROM "learning_tasks" WHERE "LessonId" = v_lesson_id AND "Title" = 'Airdrop Basics Quiz') THEN
            INSERT INTO "learning_tasks" (
                "Id", "LessonId", "Title", "Description", "TaskType", "TaskData", "RewardPoints", "OrderIndex", "IsRequired", "CreatedAt", "UpdatedAt", "IsDeleted"
            ) VALUES (
                gen_random_uuid(), v_lesson_id, 'Airdrop Basics Quiz', 'Test your knowledge about crypto airdrops', 'Quiz', 
                '{"questions":[{"id":1,"text":"What is an airdrop?","options":["Free tokens","A scam","A mining method"],"correctAnswer":0}]}', 
                10, 1, true, v_now, v_now, false
            );
        END IF;
    ELSE
        RAISE NOTICE 'Lesson "What are Crypto Airdrops?" not found. Skipping task insert.';
    END IF;

END $$;
