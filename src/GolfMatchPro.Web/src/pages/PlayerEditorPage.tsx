import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  makeStyles,
  Title1,
  Button,
  Input,
  Checkbox,
  Spinner,
  MessageBar,
  MessageBarBody,
  Field,
} from '@fluentui/react-components';
import { Save24Regular, ArrowLeft24Regular } from '@fluentui/react-icons';
import { playerService } from '../services/playerService';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    gap: '16px',
    maxWidth: '500px',
  },
  header: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
  },
  actions: {
    display: 'flex',
    gap: '8px',
  },
});

export function PlayerEditorPage() {
  const styles = useStyles();
  const navigate = useNavigate();
  const { id } = useParams();
  const isEdit = id !== undefined;

  const [loading, setLoading] = useState(isEdit);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [fullName, setFullName] = useState('');
  const [nickname, setNickname] = useState('');
  const [handicapIndex, setHandicapIndex] = useState('0');
  const [isActive, setIsActive] = useState(true);
  const [isGuest, setIsGuest] = useState(false);

  useEffect(() => {
    if (isEdit) {
      playerService.getById(Number(id))
        .then(player => {
          setFullName(player.fullName);
          setNickname(player.nickname ?? '');
          setHandicapIndex(player.handicapIndex.toString());
          setIsActive(player.isActive);
          setIsGuest(player.isGuest);
        })
        .catch(() => setError('Failed to load player.'))
        .finally(() => setLoading(false));
    }
  }, [id, isEdit]);

  const handleSave = async () => {
    setError(null);
    setSaving(true);

    try {
      if (isEdit) {
        await playerService.update(Number(id), {
          fullName,
          nickname: nickname || null,
          handicapIndex: parseFloat(handicapIndex),
          isActive,
          isGuest,
        });
      } else {
        await playerService.create({
          fullName,
          nickname: nickname || null,
          handicapIndex: parseFloat(handicapIndex),
          isGuest,
        });
      }
      navigate('/players');
    } catch (err: any) {
      const data = err?.response?.data;
      if (data?.errors) {
        setError(Array.isArray(data.errors) ? data.errors.join(', ') : JSON.stringify(data.errors));
      } else {
        setError('Failed to save player.');
      }
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <Spinner label="Loading player..." />;

  return (
    <div className={styles.root}>
      <div className={styles.header}>
        <Button
          appearance="subtle"
          icon={<ArrowLeft24Regular />}
          onClick={() => navigate('/players')}
        />
        <Title1>{isEdit ? 'Edit Player' : 'New Player'}</Title1>
      </div>

      {error && (
        <MessageBar intent="error">
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      )}

      <Field label="Full Name" required>
        <Input value={fullName} onChange={(_, d) => setFullName(d.value)} placeholder="e.g., John Smith" />
      </Field>
      <Field label="Nickname">
        <Input value={nickname} onChange={(_, d) => setNickname(d.value)} placeholder="e.g., Smitty" />
      </Field>
      <Field label="Handicap Index" required>
        <Input value={handicapIndex} onChange={(_, d) => setHandicapIndex(d.value)} type="number" step="0.1" min={-10} max={54} />
      </Field>
      <Checkbox
        label="Guest Player"
        checked={isGuest}
        onChange={(_, d) => setIsGuest(d.checked === true)}
      />
      {isEdit && (
        <Checkbox
          label="Active"
          checked={isActive}
          onChange={(_, d) => setIsActive(d.checked === true)}
        />
      )}

      <div className={styles.actions}>
        <Button
          appearance="primary"
          icon={<Save24Regular />}
          onClick={handleSave}
          disabled={saving}
        >
          {saving ? 'Saving...' : 'Save Player'}
        </Button>
        <Button appearance="secondary" onClick={() => navigate('/players')}>
          Cancel
        </Button>
      </div>
    </div>
  );
}
