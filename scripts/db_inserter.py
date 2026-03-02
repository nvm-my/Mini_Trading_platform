"""
Upsert scraped historical records into MongoDB.

The records are written to the ``HistoricalData`` collection inside the
configured database.  A compound unique index on ``(exchange, symbol, date)``
prevents duplicate entries and allows subsequent runs to update stale data.
"""

from __future__ import annotations

import logging
from typing import Sequence

from pymongo import ASCENDING, MongoClient
from pymongo.collection import Collection

from scrapers.base_scraper import HistoricalRecord

logger = logging.getLogger(__name__)

COLLECTION_NAME = "HistoricalData"


class DbInserter:
    """Inserts or updates :class:`HistoricalRecord` objects in MongoDB."""

    def __init__(
        self,
        connection_string: str,
        database_name: str = "TradingPlatformDb",
    ) -> None:
        self._client: MongoClient = MongoClient(connection_string)
        self._collection: Collection = self._client[database_name][COLLECTION_NAME]
        self._ensure_indexes()

    # ------------------------------------------------------------------
    # Public API
    # ------------------------------------------------------------------

    def upsert_records(self, records: Sequence[HistoricalRecord]) -> tuple[int, int]:
        """Upsert *records* into MongoDB.

        Returns:
            A ``(inserted, updated)`` tuple with the counts of new and
            modified documents respectively.
        """
        inserted = updated = 0
        for record in records:
            result = self._collection.update_one(
                {
                    "exchange": record.exchange,
                    "symbol": record.symbol,
                    "date": record.date,
                },
                {"$set": record.__dict__},
                upsert=True,
            )
            if result.upserted_id is not None:
                inserted += 1
            elif result.modified_count > 0:
                updated += 1

        logger.info(
            "Upserted %d records for %s exchange (%d new, %d updated)",
            len(records),
            records[0].exchange if records else "?",
            inserted,
            updated,
        )
        return inserted, updated

    def close(self) -> None:
        """Release the MongoDB connection."""
        self._client.close()

    # ------------------------------------------------------------------
    # Private helpers
    # ------------------------------------------------------------------

    def _ensure_indexes(self) -> None:
        """Create the unique compound index if it does not already exist."""
        self._collection.create_index(
            [
                ("exchange", ASCENDING),
                ("symbol", ASCENDING),
                ("date", ASCENDING),
            ],
            unique=True,
            name="exchange_symbol_date_unique",
        )
