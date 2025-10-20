import { useState, useEffect } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Search, Plus, Music, Clock } from "lucide-react";
import { songService } from "../services/song.service";
import { partyService } from "../services/party.service";
import { useToast } from "../contexts/ToastContext";

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
  const [searchQuery, setSearchQuery] = useState("");
  const [page, setPage] = useState(1);
  const [debouncedSearch, setDebouncedSearch] = useState("");
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

  const {
    data: songsData,
    isLoading,
    error,
  } = useQuery<SongsResponse>({
    queryKey: ["songs", debouncedSearch, page],
    queryFn: () =>
      songService.getAll(
        debouncedSearch || undefined,
        page,
        20
      ) as Promise<SongsResponse>,
  });

  const handleAddSong = async (songId: number) => {
    try {
      await partyService.addSong(songId);
      // Invalidate party queries to refresh party data
      queryClient.invalidateQueries({ queryKey: ["party"] });
      queryClient.invalidateQueries({ queryKey: ["parties"] });
      toast.success("Song added to your current party!");
    } catch (error) {
      console.error("Failed to add song:", error);
      toast.error("Failed to add song. Make sure you are in an active party.");
    }
  };

  const formatDuration = (duration: string) => {
    // Convert duration format if needed
    return duration;
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <div className="border-blue-600 border-b-2 rounded-full w-32 h-32 animate-spin"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <div className="text-center">
          <h2 className="mb-2 font-bold text-gray-900 text-2xl">
            Error Loading Songs
          </h2>
          <p className="text-gray-600">
            Failed to load songs. Please try again later.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto px-4 py-8 max-w-6xl">
      <div className="mb-8">
        <h1 className="mb-4 font-bold text-gray-900 text-3xl">Browse Songs</h1>
        <p className="mb-6 text-gray-600">
          Search and add songs to your current party
        </p>

        {/* Search Bar */}
        <div className="relative max-w-lg">
          <Search className="top-1/2 left-3 absolute w-5 h-5 text-gray-400 -translate-y-1/2 transform" />
          <input
            type="text"
            placeholder="Search songs, artists, or albums..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="py-3 pr-4 pl-10 border border-gray-300 focus:border-transparent rounded-lg focus:ring-2 focus:ring-blue-500 w-full"
          />
        </div>
      </div>

      {/* Results Info */}
      {songsData && (
        <div className="mb-6">
          <p className="text-gray-600">
            Showing {(page - 1) * 20 + 1}-
            {Math.min(page * 20, songsData.totalCount)} of{" "}
            {songsData.totalCount} songs
            {debouncedSearch && ` for "${debouncedSearch}"`}
          </p>
        </div>
      )}

      {/* Songs Grid */}
      <div className="gap-4 grid mb-8">
        {songsData?.songs.map((song) => (
          <div
            key={song.id}
            className="bg-white hover:shadow-md p-6 border border-gray-200 rounded-lg transition-shadow"
          >
            <div className="flex justify-between items-center">
              <div className="flex items-center space-x-4">
                <div className="flex-shrink-0">
                  <div className="flex justify-center items-center bg-blue-100 rounded-lg w-12 h-12">
                    <Music className="w-6 h-6 text-blue-600" />
                  </div>
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="font-semibold text-gray-900 text-lg truncate">
                    {song.title}
                  </h3>
                  <p className="text-gray-600 truncate">{song.artist}</p>
                  <p className="text-gray-500 text-sm truncate">{song.album}</p>
                </div>
              </div>
              <div className="flex items-center space-x-4">
                <div className="flex items-center text-gray-500">
                  <Clock className="mr-1 w-4 h-4" />
                  <span className="text-sm">
                    {formatDuration(song.duration)}
                  </span>
                </div>
                <button
                  onClick={() => handleAddSong(song.id)}
                  className="flex items-center space-x-2 bg-blue-600 hover:bg-blue-700 px-4 py-2 rounded-lg text-white transition-colors cursor-pointer"
                >
                  <Plus className="w-4 h-4" />
                  <span>Add</span>
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Pagination */}
      {songsData && songsData.totalPages > 1 && (
        <div className="flex justify-center items-center space-x-2">
          <button
            onClick={() => setPage(page - 1)}
            disabled={page === 1}
            className="hover:bg-gray-50 disabled:opacity-50 px-4 py-2 border border-gray-300 rounded-lg transition-colors cursor-pointer disabled:cursor-not-allowed"
          >
            Previous
          </button>

          <div className="flex space-x-1">
            {Array.from(
              { length: Math.min(5, songsData.totalPages) },
              (_, i) => {
                const pageNum = i + 1;
                return (
                  <button
                    key={pageNum}
                    onClick={() => setPage(pageNum)}
                    className={`px-3 py-2 rounded-lg cursor-pointer transition-colors ${
                      page === pageNum
                        ? "bg-blue-600 text-white"
                        : "border border-gray-300 hover:bg-gray-50"
                    }`}
                  >
                    {pageNum}
                  </button>
                );
              }
            )}
          </div>

          <button
            onClick={() => setPage(page + 1)}
            disabled={page === songsData.totalPages}
            className="hover:bg-gray-50 disabled:opacity-50 px-4 py-2 border border-gray-300 rounded-lg transition-colors cursor-pointer disabled:cursor-not-allowed"
          >
            Next
          </button>
        </div>
      )}

      {/* Empty State */}
      {songsData?.songs.length === 0 && (
        <div className="py-12 text-center">
          <Music className="mx-auto mb-4 w-16 h-16 text-gray-400" />
          <h3 className="mb-2 font-medium text-gray-900 text-lg">
            No songs found
          </h3>
          <p className="text-gray-600">
            {debouncedSearch
              ? `No songs match "${debouncedSearch}". Try a different search term.`
              : "No songs available in the library."}
          </p>
        </div>
      )}
    </div>
  );
}
