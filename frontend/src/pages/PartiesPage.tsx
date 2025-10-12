import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { partyService } from '../services/party.service';
import { Layout } from '../components/Layout';
import { Button } from '../components/ui/button';
import { useAuth } from '../contexts/AuthContext';
import { Plus, Music, Users, Calendar } from 'lucide-react';

export const PartiesPage: React.FC = () => {
  const queryClient = useQueryClient();
  const { user } = useAuth();

  const { data: parties, isLoading, error } = useQuery({
    queryKey: ['parties'],
    queryFn: () => partyService.getAll(),
  });

  const joinPartyMutation = useMutation({
    mutationFn: (partyId: number) => partyService.join(partyId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['parties'] });
      queryClient.invalidateQueries({ queryKey: ['my-active-party'] });
    },
  });

  const handleJoinParty = async (partyId: number) => {
    try {
      await joinPartyMutation.mutateAsync(partyId);
    } catch (error) {
      // Show user-friendly error message for common constraint violations
      const errorMessage = error instanceof Error ? error.message : 'Failed to join party';
      
      if (errorMessage.includes('already participating') || errorMessage.includes('already hosting')) {
        alert('You can only be in one active party at a time. Please leave your current party first.');
      } else {
        console.error('Failed to join party:', error);
      }
    }
  };

  // Helper function to check if current user is already in the party
  const isUserInParty = (party: any) => {
    if (!user || !party.participants) return false;
    return party.participants.some((participant: any) => 
      participant.userName === user.username
    );
  };

  // Helper function to check if current user is the host
  const isUserHost = (party: any) => {
    if (!user || !party.hostUser) return false;
    return party.hostUser.username === user.username;
  };

  if (isLoading) {
    return (
      <Layout>
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
        </div>
      </Layout>
    );
  }

  if (error) {
    return (
      <Layout>
        <div className="rounded-md bg-red-50 p-4">
          <div className="text-sm text-red-700">
            Error loading parties: {error instanceof Error ? error.message : 'Unknown error'}
          </div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Music Parties</h1>
            <p className="text-gray-600">Join or create collaborative music parties</p>
          </div>
          <Link to="/parties/create">
            <Button className="flex items-center space-x-2">
              <Plus className="h-4 w-4" />
              <span>Create Party</span>
            </Button>
          </Link>
        </div>

        {Array.isArray(parties) && parties.length > 0 ? (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {parties.map((party: any) => (
              <div key={party.id} className="bg-white rounded-lg shadow-md overflow-hidden">
                <div className="p-4">
                  <div className="flex items-center space-x-3 mb-3">
                    <div className="flex-shrink-0 h-8 w-8 bg-blue-100 rounded-full flex items-center justify-center">
                      <Music className="h-4 w-4 text-blue-600" />
                    </div>
                    <div>
                      <h3 className="text-base font-medium text-gray-900">
                        {party.name}
                      </h3>
                      <p className="text-xs text-gray-500">
                        by {party.hostUser?.username}
                      </p>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <div className="flex items-center text-xs text-gray-500">
                      <Users className="h-3 w-3 mr-1" />
                      <span>{party.participants?.length || 0} participants</span>
                    </div>
                    
                    <div className="flex items-center text-xs text-gray-500">
                      <Calendar className="h-3 w-3 mr-1" />
                      <span>
                        Created {new Date(party.createdAt).toLocaleDateString()}
                      </span>
                    </div>

                    <div className="flex items-center justify-between">
                      <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
                        party.status === 'Active' 
                          ? 'bg-green-100 text-green-800' 
                          : 'bg-gray-100 text-gray-800'
                      }`}>
                        {party.status}
                      </span>
                      
                      <div className="space-x-2">
                        <Link to={`/parties/${party.id}`}>
                          <Button variant="outline" size="sm">
                            View
                          </Button>
                        </Link>
                        {party.status === 'Active' && !isUserInParty(party) && !isUserHost(party) && (
                          <Button
                            size="sm"
                            onClick={() => handleJoinParty(party.id)}
                            disabled={joinPartyMutation.isPending}
                            className="cursor-pointer"
                          >
                            {joinPartyMutation.isPending ? 'Joining...' : 'Join'}
                          </Button>
                        )}
                        {party.status === 'Active' && isUserInParty(party) && !isUserHost(party) && (
                          <Button
                            variant="outline"
                            size="sm"
                            disabled
                            className="cursor-default"
                          >
                            Joined
                          </Button>
                        )}
                        {isUserHost(party) && (
                          <Button
                            variant="outline"
                            size="sm"
                            disabled
                            className="cursor-default text-blue-600 border-blue-600"
                          >
                            Host
                          </Button>
                        )}
                      </div>
                    </div>
                  </div>

                  {party.playlist?.songs && party.playlist.songs.length > 0 && (
                    <div className="mt-4 pt-4 border-t border-gray-200">
                      <p className="text-sm text-gray-600 mb-2">Recent songs:</p>
                      <div className="space-y-1">
                        {party.playlist.songs.slice(0, 2).map((song: any) => (
                          <div key={song.id} className="text-xs text-gray-500">
                            {song.title} - {song.artist}
                          </div>
                        ))}
                        {party.playlist.songs.length > 2 && (
                          <div className="text-xs text-gray-400">
                            +{party.playlist.songs.length - 2} more songs
                          </div>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-12">
            <Music className="mx-auto h-12 w-12 text-gray-400" />
            <h3 className="mt-2 text-sm font-medium text-gray-900">No parties available</h3>
            <p className="mt-1 text-sm text-gray-500">
              Get started by creating your first music party.
            </p>
            <div className="mt-6">
              <Link to="/parties/create">
                <Button className="flex items-center space-x-2">
                  <Plus className="h-4 w-4" />
                  <span>Create Party</span>
                </Button>
              </Link>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
};