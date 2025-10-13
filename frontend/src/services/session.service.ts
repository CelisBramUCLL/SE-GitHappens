import { BaseService } from "./base.service";

// Session service - handles music session management
export class SessionService extends BaseService {
  // Get all available sessions
  async getAll() {
    const response = await fetch(this.buildUrl("/session"), {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse(response);
  }

  // Get a specific session by ID with full details
  async getById(id: number) {
    const response = await fetch(this.buildUrl(`/session/${id}`), {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse<any>(response);
  }

  // Create a new music session
  async create(sessionData: { name: string }) {
    const response = await fetch(this.buildUrl("/session"), {
      method: "POST",
      headers: this.getAuthHeaders(),
      body: JSON.stringify(sessionData),
    });
    return this.handleResponse(response);
  }

  // Update session details
  async update(id: number, sessionData: { name?: string }) {
    const response = await fetch(this.buildUrl(`/session/${id}`), {
      method: "PUT",
      headers: this.getAuthHeaders(),
      body: JSON.stringify(sessionData),
    });
    return this.handleResponse(response);
  }

  // Delete/stop a session
  async delete(id: number) {
    const response = await fetch(this.buildUrl(`/session/${id}`), {
      method: "DELETE",
      headers: this.getAuthHeaders(),
    });

    return this.handleResponse(response);
  }

  // Join an existing session as a participant
  async join(sessionId: number) {
    const response = await fetch(this.buildUrl("/session/join"), {
      method: "POST",
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ sessionId }),
    });
    return this.handleResponse(response);
  }

  // Leave a session you're currently participating in
  async leave(sessionId: number) {
    const response = await fetch(this.buildUrl("/session/leave"), {
      method: "POST",
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ sessionId }),
    });
    return this.handleResponse(response);
  }

  // Add a song to the session playlist
  async addSong(songId: number) {
    const response = await fetch(this.buildUrl("/session/add-song"), {
      method: "POST",
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ songId }),
    });
    return this.handleResponse(response);
  }

  // Remove a song from the session playlist
  async removeSong(songId: number) {
    const response = await fetch(this.buildUrl("/session/remove-song"), {
      method: "POST",
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ songId }),
    });
    return this.handleResponse(response);
  }
}

export const sessionService = new SessionService();
