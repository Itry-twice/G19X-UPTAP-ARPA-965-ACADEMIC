from __future__ import annotations

import json
import logging
from typing import Any, Optional

import asyncpg
from pgvector.asyncpg import register_vector

from .config import get_settings

logger = logging.getLogger(__name__)

_pool: Optional[asyncpg.Pool] = None


async def init_pool() -> asyncpg.Pool:
    global _pool
    settings = get_settings()
    _pool = await asyncpg.create_pool(settings.database_url, min_size=1, max_size=10)
    async with _pool.acquire() as conn:
        await register_vector(conn)
        await conn.execute("SET search_path TO ai")
    return _pool


def get_pool() -> asyncpg.Pool:
    assert _pool is not None, "Database pool not initialized"
    return _pool


async def insert_document(conn: asyncpg.Connection, name: str, content_hash: str, version: Optional[str]) -> int:
    """Insert or get existing document id. Returns (id, created: bool)."""
    row = await conn.fetchrow(
        "SELECT id FROM ai.documents WHERE name = $1 AND content_hash = $2",
        name, content_hash,
    )
    if row:
        return row["id"], False

    row = await conn.fetchrow(
        "INSERT INTO ai.documents (name, version, content_hash) VALUES ($1, $2, $3) RETURNING id",
        name, version, content_hash,
    )
    return row["id"], True


async def insert_chunks(
    conn: asyncpg.Connection,
    document_id: int,
    chunks: list[dict],
    embeddings: list[list[float]],
) -> None:
    for idx, (chunk, emb) in enumerate(zip(chunks, embeddings)):
        await conn.execute(
            """
            INSERT INTO ai.chunks (document_id, chunk_index, content, metadata, embedding)
            VALUES ($1, $2, $3, $4::jsonb, $5)
            """,
            document_id,
            chunk["index"],
            chunk["content"],
            json.dumps(chunk["metadata"]),
            emb,
        )


async def search_chunks(embedding: list[float], top_k: int) -> list[dict]:
    pool = get_pool()
    async with pool.acquire() as conn:
        await register_vector(conn)
        rows = await conn.fetch(
            """
            SELECT content, metadata,
                   1 - (embedding <=> $1) AS similarity
            FROM ai.chunks
            ORDER BY embedding <=> $1
            LIMIT $2
            """,
            embedding,
            top_k,
        )
    return [
        {
            "content": r["content"],
            "metadata": json.loads(r["metadata"]) if r["metadata"] else {},
            "similarity": float(r["similarity"]),
        }
        for r in rows
    ]


async def list_documents() -> list[dict]:
    pool = get_pool()
    async with pool.acquire() as conn:
        rows = await conn.fetch(
            """
            SELECT d.id, d.name, d.ingested_at, count(c.id) AS chunks
            FROM ai.documents d
            LEFT JOIN ai.chunks c ON c.document_id = d.id
            GROUP BY d.id, d.name, d.ingested_at
            ORDER BY d.name
            """
        )
    return [
        {
            "id": str(r["id"]),
            "name": r["name"],
            "chunks": r["chunks"],
            "ingested_at": r["ingested_at"].isoformat(),
        }
        for r in rows
    ]


async def delete_document(document_id: str) -> str | None:
    pool = get_pool()
    async with pool.acquire() as conn:
        row = await conn.fetchrow(
            "DELETE FROM ai.documents WHERE id = $1 RETURNING name",
            document_id,
        )
    return row["name"] if row else None
