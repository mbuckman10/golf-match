import api from './api';
import type { PlayerDto, CreatePlayerRequest, UpdatePlayerRequest } from '../types';

export const playerService = {
  getAll: (search?: string) =>
    api.get<PlayerDto[]>('/players', { params: search ? { search } : {} }).then(r => r.data),

  getById: (id: number) => api.get<PlayerDto>(`/players/${id}`).then(r => r.data),

  create: (request: CreatePlayerRequest) =>
    api.post<PlayerDto>('/players', request).then(r => r.data),

  update: (id: number, request: UpdatePlayerRequest) =>
    api.put<PlayerDto>(`/players/${id}`, request).then(r => r.data),
};
