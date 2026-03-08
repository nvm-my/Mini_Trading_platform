import apiClient from './client';

export interface Trade {
  id: string;
  buyOrderId: string;
  sellOrderId: string;
  instrumentId: string;
  price: number;
  quantity: number;
  executedAt: string;
}

export const getMyTrades = () =>
  apiClient.get<Trade[]>('/api/trades/my').then((r) => r.data);
