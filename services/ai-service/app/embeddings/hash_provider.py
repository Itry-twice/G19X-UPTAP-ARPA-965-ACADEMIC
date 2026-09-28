from __future__ import annotations

import hashlib
import math
import re

from .base import EmbeddingProvider

_TOKEN_RE = re.compile(r"[a-z0-9áéíóúüñ]+")


def _tokens(text: str) -> list[str]:
    return _TOKEN_RE.findall(text.lower())


class HashEmbeddingProvider(EmbeddingProvider):
    """Embeddings determinísticos basados en hashing (sin dependencias externas).

    Útil para desarrollo y CI. Para producción se recomienda
    `SentenceTransformerEmbeddingProvider`.
    """

    def __init__(self, dimension: int = 384):
        self._dim = dimension

    @property
    def dimension(self) -> int:
        return self._dim

    def embed(self, texts: list[str]) -> list[list[float]]:
        vectors: list[list[float]] = []
        for text in texts:
            vec = [0.0] * self._dim
            tokens = _tokens(text)
            for token in tokens:
                digest = hashlib.md5(token.encode("utf-8")).hexdigest()
                idx = int(digest, 16) % self._dim
                vec[idx] += 1.0
            for i in range(1, len(tokens)):
                digest = hashlib.md5(f"{tokens[i - 1]} {tokens[i]}".encode("utf-8")).hexdigest()
                idx = int(digest, 16) % self._dim
                vec[idx] += 1.0
            norm = math.sqrt(sum(x * x for x in vec)) or 1.0
            vectors.append([x / norm for x in vec])
        return vectors
