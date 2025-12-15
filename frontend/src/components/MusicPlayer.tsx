import React, { useState, useRef, useEffect } from "react";
import { Play, Pause, SkipForward, SkipBack, Volume2, VolumeX } from "lucide-react";
import { Button } from "./ui/button";

interface Song {
  id: number;
  title: string;
  artist: string;
  album: string;
  duration: string;
  filePath: string;
}

interface MusicPlayerProps {
  currentSong: Song | null;
  playlist: Song[];
  isPlaying: boolean;
  onPlay: () => void;
  onPause: () => void;
  onNext: () => void;
  onPrevious: () => void;
  onSeek: (position: number) => void;
  externalPosition?: number;
}

export const MusicPlayer: React.FC<MusicPlayerProps> = ({
  currentSong,
  playlist,
  isPlaying,
  onPlay,
  onPause,
  onNext,
  onPrevious,
  onSeek,
  externalPosition,
}) => {
  const audioRef = useRef<HTMLAudioElement>(null);
  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(0);
  const [volume, setVolume] = useState(1);
  const [isMuted, setIsMuted] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (externalPosition !== undefined && audioRef.current) {
      const diff = Math.abs(audioRef.current.currentTime - externalPosition);
      if (diff > 2) {
        audioRef.current.currentTime = externalPosition;
      }
    }
  }, [externalPosition]);

  useEffect(() => {
    if (currentSong && audioRef.current) {
      const audioUrl = getAudioUrl(currentSong.filePath);
      setIsLoading(true);
      audioRef.current.load();
    }
  }, [currentSong]);

  useEffect(() => {
    if (!audioRef.current || !currentSong) {
      return;
    }

    if (isPlaying && !isLoading) {
      const playPromise = audioRef.current.play();
      if (playPromise !== undefined) {
        playPromise
          .catch((error) => {
            console.error("❌ Error playing audio:", error);
          });
      }
    } else if (!isPlaying) {
      audioRef.current.pause();
    }
  }, [isPlaying, currentSong, isLoading]);

  const handleTimeUpdate = () => {
    if (audioRef.current) {
      setCurrentTime(audioRef.current.currentTime);
    }
  };

  const handleLoadedMetadata = () => {
    if (audioRef.current) {
      setDuration(audioRef.current.duration);
      setIsLoading(false);
      
      if (isPlaying) {
        const playPromise = audioRef.current.play();
        if (playPromise !== undefined) {
          playPromise
            .catch((error) => {
              console.error("❌ Error auto-playing audio:", error);
            });
        }
      }
    }
  };

  const handleSeek = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newTime = parseFloat(e.target.value);
    if (audioRef.current) {
      audioRef.current.currentTime = newTime;
      setCurrentTime(newTime);
      onSeek(newTime);
    }
  };

  const handleVolumeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newVolume = parseFloat(e.target.value);
    setVolume(newVolume);
    if (audioRef.current) {
      audioRef.current.volume = newVolume;
    }
    setIsMuted(newVolume === 0);
  };

  const toggleMute = () => {
    if (audioRef.current) {
      if (isMuted) {
        audioRef.current.volume = volume;
        setIsMuted(false);
      } else {
        audioRef.current.volume = 0;
        setIsMuted(true);
      }
    }
  };

  const handleEnded = () => {
    onNext();
  };

  const formatTime = (seconds: number): string => {
    if (isNaN(seconds)) return "0:00";
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, "0")}`;
  };

  const getAudioUrl = (filePath: string): string => {
    return `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5097"}/api/audio/stream/${encodeURIComponent(filePath)}`;
  };

  const currentIndex = playlist.findIndex((s) => s.id === currentSong?.id);
  const hasPrevious = currentIndex > 0;
  const hasNext = currentIndex < playlist.length - 1;

  return (
    <div className="fixed bottom-0 left-0 right-0 bg-white shadow-lg border-gray-200 border-t w-full z-50">
      <div className="mx-auto px-4 py-3 container">
        {currentSong && (
          <audio
            ref={audioRef}
            onTimeUpdate={handleTimeUpdate}
            onLoadedMetadata={handleLoadedMetadata}
            onEnded={handleEnded}
            onCanPlay={() => setIsLoading(false)}
            onWaiting={() => setIsLoading(true)}
          >
            <source src={getAudioUrl(currentSong.filePath)} type="audio/mpeg" />
            Your browser does not support the audio element.
          </audio>
        )}

        <div className="flex items-center gap-4">
          {/* Song Info */}
          <div className="flex-1 min-w-0">
            {currentSong ? (
              <div>
                <h3 className="font-medium text-gray-900 text-sm truncate">
                  {currentSong.title}
                </h3>
                <p className="text-gray-500 text-xs truncate">
                  {currentSong.artist}
                </p>
              </div>
            ) : (
              <div className="text-gray-400 text-sm">No song selected</div>
            )}
          </div>

          {/* Playback Controls */}
          <div className="flex flex-col items-center flex-1 gap-2">
            <div className="flex items-center gap-2">
              <Button
                size="sm"
                variant="outline"
                onClick={onPrevious}
                disabled={!hasPrevious || isLoading}
                className="p-2"
              >
                <SkipBack className="w-4 h-4" />
              </Button>
              
              <Button
                size="sm"
                onClick={isPlaying ? onPause : onPlay}
                disabled={!currentSong || isLoading}
                className="p-2"
              >
                {isLoading ? (
                  <div className="border-white border-t-2 rounded-full w-4 h-4 animate-spin"></div>
                ) : isPlaying ? (
                  <Pause className="w-4 h-4" />
                ) : (
                  <Play className="w-4 h-4" />
                )}
              </Button>

              <Button
                size="sm"
                variant="outline"
                onClick={onNext}
                disabled={!hasNext || isLoading}
                className="p-2"
              >
                <SkipForward className="w-4 h-4" />
              </Button>
            </div>

            {/* Progress Bar */}
            <div className="flex items-center gap-2 w-full max-w-md">
              <span className="font-mono text-gray-600 text-xs">
                {formatTime(currentTime)}
              </span>
              <input
                type="range"
                min="0"
                max={duration || 0}
                value={currentTime}
                onChange={handleSeek}
                disabled={!currentSong}
                className="flex-1 h-1 bg-gray-200 rounded-lg appearance-none cursor-pointer"
                style={{
                  background: currentSong
                    ? `linear-gradient(to right, #3b82f6 0%, #3b82f6 ${(currentTime / duration) * 100}%, #e5e7eb ${(currentTime / duration) * 100}%, #e5e7eb 100%)`
                    : "#e5e7eb",
                }}
              />
              <span className="font-mono text-gray-600 text-xs">
                {formatTime(duration)}
              </span>
            </div>
          </div>

          {/* Volume Control */}
          <div className="flex items-center gap-2 flex-1 justify-end">
            <Button
              size="sm"
              variant="ghost"
              onClick={toggleMute}
              className="p-2"
            >
              {isMuted ? (
                <VolumeX className="w-4 h-4" />
              ) : (
                <Volume2 className="w-4 h-4" />
              )}
            </Button>
            <input
              type="range"
              min="0"
              max="1"
              step="0.01"
              value={isMuted ? 0 : volume}
              onChange={handleVolumeChange}
              className="w-24 h-1 bg-gray-200 rounded-lg appearance-none cursor-pointer"
              style={{
                background: `linear-gradient(to right, #3b82f6 0%, #3b82f6 ${(isMuted ? 0 : volume) * 100}%, #e5e7eb ${(isMuted ? 0 : volume) * 100}%, #e5e7eb 100%)`,
              }}
            />
          </div>
        </div>
      </div>
    </div>
  );
};
