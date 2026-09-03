import axios, { InternalAxiosRequestConfig } from 'axios';
import { useAuthStore } from './store';
import { User } from './types';

const baseURL = import.meta.env.VITE_API_URL ?? 'https://localhost:5001';

export const api = axios.create({
  baseURL
});

const refreshClient = axios.create({ baseURL });
type RetryableRequest = InternalAxiosRequestConfig & { _retry?: boolean };
type RefreshResponse = { accessToken: string; refreshToken: string; user: User };
let refreshPromise: Promise<string> | undefined;

const renewAccessToken = async () => {
  const currentRefreshToken = useAuthStore.getState().refreshToken;
  if (!currentRefreshToken) throw new Error('No hay una sesion renovable.');
  const { data } = await refreshClient.post<RefreshResponse>('/api/auth/refresh', { refreshToken: currentRefreshToken });
  useAuthStore.getState().refreshSession(data.accessToken, data.refreshToken, data.user);
  return data.accessToken;
};

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
    const originalRequest = error.config as RetryableRequest | undefined;
    const isRefreshRequest = originalRequest?.url?.includes('/api/auth/refresh');
    if (error.response?.status === 401 && originalRequest && !originalRequest._retry && !isRefreshRequest && useAuthStore.getState().refreshToken) {
      originalRequest._retry = true;
      try {
        refreshPromise ??= renewAccessToken().finally(() => { refreshPromise = undefined; });
        const accessToken = await refreshPromise;
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return api(originalRequest);
      } catch {
        useAuthStore.getState().logout();
      }
    } else if (error.response?.status === 401 && !useAuthStore.getState().refreshToken) {
      useAuthStore.getState().logout();
    }
    return Promise.reject(error);
  }
);
