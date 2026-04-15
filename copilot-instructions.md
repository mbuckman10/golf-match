# Golf Match Pro — Web App Specification & GitHub Copilot Instructions

> **Purpose:** This document provides a complete specification for rebuilding a country club golf gambling Excel workbook (.xlsm) as a modern, mobile-friendly web application using the Microsoft technology stack and React.

---

## 1. Source File Breakdown

The original Excel workbook (`TEMPLATE_-_Installed_4-15-2025-BACKUP.xlsm`) contains **26 worksheets** organized into functional groups. The file uses VBA macros, heavy cross-sheet VLOOKUP/INDEX formulas, conditional formatting, and protected input cells to manage an elaborate set of interconnected golf gambling games for a group of ~50 regular players.

### 1.1 Reference Data Sheets

| Sheet | Purpose |
|---|---|
| **Courses** | Master list of golf courses (~38 slots). Each course stores: name, tee color, year of info, handicap ranking per hole (1–18, must total 171), par per hole (1–18), course rating, and slope rating. New courses are added by filling light-blue cells. |
| **Players-Scores** | Central scorecard. Stores a roster of ~75 player slots (49 named + guests). Per player: name, nickname, handicap index, computed course handicap, hole-by-hole gross scores (18 holes), front/back/total sums, net score, and a "Reportable Score for Handicap" column with Equitable Stroke Control (ESC) applied. Course selection drives par/handicap hole layout across the top. |
| **DateSheet** | License management (install date, expiry). Not needed in web app. |

### 1.2 Team Betting Sheets

These three sheets share an identical structure, differing only in team size:

| Sheet | Team Size | Scores Counting |
|---|---|---|
| **Foursomes** | 4 players per team | Best 2 of 4 net scores per hole |
| **Threesomes** | 3 players per team | Best 2 of 3 net scores per hole |
| **Fivesomes** | 5 players per team | Best 3 of 5 net scores per hole |

**Common configuration for all three:**

- **Number of Teams:** 1–15 configurable
- **Nassau Bet:** Separate $ amounts for Front 9, Back 9, and 18-hole totals (default $5/$5/$5)
- **Handicap Allocation:** Percentage of handicap used (e.g., 100%, 75%), strokes allocated off the lowest handicap in the group
- **Total Strokes Bet:** $ per stroke on aggregate team net score, with configurable Maximum Net Score cap (default 82)
- **Investment Bets:** Two independent toggleable options:
  - "Off" bet — when all team members score over par+handicap on a hole, team "goes off." $ lost per off per opposing team (default $6)
  - Redemption / "N-Ons" bet — team earns $ back per redemption (a hole where all members score at or under their net par). Alternatively, a "4-Ons" / "3-Ons" / "5-Ons" bet variant
- **Dunn Investment:** Optional additional bet with configurable amount
- **Deduction for Group Expenses:** Percentage taken from winnings for a group fund (default 5–10%)
- **Team Assignment:** Enter player numbers (from Players-Scores sheet) for Captain, B, C, D (and E for Fivesomes) positions per team
- **Results:** Per-team detail (hole-by-hole net scores, running Nassau totals, off/redemption markers) plus a summary matrix showing every team-vs-team result in Nassau, Investment, and Total Strokes categories. Final W/L per man and per team after expense deduction.

### 1.3 Best Ball Betting Sheets

| Sheet | Purpose |
|---|---|
| **Best Ball Bets** (×4 copies) | 2v2 "Four Ball" (Best Ball) calculator. A permanent "Sheet Hanger" team plays against up to 46 opposing 2-man teams. Supports **Match Play** or **Medal Play**. Configurable Nassau amounts. Each matchup shows hole-by-hole net scores, running match status (up/down), and front/back/18 results. |
| **Best Ball $ W-L** (×4 copies) | Summary win/loss ledger for all Best Ball matchups. Aggregates each player's total winnings/losses across all their Best Ball bets. |

**Combo Bets** (within Best Ball sheets):
- **Wheel** (2 vs 3 players)
- **Rope** (2 vs 4 players)
- **Igg** (3 vs 3 players)
- **Big Wheel** (3 vs 4 players)
- **Big Igg** (4 vs 4 players)
- Each combo calculates max exposure per team

### 1.4 Individual Betting Sheets

| Sheet | Purpose |
|---|---|
| **Individual Bets** (×2 copies) | 1v1 match calculator. Configurable for Match Play or Medal Play. Nassau bets (Front/Back/18). **Automatic Presses** option: configurable press amount and trigger (e.g., $25 auto press when 2-down). Shows hole-by-hole running match status, results with press wins/losses tallied. Up to 6 opponents per "Bets for Player." |

### 1.5 Skins Sheets

