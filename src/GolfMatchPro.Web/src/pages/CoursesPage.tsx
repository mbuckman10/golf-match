import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  makeStyles,
  tokens,
  Title1,
  Button,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  Spinner,
} from '@fluentui/react-components';
import { Add24Regular } from '@fluentui/react-icons';
import type { CourseDto } from '../types';
import { courseService } from '../services/courseService';

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
  clickableRow: {
    cursor: 'pointer',
    '&:hover': {
      backgroundColor: tokens.colorNeutralBackground1Hover,
    },
  },
});

export function CoursesPage() {
  const styles = useStyles();
  const navigate = useNavigate();
  const [courses, setCourses] = useState<CourseDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    courseService.getAll()
      .then(setCourses)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <Spinner label="Loading courses..." />;

  return (
    <div className={styles.root}>
      <div className={styles.header}>
        <Title1>Courses</Title1>
        <Button
          appearance="primary"
          icon={<Add24Regular />}
          onClick={() => navigate('/courses/new')}
        >
          Add Course
        </Button>
      </div>
      {courses.length === 0 ? (
        <p>No courses yet. Add your first course to get started.</p>
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHeaderCell>Name</TableHeaderCell>
              <TableHeaderCell>Tee</TableHeaderCell>
              <TableHeaderCell>Par</TableHeaderCell>
              <TableHeaderCell>Rating</TableHeaderCell>
              <TableHeaderCell>Slope</TableHeaderCell>
            </TableRow>
          </TableHeader>
          <TableBody>
            {courses.map(course => (
              <TableRow
                key={course.courseId}
                className={styles.clickableRow}
                onClick={() => navigate(`/courses/${course.courseId}`)}
              >
                <TableCell>{course.name}</TableCell>
                <TableCell>{course.teeColor ?? '—'}</TableCell>
                <TableCell>{course.par}</TableCell>
                <TableCell>{course.courseRating}</TableCell>
                <TableCell>{course.slopeRating}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  );
}
