// User types
export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  username: string;
  role: Role;
}

export interface CreateUserDTO {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  username: string;
  role: string;
}

export interface LoginDTO {
  username: string;
  password: string;
}

export interface LoginResponseDTO {
  id: number;
  username: string;
  token: string;
}

export interface HostUserDTO {
  id: number;
  username: string;
}

// Party types
export interface Party {
  id: number;
  name: string;
  status: PartyStatus;
  createdAt: string;
  updatedAt: string;
  hostUser: HostUserDTO;
  playlist: PlaylistDTO;
  participants: ParticipantInPartyDTO[];
}

export interface CreatePartyDTO {
  name: string;
}

export interface JoinPartyDTO {
  partyId: number;
}

export interface LeavePartyDTO {
  partyId: number;
}

export interface UpdatePartyDTO {
  name?: string;
}

// Participant types
export interface ParticipantDTO {
  id: number;
  userId: number;
  partyId: number;
  joinedAt: string;
  user?: HostUserDTO;
}

export interface ParticipantInPartyDTO {
  id: number;
  userName: string;
  joinedAt: string;
}

// Playlist types
export interface PlaylistDTO {
  id: number;
  name: string;
  songs: SongDTO[];
}

export interface PlaylistSongDTO {
  id: number;
  songId: number;
  title: string;
  artist: string;
  album: string;
  duration: string;
  filePath: string;
  addedByUserId: number;
  position: number;
  addedAt: string;
}

// Song types
export interface SongDTO {
  id: number;
  title: string;
  artist: string;
  album: string;
  duration: string;
  filePath: string;
}

export interface AddSongDTO {
  songId: number;
}

export interface RemoveSongDTO {
  songId: number;
}

// Constants as alternative to enums
export const Role = {
  Admin: 'Admin' as const,
  User: 'User' as const,
  Guest: 'Guest' as const
} as const;

export type Role = typeof Role[keyof typeof Role];

export const PartyStatus = {
  Active: 'Active' as const,
  Ended: 'Ended' as const
} as const;

export type PartyStatus = typeof PartyStatus[keyof typeof PartyStatus];

export const ParticipantRole = {
  Host: 'Host' as const,
  Member: 'Member' as const
} as const;

export type ParticipantRole = typeof ParticipantRole[keyof typeof ParticipantRole];