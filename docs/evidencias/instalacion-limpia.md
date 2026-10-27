# Instalación en carpeta limpia — 26-oct-2026 · commit `887f606`

Se clonó el repositorio en una carpeta nueva y se siguió el `README.md` al pie de la letra, sin copiar el `.env` de trabajo.

| Paso | Qué hice | Resultado | Tiempo |
|---|---|---|---|
| 1 | `git clone` en `C:\Users\rrock\Desktop\Virtual Assistant v2\G19X-UPTAP-ARPA-965-ACADEMIC` | Bien | 1 min |
| 2 | `cp .env.example .env` y completar `GROQ_API_KEY` + contraseñas (generadas aleatoriamente) | Bien | 3 min |
| 3 | `docker compose up -d --build` | **Falla parcial**: `000_roles.sh` no ejecutable (saltos de línea CRLF del clon rompen el *shebang*). Corregido → ver nota abajo. | ~13 min |
| 4 | `docker compose down -v` + `docker compose up -d` (tras corregir el script) | 8 servicios en `healthy` | ~1 min |
| 5 | `docker exec -it plurione-ai-service python ingest_cli.py` | 4 documentos ingeridos, 3287 chunks | ~1 min |
| 6 | Entrar como colaborador, RRHH y admin + preguntar al asistente | Responde con fuentes (RAG); recorrido e2e 15/15 | 2 min |

## Problema encontrado y corregido

El clon traía `infrastructure/postgres/init/000_roles.sh` con saltos de línea **CRLF** (por `core.autocrlf=true` sin `.gitattributes`). Al arrancar, `docker-entrypoint.sh` no pudo ejecutarlo:

```
/docker-entrypoint-initdb.d/000_roles.sh: cannot execute: required file not found
```

**Solución**: se convirtió el script a LF y se agregó un `.gitattributes` a la raíz del repositorio que fuerza `eol=lf` para `*.sh`, `*.sql`, `Dockerfile` y `*.conf`, para que no vuelva a ocurrir en clonaciones futuras. Tras el arreglo, los scripts `000_roles.sh`, `001_schema.sql` y `002_seed.sql` corrieron correctamente y las cuentas DEMO quedaron sembradas.

## Cuentas DEMO verificadas

| Rol | Usuario | Contraseña |
|---|---|---|
| Colaborador | `mariana.lopez@example.com` | `PluriOne#2026` |
| RRHH | `rrhh@example.com` | `PluriOne#2026` |
| Admin | `admin@example.com` | `PluriOne#2026` |