| Sheet | Purpose |
|---|---|
| **Skins** (×2 copies — typically one Gross, one Net) | Skins game for all or selected players. Configurable: buy-in per player or amount per skin, handicap percentage (0% = gross skins, 100% = net skins). Computes lowest score per hole, determines if a skin is won (unique low) or carries over. Shows total skins pot, amount per skin, and per-player results table mapping which player won which hole's skin. |

### 1.6 Tournament Sheets

| Sheet | Purpose |
|---|---|
| **Indo Tourney** | Individual stroke-play tournament. Configurable: sponsor prize money, per-player buy-in ($20 default), expense deduction %. Allocates prize money between Gross and Net purses (must total 100%). Each purse split across 18-hole, Front 9, and Back 9 subdivisions. Configurable places paid (1st–15th) with percentage distribution. Includes a "Default %" table for quick allocation based on field size. |

### 1.7 Round Robin Sheets

| Sheet | Purpose |
|---|---|
| **Foursomes Round Robin** | Round-robin variant of the Foursomes bet where every team plays every other team. |
| **Indo Round Robin** | Individual round-robin where every player plays every other player. |
| **BB — Round Robin** | Best Ball round robin. All 2-man teams play each other. Match Play with automatic presses. Up to 23 teams. Results computed via a "Calculate Round Robin" macro. |

### 1.8 Aggregation & Output Sheets

| Sheet | Purpose |
|---|---|
| **Grand Totals** | Master ledger. Toggleable TRUE/FALSE flags determine which bet sheets (Foursomes, Threesomes, Fivesomes, Skins ×2, Indo, Indo RR, BB Bets ×4, BB RR, Foursomes RR) are included in each player's grand total. Shows per-player aggregate W/L across all active bets. |
| **Print P-S** | Print-formatted version of the Players-Scores sheet. |
| **Instructions** | User guide (content stored in VBA-generated shapes/text boxes in the original). |
| **Sheet1** | Scratch/utility sheet. |

---

## 2. Core Business Logic — Calculation Engine

### 2.1 Handicap Calculations

```
Course Handicap = Handicap Index × (Slope Rating / 113)  →  rounded to nearest integer
Adjusted Handicap = Course Handicap × Percentage Used  →  rounded
Playing Handicap = Adjusted Handicap - Lowest Adjusted Handicap in Group
```

Strokes are distributed to holes based on the course's **handicap ranking** (1 = hardest hole, 18 = easiest). A player with a Playing Handicap of 10 gets one stroke on holes ranked 1–10. Players with handicaps > 18 get two strokes on the hardest holes (e.g., handicap 22 = 2 strokes on holes ranked 1–4, 1 stroke on holes 5–18).

### 2.2 Equitable Stroke Control (ESC)

Maximum score per hole is capped based on Course Handicap:

| Course Handicap | Max Score Per Hole |
|---|---|
| ≤ 4 | Double Bogey (Par + 2) |
| 5–9 | 7 |
| 10–14 | 7 |
| 15–19 | 7 (some use 8) |
| 20–24 | 8 |
| 25–29 | 8 |
| 30–34 | 9 |
| 35–39 | 9 |
| 40+ | 10 |

The "Reportable Score" applies ESC caps before submitting for handicap purposes. The actual gambling bets use the real gross scores.

### 2.3 Nassau Bet Calculation

A Nassau is three separate bets: Front 9, Back 9, and overall 18 holes. The scoring method depends on bet type:

**Medal Play (Stroke Play):**
- Compare total net strokes per side. Lower total wins.
- Margin of victory doesn't matter for the Nassau — it's a fixed-amount bet per side.

**Match Play:**
- Compare net scores hole by hole. Lower net wins the hole. Ties halve.
- Track running status (e.g., "2 UP" / "3 DOWN").
- Front 9 result = status after hole 9. Back 9 = independent match holes 10–18. 18-hole = cumulative.

### 2.4 Team Best-Ball (N-Ball) Scoring

For each hole, take the best N net scores from the team (e.g., best 2 of 4 in Foursomes). Sum those N scores. That's the team's score for the hole. Running totals determine Nassau.

### 2.5 Investment Bets ("Offs" and Redemptions)

**Going "Off":** On a given hole, if ALL team members score strictly over par (net), the team "goes off." Each off costs `$ per off × (number of opposing teams)`.

**Redemption:** On a given hole, if ALL team members score at or under par (net), the team earns a redemption. Each redemption earns `$ per redemption × (number of opposing teams)`.

**"N-Ons" (alternative):** If ALL N team members make par or better (net) on a hole, team wins `$ per N-On × (number of opposing teams)`.

These are tracked independently from the Nassau.

### 2.6 Automatic Presses (Individual & BB Bets)

