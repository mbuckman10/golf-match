import { makeStyles, Caption1 } from '@fluentui/react-components';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    gap: '16px',
    justifyContent: 'center',
    flexWrap: 'wrap',
    padding: '8px 0',
  },
  stat: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    minWidth: '60px',
  },
  value: {
    fontSize: '20px',
    fontWeight: 'bold',
  },
});

interface RunningTotalsProps {
  holeScores: number[];
  courseHandicap: number;
  coursePar: number;
}

export function RunningTotals({ holeScores, courseHandicap, coursePar }: RunningTotalsProps) {
  const styles = useStyles();

  const front = holeScores.slice(0, 9).filter(s => s > 0).reduce((a, b) => a + b, 0);
  const back = holeScores.slice(9).filter(s => s > 0).reduce((a, b) => a + b, 0);
  const gross = front + back;
  const net = gross > 0 ? gross - courseHandicap : 0;
  const vsPar = gross > 0 ? gross - coursePar : 0;
  const holesPlayed = holeScores.filter(s => s > 0).length;

  return (
    <div className={styles.root}>
      <div className={styles.stat}>
        <div className={styles.value}>{front || '—'}</div>
        <Caption1>Front</Caption1>
      </div>
      <div className={styles.stat}>
        <div className={styles.value}>{back || '—'}</div>
        <Caption1>Back</Caption1>
      </div>
      <div className={styles.stat}>
        <div className={styles.value}>{gross || '—'}</div>
        <Caption1>Gross</Caption1>
      </div>
      <div className={styles.stat}>
        <div className={styles.value}>{net || '—'}</div>
        <Caption1>Net</Caption1>
      </div>
      <div className={styles.stat}>
        <div className={styles.value}>{gross > 0 ? (vsPar > 0 ? `+${vsPar}` : vsPar) : '—'}</div>
        <Caption1>vs Par</Caption1>
      </div>
      <div className={styles.stat}>
        <div className={styles.value}>{holesPlayed}/18</div>
        <Caption1>Holes</Caption1>
      </div>
    </div>
  );
}
