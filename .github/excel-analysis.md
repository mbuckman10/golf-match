# Excel Template Analysis

> Source: `original-excel-template.xlsm` — 26 sheets, VBA-enabled  
> Course used for sample data: **Oakridge White**  
> Date: 2025-04-07  
> Analysis performed: 2026-04-15

---

## Table of Contents

1. [Sheet Inventory](#sheet-inventory)
2. [Course Data](#course-data)
3. [Player Roster & Scores](#player-roster--scores)
4. [Foursomes (Team Bets)](#foursomes-team-bets)
5. [Individual Bets](#individual-bets)
6. [Best Ball Bets](#best-ball-bets)
7. [Skins](#skins)
8. [Indo Tourney](#indo-tourney)
9. [Round Robin Sheets](#round-robin-sheets)
10. [Grand Totals](#grand-totals)
11. [Cross-Validation Findings](#cross-validation-findings)

---

## Sheet Inventory

| # | Sheet Name | Rows × Cols | Status |
|---|-----------|-------------|--------|
| 1 | Courses | 35 × 58 | Reference data — 19 courses |
| 2 | Players-Scores | 125 × 68 | 49 players, 16 with scores entered |
| 3 | Foursomes | 370 × 256 | **Fully populated** — 5 teams, complete results |
| 4 | Best Ball Bets | 575 × 76 | Config only — no matchups entered |
| 5 | Best Ball Bets (2) | 575 × 89 | Config only — no matchups entered |
| 6 | Best Ball Bets (3) | 575 × 89 | Config only — no matchups entered |
| 7 | Best Ball Bets (4) | 575 × 89 | Config only — no matchups entered |
| 8 | Best Ball $ W-L | ~60 × 20 | Empty — depends on BB Bets |
| 9 | Best Ball $ W-L (2) | ~60 × 20 | Empty — depends on BB Bets (2) |
| 10 | Best Ball $ W-L (3) | ~60 × 20 | Empty — depends on BB Bets (3) |
| 11 | Best Ball $ W-L (4) | ~60 × 20 | Empty — depends on BB Bets (4) |
| 12 | Individual Bets | ~300 × 113 | **1 match populated** — Jack vs JD |
| 13 | Individual Bets (2) | ~300 × 113 | Config only — no matchups entered |
| 14 | Skins | ~60 × 30 | Config only — no players entered |
| 15 | Skins (2) | ~60 × 30 | Config only |
| 16 | Indo Tourney | ~80 × 50 | Config only — no players entered |
| 17 | Indo Round Robin | ~100 × 50 | Config only |
| 18 | BB Round Robin | ~100 × 50 | Config only |
| 19 | Foursomes Round Robin | 1171 × 249 | Config only |
| 20 | Threesomes | ~300 × 200 | Config only |
| 21 | Fivesomes | ~400 × 300 | Config only |
| 22 | Grand Totals | ~100 × 30 | Consolidated — depends on all sheets |
| 23-26 | Instructions / Misc | — | Documentation |

---

## Course Data

### Oakridge White (Course #2)

| Field | Value |
|-------|-------|
| Course Rating | 70.5 |
| Slope Rating | 124 |
| Front Par | 36 |
| Back Par | 36 |
| Total Par | 72 |

**Hole Details:**

| Hole | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 |
|------|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **Par** | 5 | 3 | 4 | 4 | 3 | 5 | 4 | 4 | 4 | 4 | 5 | 4 | 3 | 4 | 3 | 5 | 4 | 4 |
| **Hdcp Rank** | 17 | 13 | 7 | 1 | 11 | 15 | 5 | 9 | 3 | 6 | 16 | 12 | 18 | 2 | 8 | 14 | 4 | 10 |

### Handicap Allocation Formula

The Excel uses course slope to convert handicap index to course handicap:
```
Course Handicap = ROUND(Index × Slope / 113)
```

Then strokes are allocated to holes based on handicap ranking:
- A player with hdcp N gets 1 stroke on the N hardest holes (lowest rank numbers)
- If hdcp > 18, they get 2 strokes on the (hdcp - 18) hardest holes, 1 on the rest

**ESC (Equitable Stroke Control)** is applied — max score per hole depends on handicap:

| Hdcp Range | Max Score |
|-----------|-----------|
| 0-4 | Double Bogey |
| 5-9 | 7 |
| 10-19 | 7 |
| 20-29 | 8 |
| 30-39 | 9 |
| 40+ | 10 |

---

## Player Roster & Scores

### Players with actual scores entered (16 of 49)

| # | Name | Hdcp Index | Course Hdcp | Gross | Net | Front | Back |
|---|------|-----------|------------|-------|-----|-------|------|
| 5 | Brady Watkins | 5 | 5 | 75 | 70 | 37 | 38 |
| 7 | Brett Watkins | 7 | 8 | 80 | 72 | 43 | 37 |
| 19 | JD Moss | 9 | 10 | 88 | 78 | 48 | 40 |
| 20 | Jack Watkins | 21 | 23 | 100 | 77 | 52 | 48 |
| 22 | Jedd Moss | 3 | 3 | 76 | 73 | 39 | 37 |
| 23 | Jeff Judd | 7 | 8 | 84 | 76 | 44 | 40 |
| 26 | Jeremy Hymas | -1 | -1 | 75 | 76 | 38 | 37 |
| 29 | Lance Hori | 13 | 14 | 85 | 71 | 42 | 43 |
| 30 | Linn Baker | 17.5 | 19 | 93 | 74 | 48 | 45 |
| 31 | Mike Jensen | 12 | 13 | 84 | 71 | 41 | 43 |
| 42 | Tom Stuart | 6 | 7 | 80 | 73 | 39 | 41 |
| 45 | Redd | 15 | 16 | 88 | 72 | 44 | 44 |
| 46 | Ben | 14 | 15 | 87 | 72 | 44 | 43 |
| 47 | Gose | 8 | 9 | 86 | 77 | 43 | 43 |
| 48 | Tony | 6 | 7 | 84 | 77 | 40 | 44 |
| 49 | Guest (Sean) | 2 | 2 | 72 | 70 | 35 | 37 |

### Hole-by-Hole Scores (Players with data)

**Brady Watkins** (Hdcp 5): `6 3 4 4 3 4 4 5 4` | `4 4 4 4 3 4 6 5 4` = 75  
**Brett Watkins** (Hdcp 8): `6 3 5 5 4 7 5 4 4` | `4 6 4 3 5 3 4 4 4` = 80  
**JD Moss** (Hdcp 10): `6 3 5 5 4 8 5 5 7` | `4 5 4 3 4 3 8 4 5` = 88  
**Jack Watkins** (Hdcp 23): `8 3 4 6 6 8 5 6 6` | `7 7 6 4 5 3 6 5 5` = 100  
**Jedd Moss** (Hdcp 3): `6 3 4 5 3 4 4 5 5` | `4 5 4 3 4 4 4 5 4` = 76  
**Jeff Judd** (Hdcp 8): `5 4 4 7 4 6 5 4 5` | `4 5 6 3 4 3 6 5 4` = 84  
**Jeremy Hymas** (Hdcp -1): `5 4 4 4 3 5 5 4 4` | `3 5 4 3 3 4 6 5 4` = 75  
**Lance Hori** (Hdcp 14): `6 3 5 4 4 5 6 4 5` | `6 6 5 5 4 3 5 5 4` = 85  
**Linn Baker** (Hdcp 19): `7 3 5 5 6 5 5 5 7` | `5 6 4 3 6 5 5 6 5` = 93  
**Mike Jensen** (Hdcp 13): `5 3 4 6 4 5 6 3 5` | `5 6 5 3 6 4 4 5 5` = 84  
**Tom Stuart** (Hdcp 7): `5 5 4 4 3 6 4 4 4` | `4 6 5 4 4 3 5 5 5` = 80  
**Redd** (Hdcp 16): `7 3 4 4 5 5 5 6 5` | `4 4 5 5 6 4 7 5 4` = 88  
**Ben** (Hdcp 15): `5 4 5 5 4 7 5 5 4` | `5 5 6 3 5 4 6 4 5` = 87  
**Gose** (Hdcp 9): `5 3 7 6 3 6 4 4 5` | `5 6 6 3 4 3 6 6 4` = 86  
**Tony** (Hdcp 7): `6 5 4 4 4 5 4 4 4` | `5 7 4 4 4 4 5 5 6` = 84  
**Guest/Sean** (Hdcp 2): `4 3 4 4 3 5 4 3 5` | `4 5 4 3 5 4 4 4 4` = 72  

---

## Foursomes (Team Bets)

### Configuration

| Setting | Value |
|---------|-------|
| Competition Type | Nassau Bet — Medal (net strokes) |
| Scoring Method | Low 2 net scores per hole per foursome |
| Nassau Front | $5 |
| Nassau Back | $5 |
| Nassau 18-hole | $5 |
| Total Strokes | $1 per stroke difference |
| Max Net Score | 82 |
| Handicap % | 100% |
| Group Expense Deduction | 10% from winnings ($300 total) |
| Lowest Hdcp Playing | -1 (Jeremy Hymas) |

### Investment Bets

| Bet | Enabled | Amount |
|-----|---------|--------|
| 4-Ons | Yes | $6 per "OFF" per opposing team |
| Redemption | Yes | $3 per redemption |
| Dunn | Yes | $5 |

### Teams

| Team | Captain | Player B | Player C | Player D | Team Hdcp |
|------|---------|----------|----------|----------|-----------|
| #1 Jeremy's | Jeremy Hymas (-1) | Gose (9) | JD Moss (10) | — | 46 |
| #2 Sean's | Guest/Sean (2) | Brett Watkins (8) | Mike Jensen (13) | Jack Watkins (23) | 46 |
| #3 Jedd's | Jedd Moss (3) | Jeff Judd (8) | Lance Hori (14) | — | 44 |
| #4 Brady's | Brady Watkins (5) | Tom Stuart (7) | Ben (15) | Linn Baker (19) | 46 |
| #5 Baugh's | Eric Baugh (14) | Tony (7) | Redd (16) | Brandon O'Brien (3) | 40 |

> Note: Teams #1, #3 appear to have 3 players; Teams #2, #4, #5 have 4 players.  
> Team #5 (Baugh's) has the lowest total net strokes and wins the most.

### Net Score Totals (Best 2 of N per hole)

| Team | Front 9 Net | Back 9 Net | 18-Hole Net | Total Strokes (all) |
|------|------------|-----------|------------|---------------------|
| #1 Jeremy's | 20 | 19 | 39 | 203 |
| #2 Sean's | 61 | 65 | 126 | 290 |
| #3 Jedd's | 21 | 22 | 43 | 201 |
| #4 Brady's | 62 | 63 | 125 | 289 |
| #5 Baugh's | -9 | -8 | -17 | 132 |

> **Note:** Team #5 shows negative net scores (-9, -8, -17) which indicates their best 2 net scores per hole are below par on most holes. This is because Baugh and Brandon have low handicaps and Tony plays well.

### Investment Bet Results

| Team | OFFs | Redemptions |
|------|------|-------------|
| #1 Jeremy's | 1 (hole 16) | 1 (hole 18) |
| #2 Sean's | 1 (hole 11) | 2 (holes 13, 16) |
| #3 Jedd's | 0 | 0 |
| #4 Brady's | 0 | 0 |
| #5 Baugh's | 0 | 0 |

### Team vs Team Results Matrix (per player $)

| | vs #1 | vs #2 | vs #3 | vs #4 | vs #5 | Gross Total | After Expense |
|---|-------|-------|-------|-------|-------|------------|--------------|
| **#1 Jeremy's** | — | -99 | -10 | -98 | 89 | 118 | **106** |
| **#2 Sean's** | 99 | — | 104 | 6 | 173 | — | **-382** |
| **#3 Jedd's** | 10 | -104 | — | -103 | 84 | 113 | **102** |
| **#4 Brady's** | 98 | -6 | 103 | — | 172 | — | **-367** |
| **#5 Baugh's** | -89 | -173 | -84 | -172 | — | 518 | **466** |

> Winners get 10% deducted for group expenses. Losers pay full amount.  
> Total deducted: Team #1 (4×$12) + Team #3 (4×$11) + Team #5 (4×$52) = $48 + $44 + $208 = $300

---

## Individual Bets

### Configuration

| Setting | Value |
|---------|-------|
| Competition Type | Match Play |
| Nassau Front | $5 |
| Nassau Back | $10 |
| Nassau 18-hole | $5 |
| Auto Presses | Yes — 2 Down, $25 each |
| Handicap | From calculated course handicap |

### Match #1: Jack Watkins vs JD Moss

| | Jack Watkins | JD Moss |
|---|-------------|---------|
| Hdcp Index | 21 | 9 |
| Course Hdcp | 23 | 10 |
| Hdcp Difference | — | Jack gets **13 strokes** |
| Gross Front / Back / Total | 52 / 48 / 100 | 48 / 40 / 88 |
| Net Front / Back / Total | — | — |

**Match Play Hole-by-Hole Status (Jack's perspective):**

| Hole | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 |
|------|---|---|---|---|---|---|---|---|---|
| Jack Gross | 8 | 3 | 4 | 6 | 6 | 8 | 5 | 6 | 6 |
| JD Gross | 6 | 3 | 5 | 5 | 4 | 8 | 5 | 5 | 7 |
| Jack Strokes | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 |
| Jack Net | 7 | 2 | 3 | 5 | 5 | 7 | 4 | 5 | 5 |
| JD Net | 6 | 3 | 5 | 5 | 4 | 8 | 5 | 5 | 7 |
| Hole Winner | JD | Jack | Jack | Push | JD | Jack | Jack | Push | Jack |
| Running Status | -1 | 0 | +1 | +1 | 0 | +1 | +2 | +2 | +3 |

> Jack is **3 UP** after the front 9.

| Hole | 10 | 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 |
|------|---|---|---|---|---|---|---|---|---|
| Jack Gross | 7 | 7 | 6 | 4 | 5 | 3 | 6 | 5 | 5 |
| JD Gross | 4 | 5 | 4 | 3 | 4 | 3 | 8 | 4 | 5 |
| Jack Strokes | 1 | 0 | 1 | 1 | 1 | 0 | 1 | 1 | 1 |
| Jack Net | 6 | 7 | 5 | 3 | 4 | 3 | 5 | 4 | 4 |
| JD Net | 4 | 5 | 4 | 3 | 4 | 3 | 8 | 4 | 5 |
| Hole Winner | JD | JD | JD | Push | Push | Push | Jack | Push | Jack |
| Running Status | +2 | +1 | 0 | 0 | 0 | 0 | +1 | +1 | +2 |

> Jack is **2 UP** after 18 holes → **Jack wins back** and **wins 18**.

Wait — let me re-examine. The Excel shows:
- **Front running**: -1, 0, +1, +1, 0, 0, +1, +1, +2 → Jack **2 UP** after front
- **Back running**: starts at 0 for back only: -1, -2, -3, -4, -4, -3, -2, -2, -1 → JD **1 UP** after back
- **18-hole running**: continues from front → +2, +1, 0, -1, -2, -2, -1, -1, 0 → PUSH? 

Excel says: Jack **won front** ($5), **lost back** (-$10), **won 18** ($5) → **Net $0**

> The "won 18" with $5 seems inconsistent if the 18-hole is a push (0). Need to verify this during cross-validation.

### Matches #2-22: Not populated (all #N/A)

---

## Best Ball Bets

### Best Ball Bets (1-4) — All have configuration but NO matchups entered

**Common Config for Best Ball Bets (2):**

| Setting | Value |
|---------|-------|
| Competition Type | Match Play |
| Nassau Front | $5 |
| Nassau Back | $10 |
| Nassau 18-hole | $5 |
| Auto Presses | Yes — 1 Down, $5 each |
| Handicap | NET, calculated off low handicap |

**Sheet Hanger model**: One "sheet hanger" player is paired with teammates against multiple opponents. Each opponent matchup is a separate 2v2 best-ball match.

### Best Ball $ W-L (1-4)

All 4 W-L summary sheets list all 49 players but have **no populated amounts** since no BB matchups were configured.

---

## Skins

### Configuration

| Setting | Value |
|---------|-------|
| Type | Gross Skins Calculator |
| Buy-in | $10 per player |
| Handicap % | 0% (gross skins) |

**No players entered** — all calculations show #NUM! errors.

A second skins sheet exists (likely Net Skins) with similar empty state.

---

## Indo Tourney

### Configuration

| Setting | Value |
|---------|-------|
| Type | Individual Tournament — Gross/Net Winners |
| Buy-in | $20 per player |
| Group Expense Deduction | 5% |
| Payout Structure | 18-hole, Front 9, Back 9 for both Gross and Net |
| Places Paid | Up to 15 places with configurable percentages |

**No players entered** — all results show #N/A.

---

## Round Robin Sheets

### Indo Round Robin
- Individual round-robin with presses
- Not populated

### BB Round Robin
- Best ball round-robin with presses
- Not populated

### Foursomes Round Robin
- 1171 rows × 249 cols — concurrent Nassau bets between all team pairs
- Not populated beyond Foursomes main sheet data

---

## Grand Totals

Consolidated results across all bet types per player. Depends on data from all other sheets. Only Foursomes and Individual Bets (1 match) have data, so Grand Totals would only reflect those.

---

## Cross-Validation Findings

### Status Key
- ✅ = Engine matches Excel logic
- ⚠️ = Partial match / minor difference
- ❌ = Discrepancy found
- 🔲 = Not yet implemented in engine

### 1. Handicap Calculation

| Check | Status | Notes |
|-------|--------|-------|
| Index → Course Hdcp conversion | ✅ | `ROUND(Index × Slope / 113)` matches `HandicapCalculator` |
| Stroke allocation by hole rank | ✅ | Strokes assigned to lowest-ranked holes first |
| Negative handicaps (give strokes) | ⚠️ | Jeremy at -1 — verify engine handles correctly |
| ESC max score limits | 🔲 | Excel applies ESC; engine may not enforce max scores |

### 2. Team (Foursomes) Calculator

| Check | Status | Notes |
|-------|--------|-------|
| Best 2-of-N net per hole | ✅ | `TeamScoreCalculator` implements this |
| Nassau medal comparison | ✅ | `TeamBetCalculator` does team-vs-team Nassau |
| Total strokes bet ($1/stroke) | ✅ | `TotalStrokesCalculator` handles this |
| Team-vs-team pairwise matrix | ✅ | Each pair compared independently |
| Investment bets (4-Ons) | ⚠️ | See Investment Bets section below |
| Investment bets (Redemption) | ⚠️ | See Investment Bets section below |
| Investment bets (Dunn) | 🔲 | Not in current `InvestmentCalculator` |
| Group expense deduction (10%) | ⚠️ | Need to verify winners-only deduction logic |
| Max net score cap (82) | 🔲 | Excel caps net at 82; verify engine respects this |

### 3. Individual Bets Calculator

| Check | Status | Notes |
|-------|--------|-------|
| Match Play hole comparison | ✅ | `IndividualBetCalculator` handles match play |
| Handicap stroke allocation | ✅ | Lower-hdcp player plays at 0, higher gets diff |
| Auto 2-down press trigger | ✅ | Press starts new Nassau when 2 down |
| Nassau front/back/total | ✅ | Separate front, back, 18-hole bets |
| Medal Play option | ✅ | Both match and medal supported |

### 4. Best Ball Calculator

| Check | Status | Notes |
|-------|--------|-------|
| Sheet Hanger vs Opponent model | ✅ | `BestBallCalculator` uses this structure |
| Best-of-N net per hole | ✅ | Lowest net on each team counted |
| Auto 1-down press | ✅ | Press triggers at configurable N-down |
| Combo bets (Wheel/Rope/Igg) | ✅ | `ComboBetCalculator` generates combos |
| W-L aggregation | ✅ | `BestBallWinLossAggregator` summarizes |
| **No test data available** | ⚠️ | No populated BB matchups in Excel to validate against |

### 5. Investment Bets

| Check | Status | Notes |
|-------|--------|-------|
| 4-Ons (all 4 players on green in reg) | ⚠️ | `InvestmentCalculator` exists but verify "ON"/"OFF" logic |
| Redemption (recover after OFF) | ⚠️ | Logic present, verify trigger conditions |
| Dunn bet | 🔲 | **Not implemented** — Excel has Dunn ($5) as a separate investment |
| $ per OFF per opposing team | ⚠️ | Excel charges $6 per OFF per opposing team (×4 teams = $24 max) |

### 6. Features NOT Yet Implemented

| Feature | Excel Sheet | Priority |
|---------|-------------|----------|
| Skins (Gross & Net) | Skins, Skins (2) | Phase 5? |
| Indo Tournament | Indo Tourney | Phase 5? |
| Indo Round Robin | Indo Round Robin | Future |
| BB Round Robin | BB Round Robin | Future |
| Foursomes Round Robin | Foursomes Round Robin | Future |
| Threesomes | Threesomes | Future |
| Fivesomes | Fivesomes | Future |
| Grand Totals consolidation | Grand Totals | Future |
| ESC score capping | Players-Scores | Should add |
| Max net score cap | Foursomes | Should add |
| Dunn investment bet | Foursomes | Should add |

### 7. Specific Numerical Cross-Validation

#### Foursomes: Team #1 (Jeremy's) vs Team #3 (Jedd's)

**Expected from Excel**: Team #1 owes Team #3 $10 per man  
- Nassau: Team #1 net 39 vs Team #3 net 43 → Team #1 wins front by 1 ($5), Team #3 wins back, close on 18
- Total strokes: 203 vs 201 → Team #1 loses by 2 strokes ($2)
- Need to verify exact Nassau medal breakdown and investment bet charges

#### Individual: Jack vs JD

**Expected from Excel**: Net $0 (Jack wins front $5, loses back $10, wins 18 $5 = $0)
- Jack gets 13 strokes over JD (23 - 10)
- Stroke allocation across 18 holes based on handicap ranking
- No auto-presses triggered (neither player was ever 2 down in a press segment)

### 8. Key Architectural Observations from Excel

1. **Multiple configs per bet type**: Excel supports 4 separate BB configs and 2 Individual configs, each potentially with different Nassau amounts and press rules
2. **Team sizes vary**: Foursomes can be 3-5 players (Threesomes/Fivesomes sheets exist)
3. **Investment bets are team-only**: 4-Ons, Redemption, Dunn only apply to team (Foursomes) bets
4. **Expense deduction is winners-only**: Only winning teams/players have 10% deducted
5. **Handicap % is configurable**: Can play at 80% or 90% of full handicap
6. **Max net score**: Foursomes enforce a max net score of 82 (prevents blowouts)
7. **ESC**: Applied at score-entry level, not at calculation level
