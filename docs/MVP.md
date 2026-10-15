# MVP — Asistente Virtual para la Integración Organizacional

> **Empresa:** PluriOne S.A. de C.V. (Develop Talent & Technology)
> **Producto:** Asistente Virtual de Integración Organizacional (MVP)
> **Código:** PLU-MVP-2026-V1
> **Estado:** Funcional e integrado

---

## 1. ¿Qué es?

Plataforma web de **onboarding** organizacional con un **asistente virtual basado en IA (RAG)** que acompaña al nuevo colaborador durante su proceso de integración: resuelve dudas en lenguaje natural con base en los documentos corporativos, guía las actividades de incorporación, muestra el plan **30-60-90** y canaliza solicitudes a **Recursos Humanos** mediante tickets.

No es solo un chatbot: el asistente es un componente de una plataforma integrada con autenticación, onboarding, tickets y paneles de administración.

---

## 2. Roles

| Rol | Responsabilidades | Acceso |
|---|---|---|
| **Colaborador** (nuevo ingreso) | Avanza en su onboarding, consulta su plan 30-60-90, usa el asistente y crea solicitudes. | Inicio, Onboarding, Plan 30-60-90, Asistente, Tickets (propios) |
| **Recursos Humanos (RRHH)** | Resuelve solicitudes con base en los documentos, cambia estados, comenta y escala al admin cuando corresponde. | Inicio, Asistente, Tickets (todos), Panel RRHH |
| **Administrador** | Gestiona usuarios/roles, reinicia contraseñas, revisa auditoría, resuelve escaladas y administra la base de conocimiento. | Inicio, Panel Admin |

---

## 3. Funcionalidades por módulo

### Autenticación (AuthService)
- Login/logout con JWT + refresh token rotativo (cookies HttpOnly/Secure/SameSite).
- Contraseñas con **Argon2id** (nunca en texto plano).
- Bloqueo temporal por intentos fallidos y rate limiting (Redis).
- Cambio de contraseña propio y edición de nombre de perfil.
- Auditoría de eventos de autenticación (con nombre de usuario).

### Onboarding (OnboardingService)
- Perfil del colaborador (auto-creado en el primer acceso).
- Checklist de tareas con estados (pendiente/en curso/completada).
- Progreso calculado automáticamente.
- Plan 30-60-90 con 3 fases y actividades (configurables en BD).

### Recursos Humanos (HrService)
- Creación de solicitudes (solo colaborador).
- Categorías, prioridades, estados (`open`, `in_progress`, `resolved`, `closed`).
- Comentarios e historial de cambios.
- **Escalamiento** de solicitudes al administrador.
- **Eliminación** de solicitudes resueltas/cerradas (RRHH/admin).
- Cada ticket muestra el **nombre** del solicitante y de los autores de comentarios.

### Asistente Virtual (AI Service)
- Respuestas fundamentadas en la documentación corporativa (RAG).
- La información recuperada tiene prioridad sobre el conocimiento del modelo; **no inventa** sin contexto.
- **Function calling**: detecta intenciones como "no tengo acceso a mi correo" y crea el ticket automáticamente.
- Streaming (SSE) del chat, historial y fuentes citadas.
- Protección contra **prompt injection** (los documentos se tratan como datos, no como instrucciones).

### Administración (Panel Admin)
- **Usuarios**: crear, cambiar rol y activar/desactivar (sin auto-modificarse).
- **Contraseñas**: reiniciar (no se pueden ver, están hasheadas).
- **Auditoría**: calendario mensual; al seleccionar un día se ve quién accedió y qué hizo.
- **Base de conocimiento**: subir documentos por drag-and-drop (.md/.txt/.html), listar los indexados y eliminarlos.

### Interfaz (Web/BFF)
- Diseño propio (Bauhaus + neobrutalismo) con **modo claro/oscuro** persistente.
- Zona horaria de **Ciudad de México**.
- Pantallas: Login, Dashboard, Onboarding, Plan 30-60-90, Asistente, Tickets, Perfil, Panel RRHH, Panel Admin y página de error.

