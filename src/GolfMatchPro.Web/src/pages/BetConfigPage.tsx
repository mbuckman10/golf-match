import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Title2,
  Body1,
  Button,
  Card,
  CardHeader,
  Input,
  Label,
  Select,
  Switch,
  Spinner,
  tokens,
  Divider,
  Badge,
  MessageBar,
  MessageBarBody,
} from '@fluentui/react-components';
import { Add24Regular, Delete24Regular, ArrowLeft24Regular, Save24Regular } from '@fluentui/react-icons';
import { betService } from '../services/betService';
import { matchService } from '../services/matchService';
import type {
  BetConfigurationDto,
  CreateBetConfigurationRequest,
  MatchDetailDto,
  MatchScoreDto,
  BetType,
  CompetitionType,
  TeamPosition,
  CreateTeamRequest,
  TournamentConfigJsonDto,
} from '../types';

const BET_TYPES: BetType[] = ['Foursome', 'Threesome', 'Fivesome', 'BestBall', 'Individual', 'Skins', 'IndoTournament'];
const COMPETITION_TYPES: CompetitionType[] = ['MatchPlay', 'MedalPlay'];
const TEAM_POSITIONS: TeamPosition[] = ['Captain', 'B', 'C', 'D', 'E'];

const DEFAULT_COUNTS: Record<BetType, number> = {
  Foursome: 2,
  Threesome: 2,
  Fivesome: 3,
  BestBall: 1,
  Individual: 1,
  Skins: 1,
  IndoTournament: 1,
  RoundRobin: 1,
};

const TEAM_SIZES: Record<BetType, number> = {
  Foursome: 4,
  Threesome: 3,
  Fivesome: 5,
  BestBall: 2,
  Individual: 1,
  Skins: 1,
  IndoTournament: 1,
  RoundRobin: 1,
};

