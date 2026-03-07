import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getMyTrades, Trade } from '../api/trades';

export default function TradesPage() {
  const navigate = useNavigate();
  const [trades, setTrades] = useState<Trade[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    getMyTrades()
      .then(setTrades)
      .catch(() => setError('Failed to load trades.'))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="page">
      <header>
        <h1>My Trades</h1>
        <button onClick={() => navigate('/instruments')}>Back to Instruments</button>
      </header>
      {loading && <p>Loading…</p>}
      {error && <p className="error">{error}</p>}
      {!loading && trades.length === 0 && <p>No trades yet.</p>}
      <table>
        <thead>
          <tr>
            <th>Trade ID</th>
            <th>Instrument</th>
            <th>Price</th>
            <th>Quantity</th>
            <th>Executed At</th>
          </tr>
        </thead>
        <tbody>
          {trades.map((t) => (
            <tr key={t.id}>
              <td>{t.id}</td>
              <td>{t.instrumentId}</td>
              <td>{t.price.toFixed(2)}</td>
              <td>{t.quantity}</td>
              <td>{new Date(t.executedAt).toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
