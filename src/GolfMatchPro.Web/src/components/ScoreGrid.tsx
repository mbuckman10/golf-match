import { makeStyles, tokens } from '@fluentui/react-components';
import type { MatchScoreDto, CourseHoleDto } from '../types';

const useStyles = makeStyles({
  wrapper: {
    overflowX: 'auto',
  },
  table: {
    borderCollapse: 'collapse',
    fontSize: '13px',
    width: '100%',
    minWidth: '900px',
  },
  th: {
    padding: '6px 8px',
    backgroundColor: tokens.colorNeutralBackground3,
    borderBottom: `1px solid ${tokens.colorNeutralStroke2}`,
    textAlign: 'center',
    fontWeight: 'bold',
    whiteSpace: 'nowrap',
  },
  td: {
    padding: '4px 8px',
    borderBottom: `1px solid ${tokens.colorNeutralStroke2}`,
    textAlign: 'center',
  },
  playerName: {
    textAlign: 'left',
    fontWeight: 'bold',
    whiteSpace: 'nowrap',
  },
  subtotal: {
    backgroundColor: tokens.colorNeutralBackground2,
    fontWeight: 'bold',
  },
  underPar: { color: tokens.colorPaletteGreenForeground1, fontWeight: 'bold' },
  par: { color: tokens.colorPaletteBlueForeground2 },
  overPar: { color: tokens.colorPaletteRedForeground1, fontWeight: 'bold' },
});

interface ScoreGridProps {
  scores: MatchScoreDto[];
  holes: CourseHoleDto[];
}

export function ScoreGrid({ scores, holes }: ScoreGridProps) {
  const styles = useStyles();

  const sortedHoles = [...holes].sort((a, b) => a.holeNumber - b.holeNumber);
  const frontHoles = sortedHoles.slice(0, 9);
  const backHoles = sortedHoles.slice(9);

  const getScoreClass = (score: number, par: number) => {
    if (score === 0) return '';
    const diff = score - par;
    if (diff <= -1) return styles.underPar;
    if (diff === 0) return styles.par;
    if (diff >= 1) return styles.overPar;
    return '';
  };

  return (
    <div className={styles.wrapper}>
      <table className={styles.table}>
        <thead>
          <tr>
            <th className={styles.th}>Player</th>
            {frontHoles.map(h => (
              <th key={h.holeNumber} className={styles.th}>{h.holeNumber}</th>
            ))}
            <th className={`${styles.th} ${styles.subtotal}`}>Out</th>
            {backHoles.map(h => (
              <th key={h.holeNumber} className={styles.th}>{h.holeNumber}</th>
            ))}
            <th className={`${styles.th} ${styles.subtotal}`}>In</th>
            <th className={`${styles.th} ${styles.subtotal}`}>Gross</th>
            <th className={`${styles.th} ${styles.subtotal}`}>Net</th>
          </tr>
          <tr>
            <td className={`${styles.td} ${styles.playerName}`}>Par</td>
            {frontHoles.map(h => (
              <td key={h.holeNumber} className={styles.td}>{h.par}</td>
            ))}
            <td className={`${styles.td} ${styles.subtotal}`}>{frontHoles.reduce((s, h) => s + h.par, 0)}</td>
            {backHoles.map(h => (
              <td key={h.holeNumber} className={styles.td}>{h.par}</td>
            ))}
            <td className={`${styles.td} ${styles.subtotal}`}>{backHoles.reduce((s, h) => s + h.par, 0)}</td>
            <td className={`${styles.td} ${styles.subtotal}`}>{sortedHoles.reduce((s, h) => s + h.par, 0)}</td>
            <td className={styles.td}>—</td>
          </tr>
          <tr>
            <td className={`${styles.td} ${styles.playerName}`}>HCP</td>
            {sortedHoles.slice(0, 9).map(h => (
              <td key={h.holeNumber} className={styles.td}>{h.handicapRanking}</td>
            ))}
            <td className={styles.td}></td>
            {sortedHoles.slice(9).map(h => (
              <td key={h.holeNumber} className={styles.td}>{h.handicapRanking}</td>
            ))}
            <td className={styles.td}></td>
            <td className={styles.td}></td>
            <td className={styles.td}></td>
          </tr>
        </thead>
        <tbody>
          {scores.map(s => {
            const frontScore = s.holeScores.slice(0, 9).filter(v => v > 0).reduce((a, b) => a + b, 0);
            const backScore = s.holeScores.slice(9).filter(v => v > 0).reduce((a, b) => a + b, 0);

            return (
              <tr key={s.playerId}>
                <td className={`${styles.td} ${styles.playerName}`}>
                  {s.playerNickname ?? s.playerName}
                  <br />
                  <span style={{ fontWeight: 'normal', fontSize: '11px' }}>CH: {s.courseHandicap}</span>
                </td>
                {frontHoles.map((h, i) => (
                  <td key={h.holeNumber} className={`${styles.td} ${getScoreClass(s.holeScores[i], h.par)}`}>
                    {s.holeScores[i] || ''}
                  </td>
                ))}
                <td className={`${styles.td} ${styles.subtotal}`}>{frontScore || ''}</td>
                {backHoles.map((h, i) => (
                  <td key={h.holeNumber} className={`${styles.td} ${getScoreClass(s.holeScores[i + 9], h.par)}`}>
                    {s.holeScores[i + 9] || ''}
                  </td>
                ))}
                <td className={`${styles.td} ${styles.subtotal}`}>{backScore || ''}</td>
                <td className={`${styles.td} ${styles.subtotal}`}>{s.grossTotal || ''}</td>
                <td className={`${styles.td} ${styles.subtotal}`}>{s.netTotal || ''}</td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
