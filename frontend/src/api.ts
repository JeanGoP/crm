import axios from 'axios';
import { useAuthStore } from './store';

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'https://localhost:5001',
  headers: {
    'X-Tenant': import.meta.env.VITE_TENANT ?? 'demo'
  }
});

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) useAuthStore.getState().logout();
    return Promise.reject(error);
  }
);
