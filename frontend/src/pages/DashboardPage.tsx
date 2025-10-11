import React from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { apiService } from '../services/api';
import { Layout } from '../components/Layout';
import { Button } from '../components/ui/button';
import { Plus, Users, Music, Clock } from 'lucide-react';

export const DashboardPage: React.FC = () => {
  const { data: sessions, isLoading, error } = useQuery({
    queryKey: ['sessions'],
    queryFn: () => apiService.getAllSessions(),
  });

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
            <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
            <p className="text-gray-600">Manage your music sessions</p>
          </div>
          <Link to="/sessions/create">
            <Button className="flex items-center space-x-2">
              <Plus className="h-4 w-4" />
              <span>Create Session</span>
            </Button>
          </Link>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <div className="bg-white overflow-hidden shadow rounded-lg">
            <div className="p-5">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <Music className="h-6 w-6 text-gray-400" />
                </div>
                <div className="ml-5 w-0 flex-1">
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 truncate">
                      Active Sessions
                    </dt>
                    <dd className="text-lg font-medium text-gray-900">
                      {Array.isArray(sessions) ? sessions.filter((s: any) => s.status === 'Active').length : 0}
                    </dd>
                  </dl>
                </div>
              </div>
            </div>
          </div>

          <div className="bg-white overflow-hidden shadow rounded-lg">
            <div className="p-5">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <Users className="h-6 w-6 text-gray-400" />
                </div>
                <div className="ml-5 w-0 flex-1">
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 truncate">
                      Total Participants
                    </dt>
                    <dd className="text-lg font-medium text-gray-900">
                      {Array.isArray(sessions) ? sessions.reduce((acc: number, s: any) => acc + (s.participants?.length || 0), 0) : 0}
                    </dd>
                  </dl>
                </div>
              </div>
            </div>
          </div>

          <div className="bg-white overflow-hidden shadow rounded-lg">
            <div className="p-5">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <Clock className="h-6 w-6 text-gray-400" />
                </div>
                <div className="ml-5 w-0 flex-1">
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 truncate">
                      Recent Sessions
                    </dt>
                    <dd className="text-lg font-medium text-gray-900">
                      {Array.isArray(sessions) ? sessions.length : 0}
                    </dd>
                  </dl>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="bg-white shadow overflow-hidden sm:rounded-md">
          <div className="px-4 py-5 sm:px-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900">
              Recent Sessions
            </h3>
            <p className="mt-1 max-w-2xl text-sm text-gray-500">
              Your recent music sessions and activities.
            </p>
          </div>
          <ul className="divide-y divide-gray-200">
            {Array.isArray(sessions) && sessions.length > 0 ? (
              sessions.slice(0, 5).map((session: any) => (
                <li key={session.id}>
                  <Link
                    to={`/sessions/${session.id}`}
                    className="block hover:bg-gray-50 px-4 py-4 sm:px-6"
                  >
                    <div className="flex items-center justify-between">
                      <div className="flex items-center">
                        <div className="flex-shrink-0 h-10 w-10">
                          <div className="h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center">
                            <Music className="h-5 w-5 text-blue-600" />
                          </div>
                        </div>
                        <div className="ml-4">
                          <div className="text-sm font-medium text-gray-900">
                            {session.name}
                          </div>
                          <div className="text-sm text-gray-500">
                            Host: {session.hostUser?.username}
                          </div>
                        </div>
                      </div>
                      <div className="flex items-center space-x-2">
                        <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                          session.status === 'Active' 
                            ? 'bg-green-100 text-green-800' 
                            : 'bg-gray-100 text-gray-800'
                        }`}>
                          {session.status}
                        </span>
                        <span className="text-sm text-gray-500">
                          {session.participants?.length || 0} participants
                        </span>
                      </div>
                    </div>
                  </Link>
                </li>
              ))
            ) : (
              <li className="px-4 py-8 text-center">
                <div className="text-gray-500">
                  <Music className="mx-auto h-12 w-12 text-gray-400" />
                  <h3 className="mt-2 text-sm font-medium text-gray-900">No sessions</h3>
                  <p className="mt-1 text-sm text-gray-500">
                    Get started by creating your first session.
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
              </li>
            )}
          </ul>
        </div>
      </div>
    </Layout>
  );
};