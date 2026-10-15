# PRD — Asistente Virtual para la Integración Organizacional de Nuevos Colaboradores

> **Documento:** Product Requirements Document
> **Código:** PLU-PRD-2026-V1
> **Estado:** Borrador técnico (proyecto de estadías)
> **Propietario:** Líder de Proyecto / Evaluaciones

Este documento distingue explícitamente entre:

- **[DOC]** — Información proveniente de los documentos fuente (`docs-source/`).
- **[TEC]** — Decisión técnica propuesta por el equipo de ingeniería.

No se atribuye ninguna decisión técnica a la organización.

---

## 4.1 Resumen del producto

Plataforma web de *onboarding* organizacional con un asistente virtual basado en IA. Acompaña al nuevo colaborador durante su proceso de integración resolviendo dudas con lenguaje natural (RAG), guiando actividades de incorporación, mostrando el plan 30-60-90 y canalizando solicitudes al área de Recursos Humanos mediante tickets.

No es únicamente un chatbot: el asistente es un componente de una plataforma que incluye onboarding, progreso, tickets, panel de RR. HH. y panel de administración. [TEC]

La plataforma contempla tres roles claramente separados: **colaborador** (nuevo ingreso), **Recursos Humanos** (atención de solicitudes) y **administrador** (gestión del sistema, usuarios, auditoría y base de conocimiento). Incluye un modo visual claro/oscuro y opera en la zona horaria de la Ciudad de México. [TEC]

## 4.2 Problema

**[DOC]** El proceso de incorporación de nuevos colaboradores es un punto de alta carga operativa para Recursos Humanos. Los nuevos ingresos tienen múltiples dudas (horarios, esquema híbrido, configuración de TI, código de conducta, plan 30-60-90) que hoy se resuelven de forma manual y dispersa.

- Mejorar la experiencia de incorporación y reducir la carga operativa de RR. HH. [DOC]
- Fortalecer el portafolio de soluciones HRTech con IA conversacional. [DOC]

## 4.3 Objetivo general

Desarrollar un asistente virtual inteligente para apoyar la integración organizacional de nuevos colaboradores. [DOC]

## 4.4 Objetivos específicos

- Analizar el proceso de integración. [DOC]
- Diseñar la arquitectura conversacional. [DOC]
- Implementar agentes inteligentes. [DOC]
- Validar el funcionamiento. [DOC]
- Elaborar documentación técnica y plan de soporte. [DOC]

## 4.5 Alcance

**Incluye:**

- Autenticación y autorización con roles (colaborador, RRHH, admin). [TEC]
- Onboarding con tareas, checklist, progreso y plan 30-60-90 (solo colaborador). [TEC]
- Asistente virtual con RAG sobre la documentación corporativa. [TEC]
- Sistema de tickets / solicitudes (colaborador crea, RRHH atiende, admin resuelve escaladas). [TEC]
- Panel de administración para Recursos Humanos. [TEC]
- Panel de administración del sistema (usuarios, roles, auditoría por calendario, base de conocimiento). [TEC]
- Gestión de la base de conocimiento: subida por drag-and-drop, listado y eliminación de documentos. [TEC]
- Edición de perfil (nombre y contraseña) y reinicio de contraseña por el administrador. [TEC]
- Interfaz web (BFF) integrada con el backend, con modo claro/oscuro. [TEC]

## 4.6 Fuera de alcance

- Aplicaciones móviles nativas. [TEC]
- Sistemas de pago / nómina. [TEC]
- Múltiples proveedores de IA. [TEC]
- Kubernetes / Kafka / serverless. [TEC]
- Migración de datos históricos reales de la organización. [TEC]

## 4.7 Usuarios

1. **Nuevo colaborador** — consume información, avanza en su onboarding, usa el asistente y crea tickets. [TEC]
2. **Recursos Humanos** — atiende y resuelve solicitudes (con base en los documentos), cambia estados, comenta y escala al administrador cuando corresponde. No crea solicitudes ni tiene onboarding propio. [TEC]
3. **Administrador** — administra el sistema: crea y gestiona usuarios/roles, reinicia contraseñas, revisa auditoría (calendario de accesos), resuelve solicitudes escaladas y gestiona la base de conocimiento. No atiende tickets generales ni tiene onboarding. [TEC]
4. **Líder de Proyecto / Tech Lead** — referencia del plan 30-60-90 y evaluación. [DOC]

## 4.8 Personas

