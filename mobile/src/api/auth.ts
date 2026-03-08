import apiClient from './client';

export interface LoginRequest { email: string; password: string }
export interface RegisterRequest { name: string; email: string; password: string; role: string }
export interface AuthResponse { token: string }

export const login = (data: LoginRequest) =>
  apiClient.post<AuthResponse>('/api/auth/login', data).then((r) => r.data);

export const register = (data: RegisterRequest) =>
  apiClient.post<AuthResponse>('/api/auth/register', data).then((r) => r.data);
