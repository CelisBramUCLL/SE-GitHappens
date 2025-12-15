import React, { useState, useEffect, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { partyService } from "../services/party.service";
import { useAuth } from "../contexts/AuthContext";
import { useToast } from "../contexts/ToastContext";
import { useMusicPlayer } from "../contexts/MusicPlayerContext";
import { useSignalR } from "../hooks/useSignalR";
import { signalRService } from "../services/signalRService";
import { Button } from "../components/ui/button";
import { ConfirmDialog } from "../components/ConfirmDialog";
import {
  ArrowLeft,
  Users,
  Music,
  Plus,
  Trash2,
  Play,
  Clock,
  UserPlus,
  UserMinus,
} from "lucide-react";

export const PartyDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const toast = useToast();
  const [showStopConfirm, setShowStopConfirm] = useState(false);

  const { isConnected } = useSignalR();
  const [listenersSetUp, setListenersSetUp] = useState<string | null>(null);

  const musicPlayer = useMusicPlayer();
  const partyDataRef = useRef<any>(null);

  const {
    data: party,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["party", id],
    queryFn: () => partyService.getById(Number(id)),
    enabled: !!id,
  });

  const partyData = party as any;

  // Update ref whenever partyData changes
  useEffect(() => {
    partyDataRef.current = partyData;
  }, [partyData]);

  const isHost = partyData?.hostUser?.id === user?.id;
  const isParticipant =
    isHost ||
    partyData?.participants?.some((p: any) => p.userName === user?.username);

  useEffect(() => {
    if (partyData?.playlist?.songs) {
      musicPlayer.setPlaylist(partyData.playlist.songs);
    }
  }, [partyData?.playlist?.songs, musicPlayer]);

  useEffect(() => {
    if (id && isParticipant) {
      musicPlayer.setCurrentPartyId(Number(id));
    }
  }, [id, isParticipant, musicPlayer]);

  useEffect(() => {
    if (!isConnected || !id) return;

    if (listenersSetUp === id && signalRService.isConnected) return;

    const handleUserJoined = (_userId: number, partyId: number) => {
      if (partyId === Number(id)) {
        queryClient.invalidateQueries({ queryKey: ["party", id] });
      }
    };

    const handleUserLeft = (_userId: number, partyId: number) => {
      if (partyId === Number(id)) {
        queryClient.invalidateQueries({ queryKey: ["party", id] });
      }
    };

    const handleSongAdded = (_songId: number, _connectionId: string) => {
      queryClient.invalidateQueries({ queryKey: ["party", id] });
    };

    const handleSongRemoved = (_songId: number, _connectionId: string) => {
      queryClient.invalidateQueries({ queryKey: ["party", id] });
    };

    const handlePartyDeleted = (partyId: number, hostUserId: number) => {
      if (partyId === Number(id)) {
        signalRService.leaveParty(partyId);
        musicPlayer.stop();
        musicPlayer.setCurrentPartyId(null);
        
        if (user?.id !== hostUserId) {
          toast.info("Party has been deleted by the host");
        }
        navigate("/dashboard");
      }
    };

    signalRService.onUserJoinedParty(handleUserJoined);
    signalRService.onUserLeftParty(handleUserLeft);
    signalRService.onSongAdded(handleSongAdded);
    signalRService.onSongRemoved(handleSongRemoved);
    signalRService.onPartyDeleted(handlePartyDeleted);

    const joinWithRetry = async () => {
      let attempts = 0;
      const maxAttempts = 5;

      while (attempts < maxAttempts) {
        try {
          if (signalRService.isConnected) {
            await signalRService.joinParty(Number(id));
            break;
            } else {
            await new Promise((resolve) => setTimeout(resolve, 200));
          }
        } catch (error) {
          await new Promise((resolve) => setTimeout(resolve, 200));
        }
        attempts++;
      }
    };

    joinWithRetry();

    setListenersSetUp(id);
    return () => {
      signalRService.off("UserJoinedParty");
      signalRService.off("UserLeftParty");
      signalRService.off("SongAdded");
      signalRService.off("SongRemoved");
      signalRService.off("PartyDeleted");

      setListenersSetUp(null);
    };
    }, [isConnected, id]);

  const joinPartyMutation = useMutation({
    mutationFn: () => partyService.join(Number(id)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["party", id] });
      toast.success("Successfully joined the party!");
    },
    onError: () => {
      toast.error("Failed to join party. Please try again.");
    },
  });

  const leavePartyMutation = useMutation({
    mutationFn: () => partyService.leave(Number(id)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["party", id] });
      if (id) {
        signalRService.leaveParty(Number(id));
      }
      musicPlayer.stop();
      musicPlayer.setCurrentPartyId(null);
      toast.success("Successfully left the party.");
    },
    onError: () => {
      toast.error("Failed to leave party. Please try again.");
    },
  });

  const removeSongMutation = useMutation({
    mutationFn: (songId: number) => partyService.removeSong(songId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["party", id] });
      toast.success("Song removed from playlist.");
    },
    onError: () => {
      toast.error("Failed to remove song. Please try again.");
    },
  });

  const deletePartyMutation = useMutation({
    mutationFn: () => partyService.delete(Number(id)),
    onSuccess: () => {
      if (id) {
        signalRService.leaveParty(Number(id));
      }
      musicPlayer.stop();
      musicPlayer.setCurrentPartyId(null);
      toast.success("Party stopped successfully.");
      navigate("/parties");
    },
    onError: () => {
      toast.error("Failed to stop party. Please try again.");
    },
  });

  const handleJoinParty = () => {
    joinPartyMutation.mutate();
  };

  const handleLeaveParty = () => {
    leavePartyMutation.mutate();
  };

  const handleRemoveSong = (songId: number) => {
    removeSongMutation.mutate(songId);
  };

  const handleStopParty = () => {
    setShowStopConfirm(true);
  };

  const confirmStopParty = () => {
    deletePartyMutation.mutate();
  };

  const handleAddSongsNavigation = () => {
    navigate("/songs");
  };

  const handlePlaySongFromList = (song: any) => {
    musicPlayer.setCurrentSong(song);
    musicPlayer.setIsPlaying(true);
    musicPlayer.setCurrentPosition(0);
    signalRService.playSong(Number(id), song.id, 0);
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="border-blue-600 border-b-2 rounded-full w-32 h-32 animate-spin"></div>
      </div>
    );
  }

  if (error || !partyData) {
    return (
      <div className="bg-red-50 p-4 rounded-md">
        <div className="text-red-700 text-sm">
          Error loading party:{" "}
          {error instanceof Error ? error.message : "Party not found"}
        </div>
      </div>
    );
  }

  return (
    <>
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <div className="flex items-center space-x-4">
            <button
              onClick={() => navigate("/parties")}
              className="flex items-center text-gray-500 hover:text-gray-700 text-sm"
            >
              <ArrowLeft className="mr-1 w-4 h-4" />
              Back to Parties
            </button>
          </div>
        </div>

        <div className="bg-white shadow rounded-lg overflow-hidden">
          <div className="px-6 py-4 border-gray-200 border-b">
            <div className="flex justify-between items-center">
              <div>
                <h1 className="font-bold text-gray-900 text-2xl">
                  {partyData.name}
                </h1>
                <p className="text-gray-600">
                  Hosted by {partyData.hostUser?.username}
                  {isHost && <span className="ml-2 text-blue-600">(You)</span>}
                </p>
              </div>
              <div className="flex items-center space-x-3">
                <span
                  className={`px-3 py-1 text-sm font-semibold rounded-full ${
                    partyData.status === "Active"
                      ? "bg-green-100 text-green-800"
                      : "bg-gray-100 text-gray-800"
                  }`}
                >
                  {partyData.status}
                </span>
                {partyData.status === "Active" && !isParticipant && (
                  <Button
                    onClick={handleJoinParty}
                    disabled={joinPartyMutation.isPending}
                    className="flex items-center space-x-2"
                  >
                    <UserPlus className="w-4 h-4" />
                    <span>
                      {joinPartyMutation.isPending
                        ? "Joining..."
                        : "Join Party"}
                    </span>
                  </Button>
                )}
                {isParticipant && !isHost && (
                  <Button
                    variant="outline"
                    onClick={handleLeaveParty}
                    disabled={leavePartyMutation.isPending}
                    className="flex items-center space-x-2"
                  >
                    <UserMinus className="w-4 h-4" />
                    <span>
                      {leavePartyMutation.isPending
                        ? "Leaving..."
                        : "Leave Party"}
                    </span>
                  </Button>
                )}
                {isHost && partyData.status === "Active" && (
                  <Button
                    variant="destructive"
                    onClick={handleStopParty}
                    disabled={deletePartyMutation.isPending}
                    className="flex items-center space-x-2"
                  >
                    <Trash2 className="w-4 h-4" />
                    <span>
                      {deletePartyMutation.isPending
                        ? "Stopping..."
                        : "Stop Party"}
                    </span>
                  </Button>
                )}
              </div>
            </div>
          </div>

          <div className="px-6 py-4">
            <div className="gap-6 grid grid-cols-1 lg:grid-cols-3">
              <div className="lg:col-span-2">
                <div className="flex justify-between items-center mb-4">
                  <h3 className="flex items-center font-medium text-gray-900 text-lg">
                    <Music className="mr-2 w-5 h-5" />
                    Playlist
                  </h3>
                  {(isHost || isParticipant) && (
                    <Button
                      onClick={handleAddSongsNavigation}
                      size="sm"
                      className="flex items-center space-x-2"
                    >
                      <Plus className="w-4 h-4" />
                      <span>Browse & Add Songs</span>
                    </Button>
                  )}
                </div>

                <div className="space-y-3">
                  {partyData.playlist?.songs &&
                  partyData.playlist.songs.length > 0 ? (
                    partyData.playlist.songs.map((song: any, index: number) => (
                      <div
                        key={song.id}
                        className="flex justify-between items-center bg-gray-50 p-4 rounded-lg"
                      >
                        <div className="flex items-center space-x-3">
                          <div className="flex flex-shrink-0 justify-center items-center bg-blue-100 rounded-full w-8 h-8">
                            <span className="font-medium text-blue-600 text-sm">
                              {index + 1}
                            </span>
                          </div>
                          <div>
                            <h4 className="font-medium text-gray-900 text-sm">
                              {song.title}
                            </h4>
                            <p className="text-gray-500 text-sm">
                              {song.artist} • {song.album}
                            </p>
                          </div>
                        </div>
                        <div className="flex items-center space-x-2">
                          <Button 
                            size="sm" 
                            variant="outline"
                            onClick={() => handlePlaySongFromList(song)}
                            disabled={!isParticipant}
                          >
                            <Play className="w-4 h-4" />
                          </Button>
                          {(isHost || isParticipant) && (
                            <Button
                              size="sm"
                              variant="outline"
                              onClick={() => handleRemoveSong(song.id)}
                              disabled={removeSongMutation.isPending}
                            >
                              <Trash2 className="w-4 h-4" />
                            </Button>
                          )}
                        </div>
                      </div>
                    ))
                  ) : (
                    <div className="py-8 text-gray-500 text-center">
                      <Music className="mx-auto w-12 h-12 text-gray-400" />
                      <h3 className="mt-2 font-medium text-gray-900 text-sm">
                        No songs yet
                      </h3>
                      <p className="mt-1 text-gray-500 text-sm">
                        Add the first song to get the party started!
                      </p>
                    </div>
                  )}
                </div>
              </div>

              <div>
                <h3 className="flex items-center mb-4 font-medium text-gray-900 text-lg">
                  <Users className="mr-2 w-5 h-5" />
                  Participants ({partyData.participants?.length || 0})
                </h3>
                <div className="space-y-3">
                  <div className="flex items-center space-x-3 bg-blue-50 p-3 rounded-lg">
                    <div className="flex flex-shrink-0 justify-center items-center bg-blue-100 rounded-full w-8 h-8">
                      <span className="font-medium text-blue-600 text-sm">
                        H
                      </span>
                    </div>
                    <div>
                      <p className="font-medium text-gray-900 text-sm">
                        {partyData.hostUser?.username}
                        {isHost && (
                          <span className="ml-1 text-blue-600">(You)</span>
                        )}
                      </p>
                      <p className="text-gray-500 text-xs">Host</p>
                    </div>
                  </div>
                  {partyData.participants
                    ?.filter(
                      (participant: any) =>
                        participant.userName !== partyData.hostUser?.username
                    )
                    .map((participant: any) => (
                      <div
                        key={participant.id}
                        className="flex items-center space-x-3 bg-gray-50 p-3 rounded-lg"
                      >
                        <div className="flex flex-shrink-0 justify-center items-center bg-gray-100 rounded-full w-8 h-8">
                          <span className="font-medium text-gray-600 text-sm">
                            {participant.userName.charAt(0).toUpperCase()}
                          </span>
                        </div>
                        <div>
                          <p className="font-medium text-gray-900 text-sm">
                            {participant.userName}
                            {participant.userName === user?.username && (
                              <span className="ml-1 text-blue-600">(You)</span>
                            )}
                          </p>
                          <div className="flex items-center text-gray-500 text-xs">
                            <Clock className="mr-1 w-3 h-3" />
                            Joined{" "}
                            {new Date(
                              participant.joinedAt
                            ).toLocaleTimeString()}
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
        onConfirm={confirmStopParty}
        title="Stop Party"
        message="Are you sure you want to stop this party? This action cannot be undone."
        confirmText="Stop Party"
      />
    </>
  );
};
