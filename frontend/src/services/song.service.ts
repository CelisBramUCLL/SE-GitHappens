import { BaseService } from './base.service';

// Song service - handles song browsing and search
export class SongService extends BaseService {
  // Get all songs with pagination and optional search
  async getAll(search?: string, page: number = 1, pageSize: number = 20) {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
      ...(search && { search }),
    });
    
    const response = await fetch(this.buildUrl('/songs', params), {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse(response);
  }

  // Search songs by query with limit
  async search(query: string, limit: number = 10) {
    const params = new URLSearchParams({
      query,
      limit: limit.toString(),
    });
    
    const response = await fetch(this.buildUrl('/songs/search', params), {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse(response);
  }

  // Get a specific song by ID
  async getById(id: number) {
    const response = await fetch(this.buildUrl(`/songs/${id}`), {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse(response);
  }

  // Get songs by artist
  async getByArtist(artistName: string) {
    const params = new URLSearchParams({
      artist: artistName,
    });
    
    const response = await fetch(this.buildUrl('/songs/by-artist', params), {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse(response);
  }

  // Get songs by album
  async getByAlbum(albumName: string) {
    const params = new URLSearchParams({
      album: albumName,
    });
    
    const response = await fetch(this.buildUrl('/songs/by-album', params), {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse(response);
  }
}

export const songService = new SongService();