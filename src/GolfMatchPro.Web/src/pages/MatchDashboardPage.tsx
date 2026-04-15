import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  makeStyles,
  tokens,
  Title1,
  Button,
  Badge,
  Card,
  CardHeader,
  Body1,
  Caption1,
  Spinner,
} from '@fluentui/react-components';
import { Add24Regular } from '@fluentui/react-icons';
import type { MatchDto, MatchStatus } from '../types';
import { matchService } from '../services/matchService';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    gap: '16px',
  },
  header: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    flexWrap: 'wrap',
    gap: '8px',
  },
  grid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))',
    gap: '16px',
  },
  card: {
    cursor: 'pointer',
    '&:hover': {
      boxShadow: tokens.shadow8,
    },
  },
  cardMeta: {
    display: 'flex',
    gap: '12px',
    alignItems: 'center',
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

export function MatchDashboardPage() {
  const styles = useStyles();
  const navigate = useNavigate();
  const [matches, setMatches] = useState<MatchDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    matchService.getAll()
      .then(setMatches)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className={styles.root}>
      <div className={styles.header}>
        <Title1>Matches</Title1>
        <Button
          appearance="primary"
          icon={<Add24Regular />}
          onClick={() => navigate('/matches/new')}
        >
          New Match
        </Button>
      </div>
      {loading ? (
        <Spinner label="Loading matches..." />
      ) : matches.length === 0 ? (
        <Body1>No matches yet. Create your first match to get started!</Body1>
      ) : (
        <div className={styles.grid}>
          {matches.map(match => (
            <Card
              key={match.matchId}
              className={styles.card}
              onClick={() => navigate(
                match.status === 'Setup'
                  ? `/matches/${match.matchId}`
                  : `/matches/${match.matchId}/scorecard`
              )}
            >
              <CardHeader
                header={<Body1><b>{match.courseName}</b></Body1>}
                description={<Caption1>{match.matchDate}</Caption1>}
                action={
                  <Badge
                    appearance="filled"
                    color={statusColor[match.status]}
                  >
                    {statusLabel[match.status]}
                  </Badge>
                }
              />
              <div className={styles.cardMeta}>
                <Caption1>{match.playerCount} player{match.playerCount !== 1 ? 's' : ''}</Caption1>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
