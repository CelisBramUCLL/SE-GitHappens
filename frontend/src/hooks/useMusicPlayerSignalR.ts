import { useEffect, useRef } from 'react';
import { useMusicPlayer } from '../contexts/MusicPlayerContext';
import { signalRService } from '../services/signalRService';
import { useSignalR } from './useSignalR';

export const useMusicPlayerSignalR = () => {
  const musicPlayer = useMusicPlayer();
  const { isConnected } = useSignalR();
  const handlersSetUp = useRef(false);
  
  const musicPlayerRef = useRef(musicPlayer);
  
  useEffect(() => {
    musicPlayerRef.current = musicPlayer;
  }, [musicPlayer]);

  useEffect(() => {
    if (!isConnected) {
      handlersSetUp.current = false;
      return;
    }
    
    if (handlersSetUp.current) {
      return;
    }


    const handlePlaySong = (songId: number, position: number, connectionId: string) => {
      const player = musicPlayerRef.current;
      const song = player.playlist.find((s: any) => s.id === songId);
      if (song) {
        player.setCurrentSong(song);
        player.setIsPlaying(true);
        player.setCurrentPosition(position);
      }
    };

    const handlePauseSong = (position: number, connectionId: string) => {
      const player = musicPlayerRef.current;
      player.setIsPlaying(false);
      player.setCurrentPosition(position);
    };

    const handleSeekSong = (position: number, connectionId: string) => {
      const player = musicPlayerRef.current;
      player.setCurrentPosition(position);
    };

    const handleStopSong = (connectionId: string) => {
      const player = musicPlayerRef.current;
      player.setIsPlaying(false);
      player.setCurrentSong(null);
      player.setCurrentPosition(0);
    };

    const handleSyncPlaybackState = (
      songId: number | null,
      playing: boolean,
      position: number,
      connectionId: string
    ) => {
      const player = musicPlayerRef.current;
      if (songId) {
        const song = player.playlist.find((s: any) => s.id === songId);
        if (song) {
          player.setCurrentSong(song);
        }
      } else {
        player.setCurrentSong(null);
      }
      player.setIsPlaying(playing);
      player.setCurrentPosition(position);
    };

    signalRService.onPlaySong(handlePlaySong);
    signalRService.onPauseSong(handlePauseSong);
    signalRService.onSeekSong(handleSeekSong);
    signalRService.onStopSong(handleStopSong);
    signalRService.onSyncPlaybackState(handleSyncPlaybackState);

    handlersSetUp.current = true;

    return () => {
      signalRService.off('playSong');
      signalRService.off('pauseSong');
      signalRService.off('seekSong');
      signalRService.off('stopSong');
      signalRService.off('syncPlaybackState');
      handlersSetUp.current = false;
    };
  }, [isConnected]);
};
