import axios from 'axios';
import { useAuthStore } from './store';

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'https://localhost:5001'
});

api.interceptors.request.use((config) => {
  const { accessToken: token, activeCompanyId, user } = useAuthStore.getState();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  if (user?.email?.toLowerCase() === 'admin@demo.com' && activeCompanyId) {
    config.headers['X-Company-Id'] = activeCompanyId;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) useAuthStore.getState().logout();
    return Promise.reject(error);
  }
);
