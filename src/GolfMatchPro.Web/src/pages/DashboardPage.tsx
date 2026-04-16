import { useEffect, useMemo, useState } from 'react';
import {
  makeStyles,
  tokens,
  Title2,
  Body1,
  Body2,
  Card,
  CardHeader,
  Button,
  Spinner,
  Badge,
  Caption1,
} from '@fluentui/react-components';
import { Add24Regular } from '@fluentui/react-icons';
import { useNavigate } from 'react-router-dom';
import { matchService } from '../services/matchService';
import type { MatchDto, MatchStatus } from '../types';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    gap: '24px',
  },
  sectionHeader: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    gap: '12px',
    flexWrap: 'wrap',
  },
  section: {
    display: 'flex',
    flexDirection: 'column',
    gap: '12px',
  },
  cards: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))',
    gap: '16px',
  },
  matchCards: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))',
    gap: '16px',
  },
  card: {
    cursor: 'pointer',
    transition: 'transform 120ms ease, box-shadow 120ms ease',
    '&:hover': {
      boxShadow: tokens.shadow8,
      transform: 'translateY(-1px)',
    },
  },
  cardMeta: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
  },
  statusBadge: {
    flexShrink: 0,
    whiteSpace: 'nowrap',
    display: 'inline-flex',
    alignItems: 'center',
  },
  emptyState: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
    flexWrap: 'wrap',
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

export function DashboardPage() {
  const styles = useStyles();
  const navigate = useNavigate();
  const [matches, setMatches] = useState<MatchDto[]>([]);
  const [loadingMatches, setLoadingMatches] = useState(true);

  useEffect(() => {
    matchService
      .getAll()
      .then(setMatches)
      .catch(console.error)
      .finally(() => setLoadingMatches(false));
  }, []);

  const activeMatches = useMemo(
    () => matches.filter((m) => m.status === 'Setup' || m.status === 'InProgress'),
    [matches],
  );

  const openMatch = (match: MatchDto) => {
    navigate(match.status === 'Setup' ? `/matches/${match.matchId}` : `/matches/${match.matchId}/scorecard`);
  };

  return (
    <div className={styles.root}>
      <div className={styles.section}>
        <div className={styles.sectionHeader}>
          <Title2>Active Matches</Title2>
          {!loadingMatches && activeMatches.length > 0 && (
            <Button appearance="primary" icon={<Add24Regular />} onClick={() => navigate('/matches/new')}>
              Start a Match
            </Button>
          )}
        </div>
        {loadingMatches ? (
          <Spinner label="Loading active matches..." />
        ) : activeMatches.length === 0 ? (
          <div className={styles.emptyState}>
            <Body2>No matches at the moment.</Body2>
            <Button appearance="primary" onClick={() => navigate('/matches/new')}>
              Start one!
            </Button>
          </div>
        ) : (
          <div className={styles.matchCards}>
            {activeMatches.map((match) => (
              <Card key={match.matchId} className={styles.card} onClick={() => openMatch(match)}>
                <CardHeader
                  header={<Body1><b>{match.courseName}</b></Body1>}
                  description={<Caption1>{match.matchDate}</Caption1>}
                  action={
                    <Badge className={styles.statusBadge} appearance="filled" color={statusColor[match.status]}>
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

      <div className={styles.section}>
        <Title2>Setup & Management</Title2>
        <div className={styles.cards}>
        <Card className={styles.card} onClick={() => navigate('/courses')}>
          <CardHeader header="Manage Courses" description="Add and edit golf courses with hole data" />
        </Card>
        <Card className={styles.card} onClick={() => navigate('/players')}>
          <CardHeader header="Manage Players" description="Manage your player roster and handicaps" />
        </Card>
        <Card className={styles.card} onClick={() => navigate('/matches')}>
          <CardHeader header="All Matches" description="View history and manage every match" />
        </Card>
      </div>
      </div>
    </div>
  );
}
