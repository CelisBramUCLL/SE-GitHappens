import { BaseService } from './base.service';

// Party service - handles music party management
export class PartyService extends BaseService {
  // Get all available parties
  async getAll() {
    const response = await fetch(this.buildUrl('/party'), {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse(response);
  }

  // Get a specific party by ID with full details
  async getById(id: number) {
    const response = await fetch(this.buildUrl(`/party/${id}`), {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse(response);
  }

  // Create a new music party
  async create(partyData: { name: string }) {
    const response = await fetch(this.buildUrl('/party'), {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(partyData),
    });
    return this.handleResponse(response);
  }

  // Update party details
  async update(id: number, partyData: { name?: string }) {
    const response = await fetch(this.buildUrl(`/party/${id}`), {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(partyData),
    });
    return this.handleResponse(response);
  }

  // Delete/stop a party
  async delete(id: number) {
    const response = await fetch(this.buildUrl(`/party/${id}`), {
      method: 'DELETE',
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse(response);
  }

  // Join an existing party as a participant
  async join(partyId: number) {
    const response = await fetch(this.buildUrl('/party/join'), {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ partyId }),
    });
    return this.handleResponse(response);
  }

  // Leave a party you're currently participating in
  async leave(partyId: number) {
    const response = await fetch(this.buildUrl('/party/leave'), {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ partyId }),
    });
    return this.handleResponse(response);
  }

  // Add a song to the party playlist
  async addSong(songId: number) {
    const response = await fetch(this.buildUrl('/party/add-song'), {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ songId }),
    });
    return this.handleResponse(response);
  }

  // Remove a song from the party playlist
  async removeSong(songId: number) {
    const response = await fetch(this.buildUrl('/party/remove-song'), {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ songId }),
    });
    return this.handleResponse(response);
  }

  // Get user's current active party (if any)
  async getMyActiveParty() {
    const response = await fetch(this.buildUrl('/party/my-active-party'), {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse(response);
  }
}

export const partyService = new PartyService();