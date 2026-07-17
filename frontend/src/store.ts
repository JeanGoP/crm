import { create } from 'zustand';
import { User } from './types';

interface AuthState {
  accessToken?: string;
  refreshToken?: string;
  user?: User;
  activeCompanyId?: string;
  setSession: (accessToken: string, refreshToken: string, user: User) => void;
  setActiveCompanyId: (companyId: string) => void;
  logout: () => void;
}

const stored = localStorage.getItem('crm-session');

export const useAuthStore = create<AuthState>((set) => ({
  ...(stored ? JSON.parse(stored) : {}),
  setSession: (accessToken, refreshToken, user) => {
    const session = { accessToken, refreshToken, user, activeCompanyId: user.companyId };
    localStorage.setItem('crm-session', JSON.stringify(session));
    set(session);
  },
  setActiveCompanyId: (companyId) => set((state) => {
    const session = { ...state, activeCompanyId: companyId };
    localStorage.setItem('crm-session', JSON.stringify(session));
    return { activeCompanyId: companyId };
  }),
  logout: () => {
    localStorage.removeItem('crm-session');
    set({ accessToken: undefined, refreshToken: undefined, user: undefined, activeCompanyId: undefined });
  }
}));
