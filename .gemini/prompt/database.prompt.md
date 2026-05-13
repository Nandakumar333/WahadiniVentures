# WahadiniCryptoQuest - Database Development Prompt

## Context
You are an expert database developer working on the WahadiniCryptoQuest database system. The application uses PostgreSQL with Entity Framework Core and implements a comprehensive role-based authorization system with learning and reward data management capabilities.

## Database Architecture Overview

### Technology Stack
- **PostgreSQL 15+** as primary database
- **Entity Framework Core 8.0** for ORM
- **Code-First Migrations** for schema management
- **Role-Based Security** with hierarchical permissions
- **Soft Deletes** for data retention
- **Audit Fields** for tracking changes

### Schema Structure
```
WahadiniCryptoQuest Database
├── Core Entities
│   ├── users                    # User accounts and authentication
│   ├── accounts                 # Financial accounts (checking, savings, etc.)
│   ├── transactions             # Financial transactions
│   ├── categories               # Transaction categories
│   ├── budgets                  # Budget management
│   └── goals                    # Financial goals
├── Authorization System
│   ├── roles                    # System roles (User, Admin, etc.)
│   ├── role_permissions         # Permission assignments to roles
│   └── user_role_assignments    # User role assignments
└── Support Tables
    ├── audit_logs              # System audit trail
    ├── refresh_tokens           # JWT refresh tokens
    └── email_verification_tokens # Email verification
```

## Core Database Patterns

### 1. Entity Base Pattern
```sql
-- Base pattern for all entities
CREATE TABLE base_entity_pattern (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(255) DEFAULT 'System',
    updated_by VARCHAR(255) DEFAULT 'System',
    is_deleted BOOLEAN DEFAULT FALSE
);

-- Example: Users table with base pattern
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(255) NOT NULL,
    first_name VARCHAR(255) NOT NULL,
    last_name VARCHAR(255) NOT NULL,
    phone_number VARCHAR(50),
    is_email_verified BOOLEAN DEFAULT FALSE,
    is_phone_verified BOOLEAN DEFAULT FALSE,
    is_two_factor_enabled BOOLEAN DEFAULT FALSE,
    status INTEGER NOT NULL DEFAULT 1, -- UserStatus enum
    last_login_at TIMESTAMP,
    failed_login_attempts INTEGER DEFAULT 0,
    lockout_end TIMESTAMP,
    security_stamp VARCHAR(255) NOT NULL DEFAULT gen_random_uuid()::TEXT,
    timezone VARCHAR(50) DEFAULT 'UTC',
    culture VARCHAR(10) DEFAULT 'en-US',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(255) DEFAULT 'System',
    updated_by VARCHAR(255) DEFAULT 'System',
    is_deleted BOOLEAN DEFAULT FALSE
);

-- Indexes for performance
CREATE INDEX idx_users_email ON users(email) WHERE NOT is_deleted;
CREATE INDEX idx_users_status ON users(status) WHERE NOT is_deleted;
CREATE INDEX idx_users_security_stamp ON users(security_stamp);
```

