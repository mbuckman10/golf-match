import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Title2,
  Body1,
  Button,
  Card,
  Spinner,
  tokens,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  MessageBar,
  MessageBarBody,
} from '@fluentui/react-components';
import { ArrowLeft24Regular } from '@fluentui/react-icons';
import { betService } from '../services/betService';
import type { BestBallWinLossSummaryDto } from '../types';

export function BestBallSummaryPage() {
  const { id: matchIdStr } = useParams<{ id: string }>();
  const matchId = Number(matchIdStr);
  const navigate = useNavigate();

  const [summary, setSummary] = useState<BestBallWinLossSummaryDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const data = await betService.getBestBallSummary(matchId);
      setSummary(data);
    } catch {
      setError('Failed to load Best Ball summary');
    } finally {
      setLoading(false);
    }
  }, [matchId]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const fmt = (n: number) => (n >= 0 ? `$${n.toFixed(2)}` : `-$${Math.abs(n).toFixed(2)}`);
  const fmtColor = (n: number) => (n > 0 ? '#107c10' : n < 0 ? '#d13438' : undefined);

  if (loading) return <Spinner label="Loading summary..." />;

  return (
    <div style={{ maxWidth: 1000, margin: '0 auto', padding: tokens.spacingVerticalL }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalL }}>
        <Button icon={<ArrowLeft24Regular />} appearance="subtle" onClick={() => navigate(`/matches/${matchId}/bets`)} />
        <Title2>Best Ball W/L Summary</Title2>
      </div>

      {error && (
        <MessageBar intent="error" style={{ marginBottom: tokens.spacingVerticalM }}>
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      )}

      {summary && summary.playerSummaries.length > 0 ? (
        <Card style={{ padding: tokens.spacingVerticalM }}>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHeaderCell>Player</TableHeaderCell>
                <TableHeaderCell>Played</TableHeaderCell>
                <TableHeaderCell>Won</TableHeaderCell>
                <TableHeaderCell>Lost</TableHeaderCell>
                <TableHeaderCell>Tied</TableHeaderCell>
                <TableHeaderCell>Win/Loss</TableHeaderCell>
                <TableHeaderCell>After Expense</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {summary.playerSummaries.map((p) => (
                <TableRow key={p.playerId}>
                  <TableCell><strong>{p.playerName}</strong></TableCell>
                  <TableCell>{p.matchupsPlayed}</TableCell>
                  <TableCell>{p.matchupsWon}</TableCell>
                  <TableCell>{p.matchupsLost}</TableCell>
                  <TableCell>{p.matchupsTied}</TableCell>
                  <TableCell style={{ color: fmtColor(p.totalWinLoss), fontWeight: 'bold' }}>{fmt(p.totalWinLoss)}</TableCell>
                  <TableCell style={{ color: fmtColor(p.totalWinLossAfterExpense), fontWeight: 'bold' }}>
                    {fmt(p.totalWinLossAfterExpense)}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Card>
      ) : (
        <Body1>No Best Ball bets configured for this match.</Body1>
      )}

      <div style={{ marginTop: tokens.spacingVerticalL }}>
        <Button onClick={loadData}>Refresh</Button>
      </div>
    </div>
  );
}
