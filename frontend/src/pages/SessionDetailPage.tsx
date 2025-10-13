import React, { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { sessionService } from "../services/session.service";
import { useAuth } from "../contexts/AuthContext";
import { Button } from "../components/ui/button";
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
import { toast } from "sonner";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
} from "@/components/ui/dialog";
export const SessionDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const [showStopConfirm, setShowStopConfirm] = useState(false);

  const {
    data: session,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["session", id],
    queryFn: () => sessionService.getById(Number(id)),
    enabled: !!id,
  });

  const sessionData = session as any;

  const joinSessionMutation = useMutation({
    mutationFn: () => sessionService.join(Number(id)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["session", id] });
      toast.success("Successfully joined the session!");
    },
    onError: () => {
      toast.error("Failed to join session. Please try again.");
    },
  });

  const leaveSessionMutation = useMutation({
    mutationFn: () => sessionService.leave(Number(id)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["session", id] });
      toast.success("Successfully left the session.");
    },
    onError: () => {
      toast.error("Failed to leave session. Please try again.");
    },
  });

  const removeSongMutation = useMutation({
    mutationFn: (songId: number) => sessionService.removeSong(songId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["session", id] });
      toast.success("Song removed from playlist.");
    },
    onError: () => {
      toast.error("Failed to remove song. Please try again.");
    },
  });

  const deleteSessionMutation = useMutation({
    mutationFn: () => sessionService.delete(Number(id)),
    onSuccess: () => {
      toast.success("Session stopped successfully.");
      navigate("/sessions");
    },
    onError: () => {
      toast.error("Failed to stop session. Please try again.");
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
    navigate("/songs");
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="border-b-2 border-blue-600 rounded-full w-32 h-32 animate-spin"></div>
      </div>
    );
  }

  if (error || !sessionData) {
    return (
      <div className="bg-red-50 p-4 rounded-md">
        <div className="text-red-700 text-sm">
          Error loading session:{" "}
          {error instanceof Error ? error.message : "Session not found"}
        </div>
      </div>
    );
  }

  const isHost = sessionData?.hostUser?.id === user?.id;
  const isParticipant =
    isHost ||
    sessionData?.participants?.some((p: any) => p.userName === user?.username);

  return (
    <>
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <div className="flex items-center space-x-4">
            <button
              onClick={() => navigate("/sessions")}
              className="flex items-center text-gray-500 hover:text-gray-700 text-sm"
            >
              <ArrowLeft className="mr-1 w-4 h-4" />
              Back to Sessions
            </button>
          </div>
        </div>

        <div className="bg-white shadow rounded-lg overflow-hidden">
          <div className="px-6 py-4 border-gray-200 border-b">
            <div className="flex justify-between items-center">
              <div>
                <h1 className="font-bold text-gray-900 text-2xl">
                  {sessionData.name}
                </h1>
                <p className="text-gray-600">
                  Hosted by {sessionData.hostUser?.username}
                  {isHost && <span className="ml-2 text-blue-600">(You)</span>}
                </p>
              </div>
              <div className="flex items-center space-x-3">
                <span
                  className={`px-3 py-1 text-sm font-semibold rounded-full ${
                    sessionData.status === "Active"
                      ? "bg-green-100 text-green-800"
                      : "bg-gray-100 text-gray-800"
                  }`}
                >
                  {sessionData.status}
                </span>
                {sessionData.status === "Active" && !isParticipant && (
                  <Button
                    onClick={handleJoinSession}
                    disabled={joinSessionMutation.isPending}
                    className="flex items-center space-x-2"
                  >
                    <UserPlus className="w-4 h-4" />
                    <span>
                      {joinSessionMutation.isPending
                        ? "Joining..."
                        : "Join Session"}
                    </span>
                  </Button>
                )}
                {isParticipant && !isHost && (
                  <Button
                    variant="outline"
                    onClick={handleLeaveSession}
                    disabled={leaveSessionMutation.isPending}
                    className="flex items-center space-x-2"
                  >
                    <UserMinus className="w-4 h-4" />
                    <span>
                      {leaveSessionMutation.isPending
                        ? "Leaving..."
                        : "Leave Session"}
                    </span>
                  </Button>
                )}
                {isHost && sessionData.status === "Active" && (
                  <Button
                    variant="destructive"
                    onClick={handleStopSession}
                    disabled={deleteSessionMutation.isPending}
                    className="flex items-center space-x-2"
                  >
                    <Trash2 className="w-4 h-4" />
                    <span>
                      {deleteSessionMutation.isPending
                        ? "Stopping..."
                        : "Stop Session"}
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
                  {sessionData.playlist?.songs &&
                  sessionData.playlist.songs.length > 0 ? (
                    sessionData.playlist.songs.map(
                      (song: any, index: number) => (
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
                            <Button size="sm" variant="outline">
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
                      )
                    )
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
                  Participants ({sessionData.participants?.length || 0})
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
                        {sessionData.hostUser?.username}
                        {isHost && (
                          <span className="ml-1 text-blue-600">(You)</span>
                        )}
                      </p>
                      <p className="text-gray-500 text-xs">Host</p>
                    </div>
                  </div>
                  {sessionData.participants
                    ?.filter(
                      (participant: any) =>
                        participant.userName !== sessionData.hostUser?.username
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
      <Dialog open={showStopConfirm} onOpenChange={setShowStopConfirm}>
        <DialogContent>
          <DialogHeader>Stop Session</DialogHeader>
          <DialogDescription>
            Are you sure you want to stop this session? This action cannot be
            undone.
          </DialogDescription>
          <DialogFooter>
            <Button
              variant="secondary"
              onMouseDown={() => setShowStopConfirm(false)}
            >
              Cancel
            </Button>
            <Button onMouseDown={confirmStopSession}>Stop Session</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
};
