import { createContext, useContext, useCallback, type ReactNode } from "react";

import { useLocalStorage } from "usehooks-ts";

type User = {
  email: string;
};

type AuthContextType = {
  user: User | null;
  login: (email: string, password: string) => Promise<boolean>;
  logout: () => void;
  register: (email: string, password: string) => Promise<boolean>;
  isAuthenticated: boolean;
  isLoading: boolean;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useLocalStorage<User | null>("user", null);

  const login = useCallback(async (email: string, password: string) => {
    if (password !== "password") return false;

    await new Promise((resolve) => setTimeout(resolve, 300));
    setUser({ email });
    return true;
  }, []);

  const logout = useCallback(() => {
    setUser(null);
  }, []);

  const register = useCallback(async (email: string, password: string) => {
    if (!email || !password) return false;

    await new Promise((resolve) => setTimeout(resolve, 300));
    setUser({ email });
    return true;
  }, []);

  const isAuthenticated = !!user;
  const isLoading = false;

  return (
    <AuthContext.Provider
      value={{
        user,
        login,
        logout,
        register,
        isAuthenticated,
        isLoading,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider");
  }
  return context;
}
