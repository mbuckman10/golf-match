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
import type { TeamBetResultsDto, BetConfigurationDto, MatchDetailDto } from '../types';

type ResultTab = 'summary' | 'matchups' | 'players' | 'investments';

const BET_TYPE_LABELS: Record<string, string> = {
  Foursome: 'Foursome',
  Threesome: 'Threesome',
  Fivesome: 'Fivesome',
  BestBall: 'Best Ball',
  Individual: 'Individual',
  Skins: 'Skins',
  IndoTournament: 'Indo Tournament',
  RoundRobin: 'Round Robin',
};

export function BetResultsPage() {
  const { id: matchIdStr, betConfigId: betConfigIdStr } = useParams<{ id: string; betConfigId: string }>();
  const matchId = Number(matchIdStr);
  const betConfigId = Number(betConfigIdStr);
  const navigate = useNavigate();

  const [match, setMatch] = useState<MatchDetailDto | null>(null);
  const [bet, setBet] = useState<BetConfigurationDto | null>(null);
  const [results, setResults] = useState<TeamBetResultsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [tab, setTab] = useState<ResultTab>('summary');
  const backToBetsTabRoute = `/matches/${matchId}/scorecard?tab=bets`;

  const loadData = useCallback(async () => {
    setLoading(true);
    setError(null);

    let loadedMatch: MatchDetailDto | null = null;
    let loadedBet: BetConfigurationDto | null = null;

    try {
      loadedMatch = await matchService.getById(matchId);
      setMatch(loadedMatch);

      loadedBet = await betService.getBet(matchId, betConfigId);
      setBet(loadedBet);

      const loadedResults = await betService.getResults(matchId, betConfigId);
      setResults(loadedResults);
    } catch (e: any) {
      setMatch(loadedMatch);
      setBet(loadedBet);
      setResults(null);

      const apiMessage = e?.response?.data;
      const detail = typeof apiMessage === 'string'
        ? apiMessage
        : apiMessage?.error ?? e?.message;
      setError(detail ? `Failed to load results: ${detail}` : 'Failed to load results');
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
      await betService.saveResults(matchId, betConfigId);
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
  if (!match || !bet) {
    return (
      <div style={{ maxWidth: 900, margin: '0 auto', padding: tokens.spacingVerticalL }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalL }}>
          <Button
            icon={<Dismiss24Regular />}
            appearance="subtle"
            aria-label="Close results"
            onClick={() => navigate(backToBetsTabRoute)}
          />
          <Title2>Results</Title2>
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

  if (!results) {
    return (
      <div style={{ maxWidth: 900, margin: '0 auto', padding: tokens.spacingVerticalL }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalL }}>
          <Button
            icon={<Dismiss24Regular />}
            appearance="subtle"
            aria-label="Close results"
            onClick={() => navigate(backToBetsTabRoute)}
          />
          <Title2>Results</Title2>
        </div>

        <MessageBar intent="error" style={{ marginBottom: tokens.spacingVerticalM }}>
          <MessageBarBody>{error ?? 'No results are available for this bet yet.'}</MessageBarBody>
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
        <Title2>Results</Title2>
        <Badge appearance="outline">
          {(BET_TYPE_LABELS[bet.betType] ?? bet.betType)} — {bet.competitionType}
        </Badge>
      </div>

      {error && (
        <MessageBar intent="error" style={{ marginBottom: tokens.spacingVerticalM }}>
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      )}

      <TabList selectedValue={tab} onTabSelect={(_, d) => setTab(d.value as ResultTab)} style={{ marginBottom: tokens.spacingVerticalM }}>
        <Tab value="summary">Team Summary</Tab>
        <Tab value="matchups">Matchups</Tab>
        <Tab value="players">Per Player</Tab>
        <Tab value="investments">Investments</Tab>
      </TabList>

      {/* Team Summary */}
      {tab === 'summary' && (
        <Card style={{ padding: tokens.spacingVerticalM }}>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHeaderCell>Team</TableHeaderCell>
                <TableHeaderCell>Net Total</TableHeaderCell>
                <TableHeaderCell>Nassau</TableHeaderCell>
                <TableHeaderCell>Investments</TableHeaderCell>
                <TableHeaderCell>Total Strokes</TableHeaderCell>
                <TableHeaderCell>Grand Total</TableHeaderCell>
                <TableHeaderCell>After Expense</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {results.teamResults.map((t) => (
                <TableRow key={t.teamNumber}>
                  <TableCell>
                    <strong>{t.teamName ?? `Team ${t.teamNumber}`}</strong>
                  </TableCell>
                  <TableCell>{t.teamNetTotal}</TableCell>
                  <TableCell style={{ color: fmtColor(t.nassauTotal) }}>{fmt(t.nassauTotal)}</TableCell>
                  <TableCell style={{ color: fmtColor(t.investmentAmount) }}>{fmt(t.investmentAmount)}</TableCell>
                  <TableCell style={{ color: fmtColor(t.totalStrokesTotal) }}>{fmt(t.totalStrokesTotal)}</TableCell>
                  <TableCell style={{ color: fmtColor(t.grandTotal), fontWeight: 'bold' }}>{fmt(t.grandTotal)}</TableCell>
                  <TableCell style={{ color: fmtColor(t.grandTotalAfterExpense), fontWeight: 'bold' }}>
                    {fmt(t.grandTotalAfterExpense)}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Card>
      )}

      {/* Matchups */}
      {tab === 'matchups' &&
        results.matchups.map((m, idx) => {
          const teamA = results.teamResults.find((t) => t.teamNumber === m.teamANumber);
          const teamB = results.teamResults.find((t) => t.teamNumber === m.teamBNumber);
          return (
            <Card key={idx} style={{ marginBottom: tokens.spacingVerticalM, padding: tokens.spacingVerticalM }}>
              <Title2 style={{ fontSize: 16, marginBottom: tokens.spacingVerticalS }}>
                {teamA?.teamName ?? `Team ${m.teamANumber}`} vs {teamB?.teamName ?? `Team ${m.teamBNumber}`}
              </Title2>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: tokens.spacingHorizontalL }}>
                <div>
                  <Body2>Front 9</Body2>
                  <Body1 style={{ color: fmtColor(m.front9Result), fontWeight: 'bold' }}>
                    {m.front9Result > 0 ? `A ${m.front9Result} Up` : m.front9Result < 0 ? `B ${Math.abs(m.front9Result)} Up` : 'Halved'}{' '}
                    ({fmt(m.nassauFrontDollars)})
                  </Body1>
                </div>
                <div>
                  <Body2>Back 9</Body2>
                  <Body1 style={{ color: fmtColor(m.back9Result), fontWeight: 'bold' }}>
                    {m.back9Result > 0 ? `A ${m.back9Result} Up` : m.back9Result < 0 ? `B ${Math.abs(m.back9Result)} Up` : 'Halved'}{' '}
                    ({fmt(m.nassauBackDollars)})
                  </Body1>
                </div>
                <div>
                  <Body2>Overall 18</Body2>
                  <Body1 style={{ color: fmtColor(m.overall18Result), fontWeight: 'bold' }}>
                    {m.overall18Result > 0
                      ? `A ${m.overall18Result} Up`
                      : m.overall18Result < 0
                        ? `B ${Math.abs(m.overall18Result)} Up`
                        : 'Halved'}{' '}
                    ({fmt(m.nassau18Dollars)})
                  </Body1>
                </div>
              </div>

              <Divider style={{ margin: `${tokens.spacingVerticalS} 0` }} />

              {/* Hole-by-hole status bar */}
              <div style={{ display: 'flex', gap: 2, marginTop: tokens.spacingVerticalXS }}>
                {m.holeByHoleStatus.map((status, h) => (
                  <div
                    key={h}
                    style={{
                      width: 24,
                      height: 24,
                      borderRadius: 4,
                      backgroundColor: status > 0 ? 'var(--golf-success)' : status < 0 ? 'var(--golf-danger)' : 'var(--golf-neutral-chip)',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: 10,
                      color: status !== 0 ? tokens.colorNeutralForegroundInverted : 'var(--golf-chip-text)',
                    }}
                    title={`Hole ${h + 1}: ${status > 0 ? `A ${status} Up` : status < 0 ? `B ${Math.abs(status)} Up` : 'AS'}`}
                  >
                    {h + 1}
                  </div>
                ))}
              </div>
            </Card>
          );
        })}

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
              {results.playerResults.map((p) => {
                const team = results.teamResults.find((t) => t.teamNumber === p.teamNumber);
                return (
                  <TableRow key={p.playerId}>
                    <TableCell>{p.playerName}</TableCell>
                    <TableCell>{team?.teamName ?? `Team ${p.teamNumber}`}</TableCell>
                    <TableCell style={{ color: fmtColor(p.winLoss), fontWeight: 'bold' }}>{fmt(p.winLoss)}</TableCell>
                    <TableCell style={{ color: fmtColor(p.winLossAfterExpense), fontWeight: 'bold' }}>
                      {fmt(p.winLossAfterExpense)}
                    </TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </Card>
      )}

      {/* Investments */}
      {tab === 'investments' && (
        <Card style={{ padding: tokens.spacingVerticalM }}>
          {results.teamResults.map((t) => (
            <div key={t.teamNumber} style={{ marginBottom: tokens.spacingVerticalM }}>
              <Body1>
                <strong>{t.teamName ?? `Team ${t.teamNumber}`}</strong> — Offs: {t.totalOffs}, Redemptions: {t.totalRedemptions},{' '}
                <span style={{ color: fmtColor(t.investmentAmount) }}>{fmt(t.investmentAmount)}</span>
              </Body1>
              <div style={{ display: 'flex', gap: 2, marginTop: 4 }}>
                {t.teamHoleScores.map((score, h) => (
                  <div
                    key={h}
                    style={{
                      width: 32,
                      height: 32,
                      borderRadius: 4,
                      backgroundColor: t.isOff[h] ? 'var(--golf-danger)' : t.isRedemption[h] ? 'var(--golf-success)' : tokens.colorNeutralBackground2,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: 11,
                      color: t.isOff[h] || t.isRedemption[h] ? tokens.colorNeutralForegroundInverted : tokens.colorNeutralForeground2,
                    }}
                    title={`Hole ${h + 1}: Team score ${score}${t.isOff[h] ? ' (OFF)' : ''}${t.isRedemption[h] ? ' (REDEMPTION)' : ''}`}
                  >
                    {score || '-'}
                  </div>
                ))}
              </div>
            </div>
          ))}
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
