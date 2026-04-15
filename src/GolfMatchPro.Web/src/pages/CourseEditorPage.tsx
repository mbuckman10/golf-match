import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  makeStyles,
  Title1,
  Button,
  Input,
  Label,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  Spinner,
  MessageBar,
  MessageBarBody,
  Field,
} from '@fluentui/react-components';
import { Save24Regular, ArrowLeft24Regular } from '@fluentui/react-icons';
import type { CreateCourseRequest, CreateCourseHoleRequest } from '../types';
import { courseService } from '../services/courseService';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    gap: '16px',
    maxWidth: '900px',
  },
  header: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
  },
  formRow: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))',
    gap: '16px',
  },
  holeInput: {
    width: '70px',
  },
  actions: {
    display: 'flex',
    gap: '8px',
  },
});

function defaultHoles(): CreateCourseHoleRequest[] {
  return Array.from({ length: 18 }, (_, i) => ({
    holeNumber: i + 1,
    par: 4,
    handicapRanking: i + 1,
  }));
}

export function CourseEditorPage() {
  const styles = useStyles();
  const navigate = useNavigate();
  const { id } = useParams();
  const isEdit = id !== undefined;

  const [loading, setLoading] = useState(isEdit);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [name, setName] = useState('');
  const [teeColor, setTeeColor] = useState('');
  const [yearOfInfo, setYearOfInfo] = useState('');
  const [courseRating, setCourseRating] = useState('72.0');
  const [slopeRating, setSlopeRating] = useState('113');
  const [holes, setHoles] = useState<CreateCourseHoleRequest[]>(defaultHoles());

  useEffect(() => {
    if (isEdit) {
      courseService.getById(Number(id))
        .then(course => {
          setName(course.name);
          setTeeColor(course.teeColor ?? '');
          setYearOfInfo(course.yearOfInfo?.toString() ?? '');
          setCourseRating(course.courseRating.toString());
          setSlopeRating(course.slopeRating.toString());
          setHoles(course.holes.map(h => ({
            holeNumber: h.holeNumber,
            par: h.par,
            handicapRanking: h.handicapRanking,
          })));
        })
        .catch(() => setError('Failed to load course.'))
        .finally(() => setLoading(false));
    }
  }, [id, isEdit]);

  const updateHole = (index: number, field: 'par' | 'handicapRanking', value: string) => {
    const num = parseInt(value, 10);
    if (isNaN(num)) return;
    setHoles(prev => prev.map((h, i) => i === index ? { ...h, [field]: num } : h));
  };

  const handleSave = async () => {
    setError(null);
    setSaving(true);

    const request: CreateCourseRequest = {
      name,
      teeColor: teeColor || null,
      yearOfInfo: yearOfInfo ? parseInt(yearOfInfo, 10) : null,
      courseRating: parseFloat(courseRating),
      slopeRating: parseInt(slopeRating, 10),
      holes,
    };

    try {
      if (isEdit) {
        await courseService.update(Number(id), request);
      } else {
        await courseService.create(request);
      }
      navigate('/courses');
    } catch (err: any) {
      const data = err?.response?.data;
      if (data?.errors) {
        setError(Array.isArray(data.errors) ? data.errors.join(', ') : JSON.stringify(data.errors));
      } else {
        setError('Failed to save course.');
      }
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <Spinner label="Loading course..." />;

  return (
    <div className={styles.root}>
      <div className={styles.header}>
        <Button
          appearance="subtle"
          icon={<ArrowLeft24Regular />}
          onClick={() => navigate('/courses')}
        />
        <Title1>{isEdit ? 'Edit Course' : 'New Course'}</Title1>
      </div>

      {error && (
        <MessageBar intent="error">
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      )}

      <div className={styles.formRow}>
        <Field label="Course Name" required>
          <Input value={name} onChange={(_, d) => setName(d.value)} placeholder="e.g., Pine Valley" />
        </Field>
        <Field label="Tee Color">
          <Input value={teeColor} onChange={(_, d) => setTeeColor(d.value)} placeholder="e.g., Blue" />
        </Field>
        <Field label="Year of Info">
          <Input value={yearOfInfo} onChange={(_, d) => setYearOfInfo(d.value)} type="number" />
        </Field>
        <Field label="Course Rating" required>
          <Input value={courseRating} onChange={(_, d) => setCourseRating(d.value)} type="number" step="0.1" />
        </Field>
        <Field label="Slope Rating" required>
          <Input value={slopeRating} onChange={(_, d) => setSlopeRating(d.value)} type="number" />
        </Field>
      </div>

      <Label weight="semibold" size="large">Hole Data</Label>
      <Table size="small">
        <TableHeader>
          <TableRow>
            <TableHeaderCell>Hole</TableHeaderCell>
            <TableHeaderCell>Par</TableHeaderCell>
            <TableHeaderCell>Handicap Ranking</TableHeaderCell>
          </TableRow>
        </TableHeader>
        <TableBody>
          {holes.map((hole, i) => (
            <TableRow key={hole.holeNumber}>
              <TableCell>{hole.holeNumber}</TableCell>
              <TableCell>
                <Input
                  className={styles.holeInput}
                  value={hole.par.toString()}
                  onChange={(_, d) => updateHole(i, 'par', d.value)}
                  type="number"
                  min={3}
                  max={6}
                />
              </TableCell>
              <TableCell>
                <Input
                  className={styles.holeInput}
                  value={hole.handicapRanking.toString()}
                  onChange={(_, d) => updateHole(i, 'handicapRanking', d.value)}
                  type="number"
                  min={1}
                  max={18}
                />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <div>
        <Label>
          Handicap Ranking Sum: {holes.reduce((s, h) => s + h.handicapRanking, 0)}{' '}
          {holes.reduce((s, h) => s + h.handicapRanking, 0) !== 171 && '(must be 171)'}
        </Label>
      </div>
      <div>
        <Label>Total Par: {holes.reduce((s, h) => s + h.par, 0)}</Label>
      </div>

      <div className={styles.actions}>
        <Button
          appearance="primary"
          icon={<Save24Regular />}
          onClick={handleSave}
          disabled={saving}
        >
          {saving ? 'Saving...' : 'Save Course'}
        </Button>
        <Button appearance="secondary" onClick={() => navigate('/courses')}>
          Cancel
        </Button>
      </div>
    </div>
  );
}
