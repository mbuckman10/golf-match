export interface CourseHoleDto {
  courseHoleId: number;
  holeNumber: number;
  par: number;
  handicapRanking: number;
}

export interface CourseDto {
  courseId: number;
  name: string;
  teeColor: string | null;
  yearOfInfo: number | null;
  courseRating: number;
  slopeRating: number;
  par: number;
  holes: CourseHoleDto[];
}

export interface CreateCourseHoleRequest {
  holeNumber: number;
  par: number;
  handicapRanking: number;
}

export interface CreateCourseRequest {
  name: string;
  teeColor: string | null;
  yearOfInfo: number | null;
  courseRating: number;
  slopeRating: number;
  holes: CreateCourseHoleRequest[];
}

export interface PlayerDto {
  playerId: number;
  fullName: string;
  nickname: string | null;
  handicapIndex: number;
  isActive: boolean;
  isGuest: boolean;
}

export interface CreatePlayerRequest {
  fullName: string;
  nickname: string | null;
  handicapIndex: number;
  isGuest: boolean;
}

export interface UpdatePlayerRequest {
  fullName: string;
  nickname: string | null;
  handicapIndex: number;
  isActive: boolean;
  isGuest: boolean;
}

// Match types
export type MatchStatus = 'Setup' | 'InProgress' | 'Completed';

export interface MatchDto {
  matchId: number;
  matchName: string;
  courseId: number;
  courseName: string;
  courseTeeColor: string | null;
  matchDate: string;
  status: MatchStatus;
  isArchived: boolean;
  createdByPlayerId: number;
  playerCount: number;
}

export interface MatchDetailDto {
  matchId: number;
  matchName: string;
  course: CourseDto;
  matchDate: string;
  status: MatchStatus;
  isArchived: boolean;
  createdByPlayerId: number;
  scores: MatchScoreDto[];
}

export interface MatchScoreDto {
  matchScoreId: number;
  playerId: number;
  playerName: string;
  playerNickname: string | null;
  courseHandicap: number;
  holeScores: number[];
  grossTotal: number;
  netTotal: number;
  reportableScore: number;
  isComplete: boolean;
}

export interface CreateMatchRequest {
  matchName: string;
  courseId: number;
  matchDate: string;
  createdByPlayerId: number;
  playerIds: number[];
}

export interface HoleScoreResultDto {
  playerId: number;
  holeNumber: number;
  score: number;
  grossTotal: number;
  netTotal: number;
  frontNine: number;
  backNine: number;
  holesCompleted: number;
  isComplete: boolean;
}

// Bet types
export type BetType = 'Foursome' | 'Threesome' | 'Fivesome' | 'BestBall' | 'Individual' | 'Skins' | 'IndoTournament' | 'RoundRobin';
export type CompetitionType = 'MatchPlay' | 'MedalPlay';
export type TeamPosition = 'Captain' | 'B' | 'C' | 'D' | 'E';

export interface BetConfigurationDto {
  betConfigId: number;
  matchId: number;
  betType: BetType;
  competitionType: CompetitionType;
  handicapPercentage: number;
  nassauFront: number;
  nassauBack: number;
  nassau18: number;
  totalStrokesBetPerStroke: number | null;
  maxNetScore: number | null;
  investmentOffEnabled: boolean;
  investmentOffAmount: number;
  redemptionEnabled: boolean;
  redemptionAmount: number;
  dunnEnabled: boolean;
  dunnAmount: number;
  autoPressEnabled: boolean;
  pressAmount: number;
  pressDownThreshold: number;
  skinsBuyIn: number | null;
  skinsPerSkinAmount: number | null;
  expenseDeductionPct: number;
  scoresCountingPerHole: number;
  configJson: string | null;
  teams: TeamDto[];
}

