from __future__ import annotations

import json
import logging
import os
import re

from fastapi import APIRouter, Depends, File, HTTPException, Request, UploadFile
from fastapi.responses import StreamingResponse
from pydantic import BaseModel, Field

from ..auth import Identity, get_identity
from ..config import get_settings
from ..embeddings.sentence_provider import get_embedding_provider
from ..graph.assistant_graph import run_assistant, stream_assistant
from ..rag.ingest import ingest
from ..rate_limit import _limiter
from .. import db

logger = logging.getLogger(__name__)

router = APIRouter()

_ALLOWED_EXTENSIONS = {".md", ".txt", ".html", ".htm"}
_MAX_UPLOAD_BYTES = 10 * 1024 * 1024


def _safe_filename(filename: str) -> str:
    name = os.path.basename(filename).strip()
    name = re.sub(r"[^A-Za-z0-9._\- ]", "_", name)
    return name or "documento.txt"


class ChatRequest(BaseModel):
    question: str = Field(min_length=1, max_length=2000)


class ChatResponse(BaseModel):
    response: str
    sources: list[str]
    intent: str


@router.get("/health")
async def health():
    return {"status": "healthy", "service": "ai-service"}


@router.post("/chat", response_model=ChatResponse)
async def chat(body: ChatRequest, identity: Identity = Depends(get_identity)):
    if not _limiter.allow(identity.user_id):
        raise HTTPException(status_code=429, detail="Too many requests")

    result = await run_assistant(body.question, identity)
    if result.get("error") == "llm_unavailable":
        raise HTTPException(status_code=503, detail=result["response"])

    return ChatResponse(
        response=result["response"],
        sources=result["sources"],
        intent=result["intent"],
    )


@router.post("/chat/stream")
async def chat_stream(body: ChatRequest, identity: Identity = Depends(get_identity)):
    if not _limiter.allow(identity.user_id):
        raise HTTPException(status_code=429, detail="Too many requests")

    async def event_generator():
        async for event in stream_assistant(body.question, identity):
            yield f"data: {json.dumps(event, ensure_ascii=False)}\n\n"

    return StreamingResponse(
        event_generator(),
        media_type="text/event-stream",
        headers={"Cache-Control": "no-cache", "X-Accel-Buffering": "no"},
    )


@router.post("/ingest")
async def ingest_endpoint(identity: Identity = Depends(get_identity)):
    if identity.role != "admin":
        raise HTTPException(status_code=403, detail="Requiere rol de administrador")

    settings = get_settings()
    embedder = get_embedding_provider()
    result = await ingest([settings.docs_source_dir, settings.uploads_dir], embedder)
    return result


@router.post("/upload")
async def upload_document(
    file: UploadFile = File(...),
    identity: Identity = Depends(get_identity),
):
    if identity.role != "admin":
        raise HTTPException(status_code=403, detail="Requiere rol de administrador")

    filename = file.filename or ""
    ext = os.path.splitext(filename)[1].lower()
    if ext not in _ALLOWED_EXTENSIONS:
        raise HTTPException(
            status_code=400,
            detail="Tipo de archivo no soportado. Usa .md, .txt o .html.",
        )

    content = await file.read()
    if len(content) > _MAX_UPLOAD_BYTES:
        raise HTTPException(status_code=400, detail="El archivo supera el límite de 10 MB.")

    settings = get_settings()
    os.makedirs(settings.uploads_dir, exist_ok=True)
    safe_name = _safe_filename(filename)
    target = os.path.join(settings.uploads_dir, safe_name)
    with open(target, "wb") as fh:
        fh.write(content)

    embedder = get_embedding_provider()
    result = await ingest([settings.docs_source_dir, settings.uploads_dir], embedder)
    result["filename"] = safe_name
    return result


@router.get("/documents")
async def list_documents(identity: Identity = Depends(get_identity)):
    if identity.role != "admin":
        raise HTTPException(status_code=403, detail="Requiere rol de administrador")
    return await db.list_documents()


@router.delete("/documents/{document_id}")
async def delete_document(document_id: str, identity: Identity = Depends(get_identity)):
    if identity.role != "admin":
        raise HTTPException(status_code=403, detail="Requiere rol de administrador")

    name = await db.delete_document(document_id)
    if name is None:
        raise HTTPException(status_code=404, detail="Documento no encontrado")

    settings = get_settings()
    try:
        os.remove(os.path.join(settings.uploads_dir, name))
    except OSError:
        pass

    return {"status": "deleted", "name": name}
