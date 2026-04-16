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
  Tab,
  TabList,
} from '@fluentui/react-components';
import { Dismiss24Regular, Save24Regular } from '@fluentui/react-icons';
import { betService } from '../services/betService';
import { matchService } from '../services/matchService';
import type { BestBallResultsDto, BetConfigurationDto, MatchDetailDto } from '../types';

type ResultTab = 'matchups' | 'players';

export function BestBallResultsPage() {
  const { id: matchIdStr, betConfigId: betConfigIdStr } = useParams<{ id: string; betConfigId: string }>();
  const matchId = Number(matchIdStr);
  const betConfigId = Number(betConfigIdStr);
  const navigate = useNavigate();

  const [match, setMatch] = useState<MatchDetailDto | null>(null);
  const [bet, setBet] = useState<BetConfigurationDto | null>(null);
  const [results, setResults] = useState<BestBallResultsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [tab, setTab] = useState<ResultTab>('matchups');
  const backToBetsTabRoute = `/matches/${matchId}/scorecard?tab=bets`;

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [m, b, r] = await Promise.all([
        matchService.getById(matchId),
        betService.getBet(matchId, betConfigId),
        betService.getBestBallResults(matchId, betConfigId),
      ]);
      setMatch(m);
      setBet(b);
      setResults(r);
    } catch {
      setError('Failed to load results');
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
      await betService.saveBestBallResults(matchId, betConfigId);
      setError(null);
    } catch {
      setError('Failed to save results');
    } finally {
      setSaving(false);
    }
  };

  const fmt = (n: number) => (n >= 0 ? `$${n.toFixed(2)}` : `-$${Math.abs(n).toFixed(2)}`);
  const fmtColor = (n: number) => (n > 0 ? 'var(--golf-success)' : n < 0 ? 'var(--golf-danger)' : undefined);

  if (loading) return <Spinner label="Loading results..." />;
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
          <Title2>Best Ball Results</Title2>
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

  return (
    <div style={{ maxWidth: 1000, margin: '0 auto', padding: tokens.spacingVerticalL }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalL }}>
        <Button
          icon={<Dismiss24Regular />}
          appearance="subtle"
          aria-label="Close results"
          onClick={() => navigate(backToBetsTabRoute)}
        />
        <Title2>Best Ball Results</Title2>
        <Badge appearance="outline">
          {bet.competitionType}
          {bet.autoPressEnabled && ` — Auto Press ${bet.pressDownThreshold}-Down`}
        </Badge>
      </div>

      {error && (
        <MessageBar intent="error" style={{ marginBottom: tokens.spacingVerticalM }}>
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      )}

      <TabList selectedValue={tab} onTabSelect={(_, d) => setTab(d.value as ResultTab)} style={{ marginBottom: tokens.spacingVerticalM }}>
        <Tab value="matchups">Matchups</Tab>
        <Tab value="players">Per Player</Tab>
      </TabList>

      {/* Matchups */}
      {tab === 'matchups' &&
        results.matchups.map((m, idx) => (
          <Card key={idx} style={{ marginBottom: tokens.spacingVerticalM, padding: tokens.spacingVerticalM }}>
            <Title2 style={{ fontSize: 16, marginBottom: tokens.spacingVerticalS }}>
              {m.sheetHangerTeamName ?? `Team ${m.sheetHangerTeamNumber}`} (SH) vs{' '}
              {m.opponentTeamName ?? `Team ${m.opponentTeamNumber}`}
            </Title2>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))', gap: `${tokens.spacingVerticalM} ${tokens.spacingHorizontalXL}` }}>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
                <Body2>Front 9</Body2>
                <Body1 style={{ color: fmtColor(m.front9Result), fontWeight: 'bold' }}>
                  {m.front9Result > 0 ? `SH ${m.front9Result} Up` : m.front9Result < 0 ? `Opp ${Math.abs(m.front9Result)} Up` : 'Halved'}{' '}
                  ({fmt(m.nassauFrontDollars)})
                </Body1>
              </div>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
                <Body2>Back 9</Body2>
                <Body1 style={{ color: fmtColor(m.back9Result), fontWeight: 'bold' }}>
                  {m.back9Result > 0 ? `SH ${m.back9Result} Up` : m.back9Result < 0 ? `Opp ${Math.abs(m.back9Result)} Up` : 'Halved'}{' '}
                  ({fmt(m.nassauBackDollars)})
                </Body1>
              </div>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
                <Body2>Overall 18</Body2>
                <Body1 style={{ color: fmtColor(m.overall18Result), fontWeight: 'bold' }}>
                  {m.overall18Result > 0
                    ? `SH ${m.overall18Result} Up`
                    : m.overall18Result < 0
                      ? `Opp ${Math.abs(m.overall18Result)} Up`
                      : 'Halved'}{' '}
                  ({fmt(m.nassau18Dollars)})
                </Body1>
              </div>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
                <Body2>Total (w/ Presses)</Body2>
                <Body1 style={{ color: fmtColor(m.totalAmountSheetHanger), fontWeight: 'bold' }}>
                  {fmt(m.totalAmountSheetHanger)}
                </Body1>
              </div>
            </div>

            {m.presses.length > 0 && (
              <>
                <Divider style={{ margin: `${tokens.spacingVerticalS} 0` }} />
                <Body2 style={{ marginBottom: 4 }}>Presses</Body2>
                <div style={{ display: 'flex', gap: tokens.spacingHorizontalS, flexWrap: 'wrap' }}>
                  {m.presses.map((p, pi) => (
                    <Badge key={pi} appearance="outline" color={p.amount > 0 ? 'success' : p.amount < 0 ? 'danger' : 'informative'}>
                      Holes {p.startHole}–{p.endHole}: {fmt(p.amount)}
                    </Badge>
                  ))}
                </div>
              </>
            )}

            <Divider style={{ margin: `${tokens.spacingVerticalS} 0` }} />

            {/* Hole-by-hole best ball comparison */}
            <div style={{ display: 'flex', gap: 2, marginBottom: 4 }}>
              <Body2 style={{ width: 50 }}>SH</Body2>
              {m.sheetHangerBestBall.map((score, h) => (
                <div
                  key={h}
                  style={{
                    width: 24,
                    height: 24,
                    borderRadius: 4,
                    backgroundColor:
                      m.holeByHoleStatus[h] > 0
                        ? 'var(--golf-success)'
                        : m.holeByHoleStatus[h] < 0
                          ? 'var(--golf-danger)'
                          : 'var(--golf-neutral-chip)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: 10,
                    color: m.holeByHoleStatus[h] !== 0 ? tokens.colorNeutralForegroundInverted : 'var(--golf-chip-text)',
                  }}
                  title={`Hole ${h + 1}: SH ${score} vs Opp ${m.opponentBestBall[h]}`}
                >
                  {score || '-'}
                </div>
              ))}
            </div>
            <div style={{ display: 'flex', gap: 2 }}>
              <Body2 style={{ width: 50 }}>Opp</Body2>
              {m.opponentBestBall.map((score, h) => (
                <div
                  key={h}
                  style={{
                    width: 24,
                    height: 24,
                    borderRadius: 4,
                    backgroundColor: tokens.colorNeutralBackground2,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: 10,
                    color: tokens.colorNeutralForeground2,
                  }}
                  title={`Hole ${h + 1}`}
                >
                  {score || '-'}
                </div>
              ))}
            </div>
          </Card>
        ))}

      {/* Per Player */}
      {tab === 'players' && (
        <Card style={{ padding: tokens.spacingVerticalM }}>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHeaderCell>Player</TableHeaderCell>
                <TableHeaderCell>Team</TableHeaderCell>
                <TableHeaderCell>Win/Loss</TableHeaderCell>
                <TableHeaderCell>After Expense</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {results.playerResults.map((p) => (
                <TableRow key={`${p.playerId}-${p.teamNumber}`}>
                  <TableCell>{p.playerName}</TableCell>
                  <TableCell>Team {p.teamNumber}</TableCell>
                  <TableCell style={{ color: fmtColor(p.winLoss), fontWeight: 'bold' }}>{fmt(p.winLoss)}</TableCell>
                  <TableCell style={{ color: fmtColor(p.winLossAfterExpense), fontWeight: 'bold' }}>
                    {fmt(p.winLossAfterExpense)}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Card>
      )}

      <div style={{ display: 'flex', gap: tokens.spacingHorizontalM, marginTop: tokens.spacingVerticalL }}>
        <Button appearance="primary" icon={<Save24Regular />} onClick={handleSave} disabled={saving}>
          {saving ? 'Saving...' : 'Save Results to DB'}
        </Button>
        <Button onClick={loadData}>Refresh</Button>
      </div>
    </div>
  );
}