export interface CreateBetConfigurationRequest {
  betType: BetType;
  competitionType: CompetitionType;
  handicapPercentage: number;
  nassauFront: number;
  nassauBack: number;
  nassau18: number;
  totalStrokesBetPerStroke: number | null;
  maxNetScore: number | null;
  investmentOffEnabled: boolean;
  investmentOffAmount: number;
  redemptionEnabled: boolean;
  redemptionAmount: number;
  dunnEnabled: boolean;
  dunnAmount: number;
  autoPressEnabled: boolean;
  pressAmount: number;
  pressDownThreshold: number;
  skinsBuyIn: number | null;
  skinsPerSkinAmount: number | null;
  expenseDeductionPct: number;
  scoresCountingPerHole: number;
  configJson: string | null;
}

export interface TeamDto {
  teamId: number;
  teamNumber: number;
  teamName: string | null;
  players: TeamPlayerDto[];
}

export interface TeamPlayerDto {
  teamPlayerId: number;
  playerId: number;
  playerName: string;
  position: TeamPosition;
}

export interface CreateTeamRequest {
  teamNumber: number;
  teamName: string | null;
  players: { playerId: number; position: TeamPosition }[];
}

// Results types
export interface TeamBetResultsDto {
  teamResults: TeamResultDto[];
  matchups: TeamVsTeamResultDto[];
  playerResults: PlayerResultDto[];
}

export interface TeamResultDto {
  teamNumber: number;
  teamName: string | null;
  teamHoleScores: number[];
  isOff: boolean[];
  isRedemption: boolean[];
  totalOffs: number;
  totalRedemptions: number;
  investmentAmount: number;
  nassauTotal: number;
  totalStrokesTotal: number;
  grandTotal: number;
  grandTotalAfterExpense: number;
  teamNetTotal: number;
}

export interface TeamVsTeamResultDto {
  teamANumber: number;
  teamBNumber: number;
  nassauFrontDollars: number;
  nassauBackDollars: number;
  nassau18Dollars: number;
  totalStrokesDollars: number;
  holeByHoleStatus: number[];
  front9Result: number;
  back9Result: number;
  overall18Result: number;
}

export interface PlayerResultDto {
  playerId: number;
  playerName: string;
  teamNumber: number;
  winLoss: number;
  winLossAfterExpense: number;
}

// Individual bet result types
export interface PressResultDto {
  startHole: number;
  endHole: number;
  result: number;
  amount: number;
}

export interface IndividualMatchupResultDto {
  playerAId: number;
  playerAName: string;
  playerBId: number;
  playerBName: string;
  holeByHoleStatus: number[];
  front9Result: number;
  back9Result: number;
  overall18Result: number;
  nassauFrontDollars: number;
  nassauBackDollars: number;
  nassau18Dollars: number;
  presses: PressResultDto[];
  totalPressAmount: number;
  totalAmountPlayerA: number;
}

export interface IndividualPlayerResultDto {
  playerId: number;
  playerName: string;
  winLoss: number;
  winLossAfterExpense: number;
}

export interface IndividualBetResultsDto {
  matchups: IndividualMatchupResultDto[];
  playerResults: IndividualPlayerResultDto[];
}

// Best Ball result types
export interface BestBallMatchupResultDto {
  sheetHangerTeamNumber: number;
  sheetHangerTeamName: string | null;
  opponentTeamNumber: number;
  opponentTeamName: string | null;
  sheetHangerBestBall: number[];
  opponentBestBall: number[];
  holeByHoleStatus: number[];
  front9Result: number;
  back9Result: number;
  overall18Result: number;
  nassauFrontDollars: number;
  nassauBackDollars: number;
  nassau18Dollars: number;
  presses: PressResultDto[];
  totalPressAmount: number;
  totalAmountSheetHanger: number;
}

export interface BestBallPlayerResultDto {
  playerId: number;
  playerName: string;
  teamNumber: number;
  winLoss: number;
  winLossAfterExpense: number;
}

