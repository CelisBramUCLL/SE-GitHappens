import { BaseService } from './base.service';
import type { LoginDTO, CreateUserDTO, LoginResponseDTO } from '../types';

// Authentication service - handles login, register, logout
export class AuthService extends BaseService {
  // Authenticate user with email and password
  async login(loginData: LoginDTO): Promise<LoginResponseDTO> {
    const response = await fetch(this.buildUrl('/users/login'), {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(loginData),
    });
    return this.handleResponse<LoginResponseDTO>(response);
  }

  // Register a new user account
  async register(userData: CreateUserDTO): Promise<any> {
    const response = await fetch(this.buildUrl('/users'), {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(userData),
    });
    return this.handleResponse(response);
  }

  // Logout current user (frontend only - clears token)
  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  }
}

export const authService = new AuthService();