# Documentación de APIs

Las APIs internas se consumen a través del BFF (`web`). El navegador solo habla con el BFF; la comunicación BFF → microservicios usa `Authorization: Bearer <JWT>`.

## Convenciones

- Formato uniforme de error: `{"code": "...", "message": "...", "correlationId": "..."}`.
- Códigos HTTP: `200`/`201`/`204` éxito, `400` validación, `401` no autenticado, `403` sin permisos, `404` no encontrado, `423` bloqueado, `429` límite excedido, `503` proveedor no disponible.
- Todos los endpoints requieren JWT, excepto `login` y `refresh`.

## AuthService (`/api/auth`)

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/auth/login` | `{ email, password }` → `{ accessToken, refreshToken, expiresInSeconds, user }` |
| POST | `/api/auth/refresh` | `{ refreshToken }` → nuevos tokens (rotación) |
| POST | `/api/auth/logout` | `{ refreshToken }` → revoca sesión |
| GET | `/api/auth/me` | perfil del usuario |

## OnboardingService (`/api/onboarding`)

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/onboarding/me` | perfil (crea el registro en el primer acceso) |
| GET | `/api/onboarding/tasks` | checklist con estado |
| PUT | `/api/onboarding/tasks/{id}/status` | `{ status: pending\|in_progress\|completed }` |
| GET | `/api/onboarding/progress` | `{ total, completed, inProgress, pending, percentage }` |
| GET | `/api/onboarding/plan` | plan 30-60-90 (fases + actividades) |

## HrService (`/api/hr`)

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/hr/categories` | categorías de tickets |
| GET | `/api/hr/tickets` | listar (colaborador: propias; RRHH: todas) |
| POST | `/api/hr/tickets` | `{ subject, description?, categoryId?, priority? }` |
| GET | `/api/hr/tickets/{id}` | detalle (comentarios + historial) |
| POST | `/api/hr/tickets/{id}/comments` | `{ body }` |
| PATCH | `/api/hr/tickets/{id}/status` | `{ status, assigneeUserId? }` (RRHH/admin) |

## AI Service

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/chat` | `{ question }` → `{ response, sources, intent }` |
| POST | `/chat/stream` | streaming SSE (`token`, `sources`, `done`, `error`) |
| POST | `/ingest` | ingesta de `docs-source/` (admin) |
| GET | `/health` | healthcheck |

## Autorización

- `colaborador`: acceso a sus propios datos y tickets; usa el asistente.
- `rrhh`: acceso a todos los tickets; cambia estados; comenta.
- `admin`: operaciones administrativas (ingesta, usuarios).

La autorización se aplica en dos capas: aplicación (JWT/roles) y base de datos (RLS).
