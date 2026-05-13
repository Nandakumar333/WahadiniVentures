-- Admin Dashboard Migration - 20251216_AddAdminDashboardEntities
-- T017: Apply migration manually

-- Add ban-related columns to users table
ALTER TABLE users ADD COLUMN IF NOT EXISTS "IsBanned" boolean NOT NULL DEFAULT false;
ALTER TABLE users ADD COLUMN IF NOT EXISTS "BanReason" character varying(500);
ALTER TABLE users ADD COLUMN IF NOT EXISTS "BannedAt" timestamp with time zone;
ALTER TABLE users ADD COLUMN IF NOT EXISTS "BannedBy" uuid;

-- Add admin-related columns to discount_codes table  
ALTER TABLE discount_codes ADD COLUMN IF NOT EXISTS "UsageLimit" integer NOT NULL DEFAULT 0;
ALTER TABLE discount_codes ADD COLUMN IF NOT EXISTS "UsageCount" integer NOT NULL DEFAULT 0;
ALTER TABLE discount_codes ADD COLUMN IF NOT EXISTS "CreatedBy" uuid;

-- Create audit_log_entries table
CREATE TABLE IF NOT EXISTS audit_log_entries (
    "Id" uuid NOT NULL,
    "AdminUserId" uuid NOT NULL,
    "ActionType" character varying(100) NOT NULL,
    "ResourceType" character varying(100) NOT NULL,
    "ResourceId" character varying(100) NOT NULL,
    "BeforeValue" jsonb,
    "AfterValue" jsonb,
    "IPAddress" character varying(45) NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text NOT NULL,
    "UpdatedBy" text NOT NULL,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    CONSTRAINT "PK_audit_log_entries" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_audit_log_entries_users_AdminUserId" FOREIGN KEY ("AdminUserId") REFERENCES users("Id") ON DELETE RESTRICT
);

-- Create user_notifications table
CREATE TABLE IF NOT EXISTS user_notifications (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Type" character varying(50) NOT NULL,
    "Message" character varying(500) NOT NULL,
    "IsRead" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text NOT NULL,
    "UpdatedBy" text NOT NULL,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    CONSTRAINT "PK_user_notifications" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_user_notifications_users_UserId" FOREIGN KEY ("UserId") REFERENCES users("Id") ON DELETE CASCADE
);

-- Create point_adjustments table
CREATE TABLE IF NOT EXISTS point_adjustments (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "PreviousBalance" integer NOT NULL,
    "AdjustmentAmount" integer NOT NULL,
    "NewBalance" integer NOT NULL,
    "Reason" character varying(500) NOT NULL,
    "AdminUserId" uuid NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text NOT NULL,
    "UpdatedBy" text NOT NULL,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    CONSTRAINT "PK_point_adjustments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_point_adjustments_users_UserId" FOREIGN KEY ("UserId") REFERENCES users("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_point_adjustments_users_AdminUserId" FOREIGN KEY ("AdminUserId") REFERENCES users("Id") ON DELETE RESTRICT
);

-- Create indexes for audit_log_entries
CREATE INDEX IF NOT EXISTS "IX_AuditLog_AdminAction" ON audit_log_entries ("AdminUserId", "ActionType", "Timestamp");
CREATE INDEX IF NOT EXISTS "IX_AuditLog_Resource" ON audit_log_entries ("ResourceType", "ResourceId");
CREATE INDEX IF NOT EXISTS "IX_AuditLog_Timestamp" ON audit_log_entries ("Timestamp");

-- Create indexes for user_notifications  
CREATE INDEX IF NOT EXISTS "IX_UserNotifications_UserUnread" ON user_notifications ("UserId", "IsRead", "CreatedAt" DESC);
CREATE INDEX IF NOT EXISTS "IX_UserNotifications_User" ON user_notifications ("UserId");

-- Create indexes for point_adjustments
CREATE INDEX IF NOT EXISTS "IX_PointAdjustments_UserHistory" ON point_adjustments ("UserId", "Timestamp" DESC);
CREATE INDEX IF NOT EXISTS "IX_PointAdjustments_AdminHistory" ON point_adjustments ("AdminUserId", "Timestamp");

-- Analytics indexes (T032)
CREATE INDEX IF NOT EXISTS "IX_Users_CreatedAt" ON users ("CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_Subscriptions_StatusCreatedAt" ON subscriptions ("Status", "CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_UserTaskSubmissions_Status" ON user_task_submissions ("Status");

-- Insert migration history record
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251216000000_AddAdminDashboardEntities', '8.0.0')
ON CONFLICT DO NOTHING;

SELECT 'Admin Dashboard migration applied successfully!' AS result;
