from __future__ import annotations

from abc import ABC, abstractmethod
from typing import Any, AsyncIterator


class LLMProvider(ABC):
    """Abstracción para cambiar de proveedor LLM sin reescribir el flujo."""

    @abstractmethod
    async def generate(self, messages: list[dict], tools: list[dict] | None = None) -> dict:
        """Devuelve {"content": str, "tool_calls": [...] }."""

    @abstractmethod
    async def stream(self, messages: list[dict]) -> AsyncIterator[str]:
        """Genera tokens de forma incremental."""


class LLMError(Exception):
    pass


class LLMUnavailableError(LLMError):
    pass
