import { makeStyles, tokens, Button, Caption1, Subtitle2 } from '@fluentui/react-components';
import { Add24Regular, Subtract24Regular } from '@fluentui/react-icons';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    gap: '12px',
    padding: '16px',
  },
  holeInfo: {
    display: 'flex',
    gap: '24px',
    alignItems: 'center',
  },
  scoreRow: {
    display: 'flex',
    alignItems: 'center',
    gap: '16px',
  },
  scoreButton: {
    minWidth: '60px',
    minHeight: '60px',
    fontSize: '24px',
  },
  scoreDisplay: {
    fontSize: '48px',
    fontWeight: 'bold',
    minWidth: '80px',
    textAlign: 'center',
  },
  underPar: { color: tokens.colorPaletteGreenForeground1 },
  par: { color: tokens.colorPaletteBlueForeground2 },
  overPar: { color: tokens.colorPaletteRedForeground1 },
  quickButtons: {
    display: 'flex',
    gap: '8px',
    flexWrap: 'wrap',
    justifyContent: 'center',
  },
  birdieButton: {
    backgroundColor: tokens.colorPaletteGreenBackground1,
    color: tokens.colorPaletteGreenForeground1,
    borderColor: tokens.colorPaletteGreenBorder1,
    ':hover': {
      backgroundColor: tokens.colorPaletteGreenBackground2,
    },
  },
  parButton: {
    backgroundColor: tokens.colorPaletteBlueBackground1,
    color: tokens.colorPaletteBlueForeground1,
    borderColor: tokens.colorPaletteBlueBorder1,
    ':hover': {
      backgroundColor: tokens.colorPaletteBlueBackground2,
    },
  },
  bogeyButton: {
    backgroundColor: tokens.colorPaletteRedBackground1,
    color: tokens.colorPaletteRedForeground1,
    borderColor: tokens.colorPaletteRedBorder1,
    ':hover': {
      backgroundColor: tokens.colorPaletteRedBackground2,
    },
  },
});

interface ScoreInputProps {
  holeNumber: number;
  par: number;
  handicapRanking: number;
  handicapStrokes: number;
  currentScore: number;
  onScoreChange: (score: number) => void;
  disabled?: boolean;
}

export function ScoreInput({
  holeNumber,
  par,
  handicapRanking,
  handicapStrokes,
  currentScore,
  onScoreChange,
  disabled,
}: ScoreInputProps) {
  const styles = useStyles();

  const score = currentScore || par;
  const diff = score - par;
  const scoreColorClass =
    diff <= -1 ? styles.underPar :
    diff === 0 ? styles.par :
    styles.overPar;

  return (
    <div className={styles.root}>
      <Subtitle2>Hole {holeNumber}</Subtitle2>
      <div className={styles.holeInfo}>
        <Caption1>Par {par}</Caption1>
        <Caption1>HCP {handicapRanking}</Caption1>
        {handicapStrokes > 0 && <Caption1>+{handicapStrokes} stroke{handicapStrokes > 1 ? 's' : ''}</Caption1>}
      </div>
      <div className={styles.scoreRow}>
        <Button
          className={styles.scoreButton}
          appearance="outline"
          icon={<Subtract24Regular />}
          onClick={() => onScoreChange(Math.max(1, score - 1))}
          disabled={disabled || score <= 1}
        />
        <div className={`${styles.scoreDisplay} ${scoreColorClass}`}>
          {currentScore > 0 ? score : '—'}
        </div>
        <Button
          className={styles.scoreButton}
          appearance="outline"
          icon={<Add24Regular />}
          onClick={() => onScoreChange(Math.min(15, score + 1))}
          disabled={disabled || score >= 15}
        />
      </div>
      <div className={styles.quickButtons}>
        <Button
          appearance="outline"
          className={styles.birdieButton}
          onClick={() => onScoreChange(par - 1)}
          disabled={disabled || par - 1 < 1}
        >
          Birdie ({par - 1})
        </Button>
        <Button
          appearance="outline"
          className={styles.parButton}
          onClick={() => onScoreChange(par)}
          disabled={disabled}
        >
          Par ({par})
        </Button>
        <Button
          appearance="outline"
          className={styles.bogeyButton}
          onClick={() => onScoreChange(par + 1)}
          disabled={disabled}
        >
          Bogey ({par + 1})
        </Button>
      </div>
    </div>
  );
}
