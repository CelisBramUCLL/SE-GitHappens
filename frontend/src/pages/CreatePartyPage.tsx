import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { partyService } from '../services/party.service';
import { Layout } from '../components/Layout';
import { Button } from '../components/ui/button';
import { ArrowLeft } from 'lucide-react';

export const CreatePartyPage: React.FC = () => {
  const [partyName, setPartyName] = useState('');
  const [error, setError] = useState('');
  
  const navigate = useNavigate();
  const queryClient = useQueryClient();	

  const createPartyMutation = useMutation({
    mutationFn: (data: { name: string }) => partyService.create(data),
    onSuccess: (party: any) => {
      queryClient.invalidateQueries({ queryKey: ['parties'] });
      navigate(`/parties/${party.id}`);
    },
    onError: (error) => {
      setError(error instanceof Error ? error.message : 'Failed to create party');
    },
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    
    if (!partyName.trim()) {
      setError('Party name is required');
      return;
    }

    createPartyMutation.mutate({ name: partyName.trim() });
  };

  return (
    <Layout>
      <div className="max-w-2xl mx-auto">
        <div className="mb-6">
          <button
            onClick={() => navigate(-1)}
            className="flex items-center text-sm text-gray-500 hover:text-gray-700 mb-4"
          >
            <ArrowLeft className="h-4 w-4 mr-1" />
            Back
          </button>
          <h1 className="text-2xl font-bold text-gray-900">Create New Party</h1>
          <p className="text-gray-600">Start a new collaborative music party</p>
        </div>

        <div className="bg-white shadow rounded-lg">
          <form onSubmit={handleSubmit} className="p-6 space-y-6">
            <div>
              <label htmlFor="partyName" className="block text-sm font-medium text-gray-700">
                Party Name
              </label>
              <input
                type="text"
                id="partyName"
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 px-3 py-2"
                placeholder="Enter a name for your party"
                value={partyName}
                onChange={(e) => setPartyName(e.target.value)}
                required
              />
              <p className="mt-2 text-sm text-gray-500">
                Choose a descriptive name that others will recognize
              </p>
            </div>

            {error && (
              <div className="rounded-md bg-red-50 p-4">
                <div className="text-sm text-red-700">{error}</div>
              </div>
            )}

            <div className="bg-blue-50 p-4 rounded-lg">
              <h3 className="text-sm font-medium text-blue-900 mb-2">What happens next?</h3>
              <ul className="text-sm text-blue-700 space-y-1">
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
                onClick={() => navigate('/parties')}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                disabled={createPartyMutation.isPending || !partyName.trim()}
              >
                {createPartyMutation.isPending ? 'Creating...' : 'Create Party'}
              </Button>
            </div>
          </form>
        </div>
      </div>
    </Layout>
  );
};