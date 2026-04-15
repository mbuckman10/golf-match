import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Title2,
  Body1,
  Body2,
  Button,
  Card,
  Spinner,
  tokens,
  Badge,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  MessageBar,
  MessageBarBody,
  Divider,
} from '@fluentui/react-components';
import { ArrowLeft24Regular, Save24Regular } from '@fluentui/react-icons';
import { betService } from '../services/betService';
import { matchService } from '../services/matchService';
import type { BetConfigurationDto, MatchDetailDto, SkinsResultsDto } from '../types';

export function SkinsResultsPage() {
  const { id: matchIdStr, betConfigId: betConfigIdStr } = useParams<{ id: string; betConfigId: string }>();
  const matchId = Number(matchIdStr);
  const betConfigId = Number(betConfigIdStr);
  const navigate = useNavigate();

  const [match, setMatch] = useState<MatchDetailDto | null>(null);
  const [bet, setBet] = useState<BetConfigurationDto | null>(null);
  const [results, setResults] = useState<SkinsResultsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [m, b, r] = await Promise.all([
        matchService.getById(matchId),
        betService.getBet(matchId, betConfigId),
        betService.getSkinsResults(matchId, betConfigId),
      ]);
      setMatch(m);
      setBet(b);
      setResults(r);
    } catch {
      setError('Failed to load skins results');
    } finally {
      setLoading(false);
    }
  }, [matchId, betConfigId]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleSave = async () => {
    setSaving(true);
    try {
      await betService.saveSkinsResults(matchId, betConfigId);
      setError(null);
    } catch {
      setError('Failed to save skins results');
    } finally {
      setSaving(false);
    }
  };

  const fmt = (n: number) => (n >= 0 ? `$${n.toFixed(2)}` : `-$${Math.abs(n).toFixed(2)}`);

  if (loading) return <Spinner label="Loading skins results..." />;
  if (!match || !bet || !results) return <Body1>Not found</Body1>;

  return (
    <div style={{ maxWidth: 1100, margin: '0 auto', padding: tokens.spacingVerticalL }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalL }}>
        <Button icon={<ArrowLeft24Regular />} appearance="subtle" onClick={() => navigate(`/matches/${matchId}/bets`)} />
        <Title2>Skins Results</Title2>
        <Badge appearance="outline">{bet.betType}</Badge>
      </div>

      {error && (
        <MessageBar intent="error" style={{ marginBottom: tokens.spacingVerticalM }}>
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      )}

      <Card style={{ padding: tokens.spacingVerticalM, marginBottom: tokens.spacingVerticalM }}>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: tokens.spacingHorizontalM }}>
          <Body2>Total Pot: <strong>{fmt(results.totalPot)}</strong></Body2>
          <Body2>Awarded Skins: <strong>{results.totalSkinsAwarded}</strong></Body2>
          <Body2>Carry Remaining: <strong>{results.unresolvedCarrySkins}</strong></Body2>
          <Body2>Per Skin: <strong>{fmt(results.amountPerAwardedSkin)}</strong></Body2>
        </div>
      </Card>

      <Card style={{ padding: tokens.spacingVerticalM, marginBottom: tokens.spacingVerticalM }}>
        <Body1 style={{ marginBottom: tokens.spacingVerticalS }}><strong>Player Standings</strong></Body1>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHeaderCell>Player</TableHeaderCell>
              <TableHeaderCell>Skins Won</TableHeaderCell>
              <TableHeaderCell>Gross Winnings</TableHeaderCell>
              <TableHeaderCell>Net Winnings</TableHeaderCell>
            </TableRow>
          </TableHeader>
          <TableBody>
            {results.playerResults.map((p) => (
              <TableRow key={p.playerId}>
                <TableCell>{p.playerName}</TableCell>
                <TableCell>{p.skinsWon}</TableCell>
                <TableCell>{fmt(p.grossWinnings)}</TableCell>
                <TableCell>{fmt(p.netWinnings)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Card>

      <Card style={{ padding: tokens.spacingVerticalM, marginBottom: tokens.spacingVerticalM }}>
        <Body1 style={{ marginBottom: tokens.spacingVerticalS }}><strong>Hole by Hole</strong></Body1>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHeaderCell>Hole</TableHeaderCell>
              <TableHeaderCell>Carry In</TableHeaderCell>
              <TableHeaderCell>Winning Score</TableHeaderCell>
              <TableHeaderCell>Winner</TableHeaderCell>
              <TableHeaderCell>Skins Awarded</TableHeaderCell>
              <TableHeaderCell>Carry Out</TableHeaderCell>
            </TableRow>
          </TableHeader>
          <TableBody>
            {results.holeResults.map((h) => (
              <TableRow key={h.holeNumber}>
                <TableCell>{h.holeNumber}</TableCell>
                <TableCell>{h.carryIn}</TableCell>
                <TableCell>{h.winningScore > 0 ? h.winningScore : '-'}</TableCell>
                <TableCell>{h.winnerPlayerName ?? (h.tiedPlayerIds.length > 1 ? 'Tie' : '-')}</TableCell>
                <TableCell>{h.skinsAwarded}</TableCell>
                <TableCell>{h.carryOut}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Card>

      <Divider style={{ margin: `${tokens.spacingVerticalM} 0` }} />
      <Button appearance="primary" icon={<Save24Regular />} onClick={handleSave} disabled={saving}>
        {saving ? 'Saving...' : 'Save Results'}
      </Button>
    </div>
  );
}
