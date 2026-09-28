import math

from app.embeddings.hash_provider import HashEmbeddingProvider


def test_dimension():
    p = HashEmbeddingProvider(384)
    assert p.dimension == 384


def test_embed_returns_unit_vectors():
    p = HashEmbeddingProvider(384)
    vecs = p.embed(["hola mundo", "adios"])
    assert len(vecs) == 2
    for v in vecs:
        assert len(v) == 384
        norm = math.sqrt(sum(x * x for x in v))
        assert abs(norm - 1.0) < 1e-6


def test_embed_is_deterministic():
    p = HashEmbeddingProvider(384)
    a = p.embed(["texto"])[0]
    b = p.embed(["texto"])[0]
    assert a == b


def test_similar_texts_closer_than_different():
    p = HashEmbeddingProvider(384)
    v1 = p.embed(["horario laboral"])[0]
    v2 = p.embed(["horario de trabajo"])[0]
    v3 = p.embed(["vacaciones anuales"])[0]

    def cosine(a, b):
        return sum(x * y for x, y in zip(a, b))

    assert cosine(v1, v2) > cosine(v1, v3)
