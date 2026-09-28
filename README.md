# PluriOne — Asistente Virtual para la Integración Organizacional

Plataforma web de **onboarding** organizacional con un **asistente virtual basado en IA (RAG)** para acompañar a los nuevos colaboradores de **PluriOne S.A. de C.V. (Develop Talent & Technology)**.

El sistema resuelve dudas en lenguaje natural, guía las actividades de incorporación, muestra el plan **30-60-90**, y canaliza solicitudes al área de **Recursos Humanos** mediante tickets.

> Proyecto de estadías. La información institucional proviene de `docs-source/` (documentación de referencia). Los datos de prueba se identifican como **DEMO**.

---

## Arquitectura

Microservicios (ver `docs/architecture/overview.md`):

| Servicio | Tecnología | Responsabilidad |
|---|---|---|
| `web` (BFF) | .NET 9 · Razor Pages + JS | Interfaz, sesiones, cookies, anti-CSRF, proxy a microservicios |
| `auth-service` | .NET 9 Web API | Usuarios, login/logout, roles, Argon2id, bloqueo, auditoría |
| `onboarding-service` | .NET 9 Web API | Perfil, tareas, progreso, plan 30-60-90 |
| `hr-service` | .NET 9 Web API | Tickets, solicitudes, estados, comentarios, historial |
| `ai-service` | Python · FastAPI · LangGraph | RAG, embeddings, pgvector, Groq, function calling |

Infraestructura: **PostgreSQL 16 + pgvector**, **Redis**, **Nginx**, **Docker Compose**.

```
Browser → Nginx → Web/BFF → { AuthService, OnboardingService, HrService, AI Service }
                                        ↓                ↓
                                   PostgreSQL         Redis
```

---

## Requisitos

- Docker 24+ y Docker Compose v2
- (Opcional, para desarrollo local) .NET 9 SDK, Python 3.12

---

## Puesta en marcha

```bash
# 1. Copiar la plantilla de variables de entorno y completar los secretos
cp .env.example .env

# 2. Construir y levantar todos los servicios
docker compose up -d --build

# 3. Verificar el estado
docker compose ps
```

La aplicación queda disponible en **http://localhost** (Nginx, puerto 80).

> La primera vez, los scripts de `infrastructure/postgres/init/` crean el esquema, roles de menor privilegio y políticas **RLS**, y siembran los catálogos (plan 30-60-90, tareas, categorías). Los usuarios DEMO se siembran en tiempo de ejecución por AuthService.

### Ingesta de la base de conocimiento (RAG)

```bash
# Opción A: vía CLI dentro del contenedor
docker exec -it plurione-ai-service python ingest_cli.py

# Opción B: vía API (requiere rol admin)
# POST http://localhost/api/assistant/...  (a través del BFF) o
# POST http://ai-service:8080/ingest con Bearer token de un usuario admin
```

---

## Credenciales DEMO

| Rol | Correo | Contraseña |
|---|---|---|
| Colaborador | `mariana.lopez@example.com` | `PluriOne#2026` |
| RRHH | `rrhh@example.com` | `PluriOne#2026` |
| Admin | `admin@example.com` | `PluriOne#2026` |

> Contraseñas DEMO, usar solo en desarrollo.

---

## Variables de entorno

Ver `.env.example`. Las principales:

| Variable | Descripción |
|---|---|
| `POSTGRES_PASSWORD` | Contraseña del superusuario de PostgreSQL |
| `SVC_*_PASSWORD` | Contraseñas de los roles de aplicación (menor privilegio) |
| `REDIS_PASSWORD` | Contraseña de Redis |
| `JWT_SIGNING_KEY` | Clave de firma de los JWT (≥32 bytes) |
| `GROQ_API_KEY` | API key de Groq (solo AI Service) |
| `GROQ_MODEL` | Modelo LLM (por defecto `openai/gpt-oss-20b`) |
| `EMBEDDING_PROVIDER` | `hash` (sin dependencias) o `sentence-transformers` |
| `EMBEDDING_DIM` | Dimensión de los embeddings (384) |

