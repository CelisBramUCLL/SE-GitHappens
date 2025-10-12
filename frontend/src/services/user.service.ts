import { BaseService } from './base.service';
import type { User } from '../types';

// User service - handles user management operations
export class UserService extends BaseService {
  // Get all users in the system
  async getAll(): Promise<User[]> {
    const response = await fetch(this.buildUrl('/users'), {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse<User[]>(response);
  }

  // Get a specific user by ID
  async getById(id: number): Promise<User> {
    const response = await fetch(this.buildUrl(`/users/${id}`), {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse<User>(response);
  }

  // Update user profile information
  async update(id: number, userData: Partial<User>): Promise<User> {
    const response = await fetch(this.buildUrl(`/users/${id}`), {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(userData),
    });
    return this.handleResponse<User>(response);
  }

  // Delete a user account
  async delete(id: number): Promise<void> {
    const response = await fetch(this.buildUrl(`/users/${id}`), {
      method: 'DELETE',
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse<void>(response);
  }
}

export const userService = new UserService();