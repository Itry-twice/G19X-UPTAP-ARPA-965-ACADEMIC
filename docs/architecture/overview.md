# Arquitectura del Sistema

Este documento describe la arquitectura real implementada. Complementa el PRD (`docs/PRD.md`).

## 1. Vista general

```mermaid
flowchart TB
    Browser["Navegador (Cliente)"] -->|HTTPS| Nginx["Nginx (Reverse Proxy)"]
    Nginx --> Web["Web/BFF (.NET 9, Razor + JS)"]

    subgraph Internal["Red interna Docker"]
        Web --> Auth["AuthService (.NET 9)"]
        Web --> Onb["OnboardingService (.NET 9)"]
        Web --> Hr["HrService (.NET 9)"]
        Web --> Ai["AI Service (FastAPI)"]

        Auth --> PG["PostgreSQL 16 + pgvector"]
        Onb --> PG
        Hr --> PG
        Ai --> PG

        Auth --> Redis["Redis"]
        Web --> Redis
    end

    Ai -->|HTTP interno| Hr
    Ai -->|Groq API| Groq["Groq (openai/gpt-oss-20b)"]
```

## 2. Microservicios

| Servicio | Tecnología | Responsabilidad |
|---|---|---|
| Web/BFF | .NET 9 ASP.NET Core (Razor Pages + JS) | Interfaz, sesiones, cookies, anti-CSRF, proxy a microservicios |
| AuthService | .NET 9 Web API + EF Core | Usuarios, login/logout, roles, bloqueo, auditoría, JWT |
| OnboardingService | .NET 9 Web API + EF Core | Perfil, tareas, checklist, progreso, plan 30-60-90 |
| HrService | .NET 9 Web API + EF Core | Tickets, solicitudes, estados, comentarios, historial |
| AI Service | Python + FastAPI + LangGraph | RAG, embeddings, pgvector, Groq, function calling |

## 3. Comunicación entre servicios

```mermaid
sequenceDiagram
    participant B as Browser
    participant W as Web/BFF
    participant A as AuthService
    participant O as OnboardingService
    participant H as HrService
    participant AI as AI Service

    B->>W: POST /login (credenciales)
    W->>A: POST /auth/login
    A-->>W: access JWT + refresh token
    W-->>B: Set-Cookie (HttpOnly, Secure, SameSite)

    B->>W: GET /onboarding
    W->>O: GET /onboarding (Authorization: Bearer JWT)
    O-->>W: datos del colaborador
    W-->>B: HTML/JSON

    B->>W: POST /chat/messages
    W->>AI: POST /chat (Bearer JWT + mensaje)
    AI->>AI: RAG (pgvector) + Groq
    AI-->>W: streaming (SSE)
    W-->>B: SSE
```

## 4. Base de datos

```mermaid
erDiagram
    AUTH_USERS ||--o{ AUTH_SESSIONS : "tiene"
    AUTH_USERS }o--|| AUTH_ROLES : "asigna"

    ONB_EMPLOYEES ||--o{ ONB_EMPLOYEE_TASKS : "completa"
    ONB_TASKS ||--o{ ONB_EMPLOYEE_TASKS : "asignada"
    ONB_PLAN_PHASES ||--o{ ONB_ACTIVITIES : "contiene"

    HR_TICKETS ||--o{ HR_TICKET_COMMENTS : "tiene"
    HR_TICKETS ||--o{ HR_TICKET_STATUS_HISTORY : "registra"
    HR_CATEGORIES ||--o{ HR_TICKETS : "clasifica"

    AI_DOCUMENTS ||--o{ AI_CHUNKS : "divide"
```

Schemas lógicos: `auth`, `onboarding`, `hr`, `ai`.

## 5. Autenticación

```mermaid
sequenceDiagram
    participant U as Usuario
    participant W as Web/BFF
    participant A as AuthService
    participant R as Redis
    participant DB as PostgreSQL

    U->>W: login (email, password)
    W->>A: /auth/login
    A->>R: comprobar bloqueo / rate limit
    alt bloqueado o límite excedido
        A-->>W: 429 / 423
    else
        A->>DB: verificar usuario + Argon2id
        alt credenciales inválidas
            A->>R: incrementar intentos
            A-->>W: 401
        else
            A->>DB: crear sesión (refresh hash)
            A-->>W: access JWT + refresh
        end
    end
```

## 6. RAG

```mermaid
flowchart LR
    Q["Pregunta"] --> V["ValidateInput"]
    V --> I["DetectIntent"]
    I --> R["RetrieveContext (pgvector)"]
    R --> P["BuildPrompt"]
    P --> G["GenerateResponse (Groq)"]
    G --> VR["ValidateResponse"]
    VR --> END["Respuesta"]

    I -.->|acción| T["ToolCall"] --> H["HrService"] --> END
```

Pipeline de ingesta: Markdown → limpieza → chunking → embeddings → pgvector.

## 7. Deployment

```mermaid
flowchart TB
    subgraph Docker["Docker Compose"]
        Nginx --> Web
        Web --> Auth
        Web --> Onb
        Web --> Hr
        Web --> Ai
        Auth --> PG[(PostgreSQL)]
        Onb --> PG
        Hr --> PG
        Ai --> PG
        Auth --> Redis[(Redis)]
    end
```

## 8. Seguridad

```mermaid
flowchart LR
    subgraph Controles["Controles"]
        C1["Argon2id"]
        C2["JWT + cookies HttpOnly/Secure/SameSite"]
        C3["Rate limiting (Redis)"]
        C4["RLS en PostgreSQL"]
        C5["Anti-CSRF / XSS / SQLi / IDOR / SSRF"]
        C6["Prompt injection guard"]
        C7["Secretos fuera del código"]
        C8["Logging estructurado + correlation ID"]
    end
```
