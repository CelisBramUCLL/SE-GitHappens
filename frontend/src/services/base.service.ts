// Base service class with shared functionality for all API services

export const API_BASE_URL = 'http://localhost:5097/api';

export abstract class BaseService {
  protected getAuthHeaders(): HeadersInit {
    const token = localStorage.getItem('token');
    return {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
    };
  }

  protected async handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || `HTTP error! status: ${response.status}`);
    }
    return response.json();
  }

  protected buildUrl(endpoint: string, params?: URLSearchParams): string {
    const url = `${API_BASE_URL}${endpoint}`;
    return params ? `${url}?${params.toString()}` : url;
  }
}