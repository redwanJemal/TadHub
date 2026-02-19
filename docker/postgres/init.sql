-- =============================================================================
-- SaaS Boilerplate - PostgreSQL Initialization Script
-- =============================================================================
-- This script runs on first database creation
-- =============================================================================

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";  -- For text search optimization

-- Create a function to set app.current_tenant_id for RLS
CREATE OR REPLACE FUNCTION set_tenant_id(tenant_id uuid)
RETURNS void AS $$
BEGIN
    PERFORM set_config('app.current_tenant_id', tenant_id::text, false);
END;
$$ LANGUAGE plpgsql;

-- Create a function to get the current tenant_id
CREATE OR REPLACE FUNCTION get_tenant_id()
RETURNS uuid AS $$
BEGIN
    RETURN NULLIF(current_setting('app.current_tenant_id', true), '')::uuid;
EXCEPTION
    WHEN OTHERS THEN
        RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Create audit trigger function for tracking changes
CREATE OR REPLACE FUNCTION audit_trigger_func()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        NEW.created_at = COALESCE(NEW.created_at, NOW());
        NEW.updated_at = NEW.created_at;
        RETURN NEW;
    ELSIF TG_OP = 'UPDATE' THEN
        NEW.updated_at = NOW();
        RETURN NEW;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Create soft delete function
CREATE OR REPLACE FUNCTION soft_delete_trigger_func()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.is_deleted = false AND NEW.is_deleted = true THEN
        NEW.deleted_at = NOW();
    ELSIF OLD.is_deleted = true AND NEW.is_deleted = false THEN
        NEW.deleted_at = NULL;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- =============================================================================
-- RLS HELPER: Verify tenant context is set
-- =============================================================================
CREATE OR REPLACE FUNCTION require_tenant_context()
RETURNS void AS $$
BEGIN
    IF NULLIF(current_setting('app.current_tenant_id', true), '') IS NULL THEN
        RAISE EXCEPTION 'Tenant context not set. Call set_tenant_id() first.';
    END IF;
END;
$$ LANGUAGE plpgsql;

-- =============================================================================
-- Log successful initialization
-- =============================================================================
DO $$
BEGIN
    RAISE NOTICE '==============================================';
    RAISE NOTICE 'SaaS Boilerplate database initialized';
    RAISE NOTICE '==============================================';
    RAISE NOTICE 'Extensions: uuid-ossp, pgcrypto, pg_trgm';
    RAISE NOTICE 'Functions: set_tenant_id, get_tenant_id, require_tenant_context';
    RAISE NOTICE 'Functions: audit_trigger_func, soft_delete_trigger_func';
    RAISE NOTICE '';
    RAISE NOTICE 'NEXT STEPS:';
    RAISE NOTICE '1. Run EF migrations: dotnet ef database update';
    RAISE NOTICE '2. Apply RLS policies: psql -f rls-policies.sql';
    RAISE NOTICE '==============================================';
END $$;