| Persona | Descripción |
|---|---|
| **Mariana (Nueva colaboradora técnica)** | 26 años, desarrolladora backend. Primer día en la empresa, necesita configurar su entorno, entender horarios, esquema híbrido y su plan 30-60-90. Usa el asistente para resolver dudas sin saturar a RR. HH. |
| **Laura (Recursos Humanos)** | 34 años, analista de RR. HH. Recibe solicitudes, asigna categorías, responde tickets y da seguimiento al onboarding de cada nuevo ingreso. |
| **Edgar (Líder de Proyecto)** | 38 años, Tech Lead. Valida el progreso del plan 30-60-90 y participa en la evaluación del periodo de prueba. |
| **Soporte TI** | Atiende la habilitación de credenciales y configuración de equipos (referido en la guía de primer día). [DOC] |

## 4.9 User Stories

> Como **nuevo colaborador**,
> quiero **iniciar sesión de forma segura**,
> para **acceder a mi proceso de integración**.

> Como **nuevo colaborador**,
> quiero **preguntar en lenguaje natural sobre horarios, esquema híbrido o políticas**,
> para **resolver mis dudas sin depender de otra persona**.

> Como **nuevo colaborador**,
> quiero **ver mis tareas y mi progreso de onboarding**,
> para **saber qué me falta por completar**.

> Como **nuevo colaborador**,
> quiero **consultar mi plan 30-60-90**,
> para **conocer los hitos y criterios de mi periodo de prueba**.

> Como **nuevo colaborador**,
> quiero **crear una solicitud a Recursos Humanos**,
> para **resolver trámites administrativos o técnicos**.

> Como **colaborador de RR. HH.**,
> quiero **ver y responder las solicitudes entrantes**,
> para **atenderlas con trazabilidad**.

> Como **administrador**,
> quiero **gestionar usuarios y roles**,
> para **controlar el acceso al sistema**.

> Como **cualquier usuario**,
> quiero **editar mi perfil y cambiar mi contraseña**,
> para **mantener mi cuenta actualizada y segura**.

> Como **colaborador de RR. HH.**,
> quiero **escalar una solicitud al administrador**,
> para **canalizar casos que requieren gestión del sistema (p. ej. contraseñas olvidadas)**.

> Como **administrador**,
> quiero **reiniciar la contraseña de un usuario**,
> para **restaurar el acceso cuando se olvida la contraseña** (sin poder verla en texto plano).

> Como **administrador** o **RR. HH.**,
> quiero **eliminar solicitudes ya resueltas**,
> para **mantener la bandeja limpia**.

> Como **administrador**,
> quiero **consultar un calendario de accesos**,
> para **ver quién accedió al sistema en una fecha determinada**.

> Como **administrador**,
> quiero **subir, listar y eliminar documentos de la base de conocimiento**,
> para **mantener actualizada la información que responde el asistente**.

> Como **usuario**,
> quiero **cambiar entre modo claro y oscuro**,
> para **ajustar la interfaz a mi preferencia**.

## 4.10 Requerimientos funcionales

### Módulo Autenticación (AuthService)
- FR-AUTH-01: Login y logout. [TEC]
- FR-AUTH-02: Roles: `colaborador`, `rrhh`, `admin`. [TEC]
- FR-AUTH-03: Contraseñas con hash seguro (Argon2id); nunca en texto plano. [TEC]
- FR-AUTH-04: Bloqueo temporal por intentos fallidos y rate limiting. [TEC]
- FR-AUTH-05: Auditoría de eventos de autenticación (con nombre de usuario). [TEC]
- FR-AUTH-06: Cambio de contraseña propio (validando la actual). [TEC]
- FR-AUTH-07: Edición del nombre del perfil. [TEC]
- FR-AUTH-08: Renovación de sesión mediante refresh token. [TEC]

### Módulo Onboarding (OnboardingService)
- FR-ONB-01: Perfil del colaborador (auto-creado en el primer acceso). [TEC]
- FR-ONB-02: Lista de tareas / checklist. [TEC]
- FR-ONB-03: Progreso calculado por tareas completadas. [TEC]
- FR-ONB-04: Plan 30-60-90 con fases Días 1-30, 31-60, 61-90. [DOC]
- FR-ONB-05: Actividades configurables sin modificar la lógica principal. [TEC]
- FR-ONB-06: El onboarding personal es exclusivo del rol colaborador. [TEC]

