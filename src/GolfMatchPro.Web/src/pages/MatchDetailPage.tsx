import { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  makeStyles,
  tokens,
  Title1,
  Button,
  Badge,
  Spinner,
  MessageBar,
  MessageBarBody,
  Body1,
  Caption1,
  Subtitle2,
} from '@fluentui/react-components';
import { ArrowLeft24Regular, Play24Regular, Checkmark24Regular } from '@fluentui/react-icons';
import type { MatchDetailDto, MatchStatus } from '../types';
import { matchService } from '../services/matchService';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    gap: '16px',
  },
  header: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
  },
  info: {
    display: 'flex',
    gap: '24px',
    flexWrap: 'wrap',
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
});

const statusColor: Record<MatchStatus, 'success' | 'warning' | 'informative'> = {
  Setup: 'informative',
  InProgress: 'warning',
  Completed: 'success',
};

export function MatchDetailPage() {
  const styles = useStyles();
  const navigate = useNavigate();
  const { id } = useParams();
  const matchId = Number(id);

  const [match, setMatch] = useState<MatchDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadMatch = useCallback(() => {
    matchService.getById(matchId)
      .then(setMatch)
      .catch(() => setError('Failed to load match.'))
      .finally(() => setLoading(false));
  }, [matchId]);

  useEffect(() => { loadMatch(); }, [loadMatch]);

  const handleStatusChange = async (newStatus: MatchStatus) => {
    try {
      await matchService.updateStatus(matchId, newStatus);
      loadMatch();
    } catch {
      setError('Failed to update status.');
    }
  };

  const handleDelete = async () => {
    try {
      await matchService.delete(matchId);
      navigate('/matches');
    } catch {
      setError('Failed to delete match.');
    }
  };

  if (loading) return <Spinner label="Loading match..." />;
  if (!match) return <Body1>Match not found.</Body1>;

  return (
    <div className={styles.root}>
      <div className={styles.header}>
        <Button
          appearance="subtle"
          icon={<ArrowLeft24Regular />}
          onClick={() => navigate('/matches')}
        />
        <Title1>{match.course.name}</Title1>
        <Badge appearance="filled" color={statusColor[match.status]}>
          {match.status}
        </Badge>
      </div>

      {error && (
        <MessageBar intent="error">
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      )}

      <div className={styles.info}>
        <Caption1>Date: {match.matchDate}</Caption1>
        <Caption1>Course Rating: {match.course.courseRating}</Caption1>
        <Caption1>Slope: {match.course.slopeRating}</Caption1>
        <Caption1>Par: {match.course.par}</Caption1>
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
              onClick={() => handleStatusChange('InProgress')}
            >
              Start Match
            </Button>
            <Button
              appearance="secondary"
              onClick={handleDelete}
            >
              Delete Match
            </Button>
          </>
        )}
        {match.status === 'InProgress' && (
          <>
            <Button
              appearance="primary"
              onClick={() => navigate(`/matches/${matchId}/scorecard`)}
            >
              Open Scorecard
            </Button>
            <Button
              appearance="outline"
              onClick={() => navigate(`/matches/${matchId}/bets`)}
            >
              Bets
            </Button>
            <Button
              appearance="outline"
              icon={<Checkmark24Regular />}
              onClick={() => handleStatusChange('Completed')}
            >
              Complete Match
            </Button>
          </>
        )}
        {match.status === 'Completed' && (
          <>
            <Button
              appearance="primary"
              onClick={() => navigate(`/matches/${matchId}/scorecard`)}
            >
              View Scorecard
            </Button>
            <Button
              appearance="outline"
              onClick={() => navigate(`/matches/${matchId}/bets`)}
            >
              Bets & Results
            </Button>
          </>
        )}
      </div>
    </div>
  );
}
