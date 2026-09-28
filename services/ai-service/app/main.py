from __future__ import annotations

import logging
from contextlib import asynccontextmanager

from fastapi import FastAPI

from .api.routes import router
from .config import get_settings
from .db import init_pool, _pool

logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(name)s %(message)s")
logger = logging.getLogger(__name__)


@asynccontextmanager
async def lifespan(app: FastAPI):
    await init_pool()
    logger.info("AI Service iniciado (modelo: %s)", get_settings().groq_model)
    yield
    if _pool is not None:
        await _pool.close()


app = FastAPI(title="PluriOne AI Service", version="1.0.0", lifespan=lifespan)
app.include_router(router)
