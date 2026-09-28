from __future__ import annotations

import time
from collections import defaultdict, deque


class InMemoryRateLimiter:
    """Limitador en memoria por clave (usuario/IP). Adecuado para el MVP."""

    def __init__(self, max_requests: int, window_seconds: float):
        self._max = max_requests
        self._window = window_seconds
        self._hits: dict[str, deque] = defaultdict(deque)

    def allow(self, key: str) -> bool:
        now = time.monotonic()
        dq = self._hits[key]
        while dq and now - dq[0] > self._window:
            dq.popleft()
        if len(dq) >= self._max:
            return False
        dq.append(now)
        return True


_limiter = InMemoryRateLimiter(max_requests=20, window_seconds=60.0)
