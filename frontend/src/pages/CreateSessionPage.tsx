import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { sessionService } from "../services/session.service";
import { Button } from "../components/ui/button";
import { ArrowLeft } from "lucide-react";

export const CreateSessionPage: React.FC = () => {
  const [sessionName, setSessionName] = useState("");
  const [error, setError] = useState("");

  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const createSessionMutation = useMutation({
    mutationFn: (data: { name: string }) => sessionService.create(data),
    onSuccess: (session: any) => {
      queryClient.invalidateQueries({ queryKey: ["sessions"] });
      navigate(`/sessions/${session.id}`);
    },
    onError: (error) => {
      setError(
        error instanceof Error ? error.message : "Failed to create session"
      );
    },
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (!sessionName.trim()) {
      setError("Session name is required");
      return;
    }

    createSessionMutation.mutate({ name: sessionName.trim() });
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
        <h1 className="font-bold text-gray-900 text-2xl">Create New Session</h1>
        <p className="text-gray-600">Start a new collaborative music session</p>
      </div>

      <div className="bg-white shadow rounded-lg">
        <form onSubmit={handleSubmit} className="space-y-6 p-6">
          <div>
            <label
              htmlFor="sessionName"
              className="block font-medium text-gray-700 text-sm"
            >
              Session Name
            </label>
            <input
              type="text"
              id="sessionName"
              className="block shadow-sm mt-1 px-3 py-2 border-gray-300 focus:border-blue-500 rounded-md focus:ring-blue-500 w-full"
              placeholder="Enter a name for your session"
              value={sessionName}
              onChange={(e) => setSessionName(e.target.value)}
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
              <li>• You'll become the host of this session</li>
              <li>• Others can join using the session ID</li>
              <li>• Everyone can add songs to the shared playlist</li>
              <li>• You can manage the session and its participants</li>
            </ul>
          </div>

          <div className="flex justify-end space-x-3">
            <Button
              type="button"
              variant="outline"
              onClick={() => navigate("/sessions")}
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={createSessionMutation.isPending || !sessionName.trim()}
            >
              {createSessionMutation.isPending
                ? "Creating..."
                : "Create Session"}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
