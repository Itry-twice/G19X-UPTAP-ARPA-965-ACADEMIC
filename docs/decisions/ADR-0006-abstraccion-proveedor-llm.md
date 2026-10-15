# ADR-0006 — Abstracción del proveedor LLM

- **Estado:** Aceptado

## Contexto

Se usará Groq para el MVP, pero se desea cambiar de proveedor sin reescribir el sistema.

## Decisión

- El AI Service define una interfaz `LLMProvider` (chat + streaming). La implementación `GroqProvider` es la única para el MVP.
- El modelo se configura vía `GROQ_MODEL` (por defecto `openai/gpt-oss-20b`).

## Consecuencias

- Cambiar de proveedor implica implementar una clase, no reescribir el flujo.
