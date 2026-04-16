import { useEffect, useRef } from 'react';
import { makeStyles, tokens, Button } from '@fluentui/react-components';
import { ChevronLeft24Regular, ChevronRight24Regular } from '@fluentui/react-icons';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'flex-start',
    gap: '4px',
    width: '100%',
    maxWidth: '100%',
    minWidth: 0,
    boxSizing: 'border-box',
    overflowX: 'auto',
    overflowY: 'hidden',
    overscrollBehaviorX: 'contain',
    padding: '6px 4px',
    scrollbarWidth: 'none',
    '&::-webkit-scrollbar': { display: 'none' },
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
    backgroundColor: '#f7e7b4',
    color: '#4a3a17',
    fontWeight: '700',
    marginInline: '4px',
    border: '1px solid #d9bf6a',
    boxShadow: '0 0 0 2px var(--golf-creme-50), 0 0 0 3px #e3c86d, 0 2px 6px rgba(153,121,41,0.28)',
    transform: 'scale(1.08)',
    zIndex: '1',
    position: 'relative',
  },
  scoredUnderPar: {
    backgroundColor: tokens.colorPaletteGreenBackground3,
    color: tokens.colorNeutralForegroundOnBrand,
    fontWeight: '700',
  },
  scoredPar: {
    backgroundColor: '#1f6feb',
    color: '#ffffff',
    border: '1px solid #1b5fc8',
    boxShadow: 'inset 0 0 0 1px rgba(255,255,255,0.15)',
    fontWeight: '700',
  },
  scoredOverPar: {
    backgroundColor: tokens.colorPaletteRedBackground3,
    color: tokens.colorNeutralForegroundOnBrand,
    fontWeight: '700',
  },
  unscored: {
    backgroundColor: 'var(--golf-creme-50)',
    color: 'var(--golf-ink-soft)',
    border: '1px dashed var(--golf-creme-300)',
    fontWeight: '400',
    opacity: '0.55',
  },
});

interface HoleSelectorProps {
  currentHole: number;
  holeScores: number[];
  holePars: number[];
  onSelectHole: (hole: number) => void;
}

export function HoleSelector({ currentHole, holeScores, holePars, onSelectHole }: HoleSelectorProps) {
  const styles = useStyles();
  const holeRefs = useRef<(HTMLDivElement | null)[]>([]);

  useEffect(() => {
    const el = holeRefs.current[currentHole - 1];
    if (el) el.scrollIntoView({ behavior: 'smooth', inline: 'center', block: 'nearest' });
  }, [currentHole]);

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
        const holeScore = holeScores[hole - 1];
        const holePar = holePars[hole - 1] ?? 0;
        if (hole === currentHole) className += styles.active;
        else if (holeScore > 0) {
          if (holePar > 0 && holeScore < holePar) className += styles.scoredUnderPar;
          else if (holePar > 0 && holeScore === holePar) className += styles.scoredPar;
          else className += styles.scoredOverPar;
        }
        else className += styles.unscored;

        return (
          <div
            key={hole}
            ref={el => { holeRefs.current[hole - 1] = el; }}
            className={className}
            onClick={() => onSelectHole(hole)}
          >
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
