import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiService } from '../services/api';
import { Layout } from '../components/Layout';
import { Button } from '../components/ui/button';
import { Plus, Music, Users, Calendar, MoreVertical } from 'lucide-react';

export const SessionsPage: React.FC = () => {
  const [selectedSession, setSelectedSession] = useState<number | null>(null);
  const queryClient = useQueryClient();

  const { data: sessions, isLoading, error } = useQuery({
    queryKey: ['sessions'],
    queryFn: () => apiService.getAllSessions(),
  });

  const joinSessionMutation = useMutation({
    mutationFn: (sessionId: number) => apiService.joinSession(sessionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sessions'] });
    },
  });

  const handleJoinSession = async (sessionId: number) => {
    try {
      await joinSessionMutation.mutateAsync(sessionId);
    } catch (error) {
      console.error('Failed to join session:', error);
    }
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
            Error loading sessions: {error instanceof Error ? error.message : 'Unknown error'}
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
            <h1 className="text-2xl font-bold text-gray-900">Music Sessions</h1>
            <p className="text-gray-600">Join or create collaborative music sessions</p>
          </div>
          <Link to="/sessions/create">
            <Button className="flex items-center space-x-2">
              <Plus className="h-4 w-4" />
              <span>Create Session</span>
            </Button>
          </Link>
        </div>

        {Array.isArray(sessions) && sessions.length > 0 ? (
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            {sessions.map((session: any) => (
              <div key={session.id} className="bg-white rounded-lg shadow-md overflow-hidden">
                <div className="p-6">
                  <div className="flex items-center justify-between mb-4">
                    <div className="flex items-center space-x-3">
                      <div className="flex-shrink-0 h-10 w-10 bg-blue-100 rounded-full flex items-center justify-center">
                        <Music className="h-5 w-5 text-blue-600" />
                      </div>
                      <div>
                        <h3 className="text-lg font-medium text-gray-900">
                          {session.name}
                        </h3>
                        <p className="text-sm text-gray-500">
                          by {session.hostUser?.username}
                        </p>
                      </div>
                    </div>
                    <div className="relative">
                      <button
                        onClick={() => setSelectedSession(selectedSession === session.id ? null : session.id)}
                        className="p-1 rounded-full hover:bg-gray-100"
                      >
                        <MoreVertical className="h-4 w-4 text-gray-400" />
                      </button>
                      {selectedSession === session.id && (
                        <div className="absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg z-10 border">
                          <div className="py-1">
                            <Link
                              to={`/sessions/${session.id}`}
                              className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                              onClick={() => setSelectedSession(null)}
                            >
                              View Details
                            </Link>
                            <button
                              onClick={() => {
                                handleJoinSession(session.id);
                                setSelectedSession(null);
                              }}
                              className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                            >
                              Join Session
                            </button>
                          </div>
                        </div>
                      )}
                    </div>
                  </div>

                  <div className="space-y-3">
                    <div className="flex items-center text-sm text-gray-500">
                      <Users className="h-4 w-4 mr-2" />
                      <span>{session.participants?.length || 0} participants</span>
                    </div>
                    
                    <div className="flex items-center text-sm text-gray-500">
                      <Calendar className="h-4 w-4 mr-2" />
                      <span>
                        Created {new Date(session.createdAt).toLocaleDateString()}
                      </span>
                    </div>

                    <div className="flex items-center justify-between">
                      <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
                        session.status === 'Active' 
                          ? 'bg-green-100 text-green-800' 
                          : 'bg-gray-100 text-gray-800'
                      }`}>
                        {session.status}
                      </span>
                      
                      <div className="space-x-2">
                        <Link to={`/sessions/${session.id}`}>
                          <Button variant="outline" size="sm">
                            View
                          </Button>
                        </Link>
                        {session.status === 'Active' && (
                          <Button
                            size="sm"
                            onClick={() => handleJoinSession(session.id)}
                            disabled={joinSessionMutation.isPending}
                          >
                            {joinSessionMutation.isPending ? 'Joining...' : 'Join'}
                          </Button>
                        )}
                      </div>
                    </div>
                  </div>

                  {session.playlist?.songs && session.playlist.songs.length > 0 && (
                    <div className="mt-4 pt-4 border-t border-gray-200">
                      <p className="text-sm text-gray-600 mb-2">Recent songs:</p>
                      <div className="space-y-1">
                        {session.playlist.songs.slice(0, 2).map((song: any) => (
                          <div key={song.id} className="text-xs text-gray-500">
                            {song.title} - {song.artist}
                          </div>
                        ))}
                        {session.playlist.songs.length > 2 && (
                          <div className="text-xs text-gray-400">
                            +{session.playlist.songs.length - 2} more songs
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
            <h3 className="mt-2 text-sm font-medium text-gray-900">No sessions available</h3>
            <p className="mt-1 text-sm text-gray-500">
              Get started by creating your first music session.
            </p>
            <div className="mt-6">
              <Link to="/sessions/create">
                <Button className="flex items-center space-x-2">
                  <Plus className="h-4 w-4" />
                  <span>Create Session</span>
                </Button>
              </Link>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
};