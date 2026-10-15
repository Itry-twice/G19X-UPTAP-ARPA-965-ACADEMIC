# ADR-0005 — Row Level Security por rol de base de datos y contexto de usuario

- **Estado:** Aceptado

## Contexto

Se exige que los datos se restrinjan en base de datos, no solo en la aplicación.

## Decisión

- Cada servicio usa su propio rol de BD (`svc_auth`, `svc_onboarding`, `svc_hr`, `svc_ai`).
- Las tablas usan RLS. El filtro por propietario se aplica mediante la variable de sesión `app.user_id`, establecida por el servicio desde los claims JWT en cada conexión.
- Los roles RRHH/Admin acceden mediante políticas que evalúan `app.role`.

## Consecuencias

- Defensa en profundidad; el acceso indebido se bloquea incluso si la capa de aplicación falla.
