import apiClient from './client';

export interface PlaceOrderRequest {
  instrumentId: string;
  side: 'BUY' | 'SELL';
  orderType: 'MARKET' | 'LIMIT';
  price?: number;
  quantity: number;
}

export interface Order {
  id: string;
  instrumentId: string;
  side: string;
  orderType: string;
  price?: number;
  quantity: number;
  remainingQuantity: number;
  status: string;
  createdAt: string;
}

export const placeOrder = (data: PlaceOrderRequest) =>
  apiClient.post<Order>('/api/orders', data).then((r) => r.data);

export const cancelOrder = (id: string) =>
  apiClient.delete(`/api/orders/${id}`).then((r) => r.data);
