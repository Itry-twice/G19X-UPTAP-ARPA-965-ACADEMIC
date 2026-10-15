# ADR-0002 — Lenguajes y frameworks

- **Estado:** Aceptado

## Contexto

Se requieren servicios de backend, un BFF con frontend integrado y un servicio de IA.

## Decisión

- Backend y BFF: .NET 9 (ASP.NET Core).
- Frontend: Razor Pages + JavaScript (sin React).
- IA: Python + FastAPI + LangGraph + LangChain.
- LLM: Groq, modelo `openai/gpt-oss-20b`.

## Consecuencias

- Consistencia con el stack definido en el proyecto.
- El BFF integra el frontend y evita exponer infraestructura al navegador.
