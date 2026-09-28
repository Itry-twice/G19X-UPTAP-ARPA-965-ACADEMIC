import time

from app.rate_limit import InMemoryRateLimiter


def test_allows_within_limit():
    rl = InMemoryRateLimiter(2, 60)
    assert rl.allow("cliente-a")
    assert rl.allow("cliente-a")


def test_denies_over_limit():
    rl = InMemoryRateLimiter(1, 60)
    assert rl.allow("cliente-a")
    assert not rl.allow("cliente-a")


def test_window_expires():
    rl = InMemoryRateLimiter(1, 0.1)
    assert rl.allow("cliente-a")
    assert not rl.allow("cliente-a")
    time.sleep(0.15)
    assert rl.allow("cliente-a")


def test_keys_are_independent():
    rl = InMemoryRateLimiter(1, 60)
    assert rl.allow("cliente-a")
    assert rl.allow("cliente-b")
