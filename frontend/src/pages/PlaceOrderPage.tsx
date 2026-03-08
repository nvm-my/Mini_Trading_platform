import { FormEvent, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { placeOrder } from '../api/orders';

export default function PlaceOrderPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const instrumentId = searchParams.get('instrumentId') ?? '';

  const [side, setSide] = useState<'BUY' | 'SELL'>('BUY');
  const [orderType, setOrderType] = useState<'MARKET' | 'LIMIT'>('LIMIT');
  const [price, setPrice] = useState('');
  const [quantity, setQuantity] = useState('1');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    try {
      const order = await placeOrder({
        instrumentId,
        side,
        orderType,
        price: orderType === 'LIMIT' ? parseFloat(price) : undefined,
        quantity: parseInt(quantity, 10),
      });
      setSuccess(`Order placed – Status: ${order.status}`);
    } catch {
      setError('Failed to place order.');
    }
  };

  return (
    <div className="page">
      <h2>Place Order</h2>
      <form onSubmit={handleSubmit}>
        <label>
          Side
          <select value={side} onChange={(e) => setSide(e.target.value as 'BUY' | 'SELL')}>
            <option value="BUY">BUY</option>
            <option value="SELL">SELL</option>
          </select>
        </label>
        <label>
          Order Type
          <select value={orderType} onChange={(e) => setOrderType(e.target.value as 'MARKET' | 'LIMIT')}>
            <option value="LIMIT">LIMIT</option>
            <option value="MARKET">MARKET</option>
          </select>
        </label>
        {orderType === 'LIMIT' && (
          <label>
            Price
            <input type="number" step="0.01" min="0.01" value={price} onChange={(e) => setPrice(e.target.value)} required />
          </label>
        )}
        <label>
          Quantity
          <input type="number" min="1" value={quantity} onChange={(e) => setQuantity(e.target.value)} required />
        </label>
        {error && <p className="error">{error}</p>}
        {success && <p className="success">{success}</p>}
        <button type="submit">Submit Order</button>
        <button type="button" onClick={() => navigate('/instruments')}>Cancel</button>
      </form>
    </div>
  );
}
