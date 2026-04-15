import { useEffect, useState, useCallback, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  makeStyles,
  tokens,
  Title1,
  Button,
  Spinner,
  Body1,
  Tab,
  TabList,
  Dropdown,
  Option,
} from '@fluentui/react-components';
import { ArrowLeft24Regular } from '@fluentui/react-icons';
import type { MatchDetailDto, MatchScoreDto, CourseHoleDto } from '../types';
import { matchService } from '../services/matchService';
import { startConnection, joinMatch, leaveMatch, onScoreUpdated, offScoreUpdated } from '../services/signalRService';
import { HoleSelector } from '../components/HoleSelector';
import { ScoreInput } from '../components/ScoreInput';
import { RunningTotals } from '../components/RunningTotals';
import { ScoreGrid } from '../components/ScoreGrid';

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
    gap: '8px',
    '@media (min-width: 960px)': {
      display: 'none',
    },
  },
  desktopView: {
    display: 'none',
    '@media (min-width: 960px)': {
      display: 'block',
    },
  },
  playerSelect: {
    minWidth: '200px',
  },
});

export function ScorecardPage() {
  const styles = useStyles();
  const navigate = useNavigate();
  const { id } = useParams();
  const matchId = Number(id);

  const [match, setMatch] = useState<MatchDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [currentHole, setCurrentHole] = useState(1);
  const [selectedPlayerId, setSelectedPlayerId] = useState<number | null>(null);
  const [tab, setTab] = useState<string>('entry');
  const [saving, setSaving] = useState(false);

  const matchRef = useRef(match);
  matchRef.current = match;

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
      // Auto-advance to next hole
      if (currentHole < 18) {
        setCurrentHole(prev => prev + 1);
      }
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
          onClick={() => navigate(`/matches/${matchId}`)}
        />
        <Title1>{match.course.name}</Title1>
      </div>

      <TabList selectedValue={tab} onTabSelect={(_, d) => setTab(d.value as string)}>
        <Tab value="entry">Score Entry</Tab>
        <Tab value="grid">Full Grid</Tab>
      </TabList>

      {tab === 'entry' && (
        <>
          <Dropdown
            className={styles.playerSelect}
            placeholder="Select player"
            value={currentPlayer ? (currentPlayer.playerNickname ?? currentPlayer.playerName) : ''}
            onOptionSelect={(_, d) => setSelectedPlayerId(Number(d.optionValue))}
          >
            {match.scores.map(s => (
              <Option key={s.playerId} value={s.playerId.toString()}>
                {s.playerNickname ?? s.playerName} (CH: {s.courseHandicap})
              </Option>
            ))}
          </Dropdown>

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
