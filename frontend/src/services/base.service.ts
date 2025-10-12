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
      console.log('Error response status:', response.status);
      console.log('Error response headers:', [...response.headers.entries()]);
      
      let errorMessage = `HTTP error! status: ${response.status}`;
      
      // Clone the response so we can read it multiple times if needed
      const responseClone = response.clone();
      
      try {
        // Try to parse error response as JSON first
        const errorData = await response.json();
        console.log('Parsed error data:', errorData);
        
        // Extract message from various possible error formats
        if (errorData && typeof errorData === 'object') {
          if (errorData.message) {
            errorMessage = errorData.message;
          } else if (errorData.error) {
            errorMessage = errorData.error;
          }
        } else if (typeof errorData === 'string') {
          errorMessage = errorData;
        }
      } catch (jsonError) {
        console.log('JSON parsing failed:', jsonError);
        // If JSON parsing fails, try reading as text
        try {
          const errorText = await responseClone.text();
          console.log('Error text:', errorText);
          if (errorText && errorText.trim()) {
            errorMessage = errorText;
          }
        } catch (textError) {
          console.warn('Failed to parse error response:', jsonError, textError);
        }
      }
      
      console.log('Final error message:', errorMessage);
      throw new Error(errorMessage);
    }
    
    return response.json();
  }

  protected buildUrl(endpoint: string, params?: URLSearchParams): string {
    const url = `${API_BASE_URL}${endpoint}`;
    return params ? `${url}?${params.toString()}` : url;
  }
}