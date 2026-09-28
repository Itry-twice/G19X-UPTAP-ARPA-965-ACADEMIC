from __future__ import annotations

from functools import lru_cache

from .base import EmbeddingProvider
from .hash_provider import HashEmbeddingProvider


class SentenceTransformerEmbeddingProvider(EmbeddingProvider):
    """Embeddings semánticos con sentence-transformers (all-MiniLM-L6-v2).

    La dependencia se importa de forma perezosa para no requerir torch en
    entornos que usan el proveedor de hash.
    """

    def __init__(self, model_name: str):
        from sentence_transformers import SentenceTransformer

        self._model = SentenceTransformer(model_name)
        self._dim = self._model.get_sentence_embedding_dimension()

    @property
    def dimension(self) -> int:
        return self._dim

    def embed(self, texts: list[str]) -> list[list[float]]:
        result = self._model.encode(texts, normalize_embeddings=True)
        return [list(map(float, v)) for v in result]


@lru_cache
def get_embedding_provider() -> EmbeddingProvider:
    from ..config import get_settings

    settings = get_settings()
    if settings.embedding_provider == "sentence-transformers":
        return SentenceTransformerEmbeddingProvider(settings.embedding_model)
    return HashEmbeddingProvider(settings.embedding_dim)
