# Documentación de Seguridad

## Controles implementados

### Autenticación y sesión
- Contraseñas con **Argon2id** (time cost 3, memory 64 MiB, lanes 4). Nunca en texto plano.
- **JWT** HS256 con `iss`/`aud`/`exp`/`jti` + **refresh tokens opacos rotativos** (hash SHA-256 en BD, revocables).
- Cookies **HttpOnly / Secure / SameSite=Lax**. Sin información sensible en `localStorage`.
- Renovación automática de sesión en el BFF (`RefreshMiddleware`).
- **Rate limiting** y bloqueo temporal por intentos fallidos (Redis).

### Autorización y datos
- **Row Level Security (RLS)** en PostgreSQL:
  - Roles de BD por microservicio (`svc_auth`, `svc_onboarding`, `svc_hr`, `svc_ai`), menor privilegio.
  - Variables de sesión `app.user_id` / `app.role` establecidas desde los claims JWT (con reset en cierre de conexión para evitar fugas con pooling).
  - `FORCE ROW LEVEL SECURITY` en las tablas de `onboarding` y `hr`.
- Prevención de **IDOR** (filtrado por propietario en aplicación + RLS) y **mass assignment** (DTOs; `role`, `userId`, `requester`, etc. nunca se enlazan desde el cliente).

### Aplicación web
- **CSRF** (tokens antiforgery en formularios y fetch).
- **XSS**: el chat y los comentarios se escapan/sanitizan antes de renderizar.
- **SQLi**: EF Core con parámetros; sin SQL crudo con entrada de usuario.
- Cabeceras de seguridad en Nginx (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`).

### IA / RAG
- La API Key de Groq reside **únicamente** en el AI Service; nunca en el navegador, Razor, JS, Git o logs.
- **Prompt injection**: el contexto recuperado se inyecta como **DATOS** en una sección separada del system prompt; validación de entrada (límite de longitud) y de salida; rate limiting por usuario.
- El LLM no tiene permisos administrativos; las acciones (crear ticket) se ejecutan contra HrService con el JWT del usuario.

### Secretos y logs
- Secretos solo por variables de entorno (`.env` en `.gitignore`).
- Logging estructurado con correlation ID; **sin** contraseñas, tokens, API keys ni PII innecesaria.

## Políticas RLS (resumen)

| Tabla | Colaborador | RRHH / Admin |
|---|---|---|
| `onboarding.employees` | ve/crea su propio registro (`user_id = app.user_id`) | todo |
| `onboarding.employee_tasks` | sus tareas | todas |
| `onboarding.tasks` / `plan_phases` / `activities` | lectura | lectura + escritura |
| `hr.tickets` | propias (`requester_user_id = app.user_id`) | todas |
| `hr.ticket_comments` | las de sus tickets o propias | todas |
| `hr.ticket_status_history` | sus tickets | todas |

## A mejorar en producción (no bloqueantes para el MVP)
- Firma con claves asimétricas (RS256/ES256) o proveedor de claves externo.
- Rate limiting del AI Service distribuido (Redis compartido).
- Gestión centralizada de secretos (Vault / Secret Manager).