### 2. Financial Entities
```sql
-- Accounts table
CREATE TABLE accounts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id),
    name VARCHAR(255) NOT NULL,
    account_type INTEGER NOT NULL, -- AccountType enum
    balance DECIMAL(18,2) NOT NULL DEFAULT 0,
    currency VARCHAR(3) DEFAULT 'USD',
    description TEXT,
    account_number VARCHAR(255),
    institution VARCHAR(255),
    include_in_net_worth BOOLEAN DEFAULT TRUE,
    color VARCHAR(7) DEFAULT '#3B82F6',
    icon VARCHAR(50) DEFAULT 'bank',
    display_order INTEGER DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE,
    
    CONSTRAINT chk_accounts_balance CHECK (balance >= -999999999.99),
    CONSTRAINT chk_accounts_currency CHECK (LENGTH(currency) = 3)
);

-- Transactions table
CREATE TABLE transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id),
    account_id UUID NOT NULL REFERENCES accounts(id),
    category_id UUID REFERENCES categories(id),
    amount DECIMAL(18,2) NOT NULL,
    transaction_type INTEGER NOT NULL, -- TransactionType enum
    description VARCHAR(500) NOT NULL,
    transaction_date TIMESTAMP NOT NULL,
    status INTEGER NOT NULL DEFAULT 1, -- TransactionStatus enum
    reference_number VARCHAR(255),
    notes TEXT,
    tags TEXT[], -- PostgreSQL array type
    is_recurring BOOLEAN DEFAULT FALSE,
    recurring_rule JSONB, -- Store recurring rules as JSON
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE,
    
    CONSTRAINT chk_transactions_amount CHECK (amount > 0),
    CONSTRAINT chk_transactions_description CHECK (LENGTH(TRIM(description)) > 0)
);

-- Categories table
CREATE TABLE categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id), -- NULL for system categories
    name VARCHAR(255) NOT NULL,
    description TEXT,
    color VARCHAR(7) NOT NULL DEFAULT '#6B7280',
    icon VARCHAR(50) NOT NULL DEFAULT 'folder',
    parent_id UUID REFERENCES categories(id),
    transaction_type INTEGER NOT NULL, -- TransactionType enum
    is_system_category BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    display_order INTEGER DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE,
    
    CONSTRAINT chk_categories_color CHECK (color ~ '^#[0-9A-Fa-f]{6}$'),
    CONSTRAINT chk_categories_no_self_parent CHECK (id != parent_id)
);

-- Budgets table
CREATE TABLE budgets (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    amount DECIMAL(18,2) NOT NULL,
    period INTEGER NOT NULL, -- BudgetPeriod enum
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    category_ids UUID[], -- Array of category IDs
    alert_thresholds JSONB, -- Store alert settings as JSON
    status INTEGER NOT NULL DEFAULT 1, -- BudgetStatus enum
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE,
    
    CONSTRAINT chk_budgets_amount CHECK (amount > 0),
    CONSTRAINT chk_budgets_dates CHECK (end_date > start_date),
    CONSTRAINT chk_budgets_name CHECK (LENGTH(TRIM(name)) > 0)
);
```

### 3. Role-Based Authorization System
```sql
-- Roles table (SINGULAR name - CRITICAL!)
CREATE TABLE roles (
    id UUID PRIMARY KEY,
    role_type INTEGER NOT NULL UNIQUE, -- UserRole enum
    name VARCHAR(255) NOT NULL UNIQUE,
    description TEXT,
    is_system_role BOOLEAN DEFAULT TRUE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT chk_roles_name CHECK (LENGTH(TRIM(name)) > 0)
);

-- Role permissions table (SINGULAR name - CRITICAL!)
CREATE TABLE role_permissions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    permission INTEGER NOT NULL, -- Permission enum
    description VARCHAR(500),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uk_role_permissions_role_permission UNIQUE (role_id, permission)
);

-- User role assignments table (SINGULAR name - CRITICAL!)
CREATE TABLE user_role_assignments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id),
    role_id UUID NOT NULL REFERENCES roles(id),
    assigned_by UUID REFERENCES users(id),
    assigned_at TIMESTAMP NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMP,
    assignment_reason VARCHAR(500),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uk_user_role_assignments UNIQUE (user_id, role_id) DEFERRABLE INITIALLY DEFERRED
);
```

## Role and Permission System Implementation

### System Roles Hierarchy
```sql
-- Role definitions with specific UUIDs
INSERT INTO roles (id, role_type, name, description, is_system_role, is_active) VALUES
('11111111-1111-1111-1111-111111111111', 1, 'User', 'Standard user with basic financial management capabilities', true, true),
('22222222-2222-2222-2222-222222222222', 2, 'Employee', 'Employee with extended access and basic administrative features', true, true),
('33333333-3333-3333-3333-333333333333', 3, 'Manager', 'Manager with comprehensive access to reports and team management', true, true),
('44444444-4444-4444-4444-444444444444', 4, 'Admin', 'Administrator with full system management capabilities', true, true),
('55555555-5555-5555-5555-555555555555', 5, 'SuperAdmin', 'Super administrator with complete system access', true, true);
```

