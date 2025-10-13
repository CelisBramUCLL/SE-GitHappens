import { useEffect, useState } from 'react';
import { signalRService } from '../services/signalRService';

interface UseSignalRReturn {
  isConnected: boolean;
  joinParty: (partyId: number) => Promise<void>;
  leaveParty: (partyId: number) => Promise<void>;
  notifySongAdded: (partyId: number, songId: number) => Promise<void>;
  notifySongRemoved: (partyId: number, songId: number) => Promise<void>;
}

export const useSignalR = (): UseSignalRReturn => {
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    let mounted = true;
    

    const connect = async () => {

      if (signalRService.isConnected) {
        setIsConnected(true);
        return;
      }

      try {
        await signalRService.connect();
        if (mounted) {
          setIsConnected(true);
        }
      } catch (error) {
        if (mounted) {
          console.error('Failed to connect to SignalR:', error);
          setIsConnected(false);
        }
      }
    };

    connect();


    return () => {
      mounted = false;
    };
  }, []);

  const joinParty = async (partyId: number) => {
    await signalRService.joinParty(partyId);
  };

  const leaveParty = async (partyId: number) => {
    await signalRService.leaveParty(partyId);
  };

  const notifySongAdded = async (partyId: number, songId: number) => {
    await signalRService.notifySongAdded(partyId, songId);
  };

  const notifySongRemoved = async (partyId: number, songId: number) => {
    await signalRService.notifySongRemoved(partyId, songId);
  };

  return {
    isConnected,
    joinParty,
    leaveParty,
    notifySongAdded,
    notifySongRemoved,
  };
};