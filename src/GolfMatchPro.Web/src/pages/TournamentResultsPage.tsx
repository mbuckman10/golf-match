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
  Tab,
  TabList,
} from '@fluentui/react-components';
import { Dismiss24Regular, Save24Regular } from '@fluentui/react-icons';
import { betService } from '../services/betService';
import { matchService } from '../services/matchService';
import type {
  BetConfigurationDto,
  MatchDetailDto,
  TournamentResultsDto,
  TournamentDivisionResultDto,
} from '../types';

type DivisionTab = 'leaderboard' | 'gross18' | 'grossFront9' | 'grossBack9' | 'net18' | 'netFront9' | 'netBack9';

export function TournamentResultsPage() {
  const { id: matchIdStr, betConfigId: betConfigIdStr } = useParams<{ id: string; betConfigId: string }>();
  const matchId = Number(matchIdStr);
  const betConfigId = Number(betConfigIdStr);
  const navigate = useNavigate();

  const [match, setMatch] = useState<MatchDetailDto | null>(null);
  const [bet, setBet] = useState<BetConfigurationDto | null>(null);
  const [results, setResults] = useState<TournamentResultsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [tab, setTab] = useState<DivisionTab>('leaderboard');
  const backToBetsTabRoute = `/matches/${matchId}/scorecard?tab=bets`;

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [m, b, r] = await Promise.all([
        matchService.getById(matchId),
        betService.getBet(matchId, betConfigId),
        betService.getTournamentResults(matchId, betConfigId),
      ]);
      setMatch(m);
      setBet(b);
      setResults(r);
    } catch {
      setError('Failed to load tournament results');
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
      await betService.saveTournamentResults(matchId, betConfigId);
      setError(null);
    } catch {
      setError('Failed to save tournament results');
    } finally {
      setSaving(false);
    }
  };

  const fmt = (n: number) => (n >= 0 ? `$${n.toFixed(2)}` : `-$${Math.abs(n).toFixed(2)}`);

  if (loading) return <Spinner label="Loading tournament results..." />;
  if (!match || !bet || !results) {
    return (
      <div style={{ maxWidth: 900, margin: '0 auto', padding: tokens.spacingVerticalL }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalL }}>
          <Button
            icon={<Dismiss24Regular />}
            appearance="subtle"
            aria-label="Close results"
            onClick={() => navigate(backToBetsTabRoute)}
          />
          <Title2>Tournament Results</Title2>
        </div>

        <MessageBar intent="error" style={{ marginBottom: tokens.spacingVerticalM }}>
          <MessageBarBody>{error ?? 'Not found.'}</MessageBarBody>
        </MessageBar>

        <div style={{ display: 'flex', gap: tokens.spacingHorizontalM }}>
          <Button appearance="primary" onClick={loadData}>Retry</Button>
          <Button onClick={() => navigate(backToBetsTabRoute)}>Back to Bets</Button>
        </div>
      </div>
    );
  }

  const renderDivision = (division: TournamentDivisionResultDto) => (
    <Card style={{ padding: tokens.spacingVerticalM }}>
      <Body1 style={{ marginBottom: tokens.spacingVerticalS }}>
        <strong>{division.name}</strong> - Purse: {fmt(division.purse)}
      </Body1>
      <Table>
        <TableHeader>
          <TableRow>
            <TableHeaderCell>Place</TableHeaderCell>
            <TableHeaderCell>Player</TableHeaderCell>
            <TableHeaderCell>Score</TableHeaderCell>
            <TableHeaderCell>Payout</TableHeaderCell>
          </TableRow>
        </TableHeader>
        <TableBody>
          {division.entries.map((e) => (
            <TableRow key={`${division.name}-${e.playerId}-${e.place}`}>
              <TableCell>{e.place}</TableCell>
              <TableCell>{e.playerName}</TableCell>
              <TableCell>{e.score}</TableCell>
              <TableCell>{fmt(e.payout)}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Card>
  );

  return (
    <div style={{ maxWidth: 1100, margin: '0 auto', padding: tokens.spacingVerticalL }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalL }}>
        <Button
          icon={<Dismiss24Regular />}
          appearance="subtle"
          aria-label="Close results"
          onClick={() => navigate(backToBetsTabRoute)}
        />
        <Title2>Tournament Results</Title2>
        <Badge appearance="outline">{bet.betType}</Badge>
      </div>

      {error && (
        <MessageBar intent="error" style={{ marginBottom: tokens.spacingVerticalM }}>
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      )}

      <Card style={{ padding: tokens.spacingVerticalM, marginBottom: tokens.spacingVerticalM }}>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: tokens.spacingHorizontalM }}>
          <Body2>Prize Pool: <strong>{fmt(results.prizePool)}</strong></Body2>
          <Body2>Gross Purse: <strong>{fmt(results.grossPurse)}</strong></Body2>
          <Body2>Net Purse: <strong>{fmt(results.netPurse)}</strong></Body2>
        </div>
      </Card>

      <TabList selectedValue={tab} onTabSelect={(_, d) => setTab(d.value as DivisionTab)} style={{ marginBottom: tokens.spacingVerticalM }}>
        <Tab value="leaderboard">Leaderboard</Tab>
        <Tab value="gross18">Gross 18</Tab>
        <Tab value="grossFront9">Gross Front 9</Tab>
        <Tab value="grossBack9">Gross Back 9</Tab>
        <Tab value="net18">Net 18</Tab>
        <Tab value="netFront9">Net Front 9</Tab>
        <Tab value="netBack9">Net Back 9</Tab>
      </TabList>

      {tab === 'leaderboard' && (
        <Card style={{ padding: tokens.spacingVerticalM, marginBottom: tokens.spacingVerticalM }}>
          <Body1 style={{ marginBottom: tokens.spacingVerticalS }}><strong>Leaderboard</strong></Body1>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHeaderCell>Player</TableHeaderCell>
                <TableHeaderCell>Gross 18</TableHeaderCell>
                <TableHeaderCell>Net 18</TableHeaderCell>
                <TableHeaderCell>Gross Payout</TableHeaderCell>
                <TableHeaderCell>Net Payout</TableHeaderCell>
                <TableHeaderCell>Total</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {results.leaderboard.map((l) => (
                <TableRow key={l.playerId}>
                  <TableCell>{l.playerName}</TableCell>
                  <TableCell>{l.gross18}</TableCell>
                  <TableCell>{l.net18}</TableCell>
                  <TableCell>{fmt(l.grossPayout)}</TableCell>
                  <TableCell>{fmt(l.netPayout)}</TableCell>
                  <TableCell>{fmt(l.totalPayout)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Card>
      )}

      {tab === 'gross18' && renderDivision(results.gross18)}
      {tab === 'grossFront9' && renderDivision(results.grossFront9)}
      {tab === 'grossBack9' && renderDivision(results.grossBack9)}
      {tab === 'net18' && renderDivision(results.net18)}
      {tab === 'netFront9' && renderDivision(results.netFront9)}
      {tab === 'netBack9' && renderDivision(results.netBack9)}

      <div style={{ marginTop: tokens.spacingVerticalM }}>
        <Button appearance="primary" icon={<Save24Regular />} onClick={handleSave} disabled={saving}>
          {saving ? 'Saving...' : 'Save Results'}
        </Button>
      </div>
    </div>
  );
}
