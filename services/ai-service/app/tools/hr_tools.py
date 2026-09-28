from __future__ import annotations

import logging
import time
from typing import Any

import httpx

from ..auth import Identity
from ..config import get_settings

logger = logging.getLogger(__name__)

CREATE_HR_TICKET_TOOL = {
    "type": "function",
    "function": {
        "name": "create_hr_ticket",
        "description": (
            "Crea una solicitud (ticket) para Recursos Humanos cuando el usuario "
            "reporta un problema o necesita un trámite (por ejemplo: sin acceso al correo, "
            "equipo dañado, trámites administrativos)."
        ),
        "parameters": {
            "type": "object",
            "properties": {
                "subject": {"type": "string", "description": "Asunto breve de la solicitud."},
                "description": {"type": "string", "description": "Descripción detallada del problema."},
                "category": {
                    "type": "string",
                    "description": (
                        "Una de: Accesos y credenciales, Soporte técnico (TI), Nómina y prestaciones, "
                        "Recursos Humanos / administrativo, Equipo y hardware, Otros"
                    ),
                },
                "priority": {"type": "string", "enum": ["low", "normal", "high"]},
            },
            "required": ["subject"],
        },
    },
}

_category_cache: dict[str, str] = {}
_category_ts: float = 0.0
_CATEGORY_TTL = 300.0


async def _categories(identity: Identity) -> dict[str, str]:
    """Obtiene {nombre: id} de categorías desde HrService, con caché breve."""
    global _category_cache, _category_ts
    now = time.monotonic()
    if _category_cache and (now - _category_ts) < _CATEGORY_TTL:
        return _category_cache

    settings = get_settings()
    async with httpx.AsyncClient(timeout=10.0) as client:
        resp = await client.get(
            f"{settings.hr_service_url}/api/hr/categories",
            headers={"Authorization": f"Bearer {identity.token}"},
        )
        resp.raise_for_status()
        data = resp.json()

    mapping = {c["name"].lower(): c["id"] for c in data}
    _category_cache = mapping
    _category_ts = now
    return mapping


async def execute_create_hr_ticket(identity: Identity, arguments: dict[str, Any]) -> dict:
    """Crea un ticket en HrService usando la identidad del usuario (su JWT)."""
    settings = get_settings()

    subject = (arguments.get("subject") or "").strip()
    if not subject:
        return {"ok": False, "error": "subject_required"}

    category_id = None
    category_name = (arguments.get("category") or "").strip().lower()
    if category_name:
        categories = await _categories(identity)
        category_id = categories.get(category_name)

    payload = {
        "subject": subject[:200],
        "description": (arguments.get("description") or "")[:2000],
        "categoryId": category_id,
        "priority": arguments.get("priority") or "normal",
        "source": "assistant",
    }

    async with httpx.AsyncClient(timeout=15.0) as client:
        resp = await client.post(
            f"{settings.hr_service_url}/api/hr/tickets",
            json=payload,
            headers={"Authorization": f"Bearer {identity.token}"},
        )

    if resp.status_code >= 400:
        logger.warning("HrService create ticket failed: %s", resp.status_code)
        return {"ok": False, "error": "hr_service_error", "status": resp.status_code}

    ticket = resp.json()
    return {"ok": True, "ticket_id": ticket.get("id"), "subject": ticket.get("subject")}


async def execute_tool_call(identity: Identity, name: str, arguments: dict[str, Any]) -> str:
    if name == "create_hr_ticket":
        result = await execute_create_hr_ticket(identity, arguments)
        if result.get("ok"):
            return (
                f"He creado tu solicitud con el asunto «{result.get('subject')}». "
                f"El equipo de Recursos Humanos la revisará y te dará seguimiento. "
                f"Número de ticket: {result.get('ticket_id')}."
            )
        return "No pude crear la solicitud en este momento. Inténtalo de nuevo o contacta a Recursos Humanos."
    return "No pude completar esa acción."
