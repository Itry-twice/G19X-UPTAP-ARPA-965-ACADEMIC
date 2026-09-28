-- ==========================================================================
-- PluriOne — Esquema de base de datos (PostgreSQL 16 + pgvector)
-- Migración: 001_schema.sql
-- Aplicado por docker-entrypoint-initdb.d en orden alfabético.
-- Las extensiones, roles y schemas se crean en 000_roles.sh (antes).
--
-- Cada sección usa SET ROLE para que cada microservicio sea dueño de sus
-- tablas (menor privilegio). Con FORCE ROW LEVEL SECURITY, el dueño también
-- queda sujeto a RLS, que se evalúa con las variables de sesión app.user_id
-- y app.role establecidas por el servicio desde los claims JWT.
-- ==========================================================================

-- ==========================================================================
-- SCHEMA: auth (solo AuthService)
-- ==========================================================================
SET ROLE svc_auth;

CREATE TABLE auth.roles (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        TEXT NOT NULL UNIQUE,
    description TEXT,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE auth.users (
    id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email                 TEXT NOT NULL UNIQUE,
    password_hash         TEXT NOT NULL,
    full_name             TEXT NOT NULL,
    role_id               UUID NOT NULL REFERENCES auth.roles(id),
    employee_code         TEXT UNIQUE,
    is_active             BOOLEAN NOT NULL DEFAULT TRUE,
    must_change_password  BOOLEAN NOT NULL DEFAULT FALSE,
    failed_login_count    INT NOT NULL DEFAULT 0,
    locked_until          TIMESTAMPTZ,
    created_at            TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at            TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE auth.sessions (
    id                 UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id            UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
    refresh_token_hash TEXT NOT NULL,
    ip_address         TEXT,
    user_agent         TEXT,
    created_at         TIMESTAMPTZ NOT NULL DEFAULT now(),
    expires_at         TIMESTAMPTZ NOT NULL,
    revoked_at         TIMESTAMPTZ
);

CREATE TABLE auth.login_attempts (
    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email        TEXT NOT NULL,
    ip_address   TEXT,
    success      BOOLEAN NOT NULL,
    attempted_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE auth.audit_events (
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id        UUID,
    user_name      TEXT NOT NULL DEFAULT '',
    action         TEXT NOT NULL,
    resource       TEXT,
    metadata       TEXT,
    ip_address     TEXT,
    correlation_id TEXT,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_sessions_user ON auth.sessions(user_id);
CREATE INDEX idx_login_attempts_email ON auth.login_attempts(email, attempted_at);
CREATE INDEX idx_audit_created ON auth.audit_events(created_at);

RESET ROLE;

-- El esquema auth está aislado por privilegios (solo svc_auth tiene acceso).
GRANT REFERENCES ON auth.users TO svc_onboarding, svc_hr;

-- ==========================================================================
-- SCHEMA: onboarding
-- ==========================================================================
SET ROLE svc_onboarding;

CREATE TABLE onboarding.employees (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id    UUID NOT NULL UNIQUE REFERENCES auth.users(id) ON DELETE CASCADE,
    full_name  TEXT NOT NULL,
    email      TEXT NOT NULL,
    position   TEXT,
    start_date DATE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE onboarding.plan_phases (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    phase_key  TEXT NOT NULL UNIQUE,
    name       TEXT NOT NULL,
    days_start INT  NOT NULL,
    days_end   INT  NOT NULL,
    objective  TEXT,
    sort_order INT  NOT NULL DEFAULT 0
);

CREATE TABLE onboarding.activities (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    phase_id    UUID NOT NULL REFERENCES onboarding.plan_phases(id) ON DELETE CASCADE,
    title       TEXT NOT NULL,
    description TEXT,
    sort_order  INT NOT NULL DEFAULT 0
);

CREATE TABLE onboarding.tasks (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title       TEXT NOT NULL,
    description TEXT,
    category    TEXT,
    is_required BOOLEAN NOT NULL DEFAULT TRUE,
    sort_order  INT NOT NULL DEFAULT 0
);

CREATE TABLE onboarding.employee_tasks (
    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    employee_id  UUID NOT NULL REFERENCES onboarding.employees(id) ON DELETE CASCADE,
    task_id      UUID NOT NULL REFERENCES onboarding.tasks(id) ON DELETE CASCADE,
    status       TEXT NOT NULL DEFAULT 'pending',
    completed_at TIMESTAMPTZ,
    UNIQUE (employee_id, task_id)
);

CREATE INDEX idx_et_employee ON onboarding.employee_tasks(employee_id);

-- RLS — onboarding
ALTER TABLE onboarding.employees ENABLE ROW LEVEL SECURITY;
ALTER TABLE onboarding.employees FORCE ROW LEVEL SECURITY;

ALTER TABLE onboarding.employee_tasks ENABLE ROW LEVEL SECURITY;
ALTER TABLE onboarding.employee_tasks FORCE ROW LEVEL SECURITY;

ALTER TABLE onboarding.tasks ENABLE ROW LEVEL SECURITY;
ALTER TABLE onboarding.plan_phases ENABLE ROW LEVEL SECURITY;
ALTER TABLE onboarding.activities ENABLE ROW LEVEL SECURITY;

CREATE POLICY cat_read ON onboarding.tasks FOR SELECT
    USING (current_setting('app.user_id', true) IS NOT NULL);
CREATE POLICY cat_write ON onboarding.tasks FOR ALL
    USING (current_setting('app.role', true) IN ('rrhh','admin'))
    WITH CHECK (current_setting('app.role', true) IN ('rrhh','admin'));

CREATE POLICY ph_read ON onboarding.plan_phases FOR SELECT
    USING (current_setting('app.user_id', true) IS NOT NULL);
CREATE POLICY ph_write ON onboarding.plan_phases FOR ALL
    USING (current_setting('app.role', true) IN ('rrhh','admin'))
    WITH CHECK (current_setting('app.role', true) IN ('rrhh','admin'));

CREATE POLICY ac_read ON onboarding.activities FOR SELECT
    USING (current_setting('app.user_id', true) IS NOT NULL);
CREATE POLICY ac_write ON onboarding.activities FOR ALL
    USING (current_setting('app.role', true) IN ('rrhh','admin'))
    WITH CHECK (current_setting('app.role', true) IN ('rrhh','admin'));

CREATE POLICY emp_self ON onboarding.employees FOR ALL
    USING (user_id::text = current_setting('app.user_id', true))
    WITH CHECK (user_id::text = current_setting('app.user_id', true));
CREATE POLICY emp_staff ON onboarding.employees FOR ALL
    USING (current_setting('app.role', true) IN ('rrhh','admin'))
    WITH CHECK (current_setting('app.role', true) IN ('rrhh','admin'));

CREATE POLICY et_self ON onboarding.employee_tasks FOR ALL
    USING (
        employee_id IN (
            SELECT e.id FROM onboarding.employees e
            WHERE e.user_id::text = current_setting('app.user_id', true)
        )
    )
    WITH CHECK (
        employee_id IN (
            SELECT e.id FROM onboarding.employees e
            WHERE e.user_id::text = current_setting('app.user_id', true)
        )
    );
CREATE POLICY et_staff ON onboarding.employee_tasks FOR ALL
    USING (current_setting('app.role', true) IN ('rrhh','admin'))
    WITH CHECK (current_setting('app.role', true) IN ('rrhh','admin'));

RESET ROLE;

-- ==========================================================================
-- SCHEMA: hr
-- ==========================================================================
SET ROLE svc_hr;

CREATE TABLE hr.categories (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        TEXT NOT NULL UNIQUE,
    description TEXT
);

CREATE TABLE hr.tickets (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    subject           TEXT NOT NULL,
    description       TEXT,
    category_id       UUID REFERENCES hr.categories(id),
    requester_user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
    requester_name    TEXT NOT NULL DEFAULT '',
    assignee_user_id  UUID REFERENCES auth.users(id),
    status            TEXT NOT NULL DEFAULT 'open',
    priority          TEXT NOT NULL DEFAULT 'normal',
    source            TEXT NOT NULL DEFAULT 'manual',
    escalated         BOOLEAN NOT NULL DEFAULT false,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at        TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE hr.ticket_comments (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ticket_id       UUID NOT NULL REFERENCES hr.tickets(id) ON DELETE CASCADE,
    author_user_id  UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
    author_name     TEXT NOT NULL DEFAULT '',
    body            TEXT NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE hr.ticket_status_history (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ticket_id         UUID NOT NULL REFERENCES hr.tickets(id) ON DELETE CASCADE,
    from_status       TEXT,
    to_status         TEXT NOT NULL,
    changed_by_user_id UUID REFERENCES auth.users(id),
    changed_at        TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_tickets_requester ON hr.tickets(requester_user_id);
CREATE INDEX idx_comments_ticket ON hr.ticket_comments(ticket_id);

-- RLS — hr
ALTER TABLE hr.tickets ENABLE ROW LEVEL SECURITY;
ALTER TABLE hr.tickets FORCE ROW LEVEL SECURITY;

ALTER TABLE hr.ticket_comments ENABLE ROW LEVEL SECURITY;
ALTER TABLE hr.ticket_comments FORCE ROW LEVEL SECURITY;

ALTER TABLE hr.ticket_status_history ENABLE ROW LEVEL SECURITY;
ALTER TABLE hr.ticket_status_history FORCE ROW LEVEL SECURITY;

ALTER TABLE hr.categories ENABLE ROW LEVEL SECURITY;

CREATE POLICY cat_hr_read ON hr.categories FOR SELECT
    USING (current_setting('app.user_id', true) IS NOT NULL);
CREATE POLICY cat_hr_write ON hr.categories FOR ALL
    USING (current_setting('app.role', true) IN ('rrhh','admin'))
    WITH CHECK (current_setting('app.role', true) IN ('rrhh','admin'));

CREATE POLICY tkt_owner ON hr.tickets FOR ALL
    USING (requester_user_id::text = current_setting('app.user_id', true))
    WITH CHECK (requester_user_id::text = current_setting('app.user_id', true));
CREATE POLICY tkt_staff ON hr.tickets FOR ALL
    USING (current_setting('app.role', true) IN ('rrhh','admin'))
    WITH CHECK (current_setting('app.role', true) IN ('rrhh','admin'));

CREATE POLICY cmt_visible ON hr.ticket_comments FOR ALL
    USING (
        author_user_id::text = current_setting('app.user_id', true)
        OR current_setting('app.role', true) IN ('rrhh','admin')
        OR ticket_id IN (
            SELECT t.id FROM hr.tickets t
            WHERE t.requester_user_id::text = current_setting('app.user_id', true)
        )
    )
    WITH CHECK (
        author_user_id::text = current_setting('app.user_id', true)
        OR current_setting('app.role', true) IN ('rrhh','admin')
    );

CREATE POLICY hst_visible ON hr.ticket_status_history FOR ALL
    USING (
        current_setting('app.role', true) IN ('rrhh','admin')
        OR ticket_id IN (
            SELECT t.id FROM hr.tickets t
            WHERE t.requester_user_id::text = current_setting('app.user_id', true)
        )
    )
    WITH CHECK (
        current_setting('app.role', true) IN ('rrhh','admin')
        OR changed_by_user_id::text = current_setting('app.user_id', true)
    );

RESET ROLE;

-- ==========================================================================
-- SCHEMA: ai
-- ==========================================================================
SET ROLE svc_ai;

CREATE TABLE ai.documents (
    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name         TEXT NOT NULL,
    version      TEXT,
    content_hash TEXT NOT NULL,
    ingested_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    UNIQUE (name, content_hash)
);

CREATE TABLE ai.chunks (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    document_id UUID NOT NULL REFERENCES ai.documents(id) ON DELETE CASCADE,
    chunk_index INT NOT NULL,
    content     TEXT NOT NULL,
    metadata    JSONB,
    embedding   vector(384) NOT NULL,
    UNIQUE (document_id, chunk_index)
);

CREATE INDEX idx_chunks_document ON ai.chunks(document_id);
CREATE INDEX idx_chunks_embedding ON ai.chunks USING hnsw (embedding vector_cosine_ops);

RESET ROLE;
