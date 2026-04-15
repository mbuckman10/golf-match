import api from './api';
import type {
  BetConfigurationDto,
  CreateBetConfigurationRequest,
  TeamDto,
  CreateTeamRequest,
  TeamBetResultsDto,
  IndividualBetResultsDto,
  BestBallResultsDto,
  BestBallWinLossSummaryDto,
} from '../types';

export const betService = {
  // Bet configurations
  getBets: async (matchId: number): Promise<BetConfigurationDto[]> => {
    const { data } = await api.get<BetConfigurationDto[]>(`/matches/${matchId}/bets`);
    return data;
  },

  getBet: async (matchId: number, betConfigId: number): Promise<BetConfigurationDto> => {
    const { data } = await api.get<BetConfigurationDto>(`/matches/${matchId}/bets/${betConfigId}`);
    return data;
  },

  createBet: async (matchId: number, request: CreateBetConfigurationRequest): Promise<BetConfigurationDto> => {
    const { data } = await api.post<BetConfigurationDto>(`/matches/${matchId}/bets`, request);
    return data;
  },

  updateBet: async (matchId: number, betConfigId: number, request: CreateBetConfigurationRequest): Promise<BetConfigurationDto> => {
    const { data } = await api.put<BetConfigurationDto>(`/matches/${matchId}/bets/${betConfigId}`, request);
    return data;
  },

  deleteBet: async (matchId: number, betConfigId: number): Promise<void> => {
    await api.delete(`/matches/${matchId}/bets/${betConfigId}`);
  },

  // Teams
  getTeams: async (matchId: number, betConfigId: number): Promise<TeamDto[]> => {
    const { data } = await api.get<TeamDto[]>(`/matches/${matchId}/bets/${betConfigId}/teams`);
    return data;
  },

  createTeam: async (matchId: number, betConfigId: number, request: CreateTeamRequest): Promise<TeamDto> => {
    const { data } = await api.post<TeamDto>(`/matches/${matchId}/bets/${betConfigId}/teams`, request);
    return data;
  },

  updateTeam: async (matchId: number, betConfigId: number, teamId: number, request: CreateTeamRequest): Promise<TeamDto> => {
    const { data } = await api.put<TeamDto>(`/matches/${matchId}/bets/${betConfigId}/teams/${teamId}`, request);
    return data;
  },

  deleteTeam: async (matchId: number, betConfigId: number, teamId: number): Promise<void> => {
    await api.delete(`/matches/${matchId}/bets/${betConfigId}/teams/${teamId}`);
  },

  // Results
  getResults: async (matchId: number, betConfigId: number): Promise<TeamBetResultsDto> => {
    const { data } = await api.get<TeamBetResultsDto>(`/matches/${matchId}/bets/${betConfigId}/results`);
    return data;
  },

  saveResults: async (matchId: number, betConfigId: number): Promise<void> => {
    await api.post(`/matches/${matchId}/bets/${betConfigId}/results/save`);
  },

  // Individual results
  getIndividualResults: async (matchId: number, betConfigId: number): Promise<IndividualBetResultsDto> => {
    const { data } = await api.get<IndividualBetResultsDto>(`/matches/${matchId}/bets/${betConfigId}/individual-results`);
    return data;
  },

  saveIndividualResults: async (matchId: number, betConfigId: number): Promise<void> => {
    await api.post(`/matches/${matchId}/bets/${betConfigId}/individual-results/save`);
  },

  // Best Ball results
  getBestBallResults: async (matchId: number, betConfigId: number): Promise<BestBallResultsDto> => {
    const { data } = await api.get<BestBallResultsDto>(`/matches/${matchId}/bets/${betConfigId}/bestball-results`);
    return data;
  },

  saveBestBallResults: async (matchId: number, betConfigId: number): Promise<void> => {
    await api.post(`/matches/${matchId}/bets/${betConfigId}/bestball-results/save`);
  },

  // Best Ball W-L summary
  getBestBallSummary: async (matchId: number): Promise<BestBallWinLossSummaryDto> => {
    const { data } = await api.get<BestBallWinLossSummaryDto>(`/matches/${matchId}/bestball-summary`);
    return data;
  },
};
