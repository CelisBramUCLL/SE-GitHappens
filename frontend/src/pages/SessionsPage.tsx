import React, { useState } from "react";
import { Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { sessionService } from "../services/session.service";
import { Button } from "../components/ui/button";
import { Plus, Music, Users, Calendar, MoreVertical } from "lucide-react";

export const SessionsPage: React.FC = () => {
  const [selectedSession, setSelectedSession] = useState<number | null>(null);
  const queryClient = useQueryClient();

  const {
    data: sessions,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["sessions"],
    queryFn: () => sessionService.getAll(),
  });

  const joinSessionMutation = useMutation({
    mutationFn: (sessionId: number) => sessionService.join(sessionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["sessions"] });
    },
  });

  const handleJoinSession = async (sessionId: number) => {
    try {
      await joinSessionMutation.mutateAsync(sessionId);
    } catch (error) {
      console.error("Failed to join session:", error);
    }
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="border-b-2 border-blue-600 rounded-full w-32 h-32 animate-spin"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-50 p-4 rounded-md">
        <div className="text-red-700 text-sm">
          Error loading sessions:{" "}
          {error instanceof Error ? error.message : "Unknown error"}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="font-bold text-gray-900 text-2xl">Music Sessions</h1>
          <p className="text-gray-600">
            Join or create collaborative music sessions
          </p>
        </div>
        <Link to="/sessions/create">
          <Button className="flex items-center space-x-2">
            <Plus className="w-4 h-4" />
            <span>Create Session</span>
          </Button>
        </Link>
      </div>

      {Array.isArray(sessions) && sessions.length > 0 ? (
        <div className="gap-6 grid md:grid-cols-2 lg:grid-cols-3">
          {sessions.map((session: any) => (
            <div
              key={session.id}
              className="bg-white shadow-md rounded-lg overflow-hidden"
            >
              <div className="p-6">
                <div className="flex justify-between items-center mb-4">
                  <div className="flex items-center space-x-3">
                    <div className="flex flex-shrink-0 justify-center items-center bg-blue-100 rounded-full w-10 h-10">
                      <Music className="w-5 h-5 text-blue-600" />
                    </div>
                    <div>
                      <h3 className="font-medium text-gray-900 text-lg">
                        {session.name}
                      </h3>
                      <p className="text-gray-500 text-sm">
                        by {session.hostUser?.username}
                      </p>
                    </div>
                  </div>
                  <div className="relative">
                    <button
                      onClick={() =>
                        setSelectedSession(
                          selectedSession === session.id ? null : session.id
                        )
                      }
                      className="hover:bg-gray-100 p-1 rounded-full"
                    >
                      <MoreVertical className="w-4 h-4 text-gray-400" />
                    </button>
                    {selectedSession === session.id && (
                      <div className="right-0 z-10 absolute bg-white shadow-lg mt-2 border rounded-md w-48">
                        <div className="py-1">
                          <Link
                            to={`/sessions/${session.id}`}
                            className="block hover:bg-gray-100 px-4 py-2 text-gray-700 text-sm"
                            onClick={() => setSelectedSession(null)}
                          >
                            View Details
                          </Link>
                          <button
                            onClick={() => {
                              handleJoinSession(session.id);
                              setSelectedSession(null);
                            }}
                            className="block hover:bg-gray-100 px-4 py-2 w-full text-gray-700 text-sm text-left"
                          >
                            Join Session
                          </button>
                        </div>
                      </div>
                    )}
                  </div>
                </div>

                <div className="space-y-3">
                  <div className="flex items-center text-gray-500 text-sm">
                    <Users className="mr-2 w-4 h-4" />
                    <span>
                      {session.participants?.length || 0} participants
                    </span>
                  </div>

                  <div className="flex items-center text-gray-500 text-sm">
                    <Calendar className="mr-2 w-4 h-4" />
                    <span>
                      Created {new Date(session.createdAt).toLocaleDateString()}
                    </span>
                  </div>

                  <div className="flex justify-between items-center">
                    <span
                      className={`px-2 py-1 text-xs font-semibold rounded-full ${
                        session.status === "Active"
                          ? "bg-green-100 text-green-800"
                          : "bg-gray-100 text-gray-800"
                      }`}
                    >
                      {session.status}
                    </span>

                    <div className="space-x-2">
                      <Link to={`/sessions/${session.id}`}>
                        <Button variant="outline" size="sm">
                          View
                        </Button>
                      </Link>
                      {session.status === "Active" && (
                        <Button
                          size="sm"
                          onClick={() => handleJoinSession(session.id)}
                          disabled={joinSessionMutation.isPending}
                        >
                          {joinSessionMutation.isPending
                            ? "Joining..."
                            : "Join"}
                        </Button>
                      )}
                    </div>
                  </div>
                </div>

                {session.playlist?.songs &&
                  session.playlist.songs.length > 0 && (
                    <div className="mt-4 pt-4 border-gray-200 border-t">
                      <p className="mb-2 text-gray-600 text-sm">
                        Recent songs:
                      </p>
                      <div className="space-y-1">
                        {session.playlist.songs.slice(0, 2).map((song: any) => (
                          <div key={song.id} className="text-gray-500 text-xs">
                            {song.title} - {song.artist}
                          </div>
                        ))}
                        {session.playlist.songs.length > 2 && (
                          <div className="text-gray-400 text-xs">
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
        <div className="py-12 text-center">
          <Music className="mx-auto w-12 h-12 text-gray-400" />
          <h3 className="mt-2 font-medium text-gray-900 text-sm">
            No sessions available
          </h3>
          <p className="mt-1 text-gray-500 text-sm">
            Get started by creating your first music session.
          </p>
          <div className="mt-6">
            <Link to="/sessions/create">
              <Button className="flex items-center space-x-2">
                <Plus className="w-4 h-4" />
                <span>Create Session</span>
              </Button>
            </Link>
          </div>
        </div>
      )}
    </div>
  );
};
