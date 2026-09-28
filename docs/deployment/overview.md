# Documentación de Despliegue

## Entornos

| Entorno | Configuración |
|---|---|
| Local | `docker compose up -d --build` |
| CI/CD | GitLab CI (ver `.gitlab-ci.yml`) |

## Docker Compose

Servicios definidos en `docker-compose.yml`:

- `postgres` (pgvector/pgvector:pg16) — volumen `postgres_data`, init en `infrastructure/postgres/init/`.
- `redis` (redis:7-alpine) — volumen `redis_data`, con contraseña.
- `auth-service`, `onboarding-service`, `hr-service`, `web` (.NET 9).
- `ai-service` (FastAPI) — monta `docs-source/` en solo lectura.
- `nginx` — reverse proxy, puerto 80.

### Redes

- `frontend`: nginx ↔ web.
- `backend`: web ↔ servicios ↔ postgres/redis.

PostgreSQL y Redis **no** están expuestos al host (solo red interna).

### Health checks

Todos los servicios exponen `GET /health`. Compose usa `curl`/`pg_isready`/`redis-cli` para el estado.

## Migraciones

Los scripts versionados en `infrastructure/postgres/init/` se aplican por `docker-entrypoint-initdb.d` en orden alfabético la primera vez que se crea el volumen:

- `000_roles.sh` — extensiones (pgcrypto, vector), roles de aplicación y schemas.
- `001_schema.sql` — tablas, índices y políticas RLS.
- `002_seed.sql` — catálogos (plan 30-60-90, tareas, categorías).

Para reiniciar desde cero: `docker compose down -v`.

## Variables de entorno (secretos)

Los secretos provienen de `.env` en local y de variables CI/CD protegidas en GitLab. Ver `.env.example`.

## Despliegue manual a un servidor

```bash
# En el servidor
git clone <repo>
cp .env.example .env   # y completar secretos
docker compose up -d --build
```
