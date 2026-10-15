# ADR-0003 — Autenticación con JWT + sesiones

- **Estado:** Aceptado

## Contexto

El navegador solo habla con el BFF; los microservicios son internos. Se necesita identidad y sesión seguras.

## Decisión

- AuthService emite un access token (JWT firmado con secreto compartido) y un refresh token opaco almacenado (hash) en `auth.sessions`.
- El BFF guarda ambos en cookies HttpOnly/Secure/SameSite.
- El BFF propaga el JWT a los microservicios vía `Authorization: Bearer`.
- Cada servicio valida la firma JWT; el AI Service verifica la firma (confía en claims emitidos por el backend) sin gestionar usuarios.

## Consecuencias

- Sesiones revocables (logout invalida refresh).
- No se requiere sesión de servidor en el BFF.
