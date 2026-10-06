# Documentación de Pruebas

## Pruebas unitarias (.NET)

```bash
dotnet test tests/PluriOne.Tests/PluriOne.Tests.csproj
```

Cobertura (25 pruebas):

- `PasswordHasherTests` — Argon2id (hash/verify, salting, sin texto plano).
- `JwtTokenServiceTests` — claims, issuer/audiencia, validación.
- `HrManagerTests` — creación de ticket (requester = usuario), rechazo de prioridad inválida, aislamiento por propietario (IDOR), listado para RRHH, prohibición de cambio de estado para colaborador, historial, comentarios, escalamiento (solo RRHH) y eliminación (solo resueltas/cerradas, por RRHH/admin).
- `OnboardingManagerTests` — asignación de tareas, idempotencia, validación de estado, progreso.

## Pruebas unitarias (AI Service — pytest)

```bash
cd services/ai-service
pip install pytest
python -m pytest tests -v
```

Cobertura (17 pruebas):

- `test_ingest.py` — limpieza de markdown, chunking, división por encabezados y metadata (incluye que el encabezado se mantiene con su contenido).
- `test_hash_provider.py` — embeddings determinísticos, normalizados y de dimensión correcta.
- `test_rate_limit.py` — límite, expiración de ventana e independencia de claves.

## Pruebas de integración (stack Docker)

### Automatizadas

Con el stack en ejecución (`docker compose up -d`):

```bash
python tests/e2e/integration_test.py
```

Valida (15 comprobaciones) a través del BFF: navegación por rol, restricción del onboarding al colaborador, creación de tickets, aislamiento RLS (un colaborador no ve tickets ajenos), respuesta del asistente con streaming y fuentes, y acceso al panel admin.

### Manuales

Levantar el stack y validar end-to-end:

1. Login de cada rol → JWT válido.
2. Onboarding: creación de empleado, tareas, progreso, plan.
3. HR: creación de ticket, listado por rol, cambio de estado (RRHH), comentarios.
4. RLS: un colaborador no accede a tickets ajenos (verificado a nivel de base de datos).
5. AI: ingesta de `docs-source/` (chunks en pgvector) y respuesta con/sin API key.

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
