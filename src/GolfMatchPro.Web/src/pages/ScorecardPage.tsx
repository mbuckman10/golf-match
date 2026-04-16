import { useEffect, useState, useCallback, useRef } from 'react';
import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import {
  makeStyles,
  tokens,
  Title1,
  Title2,
  Button,
  Spinner,
  Body1,
  Tab,
  TabList,
  Dropdown,
  Option,
  Badge,
  Caption1,
  Subtitle2,
  Dialog,
  DialogTrigger,
  DialogSurface,
  DialogTitle,
  DialogBody,
  DialogContent,
  DialogActions,
} from '@fluentui/react-components';
import { ArrowLeft24Regular, Play24Regular, Checkmark24Regular, Delete24Regular, Archive24Regular } from '@fluentui/react-icons';
import type { MatchDetailDto, MatchScoreDto, CourseHoleDto } from '../types';
import { matchService } from '../services/matchService';
import { startConnection, joinMatch, leaveMatch, onScoreUpdated, offScoreUpdated } from '../services/signalRService';
import { HoleSelector } from '../components/HoleSelector';
import { ScoreInput } from '../components/ScoreInput';
import { RunningTotals } from '../components/RunningTotals';
import { ScoreGrid } from '../components/ScoreGrid';
import { BetConfigPage } from './BetConfigPage';
import { MessageBar, MessageBarBody } from '@fluentui/react-components';
import { annotate } from 'rough-notation';
import { formatDateMdY } from '../utils/date';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    gap: '12px',
  },
  header: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
    flexWrap: 'wrap',
  },
  mobileView: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    gap: '8px',
    '@media (min-width: 960px)': {
      display: 'none',
    },
  },
  desktopView: {
    display: 'none',
    '@media (min-width: 960px)': {
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: '8px',
    },
  },
  playerSelect: {
    minWidth: '240px',
    fontSize: '17px',
    fontWeight: '700',
    fontFamily: 'var(--golf-font-classic-display)',
  },
  playerLabel: {
    textAlign: 'center',
    fontFamily: 'var(--golf-font-classic-display)',
    fontWeight: '700',
    color: 'var(--golf-ink)',
    letterSpacing: '0.02em',
    marginBottom: '-4px',
  },
  tabList: {
    overflow: 'visible',
    marginBottom: '14px',
    '& [role="tab"]::before': {
      content: 'none !important',
      borderBottomColor: 'transparent !important',
    },
    '& [role="tab"]::after': {
      content: 'none !important',
      borderBottomColor: 'transparent !important',
    },
    '& [role="tab"][aria-selected="true"]': {
      borderBottomColor: 'transparent',
      color: 'var(--golf-green-700)',
      overflow: 'visible',
      boxShadow: 'none',
    },
  },
  matchInfo: {
    display: 'flex',
    gap: '24px',
    flexWrap: 'wrap',
  },
  metaLabel: {
    fontWeight: 600,
  },
  playerList: {
    display: 'flex',
    flexDirection: 'column',
    gap: '8px',
  },
  playerRow: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: '8px 12px',
    backgroundColor: tokens.colorNeutralBackground2,
    borderRadius: tokens.borderRadiusMedium,
  },
  actions: {
    display: 'flex',
    gap: '8px',
    flexWrap: 'wrap',
  },
  deleteBtn: {
    color: 'var(--golf-ink-soft)',
    '& svg': { color: 'inherit' },
    ':hover': {
      color: 'var(--golf-danger)',
      backgroundColor: 'rgba(163,62,62,0.08)',
    },
    ':hover svg': { color: 'var(--golf-danger)' },
    ':active': {
      color: '#7a1f1f',
      backgroundColor: 'rgba(163,62,62,0.16)',
    },
    ':active svg': { color: '#7a1f1f' },
  },
  matchRoot: {
    display: 'flex',
    flexDirection: 'column',
    gap: '16px',
  },
});

