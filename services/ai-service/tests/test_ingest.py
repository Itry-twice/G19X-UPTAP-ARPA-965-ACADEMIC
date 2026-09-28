from app.rag.ingest import clean_markdown, chunk_text, split_sections, build_chunks


def test_clean_markdown_removes_headers():
    assert clean_markdown("## Titulo\n") == "Titulo"
    assert clean_markdown("### Subtitulo\n") == "Subtitulo"


def test_clean_markdown_removes_bold_and_italic():
    assert clean_markdown("**negrita** y *cursiva*") == "negrita y cursiva"


def test_clean_markdown_removes_links_keeps_text():
    assert "texto" in clean_markdown("[texto](https://example.com)")


def test_clean_markdown_removes_bullets_and_numbering():
    texto = clean_markdown("1. item uno\n2. item dos\n* item tres")
    assert "item uno" in texto and "item dos" in texto and "item tres" in texto


def test_chunk_text_splits_long_content():
    largo = " ".join(["palabra"] * 2000)
    chunks = chunk_text(largo)
    assert len(chunks) > 1


def test_chunk_text_short_content_single_chunk():
    chunks = chunk_text("Un texto corto de prueba.")
    assert len(chunks) == 1


def test_split_sections_by_headers():
    md = "# Seccion A\ncontenido A\n\n# Seccion B\ncontenido B"
    sections = split_sections(md)
    assert len(sections) == 2
    assert "Seccion A" in sections[0] and "contenido A" in sections[0]
    assert "Seccion B" in sections[1] and "contenido B" in sections[1]


def test_build_chunks_keeps_header_with_content():
    content = "## Esquema Hibrido (Modelo 3x2)\n\nPluriOne implementa 3 dias presencial y 2 dias remoto."
    chunks = build_chunks("doc.md", content)
    assert len(chunks) == 1
    assert "Esquema Hibrido" in chunks[0].content
    assert "3 dias presencial" in chunks[0].content


def test_build_chunks_metadata():
    chunks = build_chunks("doc.md", "Contenido de prueba.")
    assert chunks[0].metadata["document"] == "doc.md"
    assert "hash" in chunks[0].metadata
    assert chunks[0].index == 0