### Módulo Recursos Humanos (HrService)
- FR-HR-01: Creación de tickets/solicitudes (solo colaborador). [TEC]
- FR-HR-02: Categorías, estados y comentarios. [TEC]
- FR-HR-03: Historial de cambios. [TEC]
- FR-HR-04: Asignación y atención de solicitudes (solo RRHH). [TEC]
- FR-HR-05: Un colaborador solo accede a sus propios tickets. [TEC]
- FR-HR-06: Escalamiento de solicitudes al administrador (RRHH). [TEC]
- FR-HR-07: Eliminación de solicitudes resueltas o cerradas (RRHH/admin). [TEC]
- FR-HR-08: Registro del nombre del solicitante/autor en cada ticket y comentario. [TEC]

### Módulo Asistente Virtual (AI Service)
- FR-AI-01: Preguntas en lenguaje natural. [TEC]
- FR-AI-02: Respuestas fundamentadas mediante RAG. [TEC]
- FR-AI-03: La información recuperada tiene prioridad sobre el conocimiento del modelo. [TEC]
- FR-AI-04: Si no hay información suficiente, indicarlo y recomendar el canal correspondiente. [TEC]
- FR-AI-05: Function calling para crear tickets (ej. "No tengo acceso a mi correo" → `CREATE_HR_TICKET`). [TEC]
- FR-AI-06: Protección contra prompt injection / jailbreak. [TEC]
- FR-AI-07: Subida de documentos (.md, .txt, .html) e ingesta inmediata. [TEC]
- FR-AI-08: Listado y eliminación de documentos indexados. [TEC]

### Módulo Web (BFF)
- FR-WEB-01: Pantallas: Login, Dashboard, Asistente, Onboarding, Plan 30-60-90, Tickets, Perfil, Panel RR. HH., Panel Admin y página de error. [TEC]
- FR-WEB-02: El navegador nunca se conecta directamente a PostgreSQL, Redis ni al proveedor de IA. [TEC]
- FR-WEB-03: Streaming/SSE para el chat. [TEC]
- FR-WEB-04: Navegación y accesos por rol (colaborador/RRHH/admin). [TEC]
- FR-WEB-05: Modo claro/oscuro con persistencia de la preferencia. [TEC]
- FR-WEB-06: Panel Admin con gestión de usuarios, calendario de auditoría y base de conocimiento. [TEC]

### Módulo Administración (AuthService + AI Service)
- FR-ADM-01: Crear usuarios (correo, nombre, rol, código, contraseña). [TEC]
- FR-ADM-02: Cambiar rol y activar/desactivar usuarios (sin poder auto-modificarse). [TEC]
- FR-ADM-03: Reiniciar contraseña de un usuario (sin exponerla). [TEC]
- FR-ADM-04: Consultar auditoría por fecha (calendario). [TEC]

## 4.11 Requerimientos no funcionales

- **Seguridad:** secretos fuera del código; RLS; cookies HttpOnly/Secure/SameSite; hash de contraseñas; rate limiting; validación de entradas; sanitización; CSRF; XSS; SQLi; IDOR; SSRF; prompt injection; auditoría. [TEC]
- **Rendimiento:** respuestas del asistente en streaming; cache en Redis; índices vectoriales. [TEC]
- **Disponibilidad:** health checks en todos los servicios; Nginx como reverse proxy. [TEC]
- **Mantenibilidad:** microservicios con responsabilidades claras; DTOs; migraciones. [TEC]
- **Escalabilidad:** arquitectura modular preparada para crecer sin complejidad innecesaria. [TEC]
- **Observabilidad:** logging estructurado, correlation ID; sin registrar secretos ni PII innecesaria. [TEC]
- **Privacidad:** mínima PII; aislamiento de datos por rol y por propietario. [TEC]
- **Localización:** zona horaria de Ciudad de México (America/Mexico_City) para saludos y fechas. [TEC]
- **Usabilidad:** modo claro/oscuro persistente y responsive. [TEC]

## 4.12 Reglas de negocio

- Jornada L-V de 09:00 a 18:00, receso de 14:00 a 15:00. [DOC]
- Esquema híbrido 3x2: 3 días presencial, 2 días remoto. [DOC]
- Home Office requiere ≥ 50 Mbps, VPN y Microsoft Entra ID. [DOC]
- Código de vestimenta Smart Casual (L-J) y Casual Fridays. [DOC]
- Evaluación del periodo de prueba: 40% técnica, 30% estándares/seguridad, 30% trabajo en equipo; umbral 80/100. [DOC]
- Contraseña corporativa: ≥ 16 caracteres, vence cada 90 días, MFA. [DOC]
- Todo desarrollo creado en horario laboral pertenece a PluriOne. [DOC]
- Solo el colaborador crea solicitudes; RRHH las atiende y puede escalarlas al administrador. [TEC]
- Solo RRHH y admin pueden eliminar solicitudes, y únicamente si están resueltas o cerradas. [TEC]
- El admin reinicia contraseñas (no las ve); no puede cambiar su propio rol ni desactivarse a sí mismo. [TEC]
- El onboarding personal y el plan 30-60-90 son exclusivos del rol colaborador. [TEC]

