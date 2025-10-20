import React from "react";
import { Link } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { partyService } from "../services/party.service";
import { useGlobalPartyEvents } from "../hooks/useGlobalPartyEvents";
import { Button } from "../components/ui/button";
import { Plus, Users, Music, Clock } from "lucide-react";

export const DashboardPage: React.FC = () => {
  useGlobalPartyEvents("Dashboard");

  const {
    data: parties,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["parties"],
    queryFn: () => partyService.getAll(),
  });

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
          <h1 className="font-bold text-gray-900 text-2xl">Dashboard</h1>
          <p className="text-gray-600">Manage your music parties</p>
        </div>
        <Link to="/parties/create">
          <Button className="flex items-center space-x-2">
            <Plus className="w-4 h-4" />
            <span>Create Party</span>
          </Button>
        </Link>
      </div>

      <div className="gap-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
        <div className="bg-white shadow rounded-lg overflow-hidden">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <Music className="w-6 h-6 text-gray-400" />
              </div>
              <div className="flex-1 ml-5 w-0">
                <dl>
                  <dt className="font-medium text-gray-500 text-sm truncate">
                    Active Parties
                  </dt>
                  <dd className="font-medium text-gray-900 text-lg">
                    {Array.isArray(parties)
                      ? parties.filter((p: any) => p.status === "Active").length
                      : 0}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div className="bg-white shadow rounded-lg overflow-hidden">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <Users className="w-6 h-6 text-gray-400" />
              </div>
              <div className="flex-1 ml-5 w-0">
                <dl>
                  <dt className="font-medium text-gray-500 text-sm truncate">
                    Total Participants
                  </dt>
                  <dd className="font-medium text-gray-900 text-lg">
                    {Array.isArray(parties)
                      ? parties.reduce(
                          (acc: number, p: any) =>
                            acc + (p.participants?.length || 0),
                          0
                        )
                      : 0}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div className="bg-white shadow rounded-lg overflow-hidden">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <Clock className="w-6 h-6 text-gray-400" />
              </div>
              <div className="flex-1 ml-5 w-0">
                <dl>
                  <dt className="font-medium text-gray-500 text-sm truncate">
                    Recent Parties
                  </dt>
                  <dd className="font-medium text-gray-900 text-lg">
                    {Array.isArray(parties) ? parties.length : 0}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="bg-white shadow sm:rounded-md overflow-hidden">
        <div className="px-4 sm:px-6 py-5">
          <h3 className="font-medium text-gray-900 text-lg leading-6">
            Recent Parties
          </h3>
          <p className="mt-1 max-w-2xl text-gray-500 text-sm">
            Your recent music parties and activities.
          </p>
        </div>
        <ul className="divide-y divide-gray-200">
          {Array.isArray(parties) && parties.length > 0 ? (
            parties.slice(0, 5).map((party: any) => (
              <li key={party.id}>
                <Link
                  to={`/parties/${party.id}`}
                  className="block hover:bg-gray-50 px-4 sm:px-6 py-4"
                >
                  <div className="flex justify-between items-center">
                    <div className="flex items-center">
                      <div className="flex-shrink-0 w-10 h-10">
                        <div className="flex justify-center items-center bg-blue-100 rounded-full w-10 h-10">
                          <Music className="w-5 h-5 text-blue-600" />
                        </div>
                      </div>
                      <div className="ml-4">
                        <div className="font-medium text-gray-900 text-sm">
                          {party.name}
                        </div>
                        <div className="text-gray-500 text-sm">
                          Host: {party.hostUser?.username}
                        </div>
                      </div>
                    </div>
                    <div className="flex items-center space-x-2">
                      <span
                        className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                          party.status === "Active"
                            ? "bg-green-100 text-green-800"
                            : "bg-gray-100 text-gray-800"
                        }`}
                      >
                        {party.status}
                      </span>
                      <span className="text-gray-500 text-sm">
                        {party.participants?.length || 0} participants
                      </span>
                    </div>
                  </div>
                </Link>
              </li>
            ))
          ) : (
            <li className="px-4 py-8 text-center">
              <div className="text-gray-500">
                <Music className="mx-auto w-12 h-12 text-gray-400" />
                <h3 className="mt-2 font-medium text-gray-900 text-sm">
                  No parties
                </h3>
                <p className="mt-1 text-gray-500 text-sm">
                  Get started by creating your first party.
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
            </li>
          )}
        </ul>
      </div>
    </div>
  );
};
