import { makeStyles, tokens, Title1, Body1, Card, CardHeader } from '@fluentui/react-components';
import { useNavigate } from 'react-router-dom';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    gap: '24px',
  },
  cards: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))',
    gap: '16px',
  },
  card: {
    cursor: 'pointer',
    '&:hover': {
      boxShadow: tokens.shadow8,
    },
  },
});

export function DashboardPage() {
  const styles = useStyles();
  const navigate = useNavigate();

  return (
    <div className={styles.root}>
      <Title1>Dashboard</Title1>
      <Body1>Welcome to Golf Match Pro. Get started by setting up courses and players.</Body1>
      <div className={styles.cards}>
        <Card className={styles.card} onClick={() => navigate('/courses')}>
          <CardHeader header="Manage Courses" description="Add and edit golf courses with hole data" />
        </Card>
        <Card className={styles.card} onClick={() => navigate('/players')}>
          <CardHeader header="Manage Players" description="Manage your player roster and handicaps" />
        </Card>
        <Card className={styles.card} onClick={() => navigate('/matches')}>
          <CardHeader header="Matches" description="Create and manage golf matches (coming soon)" />
        </Card>
      </div>
    </div>
  );
}
