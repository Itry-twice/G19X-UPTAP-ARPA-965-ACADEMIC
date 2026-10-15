# ADR-0004 — Esquema de base de datos gestionado por migraciones SQL

- **Estado:** Aceptado

## Contexto

Tres servicios .NET y un servicio Python comparten PostgreSQL con pgvector. Coordinar migraciones EF Core concurrentes es frágil.

## Decisión

- El esquema se define mediante scripts SQL versionados en `infrastructure/postgres/init/` aplicados por `docker-entrypoint-initdb.d`.
- Los servicios .NET usan EF Core (Npgsql) como capa de acceso sobre el esquema existente (modelo database-first), sin `EnsureCreated`.
- pgvector y los roles/políticas RLS se crean en el mismo init.

## Consecuencias

- Orden de aplicación determinista y reproducible.
- Una única fuente de verdad para el esquema.
