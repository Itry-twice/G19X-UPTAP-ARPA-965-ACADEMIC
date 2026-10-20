from functools import lru_cache
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

    database_url: str = ""
    groq_api_key: str = ""
    groq_model: str = "openai/gpt-oss-20b"
    groq_base_url: str = "https://api.groq.com/openai/v1"

    embedding_provider: str = "sentence-transformers"
    embedding_model: str = "sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2"
    embedding_dim: int = 384

    jwt_signing_key: str = ""
    jwt_issuer: str = "plurione-auth"
    jwt_audience: str = "plurione"

    hr_service_url: str = "http://localhost:8080"
    docs_source_dir: str = "/app/docs-source"
    uploads_dir: str = "/app/uploads"

    max_question_length: int = 2000
    top_k: int = 8


@lru_cache
def get_settings() -> Settings:
    return Settings()