## 4.13 Casos de uso

| ID | Caso de uso | Actor principal |
|---|---|---|
| UC-01 | Iniciar sesión | Colaborador / RRHH / Admin |
| UC-02 | Consultar onboarding y progreso | Colaborador |
| UC-03 | Consultar plan 30-60-90 | Colaborador |
| UC-04 | Preguntar al asistente (RAG) | Colaborador / RRHH |
| UC-05 | Crear ticket vía asistente | Colaborador |
| UC-06 | Crear ticket manual | Colaborador |
| UC-07 | Atender ticket | RRHH |
| UC-08 | Escalar ticket al administrador | RRHH |
| UC-09 | Eliminar ticket resuelto | RRHH / Admin |
| UC-10 | Gestionar usuarios y roles | Admin |
| UC-11 | Reiniciar contraseña de un usuario | Admin |
| UC-12 | Consultar auditoría por calendario | Admin |
| UC-13 | Subir/listar/eliminar documentos (KB) | Admin |
| UC-14 | Editar perfil y cambiar contraseña | Todos |
| UC-15 | Alternar modo claro/oscuro | Todos |

## 4.14 Flujos principales

### Login → Dashboard → Acciones
```
Login (AuthService) → Sesión (cookie segura) → Dashboard (BFF)
   ├─ Chat → AI Service → RAG (pgvector) → Groq → Respuesta
   ├─ Onboarding → OnboardingService → Progreso
   ├─ Plan 30-60-90 → OnboardingService
   └─ Tickets → HrService
```

### Creación de ticket vía asistente
```
Usuario: "No tengo acceso a mi correo"
  → AI Service detecta intención CREATE_HR_TICKET
  → ToolCall autorizado por backend
  → HrService crea ticket (propietario = usuario)
  → Respuesta con confirmación + número de ticket
```

### Escalamiento de solicitud al administrador
```
RRHH recibe ticket que requiere gestión del sistema (p. ej. contraseña olvidada)
  → RRHH marca "Escalar a administrador"
  → El ticket aparece en el Panel Admin (solicitudes escaladas)
  → Admin resuelve (p. ej. reinicia contraseña) y elimina el ticket resuelto
```

### Gestión de la base de conocimiento
```
Admin arrastra un documento (.md/.txt/.html)
  → BFF recibe el archivo → AI Service lo guarda e indexa en pgvector
  → El documento queda disponible para el RAG inmediatamente
  → Admin puede listarlo y eliminarlo cuando haya una nueva versión
```

## 4.15 Arquitectura propuesta

Microservicios iniciales [TEC]:

1. **Web/BFF** (.NET 9, Razor Pages + JavaScript)
2. **AuthService** (.NET 9 Web API)
3. **OnboardingService** (.NET 9 Web API)
4. **HrService** (.NET 9 Web API)
5. **AI Service** (Python + FastAPI + LangGraph + LangChain)

Infraestructura: PostgreSQL 16 + pgvector, Redis, Nginx, Docker Compose. [TEC]

Ver `docs/architecture/`.

## 4.16 Seguridad

- Contraseñas con Argon2id. [TEC]
- RLS en PostgreSQL (Colaborador, RRHH, Admin). [TEC]
- Cookies HttpOnly/Secure/SameSite. [TEC]
- Rate limiting con Redis. [TEC]
- Secretos en variables de entorno (`.env.example`, nunca `.env` en git). [TEC]
- La API Key de Groq solo reside en AI Service. [TEC]
- Protección contra prompt injection: documentos RAG tratados como datos, no como instrucciones. [TEC]
- Prevención de IDOR y mass assignment (DTOs). [TEC]
- El administrador **reinicia** contraseñas; nunca las ve (están hasheadas). [TEC]
- Subida de archivos restringida a admin, con validación de extensión y tamaño. [TEC]

## 4.17 Datos

**Schemas lógicos** [TEC]: `auth`, `onboarding`, `hr`, `ai`.

**Entidades principales** [TEC]:
- `auth.users`, `auth.roles`, `auth.sessions`, `auth.audit_events` (con `user_name`), `auth.login_attempts`.
- `onboarding.employees`, `onboarding.tasks`, `onboarding.employee_tasks`, `onboarding.plan_phases`, `onboarding.activities`.
- `hr.tickets` (con `requester_name` y `escalated`), `hr.ticket_comments` (con `author_name`), `hr.ticket_status_history`, `hr.categories`.
- `ai.documents`, `ai.chunks` (con embedding vectorial).

