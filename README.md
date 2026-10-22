# PluriOne — Asistente Virtual para la Integración Organizacional

[![CI](https://github.com/Itry-twice/G19X-UPTAP-ARPA-965-ACADEMIC/actions/workflows/ci.yml/badge.svg)](https://github.com/Itry-twice/G19X-UPTAP-ARPA-965-ACADEMIC/actions/workflows/ci.yml)

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

### 1. Requisitos

- **Docker** y **Docker Compose** instalados y en ejecución.

### 2. Configurar variables de entorno

```bash
# Copia la plantilla (contiene placeholders, NO claves reales)
cp .env.example .env
```

Abre `.env` y completa al menos:
- `GROQ_API_KEY` — tu clave de Groq (necesaria para que el asistente responda).
- `POSTGRES_PASSWORD`, `REDIS_PASSWORD`, `SVC_*_PASSWORD`, `JWT_SIGNING_KEY` — cualquier valor seguro.
- `SEED_DEMO=true` — para crear las cuentas de demostración (solo desarrollo/demos; en producción déjala en `false` o quítala).

### 3. Construir y levantar los servicios

```bash
docker compose up -d --build
```

> La primera construcción tarda aproximadamente **10–15 minutos** (descarga las imágenes base y el modelo de embeddings `paraphrase-multilingual-MiniLM-L12-v2`, que se baja en el Dockerfile del AI Service). Las siguientes reconstrucciones son mucho más rápidas.

### 4. Verificar

```bash
docker compose ps
```

Verificado: los servicios quedan en **`healthy`** (postgres, redis, auth, onboarding, hr, ai, web y nginx). La aplicación queda en **http://localhost**.

### 5. Ingerir la base de conocimiento (para que el asistente responda)

```bash
docker exec -it plurione-ai-service python ingest_cli.py
```

Alternativa: en el **Panel Admin**, botón "Re-ingestar documentos".

### 6. Entrar

Usa las **Credenciales DEMO** de la siguiente sección.

> La primera vez, `infrastructure/postgres/init/` crea el esquema, los roles y las políticas **RLS**, y siembra los catálogos. Las cuentas DEMO solo se crean si `SEED_DEMO=true` (o en entorno de desarrollo).

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
| `SEED_DEMO` | `true` para crear las cuentas de demostración (solo desarrollo/demos) |

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
- **Rate limiting**: bloqueo por intentos fallidos con **Redis** (AuthService) y límite por usuario en memoria (AI Service).
- **Row Level Security (RLS)** en PostgreSQL: cada servicio usa su propio rol de menor privilegio; el filtrado por propietario se aplica con `app.user_id`/`app.role`.
- Prevención de **IDOR**, **mass assignment** (DTOs), **CSRF**, **XSS** y **SQLi**.
- La API Key de Groq **solo reside en el AI Service**; nunca en el navegador ni en el código versionado.
- **Protección contra prompt injection**: el contexto RAG se trata como datos, no como instrucciones.
- Logging estructurado con **correlation ID** (sin secretos ni PII innecesaria).

Ver `docs/security/`.

---

## Pruebas

```bash
# Pruebas unitarias .NET (25)
dotnet test tests/PluriOne.Tests/PluriOne.Tests.csproj

# Pruebas unitarias del AI Service (17)
cd services/ai-service
pip install pytest
python -m pytest tests -v

# Pruebas de integración end-to-end (requiere el stack en ejecución)
python tests/e2e/integration_test.py
```

Cobertura: Argon2id, JWT, autorización de HR (IDOR/roles), onboarding, ingesta/RAG, rate limiting, y flujos críticos (login por rol, RLS, escalamiento, RAG). Ver `docs/testing/`.

> Resultado verificado: 26 pruebas .NET, 17 de pytest y 15 del recorrido end-to-end, todas en verde (ver `docs/verification.md`).

---

## Estructura del proyecto

```
docs-source/            # Documentos de referencia (no modificar)
docs/                   # Documentación técnica (PRD, MVP, arquitectura, API, seguridad, ADRs)
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
.github/workflows/      # CI/CD (GitHub Actions)
docker-compose.yml
```

---

## Documentación

- `docs/PRD.md` — requerimientos y análisis
- `docs/MVP.md` — alcance y estado del MVP
- `docs/architecture/overview.md` — arquitectura y diagramas
- `docs/decisions/` — ADRs (decisiones de arquitectura)
- `docs/api/`, `docs/security/`, `docs/deployment/`, `docs/testing/`

---

## Uso de inteligencia artificial

Parte del código y la documentación de este proyecto fue elaborada con asistencia de herramientas de inteligencia artificial (generación y revisión de código, documentación y configuración), siempre con revisión y validación manual del autor.
