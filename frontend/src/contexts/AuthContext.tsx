import React, { createContext, useContext, useState, useEffect } from 'react';
import type { User, LoginDTO, CreateUserDTO } from '../types';
import { authService } from '../services/auth.service';
import { signalRService } from '../services/signalRService';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginDTO) => Promise<void>;
  register: (userData: CreateUserDTO) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: React.ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem('token');
    const userData = localStorage.getItem('user');
    
    if (token && userData) {
      try {
        const parsedUser = JSON.parse(userData);
        setUser(parsedUser);
        
        // Auto-connect to SignalR when user is authenticated on load
        signalRService.connect().catch((error) => {
          console.error('Failed to connect to SignalR on app load:', error);
        });
      } catch (error) {
        console.error('Error parsing user data:', error);
        localStorage.removeItem('token');
        localStorage.removeItem('user');
      }
    }
    setIsLoading(false);
  }, []);

  const login = async (credentials: LoginDTO) => {
    try {
      const response = await authService.login(credentials);
      localStorage.setItem('token', response.token);
      
      const userData: User = {
        id: response.id,
        username: response.username,
        firstName: '',
        lastName: '',
        email: '',
        role: 'User' as any
      };
      
      localStorage.setItem('user', JSON.stringify(userData));
      setUser(userData);
      
      // Connect to SignalR after successful login
      signalRService.connect().catch((error) => {
        console.error('Failed to connect to SignalR after login:', error);
      });
    } catch (error) {
      throw error;
    }
  };

  const register = async (userData: CreateUserDTO) => {
    try {
      const user = await authService.register(userData);
      const loginResponse = await authService.login({
        email: userData.email,
        password: userData.password
      });
      
      localStorage.setItem('token', loginResponse.token);
      localStorage.setItem('user', JSON.stringify(user));
      setUser(user);
      
      // Connect to SignalR after successful registration
      signalRService.connect().catch((error) => {
        console.error('Failed to connect to SignalR after registration:', error);
      });
    } catch (error) {
      throw error;
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
    
    // Disconnect from SignalR on logout
    signalRService.disconnect().catch((error) => {
      console.error('Failed to disconnect from SignalR on logout:', error);
    });
  };

  const value = {
    user,
    isAuthenticated: !!user,
    isLoading,
    login,
    register,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};