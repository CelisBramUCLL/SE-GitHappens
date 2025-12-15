import React, { createContext, useContext, useState, useCallback } from "react";
import { signalRService } from "../services/signalRService";

interface Song {
  id: number;
  title: string;
  artist: string;
  album: string;
  duration: string;
  filePath: string;
}

interface MusicPlayerContextType {
  currentSong: Song | null;
  playlist: Song[];
  isPlaying: boolean;
  currentPosition: number;
  currentPartyId: number | null;
  setCurrentSong: (song: Song | null) => void;
  setPlaylist: (songs: Song[]) => void;
  setIsPlaying: (playing: boolean) => void;
  setCurrentPosition: (position: number) => void;
  setCurrentPartyId: (partyId: number | null) => void;
  play: (song: Song) => void;
  pause: () => void;
  stop: () => void;
  next: () => void;
  previous: () => void;
  seek: (position: number) => void;
  resume: () => void;
  syncFromSignalR: (songId: number, playing: boolean, position: number) => void;
}

const MusicPlayerContext = createContext<MusicPlayerContextType | undefined>(undefined);

export const MusicPlayerProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [currentSong, setCurrentSong] = useState<Song | null>(null);
  const [playlist, setPlaylist] = useState<Song[]>([]);
  const [isPlaying, setIsPlaying] = useState(false);
  const [currentPosition, setCurrentPosition] = useState(0);
  const [currentPartyId, setCurrentPartyId] = useState<number | null>(null);

  const play = useCallback((song: Song) => {
    setCurrentSong(song);
    setIsPlaying(true);
    setCurrentPosition(0);
    
    if (currentPartyId && signalRService.isConnected) {
      signalRService.playSong(currentPartyId, song.id, 0);
    }
  }, [currentPartyId]);

  const pause = useCallback(() => {
    setIsPlaying(false);
    
    if (currentPartyId && signalRService.isConnected && currentSong) {
      signalRService.pauseSong(currentPartyId, currentPosition);
    }
  }, [currentPartyId, currentPosition, currentSong]);

  const resume = useCallback(() => {
    if (!currentSong) return;
    setIsPlaying(true);
    
    if (currentPartyId && signalRService.isConnected) {
      signalRService.playSong(currentPartyId, currentSong.id, currentPosition);
    }
  }, [currentPartyId, currentSong, currentPosition]);

  const stop = useCallback(() => {
    setIsPlaying(false);
    setCurrentSong(null);
    setCurrentPosition(0);
    
    if (currentPartyId && signalRService.isConnected) {
      signalRService.stopSong(currentPartyId);
    }
  }, [currentPartyId]);

  const next = useCallback(() => {
    if (currentSong && playlist.length > 0) {
      const currentIndex = playlist.findIndex((s) => s.id === currentSong.id);
      if (currentIndex < playlist.length - 1) {
        const nextSong = playlist[currentIndex + 1];
        play(nextSong);
      }
    }
  }, [currentSong, playlist, play, currentPartyId]);

  const previous = useCallback(() => {
    if (currentSong && playlist.length > 0) {
      const currentIndex = playlist.findIndex((s) => s.id === currentSong.id);
      if (currentIndex > 0) {
        const prevSong = playlist[currentIndex - 1];
        play(prevSong);
      }
    }
  }, [currentSong, playlist, play, currentPartyId]);

  const seek = useCallback((position: number) => {
    setCurrentPosition(position);
    
    if (currentPartyId && signalRService.isConnected) {
      signalRService.seekSong(currentPartyId, position);
    }
  }, [currentPartyId]);

  const syncFromSignalR = useCallback((songId: number, playing: boolean, position: number) => {
    const song = playlist.find((s) => s.id === songId);
    if (song) {
      setCurrentSong(song);
      setIsPlaying(playing);
      setCurrentPosition(position);
    }
  }, [playlist]);

  return (
    <MusicPlayerContext.Provider
      value={{
        currentSong,
        playlist,
        isPlaying,
        currentPosition,
        currentPartyId,
        setCurrentSong,
        setPlaylist,
        setIsPlaying,
        setCurrentPosition,
        setCurrentPartyId,
        play,
        pause,
        stop,
        next,
        previous,
        seek,
        resume,
        syncFromSignalR,
      }}
    >
      {children}
    </MusicPlayerContext.Provider>
  );
};

export const useMusicPlayer = () => {
  const context = useContext(MusicPlayerContext);
  if (context === undefined) {
    throw new Error("useMusicPlayer must be used within a MusicPlayerProvider");
  }
  return context;
};