### Permission Categories
```sql
-- User Management Permissions (1-6)
-- Account Management Permissions (10-14)
-- Transaction Management Permissions (20-24)
-- Budget Management Permissions (30-34)
-- Category Management Permissions (40-44)
-- Goal Management Permissions (50-54)
-- Report Management Permissions (60-63)
-- System Administration Permissions (70-77)
-- Notification Management Permissions (80-83)

-- Example: Insert User role permissions (23 total)
INSERT INTO role_permissions (role_id, permission, description) VALUES
-- User permissions include basic financial management
('11111111-1111-1111-1111-111111111111', 10, 'View personal financial accounts'),
('11111111-1111-1111-1111-111111111111', 11, 'Create new financial accounts'),
('11111111-1111-1111-1111-111111111111', 12, 'Edit account information and settings'),
('11111111-1111-1111-1111-111111111111', 13, 'Delete financial accounts'),
('11111111-1111-1111-1111-111111111111', 20, 'View personal financial transactions'),
-- ... continue with all 23 permissions
;
```

### Hierarchical Permission Structure
```sql
-- Query to show role hierarchy
WITH role_hierarchy AS (
    SELECT 
        r.name as role_name,
        r.role_type,
        COUNT(rp.permission) as permission_count
    FROM roles r
    LEFT JOIN role_permissions rp ON r.id = rp.role_id AND rp.is_active = true
    WHERE r.is_active = true
    GROUP BY r.id, r.name, r.role_type
    ORDER BY r.role_type
)
SELECT 
    role_name,
    permission_count,
    CASE 
        WHEN role_type = 1 THEN 'Base permissions'
        WHEN role_type = 2 THEN 'User + ' || (permission_count - 23) || ' additional'
        WHEN role_type = 3 THEN 'Employee + ' || (permission_count - 27) || ' additional'
        WHEN role_type = 4 THEN 'Manager + ' || (permission_count - 34) || ' additional'
        WHEN role_type = 5 THEN 'Admin + ' || (permission_count - 46) || ' additional'
    END as permission_structure
FROM role_hierarchy;
```

## Advanced Database Features

### 1. Triggers and Functions
```sql
-- Auto-update timestamp trigger
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Apply to all tables
CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_accounts_updated_at BEFORE UPDATE ON accounts
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_transactions_updated_at BEFORE UPDATE ON transactions
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Account balance update trigger
CREATE OR REPLACE FUNCTION update_account_balance()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        UPDATE accounts 
        SET balance = CASE 
            WHEN NEW.transaction_type = 1 THEN balance + NEW.amount  -- Income
            WHEN NEW.transaction_type = 2 THEN balance - NEW.amount  -- Expense
            ELSE balance  -- Transfer handled separately
        END,
        updated_at = NOW()
        WHERE id = NEW.account_id;
        RETURN NEW;
    ELSIF TG_OP = 'UPDATE' THEN
        -- Revert old transaction
        UPDATE accounts 
        SET balance = CASE 
            WHEN OLD.transaction_type = 1 THEN balance - OLD.amount
            WHEN OLD.transaction_type = 2 THEN balance + OLD.amount
            ELSE balance
        END
        WHERE id = OLD.account_id;
        
        -- Apply new transaction
        UPDATE accounts 
        SET balance = CASE 
            WHEN NEW.transaction_type = 1 THEN balance + NEW.amount
            WHEN NEW.transaction_type = 2 THEN balance - NEW.amount
            ELSE balance
        END,
        updated_at = NOW()
        WHERE id = NEW.account_id;
        RETURN NEW;
    ELSIF TG_OP = 'DELETE' THEN
        UPDATE accounts 
        SET balance = CASE 
            WHEN OLD.transaction_type = 1 THEN balance - OLD.amount
            WHEN OLD.transaction_type = 2 THEN balance + OLD.amount
            ELSE balance
        END,
        updated_at = NOW()
        WHERE id = OLD.account_id;
        RETURN OLD;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_account_balance
    AFTER INSERT OR UPDATE OR DELETE ON transactions
    FOR EACH ROW EXECUTE FUNCTION update_account_balance();
```

