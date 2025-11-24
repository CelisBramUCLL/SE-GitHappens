import { useState, useEffect } from 'react';
import { signalRService } from '../services/signalRService';
import { activeUserService } from '../services/activeUser.service';

export const useActiveUsers = () => {
  const [activeUserCount, setActiveUserCount] = useState<number>(0);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // Fetch initial count
  const fetchActiveUserCount = async () => {
    try {
      setIsLoading(true);
      const response = await activeUserService.getActiveUserCount();
      setActiveUserCount(response.activeUserCount);
      setError(null);
    } catch (err) {
      console.error('Failed to fetch active user count:', err);
      setError(err instanceof Error ? err.message : 'Failed to load active user count');
    } finally {
      setIsLoading(false);
    }
  };

  // Listen to real-time updates from SignalR
  useEffect(() => {
    // Fetch initial count
    fetchActiveUserCount();

    const handleActiveUserCountUpdate = (count: number) => {
      setActiveUserCount(count);
      setIsLoading(false);
      setError(null);
    };

    // Set up SignalR listener for real-time updates
    if (signalRService.isConnected) {
      signalRService.onActiveUserCountUpdated(handleActiveUserCountUpdate);
    } else {
      // If not connected, establish connection first
      const connectAndListen = async () => {
        try {
          await signalRService.connect();
          signalRService.onActiveUserCountUpdated(handleActiveUserCountUpdate);
        } catch (error) {
          console.error('Failed to connect to SignalR:', error);
        }
      };
      connectAndListen();
    }

    // Cleanup function
    return () => {
      signalRService.off('ActiveUserCountUpdated');
    };
  }, []);

  return {
    activeUserCount,
    isLoading,
    error,
    refreshCount: fetchActiveUserCount
  };
};