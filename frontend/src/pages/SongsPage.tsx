import { useState, useEffect } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Search, Plus, Music, Clock } from 'lucide-react';
import { apiService } from '../services/api';
import { Layout } from '../components/Layout';
import { useToast } from '../contexts/ToastContext';

interface Song {
  id: number;
  title: string;
  artist: string;
  album: string;
  duration: string;
  filePath: string;
}

interface SongsResponse {
  songs: Song[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export default function SongsPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [page, setPage] = useState(1);
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const queryClient = useQueryClient();
  const toast = useToast();

  // Debounce search input
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(searchQuery);
      setPage(1); // Reset to first page when searching
    }, 300);

    return () => clearTimeout(timer);
  }, [searchQuery]);

  const { data: songsData, isLoading, error } = useQuery<SongsResponse>({
    queryKey: ['songs', debouncedSearch, page],
    queryFn: () => apiService.getAllSongs(debouncedSearch || undefined, page, 20) as Promise<SongsResponse>,
  });

  const handleAddSong = async (songId: number) => {
    try {
      await apiService.addSongToSession(songId);
      // Invalidate session queries to refresh session data
      queryClient.invalidateQueries({ queryKey: ['session'] });
      queryClient.invalidateQueries({ queryKey: ['sessions'] });
      toast.success('Song added to your current session!');
    } catch (error) {
      console.error('Failed to add song:', error);
      toast.error('Failed to add song. Make sure you are in an active session.');
    }
  };

  const formatDuration = (duration: string) => {
    // Convert duration format if needed
    return duration;
  };

  if (isLoading) {
    return (
      <Layout>
        <div className="flex items-center justify-center min-h-screen">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
        </div>
      </Layout>
    );
  }

  if (error) {
    return (
      <Layout>
        <div className="flex items-center justify-center min-h-screen">
          <div className="text-center">
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Error Loading Songs</h2>
            <p className="text-gray-600">Failed to load songs. Please try again later.</p>
          </div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="max-w-6xl mx-auto px-4 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-4">Browse Songs</h1>
        <p className="text-gray-600 mb-6">
          Search and add songs to your current session
        </p>

        {/* Search Bar */}
        <div className="relative max-w-lg">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-5 w-5" />
          <input
            type="text"
            placeholder="Search songs, artists, or albums..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
        </div>
      </div>

      {/* Results Info */}
      {songsData && (
        <div className="mb-6">
          <p className="text-gray-600">
            Showing {songsData.songs.length} of {songsData.totalCount} songs
            {debouncedSearch && ` for "${debouncedSearch}"`}
          </p>
        </div>
      )}

      {/* Songs Grid */}
      <div className="grid gap-4 mb-8">
        {songsData?.songs.map((song) => (
          <div
            key={song.id}
            className="bg-white rounded-lg border border-gray-200 p-6 hover:shadow-md transition-shadow"
          >
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-4">
                <div className="flex-shrink-0">
                  <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
                    <Music className="h-6 w-6 text-blue-600" />
                  </div>
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="text-lg font-semibold text-gray-900 truncate">
                    {song.title}
                  </h3>
                  <p className="text-gray-600 truncate">{song.artist}</p>
                  <p className="text-sm text-gray-500 truncate">{song.album}</p>
                </div>
              </div>
              <div className="flex items-center space-x-4">
                <div className="flex items-center text-gray-500">
                  <Clock className="h-4 w-4 mr-1" />
                  <span className="text-sm">{formatDuration(song.duration)}</span>
                </div>
                <button
                  onClick={() => handleAddSong(song.id)}
                  className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg flex items-center space-x-2 transition-colors"
                >
                  <Plus className="h-4 w-4" />
                  <span>Add</span>
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Pagination */}
      {songsData && songsData.totalPages > 1 && (
        <div className="flex items-center justify-center space-x-2">
          <button
            onClick={() => setPage(page - 1)}
            disabled={page === 1}
            className="px-4 py-2 border border-gray-300 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
          >
            Previous
          </button>
          
          <div className="flex space-x-1">
            {Array.from({ length: Math.min(5, songsData.totalPages) }, (_, i) => {
              const pageNum = i + 1;
              return (
                <button
                  key={pageNum}
                  onClick={() => setPage(pageNum)}
                  className={`px-3 py-2 rounded-lg ${
                    page === pageNum
                      ? 'bg-blue-600 text-white'
                      : 'border border-gray-300 hover:bg-gray-50'
                  }`}
                >
                  {pageNum}
                </button>
              );
            })}
          </div>

          <button
            onClick={() => setPage(page + 1)}
            disabled={page === songsData.totalPages}
            className="px-4 py-2 border border-gray-300 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
          >
            Next
          </button>
        </div>
      )}

      {/* Empty State */}
      {songsData?.songs.length === 0 && (
        <div className="text-center py-12">
          <Music className="h-16 w-16 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">No songs found</h3>
          <p className="text-gray-600">
            {debouncedSearch
              ? `No songs match "${debouncedSearch}". Try a different search term.`
              : 'No songs available in the library.'}
          </p>
        </div>
      )}
    </div>
    </Layout>
  );
}