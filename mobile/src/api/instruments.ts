import apiClient from './client';

export interface Instrument {
  id: string;
  symbol: string;
  companyName: string;
  currentPrice: number;
  isActive: boolean;
}

export const getInstruments = () =>
  apiClient.get<Instrument[]>('/api/instruments').then((r) => r.data);
