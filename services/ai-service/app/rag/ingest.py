from __future__ import annotations

import hashlib
import logging
import re
from dataclasses import dataclass
from pathlib import Path

from ..embeddings.base import EmbeddingProvider

logger = logging.getLogger(__name__)

CHUNK_SIZE = 900
CHUNK_OVERLAP = 120


@dataclass
class Chunk:
    index: int
    content: str
    metadata: dict


def clean_markdown(text: str) -> str:
    """Limpieza básica del markdown para su indexación."""
    text = re.sub(r"!\[[^\]]*\]\([^)]*\)", "", text)  # imágenes
    text = re.sub(r"\[([^\]]+)\]\([^)]*\)", r"\1", text)  # enlaces -> texto
    text = re.sub(r"#{1,6}\s*", "", text)  # encabezados
    text = re.sub(r"(\*\*|__)(.*?)\1", r"\2", text)  # negritas
    text = re.sub(r"(\*|_)(.*?)\1", r"\2", text)  # cursivas
    text = re.sub(r"`{1,3}[^`]*`{1,3}", "", text)  # código inline
    text = re.sub(r"^\s*[-*+]\s+", "", text, flags=re.MULTILINE)  # viñetas
    text = re.sub(r"^\s*\d+\.\s+", "", text, flags=re.MULTILINE)  # listas numeradas
    text = re.sub(r"\|", " ", text)  # tablas
    text = re.sub(r"-{3,}", "", text)  # líneas divisorias
    text = re.sub(r"[ \t]+", " ", text)
    text = re.sub(r"\n{3,}", "\n\n", text)
    return text.strip()


def chunk_text(text: str) -> list[str]:
    """Divide el texto en fragmentos con solapamiento."""
    paragraphs = [p.strip() for p in text.split("\n\n") if p.strip()]
    chunks: list[str] = []
    current = ""

    for para in paragraphs:
        if len(current) + len(para) + 2 <= CHUNK_SIZE:
            current = f"{current}\n\n{para}".strip()
        else:
            if current:
                chunks.append(current)
            if len(para) > CHUNK_SIZE:
                # fragmento largo: dividir en ventanas
                words = para.split(" ")
                window: list[str] = []
                length = 0
                for w in words:
                    if length + len(w) + 1 > CHUNK_SIZE and window:
                        chunks.append(" ".join(window))
                        overlap = window[-max(1, CHUNK_OVERLAP // 12):]
                        window = list(overlap)
                        length = sum(len(x) + 1 for x in window)
                    window.append(w)
                    length += len(w) + 1
                current = " ".join(window)
            else:
                current = para

    if current:
        chunks.append(current)

    return chunks


def split_sections(text: str) -> list[str]:
    """Divide el markdown por encabezados para mantener cada título con su contenido."""
    sections: list[str] = []
    current: list[str] = []

    for line in text.split("\n"):
        if re.match(r"^#{1,6}\s+", line):
            if current:
                sections.append("\n".join(current))
            current = [line]
        else:
            current.append(line)

    if current:
        sections.append("\n".join(current))

    return sections


def build_chunks(name: str, content: str) -> list[Chunk]:
    chunks: list[Chunk] = []
    index = 0

    for section in split_sections(content):
        cleaned = clean_markdown(section)
        for text in chunk_text(cleaned):
            chunks.append(Chunk(
                index=index,
                content=text,
                metadata={
                    "document": name,
                    "chunk": index,
                    "hash": hashlib.sha256(text.encode("utf-8")).hexdigest(),
                },
            ))
            index += 1

    return chunks


def read_documents(dirs: list[str]) -> list[tuple[str, str]]:
    """Lee todos los documentos de texto (md/txt/html) de los directorios dados."""
    docs: list[tuple[str, str]] = []
    for dir_path in dirs:
        base = Path(dir_path)
        if not base.exists():
            continue
        for ext in ("*.md", "*.txt", "*.html", "*.htm"):
            for path in sorted(base.rglob(ext)):
                try:
                    docs.append((path.name, path.read_text(encoding="utf-8", errors="replace")))
                except Exception:
                    continue
    return docs


async def ingest(docs_dirs: list[str], embedder: EmbeddingProvider) -> dict:
    """Pipeline de ingesta completo. Retorna un resumen."""
    from .. import db

    docs = read_documents(docs_dirs)
    if not docs:
        return {"documents": 0, "chunks": 0, "status": "no_documents"}

    pool = db.get_pool()
    created_docs = 0
    total_chunks = 0

    async with pool.acquire() as conn:
        from pgvector.asyncpg import register_vector

        await register_vector(conn)
        for name, content in docs:
            content_hash = hashlib.sha256(content.encode("utf-8")).hexdigest()
            doc_id, created = await db.insert_document(conn, name, content_hash, version="2026-V1")
            if created:
                created_docs += 1
                chunks = build_chunks(name, content)
                if chunks:
                    embeddings = embedder.embed([c.content for c in chunks])
                    await db.insert_chunks(conn, doc_id, [c.__dict__ for c in chunks], embeddings)
                    total_chunks += len(chunks)

    return {
        "documents": len(docs),
        "created_documents": created_docs,
        "chunks": total_chunks,
        "status": "ok",
    }