---

## 4. Flujos principales

```
Colaborador → Login → Dashboard
   ├─ Onboarding → checklist + progreso
   ├─ Plan 30-60-90 → fases e hitos
   ├─ Asistente → RAG (pgvector) → Groq → respuesta
   └─ Tickets → crea solicitud

RRHH → Login → Panel RRHH
   ├─ Atiende solicitudes (estado + comentarios)
   └─ Escala al admin cuando requiere gestión del sistema

Admin → Login → Panel Admin
   ├─ Usuarios/roles + reset de contraseñas
   ├─ Auditoría (calendario)
   └─ Base de conocimiento (subir/listar/eliminar)
```

---

## 5. Stack técnico

| Capa | Tecnología |
|---|---|
| Web/BFF | .NET 9 · ASP.NET Core (Razor Pages + JavaScript) |
| AuthService | .NET 9 Web API + EF Core + Argon2id |
| OnboardingService | .NET 9 Web API + EF Core |
| HrService | .NET 9 Web API + EF Core |
| AI Service | Python · FastAPI · LangGraph · LangChain |
| LLM | Groq · `openai/gpt-oss-20b` |
| Embeddings | sentence-transformers `paraphrase-multilingual-MiniLM-L12-v2` |
| Base de datos | PostgreSQL 16 + pgvector |
| Cache / rate limiting | Redis |
| Proxy | Nginx |
| Orquestación | Docker Compose |
| CI/CD | GitLab CI/CD (`.gitlab-ci.yml`) |

---

## 6. Seguridad (resumen)

- Argon2id, JWT + refresh rotativo, cookies seguras.
- **Row Level Security (RLS)** en PostgreSQL (rol por microservicio + contexto de usuario).
- Rate limiting, bloqueo por intentos, IDOR/mass assignment, CSRF, XSS, SQLi.
- Prompt injection guard; API key de Groq solo en el AI Service.
- Secretos fuera del código; logging estructurado con correlation ID.

---

## 7. Cómo ejecutarlo

```bash
cp .env.example .env          # completar secretos (POSTGRES, JWT, GROQ_API_KEY)
docker compose up -d --build  # levantar todos los servicios
```

Acceder a **http://localhost**.

Ingesta de la base de conocimiento:
```bash
docker exec -it plurione-ai-service python ingest_cli.py
```

---

## 8. Credenciales DEMO

| Rol | Correo | Contraseña |
|---|---|---|
| Colaborador | `mariana.lopez@example.com` | `PluriOne#2026` |
| RRHH | `rrhh@example.com` | `PluriOne#2026` |
| Admin | `admin@example.com` | `PluriOne#2026` |

> Credenciales de demostración. En producción se gestionan desde el Panel Admin.

---

## 9. Alcance

**Incluido:** autenticación con roles, onboarding + plan 30-60-90, asistente RAG, tickets con escalamiento y eliminación, panel admin completo, gestión de base de conocimiento, modo oscuro y zona horaria MX.

**Fuera de alcance (MVP):** aplicaciones móviles, nómina/pagos, múltiples proveedores de IA, Kubernetes/Kafka, notificaciones complejas.

---

## 10. Estado

| Área | Estado |
|---|---|
| 8 fases de desarrollo | ✅ Completadas |
| Funcionalidades post-MVP (roles, panel admin, KB, modo oscuro) | ✅ Completadas |
| Pruebas unitarias (.NET) | ✅ 25/25 en verde |
| Suite pytest del AI Service | ✅ 17/17 en verde |
| Pruebas de integración end-to-end (login, onboarding, tickets, RAG, RLS) | ✅ 15/15 en verde |
| Repositorio Git + primer commit | ✅ Completado |
| Cierre formal del MVP | ✅ Completado |

**MVP cerrado.** El producto está funcional, integrado, probado y documentado.

---

*Documento complementario al PRD (`docs/PRD.md`) y a la documentación técnica (`docs/`).*
