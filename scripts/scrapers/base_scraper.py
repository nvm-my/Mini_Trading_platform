"""
Shared data model and HTTP session setup for DSE-BD and CSE-BD scrapers.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from datetime import datetime
from typing import Optional

import requests
from requests.adapters import HTTPAdapter
from urllib3.util.retry import Retry


@dataclass
class HistoricalRecord:
    """One row of OHLCV data for a stock on a given exchange and date."""

    symbol: str
    exchange: str       # "DSE" or "CSE"
    date: str           # ISO-8601: YYYY-MM-DD
    open: float
    high: float
    low: float
    close: float
    volume: int
    trade_count: Optional[int] = field(default=None)
    value: Optional[float] = field(default=None)  # turnover in local currency


class BaseScraper:
    """Base class providing a resilient requests session and common helpers."""

    EXCHANGE: str = ""

    def __init__(self, timeout: int = 30, retries: int = 3) -> None:
        self.timeout = timeout
        self.session = self._build_session(retries)

    # ------------------------------------------------------------------
    # Public API
    # ------------------------------------------------------------------

    def fetch_historical(
        self,
        symbol: str,
        start_date: str,
        end_date: str,
    ) -> list[HistoricalRecord]:
        """Return OHLCV records for *symbol* between *start_date* and *end_date*.

        Both dates must be in ``YYYY-MM-DD`` format.
        Subclasses must override this method.
        """
        raise NotImplementedError

    # ------------------------------------------------------------------
    # Internal helpers
    # ------------------------------------------------------------------

    def _build_session(self, retries: int) -> requests.Session:
        session = requests.Session()
        retry = Retry(
            total=retries,
            backoff_factor=0.5,
            status_forcelist=[429, 500, 502, 503, 504],
        )
        adapter = HTTPAdapter(max_retries=retry)
        session.mount("https://", adapter)
        session.mount("http://", adapter)
        session.headers.update(
            {
                "User-Agent": (
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
                    "AppleWebKit/537.36 (KHTML, like Gecko) "
                    "Chrome/124.0.0.0 Safari/537.36"
                ),
                "Accept-Language": "en-US,en;q=0.9",
            }
        )
        return session


# ------------------------------------------------------------------
# Shared parsing utilities
# ------------------------------------------------------------------


def normalise_date(raw: str) -> str:
    """Convert common Bangladesh-market date strings to ``YYYY-MM-DD``."""
    raw = raw.strip()
    for fmt in ("%d-%b-%Y", "%Y-%m-%d", "%d/%m/%Y", "%m/%d/%Y", "%d %b %Y"):
        try:
            return datetime.strptime(raw, fmt).strftime("%Y-%m-%d")
        except ValueError:
            continue
    return raw  # return as-is if no format matches


def clean_number(raw: str) -> str:
    """Strip commas and whitespace from a numeric string."""
    return raw.replace(",", "").replace(" ", "").strip()


def to_float(raw: str) -> float:
    """Parse a numeric string (with optional commas) to float."""
    return float(clean_number(raw))


def to_int(raw: str) -> int:
    """Parse a numeric string (with optional commas) to int."""
    return int(clean_number(raw))
