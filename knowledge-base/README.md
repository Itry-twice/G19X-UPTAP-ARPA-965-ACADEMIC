# Base de Conocimiento (RAG)

La base de conocimiento del asistente se construye desde los documentos Markdown de `docs-source/`.

## Pipeline

```
Markdown → Limpieza → Chunking → Embeddings → pgvector
```

El pipeline de ingesta está implementado en el **AI Service**:

- `services/ai-service/app/rag/ingest.py` — limpieza, chunking y orquestación.
- `services/ai-service/app/embeddings/` — proveedores de embeddings (`hash` sin dependencias, o `sentence-transformers`).
- `services/ai-service/ingest_cli.py` — CLI de ingesta.

## Ejecutar la ingesta

```bash
# Dentro del contenedor
docker exec -it plurione-ai-service python ingest_cli.py
```

## Metadatos por chunk

Cada chunk almacena: `document`, `chunk`, `hash`, y se guarda en `ai.chunks` con su `embedding` (vector de 384 dimensiones).

## Deduplicación

La ingesta calcula un hash por documento (`content_hash`). Si el hash no cambió, el documento no se re-ingesta, evitando chunks duplicados.

## Ampliar a otros formatos

El pipeline está diseñado para soportar posteriormente PDF, DOCX, TXT y HTML (el lector actual es Markdown; basta añadir nuevos *parsers* en `ingest.py`).
