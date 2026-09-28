#!/bin/bash
# ==========================================================================
# PluriOne — Roles, extensiones y schemas (menor privilegio)
# Migración: 000_roles.sh  (se ejecuta antes que los .sql)
# Las contraseñas de los roles de aplicación provienen del entorno.
# ==========================================================================
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE EXTENSION IF NOT EXISTS pgcrypto;
    CREATE EXTENSION IF NOT EXISTS vector;

    DO \$\$
    BEGIN
       IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'svc_auth') THEN
          CREATE ROLE svc_auth LOGIN PASSWORD '${SVC_AUTH_PASSWORD}';
       END IF;
       IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'svc_onboarding') THEN
          CREATE ROLE svc_onboarding LOGIN PASSWORD '${SVC_ONBOARDING_PASSWORD}';
       END IF;
       IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'svc_hr') THEN
          CREATE ROLE svc_hr LOGIN PASSWORD '${SVC_HR_PASSWORD}';
       END IF;
       IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'svc_ai') THEN
          CREATE ROLE svc_ai LOGIN PASSWORD '${SVC_AI_PASSWORD}';
       END IF;
    END
    \$\$;

    CREATE SCHEMA IF NOT EXISTS auth AUTHORIZATION svc_auth;
    CREATE SCHEMA IF NOT EXISTS onboarding AUTHORIZATION svc_onboarding;
    CREATE SCHEMA IF NOT EXISTS hr AUTHORIZATION svc_hr;
    CREATE SCHEMA IF NOT EXISTS ai AUTHORIZATION svc_ai;

    GRANT USAGE ON SCHEMA auth TO svc_onboarding, svc_hr, svc_ai;
EOSQL
