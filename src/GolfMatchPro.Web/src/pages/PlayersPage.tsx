import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  makeStyles,
  tokens,
  Title1,
  Button,
  Input,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  Spinner,
  Badge,
} from '@fluentui/react-components';
import { Add24Regular, Search24Regular } from '@fluentui/react-icons';
import type { PlayerDto } from '../types';
import { playerService } from '../services/playerService';

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
  search: {
    width: '300px',
    '@media (max-width: 600px)': {
      width: '100%',
    },
  },
  clickableRow: {
    cursor: 'pointer',
    '&:hover': {
      backgroundColor: tokens.colorNeutralBackground1Hover,
    },
  },
});

export function PlayersPage() {
  const styles = useStyles();
  const navigate = useNavigate();
  const [players, setPlayers] = useState<PlayerDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');

  const loadPlayers = (term?: string) => {
    setLoading(true);
    playerService.getAll(term)
      .then(setPlayers)
      .catch(console.error)
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadPlayers();
  }, []);

  useEffect(() => {
    const timeout = setTimeout(() => loadPlayers(search || undefined), 300);
    return () => clearTimeout(timeout);
  }, [search]);

  return (
    <div className={styles.root}>
      <div className={styles.header}>
        <Title1>Players</Title1>
        <Button
          appearance="primary"
          icon={<Add24Regular />}
          onClick={() => navigate('/players/new')}
        >
          Add Player
        </Button>
      </div>
      <Input
        className={styles.search}
        placeholder="Search players..."
        contentBefore={<Search24Regular />}
        value={search}
        onChange={(_, d) => setSearch(d.value)}
      />
      {loading ? (
        <Spinner label="Loading players..." />
      ) : players.length === 0 ? (
        <p>No players found.</p>
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHeaderCell>Name</TableHeaderCell>
              <TableHeaderCell>Nickname</TableHeaderCell>
              <TableHeaderCell>Handicap</TableHeaderCell>
              <TableHeaderCell>Status</TableHeaderCell>
            </TableRow>
          </TableHeader>
          <TableBody>
            {players.map(player => (
              <TableRow
                key={player.playerId}
                className={styles.clickableRow}
                onClick={() => navigate(`/players/${player.playerId}`)}
              >
                <TableCell>{player.fullName}</TableCell>
                <TableCell>{player.nickname ?? '—'}</TableCell>
                <TableCell>{player.handicapIndex}</TableCell>
                <TableCell>
                  {player.isGuest ? (
                    <Badge appearance="outline" color="informative">Guest</Badge>
                  ) : player.isActive ? (
                    <Badge appearance="filled" color="success">Active</Badge>
                  ) : (
                    <Badge appearance="outline" color="danger">Inactive</Badge>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  );
}
