# Guía de verificación (para el evaluador)

Pasos exactos para levantar y probar el sistema de punta a punta.

## Requisitos

- **Docker** y **Docker Compose** en ejecución.
- Una **clave de Groq** (`GROQ_API_KEY`) para que el asistente responda.

## 1. Configurar

```bash
cp .env.example .env
```

Editar `.env` y completar al menos `GROQ_API_KEY` (y las contraseñas si se desea).

## 2. Levantar

```bash
docker compose up -d --build
docker compose ps
```

> La primera compilación descarga imágenes y el modelo de embeddings (varios minutos). Todos los servicios deben quedar `Up` (`postgres` y `redis` en `healthy`).

## 3. Ingerir la base de conocimiento

```bash
docker exec -it plurione-ai-service python ingest_cli.py
```

## 4. Probar los flujos (http://localhost)

### Colaborador (`mariana.lopez@example.com` / `PluriOne#2026`)
- Iniciar sesión → ver Dashboard con progreso.
- **Onboarding**: marcar tareas y ver el progreso.
- **Plan 30-60-90**: ver las 3 fases con hitos.
- **Asistente**: preguntar "¿Cuál es el horario laboral?" → debe responder con datos del documento (RAG) y citar fuentes.
- **Asistente (function calling)**: escribir "no tengo acceso a mi correo" → debe crear un ticket automáticamente.
- **Tickets**: crear una solicitud manualmente y verla en "Mis solicitudes".

### RRHH (`rrhh@example.com` / `PluriOne#2026`)
- Ver **Panel RRHH** con todas las solicitudes.
- Cambiar el estado de un ticket y agregar un comentario.
- **Escalar** un ticket al administrador.

### Admin (`admin@example.com` / `PluriOne#2026`)
- Ver **Panel Admin**: lista de usuarios, cambiar rol, activar/desactivar.
- Ver el **calendario de auditoría** (seleccionar un día con actividad).
- Ver las **solicitudes escaladas** y eliminarlas si están resueltas.
- **Base de conocimiento**: subir un `.md` y verlo en la lista de documentos.

## 5. Pruebas automatizadas

```bash
# .NET (47 pruebas)
dotnet test tests/PluriOne.Tests/PluriOne.Tests.csproj

# AI Service (17 pruebas)
cd services/ai-service && pip install pytest && python -m pytest tests -v

# Integración end-to-end (requiere el stack arriba)
python tests/e2e/integration_test.py
```

## 6. Integración continua

Última corrida en verde: 29-oct-2026, commit `4dafb96`.

- Build y tests (.NET): 47 pruebas, correctas
- Tests (AI Service): 17 pruebas, correctas

Enlace: https://github.com/Itry-twice/G19X-UPTAP-ARPA-965-ACADEMIC/actions/runs/40432850521