**Información sensible** [DOC]: PII de colaboradores, credenciales, tokens, llaves de API, cadenas de conexión. Nunca se registran ni se envían a proveedores públicos.

## 4.18 RAG e inteligencia artificial

- **Proveedor:** Groq, modelo `openai/gpt-oss-20b`. [TEC]
- **Pipeline:** Markdown/TXT/HTML → limpieza → chunking → embeddings → pgvector. [TEC]
- **Embeddings:** sentence-transformers `paraphrase-multilingual-MiniLM-L12-v2` (384 dims). [TEC]
- **Metadata por chunk:** documento, versión, hash, chunk, embedding, metadata, fecha de ingesta. [TEC]
- **Deduplicación:** no re-ingestar cuando el hash no cambió. [TEC]
- **Orquestación:** LangGraph con nodos `ValidateInput → DetectIntent → RetrieveContext → BuildPrompt → GenerateResponse → ValidateResponse`. [TEC]
- **Gestión de documentos:** subida por drag-and-drop (.md/.txt/.html), listado con número de chunks y eliminación. [TEC]
- **Fuentes del RAG** [DOC]: Manual de Cultura/Horarios, Guía de Primer Día TI, Código de Conducta y Seguridad, Plan 30-60-90, Ficha del proyecto (más documentos subidos por el admin).

## 4.19 Criterios de aceptación

1. Un colaborador puede iniciar sesión y acceder solo a sus propios datos.
2. El asistente responde con base en la documentación (RAG) y no inventa cuando no hay contexto.
3. Un ticket creado vía asistente aparece en HrService con el usuario correcto como propietario.
4. RR. HH. puede ver y cambiar el estado de los tickets y escalarlos al administrador.
5. El administrador puede crear/gestionar usuarios, reiniciar contraseñas y ver la auditoría por calendario.
6. El administrador puede subir, listar y eliminar documentos de la base de conocimiento.
7. Un ticket siempre muestra el nombre del solicitante y de los autores de comentarios.
8. Solo se eliminan tickets resueltos o cerrados, y únicamente por RRHH/admin.
9. Ningún secreto aparece en el frontend ni en los logs.
10. Las políticas RLS restringen el acceso en base de datos.
11. Todos los servicios pasan health checks y se ejecutan con Docker Compose.
12. El onboarding y el plan 30-60-90 son visibles únicamente para el colaborador.

## 4.20 Métricas / KPIs

- Tiempo promedio de primera respuesta del asistente. [TEC]
- Precisión de respuestas con contexto recuperado (evaluación manual). [TEC]
- % de tickets resueltos en N días. [TEC]
- % de onboarding completado por colaborador. [TEC]
- Tasa de preguntas sin contexto suficiente (fallback a canal humano). [TEC]

## 4.21 Roadmap

| Fase | Entregable | Estado |
|---|---|---|
| FASE 0 | PRD + arquitectura | ✅ Completado |
| FASE 1 | Infraestructura (Docker, PostgreSQL+pgvector, Redis, migraciones) | ✅ Completado |
| FASE 2 | AuthService | ✅ Completado |
| FASE 3 | OnboardingService | ✅ Completado |
| FASE 4 | HrService | ✅ Completado |
| FASE 5 | AI Service (RAG, LangGraph, Groq) | ✅ Completado |
| FASE 6 | Web/BFF | ✅ Completado |
| FASE 7 | Integración end-to-end | ✅ Completado |
| FASE 8 | QA y seguridad | ✅ Completado |
| Post-MVP | Separación de roles + panel admin (usuarios, auditoría, KB) | ✅ Completado |
| Post-MVP | Subida/listado/eliminación de documentos, reset de contraseña, modo oscuro, zona horaria MX | ✅ Completado |
| Post-MVP | Pruebas unitarias .NET (25) + pytest AI Service (17) + integración end-to-end (15) | ✅ Completado |
| Post-MVP | Repositorio Git + primer commit | ✅ Completado |
| Post-MVP | Cierre formal del MVP | ✅ Completado |

---

## Nota sobre la procedencia de la información

Los datos institucionales (nombre, RFC, dirección, horarios, políticas, plan 30-60-90, canales, etc.) provienen exclusivamente de `docs-source/`. Los datos de prueba se identifican con los prefijos `DEMO`, `TEST` o `EXAMPLE`. Toda decisión técnica (stack, arquitectura, modelos de datos) es propuesta del equipo y está marcada como [TEC].
