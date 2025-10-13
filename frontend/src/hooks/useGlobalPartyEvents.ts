import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { useSignalR } from './useSignalR';
import { signalRService } from '../services/signalRService';

/**
 * Custom hook that manages global SignalR events for party management
 * This should be used by components that need to listen to global party events
 * like party creation and deletion for dashboard updates
 */
export const useGlobalPartyEvents = (componentName?: string) => {
  const queryClient = useQueryClient();
  const { isConnected } = useSignalR();
  const component = componentName || 'Unknown';

  useEffect(() => {
    if (!isConnected) return;

    const handlePartyCreated = async (party: any) => {
      console.log(`🎉 [${component}] Global party created event:`, party);
      console.log(`📊 [${component}] Current parties cache before creation:`, queryClient.getQueryData(['parties']));
      
      try {
        // Invalidate and refetch parties list
        await queryClient.invalidateQueries({ queryKey: ['parties'] });
        console.log(`✅ [${component}] Party creation - Query invalidated successfully`);
      } catch (error) {
        console.error(`❌ [${component}] Error updating parties cache on creation:`, error);
      }
    };

    const handlePartyDeleted = async (partyId: number) => {
      console.log(`🗑️ [${component}] Global party deleted event:`, partyId);
      console.log(`📊 [${component}] Current parties cache before update:`, queryClient.getQueryData(['parties']));
      
      try {
        // Manually update the cache by removing the deleted party
        queryClient.setQueryData(['parties'], (oldParties: any) => {
          if (!oldParties) return oldParties;
          console.log(`🔄 [${component}] Removing party`, partyId, 'from cache');
          const filteredParties = oldParties.filter((party: any) => party.id !== partyId);
          console.log(`📊 [${component}] Parties after filtering:`, filteredParties);
          return filteredParties;
        });
        
        console.log(`✅ [${component}] Party deleted - Cache updated (no refetch needed)`);
        
      } catch (error) {
        console.error(`❌ [${component}] Error updating parties cache:`, error);
      }
    };

    // Set up event listeners with unique references
    signalRService.onPartyCreated(handlePartyCreated);
    signalRService.onPartyDeletedGlobal(handlePartyDeleted);

    // Store references for cleanup - use the existing off method
    const cleanup = () => {
      // For now, use the general off method
      // This is acceptable since we're managing global events in a single hook
      signalRService.off('PartyCreated');
      signalRService.off('PartyDeletedGlobal');
    };

    return cleanup;
  }, [isConnected, queryClient]);
};