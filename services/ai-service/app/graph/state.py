from __future__ import annotations

from typing import Any, TypedDict

from ..auth import Identity


class AssistantState(TypedDict, total=False):
    question: str
    identity: Identity
    valid: bool
    error: str | None
    tool_calls: list[dict]
    tool_result: str | None
    context: list[dict]
    messages: list[dict]
    response: str
    sources: list[str]
    intent: str