export function BetConfigPage() {
  const { id: matchIdStr } = useParams<{ id: string }>();
  const matchId = Number(matchIdStr);
  const navigate = useNavigate();

  const [match, setMatch] = useState<MatchDetailDto | null>(null);
  const [bets, setBets] = useState<BetConfigurationDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Form state for new/editing bet
  const [editingBetId, setEditingBetId] = useState<number | null>(null);
  const [form, setForm] = useState<CreateBetConfigurationRequest>({
    betType: 'Foursome',
    competitionType: 'MatchPlay',
    handicapPercentage: 100,
    nassauFront: 5,
    nassauBack: 5,
    nassau18: 5,
    totalStrokesBetPerStroke: null,
    maxNetScore: null,
    investmentOffEnabled: false,
    investmentOffAmount: 6,
    redemptionEnabled: false,
    redemptionAmount: 4,
    dunnEnabled: false,
    dunnAmount: 2,
    autoPressEnabled: false,
    pressAmount: 5,
    pressDownThreshold: 2,
    skinsBuyIn: null,
    skinsPerSkinAmount: null,
    expenseDeductionPct: 10,
    scoresCountingPerHole: 2,
    configJson: null,
  });

  const [skinsUseNetScores, setSkinsUseNetScores] = useState(true);
  const [tournamentConfig, setTournamentConfig] = useState<TournamentConfigJsonDto>({
    sponsorMoney: 0,
    buyInPerPlayer: 20,
    grossPursePercent: 50,
    netPursePercent: 50,
    eighteenHolePercent: 60,
    frontNinePercent: 20,
    backNinePercent: 20,
    placePayouts: [
      { place: 1, percent: 50 },
      { place: 2, percent: 30 },
      { place: 3, percent: 20 },
    ],
  });

  // Team assignment state
  const [teamAssignments, setTeamAssignments] = useState<
    { teamNumber: number; teamName: string; players: { playerId: number; position: TeamPosition }[] }[]
  >([]);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [m, b] = await Promise.all([matchService.getById(matchId), betService.getBets(matchId)]);
      setMatch(m);
      setBets(b);
    } catch {
      setError('Failed to load data');
    } finally {
      setLoading(false);
    }
  }, [matchId]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleFormChange = (field: keyof CreateBetConfigurationRequest, value: unknown) => {
    setForm((prev) => {
      const next = { ...prev, [field]: value };
      if (field === 'betType') {
        next.scoresCountingPerHole = DEFAULT_COUNTS[value as BetType] ?? 2;
      }
      return next;
    });
  };

  const initTeams = (betType: BetType, players: MatchScoreDto[]) => {
    const teamSize = TEAM_SIZES[betType] ?? 4;
    const numTeams = Math.max(2, Math.floor(players.length / teamSize));
    const teams = [];
    for (let i = 0; i < numTeams; i++) {
      const teamPlayers = players.slice(i * teamSize, (i + 1) * teamSize).map((p, idx) => ({
        playerId: p.playerId,
        position: TEAM_POSITIONS[idx] ?? ('Captain' as TeamPosition),
      }));
      teams.push({
        teamNumber: i + 1,
        teamName: `Team ${i + 1}`,
        players: teamPlayers,
      });
    }
    setTeamAssignments(teams);
  };

  const requiresTeams = (betType: BetType) =>
    betType !== 'Skins' && betType !== 'IndoTournament';

  const handleSaveBet = async () => {
    setSaving(true);
    setError(null);
    try {
      const request: CreateBetConfigurationRequest = {
        ...form,
        configJson:
          form.betType === 'Skins'
            ? JSON.stringify({ useNetScores: skinsUseNetScores })
            : form.betType === 'IndoTournament'
              ? JSON.stringify(tournamentConfig)
              : form.configJson,
      };

      let bet: BetConfigurationDto;
      if (editingBetId) {
        bet = await betService.updateBet(matchId, editingBetId, request);
      } else {
        bet = await betService.createBet(matchId, request);
      }

      if (requiresTeams(form.betType)) {
        // Save teams
        // First delete existing teams
        for (const existingTeam of bet.teams) {
          await betService.deleteTeam(matchId, bet.betConfigId, existingTeam.teamId);
        }
        // Create new teams
        for (const ta of teamAssignments) {
          const teamReq: CreateTeamRequest = {
            teamNumber: ta.teamNumber,
            teamName: ta.teamName,
            players: ta.players,
          };
          await betService.createTeam(matchId, bet.betConfigId, teamReq);
        }
      }

      setSuccess('Bet configuration saved!');
      setEditingBetId(null);
      await loadData();
    } catch {
      setError('Failed to save bet configuration');
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteBet = async (betConfigId: number) => {
    try {
      await betService.deleteBet(matchId, betConfigId);
      await loadData();
    } catch {
      setError('Failed to delete bet');
    }
  };

  const handleEditBet = (bet: BetConfigurationDto) => {
    setEditingBetId(bet.betConfigId);
    setForm({
      betType: bet.betType,
      competitionType: bet.competitionType,
      handicapPercentage: bet.handicapPercentage,
      nassauFront: bet.nassauFront,
      nassauBack: bet.nassauBack,
      nassau18: bet.nassau18,
      totalStrokesBetPerStroke: bet.totalStrokesBetPerStroke,
      maxNetScore: bet.maxNetScore,
      investmentOffEnabled: bet.investmentOffEnabled,
      investmentOffAmount: bet.investmentOffAmount,
      redemptionEnabled: bet.redemptionEnabled,
      redemptionAmount: bet.redemptionAmount,
      dunnEnabled: bet.dunnEnabled,
      dunnAmount: bet.dunnAmount,
      autoPressEnabled: bet.autoPressEnabled,
      pressAmount: bet.pressAmount,
      pressDownThreshold: bet.pressDownThreshold,
      skinsBuyIn: bet.skinsBuyIn,
      skinsPerSkinAmount: bet.skinsPerSkinAmount,
      expenseDeductionPct: bet.expenseDeductionPct,
      scoresCountingPerHole: bet.scoresCountingPerHole,
      configJson: bet.configJson,
    });
    setTeamAssignments(
      bet.teams.map((t) => ({
        teamNumber: t.teamNumber,
        teamName: t.teamName ?? '',
        players: t.players.map((p) => ({ playerId: p.playerId, position: p.position })),
      }))
    );

    if (bet.betType === 'Skins') {
      try {
        const parsed = bet.configJson ? JSON.parse(bet.configJson) : null;
        setSkinsUseNetScores(parsed?.useNetScores ?? true);
      } catch {
        setSkinsUseNetScores(true);
      }
    }

    if (bet.betType === 'IndoTournament') {
      try {
        const parsed = bet.configJson ? JSON.parse(bet.configJson) : null;
        setTournamentConfig({
          sponsorMoney: parsed?.sponsorMoney ?? 0,
          buyInPerPlayer: parsed?.buyInPerPlayer ?? 20,
          grossPursePercent: parsed?.grossPursePercent ?? 50,
          netPursePercent: parsed?.netPursePercent ?? 50,
          eighteenHolePercent: parsed?.eighteenHolePercent ?? 60,
          frontNinePercent: parsed?.frontNinePercent ?? 20,
          backNinePercent: parsed?.backNinePercent ?? 20,
          placePayouts: parsed?.placePayouts ?? [
            { place: 1, percent: 50 },
            { place: 2, percent: 30 },
            { place: 3, percent: 20 },
          ],
        });
      } catch {
        setTournamentConfig({
          sponsorMoney: 0,
          buyInPerPlayer: 20,
          grossPursePercent: 50,
          netPursePercent: 50,
          eighteenHolePercent: 60,
          frontNinePercent: 20,
          backNinePercent: 20,
          placePayouts: [
            { place: 1, percent: 50 },
            { place: 2, percent: 30 },
            { place: 3, percent: 20 },
          ],
        });
      }
    }
  };

  const handleNewBet = () => {
    setEditingBetId(null);
    setForm({
      betType: 'Foursome',
      competitionType: 'MatchPlay',
      handicapPercentage: 100,
      nassauFront: 5,
      nassauBack: 5,
      nassau18: 5,
      totalStrokesBetPerStroke: null,
      maxNetScore: null,
      investmentOffEnabled: false,
      investmentOffAmount: 6,
      redemptionEnabled: false,
      redemptionAmount: 4,
      dunnEnabled: false,
      dunnAmount: 2,
      autoPressEnabled: false,
      pressAmount: 5,
      pressDownThreshold: 2,
      skinsBuyIn: null,
      skinsPerSkinAmount: null,
      expenseDeductionPct: 10,
      scoresCountingPerHole: 2,
      configJson: null,
    });
    if (match) {
      initTeams('Foursome', match.scores);
    }
    setSkinsUseNetScores(true);
    setTournamentConfig({
      sponsorMoney: 0,
      buyInPerPlayer: 20,
      grossPursePercent: 50,
      netPursePercent: 50,
      eighteenHolePercent: 60,
      frontNinePercent: 20,
      backNinePercent: 20,
      placePayouts: [
        { place: 1, percent: 50 },
        { place: 2, percent: 30 },
        { place: 3, percent: 20 },
      ],
    });
  };

  const updateTeamPlayer = (teamIdx: number, playerIdx: number, playerId: number) => {
    setTeamAssignments((prev) => {
      const next = prev.map((t) => ({ ...t, players: [...t.players] }));
      next[teamIdx].players[playerIdx] = { ...next[teamIdx].players[playerIdx], playerId };
      return next;
    });
  };

  const addTeam = () => {
    setTeamAssignments((prev) => [
      ...prev,
      {
        teamNumber: prev.length + 1,
        teamName: `Team ${prev.length + 1}`,
        players: [],
      },
    ]);
  };

  const removeTeam = (idx: number) => {
    setTeamAssignments((prev) => prev.filter((_, i) => i !== idx).map((t, i) => ({ ...t, teamNumber: i + 1 })));
  };

  const addPlayerToTeam = (teamIdx: number) => {
    setTeamAssignments((prev) => {
      const next = [...prev];
      next[teamIdx] = {
        ...next[teamIdx],
        players: [...next[teamIdx].players, { playerId: 0, position: TEAM_POSITIONS[next[teamIdx].players.length] ?? 'Captain' }],
      };
      return next;
    });
  };

  const removePlayerFromTeam = (teamIdx: number, playerIdx: number) => {
    setTeamAssignments((prev) => {
      const next = [...prev];
      next[teamIdx] = {
        ...next[teamIdx],
        players: next[teamIdx].players.filter((_, i) => i !== playerIdx),
      };
      return next;
    });
  };

  if (loading) return <Spinner label="Loading..." />;
  if (!match) return <Body1>Match not found</Body1>;

  const showForm = editingBetId !== null || editingBetId === null;

  return (
    <div style={{ maxWidth: 900, margin: '0 auto', padding: tokens.spacingVerticalL }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalL }}>
        <Button icon={<ArrowLeft24Regular />} appearance="subtle" onClick={() => navigate(`/matches/${matchId}`)} />
        <Title2>Bet Configuration</Title2>
        <Badge appearance="outline">{match.scores.length} players</Badge>
      </div>

      {error && (
        <MessageBar intent="error" style={{ marginBottom: tokens.spacingVerticalM }}>
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      )}
      {success && (
        <MessageBar intent="success" style={{ marginBottom: tokens.spacingVerticalM }}>
          <MessageBarBody>{success}</MessageBarBody>
        </MessageBar>
      )}

      {/* Existing bets */}
      {bets.length > 0 && (
        <div style={{ marginBottom: tokens.spacingVerticalL }}>
          <Title2 style={{ fontSize: 18, marginBottom: tokens.spacingVerticalS }}>Existing Bets</Title2>
          {bets.map((bet) => (
            <Card key={bet.betConfigId} style={{ marginBottom: tokens.spacingVerticalS }}>
              <CardHeader
                header={
                  <Body1>
                    <strong>
                      {bet.betType} — {bet.competitionType}
                    </strong>{' '}
                    | Nassau: ${bet.nassauFront}/${bet.nassauBack}/${bet.nassau18} | {bet.teams.length} teams
                  </Body1>
                }
                action={
                  <div style={{ display: 'flex', gap: tokens.spacingHorizontalS }}>
                    <Button size="small" onClick={() => handleEditBet(bet)}>
                      Edit
                    </Button>
                    <Button
                      size="small"
                      appearance="primary"
                      onClick={() => {
                        const base = `/matches/${matchId}/bets/${bet.betConfigId}`;
                        if (bet.betType === 'Individual') navigate(`${base}/individual-results`);
                        else if (bet.betType === 'BestBall') navigate(`${base}/bestball-results`);
                        else if (bet.betType === 'Skins') navigate(`${base}/skins-results`);
                        else if (bet.betType === 'IndoTournament') navigate(`${base}/tournament-results`);
                        else navigate(`${base}/results`);
                      }}
                    >
                      Results
                    </Button>
                    <Button size="small" icon={<Delete24Regular />} onClick={() => handleDeleteBet(bet.betConfigId)} />
                  </div>
                }
              />
            </Card>
          ))}
        </div>
      )}

      <div style={{ display: 'flex', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalM }}>
        <Button appearance="primary" icon={<Add24Regular />} onClick={handleNewBet}>
          New Bet
        </Button>
        {bets.some((b) => b.betType === 'BestBall') && (
          <Button appearance="secondary" onClick={() => navigate(`/matches/${matchId}/bestball-summary`)}>
            BB W/L Summary
          </Button>
        )}
      </div>

      {showForm && (
        <Card style={{ padding: tokens.spacingVerticalL }}>
          <Title2 style={{ fontSize: 18, marginBottom: tokens.spacingVerticalM }}>
            {editingBetId ? 'Edit Bet' : 'New Bet'}
          </Title2>

          {/* Row 1: Type + Competition */}
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalM }}>
            <div>
              <Label>Bet Type</Label>
              <Select value={form.betType} onChange={(_, d) => handleFormChange('betType', d.value as BetType)}>
                {BET_TYPES.map((t) => (
                  <option key={t} value={t}>
                    {t}
                  </option>
                ))}
              </Select>
            </div>
            <div>
              <Label>Competition Type</Label>
              <Select value={form.competitionType} onChange={(_, d) => handleFormChange('competitionType', d.value as CompetitionType)}>
                {COMPETITION_TYPES.map((t) => (
                  <option key={t} value={t}>
                    {t}
                  </option>
                ))}
              </Select>
            </div>
          </div>

          {/* Nassau */}
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr 1fr', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalM }}>
            <div>
              <Label>Front 9 ($)</Label>
              <Input type="number" value={String(form.nassauFront)} onChange={(_, d) => handleFormChange('nassauFront', Number(d.value))} />
            </div>
            <div>
              <Label>Back 9 ($)</Label>
              <Input type="number" value={String(form.nassauBack)} onChange={(_, d) => handleFormChange('nassauBack', Number(d.value))} />
            </div>
            <div>
              <Label>18-Hole ($)</Label>
              <Input type="number" value={String(form.nassau18)} onChange={(_, d) => handleFormChange('nassau18', Number(d.value))} />
            </div>
            <div>
              <Label>Handicap %</Label>
              <Input type="number" value={String(form.handicapPercentage)} onChange={(_, d) => handleFormChange('handicapPercentage', Number(d.value))} />
            </div>
          </div>

          {/* Investments */}
          <Divider style={{ margin: `${tokens.spacingVerticalM} 0` }}>Investments</Divider>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalM }}>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
              <Switch label="Off Enabled" checked={form.investmentOffEnabled} onChange={(_, d) => handleFormChange('investmentOffEnabled', d.checked)} />
              {form.investmentOffEnabled && (
                <Input type="number" value={String(form.investmentOffAmount)} onChange={(_, d) => handleFormChange('investmentOffAmount', Number(d.value))} />
              )}
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
              <Switch label="Redemption" checked={form.redemptionEnabled} onChange={(_, d) => handleFormChange('redemptionEnabled', d.checked)} />
              {form.redemptionEnabled && (
                <Input type="number" value={String(form.redemptionAmount)} onChange={(_, d) => handleFormChange('redemptionAmount', Number(d.value))} />
              )}
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
              <Switch label="Dunn" checked={form.dunnEnabled} onChange={(_, d) => handleFormChange('dunnEnabled', d.checked)} />
              {form.dunnEnabled && (
                <Input type="number" value={String(form.dunnAmount)} onChange={(_, d) => handleFormChange('dunnAmount', Number(d.value))} />
              )}
            </div>
          </div>

          {/* Expense + Counting */}
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalM }}>
            <div>
              <Label>Expense Deduction %</Label>
              <Input type="number" value={String(form.expenseDeductionPct)} onChange={(_, d) => handleFormChange('expenseDeductionPct', Number(d.value))} />
            </div>
            <div>
              <Label>Scores Counting Per Hole</Label>
              <Input type="number" value={String(form.scoresCountingPerHole)} onChange={(_, d) => handleFormChange('scoresCountingPerHole', Number(d.value))} />
            </div>
          </div>

          {form.betType === 'Skins' && (
            <>
              <Divider style={{ margin: `${tokens.spacingVerticalM} 0` }}>Skins Settings</Divider>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalM }}>
                <div style={{ display: 'flex', alignItems: 'center' }}>
                  <Switch
                    label="Use Net Scores"
                    checked={skinsUseNetScores}
                    onChange={(_, d) => setSkinsUseNetScores(!!d.checked)}
                  />
                </div>
                <div>
                  <Label>Buy-In Per Player ($)</Label>
                  <Input
                    type="number"
                    value={form.skinsBuyIn == null ? '' : String(form.skinsBuyIn)}
                    onChange={(_, d) => handleFormChange('skinsBuyIn', d.value === '' ? null : Number(d.value))}
                  />
                </div>
                <div>
                  <Label>Amount Per Skin ($)</Label>
                  <Input
                    type="number"
                    value={form.skinsPerSkinAmount == null ? '' : String(form.skinsPerSkinAmount)}
                    onChange={(_, d) => handleFormChange('skinsPerSkinAmount', d.value === '' ? null : Number(d.value))}
                  />
                </div>
              </div>
              <Body1 style={{ marginBottom: tokens.spacingVerticalM }}>
                Use either Buy-In or Amount Per Skin. If both are provided, the API will reject the request.
              </Body1>
            </>
          )}

          {form.betType === 'IndoTournament' && (
            <>
              <Divider style={{ margin: `${tokens.spacingVerticalM} 0` }}>Tournament Settings</Divider>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalM }}>
                <div>
                  <Label>Sponsor Money ($)</Label>
                  <Input
                    type="number"
                    value={String(tournamentConfig.sponsorMoney)}
                    onChange={(_, d) => setTournamentConfig((prev) => ({ ...prev, sponsorMoney: Number(d.value) || 0 }))}
                  />
                </div>
                <div>
                  <Label>Buy-In Per Player ($)</Label>
                  <Input
                    type="number"
                    value={String(tournamentConfig.buyInPerPlayer)}
                    onChange={(_, d) => setTournamentConfig((prev) => ({ ...prev, buyInPerPlayer: Number(d.value) || 0 }))}
                  />
                </div>
                <div>
                  <Label>Gross / Net Purse %</Label>
                  <Input
                    type="text"
                    value={`${tournamentConfig.grossPursePercent}/${tournamentConfig.netPursePercent}`}
                    readOnly
                  />
                </div>
              </div>
              <Body1 style={{ marginBottom: tokens.spacingVerticalM }}>
                Detailed payout table is stored in ConfigJson and can be edited later; defaults are 50/30/20 for top 3 places.
              </Body1>
            </>
          )}

          {requiresTeams(form.betType) && (
            <>
              {/* Team Assignments */}
              <Divider style={{ margin: `${tokens.spacingVerticalM} 0` }}>Teams</Divider>
              {teamAssignments.map((team, tIdx) => (
                <Card key={tIdx} style={{ marginBottom: tokens.spacingVerticalS, padding: tokens.spacingVerticalS }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: tokens.spacingHorizontalM, marginBottom: tokens.spacingVerticalS }}>
                    <Input
                      value={team.teamName}
                      onChange={(_, d) => {
                        setTeamAssignments((prev) => {
                          const next = [...prev];
                          next[tIdx] = { ...next[tIdx], teamName: d.value };
                          return next;
                        });
                      }}
                      style={{ width: 160 }}
                    />
                    <Button size="small" icon={<Delete24Regular />} onClick={() => removeTeam(tIdx)} />
                  </div>
                  {team.players.map((p, pIdx) => (
                    <div key={pIdx} style={{ display: 'flex', gap: tokens.spacingHorizontalS, marginBottom: 4, alignItems: 'center' }}>
                      <Select
                        value={String(p.playerId)}
                        onChange={(_, d) => updateTeamPlayer(tIdx, pIdx, Number(d.value))}
                        style={{ minWidth: 200 }}
                      >
                        <option value="0">Select player...</option>
                        {match.scores.map((s) => (
                          <option key={s.playerId} value={String(s.playerId)}>
                            {s.playerName} (CH: {s.courseHandicap})
                          </option>
                        ))}
                      </Select>
                      <Badge appearance="outline" size="small">
                        {TEAM_POSITIONS[pIdx] ?? 'E'}
                      </Badge>
                      <Button size="small" icon={<Delete24Regular />} onClick={() => removePlayerFromTeam(tIdx, pIdx)} />
                    </div>
                  ))}
                  <Button size="small" onClick={() => addPlayerToTeam(tIdx)}>
                    Add Player
                  </Button>
                </Card>
              ))}
              <Button size="small" icon={<Add24Regular />} onClick={addTeam} style={{ marginBottom: tokens.spacingVerticalM }}>
                Add Team
              </Button>
            </>
          )}

          <div style={{ display: 'flex', gap: tokens.spacingHorizontalM, marginTop: tokens.spacingVerticalM }}>
            <Button appearance="primary" icon={<Save24Regular />} onClick={handleSaveBet} disabled={saving}>
              {saving ? 'Saving...' : 'Save Bet & Teams'}
            </Button>
          </div>
        </Card>
      )}
    </div>
  );
}
