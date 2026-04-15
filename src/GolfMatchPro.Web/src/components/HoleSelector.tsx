import { makeStyles, tokens, Button } from '@fluentui/react-components';
import { ChevronLeft24Regular, ChevronRight24Regular } from '@fluentui/react-icons';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    alignItems: 'center',
    gap: '4px',
    overflowX: 'auto',
    padding: '4px 0',
  },
  hole: {
    minWidth: '40px',
    height: '40px',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: tokens.borderRadiusMedium,
    cursor: 'pointer',
    fontWeight: 'bold',
    fontSize: '14px',
    flexShrink: 0,
  },
  active: {
    backgroundColor: tokens.colorBrandBackground,
    color: tokens.colorNeutralForegroundOnBrand,
  },
  scored: {
    backgroundColor: tokens.colorNeutralBackground3,
    color: tokens.colorNeutralForeground1,
  },
  unscored: {
    backgroundColor: tokens.colorNeutralBackground2,
    color: tokens.colorNeutralForeground3,
  },
});

interface HoleSelectorProps {
  currentHole: number;
  holeScores: number[];
  onSelectHole: (hole: number) => void;
}

export function HoleSelector({ currentHole, holeScores, onSelectHole }: HoleSelectorProps) {
  const styles = useStyles();

  return (
    <div className={styles.root}>
      <Button
        appearance="subtle"
        icon={<ChevronLeft24Regular />}
        size="small"
        disabled={currentHole <= 1}
        onClick={() => onSelectHole(currentHole - 1)}
      />
      {Array.from({ length: 18 }, (_, i) => i + 1).map(hole => {
        let className = styles.hole + ' ';
        if (hole === currentHole) className += styles.active;
        else if (holeScores[hole - 1] > 0) className += styles.scored;
        else className += styles.unscored;

        return (
          <div key={hole} className={className} onClick={() => onSelectHole(hole)}>
            {hole}
          </div>
        );
      })}
      <Button
        appearance="subtle"
        icon={<ChevronRight24Regular />}
        size="small"
        disabled={currentHole >= 18}
        onClick={() => onSelectHole(currentHole + 1)}
      />
    </div>
  );
}
