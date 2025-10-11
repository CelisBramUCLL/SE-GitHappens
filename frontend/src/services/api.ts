import type { LoginDTO, CreateUserDTO, LoginResponseDTO, User } from '../types';

const API_BASE_URL = 'http://localhost:5097/api';

class ApiService {
  private getAuthHeaders(): HeadersInit {
    const token = localStorage.getItem('token');
    return {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
    };
  }

  private async handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || `HTTP error! status: ${response.status}`);
    }
    return response.json();
  }

  // Auth methods
  async login(loginData: LoginDTO): Promise<LoginResponseDTO> {
    const response = await fetch(`${API_BASE_URL}/users/login`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(loginData),
    });
    return this.handleResponse<LoginResponseDTO>(response);
  }

  async register(userData: CreateUserDTO): Promise<User> {
    const response = await fetch(`${API_BASE_URL}/users`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(userData),
    });
    return this.handleResponse<User>(response);
  }

  // User methods
  async getAllUsers(): Promise<User[]> {
    const response = await fetch(`${API_BASE_URL}/users`, {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse<User[]>(response);
  }

  async getUserById(id: number): Promise<User> {
    const response = await fetch(`${API_BASE_URL}/users/${id}`, {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse<User>(response);
  }

  // Session methods
  async getAllSessions() {
    const response = await fetch(`${API_BASE_URL}/session`, {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse(response);
  }

  async getSessionById(id: number) {
    const response = await fetch(`${API_BASE_URL}/session/${id}`, {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse(response);
  }

  async createSession(sessionData: { name: string }) {
    const response = await fetch(`${API_BASE_URL}/session`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(sessionData),
    });
    return this.handleResponse(response);
  }

  async updateSession(id: number, sessionData: { name?: string }) {
    const response = await fetch(`${API_BASE_URL}/session/${id}`, {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(sessionData),
    });
    return this.handleResponse(response);
  }

  async deleteSession(id: number) {
    const response = await fetch(`${API_BASE_URL}/session/${id}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse(response);
  }

  async joinSession(sessionId: number) {
    const response = await fetch(`${API_BASE_URL}/session/join`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ sessionId }),
    });
    return this.handleResponse(response);
  }

  async leaveSession(sessionId: number) {
    const response = await fetch(`${API_BASE_URL}/session/leave`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ sessionId }),
    });
    return this.handleResponse(response);
  }

  async addSongToSession(songId: number) {
    const response = await fetch(`${API_BASE_URL}/session/add-song`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ songId }),
    });
    return this.handleResponse(response);
  }

  async removeSongFromSession(songId: number) {
    const response = await fetch(`${API_BASE_URL}/session/remove-song`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ songId }),
    });
    return this.handleResponse(response);
  }

  // Songs methods
  async getAllSongs(search?: string, page: number = 1, pageSize: number = 20) {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
      ...(search && { search }),
    });
    
    const response = await fetch(`${API_BASE_URL}/songs?${params}`, {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse(response);
  }

  async searchSongs(query: string, limit: number = 10) {
    const params = new URLSearchParams({
      query,
      limit: limit.toString(),
    });
    
    const response = await fetch(`${API_BASE_URL}/songs/search?${params}`, {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse(response);
  }

  async getSongById(id: number) {
    const response = await fetch(`${API_BASE_URL}/songs/${id}`, {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse(response);
  }
}

export const apiService = new ApiService();