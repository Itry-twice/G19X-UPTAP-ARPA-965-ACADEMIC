#!/usr/bin/env python
"""CLI para ingestar los documentos de docs-source/ en pgvector.

Uso:
    python ingest_cli.py --docs-dir ./docs-source
"""
from __future__ import annotations

import argparse
import asyncio
import logging

from app.config import get_settings
from app.db import init_pool, _pool
from app.embeddings.sentence_provider import get_embedding_provider
from app.rag.ingest import ingest

logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(name)s %(message)s")
logger = logging.getLogger(__name__)


async def main(docs_dir: str) -> None:
    await init_pool()
    embedder = get_embedding_provider()
    logger.info("Usando proveedor de embeddings: %s (dim=%s)", type(embedder).__name__, embedder.dimension)
    result = await ingest([docs_dir], embedder)
    logger.info("Ingesta completada: %s", result)
    if _pool is not None:
        await _pool.close()


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Ingesta de la base de conocimiento")
    parser.add_argument("--docs-dir", default=None, help="Directorio con los documentos Markdown")
    args = parser.parse_args()

    docs_dir = args.docs_dir or get_settings().docs_source_dir
    asyncio.run(main(docs_dir))
