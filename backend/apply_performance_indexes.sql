CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE categories (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "IconUrl" character varying(500),
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_categories" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE discount_codes (
        "Id" uuid NOT NULL,
        "Code" character varying(50) NOT NULL,
        "DiscountPercentage" integer NOT NULL,
        "RequiredPoints" integer NOT NULL,
        "MaxRedemptions" integer NOT NULL,
        "CurrentRedemptions" integer NOT NULL,
        "ExpiryDate" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_discount_codes" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE permissions (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "Resource" character varying(50) NOT NULL,
        "Action" character varying(50) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_permissions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE roles (
        "Id" uuid NOT NULL,
        "Name" character varying(50) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_roles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE users (
        "Id" uuid NOT NULL,
        "Email" character varying(320) NOT NULL,
        "FirstName" character varying(100) NOT NULL,
        "LastName" character varying(100) NOT NULL,
        "PasswordHash" character varying(255) NOT NULL,
        "EmailConfirmed" boolean NOT NULL DEFAULT FALSE,
        "EmailVerified" boolean NOT NULL,
        "EmailConfirmedAt" timestamp with time zone,
        "LastLoginAt" timestamp with time zone,
        "FailedLoginAttempts" integer NOT NULL DEFAULT 0,
        "LockoutEnd" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        "Role" character varying(20) NOT NULL DEFAULT 'Free',
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "CreatedBy" character varying(100) NOT NULL DEFAULT 'System',
        "UpdatedBy" character varying(100) NOT NULL DEFAULT 'System',
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE role_permissions (
        "Id" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        "PermissionId" uuid NOT NULL,
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_role_permissions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_role_permissions_permissions_PermissionId" FOREIGN KEY ("PermissionId") REFERENCES permissions ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_role_permissions_roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES roles ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE courses (
        "Id" uuid NOT NULL,
        "CategoryId" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Description" character varying(2000) NOT NULL,
        "ThumbnailUrl" character varying(500),
        "DifficultyLevel" text NOT NULL,
        "EstimatedDuration" integer NOT NULL,
        "IsPremium" boolean NOT NULL,
        "RewardPoints" integer NOT NULL,
        "IsPublished" boolean NOT NULL,
        "ViewCount" integer NOT NULL,
        "CreatedByUserId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_courses" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_courses_categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES categories ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_courses_users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES users ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE email_verification_tokens (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Token" character varying(500) NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "IsUsed" boolean NOT NULL DEFAULT FALSE,
        "UsedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_email_verification_tokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_email_verification_tokens_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE password_reset_tokens (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Token" character varying(500) NOT NULL,
        "HashedToken" text NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "IsUsed" boolean NOT NULL,
        "UsedAt" text,
        "ClientInfo" text,
        "IpAddress" text,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_password_reset_tokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_password_reset_tokens_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE refresh_tokens (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Token" character varying(500) NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "IsUsed" boolean NOT NULL,
        "IsRevoked" boolean NOT NULL,
        "UsedAt" timestamp with time zone,
        "RevokedAt" timestamp with time zone,
        "DeviceInfo" text,
        "IpAddress" text,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_refresh_tokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_refresh_tokens_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE reward_transactions (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Amount" integer NOT NULL,
        "TransactionType" text NOT NULL,
        "ReferenceId" character varying(100),
        "ReferenceType" character varying(50),
        "Description" character varying(500) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_reward_transactions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_reward_transactions_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE user_discount_redemptions (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "DiscountCodeId" uuid NOT NULL,
        "RedeemedAt" timestamp with time zone NOT NULL,
        "UsedInSubscription" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_user_discount_redemptions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_user_discount_redemptions_discount_codes_DiscountCodeId" FOREIGN KEY ("DiscountCodeId") REFERENCES discount_codes ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_user_discount_redemptions_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE user_roles (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        "AssignedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "ExpiresAt" timestamp with time zone,
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_user_roles" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_user_roles_roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES roles ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_user_roles_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE lessons (
        "Id" uuid NOT NULL,
        "CourseId" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Description" character varying(2000) NOT NULL,
        "YouTubeVideoId" character varying(50) NOT NULL,
        "Duration" integer NOT NULL,
        video_duration integer,
        "OrderIndex" integer NOT NULL,
        "IsPremium" boolean NOT NULL,
        "RewardPoints" integer NOT NULL,
        "ContentMarkdown" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_lessons" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_lessons_courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES courses ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE user_course_enrollments (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "CourseId" uuid NOT NULL,
        "EnrolledAt" timestamp with time zone NOT NULL,
        "LastAccessedAt" timestamp with time zone NOT NULL,
        "CompletionPercentage" numeric NOT NULL,
        "IsCompleted" boolean NOT NULL,
        "CompletedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_user_course_enrollments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_user_course_enrollments_courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES courses ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_user_course_enrollments_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE learning_tasks (
        "Id" uuid NOT NULL,
        "LessonId" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Description" character varying(2000) NOT NULL,
        "TaskType" text NOT NULL,
        "TaskData" jsonb NOT NULL,
        "RewardPoints" integer NOT NULL,
        "TimeLimit" integer,
        "OrderIndex" integer NOT NULL,
        "IsRequired" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_learning_tasks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_learning_tasks_lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES lessons ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE lesson_completions (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "LessonId" uuid NOT NULL,
        "CompletedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "PointsAwarded" integer NOT NULL DEFAULT 0,
        "CompletionPercentage" numeric(5,2) NOT NULL DEFAULT 0.0,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_lesson_completions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_lesson_completions_lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES lessons ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_lesson_completions_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE user_progress (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "LessonId" uuid NOT NULL,
        "LastWatchedPosition" integer NOT NULL,
        "VideoWatchTimeSeconds" integer NOT NULL,
        "CompletionPercentage" numeric(5,2) NOT NULL,
        "TotalWatchTime" integer NOT NULL,
        "IsCompleted" boolean NOT NULL,
        "CompletedAt" timestamp with time zone,
        "RewardPointsClaimed" boolean NOT NULL,
        "LastUpdatedAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_user_progress" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_user_progress_lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES lessons ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_user_progress_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE TABLE user_task_submissions (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "TaskId" uuid NOT NULL,
        "SubmissionData" jsonb NOT NULL,
        "Status" text NOT NULL,
        "SubmittedAt" timestamp with time zone NOT NULL,
        "ReviewedAt" timestamp with time zone,
        "ReviewedByUserId" uuid,
        "FeedbackText" text,
        "RewardPointsAwarded" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_user_task_submissions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_user_task_submissions_learning_tasks_TaskId" FOREIGN KEY ("TaskId") REFERENCES learning_tasks ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_user_task_submissions_users_ReviewedByUserId" FOREIGN KEY ("ReviewedByUserId") REFERENCES users ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_user_task_submissions_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_categories_DisplayOrder" ON categories ("DisplayOrder");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_categories_IsActive" ON categories ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_courses_CategoryId" ON courses ("CategoryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_courses_CreatedByUserId" ON courses ("CreatedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_courses_DifficultyLevel" ON courses ("DifficultyLevel");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_courses_IsPremium" ON courses ("IsPremium");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_courses_IsPublished" ON courses ("IsPublished");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_courses_IsPublished_CategoryId_DifficultyLevel_IsPremium" ON courses ("IsPublished", "CategoryId", "DifficultyLevel", "IsPremium");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_discount_codes_Code" ON discount_codes ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_discount_codes_IsActive" ON discount_codes ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE UNIQUE INDEX idx_email_verification_tokens_token ON email_verification_tokens ("Token");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX idx_email_verification_tokens_user_used ON email_verification_tokens ("UserId", "IsUsed");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_email_verification_tokens_ExpiresAt" ON email_verification_tokens ("ExpiresAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_email_verification_tokens_IsUsed" ON email_verification_tokens ("IsUsed");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_email_verification_tokens_UserId" ON email_verification_tokens ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_email_verification_tokens_UserId_IsUsed_ExpiresAt" ON email_verification_tokens ("UserId", "IsUsed", "ExpiresAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_learning_tasks_LessonId" ON learning_tasks ("LessonId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_learning_tasks_TaskType" ON learning_tasks ("TaskType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX idx_lesson_completions_completed_at ON lesson_completions ("CompletedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE UNIQUE INDEX idx_lesson_completions_user_lesson ON lesson_completions ("UserId", "LessonId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_lesson_completions_LessonId" ON lesson_completions ("LessonId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_lessons_CourseId" ON lessons ("CourseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_lessons_OrderIndex" ON lessons ("OrderIndex");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_password_reset_tokens_ExpiresAt" ON password_reset_tokens ("ExpiresAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_password_reset_tokens_Token" ON password_reset_tokens ("Token");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_password_reset_tokens_UserId" ON password_reset_tokens ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE UNIQUE INDEX idx_permissions_name ON permissions ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX idx_permissions_resource_action ON permissions ("Resource", "Action");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_refresh_tokens_ExpiresAt" ON refresh_tokens ("ExpiresAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_refresh_tokens_Token" ON refresh_tokens ("Token");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_refresh_tokens_UserId" ON refresh_tokens ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_reward_transactions_CreatedAt" ON reward_transactions ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_reward_transactions_TransactionType" ON reward_transactions ("TransactionType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_reward_transactions_UserId" ON reward_transactions ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX idx_role_permissions_active ON role_permissions ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE UNIQUE INDEX idx_role_permissions_role_permission ON role_permissions ("RoleId", "PermissionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_role_permissions_PermissionId" ON role_permissions ("PermissionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE UNIQUE INDEX idx_roles_name ON roles ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_user_course_enrollments_CourseId" ON user_course_enrollments ("CourseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_user_course_enrollments_UserId" ON user_course_enrollments ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_user_course_enrollments_UserId_CourseId" ON user_course_enrollments ("UserId", "CourseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_user_discount_redemptions_DiscountCodeId" ON user_discount_redemptions ("DiscountCodeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_user_discount_redemptions_UserId" ON user_discount_redemptions ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX idx_user_progress_completed ON user_progress ("IsCompleted");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX idx_user_progress_updated_at ON user_progress ("UpdatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE UNIQUE INDEX idx_user_progress_user_lesson ON user_progress ("UserId", "LessonId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_user_progress_LessonId" ON user_progress ("LessonId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_user_progress_UserId" ON user_progress ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX idx_user_roles_active ON user_roles ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX idx_user_roles_expires_at ON user_roles ("ExpiresAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX idx_user_roles_user_role ON user_roles ("UserId", "RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_user_roles_RoleId" ON user_roles ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_user_task_submissions_ReviewedByUserId" ON user_task_submissions ("ReviewedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_user_task_submissions_Status" ON user_task_submissions ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_user_task_submissions_SubmittedAt" ON user_task_submissions ("SubmittedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_user_task_submissions_TaskId" ON user_task_submissions ("TaskId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX "IX_user_task_submissions_UserId" ON user_task_submissions ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX idx_users_created_at ON users ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE UNIQUE INDEX idx_users_email ON users ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX idx_users_email_confirmed ON users ("EmailConfirmed");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX idx_users_is_deleted ON users ("IsDeleted");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    CREATE INDEX idx_users_role ON users ("Role");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128060826_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251128060826_InitialCreate', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128122014_AddTaskSubmissionVersioning') THEN
    ALTER TABLE user_task_submissions ADD "Version" bytea NOT NULL DEFAULT BYTEA E'\\x';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251128122014_AddTaskSubmissionVersioning') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251128122014_AddTaskSubmissionVersioning', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204115729_AddRewardSystem') THEN
    ALTER TABLE users ADD "CurrentPoints" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204115729_AddRewardSystem') THEN
    ALTER TABLE users ADD "ReferralCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204115729_AddRewardSystem') THEN
    ALTER TABLE users ADD "TotalPointsEarned" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204115729_AddRewardSystem') THEN
    CREATE TABLE "ReferralAttributions" (
        "Id" uuid NOT NULL,
        "ReferrerId" uuid NOT NULL,
        "ReferredUserId" uuid NOT NULL,
        "ReferredAt" timestamp with time zone NOT NULL,
        "RewardClaimed" boolean NOT NULL,
        "RewardClaimedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_ReferralAttributions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ReferralAttributions_users_ReferredUserId" FOREIGN KEY ("ReferredUserId") REFERENCES users ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ReferralAttributions_users_ReferrerId" FOREIGN KEY ("ReferrerId") REFERENCES users ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204115729_AddRewardSystem') THEN
    CREATE TABLE "UserAchievements" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "AchievementId" character varying(100) NOT NULL,
        "UnlockedAt" timestamp with time zone NOT NULL,
        "Progress" integer NOT NULL,
        "IsUnlocked" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_UserAchievements" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserAchievements_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204115729_AddRewardSystem') THEN
    CREATE TABLE "UserStreaks" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "CurrentStreak" integer NOT NULL,
        "LongestStreak" integer NOT NULL,
        "LastActivityDate" timestamp with time zone NOT NULL,
        "LastStreakFreezeUsedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_UserStreaks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserStreaks_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204115729_AddRewardSystem') THEN
    CREATE UNIQUE INDEX "IX_ReferralAttributions_ReferredUserId" ON "ReferralAttributions" ("ReferredUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204115729_AddRewardSystem') THEN
    CREATE INDEX "IX_ReferralAttributions_ReferrerId" ON "ReferralAttributions" ("ReferrerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204115729_AddRewardSystem') THEN
    CREATE UNIQUE INDEX "IX_UserAchievements_UserId_AchievementId" ON "UserAchievements" ("UserId", "AchievementId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204115729_AddRewardSystem') THEN
    CREATE UNIQUE INDEX "IX_UserStreaks_UserId" ON "UserStreaks" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204115729_AddRewardSystem') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251204115729_AddRewardSystem', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204122131_AddUserRowVersionForConcurrency') THEN
    ALTER TABLE "UserStreaks" RENAME COLUMN "LastActivityDate" TO "LastLoginDate";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204122131_AddUserRowVersionForConcurrency') THEN
    ALTER TABLE users ALTER COLUMN "TotalPointsEarned" SET DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204122131_AddUserRowVersionForConcurrency') THEN
    ALTER TABLE users ALTER COLUMN "ReferralCode" TYPE character varying(6);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204122131_AddUserRowVersionForConcurrency') THEN
    ALTER TABLE users ALTER COLUMN "CurrentPoints" SET DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204122131_AddUserRowVersionForConcurrency') THEN
    ALTER TABLE users ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204122131_AddUserRowVersionForConcurrency') THEN
    ALTER TABLE reward_transactions ADD "AdminUserId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204122131_AddUserRowVersionForConcurrency') THEN
    ALTER TABLE reward_transactions ADD "BalanceAfter" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204122131_AddUserRowVersionForConcurrency') THEN
    CREATE INDEX "IX_reward_transactions_AdminUserId" ON reward_transactions ("AdminUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204122131_AddUserRowVersionForConcurrency') THEN
    ALTER TABLE reward_transactions ADD CONSTRAINT "FK_reward_transactions_users_AdminUserId" FOREIGN KEY ("AdminUserId") REFERENCES users ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204122131_AddUserRowVersionForConcurrency') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251204122131_AddUserRowVersionForConcurrency', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204125802_AddRewardSystemDeduplicationIndex') THEN
    ALTER TABLE "UserAchievements" ADD "Notified" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204125802_AddRewardSystemDeduplicationIndex') THEN
    CREATE UNIQUE INDEX "IX_reward_transactions_UserId_ReferenceId_ReferenceType" ON reward_transactions ("UserId", "ReferenceId", "ReferenceType") WHERE "ReferenceId" IS NOT NULL AND "ReferenceType" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251204125802_AddRewardSystemDeduplicationIndex') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251204125802_AddRewardSystemDeduplicationIndex', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251215190210_AddStripeSubscriptionV2') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251215190210_AddStripeSubscriptionV2', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    DROP TABLE IF EXISTS "CurrencyPricings" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    DROP TABLE IF EXISTS "Subscriptions" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    DROP TABLE IF EXISTS "SubscriptionHistories" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    DROP TABLE IF EXISTS "WebhookEvents" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    DROP TABLE IF EXISTS "currency_pricings" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    DROP TABLE IF EXISTS "subscriptions" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    DROP TABLE IF EXISTS "subscription_histories" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    DROP TABLE IF EXISTS "webhook_events" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE TABLE currency_pricings (
        "Id" uuid NOT NULL,
        "CurrencyCode" character varying(3) NOT NULL,
        "StripePriceId" character varying(255) NOT NULL,
        "MonthlyPrice" numeric(18,2) NOT NULL,
        "YearlyPrice" numeric(18,2) NOT NULL,
        "YearlySavingsPercent" numeric(5,2) NOT NULL,
        "CurrencySymbol" character varying(10) NOT NULL,
        "ShowDecimal" boolean NOT NULL,
        "DecimalPlaces" integer NOT NULL,
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" character varying(450) NOT NULL,
        "UpdatedBy" character varying(450) NOT NULL,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_currency_pricings" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE TABLE subscriptions (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "StripeCustomerId" character varying(255) NOT NULL,
        "StripeSubscriptionId" character varying(255) NOT NULL,
        "StripePriceId" character varying(255) NOT NULL,
        "Tier" integer NOT NULL,
        "Status" integer NOT NULL,
        "CurrencyCode" character varying(3) NOT NULL,
        "CurrentPeriodStart" timestamp with time zone NOT NULL,
        "CurrentPeriodEnd" timestamp with time zone NOT NULL,
        "IsCancelledAtPeriodEnd" boolean NOT NULL,
        "CancelledAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" character varying(450) NOT NULL,
        "UpdatedBy" character varying(450) NOT NULL,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_subscriptions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_subscriptions_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE TABLE subscription_histories (
        "Id" uuid NOT NULL,
        "SubscriptionId" uuid NOT NULL,
        "ChangeType" character varying(50) NOT NULL,
        "PreviousTier" integer,
        "NewTier" integer,
        "PreviousStatus" integer,
        "NewStatus" integer,
        "PreviousPeriodEnd" timestamp with time zone,
        "NewPeriodEnd" timestamp with time zone,
        "Notes" character varying(1000),
        "TriggeredBy" character varying(450),
        "WebhookEventId" character varying(255),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" character varying(450) NOT NULL,
        "UpdatedBy" character varying(450) NOT NULL,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_subscription_histories" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_subscription_histories_subscriptions_SubscriptionId" FOREIGN KEY ("SubscriptionId") REFERENCES subscriptions ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE TABLE webhook_events (
        "Id" uuid NOT NULL,
        "StripeEventId" character varying(255) NOT NULL,
        "EventType" character varying(100) NOT NULL,
        "EventCreatedAt" timestamp with time zone NOT NULL,
        "Status" integer NOT NULL,
        "ProcessedAt" timestamp with time zone,
        "FailedAt" timestamp with time zone,
        "RetryCount" integer NOT NULL DEFAULT 0,
        "ErrorMessage" character varying(2000),
        "PayloadJson" text NOT NULL,
        "SubscriptionId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" character varying(450) NOT NULL,
        "UpdatedBy" character varying(450) NOT NULL,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_webhook_events" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_webhook_events_subscriptions_SubscriptionId" FOREIGN KEY ("SubscriptionId") REFERENCES subscriptions ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE UNIQUE INDEX "IX_currency_pricings_CurrencyCode" ON currency_pricings ("CurrencyCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE INDEX "IX_currency_pricings_IsActive" ON currency_pricings ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE INDEX "IX_currency_pricings_StripePriceId" ON currency_pricings ("StripePriceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE INDEX "IX_subscription_histories_ChangeType" ON subscription_histories ("ChangeType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE INDEX "IX_subscription_histories_SubscriptionId" ON subscription_histories ("SubscriptionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE INDEX "IX_subscription_histories_SubscriptionId_CreatedAt" ON subscription_histories ("SubscriptionId", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE INDEX "IX_subscription_histories_TriggeredBy" ON subscription_histories ("TriggeredBy") WHERE "TriggeredBy" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE INDEX "IX_subscription_histories_WebhookEventId" ON subscription_histories ("WebhookEventId") WHERE "WebhookEventId" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE INDEX "IX_subscriptions_CancelAtPeriodEnd_PeriodEnd" ON subscriptions ("IsCancelledAtPeriodEnd", "CurrentPeriodEnd") WHERE "IsCancelledAtPeriodEnd" = true;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE INDEX "IX_subscriptions_Status" ON subscriptions ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE INDEX "IX_subscriptions_StripeCustomerId" ON subscriptions ("StripeCustomerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE UNIQUE INDEX "IX_subscriptions_StripeSubscriptionId" ON subscriptions ("StripeSubscriptionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE INDEX "IX_subscriptions_UserId" ON subscriptions ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE INDEX "IX_subscriptions_UserId_Status" ON subscriptions ("UserId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE INDEX "IX_webhook_events_EventType_CreatedAt" ON webhook_events ("EventType", "EventCreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE INDEX "IX_webhook_events_Status" ON webhook_events ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE INDEX "IX_webhook_events_Status_RetryCount" ON webhook_events ("Status", "RetryCount") WHERE "Status" = 3;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE UNIQUE INDEX "IX_webhook_events_StripeEventId" ON webhook_events ("StripeEventId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    CREATE INDEX "IX_webhook_events_SubscriptionId" ON webhook_events ("SubscriptionId") WHERE "SubscriptionId" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216000001_FixSubscriptionTables') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251216000001_FixSubscriptionTables', '8.0.11');
    END IF;
END $EF$;
COMMIT;

