import { create } from 'zustand';
import { User } from './types';

interface AuthState {
  accessToken?: string;
  refreshToken?: string;
  user?: User;
  setSession: (accessToken: string, refreshToken: string, user: User) => void;
  logout: () => void;
}

const stored = localStorage.getItem('crm-session');

export const useAuthStore = create<AuthState>((set) => ({
  ...(stored ? JSON.parse(stored) : {}),
  setSession: (accessToken, refreshToken, user) => {
    const session = { accessToken, refreshToken, user };
    localStorage.setItem('crm-session', JSON.stringify(session));
    set(session);
  },
  logout: () => {
    localStorage.removeItem('crm-session');
    set({ accessToken: undefined, refreshToken: undefined, user: undefined });
  }
}));
