"""
Scraper for historical stock data from the Dhaka Stock Exchange (DSE-BD).

DSE website: https://dsebd.org
Historical data endpoint:
  https://dsebd.org/dseX_share.php?startDate=YYYY-MM-DD&endDate=YYYY-MM-DD
                                  &name=SYMBOL&format=tableview

The page returns an HTML table with columns:
  Date | Open | High | Low | Closingprice | Volume | Trade | Value

Usage example::

    scraper = DseScraper()
    records = scraper.fetch_historical("BRACBANK", "2024-01-01", "2024-03-31")
"""

from __future__ import annotations

from bs4 import BeautifulSoup

from .base_scraper import (
    BaseScraper,
    HistoricalRecord,
    clean_number,
    normalise_date,
    to_float,
    to_int,
)

DSE_HISTORICAL_URL = "https://dsebd.org/dseX_share.php"


class DseScraper(BaseScraper):
    """Fetch historical OHLCV data from Dhaka Stock Exchange (dsebd.org)."""

    EXCHANGE = "DSE"

    def fetch_historical(
        self,
        symbol: str,
        start_date: str,
        end_date: str,
    ) -> list[HistoricalRecord]:
        """Download and parse the DSE historical data table for *symbol*.

        Args:
            symbol:     DSE ticker symbol (e.g. ``"BRACBANK"``, ``"ACME"``).
            start_date: Inclusive start date in ``YYYY-MM-DD`` format.
            end_date:   Inclusive end date in ``YYYY-MM-DD`` format.

        Returns:
            A (possibly empty) list of :class:`HistoricalRecord` objects,
            one per trading day.

        Raises:
            requests.HTTPError: If the server returns a non-2xx status code.
        """
        params = {
            "startDate": start_date,
            "endDate": end_date,
            "name": symbol,
            "format": "tableview",
        }
        response = self.session.get(
            DSE_HISTORICAL_URL, params=params, timeout=self.timeout
        )
        response.raise_for_status()
        return self._parse_html(symbol, response.text)

    # ------------------------------------------------------------------
    # Private helpers
    # ------------------------------------------------------------------

    def _parse_html(self, symbol: str, html: str) -> list[HistoricalRecord]:
        """Extract records from the DSE historical data HTML table."""
        soup = BeautifulSoup(html, "lxml")
        records: list[HistoricalRecord] = []

        # The table may have class "table" or similar; fall back to the first table
        table = soup.find("table", {"class": "table"}) or soup.find("table")
        if table is None:
            return records

        # Skip the header row
        for row in table.find_all("tr")[1:]:
            cols = [td.get_text(strip=True) for td in row.find_all("td")]
            if len(cols) < 6:
                continue
            try:
                record = HistoricalRecord(
                    symbol=symbol,
                    exchange=self.EXCHANGE,
                    date=normalise_date(cols[0]),
                    open=to_float(cols[1]),
                    high=to_float(cols[2]),
                    low=to_float(cols[3]),
                    close=to_float(cols[4]),
                    volume=to_int(cols[5]),
                    trade_count=to_int(cols[6]) if len(cols) > 6 else None,
                    value=to_float(cols[7]) if len(cols) > 7 else None,
                )
                records.append(record)
            except (ValueError, IndexError):
                # Skip malformed rows silently
                continue

        return records
