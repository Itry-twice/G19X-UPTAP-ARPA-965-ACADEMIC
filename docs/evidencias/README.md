# Evidencias del MVP

Esta carpeta contiene evidencia de que el sistema funciona de punta a punta.

## Recorrido end-to-end (automatizado)

Última corrida: 29-oct-2026, commit `4dafb96`.

`integration_test_output.txt` — salida de `tests/e2e/integration_test.py`, ejecutado con el stack en Docker.

**Resultado: 15 comprobaciones correctas, 0 fallidas.**

El recorrido valida:

1. Autenticación y navegación por rol (colaborador, RRHH, admin).
2. Restricción del onboarding al rol colaborador (403 para RRHH/admin).
3. Tickets: creación, aislamiento por propietario (RLS) y visibilidad para RRHH.
4. Asistente (RAG): respuesta con streaming, fuentes citadas y término correcto.
5. Panel admin: lista de usuarios y acceso restringido (403 para RRHH).

## Instalación en carpeta limpia

Ver [`instalacion-limpia.md`](instalacion-limpia.md): bitácora de la instalación desde cero siguiendo el `README.md` (26-oct-2026, commit `887f606`).

## Cómo reproducirlo

```bash
docker compose up -d --build
docker exec -it plurione-ai-service python ingest_cli.py
python tests/e2e/integration_test.py
```

## Capturas del recorrido

1. **Login** — inicio de sesión.
   ![Login](captura-login.png)

2. **Chat con el asistente (RAG)** — respuesta con fuentes citadas.
   ![Chat con el asistente](chat-asistente.png)

3. **Ticket creado desde el chat** — *function calling* (`create_hr_ticket`).
   ![Chat que crea un ticket](chat-crea-ticket.png)

4. **Checklist 30-60-90** — onboarding y plan de integración.
   ![Checklist 30-60-90](checklist-30-60-90.png)

5. **Panel de Recursos Humanos** — lista de solicitudes.
   ![Panel de RRHH](panel-rrhh.png)

6. **Negritas en la respuesta del asistente** — el texto se pinta al terminar el streaming.
   ![Chat con negritas](chat-negritas.png)

7. **Checklist con estados en español** — Pendiente, En proceso, Completada.
   ![Checklist en español](checklist-estados-es.png)

8. **Perfil con el rol en español** — Colaborador / Recursos Humanos / Administrador.
   ![Perfil con rol en español](perfil-rol-es.png)

9. **Onboarding con estados de tareas** — tareas en «Pendiente» y «Completada».
   ![Onboarding con tareas](onboarding-tareas-estados.png)

10. **Ticket «Cerrada» en tema oscuro** — la etiqueta se lee sobre el fondo oscuro.
    ![Cerrada en tema oscuro](tickets-cerrada-oscuro.png)

11. **Chat con tabla de comparación** — el asistente responde con una tabla renderizada.
    ![Chat con tabla](chat-tabla-comparacion.png)
