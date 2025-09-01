-- ==========================================
-- Multi-tenant installation script
-- ==========================================

-- 1. Generic trigger function
CREATE OR REPLACE FUNCTION enforce_tenant_id_generic()
RETURNS TRIGGER AS $$
BEGIN
  -- INSERT: require tenant_id is provided explicitly
  IF TG_OP = 'INSERT' THEN
    IF NEW.tenant_id IS NULL THEN
      RAISE EXCEPTION 'tenant_id must be supplied on insert';
    END IF;
  ELSIF TG_OP = 'UPDATE' THEN
    -- Prevent cross-tenant changes
    IF NEW.tenant_id <> OLD.tenant_id THEN
      RAISE EXCEPTION 'Cannot change tenant_id';
    END IF;
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- 2. Apply RLS + Trigger to all tables with tenant_id
DO $$
DECLARE
  t record;
  trigger_name text := 'tenant_id_enforcer';
  policy_name  text := 'tenant_policy';
  excluded_tables text[] := ARRAY[
    '__EFMigrationsHistory',
    'app_installation_status',
    'tenants',
    'dynamic_object_field_types'
  ]; -- exceptions
BEGIN
  FOR t IN
    SELECT table_schema, table_name
    FROM information_schema.columns
    WHERE column_name = 'tenant_id'
      AND table_schema = 'public'
      AND table_name <> ALL(excluded_tables)
  LOOP
    RAISE NOTICE 'Processing table: %', t.table_name;

    -- Enable RLS
    EXECUTE format('ALTER TABLE %I ENABLE ROW LEVEL SECURITY', t.table_name);
    EXECUTE format('ALTER TABLE %I FORCE ROW LEVEL SECURITY', t.table_name);

    -- Drop old policy if exists
    IF EXISTS (
      SELECT 1
      FROM pg_policies
      WHERE schemaname = t.table_schema
        AND tablename = t.table_name
        AND policyname = policy_name
    ) THEN
      EXECUTE format('DROP POLICY %I ON %I', policy_name, t.table_name);
    END IF;

    -- Create fresh policy: require tenant_id is not null
    EXECUTE format(
      'CREATE POLICY %I ON %I
       USING (tenant_id IS NOT NULL)
       WITH CHECK (tenant_id IS NOT NULL)',
      policy_name, t.table_name
    );

    -- Drop old trigger if exists
    IF EXISTS (
      SELECT 1
      FROM pg_trigger
      WHERE tgrelid = (t.table_schema||'.'||t.table_name)::regclass
        AND tgname = trigger_name
    ) THEN
      EXECUTE format('DROP TRIGGER %I ON %I', trigger_name, t.table_name);
    END IF;

    -- Create trigger
    EXECUTE format(
      'CREATE TRIGGER %I
       BEFORE INSERT OR UPDATE ON %I
       FOR EACH ROW
       EXECUTE FUNCTION enforce_tenant_id_generic()',
      trigger_name, t.table_name
    );

  END LOOP;
END;
$$;