### 2. Performance Indexes
```sql
-- Core performance indexes
CREATE INDEX idx_accounts_user_id ON accounts(user_id) WHERE NOT is_deleted;
CREATE INDEX idx_accounts_type ON accounts(account_type) WHERE NOT is_deleted;

CREATE INDEX idx_transactions_user_id ON transactions(user_id) WHERE NOT is_deleted;
CREATE INDEX idx_transactions_account_id ON transactions(account_id) WHERE NOT is_deleted;
CREATE INDEX idx_transactions_category_id ON transactions(category_id) WHERE NOT is_deleted;
CREATE INDEX idx_transactions_date ON transactions(transaction_date) WHERE NOT is_deleted;
CREATE INDEX idx_transactions_type ON transactions(transaction_type) WHERE NOT is_deleted;

-- Composite indexes for common queries
CREATE INDEX idx_transactions_user_date ON transactions(user_id, transaction_date) WHERE NOT is_deleted;
CREATE INDEX idx_transactions_account_date ON transactions(account_id, transaction_date) WHERE NOT is_deleted;
CREATE INDEX idx_transactions_user_type_date ON transactions(user_id, transaction_type, transaction_date) WHERE NOT is_deleted;

-- Text search indexes
CREATE INDEX idx_transactions_description ON transactions USING gin(to_tsvector('english', description)) WHERE NOT is_deleted;
CREATE INDEX idx_transactions_tags ON transactions USING gin(tags) WHERE NOT is_deleted;

-- Authorization system indexes
CREATE INDEX idx_user_role_assignments_user ON user_role_assignments(user_id) WHERE is_active = true;
CREATE INDEX idx_user_role_assignments_role ON user_role_assignments(role_id) WHERE is_active = true;
CREATE INDEX idx_role_permissions_role ON role_permissions(role_id) WHERE is_active = true;
CREATE INDEX idx_role_permissions_permission ON role_permissions(permission) WHERE is_active = true;
```

### 3. Data Validation Constraints
```sql
-- Email format validation
ALTER TABLE users ADD CONSTRAINT chk_users_email 
    CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$');

-- Phone number format (international)
ALTER TABLE users ADD CONSTRAINT chk_users_phone 
    CHECK (phone_number IS NULL OR phone_number ~ '^\+?[1-9]\d{1,14}$');

-- Currency code validation (ISO 4217)
ALTER TABLE accounts ADD CONSTRAINT chk_accounts_currency_iso 
    CHECK (currency ~ '^[A-Z]{3}$');

-- Color hex code validation
ALTER TABLE categories ADD CONSTRAINT chk_categories_color_hex 
    CHECK (color ~ '^#[0-9A-Fa-f]{6}$');

-- Date range validation for budgets
ALTER TABLE budgets ADD CONSTRAINT chk_budgets_date_range 
    CHECK (end_date > start_date AND start_date >= CURRENT_DATE - INTERVAL '10 years');

-- Amount validation
ALTER TABLE transactions ADD CONSTRAINT chk_transactions_amount_range 
    CHECK (amount BETWEEN 0.01 AND 999999999.99);

ALTER TABLE budgets ADD CONSTRAINT chk_budgets_amount_range 
    CHECK (amount BETWEEN 0.01 AND 999999999.99);
```

