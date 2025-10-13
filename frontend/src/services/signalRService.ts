import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

class SignalRService {
  private connection: HubConnection | null = null;

  // Initialize connection
  async connect(): Promise<void> {
    // Prevent multiple simultaneous connections
    if (this.connection?.state === 'Connected' || this.connection?.state === 'Connecting') {
      console.log('SignalR already connected or connecting, skipping...');
      return;
    }

    // Clean up any existing connection
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }

    // Use HTTP for development since backend doesn't have proper HTTPS setup
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5097';
    
    this.connection = new HubConnectionBuilder()
      .withUrl(`${baseUrl}/partyHub`, {
        withCredentials: true, // Enable credentials for CORS
        skipNegotiation: false,
        transport: undefined // Let SignalR choose the best transport
      })
      .configureLogging(LogLevel.Information)
      .build();

    try {
      await this.connection.start();
      console.log('✅ SignalR connected successfully to', baseUrl);
    } catch (error) {
      console.error('❌ SignalR connection failed:', error);
      throw error;
    }
  }

  // Disconnect
  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      console.log('SignalR disconnected');
    }
  }

  // Join a party
  async joinParty(partyId: number): Promise<void> {
    if (this.connection?.state === 'Connected') {
      await this.connection.invoke('JoinParty', partyId);
      console.log(`📞 Joined party ${partyId}`);
    }
  }

  // Leave a party
  async leaveParty(partyId: number): Promise<void> {
    if (this.connection?.state === 'Connected') {
      await this.connection.invoke('LeaveParty', partyId);
      console.log(`📞 Left party ${partyId}`);
    }
  }

  // Notify song added
  async notifySongAdded(partyId: number, songId: number): Promise<void> {
    if (this.connection?.state === 'Connected') {
      await this.connection.invoke('NotifySongAdded', partyId, songId);
      console.log(`📞 Notified song ${songId} added to party ${partyId}`);
    }
  }

  // Notify song removed
  async notifySongRemoved(partyId: number, songId: number): Promise<void> {
    if (this.connection?.state === 'Connected') {
      await this.connection.invoke('NotifySongRemoved', partyId, songId);
      console.log(`📞 Notified song ${songId} removed from party ${partyId}`);
    }
  }

  // Set up event listeners
  onUserJoinedParty(callback: (userId: number, partyId: number) => void): void {
    this.connection?.on('UserJoinedParty', callback);
  }

  onUserLeftParty(callback: (userId: number, partyId: number) => void): void {
    this.connection?.on('UserLeftParty', callback);
  }

  onSongAdded(callback: (songId: number, connectionId: string) => void): void {
    this.connection?.on('SongAdded', callback);
  }

  onSongRemoved(callback: (songId: number, connectionId: string) => void): void {
    this.connection?.on('SongRemoved', callback);
  }

  // Remove event listeners
  off(eventName: string): void {
    this.connection?.off(eventName);
  }

  // Get connection state
  get isConnected(): boolean {
    return this.connection?.state === 'Connected';
  }
}

// Export a singleton instance
export const signalRService = new SignalRService();