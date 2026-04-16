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
import { ArrowLeft24Regular, Save24Regular } from '@fluentui/react-icons';
import { betService } from '../services/betService';
import { matchService } from '../services/matchService';
import type { IndividualBetResultsDto, BetConfigurationDto, MatchDetailDto } from '../types';

type ResultTab = 'matchups' | 'players' | 'presses';

export function IndividualResultsPage() {
  const { id: matchIdStr, betConfigId: betConfigIdStr } = useParams<{ id: string; betConfigId: string }>();
  const matchId = Number(matchIdStr);
  const betConfigId = Number(betConfigIdStr);
  const navigate = useNavigate();

  const [match, setMatch] = useState<MatchDetailDto | null>(null);
  const [bet, setBet] = useState<BetConfigurationDto | null>(null);
  const [results, setResults] = useState<IndividualBetResultsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [tab, setTab] = useState<ResultTab>('matchups');

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [m, b, r] = await Promise.all([
        matchService.getById(matchId),
        betService.getBet(matchId, betConfigId),
        betService.getIndividualResults(matchId, betConfigId),
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
      await betService.saveIndividualResults(matchId, betConfigId);
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
  if (!match || !bet || !results) return <Body1>Not found</Body1>;

  return (
    <div style={{ maxWidth: 1000, margin: '0 auto', padding: tokens.spacingVerticalL }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalL }}>
        <Button icon={<ArrowLeft24Regular />} appearance="subtle" onClick={() => navigate(`/matches/${matchId}/bets`)} />
        <Title2>Individual Bet Results</Title2>
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
        {bet.autoPressEnabled && <Tab value="presses">Presses</Tab>}
      </TabList>

      {/* Matchups */}
      {tab === 'matchups' &&
        results.matchups.map((m, idx) => (
          <Card key={idx} style={{ marginBottom: tokens.spacingVerticalM, padding: tokens.spacingVerticalM }}>
            <Title2 style={{ fontSize: 16, marginBottom: tokens.spacingVerticalS }}>
              {m.playerAName} vs {m.playerBName}
            </Title2>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr 1fr', gap: tokens.spacingHorizontalL }}>
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
              <div>
                <Body2>Total (w/ Presses)</Body2>
                <Body1 style={{ color: fmtColor(m.totalAmountPlayerA), fontWeight: 'bold' }}>
                  {fmt(m.totalAmountPlayerA)}
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

            {/* Hole-by-hole status bar */}
            <div style={{ display: 'flex', gap: 2 }}>
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
        ))}

      {/* Per Player */}
      {tab === 'players' && (
        <Card style={{ padding: tokens.spacingVerticalM }}>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHeaderCell>Player</TableHeaderCell>
                <TableHeaderCell>Win/Loss</TableHeaderCell>
                <TableHeaderCell>After Expense</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {results.playerResults.map((p) => (
                <TableRow key={p.playerId}>
                  <TableCell>{p.playerName}</TableCell>
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

      {/* Presses detail */}
      {tab === 'presses' && (
        <Card style={{ padding: tokens.spacingVerticalM }}>
          {results.matchups.map((m, idx) => (
            <div key={idx} style={{ marginBottom: tokens.spacingVerticalM }}>
              <Body1 style={{ fontWeight: 'bold' }}>
                {m.playerAName} vs {m.playerBName}
              </Body1>
              {m.presses.length === 0 ? (
                <Body2>No presses triggered</Body2>
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHeaderCell>Start</TableHeaderCell>
                      <TableHeaderCell>End</TableHeaderCell>
                      <TableHeaderCell>Result</TableHeaderCell>
                      <TableHeaderCell>Amount</TableHeaderCell>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {m.presses.map((p, pi) => (
                      <TableRow key={pi}>
                        <TableCell>Hole {p.startHole}</TableCell>
                        <TableCell>Hole {p.endHole}</TableCell>
                        <TableCell>
                          {p.result > 0 ? `A wins` : p.result < 0 ? `B wins` : 'Push'}
                        </TableCell>
                        <TableCell style={{ color: fmtColor(p.amount), fontWeight: 'bold' }}>{fmt(p.amount)}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
              <Body2 style={{ marginTop: 4, color: fmtColor(m.totalPressAmount) }}>
                Total Press: {fmt(m.totalPressAmount)}
              </Body2>
              <Divider style={{ marginTop: tokens.spacingVerticalS }} />
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