### 4. Audit and Security
```sql
-- Audit log table
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id),
    table_name VARCHAR(255) NOT NULL,
    record_id UUID NOT NULL,
    action VARCHAR(50) NOT NULL, -- INSERT, UPDATE, DELETE
    old_values JSONB,
    new_values JSONB,
    changed_fields TEXT[],
    ip_address INET,
    user_agent TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Audit trigger function
CREATE OR REPLACE FUNCTION audit_trigger_function()
RETURNS TRIGGER AS $$
DECLARE
    old_data JSONB;
    new_data JSONB;
    changed_fields TEXT[];
BEGIN
    IF TG_OP = 'DELETE' THEN
        old_data = to_jsonb(OLD);
        INSERT INTO audit_logs (table_name, record_id, action, old_values)
        VALUES (TG_TABLE_NAME, OLD.id, TG_OP, old_data);
        RETURN OLD;
    ELSIF TG_OP = 'UPDATE' THEN
        old_data = to_jsonb(OLD);
        new_data = to_jsonb(NEW);
        
        -- Identify changed fields
        SELECT array_agg(key) INTO changed_fields
        FROM jsonb_each(old_data) old_kv
        JOIN jsonb_each(new_data) new_kv ON old_kv.key = new_kv.key
        WHERE old_kv.value IS DISTINCT FROM new_kv.value;
        
        INSERT INTO audit_logs (table_name, record_id, action, old_values, new_values, changed_fields)
        VALUES (TG_TABLE_NAME, NEW.id, TG_OP, old_data, new_data, changed_fields);
        RETURN NEW;
    ELSIF TG_OP = 'INSERT' THEN
        new_data = to_jsonb(NEW);
        INSERT INTO audit_logs (table_name, record_id, action, new_values)
        VALUES (TG_TABLE_NAME, NEW.id, TG_OP, new_data);
        RETURN NEW;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Apply audit triggers to sensitive tables
CREATE TRIGGER audit_users AFTER INSERT OR UPDATE OR DELETE ON users
    FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

CREATE TRIGGER audit_transactions AFTER INSERT OR UPDATE OR DELETE ON transactions
    FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

CREATE TRIGGER audit_user_role_assignments AFTER INSERT OR UPDATE OR DELETE ON user_role_assignments
    FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();
```

## Query Patterns and Optimization