**Los secretos nunca se suben al repositorio** (`.env` está en `.gitignore`).

---

## Servicios y endpoints principales

### AuthService (`/api/auth`)
- `POST /api/auth/login` — login (JWT + refresh)
- `POST /api/auth/refresh` — renovación de sesión
- `POST /api/auth/logout` — revocación de sesión
- `GET /api/auth/me` — perfil del usuario autenticado

### OnboardingService (`/api/onboarding`)
- `GET /api/onboarding/me` — perfil del colaborador
- `GET /api/onboarding/tasks` — checklist
- `PUT /api/onboarding/tasks/{id}/status` — actualizar estado
- `GET /api/onboarding/progress` — progreso
- `GET /api/onboarding/plan` — plan 30-60-90

### HrService (`/api/hr`)
- `GET /api/hr/categories` — categorías
- `GET /api/hr/tickets` — listar (colaborador: propias; RRHH: todas)
- `POST /api/hr/tickets` — crear solicitud
- `GET /api/hr/tickets/{id}` — detalle con comentarios e historial
- `POST /api/hr/tickets/{id}/comments` — comentar
- `PATCH /api/hr/tickets/{id}/status` — cambiar estado (RRHH/admin)

### AI Service
- `POST /chat` — respuesta RAG (JSON)
- `POST /chat/stream` — respuesta con streaming (SSE)
- `POST /ingest` — ingesta de la base de conocimiento (admin)

---

## Seguridad

- Contraseñas con **Argon2id** (nunca en texto plano).
- **JWT** firmado (HS256) + refresh tokens rotativos revocables.
- Cookies **HttpOnly / Secure / SameSite** en el BFF.
- **Rate limiting** y bloqueo por intentos fallidos con Redis.
- **Row Level Security (RLS)** en PostgreSQL: cada servicio usa su propio rol de menor privilegio; el filtrado por propietario se aplica con `app.user_id`/`app.role`.
- Prevención de **IDOR**, **mass assignment** (DTOs), **CSRF**, **XSS** y **SQLi**.
- La API Key de Groq **solo reside en el AI Service**; nunca en el navegador ni en el código versionado.
- **Protección contra prompt injection**: el contexto RAG se trata como datos, no como instrucciones.
- Logging estructurado con **correlation ID** (sin secretos ni PII innecesaria).

Ver `docs/security/`.

---

## Pruebas

```bash
dotnet test tests/PluriOne.Tests/PluriOne.Tests.csproj
```

Cobertura: hashing de contraseñas (Argon2id), emisión/validación de JWT, autorización de HR (IDOR/roles), onboarding (tareas/progreso). Las políticas RLS se validan de forma integral con el stack Docker (ver `docs/testing/`).

---

## Estructura del proyecto

```
docs-source/            # Documentos de referencia (no modificar)
docs/                   # PRD, arquitectura, API, seguridad, ADRs
infrastructure/
  postgres/init/        # Migraciones SQL (schema, RLS, seed)
  nginx/                # Reverse proxy
services/
  PluriOne.Shared/      # Librería compartida (.NET)
  auth-service/         # AuthService
  onboarding-service/   # OnboardingService
  hr-service/           # HrService
  ai-service/           # AI Service (FastAPI + LangGraph)
  web/                  # Web/BFF (Razor Pages)
tests/                  # Pruebas .NET
knowledge-base/         # Pipeline de ingesta (fuente en ai-service)
docker-compose.yml
.gitlab-ci.yml
```

---

## Documentación

- `docs/PRD.md` — requerimientos y análisis
- `docs/architecture/overview.md` — arquitectura y diagramas
- `docs/decisions/` — ADRs
- `docs/api/`, `docs/security/`, `docs/deployment/`, `docs/testing/`
