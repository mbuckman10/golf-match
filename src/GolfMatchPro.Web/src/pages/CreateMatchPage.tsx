import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  makeStyles,
  Title1,
  Button,
  Field,
  Input,
  Dropdown,
  Option,
  Checkbox,
  Spinner,
  MessageBar,
  MessageBarBody,
  Body1,
} from '@fluentui/react-components';
import { ArrowLeft24Regular, Save24Regular } from '@fluentui/react-icons';
import type { CourseDto, PlayerDto } from '../types';
import { courseService } from '../services/courseService';
import { playerService } from '../services/playerService';
import { matchService } from '../services/matchService';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    gap: '16px',
    maxWidth: '600px',
  },
  header: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
  },
  playerList: {
    display: 'flex',
    flexDirection: 'column',
    gap: '4px',
    maxHeight: '300px',
    overflowY: 'auto',
  },
  actions: {
    display: 'flex',
    gap: '8px',
  },
});

export function CreateMatchPage() {
  const styles = useStyles();
  const navigate = useNavigate();

  const [courses, setCourses] = useState<CourseDto[]>([]);
  const [players, setPlayers] = useState<PlayerDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [selectedCourseId, setSelectedCourseId] = useState<number | null>(null);
  const [matchDate, setMatchDate] = useState(new Date().toISOString().split('T')[0]);
  const [selectedPlayerIds, setSelectedPlayerIds] = useState<Set<number>>(new Set());

  useEffect(() => {
    Promise.all([courseService.getAll(), playerService.getAll()])
      .then(([c, p]) => {
        setCourses(c);
        setPlayers(p.filter(pl => pl.isActive));
      })
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const togglePlayer = (playerId: number) => {
    setSelectedPlayerIds(prev => {
      const next = new Set(prev);
      if (next.has(playerId)) next.delete(playerId);
      else next.add(playerId);
      return next;
    });
  };

  const handleCreate = async () => {
    if (!selectedCourseId) {
      setError('Please select a course.');
      return;
    }
    if (selectedPlayerIds.size === 0) {
      setError('Please select at least one player.');
      return;
    }

    setError(null);
    setSaving(true);

    try {
      // Use first selected player as creator for now (no auth yet)
      const creatorId = [...selectedPlayerIds][0];
      const match = await matchService.create({
        courseId: selectedCourseId,
        matchDate: matchDate,
        createdByPlayerId: creatorId,
        playerIds: [...selectedPlayerIds],
      });
      navigate(`/matches/${match.matchId}`);
    } catch (err: any) {
      setError(err?.response?.data?.error ?? 'Failed to create match.');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <Spinner label="Loading..." />;

  return (
    <div className={styles.root}>
      <div className={styles.header}>
        <Button
          appearance="subtle"
          icon={<ArrowLeft24Regular />}
          onClick={() => navigate('/matches')}
        />
        <Title1>New Match</Title1>
      </div>

      {error && (
        <MessageBar intent="error">
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      )}

      <Field label="Course" required>
        <Dropdown
          placeholder="Select a course"
          value={courses.find(c => c.courseId === selectedCourseId)?.name ?? ''}
          onOptionSelect={(_, d) => setSelectedCourseId(Number(d.optionValue))}
        >
          {courses.map(c => (
            <Option key={c.courseId} value={c.courseId.toString()}>
              {c.name}{c.teeColor ? ` (${c.teeColor})` : ''}
            </Option>
          ))}
        </Dropdown>
      </Field>

      <Field label="Date" required>
        <Input
          type="date"
          value={matchDate}
          onChange={(_, d) => setMatchDate(d.value)}
        />
      </Field>

      <Field label="Players">
        <Body1>{selectedPlayerIds.size} selected</Body1>
      </Field>
      <div className={styles.playerList}>
        {players.map(p => (
          <Checkbox
            key={p.playerId}
            label={p.nickname ? `${p.fullName} (${p.nickname})` : p.fullName}
            checked={selectedPlayerIds.has(p.playerId)}
            onChange={() => togglePlayer(p.playerId)}
          />
        ))}
      </div>

      <div className={styles.actions}>
        <Button
          appearance="primary"
          icon={<Save24Regular />}
          onClick={handleCreate}
          disabled={saving}
        >
          {saving ? 'Creating...' : 'Create Match'}
        </Button>
        <Button appearance="secondary" onClick={() => navigate('/matches')}>
          Cancel
        </Button>
      </div>
    </div>
  );
}
