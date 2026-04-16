import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  makeStyles,
  tokens,
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
  Caption1,
  Badge,
} from '@fluentui/react-components';
import { ArrowLeft24Regular, Save24Regular, Dismiss24Regular } from '@fluentui/react-icons';
import type { CourseDto, PlayerDto } from '../types';
import { courseService } from '../services/courseService';
import { playerService } from '../services/playerService';
import { matchService } from '../services/matchService';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    gap: '16px',
    maxWidth: '700px',
  },
  header: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
  },
  selectedSection: {
    padding: '12px',
    backgroundColor: 'var(--golf-creme-50)',
    borderRadius: tokens.borderRadiusMedium,
    border: `1px solid var(--golf-creme-300)`,
  },
  selectedLabel: {
    fontFamily: 'var(--golf-font-classic-display)',
    fontWeight: 700,
    marginBottom: '8px',
    display: 'flex',
    alignItems: 'center',
    gap: '8px',
  },
  selectedPlayers: {
    display: 'flex',
    flexWrap: 'wrap',
    gap: '6px',
  },
  playerChip: {
    display: 'inline-flex',
    alignItems: 'center',
    gap: '4px',
    padding: '4px 8px',
    backgroundColor: 'var(--golf-green-500)',
    color: '#fff',
    borderRadius: tokens.borderRadiusMedium,
    fontSize: '12px',
    fontWeight: 600,
  },
  chipRemove: {
    cursor: 'pointer',
    display: 'flex',
    alignItems: 'center',
    marginLeft: '4px',
    '&:hover': {
      opacity: 0.7,
    },
  },
  playersGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))',
    gap: '8px',
    padding: '12px',
    backgroundColor: tokens.colorNeutralBackground2,
    borderRadius: tokens.borderRadiusMedium,
    maxHeight: '400px',
    overflowY: 'auto',
  },
  playerCheckbox: {
    padding: '8px',
    borderRadius: tokens.borderRadiusSmall,
    '&:hover': {
      backgroundColor: tokens.colorNeutralBackground3,
    },
  },
  emptyState: {
    textAlign: 'center',
    padding: '24px 12px',
    color: tokens.colorNeutralForeground3,
  },
  actions: {
    display: 'flex',
    gap: '8px',
  },
  fieldControl: {
    '--colorCompoundBrandStroke': 'var(--golf-green-500)',
    '--colorCompoundBrandStrokeHover': 'var(--golf-green-600)',
    '--colorCompoundBrandStrokePressed': 'var(--golf-green-700)',
    '--colorStrokeFocus2': 'var(--golf-green-500)',
    backgroundColor: '#fffdf8',
    minHeight: '32px',
    ':hover': {
      backgroundColor: '#fffefb',
      boxShadow: 'inset 0 0 0 1px rgba(43,130,80,0.28)',
    },
    ':focus-within': {
      backgroundColor: '#fffefb',
      boxShadow: '0 0 0 2px rgba(43,130,80,0.18)',
    },
    '& input:focus': {
      outlineColor: 'var(--golf-green-500)',
    },
    '& select:focus': {
      outlineColor: 'var(--golf-green-500)',
    },
  },
  dropdownListbox: {
    backgroundColor: '#fffefb',
    border: '1px solid var(--golf-creme-300)',
    '& [role="option"]': {
      color: 'var(--golf-ink)',
    },
    '& [role="option"]:hover': {
      backgroundColor: 'var(--golf-creme-50)',
    },
    '& [role="option"][aria-selected="true"]': {
      backgroundColor: 'var(--golf-green-100)',
      color: 'var(--golf-green-700)',
      fontWeight: 600,
    },
    '& [role="option"][aria-selected="true"]:hover': {
      backgroundColor: 'var(--golf-green-200)',
    },
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

  const removePlayer = (playerId: number) => {
    setSelectedPlayerIds(prev => {
      const next = new Set(prev);
      next.delete(playerId);
      return next;
    });
  };

  const getPlayerName = (playerId: number) => {
    return players.find(p => p.playerId === playerId);
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
          className={styles.fieldControl}
          listbox={{ className: styles.dropdownListbox }}
          placeholder="Select a course"
          value={courses.find(c => c.courseId === selectedCourseId)?.name ?? ''}
          onOptionSelect={(_, d) => setSelectedCourseId(Number(d.optionValue))}
        >
          {courses.map(c => (
            <Option
              key={c.courseId}
              value={c.courseId.toString()}
              text={`${c.name}${c.teeColor ? ` (${c.teeColor})` : ''}`}
            >
              {c.name}{c.teeColor ? ` (${c.teeColor})` : ''}
            </Option>
          ))}
        </Dropdown>
      </Field>

      <Field label="Date" required>
        <Input
          className={styles.fieldControl}
          type="date"
          value={matchDate}
          onChange={(_, d) => setMatchDate(d.value)}
        />
      </Field>

      <Field label="Players" required>
        {selectedPlayerIds.size > 0 && (
          <div className={styles.selectedSection}>
            <div className={styles.selectedLabel}>
              <Badge appearance="filled" color="success">
                {selectedPlayerIds.size}
              </Badge>
              Selected Players
            </div>
            <div className={styles.selectedPlayers}>
              {[...selectedPlayerIds].map(playerId => {
                const player = getPlayerName(playerId);
                if (!player) return null;
                return (
                  <div key={playerId} className={styles.playerChip}>
                    <span>{player.nickname ?? player.fullName}</span>
                    <div
                      className={styles.chipRemove}
                      onClick={() => removePlayer(playerId)}
                      role="button"
                      tabIndex={0}
                    >
                      <Dismiss24Regular fontSize={12} />
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        )}
      </Field>

      <Field label="Add Players">
        <div className={styles.playersGrid}>
          {players.length === 0 ? (
            <div className={styles.emptyState}>
              <Caption1>No active players available</Caption1>
            </div>
          ) : (
            players.map(p => (
              <div
                key={p.playerId}
                className={styles.playerCheckbox}
                onClick={() => togglePlayer(p.playerId)}
                role="button"
                tabIndex={0}
                onKeyDown={(e) => {
                  if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    togglePlayer(p.playerId);
                  }
                }}
                style={{
                  backgroundColor: selectedPlayerIds.has(p.playerId)
                    ? 'rgba(43, 130, 80, 0.1)'
                    : 'transparent',
                  cursor: 'pointer',
                }}
              >
                <Checkbox
                  label={p.nickname ? `${p.fullName} (${p.nickname})` : p.fullName}
                  checked={selectedPlayerIds.has(p.playerId)}
                  onChange={() => togglePlayer(p.playerId)}
                />
              </div>
            ))
          )}
        </div>
      </Field>

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