export function ScorecardPage() {
  const styles = useStyles();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { id } = useParams();
  const matchId = Number(id);

  const [match, setMatch] = useState<MatchDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [currentHole, setCurrentHole] = useState(1);
  const [selectedPlayerId, setSelectedPlayerId] = useState<number | null>(null);
  const [tab, setTab] = useState<string>('entry');
  const [saving, setSaving] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [archiveOpen, setArchiveOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const requestedTab = searchParams.get('tab');

  const statusColor: Record<string, 'success' | 'warning' | 'informative'> = {
    Setup: 'informative',
    InProgress: 'warning',
    Completed: 'success',
  };

  const statusLabel: Record<string, string> = {
    Setup: 'Setup',
    InProgress: 'In Progress',
    Completed: 'Completed',
  };

  const matchRef = useRef(match);
  matchRef.current = match;

  const detailsTabRef = useRef<HTMLButtonElement | null>(null);
  const betsTabRef = useRef<HTMLButtonElement | null>(null);
  const entryTabRef = useRef<HTMLButtonElement | null>(null);
  const gridTabRef = useRef<HTMLButtonElement | null>(null);
  const tabAnnotationRef = useRef<ReturnType<typeof annotate> | null>(null);

  const loadMatch = useCallback(async () => {
    try {
      const data = await matchService.getById(matchId);
      setMatch(data);
      if (!selectedPlayerId && data.scores.length > 0) {
        setSelectedPlayerId(data.scores[0].playerId);
      }
    } catch {
      // ignore
    } finally {
      setLoading(false);
    }
  }, [matchId, selectedPlayerId]);

  useEffect(() => {
    loadMatch();
  }, [loadMatch]);

  useEffect(() => {
    if (!requestedTab) return;
    if (requestedTab === 'details' || requestedTab === 'bets' || requestedTab === 'entry' || requestedTab === 'grid') {
      setTab(requestedTab);
    }
  }, [requestedTab]);

  useEffect(() => {
    if (tabAnnotationRef.current) {
      tabAnnotationRef.current.remove();
      tabAnnotationRef.current = null;
    }

    const target =
      tab === 'details'
        ? detailsTabRef.current
        : tab === 'bets'
          ? betsTabRef.current
        : tab === 'entry'
          ? entryTabRef.current
          : tab === 'grid'
            ? gridTabRef.current
            : null;

    if (!target) return;

    const annotation = annotate(target, {
      type: 'underline',
      color: '#1e6a3a',
      strokeWidth: 2,
      iterations: 2,
      multiline: false,
      padding: [0, -6, -2, -6],
      animate: false,
    });

    annotation.show();
    tabAnnotationRef.current = annotation;

    return () => {
      if (tabAnnotationRef.current) {
        tabAnnotationRef.current.remove();
        tabAnnotationRef.current = null;
      }
    };
  }, [tab, loading, matchId]);

  // SignalR connection
  useEffect(() => {
    const handleScoreUpdate = (_mId: number, _pId: number, _hole: number, _score: number) => {
      // Reload all scores when any update comes in
      loadMatch();
    };

    startConnection()
      .then(() => joinMatch(matchId))
      .catch(console.error);

    onScoreUpdated(handleScoreUpdate);

    return () => {
      offScoreUpdated(handleScoreUpdate);
      leaveMatch(matchId).catch(() => {});
    };
  }, [matchId, loadMatch]);

  const handleScoreChange = async (score: number) => {
    if (!selectedPlayerId || saving) return;
    setSaving(true);
    try {
      await matchService.updateHoleScore(matchId, selectedPlayerId, currentHole, score);
      await loadMatch();
    } catch (err) {
      console.error('Failed to save score', err);
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <Spinner label="Loading scorecard..." />;
  if (!match) return <Body1>Match not found.</Body1>;

  const currentPlayer = match.scores.find(s => s.playerId === selectedPlayerId);
  const sortedHoles = [...match.course.holes].sort((a, b) => a.holeNumber - b.holeNumber);
  const currentHoleData = sortedHoles[currentHole - 1];
  const isReadonly = match.status === 'Completed';

  // Compute handicap strokes for the selected player on the current hole
  const getHandicapStrokes = (player: MatchScoreDto, hole: CourseHoleDto): number => {
    if (!player) return 0;
    const ch = player.courseHandicap;
    if (ch <= 0) return 0;
    const fullPasses = Math.floor(ch / 18);
    const remainder = ch % 18;
    return fullPasses + (hole.handicapRanking <= remainder ? 1 : 0);
  };

  return (
    <div className={styles.root}>
      <div className={styles.header}>
        <Button
          appearance="subtle"
          icon={<ArrowLeft24Regular />}
          onClick={() => navigate('/matches')}
        />
        <Title1>{match.matchName}</Title1>
        <Caption1>{match.course.name}{match.course.teeColor ? ` (${match.course.teeColor})` : ''}</Caption1>
        {match && (
          <Badge appearance="filled" color={statusColor[match.status]}>
            {statusLabel[match.status]}
          </Badge>
        )}
      </div>

      <TabList
        selectedValue={tab}
        onTabSelect={(_, d) => setTab(d.value as string)}
        className={styles.tabList}
      >
        <Tab value="details" ref={detailsTabRef}>Match Details</Tab>
        <Tab value="bets" ref={betsTabRef}>Bets</Tab>
        <Tab value="entry" ref={entryTabRef}>Score Entry</Tab>
        <Tab value="grid" ref={gridTabRef}>Full Grid</Tab>
      </TabList>

      {tab === 'bets' && <BetConfigPage embedded />}

      {tab === 'details' && match && (
        <div className={styles.matchRoot}>
          {error && (
            <MessageBar intent="error">
              <MessageBarBody>{error}</MessageBarBody>
            </MessageBar>
          )}

          <div className={styles.matchInfo}>
            <Caption1><span className={styles.metaLabel}>Date:</span> {formatDateMdY(match.matchDate)}</Caption1>
            <Caption1><span className={styles.metaLabel}>Course Rating:</span> {match.course.courseRating}</Caption1>
            <Caption1><span className={styles.metaLabel}>Slope:</span> {match.course.slopeRating}</Caption1>
            <Caption1><span className={styles.metaLabel}>Par:</span> {match.course.par}</Caption1>
          </div>

          <Subtitle2>Players ({match.scores.length})</Subtitle2>
          <div className={styles.playerList}>
            {match.scores.map(s => (
              <div key={s.playerId} className={styles.playerRow}>
                <div>
                  <Body1><b>{s.playerNickname ?? s.playerName}</b></Body1>
                  <Caption1> — Course Handicap: {s.courseHandicap}</Caption1>
                </div>
                {s.grossTotal > 0 && (
                  <Caption1>Gross: {s.grossTotal} | Net: {s.netTotal}</Caption1>
                )}
              </div>
            ))}
          </div>

          <div className={styles.actions}>
            {match.status === 'Setup' && (
              <>
                <Button
                  appearance="primary"
                  icon={<Play24Regular />}
                  onClick={() => {
                    matchService.updateStatus(matchId, 'InProgress').then(() => loadMatch()).catch(() => setError('Failed to start match'));
                  }}
                >
                  Start Match
                </Button>
                <Button
                  appearance="subtle"
                  className={styles.deleteBtn}
                  icon={<Delete24Regular />}
                  onClick={() => setDeleteOpen(true)}
                >
                  Delete
                </Button>
              </>
            )}
            {match.status === 'InProgress' && (
              <>
                <Button
                  appearance="primary"
                  onClick={() => setTab('entry')}
                >
                  Open Scorecard
                </Button>
                <Button
                  appearance="outline"
                  icon={<Checkmark24Regular />}
                  onClick={() => {
                    matchService.updateStatus(matchId, 'Completed').then(() => loadMatch()).catch(() => setError('Failed to complete match'));
                  }}
                >
                  Complete Match
                </Button>
                <Button
                  appearance="subtle"
                  className={styles.deleteBtn}
                  icon={<Delete24Regular />}
                  onClick={() => setDeleteOpen(true)}
                >
                  Delete
                </Button>
              </>
            )}
            {match.status === 'Completed' && (
              <>
                <Button
                  appearance="primary"
                  onClick={() => setTab('entry')}
                >
                  View Scorecard
                </Button>
                <Button
                  appearance="outline"
                  icon={<Archive24Regular />}
                  onClick={() => setArchiveOpen(true)}
                >
                  Archive
                </Button>
                <Button
                  appearance="subtle"
                  className={styles.deleteBtn}
                  icon={<Delete24Regular />}
                  onClick={() => setDeleteOpen(true)}
                >
                  Delete
                </Button>
              </>
            )}
          </div>

          <Dialog open={deleteOpen} onOpenChange={(_, d) => setDeleteOpen(d.open)}>
            <DialogSurface>
              <DialogBody>
                <DialogTitle>Delete this match?</DialogTitle>
                <DialogContent>
                  This will permanently remove the match and all associated scores. This cannot be undone.
                </DialogContent>
                <DialogActions>
                  <DialogTrigger disableButtonEnhancement>
                    <Button appearance="secondary">Cancel</Button>
                  </DialogTrigger>
                  <Button
                    appearance="primary"
                    onClick={async () => {
                      try {
                        await matchService.delete(matchId);
                        navigate('/');
                      } catch (err: any) {
                        setError(err?.response?.data?.error ?? 'Failed to delete match.');
                      } finally {
                        setDeleteOpen(false);
                      }
                    }}
                    icon={<Delete24Regular />}
                    style={{ backgroundColor: '#a33e3e', borderColor: '#a33e3e', color: '#fff' }}
                  >
                    Delete Match
                  </Button>
                </DialogActions>
              </DialogBody>
            </DialogSurface>
          </Dialog>

          <Dialog open={archiveOpen} onOpenChange={(_, d) => setArchiveOpen(d.open)}>
            <DialogSurface>
              <DialogBody>
                <DialogTitle>Archive this match?</DialogTitle>
                <DialogContent>
                  The match will be hidden from the matches list. You can access archived matches later from the archive screen.
                </DialogContent>
                <DialogActions>
                  <DialogTrigger disableButtonEnhancement>
                    <Button appearance="secondary">Cancel</Button>
                  </DialogTrigger>
                  <Button
                    appearance="primary"
                    icon={<Archive24Regular />}
                    onClick={async () => {
                      try {
                        await matchService.archive(matchId);
                        navigate('/');
                      } catch (err: any) {
                        setError(err?.response?.data?.error ?? 'Failed to archive match.');
                      } finally {
                        setArchiveOpen(false);
                      }
                    }}
                  >
                    Archive Match
                  </Button>
                </DialogActions>
              </DialogBody>
            </DialogSurface>
          </Dialog>
        </div>
      )}

      {tab === 'entry' && (
        <>
          <Dropdown
            className={styles.playerSelect}
            placeholder="Select player"
            value={currentPlayer ? (currentPlayer.playerNickname ?? currentPlayer.playerName) : ''}
            onOptionSelect={(_, d) => setSelectedPlayerId(Number(d.optionValue))}
            size="large"
          >
            {match.scores.map(s => (
              <Option
                key={s.playerId}
                value={s.playerId.toString()}
                text={`${s.playerNickname ?? s.playerName} (CH: ${s.courseHandicap})`}
              >
                {s.playerNickname ?? s.playerName} (CH: {s.courseHandicap})
              </Option>
            ))}
          </Dropdown>

          {currentPlayer && (
            <Title2 className={styles.playerLabel}>
              {currentPlayer.playerNickname ?? currentPlayer.playerName}
            </Title2>
          )}

          {currentPlayer && (
            <>
              {/* Mobile view */}
              <div className={styles.mobileView}>
                <HoleSelector
                  currentHole={currentHole}
                  holeScores={currentPlayer.holeScores}
                  onSelectHole={setCurrentHole}
                />
                <ScoreInput
                  holeNumber={currentHole}
                  par={currentHoleData.par}
                  handicapRanking={currentHoleData.handicapRanking}
                  handicapStrokes={getHandicapStrokes(currentPlayer, currentHoleData)}
                  currentScore={currentPlayer.holeScores[currentHole - 1]}
                  onScoreChange={handleScoreChange}
                  disabled={isReadonly || saving}
                />
                <RunningTotals
                  holeScores={currentPlayer.holeScores}
                  courseHandicap={currentPlayer.courseHandicap}
                  coursePar={match.course.par}
                />
              </div>

              {/* Desktop view: still show entry controls + grid */}
              <div className={styles.desktopView}>
                <HoleSelector
                  currentHole={currentHole}
                  holeScores={currentPlayer.holeScores}
                  onSelectHole={setCurrentHole}
                />
                <ScoreInput
                  holeNumber={currentHole}
                  par={currentHoleData.par}
                  handicapRanking={currentHoleData.handicapRanking}
                  handicapStrokes={getHandicapStrokes(currentPlayer, currentHoleData)}
                  currentScore={currentPlayer.holeScores[currentHole - 1]}
                  onScoreChange={handleScoreChange}
                  disabled={isReadonly || saving}
                />
                <RunningTotals
                  holeScores={currentPlayer.holeScores}
                  courseHandicap={currentPlayer.courseHandicap}
                  coursePar={match.course.par}
                />
              </div>
            </>
          )}
        </>
      )}

      {tab === 'grid' && (
        <ScoreGrid scores={match.scores} holes={match.course.holes} />
      )}
    </div>
  );
}