### 1. Common Financial Queries
```sql
-- Monthly spending by category
WITH monthly_spending AS (
    SELECT 
        c.name as category_name,
        DATE_TRUNC('month', t.transaction_date) as month,
        SUM(t.amount) as total_spent
    FROM transactions t
    JOIN categories c ON t.category_id = c.id
    WHERE t.user_id = $1 
      AND t.transaction_type = 2 -- Expense
      AND t.transaction_date >= DATE_TRUNC('year', CURRENT_DATE)
      AND NOT t.is_deleted
    GROUP BY c.id, c.name, DATE_TRUNC('month', t.transaction_date)
)
SELECT 
    category_name,
    month,
    total_spent,
    LAG(total_spent) OVER (PARTITION BY category_name ORDER BY month) as previous_month,
    CASE 
        WHEN LAG(total_spent) OVER (PARTITION BY category_name ORDER BY month) > 0 
        THEN ROUND(((total_spent - LAG(total_spent) OVER (PARTITION BY category_name ORDER BY month)) / 
                   LAG(total_spent) OVER (PARTITION BY category_name ORDER BY month) * 100), 2)
        ELSE NULL
    END as percentage_change
FROM monthly_spending
ORDER BY month DESC, total_spent DESC;

-- Account balance history
WITH daily_balances AS (
    SELECT 
        a.id as account_id,
        a.name as account_name,
        d.date,
        COALESCE(SUM(
            CASE 
                WHEN t.transaction_type = 1 THEN t.amount -- Income
                WHEN t.transaction_type = 2 THEN -t.amount -- Expense
                ELSE 0
            END
        ) OVER (PARTITION BY a.id ORDER BY d.date), 0) + 
        (SELECT COALESCE(SUM(
            CASE 
                WHEN transaction_type = 1 THEN amount
                WHEN transaction_type = 2 THEN -amount
                ELSE 0
            END
        ), 0) FROM transactions 
         WHERE account_id = a.id 
           AND transaction_date < d.date 
           AND NOT is_deleted) as running_balance
    FROM accounts a
    CROSS JOIN (
        SELECT generate_series(
            CURRENT_DATE - INTERVAL '30 days',
            CURRENT_DATE,
            INTERVAL '1 day'
        )::DATE as date
    ) d
    LEFT JOIN transactions t ON t.account_id = a.id 
                            AND t.transaction_date::DATE = d.date
                            AND NOT t.is_deleted
    WHERE a.user_id = $1 AND NOT a.is_deleted
)
SELECT account_id, account_name, date, running_balance
FROM daily_balances
ORDER BY account_name, date;

-- Budget vs actual spending
SELECT 
    b.name as budget_name,
    b.amount as budgeted_amount,
    COALESCE(SUM(t.amount), 0) as actual_spent,
    b.amount - COALESCE(SUM(t.amount), 0) as remaining,
    CASE 
        WHEN b.amount > 0 THEN ROUND((COALESCE(SUM(t.amount), 0) / b.amount * 100), 2)
        ELSE 0
    END as percentage_used,
    b.start_date,
    b.end_date
FROM budgets b
LEFT JOIN transactions t ON t.user_id = b.user_id
                        AND t.category_id = ANY(b.category_ids)
                        AND t.transaction_date BETWEEN b.start_date AND b.end_date
                        AND t.transaction_type = 2 -- Expense
                        AND NOT t.is_deleted
WHERE b.user_id = $1 
  AND NOT b.is_deleted
  AND b.status = 1 -- Active
  AND CURRENT_DATE BETWEEN b.start_date AND b.end_date
GROUP BY b.id, b.name, b.amount, b.start_date, b.end_date
ORDER BY percentage_used DESC;
```

### 2. Authorization Queries
```sql
-- Get user permissions
WITH user_permissions AS (
    SELECT DISTINCT rp.permission
    FROM user_role_assignments ura
    JOIN role_permissions rp ON ura.role_id = rp.role_id
    WHERE ura.user_id = $1 
      AND ura.is_active = true
      AND rp.is_active = true
      AND (ura.expires_at IS NULL OR ura.expires_at > NOW())
)
SELECT permission FROM user_permissions ORDER BY permission;

-- Check specific permission
SELECT EXISTS (
    SELECT 1
    FROM user_role_assignments ura
    JOIN role_permissions rp ON ura.role_id = rp.role_id
    WHERE ura.user_id = $1
      AND rp.permission = $2
      AND ura.is_active = true
      AND rp.is_active = true
      AND (ura.expires_at IS NULL OR ura.expires_at > NOW())
) as has_permission;

-- Get user roles with permissions
SELECT 
    r.name as role_name,
    r.description as role_description,
    array_agg(rp.permission ORDER BY rp.permission) as permissions
FROM user_role_assignments ura
JOIN roles r ON ura.role_id = r.id
JOIN role_permissions rp ON r.id = rp.role_id
WHERE ura.user_id = $1
  AND ura.is_active = true
  AND r.is_active = true
  AND rp.is_active = true
  AND (ura.expires_at IS NULL OR ura.expires_at > NOW())
GROUP BY r.id, r.name, r.description
ORDER BY r.role_type;
```

## Migration Patterns

