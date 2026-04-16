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
  TableBody
} from '@fluentui/react-components';
import { styles } from './RoundRobinResultsPage.styles';
import type { RoundRobinResultDto, MatchupResultDto, LeaderboardEntryDto } from '../types';

interface RoundRobinResultsPageProps {
  matchId: number;
  roundRobinId: number;
  results?: RoundRobinResultDto;
  isLoading?: boolean;
}

export const RoundRobinResultsPage: React.FC<RoundRobinResultsPageProps> = ({
  matchId: _matchId,
  roundRobinId: _roundRobinId,
  results,
  isLoading: _isLoading = false
}) => {
  const styleClasses = styles();
  const [matchups, setMatchups] = useState<MatchupResultDto[]>([]);
  const [leaderboard, setLeaderboard] = useState<LeaderboardEntryDto[]>([]);

  useEffect(() => {
    if (results) {
      setMatchups(results.matchups || []);
      setLeaderboard(results.leaderboard || []);
    }
  }, [results]);

  return (
    <div className={styleClasses.container}>
      {/* Leaderboard */}
      <Card>
        <CardHeader header={<Body1Strong>Leaderboard</Body1Strong>} />
        <div>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHeaderCell>Rank</TableHeaderCell>
                <TableHeaderCell>Name</TableHeaderCell>
                <TableHeaderCell>Matchups Played</TableHeaderCell>
                <TableHeaderCell>Total W/L</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {leaderboard.map((entry) => (
                <TableRow key={entry.entityId}>
                  <TableCell>{entry.rank}</TableCell>
                  <TableCell>{entry.entityName}</TableCell>
                  <TableCell>{entry.matchupsPlayed}</TableCell>
                  <TableCell
                    className={
                      entry.totalWinLoss > 0
                        ? styleClasses.positiveAmount
                        : entry.totalWinLoss < 0
                        ? styleClasses.negativeAmount
                        : ''
                    }
                  >
                    ${entry.totalWinLoss.toFixed(2)}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </Card>

      {/* Matchups */}
      <Card>
        <CardHeader header={<Body1Strong>All Matchups</Body1Strong>} />
        <div>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHeaderCell>Matchup</TableHeaderCell>
                <TableHeaderCell>Team/Player A</TableHeaderCell>
                <TableHeaderCell>A W/L</TableHeaderCell>
                <TableHeaderCell>Team/Player B</TableHeaderCell>
                <TableHeaderCell>B W/L</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {matchups.map((matchup) => (
                <TableRow key={`${matchup.matchup}`}>
                  <TableCell>#{matchup.matchup}</TableCell>
                  <TableCell>{matchup.entityAName}</TableCell>
                  <TableCell
                    className={
                      matchup.entityAWinLoss > 0
                        ? styleClasses.positiveAmount
                        : matchup.entityAWinLoss < 0
                        ? styleClasses.negativeAmount
                        : ''
                    }
                  >
                    ${matchup.entityAWinLoss.toFixed(2)}
                  </TableCell>
                  <TableCell>{matchup.entityBName}</TableCell>
                  <TableCell
                    className={
                      matchup.entityBWinLoss > 0
                        ? styleClasses.positiveAmount
                        : matchup.entityBWinLoss < 0
                        ? styleClasses.negativeAmount
                        : ''
                    }
                  >
                    ${matchup.entityBWinLoss.toFixed(2)}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </Card>
    </div>
  );
};

export default RoundRobinResultsPage;
