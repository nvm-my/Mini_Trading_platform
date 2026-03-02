"""
CLI entry point for scraping DSE-BD and/or CSE-BD historical data
and inserting the results into MongoDB.

Example usage
-------------
Scrape the last year of data for two symbols from both exchanges::

    python main.py --symbols BRACBANK ACME --exchange ALL

Scrape only DSE, custom date range::

    python main.py --symbols BRACBANK --exchange DSE \\
        --start-date 2023-01-01 --end-date 2023-12-31

Point at a remote MongoDB cluster::

    python main.py --symbols ACME --mongo-uri "mongodb+srv://user:pw@cluster/db"
"""

from __future__ import annotations

import argparse
import logging
import sys
from datetime import date, timedelta

from db_inserter import DbInserter
from scrapers.cse_scraper import CseScraper
from scrapers.dse_scraper import DseScraper

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s  %(levelname)-8s  %(message)s",
    datefmt="%Y-%m-%d %H:%M:%S",
)
logger = logging.getLogger(__name__)


def _build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Scrape DSE-BD / CSE-BD historical OHLCV data into MongoDB.",
    )
    parser.add_argument(
        "--symbols",
        nargs="+",
        required=True,
        metavar="SYMBOL",
        help="One or more stock ticker symbols (e.g. BRACBANK ACME).",
    )
    parser.add_argument(
        "--exchange",
        choices=["DSE", "CSE", "ALL"],
        default="ALL",
        help="Exchange to scrape.  Use ALL for both DSE and CSE (default: ALL).",
    )
    parser.add_argument(
        "--start-date",
        default=str(date.today() - timedelta(days=365)),
        metavar="YYYY-MM-DD",
        help="Inclusive start date (default: one year ago).",
    )
    parser.add_argument(
        "--end-date",
        default=str(date.today()),
        metavar="YYYY-MM-DD",
        help="Inclusive end date (default: today).",
    )
    parser.add_argument(
        "--mongo-uri",
        default="mongodb://localhost:27017",
        metavar="URI",
        help="MongoDB connection string (default: mongodb://localhost:27017).",
    )
    parser.add_argument(
        "--db-name",
        default="TradingPlatformDb",
        metavar="NAME",
        help="Target MongoDB database name (default: TradingPlatformDb).",
    )
    return parser


def main(argv: list[str] | None = None) -> int:
    """Run the scraper pipeline.  Returns an exit code (0 = success)."""
    args = _build_arg_parser().parse_args(argv)

    # Select which scrapers to run
    scrapers = []
    if args.exchange in ("DSE", "ALL"):
        scrapers.append(DseScraper())
    if args.exchange in ("CSE", "ALL"):
        scrapers.append(CseScraper())

    inserter = DbInserter(args.mongo_uri, args.db_name)
    total_inserted = total_updated = 0
    errors = 0

    try:
        for scraper in scrapers:
            for symbol in args.symbols:
                logger.info(
                    "Scraping %s on %s  [%s → %s]",
                    symbol,
                    scraper.EXCHANGE,
                    args.start_date,
                    args.end_date,
                )
                try:
                    records = scraper.fetch_historical(
                        symbol, args.start_date, args.end_date
                    )
                    if records:
                        ins, upd = inserter.upsert_records(records)
                        total_inserted += ins
                        total_updated += upd
                    else:
                        logger.warning(
                            "No records returned for %s on %s", symbol, scraper.EXCHANGE
                        )
                except Exception as exc:  # noqa: BLE001
                    logger.error(
                        "Error scraping %s on %s: %s", symbol, scraper.EXCHANGE, exc
                    )
                    errors += 1
    finally:
        inserter.close()

    logger.info(
        "Finished. %d inserted, %d updated, %d error(s).",
        total_inserted,
        total_updated,
        errors,
    )
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
