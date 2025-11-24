import { BaseService } from './base.service';

export interface ActiveUsersCountResponse {
  activeUserCount: number;
}

export interface ActiveUsersResponse {
  activeUsers: number[];
  count: number;
}

export interface UserStatusResponse {
  userId: number;
  isActive: boolean;
}

export class ActiveUserService extends BaseService {
  
  async getActiveUserCount(): Promise<ActiveUsersCountResponse> {
    const response = await fetch(this.buildUrl('/activeusers/count'));
    return this.handleResponse<ActiveUsersCountResponse>(response);
  }

  async getActiveUsers(): Promise<ActiveUsersResponse> {
    const response = await fetch(this.buildUrl('/activeusers/users'), {
      headers: this.getAuthHeaders(),
    });
    return this.handleResponse<ActiveUsersResponse>(response);
  }

  async getUserStatus(userId: number): Promise<UserStatusResponse> {
    const response = await fetch(this.buildUrl(`/activeusers/users/${userId}/status`));
    return this.handleResponse<UserStatusResponse>(response);
  }
}

export const activeUserService = new ActiveUserService();