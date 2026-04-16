import api from './api';
import type { MatchDto, MatchDetailDto, CreateMatchRequest, MatchScoreDto, HoleScoreResultDto } from '../types';

export const matchService = {
  getAll: async (status?: string, from?: string, to?: string): Promise<MatchDto[]> => {
    const params = new URLSearchParams();
    if (status) params.append('status', status);
    if (from) params.append('from', from);
    if (to) params.append('to', to);
    const { data } = await api.get<MatchDto[]>(`/matches?${params}`);
    return data;
  },

  getById: async (id: number): Promise<MatchDetailDto> => {
    const { data } = await api.get<MatchDetailDto>(`/matches/${id}`);
    return data;
  },

  create: async (request: CreateMatchRequest): Promise<MatchDetailDto> => {
    const { data } = await api.post<MatchDetailDto>('/matches', request);
    return data;
  },

  updateStatus: async (id: number, status: string): Promise<void> => {
    await api.put(`/matches/${id}/status`, { status });
  },

  addPlayers: async (matchId: number, playerIds: number[]): Promise<MatchScoreDto[]> => {
    const { data } = await api.post<MatchScoreDto[]>(`/matches/${matchId}/players`, { playerIds });
    return data;
  },

  removePlayer: async (matchId: number, playerId: number): Promise<void> => {
    await api.delete(`/matches/${matchId}/players/${playerId}`);
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/matches/${id}`);
  },

  archive: async (id: number): Promise<void> => {
    await api.put(`/matches/${id}/archive`);
  },

  // Scores
  getScores: async (matchId: number): Promise<MatchScoreDto[]> => {
    const { data } = await api.get<MatchScoreDto[]>(`/matches/${matchId}/scores`);
    return data;
  },

  getPlayerScore: async (matchId: number, playerId: number): Promise<MatchScoreDto> => {
    const { data } = await api.get<MatchScoreDto>(`/matches/${matchId}/scores/${playerId}`);
    return data;
  },

  updateHoleScore: async (matchId: number, playerId: number, holeNumber: number, score: number): Promise<HoleScoreResultDto> => {
    const { data } = await api.post<HoleScoreResultDto>(
      `/matches/${matchId}/scores/${playerId}/hole/${holeNumber}`,
      { holeNumber, score }
    );
    return data;
  },

  bulkUpdateScores: async (matchId: number, playerId: number, holeScores: number[]): Promise<MatchScoreDto> => {
    const { data } = await api.put<MatchScoreDto>(`/matches/${matchId}/scores/${playerId}`, { holeScores });
    return data;
  },
};
