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

