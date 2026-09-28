# Documentación de Pruebas

## Pruebas unitarias (.NET)

```bash
dotnet test tests/PluriOne.Tests/PluriOne.Tests.csproj
```

Cobertura:

- `PasswordHasherTests` — Argon2id (hash/verify, salting, sin texto plano).
- `JwtTokenServiceTests` — claims, issuer/audiencia, validación.
- `HrManagerTests` — creación de ticket (requester = usuario), rechazo de prioridad inválida, aislamiento por propietario (IDOR), listado para RRHH, prohibición de cambio de estado para colaborador, historial, comentarios.
- `OnboardingManagerTests` — asignación de tareas, idempotencia, validación de estado, progreso.

## Pruebas de integración (stack Docker)

Levantar el stack y validar end-to-end:

1. Login de cada rol → JWT válido.
2. Onboarding: creación de empleado, tareas, progreso, plan.
3. HR: creación de ticket, listado por rol, cambio de estado (RRHH), comentarios.
4. RLS: un colaborador no accede a tickets ajenos (verificado a nivel de base de datos).
5. AI: ingesta de `docs-source/` (40 chunks en pgvector) y respuesta con/sin API key.

## Pruebas de seguridad

- Fuerza bruta / bloqueo (Redis): `POST /api/auth/login` con credenciales inválidas.
- IDOR: intentar `GET /api/hr/tickets/{id}` de otro usuario.
- Mass assignment: enviar `requesterUserId`/`role` en el body → se ignora (DTO).
- CSRF: peticiones POST sin token antiforgery → rechazadas.
- Prompt injection: contexto RAG con instrucciones inyectadas → tratadas como datos.

## Pruebas del RAG

- Ingesta reproducible: hash por documento; no se re-ingesta si el hash no cambió.
- Búsqueda pgvector: cosine distance sobre `vector(384)`.
- Sin contexto suficiente: el asistente indica que no cuenta con la información y recomienda el canal oficial.

## Comandos útiles

```bash
# Logs de un servicio
docker logs -f plurione-ai-service

# Ejecutar la ingesta manualmente
docker exec -it plurione-ai-service python ingest_cli.py

# Consultar chunks indexados
docker exec plurione-postgres psql -U postgres -d plurione -c "SELECT count(*) FROM ai.chunks;"
```
