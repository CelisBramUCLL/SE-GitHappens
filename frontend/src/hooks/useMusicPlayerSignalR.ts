import { useEffect, useRef } from 'react';
import { useMusicPlayer } from '../contexts/MusicPlayerContext';
import { signalRService } from '../services/signalRService';
import { useSignalR } from './useSignalR';
import { partyService } from '../services/party.service';

export const useMusicPlayerSignalR = () => {
  const musicPlayer = useMusicPlayer();
  const { isConnected } = useSignalR();
  const handlersSetUp = useRef(false);
  
  const musicPlayerRef = useRef(musicPlayer);
  const currentPartyIdRef = useRef(musicPlayer.currentPartyId);
  
  useEffect(() => {
    musicPlayerRef.current = musicPlayer;
  }, [musicPlayer]);

  useEffect(() => {
    currentPartyIdRef.current = musicPlayer.currentPartyId;
  }, [musicPlayer.currentPartyId]);

  useEffect(() => {
    if (!isConnected) {
      handlersSetUp.current = false;
      return;
    }
    
    if (handlersSetUp.current) {
      return;
    }


    const handlePlaySong = async (songId: number, position: number, connectionId: string) => {
      const player = musicPlayerRef.current;
      let song = player.playlist.find((s: any) => s.id === songId);
      
      // If song not found in playlist, fetch updated playlist from API
      if (!song) {
        const partyId = currentPartyIdRef.current;
        if (partyId) {
          try {
            const party = await partyService.getById(partyId);
            if (party?.playlist?.songs) {
              player.setPlaylist(party.playlist.songs);
              song = party.playlist.songs.find((s: any) => s.id === songId);
            }
          } catch (error) {
            console.error('Error fetching updated playlist:', error);
          }
        }
      }
      
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

    const handleSongAdded = async (songId: number, connectionId: string) => {
      const partyId = currentPartyIdRef.current;
      if (!partyId) return;

      try {
        const party = await partyService.getById(partyId);
        if (party?.playlist?.songs) {
          musicPlayerRef.current.setPlaylist(party.playlist.songs);
        }
      } catch (error) {
        console.error('Error fetching updated playlist:', error);
      }
    };

    const handleSongRemoved = async (songId: number, connectionId: string) => {
      const partyId = currentPartyIdRef.current;
      if (!partyId) return;

      try {
        const party = await partyService.getById(partyId);
        if (party?.playlist?.songs) {
          musicPlayerRef.current.setPlaylist(party.playlist.songs);
        }
      } catch (error) {
        console.error('Error fetching updated playlist:', error);
      }
    };

    signalRService.onPlaySong(handlePlaySong);
    signalRService.onPauseSong(handlePauseSong);
    signalRService.onSeekSong(handleSeekSong);
    signalRService.onStopSong(handleStopSong);
    signalRService.onSyncPlaybackState(handleSyncPlaybackState);
    signalRService.onSongAdded(handleSongAdded);
    signalRService.onSongRemoved(handleSongRemoved);

    handlersSetUp.current = true;

    return () => {
      signalRService.off('playSong');
      signalRService.off('pauseSong');
      signalRService.off('seekSong');
      signalRService.off('stopSong');
      signalRService.off('syncPlaybackState');
      signalRService.off('SongAdded');
      signalRService.off('SongRemoved');
      handlersSetUp.current = false;
    };
  }, [isConnected]);
};
