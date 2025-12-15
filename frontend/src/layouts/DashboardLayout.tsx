import { Link, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { useMusicPlayer } from "../contexts/MusicPlayerContext";
import { useMusicPlayerSignalR } from "../hooks/useMusicPlayerSignalR";
import { Music, LogOut } from "lucide-react";
import { Button } from "@/components/ui/button";
import { ActiveUserCounter } from "../components/ActiveUserCounter";
import { MusicPlayer } from "../components/MusicPlayer";

export const DashboardLayout = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const musicPlayer = useMusicPlayer();
  
  // Set up global music player SignalR handlers
  useMusicPlayerSignalR();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  // Wrapper functions for music player callbacks
  const handlePlay = () => {
    if (musicPlayer.currentSong) {
      // Resume from current position without resetting to 0
      musicPlayer.resume();
    } else if (musicPlayer.playlist.length > 0) {
      musicPlayer.play(musicPlayer.playlist[0]);
    }
  };

  return (
    <div className="bg-gray-50 min-h-screen">
      <nav className="bg-white shadow-sm border-b">
        <div className="mx-auto px-4 sm:px-6 lg:px-8 max-w-7xl">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <Link to="/dashboard" className="flex items-center space-x-2">
                <Music className="w-8 h-8 text-blue-600" />
                <span className="font-bold text-gray-900 text-xl">
                  GitHappens
                </span>
              </Link>
              <div className="hidden md:flex md:space-x-8 md:ml-6">
                <Link
                  to="/dashboard"
                  className="px-3 py-2 rounded-md font-medium text-gray-500 hover:text-gray-700 text-sm"
                >
                  Dashboard
                </Link>
                <Link
                  to="/parties"
                  className="px-3 py-2 rounded-md font-medium text-gray-500 hover:text-gray-700 text-sm"
                >
                  Parties
                </Link>
                <Link
                  to="/songs"
                  className="px-3 py-2 rounded-md font-medium text-gray-500 hover:text-gray-700 text-sm"
                >
                  Songs
                </Link>
              </div>
            </div>
            <div className="flex items-center space-x-4">
              <ActiveUserCounter />
              <span className="text-gray-700 text-sm">
                Welcome, {user?.username}
              </span>
              <Button
                variant="outline"
                size="sm"
                onClick={handleLogout}
                className="flex items-center space-x-1"
              >
                <LogOut className="w-4 h-4" />
                <span>Logout</span>
              </Button>
            </div>
          </div>
        </div>
      </nav>
      <main className="mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 max-w-7xl">
        <Outlet />
      </main>
      <MusicPlayer
        currentSong={musicPlayer.currentSong}
        playlist={musicPlayer.playlist}
        isPlaying={musicPlayer.isPlaying}
        onPlay={handlePlay}
        onPause={musicPlayer.pause}
        onNext={musicPlayer.next}
        onPrevious={musicPlayer.previous}
        onSeek={musicPlayer.seek}
        externalPosition={musicPlayer.currentPosition}
      />
    </div>
  );
};
