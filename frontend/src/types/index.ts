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

// Session types
export interface Session {
  id: number;
  name: string;
  status: SessionStatus;
  createdAt: string;
  updatedAt: string;
  hostUser: HostUserDTO;
  playlist: PlaylistDTO;
  participants: ParticipantInSessionDTO[];
}

export interface CreateSessionDTO {
  name: string;
}

export interface JoinSessionDTO {
  sessionId: number;
}

export interface LeaveSessionDTO {
  sessionId: number;
}

export interface UpdateSessionDTO {
  name?: string;
}

// Participant types
export interface ParticipantDTO {
  id: number;
  userId: number;
  sessionId: number;
  joinedAt: string;
  user?: HostUserDTO;
}

export interface ParticipantInSessionDTO {
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

export const SessionStatus = {
  Active: 'Active' as const,
  Ended: 'Ended' as const
} as const;

export type SessionStatus = typeof SessionStatus[keyof typeof SessionStatus];

export const ParticipantRole = {
  Host: 'Host' as const,
  Member: 'Member' as const
} as const;

export type ParticipantRole = typeof ParticipantRole[keyof typeof ParticipantRole];