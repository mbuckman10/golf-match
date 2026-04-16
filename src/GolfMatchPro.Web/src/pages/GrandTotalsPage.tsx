import { useState, useEffect } from 'react';
import {
  Card,
  CardHeader,
  Body1Strong,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableCell,
  TableBody,
  Checkbox,
  Button
} from '@fluentui/react-components';
import { styles } from './GrandTotalsPage.styles';
import type { GrandTotalsDto, PlayerGrandTotalDto } from '../types';

interface GrandTotalsPageProps {
  matchId: number;
  grandTotals?: GrandTotalsDto;
  isLoading?: boolean;
  onCalculate?: (filters: GrandTotalFilters) => Promise<void>;
}

interface GrandTotalFilters {
  includeFoursomes: boolean;
  includeThreesomes: boolean;
  includeFivesomes: boolean;
  includeIndividual: boolean;
  includeBestBall: boolean;
  includeSkinsGross: boolean;
  includeSkinsNet: boolean;
  includeIndoTourney: boolean;
  includeRoundRobins: boolean;
}

export const GrandTotalsPage: React.FC<GrandTotalsPageProps> = ({
  matchId: _matchId,
  grandTotals,
  isLoading = false,
  onCalculate
}) => {
  const styleClasses = styles();
  const [playerTotals, setPlayerTotals] = useState<PlayerGrandTotalDto[]>([]);
  const [filters, setFilters] = useState<GrandTotalFilters>({
    includeFoursomes: true,
    includeThreesomes: true,
    includeFivesomes: true,
    includeIndividual: true,
    includeBestBall: true,
    includeSkinsGross: true,
    includeSkinsNet: true,
    includeIndoTourney: true,
    includeRoundRobins: true
  });

  useEffect(() => {
    if (grandTotals) {
      setPlayerTotals(grandTotals.playerTotals || []);
    }
  }, [grandTotals]);

  const handleFilterChange = (key: keyof GrandTotalFilters, value: boolean) => {
    const newFilters = { ...filters, [key]: value };
    setFilters(newFilters);
  };

  const handleRecalculate = async () => {
    if (onCalculate) {
      await onCalculate(filters);
    }
  };

  return (
    <div className={styleClasses.container}>
      {/* Filters */}
      <Card>
        <CardHeader header={<Body1Strong>Filter Bet Types</Body1Strong>} />
        <div>
          <div className={styleClasses.filterGrid}>
            <Checkbox
              label="Foursomes"
              checked={filters.includeFoursomes}
              onChange={(_, data) => handleFilterChange('includeFoursomes', !!data.checked)}
            />
            <Checkbox
              label="Threesomes"
              checked={filters.includeThreesomes}
              onChange={(_, data) => handleFilterChange('includeThreesomes', !!data.checked)}
            />
            <Checkbox
              label="Fivesomes"
              checked={filters.includeFivesomes}
              onChange={(_, data) => handleFilterChange('includeFivesomes', !!data.checked)}
            />
            <Checkbox
              label="Individual"
              checked={filters.includeIndividual}
              onChange={(_, data) => handleFilterChange('includeIndividual', !!data.checked)}
            />
            <Checkbox
              label="Best Ball"
              checked={filters.includeBestBall}
              onChange={(_, data) => handleFilterChange('includeBestBall', !!data.checked)}
            />
            <Checkbox
              label="Skins (Gross)"
              checked={filters.includeSkinsGross}
              onChange={(_, data) => handleFilterChange('includeSkinsGross', !!data.checked)}
            />
            <Checkbox
              label="Skins (Net)"
              checked={filters.includeSkinsNet}
              onChange={(_, data) => handleFilterChange('includeSkinsNet', !!data.checked)}
            />
            <Checkbox
              label="Tournament"
              checked={filters.includeIndoTourney}
              onChange={(_, data) => handleFilterChange('includeIndoTourney', !!data.checked)}
            />
            <Checkbox
              label="Round Robins"
              checked={filters.includeRoundRobins}
              onChange={(_, data) => handleFilterChange('includeRoundRobins', !!data.checked)}
            />
          </div>
          <Button
            appearance="primary"
            onClick={handleRecalculate}
            disabled={isLoading || !onCalculate}
            style={{ marginTop: '16px' }}
          >
            {isLoading ? 'Recalculating...' : 'Recalculate Totals'}
          </Button>
        </div>
      </Card>

      {/* Grand Totals Table */}
      <Card>
        <CardHeader header={<Body1Strong>Grand Totals by Player</Body1Strong>} />
        <div>
          {playerTotals.length === 0 ? (
            <p>No player data available. Configure and complete bets to see results.</p>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHeaderCell>Player</TableHeaderCell>
                  <TableHeaderCell>Foursomes</TableHeaderCell>
                  <TableHeaderCell>Threesomes</TableHeaderCell>
                  <TableHeaderCell>Fivesomes</TableHeaderCell>
                  <TableHeaderCell>Individual</TableHeaderCell>
                  <TableHeaderCell>Best Ball</TableHeaderCell>
                  <TableHeaderCell>Skins</TableHeaderCell>
                  <TableHeaderCell>Tournament</TableHeaderCell>
                  <TableHeaderCell>Round Robin</TableHeaderCell>
                  <TableHeaderCell>Total W/L</TableHeaderCell>
                </TableRow>
              </TableHeader>
              <TableBody>
                {playerTotals.map((player) => (
                  <TableRow key={player.playerId}>
                    <TableCell>{player.playerName}</TableCell>
                    <TableCell className={getCellClass(player.foursomesWinLoss, styleClasses)}>
                      ${player.foursomesWinLoss.toFixed(2)}
                    </TableCell>
                    <TableCell className={getCellClass(player.threesomesWinLoss, styleClasses)}>
                      ${player.threesomesWinLoss.toFixed(2)}
                    </TableCell>
                    <TableCell className={getCellClass(player.fivesomesWinLoss, styleClasses)}>
                      ${player.fivesomesWinLoss.toFixed(2)}
                    </TableCell>
                    <TableCell className={getCellClass(player.individualWinLoss, styleClasses)}>
                      ${player.individualWinLoss.toFixed(2)}
                    </TableCell>
                    <TableCell className={getCellClass(player.bestBallWinLoss, styleClasses)}>
                      ${player.bestBallWinLoss.toFixed(2)}
                    </TableCell>
                    <TableCell className={getCellClass(player.skinsGrossWinLoss + player.skinsNetWinLoss, styleClasses)}>
                      ${(player.skinsGrossWinLoss + player.skinsNetWinLoss).toFixed(2)}
                    </TableCell>
                    <TableCell className={getCellClass(player.indoTourneyWinLoss, styleClasses)}>
                      ${player.indoTourneyWinLoss.toFixed(2)}
                    </TableCell>
                    <TableCell className={getCellClass(player.roundRobinWinLoss, styleClasses)}>
                      ${player.roundRobinWinLoss.toFixed(2)}
                    </TableCell>
                    <TableCell className={styleClasses.total + ' ' + getCellClass(player.totalWinLoss, styleClasses)}>
                      ${player.totalWinLoss.toFixed(2)}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </div>
      </Card>
    </div>
  );
};

function getCellClass(value: number, styleClasses: ReturnType<typeof styles>): string {
  if (value > 0) return styleClasses.positiveAmount;
  if (value < 0) return styleClasses.negativeAmount;
  return '';
}

export default GrandTotalsPage;
