import { useEffect, useRef } from 'react';
import { makeStyles, tokens, Button } from '@fluentui/react-components';
import { ChevronLeft24Regular, ChevronRight24Regular } from '@fluentui/react-icons';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    alignItems: 'center',
    gap: '4px',
    overflowX: 'auto',
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
    backgroundColor: 'var(--golf-green-500)',
    color: '#fefaf1',
    fontWeight: '700',
    boxShadow: '0 0 0 2px var(--golf-creme-50), 0 0 0 3px var(--golf-green-500), 0 2px 6px rgba(43,130,80,0.35)',
    transform: 'scale(1.08)',
    zIndex: '1',
    position: 'relative',
  },
  scored: {
    backgroundColor: 'var(--golf-green-700)',
    color: '#fefaf1',
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
  onSelectHole: (hole: number) => void;
}

export function HoleSelector({ currentHole, holeScores, onSelectHole }: HoleSelectorProps) {
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
        if (hole === currentHole) className += styles.active;
        else if (holeScores[hole - 1] > 0) className += styles.scored;
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
