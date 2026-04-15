import * as signalR from '@microsoft/signalr';

let connection: signalR.HubConnection | null = null;

export function getConnection(): signalR.HubConnection {
  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/match')
      .withAutomaticReconnect()
      .build();
  }
  return connection;
}

export async function startConnection(): Promise<void> {
  const conn = getConnection();
  if (conn.state === signalR.HubConnectionState.Disconnected) {
    await conn.start();
  }
}

export async function joinMatch(matchId: number): Promise<void> {
  const conn = getConnection();
  await conn.invoke('JoinMatch', matchId);
}

export async function leaveMatch(matchId: number): Promise<void> {
  const conn = getConnection();
  await conn.invoke('LeaveMatch', matchId);
}

export function onScoreUpdated(
  callback: (matchId: number, playerId: number, holeNumber: number, score: number) => void
): void {
  const conn = getConnection();
  conn.on('ScoreUpdated', callback);
}

export function offScoreUpdated(
  callback: (matchId: number, playerId: number, holeNumber: number, score: number) => void
): void {
  const conn = getConnection();
  conn.off('ScoreUpdated', callback);
}

export function onMatchStatusChanged(
  callback: (matchId: number, newStatus: string) => void
): void {
  const conn = getConnection();
  conn.on('MatchStatusChanged', callback);
}
