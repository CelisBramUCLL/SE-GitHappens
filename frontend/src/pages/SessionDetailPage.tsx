import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { sessionService } from '../services/session.service';
import { useAuth } from '../contexts/AuthContext';
import { useToast } from '../contexts/ToastContext';
import { Layout } from '../components/Layout';
import { Button } from '../components/ui/button';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { 
  ArrowLeft, 
  Users, 
  Music, 
  Plus, 
  Trash2, 
  Play, 
  Clock,
  UserPlus,
  UserMinus 
} from 'lucide-react';

export const SessionDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const toast = useToast();
  const [showStopConfirm, setShowStopConfirm] = useState(false);

  const { data: session, isLoading, error } = useQuery({
    queryKey: ['session', id],
    queryFn: () => sessionService.getById(Number(id)),
    enabled: !!id,
  });

  const sessionData = session as any;

  const joinSessionMutation = useMutation({
    mutationFn: () => sessionService.join(Number(id)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['session', id] });
      toast.success('Successfully joined the session!');
    },
    onError: () => {
      toast.error('Failed to join session. Please try again.');
    },
  });

  const leaveSessionMutation = useMutation({
    mutationFn: () => sessionService.leave(Number(id)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['session', id] });
      toast.success('Successfully left the session.');
    },
    onError: () => {
      toast.error('Failed to leave session. Please try again.');
    },
  });

  const removeSongMutation = useMutation({
    mutationFn: (songId: number) => sessionService.removeSong(songId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['session', id] });
      toast.success('Song removed from playlist.');
    },
    onError: () => {
      toast.error('Failed to remove song. Please try again.');
    },
  });

  const deleteSessionMutation = useMutation({
    mutationFn: () => sessionService.delete(Number(id)),
    onSuccess: () => {
      toast.success('Session stopped successfully.');
      navigate('/sessions');
    },
    onError: () => {
      toast.error('Failed to stop session. Please try again.');
    },
  });

  const handleJoinSession = () => {
    joinSessionMutation.mutate();
  };

  const handleLeaveSession = () => {
    leaveSessionMutation.mutate();
  };

  const handleRemoveSong = (songId: number) => {
    removeSongMutation.mutate(songId);
  };

  const handleStopSession = () => {
    setShowStopConfirm(true);
  };

  const confirmStopSession = () => {
    deleteSessionMutation.mutate();
  };

  const handleAddSongsNavigation = () => {
    navigate('/songs');
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

  if (error || !sessionData) {
    return (
      <Layout>
        <div className="rounded-md bg-red-50 p-4">
          <div className="text-sm text-red-700">
            Error loading session: {error instanceof Error ? error.message : 'Session not found'}
          </div>
        </div>
      </Layout>
    );
  }

  const isHost = sessionData?.hostUser?.id === user?.id;
  const isParticipant = isHost || sessionData?.participants?.some((p: any) => p.userName === user?.username);

  return (
    <Layout>
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-4">
            <button
              onClick={() => navigate('/sessions')}
              className="flex items-center text-sm text-gray-500 hover:text-gray-700"
            >
              <ArrowLeft className="h-4 w-4 mr-1" />
              Back to Sessions
            </button>
          </div>
        </div>

        <div className="bg-white shadow rounded-lg overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-2xl font-bold text-gray-900">{sessionData.name}</h1>
                <p className="text-gray-600">
                  Hosted by {sessionData.hostUser?.username}
                  {isHost && <span className="ml-2 text-blue-600">(You)</span>}
                </p>
              </div>
              <div className="flex items-center space-x-3">
                <span className={`px-3 py-1 text-sm font-semibold rounded-full ${
                  sessionData.status === 'Active' 
                    ? 'bg-green-100 text-green-800' 
                    : 'bg-gray-100 text-gray-800'
                }`}>
                  {sessionData.status}
                </span>
                {sessionData.status === 'Active' && !isParticipant && (
                  <Button
                    onClick={handleJoinSession}
                    disabled={joinSessionMutation.isPending}
                    className="flex items-center space-x-2"
                  >
                    <UserPlus className="h-4 w-4" />
                    <span>{joinSessionMutation.isPending ? 'Joining...' : 'Join Session'}</span>
                  </Button>
                )}
                {isParticipant && !isHost && (
                  <Button
                    variant="outline"
                    onClick={handleLeaveSession}
                    disabled={leaveSessionMutation.isPending}
                    className="flex items-center space-x-2"
                  >
                    <UserMinus className="h-4 w-4" />
                    <span>{leaveSessionMutation.isPending ? 'Leaving...' : 'Leave Session'}</span>
                  </Button>
                )}
                {isHost && sessionData.status === 'Active' && (
                  <Button
                    variant="destructive"
                    onClick={handleStopSession}
                    disabled={deleteSessionMutation.isPending}
                    className="flex items-center space-x-2"
                  >
                    <Trash2 className="h-4 w-4" />
                    <span>{deleteSessionMutation.isPending ? 'Stopping...' : 'Stop Session'}</span>
                  </Button>
                )}
              </div>
            </div>
          </div>

          <div className="px-6 py-4">
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
              <div className="lg:col-span-2">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-lg font-medium text-gray-900 flex items-center">
                    <Music className="h-5 w-5 mr-2" />
                    Playlist
                  </h3>
                  {(isHost || isParticipant) && (
                    <Button
                      onClick={handleAddSongsNavigation}
                      size="sm"
                      className="flex items-center space-x-2"
                    >
                      <Plus className="h-4 w-4" />
                      <span>Browse & Add Songs</span>
                    </Button>
                  )}
                </div>

                <div className="space-y-3">
                  {sessionData.playlist?.songs && sessionData.playlist.songs.length > 0 ? (
                    sessionData.playlist.songs.map((song: any, index: number) => (
                      <div key={song.id} className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                        <div className="flex items-center space-x-3">
                          <div className="flex-shrink-0 w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center">
                            <span className="text-sm font-medium text-blue-600">{index + 1}</span>
                          </div>
                          <div>
                            <h4 className="text-sm font-medium text-gray-900">{song.title}</h4>
                            <p className="text-sm text-gray-500">{song.artist} • {song.album}</p>
                          </div>
                        </div>
                        <div className="flex items-center space-x-2">
                          <Button size="sm" variant="outline">
                            <Play className="h-4 w-4" />
                          </Button>
                          {(isHost || isParticipant) && (
                            <Button
                              size="sm"
                              variant="outline"
                              onClick={() => handleRemoveSong(song.id)}
                              disabled={removeSongMutation.isPending}
                            >
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          )}
                        </div>
                      </div>
                    ))
                  ) : (
                    <div className="text-center py-8 text-gray-500">
                      <Music className="mx-auto h-12 w-12 text-gray-400" />
                      <h3 className="mt-2 text-sm font-medium text-gray-900">No songs yet</h3>
                      <p className="mt-1 text-sm text-gray-500">
                        Add the first song to get the party started!
                      </p>
                    </div>
                  )}
                </div>
              </div>

              <div>
                <h3 className="text-lg font-medium text-gray-900 flex items-center mb-4">
                  <Users className="h-5 w-5 mr-2" />
                  Participants ({sessionData.participants?.length || 0})
                </h3>
                <div className="space-y-3">
                  <div className="flex items-center space-x-3 p-3 bg-blue-50 rounded-lg">
                    <div className="flex-shrink-0 w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center">
                      <span className="text-sm font-medium text-blue-600">H</span>
                    </div>
                    <div>
                      <p className="text-sm font-medium text-gray-900">
                        {sessionData.hostUser?.username}
                        {isHost && <span className="ml-1 text-blue-600">(You)</span>}
                      </p>
                      <p className="text-xs text-gray-500">Host</p>
                    </div>
                  </div>
                  {sessionData.participants?.filter((participant: any) => 
                    participant.userName !== sessionData.hostUser?.username
                  ).map((participant: any) => (
                    <div key={participant.id} className="flex items-center space-x-3 p-3 bg-gray-50 rounded-lg">
                      <div className="flex-shrink-0 w-8 h-8 bg-gray-100 rounded-full flex items-center justify-center">
                        <span className="text-sm font-medium text-gray-600">
                          {participant.userName.charAt(0).toUpperCase()}
                        </span>
                      </div>
                      <div>
                        <p className="text-sm font-medium text-gray-900">
                          {participant.userName}
                          {participant.userName === user?.username && <span className="ml-1 text-blue-600">(You)</span>}
                        </p>
                        <div className="flex items-center text-xs text-gray-500">
                          <Clock className="h-3 w-3 mr-1" />
                          Joined {new Date(participant.joinedAt).toLocaleTimeString()}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <ConfirmDialog
        isOpen={showStopConfirm}
        onClose={() => setShowStopConfirm(false)}
        onConfirm={confirmStopSession}
        title="Stop Session"
        message="Are you sure you want to stop this session? This action cannot be undone."
        confirmText="Stop Session"
        cancelText="Cancel"
        type="danger"
      />
    </Layout>
  );
};