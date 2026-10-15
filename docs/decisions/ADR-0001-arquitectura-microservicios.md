# ADR-0001 — Arquitectura de microservicios

- **Estado:** Aceptado
- **Fecha:** 2026-09-09

## Contexto

El proyecto requiere una plataforma modular (onboarding + IA + tickets) que pueda crecer. Se evalúa entre un monolito y microservicios.

## Decisión

Se adopta una arquitectura de microservicios con 5 componentes: Web/BFF, AuthService, OnboardingService, HrService y AI Service.

## Consecuencias

- Separación clara de responsabilidades y despliegue independiente.
- Mayor complejidad operativa (redes, health checks, comunicación interna).
- Cada servicio se escala y versiona de forma independiente.
