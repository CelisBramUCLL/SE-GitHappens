import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { useSignalR } from './useSignalR';
import { signalRService } from '../services/signalRService';

// Hook for global party events (creation/deletion)
export const useGlobalPartyEvents = (componentName?: string) => {
  const queryClient = useQueryClient();
  const { isConnected } = useSignalR();
  const component = componentName || 'Unknown';

  useEffect(() => {
    if (!isConnected) return;

    const handlePartyCreated = async (party: any) => {
      console.log(`Party created:`, party);
      
      try {
        await queryClient.invalidateQueries({ queryKey: ['parties'] });
      } catch (error) {
        console.error('Error updating parties:', error);
      }
    };

    const handlePartyDeleted = async (partyId: number) => {
      console.log(`Party deleted:`, partyId);
      
      try {
        queryClient.setQueryData(['parties'], (oldParties: any) => {
          if (!oldParties) return oldParties;
          return oldParties.filter((party: any) => party.id !== partyId);
        });
        
      } catch (error) {
        console.error('Error updating parties:', error);
      }
    };


    signalRService.onPartyCreated(handlePartyCreated);
    signalRService.onPartyDeletedGlobal(handlePartyDeleted);

    const cleanup = () => {
      signalRService.off('PartyCreated');
      signalRService.off('PartyDeletedGlobal');
    };

    return cleanup;
  }, [isConnected, queryClient]);
};