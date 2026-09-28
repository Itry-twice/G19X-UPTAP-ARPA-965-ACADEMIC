from __future__ import annotations

import json
import logging
from typing import Any, AsyncIterator

import httpx

from ..config import get_settings
from .base import LLMProvider, LLMUnavailableError

logger = logging.getLogger(__name__)

DEFAULT_MAX_TOKENS = 1024
DEFAULT_TEMPERATURE = 0.2


class GroqProvider(LLMProvider):
    def __init__(self):
        self._settings = get_settings()
        if not self._settings.groq_api_key:
            raise LLMUnavailableError("No se configuró una API key para el proveedor de IA.")

    def _headers(self) -> dict:
        return {
            "Authorization": f"Bearer {self._settings.groq_api_key}",
            "Content-Type": "application/json",
        }

    def _url(self) -> str:
        return f"{self._settings.groq_base_url}/chat/completions"

    async def generate(self, messages: list[dict], tools: list[dict] | None = None) -> dict:
        payload: dict[str, Any] = {
            "model": self._settings.groq_model,
            "messages": messages,
            "temperature": DEFAULT_TEMPERATURE,
            "max_tokens": DEFAULT_MAX_TOKENS,
            "stream": False,
        }
        if tools:
            payload["tools"] = tools
            payload["tool_choice"] = "auto"

        try:
            async with httpx.AsyncClient(timeout=60.0) as client:
                resp = await client.post(self._url(), headers=self._headers(), json=payload)
                resp.raise_for_status()
                data = resp.json()
        except httpx.HTTPError as exc:
            logger.error("Groq request failed: %s", exc)
            raise LLMUnavailableError("No fue posible contactar al proveedor de IA.") from exc

        choice = data.get("choices", [{}])[0]
        message = choice.get("message", {})
        content = message.get("content") or ""

        tool_calls: list[dict] = []
        for tc in message.get("tool_calls", []):
            fn = tc.get("function", {})
            args = {}
            try:
                args = json.loads(fn.get("arguments") or "{}")
            except json.JSONDecodeError:
                args = {}
            tool_calls.append({"id": tc.get("id"), "name": fn.get("name"), "arguments": args})

        return {"content": content, "tool_calls": tool_calls}

    async def stream(self, messages: list[dict]) -> AsyncIterator[str]:
        payload: dict[str, Any] = {
            "model": self._settings.groq_model,
            "messages": messages,
            "temperature": DEFAULT_TEMPERATURE,
            "max_tokens": DEFAULT_MAX_TOKENS,
            "stream": True,
        }

        try:
            async with httpx.AsyncClient(timeout=120.0) as client:
                async with client.stream("POST", self._url(), headers=self._headers(), json=payload) as resp:
                    resp.raise_for_status()
                    async for line in resp.aiter_lines():
                        if not line.startswith("data:"):
                            continue
                        data = line[len("data:"):].strip()
                        if data == "[DONE]":
                            break
                        try:
                            obj = json.loads(data)
                        except json.JSONDecodeError:
                            continue
                        delta = obj.get("choices", [{}])[0].get("delta", {}).get("content")
                        if delta:
                            yield delta
        except httpx.HTTPError as exc:
            logger.error("Groq stream failed: %s", exc)
            raise LLMUnavailableError("No fue posible contactar al proveedor de IA.") from exc
