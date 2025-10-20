import React, { useEffect } from "react";
import { Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { partyService } from "../services/party.service";
import { useGlobalPartyEvents } from "../hooks/useGlobalPartyEvents";
import { Button } from "../components/ui/button";
import { useAuth } from "../contexts/AuthContext";
import { Plus, Music, Users, Calendar } from "lucide-react";

export const PartiesPage: React.FC = () => {
  const queryClient = useQueryClient();
  const { user } = useAuth();

  useGlobalPartyEvents("Parties");

  const {
    data: parties,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["parties"],
    queryFn: () => partyService.getAll(),
  });

  const joinPartyMutation = useMutation({
    mutationFn: (partyId: number) => partyService.join(partyId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["parties"] });
      queryClient.invalidateQueries({ queryKey: ["my-active-party"] });
    },
  });

  const handleJoinParty = async (partyId: number) => {
    try {
      await joinPartyMutation.mutateAsync(partyId);
    } catch (error) {
      // Show user-friendly error message for common constraint violations
      const errorMessage =
        error instanceof Error ? error.message : "Failed to join party";

      if (
        errorMessage.includes("already participating") ||
        errorMessage.includes("already hosting")
      ) {
        alert(
          "You can only be in one active party at a time. Please leave your current party first."
        );
      } else {
        console.error("Failed to join party:", error);
      }
    }
  };

  const isUserInParty = (party: any) => {
    if (!user || !party.participants) return false;
    return party.participants.some(
      (participant: any) => participant.userName === user.username
    );
  };

  const isUserHost = (party: any) => {
    if (!user || !party.hostUser) return false;
    return party.hostUser.username === user.username;
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="border-blue-600 border-b-2 rounded-full w-32 h-32 animate-spin"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-50 p-4 rounded-md">
        <div className="text-red-700 text-sm">
          Error loading parties:{" "}
          {error instanceof Error ? error.message : "Unknown error"}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="font-bold text-gray-900 text-2xl">Music Parties</h1>
          <p className="text-gray-600">
            Join or create collaborative music parties
          </p>
        </div>
        <Link to="/parties/create">
          <Button className="flex items-center space-x-2">
            <Plus className="w-4 h-4" />
            <span>Create Party</span>
          </Button>
        </Link>
      </div>

      {Array.isArray(parties) && parties.length > 0 ? (
        <div className="gap-4 grid md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {parties.map((party: any) => (
            <div
              key={party.id}
              className="bg-white shadow-md rounded-lg overflow-hidden"
            >
              <div className="p-4">
                <div className="flex items-center space-x-3 mb-3">
                  <div className="flex flex-shrink-0 justify-center items-center bg-blue-100 rounded-full w-8 h-8">
                    <Music className="w-4 h-4 text-blue-600" />
                  </div>
                  <div>
                    <h3 className="font-medium text-gray-900 text-base">
                      {party.name}
                    </h3>
                    <p className="text-gray-500 text-xs">
                      by {party.hostUser?.username}
                    </p>
                  </div>
                </div>

                <div className="space-y-2">
                  <div className="flex items-center text-gray-500 text-xs">
                    <Users className="mr-1 w-3 h-3" />
                    <span>{party.participants?.length || 0} participants</span>
                  </div>

                  <div className="flex items-center text-gray-500 text-xs">
                    <Calendar className="mr-1 w-3 h-3" />
                    <span>
                      Created {new Date(party.createdAt).toLocaleDateString()}
                    </span>
                  </div>

                  <div className="flex justify-between items-center">
                    <span
                      className={`px-2 py-1 text-xs font-semibold rounded-full ${
                        party.status === "Active"
                          ? "bg-green-100 text-green-800"
                          : "bg-gray-100 text-gray-800"
                      }`}
                    >
                      {party.status}
                    </span>

                    <div className="space-x-2">
                      <Link to={`/parties/${party.id}`}>
                        <Button variant="outline" size="sm">
                          View
                        </Button>
                      </Link>
                      {party.status === "Active" &&
                        !isUserInParty(party) &&
                        !isUserHost(party) && (
                          <Button
                            size="sm"
                            onClick={() => handleJoinParty(party.id)}
                            disabled={joinPartyMutation.isPending}
                            className="cursor-pointer"
                          >
                            {joinPartyMutation.isPending
                              ? "Joining..."
                              : "Join"}
                          </Button>
                        )}
                      {party.status === "Active" &&
                        isUserInParty(party) &&
                        !isUserHost(party) && (
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
                          className="border-blue-600 text-blue-600 cursor-default"
                        >
                          Host
                        </Button>
                      )}
                    </div>
                  </div>
                </div>

                {party.playlist?.songs && party.playlist.songs.length > 0 && (
                  <div className="mt-4 pt-4 border-gray-200 border-t">
                    <p className="mb-2 text-gray-600 text-sm">Recent songs:</p>
                    <div className="space-y-1">
                      {party.playlist.songs.slice(0, 2).map((song: any) => (
                        <div key={song.id} className="text-gray-500 text-xs">
                          {song.title} - {song.artist}
                        </div>
                      ))}
                      {party.playlist.songs.length > 2 && (
                        <div className="text-gray-400 text-xs">
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
        <div className="py-12 text-center">
          <Music className="mx-auto w-12 h-12 text-gray-400" />
          <h3 className="mt-2 font-medium text-gray-900 text-sm">
            No parties available
          </h3>
          <p className="mt-1 text-gray-500 text-sm">
            Get started by creating your first music party.
          </p>
          <div className="mt-6">
            <Link to="/parties/create">
              <Button className="flex items-center space-x-2">
                <Plus className="w-4 h-4" />
                <span>Create Party</span>
              </Button>
            </Link>
          </div>
        </div>
      )}
    </div>
  );
};