export interface BestBallResultsDto {
  matchups: BestBallMatchupResultDto[];
  playerResults: BestBallPlayerResultDto[];
}

// Best Ball W-L summary
export interface BestBallPlayerSummaryDto {
  playerId: number;
  playerName: string;
  totalWinLoss: number;
  totalWinLossAfterExpense: number;
  matchupsPlayed: number;
  matchupsWon: number;
  matchupsLost: number;
  matchupsTied: number;
}

export interface BestBallWinLossSummaryDto {
  playerSummaries: BestBallPlayerSummaryDto[];
}

// Skins result types
export interface SkinsHoleResultDto {
  holeNumber: number;
  carryIn: number;
  carryOut: number;
  winnerPlayerId: number | null;
  winnerPlayerName: string | null;
  skinsAwarded: number;
  winningScore: number;
  tiedPlayerIds: number[];
}

export interface SkinsPlayerResultDto {
  playerId: number;
  playerName: string;
  skinsWon: number;
  grossWinnings: number;
  netWinnings: number;
}

export interface SkinsResultsDto {
  totalSkinsAwarded: number;
  unresolvedCarrySkins: number;
  totalPot: number;
  amountPerAwardedSkin: number;
  holeResults: SkinsHoleResultDto[];
  playerResults: SkinsPlayerResultDto[];
}

// Tournament result types
export interface PlacePayoutDto {
  place: number;
  percent: number;
}

export interface TournamentConfigJsonDto {
  sponsorMoney: number;
  buyInPerPlayer: number;
  grossPursePercent: number;
  netPursePercent: number;
  eighteenHolePercent: number;
  frontNinePercent: number;
  backNinePercent: number;
  placePayouts: PlacePayoutDto[];
}

export interface TournamentDivisionEntryDto {
  playerId: number;
  playerName: string;
  score: number;
  place: number;
  payout: number;
}

export interface TournamentDivisionResultDto {
  name: string;
  purse: number;
  entries: TournamentDivisionEntryDto[];
}

export interface TournamentLeaderboardEntryDto {
  playerId: number;
  playerName: string;
  gross18: number;
  net18: number;
  grossPayout: number;
  netPayout: number;
  totalPayout: number;
}

export interface TournamentResultsDto {
  prizePool: number;
  grossPurse: number;
  netPurse: number;
  gross18: TournamentDivisionResultDto;
  grossFront9: TournamentDivisionResultDto;
  grossBack9: TournamentDivisionResultDto;
  net18: TournamentDivisionResultDto;
  netFront9: TournamentDivisionResultDto;
  netBack9: TournamentDivisionResultDto;
  leaderboard: TournamentLeaderboardEntryDto[];
}

// Round robin types
export interface MatchupResultDto {
  matchup: number;
  entityAId: number;
  entityAName: string;
  entityBId: number;
  entityBName: string;
  entityAWinLoss: number;
  entityBWinLoss: number;
  resultDetails: string | null;
}

export interface LeaderboardEntryDto {
  entityId: number;
  entityName: string;
  totalWinLoss: number;
  rank: number;
  matchupsPlayed: number;
}

export interface RoundRobinResultDto {
  roundRobinId: number;
  matchId: number;
  betConfigId: number;
  matchups: MatchupResultDto[];
  leaderboard: LeaderboardEntryDto[];
}

// Grand totals types
export interface PlayerGrandTotalDto {
  playerId: number;
  playerName: string;
  foursomesWinLoss: number;
  threesomesWinLoss: number;
  fivesomesWinLoss: number;
  individualWinLoss: number;
  bestBallWinLoss: number;
  skinsGrossWinLoss: number;
  skinsNetWinLoss: number;
  indoTourneyWinLoss: number;
  roundRobinWinLoss: number;
  totalWinLoss: number;
  status: string;
}

export interface GrandTotalsDto {
  matchId: number;
  playerTotals: PlayerGrandTotalDto[];
}