When a player/team falls N-down (configurable, default 2), a new "press" bet automatically starts from that hole forward. Each press is a separate Nassau at the configured press amount. Press wins/losses are tallied independently and added to the overall result.

### 2.7 Skins

For each hole, the player with the uniquely lowest score (gross or net, depending on config) wins a "skin." If two or more players tie for low, the skin carries over to the next hole (accumulates). Payout is either: (a) split the total pot by total skins won, or (b) fixed amount per skin.

### 2.8 Tournament Payouts

Prize pool = (Sponsor Money) + (Per-Player Buy-In × Number of Players) - (Expense Deduction %).

Split into Gross % and Net %. Each subdivision (18-hole, Front 9, Back 9) has its own allocation and places-paid structure.

---

## 3. Suggested Architecture (Microsoft + React)

### 3.1 Technology Stack

| Layer | Technology | Purpose |
|---|---|---|
| **Frontend** | React 18+ with TypeScript | SPA, mobile-first responsive UI |
| **UI Framework** | Fluent UI React (v9) | Microsoft design system, accessible components |
| **State Management** | Zustand or React Context + useReducer | Client-side state for active scorecard |
| **Routing** | React Router v6 | Page navigation |
| **Backend API** | ASP.NET Core 8 Web API (C#) | REST endpoints, business logic, calculation engine |
| **ORM** | Entity Framework Core 8 | Database access |
| **Database** | Azure SQL Database | Relational data storage |
| **Authentication** | Microsoft Entra ID (Azure AD) | Player login via Microsoft accounts |
| **Hosting (API)** | Azure App Service | Scalable API hosting |
| **Hosting (Frontend)** | Azure Static Web Apps | CDN-backed React hosting with Entra ID integration |
| **Real-Time** | ASP.NET Core SignalR | Live score updates pushed to all connected clients |
| **File Storage** | Azure Blob Storage | Course images, exported scorecards |
| **CI/CD** | Azure DevOps Pipelines | Build, test, deploy |
| **Monitoring** | Azure Application Insights | Telemetry, error tracking |

### 3.2 High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────┐
│                  Azure Static Web Apps               │
│  ┌───────────────────────────────────────────────┐   │
│  │         React + TypeScript + Fluent UI        │   │
│  │  ┌──────┐ ┌──────────┐ ┌──────────────────┐  │   │
│  │  │Score │ │ Bet Mgmt │ │ Results/Leaderboard│ │   │
│  │  │Entry │ │ Config   │ │ Dashboard         │  │   │
│  │  └──┬───┘ └────┬─────┘ └────────┬─────────┘  │   │
│  │     └──────────┼────────────────┘             │   │
│  │                │ REST + SignalR                │   │
│  └────────────────┼──────────────────────────────┘   │
└───────────────────┼──────────────────────────────────┘
                    │
┌───────────────────┼──────────────────────────────────┐
│          Azure App Service (ASP.NET Core 8)          │
│  ┌────────────────┼──────────────────────────────┐   │
│  │  ┌─────────────▼───────────┐                  │   │
│  │  │     API Controllers     │                  │   │
│  │  │  /api/matches           │                  │   │
│  │  │  /api/scores            │                  │   │
│  │  │  /api/bets              │                  │   │
│  │  │  /api/players           │                  │   │
│  │  │  /api/courses           │                  │   │
│  │  └─────────┬───────────────┘                  │   │
│  │            │                                  │   │
│  │  ┌─────────▼───────────────┐  ┌────────────┐  │   │
│  │  │  Calculation Engine     │  │  SignalR    │  │   │
│  │  │  (Handicaps, Nassau,    │  │  Hub        │  │   │
│  │  │   Skins, Investments,   │  │  (Live      │  │   │
│  │  │   Presses, Tournaments) │  │   Scores)   │  │   │
│  │  └─────────┬───────────────┘  └────────────┘  │   │
│  │            │                                  │   │
│  │  ┌─────────▼───────────────┐                  │   │
│  │  │  Entity Framework Core  │                  │   │
│  │  └─────────┬───────────────┘                  │   │
│  └────────────┼──────────────────────────────────┘   │
└───────────────┼──────────────────────────────────────┘
                │
┌───────────────▼──────────────────────────────────────┐
│              Azure SQL Database                       │
│  ┌────────┐ ┌────────┐ ┌────────┐ ┌──────────────┐   │
│  │Courses │ │Players │ │Matches │ │ Bets/Results │   │
│  └────────┘ └────────┘ └────────┘ └──────────────┘   │
└──────────────────────────────────────────────────────┘
```

### 3.3 Database Schema (Entity Framework Core Models)

```
Courses
├── CourseId (PK)
├── Name (string)
├── TeeColor (string)
├── YearOfInfo (int)
├── CourseRating (decimal)
├── SlopeRating (int)
├── Par (int, computed from holes)
└── CourseHoles[] (1:many)

CourseHoles
├── CourseHoleId (PK)
├── CourseId (FK)
├── HoleNumber (1–18)
├── Par (int)
└── HandicapRanking (1–18)

Players
├── PlayerId (PK)
├── FullName (string)
├── Nickname (string)
├── HandicapIndex (decimal)
├── IsActive (bool)
├── IsGuest (bool)
└── EntraUserId (string, nullable — for linked accounts)

Matches
├── MatchId (PK)
├── CourseId (FK)
├── MatchDate (DateOnly)
├── Status (enum: Setup, InProgress, Completed)
├── CreatedBy (FK → Players)
└── MatchScores[] (1:many)

MatchScores
├── MatchScoreId (PK)
├── MatchId (FK)
├── PlayerId (FK)
├── CourseHandicap (int, computed at match creation)
├── HoleScores (JSON or 18 int columns: Hole1–Hole18)
├── GrossTotal (int, computed)
├── NetTotal (int, computed)
├── ReportableScore (int, ESC-adjusted)
└── IsComplete (bool)

BetConfigurations
├── BetConfigId (PK)
├── MatchId (FK)
├── BetType (enum: Foursome, Threesome, Fivesome, BestBall,
│            Individual, Skins, IndoTournament, RoundRobin)
├── CompetitionType (enum: MatchPlay, MedalPlay)
├── HandicapPercentage (decimal)
├── NassauFront (decimal)
├── NassauBack (decimal)
├── Nassau18 (decimal)
├── TotalStrokesBetPerStroke (decimal, nullable)
├── MaxNetScore (int, nullable)
├── InvestmentOffEnabled (bool)
├── InvestmentOffAmount (decimal)
├── RedemptionEnabled (bool)
├── RedemptionAmount (decimal)
├── DunnEnabled (bool)
├── DunnAmount (decimal)
├── AutoPressEnabled (bool)
├── PressAmount (decimal)
├── PressDownThreshold (int)
├── SkinsBuyIn (decimal, nullable)
├── SkinsPerSkinAmount (decimal, nullable)
├── ExpenseDeductionPct (decimal)
├── ScoresCountingPerHole (int)
└── ConfigJson (NVARCHAR(MAX) — flexible overflow for
    tournament payout structures, combo bet configs, etc.)

Teams
├── TeamId (PK)
├── BetConfigId (FK)
├── TeamNumber (int)
├── TeamName (string, nullable)
└── TeamPlayers[] (1:many)

TeamPlayers
├── TeamPlayerId (PK)
├── TeamId (FK)
├── PlayerId (FK)
└── Position (enum: Captain, B, C, D, E)

BetResults
├── BetResultId (PK)
├── BetConfigId (FK)
├── PlayerId (FK)
├── WinLossAmount (decimal)
├── NassauFrontResult (decimal)
├── NassauBackResult (decimal)
├── Nassau18Result (decimal)
├── InvestmentResult (decimal)
├── TotalStrokesResult (decimal)
├── SkinsWon (int, nullable)
├── SkinsAmount (decimal, nullable)
├── PressResult (decimal, nullable)
└── ResultDetailsJson (NVARCHAR(MAX) — hole-by-hole breakdown)
```

### 3.4 API Endpoint Structure

```
/api/courses
  GET    /                        → List all courses
  GET    /{id}                    → Course detail with holes
  POST   /                        → Create course
  PUT    /{id}                    → Update course

/api/players
  GET    /                        → List all players (with search/filter)
  GET    /{id}                    → Player detail
  POST   /                        → Create player
  PUT    /{id}                    → Update player (name, handicap)

/api/matches
  GET    /                        → List matches (paginated, filterable by date)
  GET    /{id}                    → Match detail (course, players, scores)
  POST   /                        → Create new match (select course, date)
  PUT    /{id}/status             → Change match status
  DELETE /{id}                    → Delete match (admin only)

/api/matches/{matchId}/scores
  GET    /                        → All scores for match
  GET    /{playerId}              → Player's scorecard
  PUT    /{playerId}              → Update scores (partial: single hole or bulk)
  POST   /{playerId}/hole/{num}   → Submit single hole score (mobile-optimized)

/api/matches/{matchId}/bets
  GET    /                        → List all bet configs for match
  POST   /                        → Create bet config
  PUT    /{betConfigId}           → Update bet config
  DELETE /{betConfigId}           → Remove bet config

/api/matches/{matchId}/bets/{betConfigId}/teams
  GET    /                        → List teams
  POST   /                        → Create/update team assignments
  PUT    /{teamId}                → Update team players

/api/matches/{matchId}/bets/{betConfigId}/results
  GET    /                        → Computed results (calls calculation engine)

/api/matches/{matchId}/grand-totals
  GET    /                        → Aggregated results across all bets

/api/matches/{matchId}/skins
  GET    /                        → Skins results

/hub/match                        → SignalR hub for live score updates
```

---

## 4. React Frontend Structure

### 4.1 Page/Route Hierarchy

```
/                               → Dashboard (upcoming/recent matches)
/matches/new                    → Create match (select course, date)
/matches/:id                    → Match hub (overview, all players/scores)
/matches/:id/scorecard          → Score entry (mobile-optimized)
/matches/:id/scorecard/:player  → Individual player score entry
/matches/:id/bets               → Bet configuration panel
/matches/:id/bets/:betId        → Bet detail & team assignment
/matches/:id/results            → Results dashboard
/matches/:id/results/foursomes  → Foursome results detail
/matches/:id/results/bestball   → Best Ball results detail
/matches/:id/results/individual → Individual bet results
/matches/:id/results/skins      → Skins results
/matches/:id/results/tournament → Tournament results
/matches/:id/grand-totals       → Grand totals across all bets
/courses                        → Course management
/courses/:id                    → Course detail/edit
/players                        → Player roster management
/players/:id                    → Player profile
/settings                       → App settings
```

### 4.2 Mobile-First Score Entry Design

The score entry screen is the most critical mobile UX. Design principles:

1. **One hole at a time**: Large, thumb-friendly number pad. Show current hole's par and handicap strokes received. Tap to enter score, auto-advance to next hole.
2. **Player carousel**: Swipe between players in the group. Show player name, handicap, running total.
3. **Quick entry mode**: For the "operator" entering scores for the whole group after a round — tab through all 4 players × 18 holes efficiently.
4. **Live preview**: Show running Nassau status, net scores, and team standings as scores are entered.
5. **Offline support**: Use service worker + IndexedDB to queue score entries when cell coverage is spotty on the course. Sync when back online.

### 4.3 Key React Components

```
<App />
├── <AppShell /> (Fluent UI navigation, responsive sidebar/bottom nav)
│   ├── <MatchDashboard />
│   │   ├── <MatchCard /> (match summary tile)
│   │   └── <CreateMatchDialog />
│   ├── <MatchHub />
│   │   ├── <MatchHeader /> (course, date, status)
│   │   ├── <PlayerList /> (enrolled players with score status)
│   │   └── <BetSummaryCards /> (quick view of active bets)
│   ├── <ScorecardPage />
│   │   ├── <HoleSelector /> (hole number picker or swipe)
│   │   ├── <ScoreInput /> (large numeric input with +/- buttons)
│   │   ├── <PlayerScoreRow /> (one row per player: name, hdcp, scores)
│   │   ├── <RunningTotals /> (front/back/total, net, vs par)
│   │   └── <ScoreGrid /> (full 18-hole grid view, desktop-optimized)
│   ├── <BetConfigPage />
│   │   ├── <BetTypeSelector /> (Foursome/Threesome/BB/Individual/Skins/Tournament)
│   │   ├── <NassauConfig /> (front/back/18 amounts)
│   │   ├── <HandicapConfig /> (percentage, allocation method)
│   │   ├── <InvestmentConfig /> (offs, redemptions, dunn)
│   │   ├── <PressConfig /> (auto press toggle, amount, threshold)
│   │   ├── <TeamAssignment /> (drag-drop or picker for player numbers)
│   │   └── <TournamentPayoutConfig /> (places paid, percentages, defaults)
│   ├── <ResultsPage />
│   │   ├── <NassauResultsTable /> (team vs team matrix)
│   │   ├── <HoleByHoleDetail /> (expandable per-team detail)
│   │   ├── <InvestmentSummary />
│   │   ├── <SkinsTable /> (who won which hole)
│   │   ├── <TournamentLeaderboard />
│   │   └── <GrandTotalsTable /> (all bets aggregated per player)
│   ├── <CoursesPage />
│   │   ├── <CourseList />
│   │   └── <CourseEditor /> (hole-by-hole par and handicap entry)
│   └── <PlayersPage />
│       ├── <PlayerRoster />
│       └── <PlayerEditor />
└── <SignalRProvider /> (context for live score updates)
```

---

## 5. Calculation Engine — C# Implementation Guide

The calculation engine is the heart of the application. It should be implemented as a set of pure, stateless service classes in the ASP.NET Core backend.

### 5.1 Service Structure

```csharp
namespace GolfMatchPro.Engine
{
    // Core interfaces
    public interface IHandicapCalculator
    {
        int ComputeCourseHandicap(decimal handicapIndex, int slopeRating);
        int ComputePlayingHandicap(int courseHandicap, decimal percentageUsed, int lowestInGroup);
        int[] DistributeStrokes(int playingHandicap, int[] holeHandicapRankings);
        int ApplyESC(int grossScore, int par, int courseHandicap);
    }

    public interface INassauCalculator
    {
        NassauResult CalculateMatchPlay(int[] netScoresA, int[] netScoresB);
        NassauResult CalculateMedalPlay(int[] netScoresA, int[] netScoresB);
        PressResult[] CalculateAutoPresses(int[] holeByHoleStatus, int downThreshold, decimal pressAmount);
    }

    public interface ITeamBetCalculator
    {
        TeamBetResult Calculate(TeamBetConfig config, List<TeamData> teams);
    }

    public interface IBestBallCalculator
    {
        BestBallResult Calculate(BestBallConfig config, TeamPair sheetHangers, List<TeamPair> opponents);
    }

    public interface ISkinsCalculator
    {
        SkinsResult Calculate(SkinsConfig config, List<PlayerScoreData> players);
    }

    public interface ITournamentCalculator
    {
        TournamentResult Calculate(TournamentConfig config, List<PlayerScoreData> players);
    }

    public interface IGrandTotalCalculator
    {
        List<PlayerGrandTotal> Calculate(int matchId, Dictionary<string, bool> includedBets);
    }
}
```

### 5.2 Key Algorithm: Stroke Distribution

```csharp
/// Distributes handicap strokes to holes based on handicap ranking.
/// Returns an array of length 18 where each element is the number of
/// strokes the player receives on that hole.
public int[] DistributeStrokes(int playingHandicap, int[] holeHandicapRankings)
{
    var strokes = new int[18];
    if (playingHandicap <= 0)
    {
        // Plus handicap: player GIVES strokes on easiest holes
        for (int i = 0; i < 18; i++)
        {
            if (holeHandicapRankings[i] > 18 + playingHandicap)
                strokes[i] = -1; // gives a stroke
        }
        return strokes;
    }

    int fullPasses = playingHandicap / 18;
    int remainder = playingHandicap % 18;

    for (int i = 0; i < 18; i++)
    {
        strokes[i] = fullPasses;
        if (holeHandicapRankings[i] <= remainder)
            strokes[i]++;
    }
    return strokes;
}
```

### 5.3 Key Algorithm: Team N-Ball Nassau

```csharp
/// For each hole, select the best N net scores from the team.
/// Sum them and compare running totals between teams.
public int[] ComputeTeamHoleScores(List<int[]> playerNetScores, int scoresCountingPerHole)
{
    int holes = 18;
    var teamScores = new int[holes];

    for (int h = 0; h < holes; h++)
    {
        var holeScores = playerNetScores
            .Select(p => p[h])
            .OrderBy(s => s)
            .Take(scoresCountingPerHole);
        teamScores[h] = holeScores.Sum();
    }
    return teamScores;
}
```

### 5.4 Key Algorithm: Investment Offs/Redemptions

```csharp
public (bool isOff, bool isRedemption) EvaluateHole(
    List<int> teamGrossScores,
    List<int> teamNetPars,  // par + strokes received per player
    int holeIndex)
{
    bool allOverPar = teamGrossScores
        .Zip(teamNetPars, (gross, netPar) => gross > netPar)
        .All(x => x);

    bool allAtOrUnderPar = teamGrossScores
        .Zip(teamNetPars, (gross, netPar) => gross <= netPar)
        .All(x => x);

    return (allOverPar, allAtOrUnderPar);
}
```

---

## 6. Implementation Phases

### Phase 1: Foundation (Weeks 1–3)
- [ ] Azure resource provisioning (SQL, App Service, Static Web Apps)
- [ ] ASP.NET Core API scaffold with EF Core migrations
- [ ] Entra ID authentication (API + React)
- [ ] Course CRUD (manage golf courses with hole data)
- [ ] Player CRUD (roster management with handicaps)
- [ ] React shell with Fluent UI, routing, auth

### Phase 2: Core Scoring (Weeks 4–6)
- [ ] Match creation and lifecycle management
- [ ] Score entry UI (mobile-first hole-by-hole input)
- [ ] Handicap calculation engine (course handicap, playing handicap, stroke distribution)
- [ ] ESC computation for reportable scores
- [ ] SignalR hub for live score broadcasting
- [ ] Full scorecard grid view (desktop)

### Phase 3: Team Bets (Weeks 7–9)
- [ ] Bet configuration UI (Nassau, handicap %, investments, deductions)
- [ ] Team assignment UI (drag-drop player numbers)
- [ ] Foursome/Threesome/Fivesome calculation engine
- [ ] Nassau (Match Play + Medal Play) calculator
- [ ] Investment bets (Offs, Redemptions, N-Ons, Dunn) calculator
- [ ] Total Strokes bet calculator
- [ ] Team-vs-team results matrix
- [ ] Expense deduction logic

### Phase 4: Individual & Best Ball Bets (Weeks 10–12)
- [ ] Individual 1v1 bet calculator with presses
- [ ] Automatic press logic
- [ ] Best Ball (2v2) calculator with Sheet Hanger model
- [ ] Combo bets (Wheel, Rope, Igg, Big Wheel, Big Igg)
- [ ] Best Ball W-L summary aggregation

### Phase 5: Skins & Tournaments (Weeks 13–14)
- [ ] Skins calculator (gross + net variants)
- [ ] Carry-over logic
- [ ] Individual Tournament payout engine
- [ ] Configurable payout structure with default percentages
- [ ] Tournament leaderboard

### Phase 6: Round Robins & Grand Totals (Weeks 15–16)
- [ ] Round Robin generators (Foursome, Individual, Best Ball)
- [ ] Grand Totals aggregation with toggleable bet inclusion
- [ ] Print/export views (PDF generation via Azure)

### Phase 7: Polish & Advanced Features (Weeks 17–20)
- [ ] Offline support (service worker + IndexedDB)
- [ ] PWA manifest for mobile "Add to Home Screen"
- [ ] Push notifications (score updates, match results)
- [ ] Historical match archive and player statistics
- [ ] Handicap trend tracking
- [ ] Admin panel for user management
- [ ] Azure DevOps CI/CD pipeline

---

## 7. Data Migration Strategy

### 7.1 Import from Excel

Build a one-time migration tool (console app or admin endpoint) that:

1. Reads the `.xlsm` file using **EPPlus** or **ClosedXML** (C# libraries)
2. Parses the **Courses** sheet → `Courses` + `CourseHoles` tables
3. Parses the **Players-Scores** sheet → `Players` table (names, nicknames, handicap indexes)
4. Optionally imports historical match data if scores are populated

```csharp
// Example using ClosedXML:
using var workbook = new XLWorkbook("template.xlsm");
var coursesSheet = workbook.Worksheet("Courses");

foreach (var row in coursesSheet.RowsUsed().Skip(3)) // skip headers
{
    var courseName = row.Cell(2).GetString();
    if (string.IsNullOrWhiteSpace(courseName)) continue;

    var course = new Course
    {
        Name = courseName,
        YearOfInfo = row.Cell(3).GetValue<int>(),
        CourseRating = row.Cell("AO").GetValue<decimal>(),
        SlopeRating = row.Cell("AP").GetValue<int>()
    };
    // Parse 18 handicap rankings and 18 pars...
}
```

---

## 8. Key Copilot Prompting Patterns

When working with GitHub Copilot on this project, use these prompt patterns:

### For Calculation Logic:
```
// Calculate the Nassau Match Play result for two players.
// Player A's net scores per hole: netScoresA (int[18])
// Player B's net scores per hole: netScoresB (int[18])
// Return: front 9 result (positive = A wins), back 9 result, 18-hole result
// A hole is won by the lower net score. Ties = halved (no change).
// Front result = running total after hole 9. Back = independent holes 10-18.
```

### For React Components:
```
// Create a Fluent UI React component for mobile score entry.
// Shows one hole at a time with large +/- buttons.
// Displays: hole number, par, handicap strokes received, current score.
// Auto-advances to next hole on entry.
// Shows running front/back/total.
// Uses Zustand store for score state.
```

### For API Endpoints:
```
// ASP.NET Core controller for managing match bets.
// POST /api/matches/{matchId}/bets creates a new bet configuration.
// BetType enum: Foursome, Threesome, Fivesome, BestBall, Individual, Skins, Tournament
// Validates that all referenced player IDs exist and have scores in the match.
// Returns 201 with the computed results preview.
```

---

## 9. Testing Strategy

### 9.1 Unit Tests (xUnit)

The calculation engine is the highest-priority test target. Use the Excel file's pre-populated match data as golden test cases:

```csharp
[Fact]
public void FoursomesNassau_WithSampleData_MatchesExcelResults()
{
    // Team 1: Jeremy (-1), Gose (9), JD (10), Ronnie (28)
    // Expected: Nassau scores 20 + 19 = 39
    // Expected: Win $106 each after 10% deduction

    var result = _calculator.Calculate(config, teams);

    Assert.Equal(39, result.Teams[0].NassauTotal);
    Assert.Equal(106m, result.Teams[0].WinLossPerMan);
}
```

### 9.2 Integration Tests

- API endpoint tests with in-memory database
- SignalR hub connection tests
- Authentication flow tests

### 9.3 E2E Tests (Playwright)

- Full score entry flow on mobile viewport
- Bet configuration and results verification
- Match lifecycle (create → score → finalize → view results)

---

## 10. Environment Configuration

### 10.1 `appsettings.json` Structure

```json
{
  "ConnectionStrings": {
    "GolfMatchDb": "Server=tcp:golfmatch.database.windows.net..."
  },
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<tenant-id>",
    "ClientId": "<client-id>",
    "Audience": "api://<client-id>"
  },
  "SignalR": {
    "AzureSignalRConnectionString": "<optional-azure-signalr>"
  }
}
```

### 10.2 Solution Structure

```
GolfMatchPro/
├── src/
│   ├── GolfMatchPro.Api/              # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   ├── Hubs/                      # SignalR hubs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── GolfMatchPro.Engine/           # Calculation engine (class library)
│   │   ├── Handicaps/
│   │   ├── Nassau/
│   │   ├── Investments/
│   │   ├── Skins/
│   │   ├── Tournament/
│   │   └── GrandTotals/
│   ├── GolfMatchPro.Data/            # EF Core models + migrations
│   │   ├── Entities/
│   │   ├── Migrations/
│   │   └── GolfMatchDbContext.cs
│   ├── GolfMatchPro.Shared/          # DTOs, enums, contracts
│   │   ├── Dtos/
│   │   └── Enums/
│   └── GolfMatchPro.Web/             # React app (CRA or Vite)
│       ├── src/
│       │   ├── components/
│       │   ├── pages/
│       │   ├── hooks/
│       │   ├── stores/               # Zustand stores
│       │   ├── services/             # API client + SignalR
│       │   └── utils/                # Calculation helpers (client-side previews)
│       ├── public/
│       └── package.json
├── tests/
│   ├── GolfMatchPro.Engine.Tests/    # xUnit calculation tests
│   ├── GolfMatchPro.Api.Tests/       # Integration tests
│   └── GolfMatchPro.E2E/            # Playwright tests
├── tools/
│   └── GolfMatchPro.Migration/       # Excel import tool
├── .github/
│   └── copilot-instructions.md       # THIS FILE
├── azure-pipelines.yml
└── GolfMatchPro.sln
```

---

## 11. Golf Terminology Quick Reference

| Term | Definition |
|---|---|
| **Gross Score** | Actual strokes taken on a hole |
| **Net Score** | Gross minus handicap strokes received on that hole |
| **Nassau** | Three separate bets: Front 9, Back 9, 18-hole total |
| **Press** | A new bet triggered mid-match when a player falls behind by a set number of holes |
| **Skins** | Each hole is a separate bet; lowest unique score wins the pot for that hole |
| **Best Ball / Four Ball** | Team format; best net score from the team counts for each hole |
| **Going Off** | All team members score over par on a hole (costs money) |
| **Redemption** | All team members make par or better (earns money back) |
| **Sheet Hangers** | The fixed "home" team in a Best Ball bet that plays against multiple opponent teams |
| **ESC** | Equitable Stroke Control — caps max score per hole for handicap reporting purposes |
| **Slope Rating** | Measure of course difficulty for bogey golfers relative to scratch golfers (113 = standard) |
| **Course Rating** | Expected score for a scratch golfer on the course |
| **Dunn** | An additional side investment bet between teams |
| **Combo Bet** | Multi-team combinations (Wheel, Rope, Igg) where subgroups of players are paired in various permutations |

---

## 12. Important Business Rules & Edge Cases

1. **Players on multiple teams:** The Excel flags players appearing on 2+ teams with red text/background. The web app must enforce unique team assignment per bet type (one player per team per bet config).

2. **Guests:** Guest players can be entered in empty roster slots or can replace non-playing regulars. The web app should support ad-hoc guest entry with manual handicap.

3. **Negative handicaps (plus handicaps):** Players like Jeremy Hymas (-1 or -2 index) have their handicap subtracted, making par harder. The stroke distribution algorithm must handle this correctly.

4. **Maximum Net Score cap:** When enabled (e.g., 82), any player whose net score exceeds 82 is capped at 82 for the Total Strokes bet. This prevents one bad player from blowing up team totals.

5. **Halved holes in Match Play:** When both players/teams tie on a hole, the match status doesn't change. The running count stays the same.

6. **Press timing:** Presses start from the NEXT hole after falling N-down, not the current hole. Each press is an independent mini-Nassau for the remaining holes.

7. **Expense deduction:** Applied only to winners' earnings, not to losers' losses. The deducted amount goes to a group fund.

8. **Round Robin macro:** The original Excel uses a VBA macro to compute all pairwise matchups. The web app should compute these server-side in the calculation engine, iterating through all `C(n,2)` combinations.

9. **Skins carry-over:** When no player has a unique low score on a hole, the skin carries over and accumulates. The next hole is worth 2+ skins. This can cascade.

10. **Tournament ties:** When multiple players tie for a place, prize money for the tied positions is pooled and split evenly.
