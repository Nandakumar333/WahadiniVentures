-- Apply Performance Indexes for Admin Analytics (T032)
-- Adds indexes for optimized dashboard statistics queries

-- Analytics index: User growth trend (T032 - US1)
CREATE INDEX IF NOT EXISTS "IX_Users_CreatedAt" ON users ("CreatedAt");

-- Analytics index: Active subscribers and revenue trend (T032 - US1)
CREATE INDEX IF NOT EXISTS "IX_Subscriptions_Status_CreatedAt" ON subscriptions ("Status", "CreatedAt");

-- Analytics index: Pending task count (T032 - US1)
CREATE INDEX IF NOT EXISTS "IX_UserTaskSubmissions_Status" ON user_task_submissions ("Status");

-- Verify indexes created
SELECT 
    schemaname,
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE indexname IN ('IX_Users_CreatedAt', 'IX_Subscriptions_Status_CreatedAt', 'IX_UserTaskSubmissions_Status')
ORDER BY tablename, indexname;