### 1. EF Core Migration Template
```csharp
public partial class AddTransactionTagsAndRecurring : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add new columns
        migrationBuilder.AddColumn<string[]>(
            name: "tags",
            table: "transactions",
            type: "text[]",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "is_recurring",
            table: "transactions",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "recurring_rule",
            table: "transactions",
            type: "jsonb",
            nullable: true);

        // Add indexes
        migrationBuilder.Sql(@"
            CREATE INDEX idx_transactions_tags 
            ON transactions USING gin(tags) 
            WHERE NOT is_deleted;
        ");

        // Add constraints
        migrationBuilder.Sql(@"
            ALTER TABLE transactions 
            ADD CONSTRAINT chk_transactions_recurring_rule 
            CHECK (
                (is_recurring = false AND recurring_rule IS NULL) OR
                (is_recurring = true AND recurring_rule IS NOT NULL)
            );
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS idx_transactions_tags;");
        
        migrationBuilder.DropColumn(name: "tags", table: "transactions");
        migrationBuilder.DropColumn(name: "is_recurring", table: "transactions");
        migrationBuilder.DropColumn(name: "recurring_rule", table: "transactions");
    }
}
```

### 2. Data Migration Patterns
```sql
-- Safe data migration pattern
DO $$
DECLARE
    batch_size INTEGER := 1000;
    processed INTEGER := 0;
    total_records INTEGER;
BEGIN
    -- Get total count
    SELECT COUNT(*) INTO total_records FROM old_table WHERE needs_migration = true;
    
    RAISE NOTICE 'Starting migration of % records', total_records;
    
    WHILE processed < total_records LOOP
        -- Process in batches
        WITH batch AS (
            SELECT id FROM old_table 
            WHERE needs_migration = true 
            ORDER BY id 
            LIMIT batch_size 
            OFFSET processed
        )
        UPDATE old_table SET 
            new_column = transform_function(old_column),
            needs_migration = false,
            updated_at = NOW()
        WHERE id IN (SELECT id FROM batch);
        
        processed := processed + batch_size;
        RAISE NOTICE 'Processed % of % records', processed, total_records;
        
        -- Commit batch
        COMMIT;
    END LOOP;
    
    RAISE NOTICE 'Migration completed successfully';
END;
$$;
```

## Best Practices

### 1. Security
- **Use SINGULAR table names** (roles, not Roles) to match actual schema
- **Implement row-level security** for multi-tenant data
- **Use UUID primary keys** to prevent enumeration attacks
- **Encrypt sensitive data** at rest and in transit
- **Audit all data changes** especially financial transactions

### 2. Performance
- **Index foreign keys** and frequently queried columns
- **Use partial indexes** with WHERE clauses for soft deletes
- **Implement query timeout limits** to prevent runaway queries
- **Use EXPLAIN ANALYZE** to optimize query performance
- **Partition large tables** by date for historical data

### 3. Data Integrity
- **Use check constraints** for business rule validation
- **Implement triggers** for automatic data updates
- **Use transactions** for multi-table operations
- **Validate data types** and formats at database level
- **Implement soft deletes** for data retention

### 4. Maintenance
- **Regular VACUUM and ANALYZE** for query optimization
- **Monitor index usage** and remove unused indexes
- **Archive old data** to separate tables or databases
- **Backup and test restore procedures** regularly
- **Document schema changes** in migration files

## Instructions

When working on the WahadiniCryptoQuest database:

1. **Use Singular Table Names**: Always reference `roles`, `role_permissions`, and `user_role_assignments` (not plural)
2. **Follow Base Entity Pattern**: Include id, created_at, updated_at, and is_deleted columns
3. **Implement Proper Constraints**: Add check constraints for business rules validation
4. **Index Performance-Critical Queries**: Create indexes for commonly used query patterns
5. **Use Transactions**: Wrap multi-table operations in database transactions
6. **Audit Sensitive Operations**: Log all changes to learning and reward data and user permissions
7. **Validate at Database Level**: Implement constraints and triggers for data integrity
8. **Test Migrations**: Always test migrations on a copy of production data
9. **Document Changes**: Include clear comments in migration files
10. **Monitor Performance**: Use EXPLAIN ANALYZE to validate query performance

Always consider data consistency, security, and performance implications when making database changes.
