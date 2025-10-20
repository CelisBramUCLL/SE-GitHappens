import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useMutation, useQueryClient, useQuery } from "@tanstack/react-query";
import { partyService } from "../services/party.service";
import { Button } from "../components/ui/button";
import { ArrowLeft, AlertCircle } from "lucide-react";

export const CreatePartyPage: React.FC = () => {
  const [partyName, setPartyName] = useState("");
  const [error, setError] = useState("");

  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const { data: activePartyData, isLoading: isCheckingActiveParty } = useQuery({
    queryKey: ["my-active-party"],
    queryFn: () => partyService.getMyActiveParty(),
  });

  const createPartyMutation = useMutation({
    mutationFn: (data: { name: string }) => partyService.create(data),
    onSuccess: (party: any) => {
      queryClient.invalidateQueries({ queryKey: ["parties"] });
      queryClient.invalidateQueries({ queryKey: ["my-active-party"] });
      navigate(`/parties/${party.id}`);
    },
    onError: (error) => {
      setError(
        error instanceof Error ? error.message : "Failed to create party"
      );
    },
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (!partyName.trim()) {
      setError("Party name is required");
      return;
    }

    createPartyMutation.mutate({ name: partyName.trim() });
  };

  return (
    <div className="mx-auto max-w-2xl">
      <div className="mb-6">
        <button
          onClick={() => navigate(-1)}
          className="flex items-center mb-4 text-gray-500 hover:text-gray-700 text-sm"
        >
          <ArrowLeft className="mr-1 w-4 h-4" />
          Back
        </button>
        <h1 className="font-bold text-gray-900 text-2xl">Create New Party</h1>
        <p className="text-gray-600">Start a collaborative music experience</p>
      </div>

      {isCheckingActiveParty ? (
        <div className="flex justify-center items-center h-64">
          <div className="border-blue-600 border-b-2 rounded-full w-32 h-32 animate-spin"></div>
        </div>
      ) : (activePartyData as any)?.hasActiveParty ? (
        <div className="bg-white shadow p-6 rounded-lg">
          <div className="flex items-center mb-4">
            <AlertCircle className="mr-3 w-6 h-6 text-amber-500" />
            <h2 className="font-medium text-gray-900 text-lg">
              You already have an active party
            </h2>
          </div>
          <p className="mb-4 text-gray-600">
            You can only have one active party at a time. You're currently{" "}
            {(activePartyData as any).party.hostUser.username ===
            (activePartyData as any).party.hostUser.username
              ? "hosting"
              : "participating in"}
            the party "<strong>{(activePartyData as any).party.name}</strong>".
          </p>
          <div className="flex space-x-3">
            <Button
              onClick={() =>
                navigate(`/parties/${(activePartyData as any).party.id}`)
              }
              className="flex-1"
            >
              Go to Your Active Party
            </Button>
            <Button
              variant="outline"
              onClick={() => navigate("/parties")}
              className="flex-1"
            >
              View All Parties
            </Button>
          </div>
        </div>
      ) : (
        <div className="bg-white shadow rounded-lg">
          <form onSubmit={handleSubmit} className="space-y-6 p-6">
            <div>
              <label
                htmlFor="partyName"
                className="block font-medium text-gray-700 text-sm"
              >
                Party Name
              </label>
              <input
                type="text"
                id="partyName"
                className="block shadow-sm mt-1 px-3 py-2 border-gray-300 focus:border-blue-500 rounded-md focus:ring-blue-500 w-full"
                placeholder="Enter a name for your party"
                value={partyName}
                onChange={(e) => setPartyName(e.target.value)}
                required
              />
              <p className="mt-2 text-gray-500 text-sm">
                Choose a descriptive name that others will recognize
              </p>
            </div>

            {error && (
              <div className="bg-red-50 p-4 rounded-md">
                <div className="text-red-700 text-sm">{error}</div>
              </div>
            )}

            <div className="bg-blue-50 p-4 rounded-lg">
              <h3 className="mb-2 font-medium text-blue-900 text-sm">
                What happens next?
              </h3>
              <ul className="space-y-1 text-blue-700 text-sm">
                <li>• You'll become the host of this party</li>
                <li>• Others can join using the party ID</li>
                <li>• Everyone can add songs to the shared playlist</li>
                <li>• You can manage the party and its participants</li>
              </ul>
            </div>

            <div className="flex justify-end space-x-3">
              <Button
                type="button"
                variant="outline"
                onClick={() => navigate("/parties")}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                disabled={createPartyMutation.isPending || !partyName.trim()}
              >
                {createPartyMutation.isPending ? "Creating..." : "Create Party"}
              </Button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
};
