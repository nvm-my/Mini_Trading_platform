import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getInstruments, Instrument } from '../api/instruments';
import { useAuth } from '../context/AuthContext';

export default function InstrumentsPage() {
  const { signOut } = useAuth();
  const navigate = useNavigate();
  const [instruments, setInstruments] = useState<Instrument[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    getInstruments()
      .then(setInstruments)
      .catch(() => setError('Failed to load instruments.'))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="page">
      <header>
        <h1>Instruments</h1>
        <nav>
          <button onClick={() => navigate('/trades')}>My Trades</button>
          <button onClick={signOut}>Sign Out</button>
        </nav>
      </header>
      {loading && <p>Loading…</p>}
      {error && <p className="error">{error}</p>}
      <table>
        <thead>
          <tr>
            <th>Symbol</th>
            <th>Company</th>
            <th>Price</th>
            <th>Action</th>
          </tr>
        </thead>
        <tbody>
          {instruments.map((inst) => (
            <tr key={inst.id}>
              <td>{inst.symbol}</td>
              <td>{inst.companyName}</td>
              <td>{inst.currentPrice.toFixed(2)}</td>
              <td>
                <button onClick={() => navigate(`/orders/new?instrumentId=${inst.id}`)}>
                  Trade
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
