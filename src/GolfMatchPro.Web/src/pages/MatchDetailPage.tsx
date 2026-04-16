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
  Dialog,
  DialogTrigger,
  DialogSurface,
  DialogTitle,
  DialogBody,
  DialogContent,
  DialogActions,
} from '@fluentui/react-components';
import { ArrowLeft24Regular, Play24Regular, Checkmark24Regular, Delete24Regular, Archive24Regular } from '@fluentui/react-icons';
import type { MatchDetailDto, MatchStatus } from '../types';
import { matchService } from '../services/matchService';
import { formatDateMdY } from '../utils/date';

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
});

const statusColor: Record<MatchStatus, 'success' | 'warning' | 'informative'> = {
  Setup: 'informative',
  InProgress: 'warning',
  Completed: 'success',
};

const statusLabel: Record<MatchStatus, string> = {
  Setup: 'Setup',
  InProgress: 'In Progress',
  Completed: 'Completed',
};

export function MatchDetailPage() {
  const styles = useStyles();
  const navigate = useNavigate();
  const { id } = useParams();
  const matchId = Number(id);

  const [match, setMatch] = useState<MatchDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [archiveOpen, setArchiveOpen] = useState(false);

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
      if (newStatus === 'InProgress') {
        navigate(`/matches/${matchId}/scorecard`);
        return;
      }

      loadMatch();
    } catch {
      setError('Failed to update status.');
    }
  };

  const handleDelete = async () => {
    try {
      await matchService.delete(matchId);
      navigate('/');
    } catch {
      setError('Failed to delete match.');
    } finally {
      setDeleteOpen(false);
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
          onClick={() => navigate('/')}
        />
        <Title1>{match.matchName}</Title1>
        <Badge appearance="filled" color={statusColor[match.status]}>
          {statusLabel[match.status]}
        </Badge>
      </div>

      {error && (
        <MessageBar intent="error">
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      )}

      <div className={styles.info}>
        <Caption1>Course: {match.course.name}{match.course.teeColor ? ` (${match.course.teeColor})` : ''}</Caption1>
        <Caption1>Date: {formatDateMdY(match.matchDate)}</Caption1>
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
                onClick={handleDelete}
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
  );
}
