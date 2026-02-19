-- =============================================================================
-- SaaS Boilerplate - Row Level Security (RLS) Policies
-- =============================================================================
-- Run this script AFTER migrations have created the tables
-- RLS provides defense-in-depth: even raw SQL cannot cross tenant boundaries
-- =============================================================================

-- =============================================================================
-- HELPER: Create RLS policy for a tenant-scoped table
-- =============================================================================
CREATE OR REPLACE FUNCTION create_tenant_rls_policy(table_name text)
RETURNS void AS $$
BEGIN
    -- Enable RLS on the table
    EXECUTE format('ALTER TABLE %I ENABLE ROW LEVEL SECURITY', table_name);
    
    -- Force RLS for table owner too (important for superuser connections)
    EXECUTE format('ALTER TABLE %I FORCE ROW LEVEL SECURITY', table_name);
    
    -- Drop existing policy if it exists
    EXECUTE format('DROP POLICY IF EXISTS tenant_isolation_policy ON %I', table_name);
    
    -- Create the tenant isolation policy
    -- Uses app.current_tenant_id session variable set by RlsInterceptor
    EXECUTE format(
        'CREATE POLICY tenant_isolation_policy ON %I
         FOR ALL
         USING (tenant_id = NULLIF(current_setting(''app.current_tenant_id'', true), '''')::uuid)
         WITH CHECK (tenant_id = NULLIF(current_setting(''app.current_tenant_id'', true), '''')::uuid)',
        table_name
    );
    
    RAISE NOTICE 'RLS policy created for table: %', table_name;
END;
$$ LANGUAGE plpgsql;

-- =============================================================================
-- HELPER: Create bypass policy for platform admins (optional)
-- =============================================================================
CREATE OR REPLACE FUNCTION create_platform_admin_bypass_policy(table_name text)
RETURNS void AS $$
BEGIN
    -- Drop existing bypass policy if it exists
    EXECUTE format('DROP POLICY IF EXISTS platform_admin_bypass ON %I', table_name);
    
    -- Create bypass policy for platform admins
    -- Platform admins have current_setting('app.is_platform_admin') = 'true'
    EXECUTE format(
        'CREATE POLICY platform_admin_bypass ON %I
         FOR ALL
         USING (current_setting(''app.is_platform_admin'', true) = ''true'')
         WITH CHECK (current_setting(''app.is_platform_admin'', true) = ''true'')',
        table_name
    );
    
    RAISE NOTICE 'Platform admin bypass policy created for table: %', table_name;
END;
$$ LANGUAGE plpgsql;

-- =============================================================================
-- APPLY RLS POLICIES TO TENANT-SCOPED TABLES
-- =============================================================================
-- Note: These tables must exist before running this script
-- Run after EF migrations: dotnet ef database update

-- Identity Module
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'tenant_users') THEN
        PERFORM create_tenant_rls_policy('tenant_users');
        PERFORM create_platform_admin_bypass_policy('tenant_users');
    END IF;
END $$;

-- Authorization Module
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'roles') THEN
        PERFORM create_tenant_rls_policy('roles');
        PERFORM create_platform_admin_bypass_policy('roles');
    END IF;
END $$;

DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'user_roles') THEN
        PERFORM create_tenant_rls_policy('user_roles');
        PERFORM create_platform_admin_bypass_policy('user_roles');
    END IF;
END $$;

DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'permissions') THEN
        PERFORM create_tenant_rls_policy('permissions');
        PERFORM create_platform_admin_bypass_policy('permissions');
    END IF;
END $$;

DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'role_permissions') THEN
        PERFORM create_tenant_rls_policy('role_permissions');
        PERFORM create_platform_admin_bypass_policy('role_permissions');
    END IF;
END $$;

-- API Management Module
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'api_keys') THEN
        PERFORM create_tenant_rls_policy('api_keys');
        PERFORM create_platform_admin_bypass_policy('api_keys');
    END IF;
END $$;

-- Notification Module
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'notifications') THEN
        PERFORM create_tenant_rls_policy('notifications');
        PERFORM create_platform_admin_bypass_policy('notifications');
    END IF;
END $$;

DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'notification_preferences') THEN
        PERFORM create_tenant_rls_policy('notification_preferences');
        PERFORM create_platform_admin_bypass_policy('notification_preferences');
    END IF;
END $$;

-- Subscription Module
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'subscriptions') THEN
        PERFORM create_tenant_rls_policy('subscriptions');
        PERFORM create_platform_admin_bypass_policy('subscriptions');
    END IF;
END $$;

DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'invoices') THEN
        PERFORM create_tenant_rls_policy('invoices');
        PERFORM create_platform_admin_bypass_policy('invoices');
    END IF;
END $$;

DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'usage_records') THEN
        PERFORM create_tenant_rls_policy('usage_records');
        PERFORM create_platform_admin_bypass_policy('usage_records');
    END IF;
END $$;

-- Audit Module
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'audit_logs') THEN
        PERFORM create_tenant_rls_policy('audit_logs');
        PERFORM create_platform_admin_bypass_policy('audit_logs');
    END IF;
END $$;

-- Feature Flags Module
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'feature_flags') THEN
        PERFORM create_tenant_rls_policy('feature_flags');
        PERFORM create_platform_admin_bypass_policy('feature_flags');
    END IF;
END $$;

DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'feature_flag_overrides') THEN
        PERFORM create_tenant_rls_policy('feature_flag_overrides');
        PERFORM create_platform_admin_bypass_policy('feature_flag_overrides');
    END IF;
END $$;

-- Content Module
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'pages') THEN
        PERFORM create_tenant_rls_policy('pages');
        PERFORM create_platform_admin_bypass_policy('pages');
    END IF;
END $$;

DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'media') THEN
        PERFORM create_tenant_rls_policy('media');
        PERFORM create_platform_admin_bypass_policy('media');
    END IF;
END $$;

-- =============================================================================
-- VERIFICATION QUERY
-- =============================================================================
-- Run this to verify RLS is enabled on tables:
-- SELECT schemaname, tablename, rowsecurity 
-- FROM pg_tables 
-- WHERE schemaname = 'public' AND rowsecurity = true;

-- =============================================================================
-- TESTING RLS
-- =============================================================================
-- To test RLS is working:
-- 
-- 1. Without tenant context (should return 0 rows):
--    SELECT * FROM tenant_users;
--
-- 2. With tenant context (should return only that tenant's rows):
--    SET app.current_tenant_id = 'your-tenant-uuid';
--    SELECT * FROM tenant_users;
--
-- 3. As platform admin (should return all rows):
--    SET app.is_platform_admin = 'true';
--    SELECT * FROM tenant_users;

DO $$
BEGIN
    RAISE NOTICE '==============================================';
    RAISE NOTICE 'RLS policies script completed';
    RAISE NOTICE 'Run verification query to confirm RLS is active';
    RAISE NOTICE '==============================================';
END $$;
