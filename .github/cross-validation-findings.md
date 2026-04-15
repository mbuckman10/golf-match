# Cross-Validation Findings — Engine vs Excel

> **Date:** April 15, 2026  
> **Source:** `original-excel-template.xlsm` (Foursomes sheet, Individual Bets sheet)  
> **Course:** Oakridge White — Par 72, Rating 70.5, Slope 124  
> **Test file:** `tests/GolfMatchPro.Engine.Tests/CrossValidation/`

---

## Summary

128 total tests pass (127 Engine + 1 API). Of the 63 cross-validation tests:
- **55 Foursomes** tests: all pass
- **8 Individual Bets** tests: all pass

Two engine bugs were found and fixed. All known discrepancies have been resolved — the engine now matches the Excel exactly.

---

## Bug Found & Fixed: Absent Player Handling

### The Problem

The engine produced incorrect team scores for any team with absent players (gross = 0 for all holes). Out of 5 teams, 3 had at least one absent player, causing **27 of 63 tests to fail**.

| Team | Absent Player(s) | Engine Team Score (Before Fix) | Excel Team Score | Engine Team Score (After Fix) |
|------|-------------------|-------------------------------|-----------------|-------------------------------|
| #1 Jeremy's | Ronnie (hdcp 28) | Front 71, Back 68, Total 139 | Front 20, Back 19, Total 39 | Front 20, Back 19, Total 39 |
| #2 Sean's | *(none)* | Front 61, Back 65, Total 126 | Front 61, Back 65, Total 126 | *(unchanged — already matched)* |
| #3 Jedd's | Raff (hdcp 19) | Front 68, Back 64, Total 132 | Front 21, Back 22, Total 43 | Front 21, Back 22, Total 43 |
| #4 Brady's | *(none)* | Front 62, Back 63, Total 125 | Front 62, Back 63, Total 125 | *(unchanged — already matched)* |
| #5 Baugh's | Baugh (hdcp 14), Brandon (hdcp 3) | Front 72, Back 77, Total 149 | Front -9, Back -8, Total -17 | Front -9, Back -8, Total -17 |

### Root Cause

The Excel computes `net = gross - handicapStrokes` for **all** players, including absent ones. When `gross = 0`:
- Ronnie (hdcp 28): net per hole = `0 - strokes` → values like -1, -2 per hole
- These negative net scores are included in best-2-ball selection and become the "best" scores

The engine's `TeamScoreCalculator.ComputeTeamHoleScores` had a filter:
```csharp
if (playerScores[hole] > 0) // Only include if played
    holeScores.Add(playerScores[hole]);
```
This excluded all non-positive scores, meaning absent players were ignored entirely. With only 2-3 active players to choose from, the "best 2" selection picked higher (worse) scores.

### Fix Applied

**File:** `src/GolfMatchPro.Engine/Teams/TeamScoreCalculator.cs`  
**Change:** Removed the `> 0` filter — all player net scores are now included unconditionally:
```csharp
foreach (var playerScores in playerNetScores)
{
    holeScores.Add(playerScores[hole]);
}
```

**File:** `tests/.../CrossValidation/FoursomesCrossValidationTests.cs`  
**Change:** The test helper `ComputePlayerNets` also had `if (gross[h] > 0)` which set net = 0 for absent players instead of computing the correct negative value. Fixed to always compute `net[h] = gross[h] - strokes[h]`.

### Impact

- All 27 previously failing tests now pass
- All 65 pre-existing tests remain unaffected (they only used positive scores)
- This matches the Excel behavior exactly

### Design Note

This fix means the engine now treats `gross = 0` with handicap strokes as a valid (negative) net score, which is the Excel's behavior. If the web app needs to distinguish "not yet entered" from "absent player with 0 gross", a different sentinel (e.g., `null` or `int.MinValue`) should be used at a higher level. The current behavior is correct for the Foursomes gambling calculation.

---

## Bug Found & Fixed: Investment Calculator (OFFs / Redemptions)

### The Problem

The Investment calculator used a **different method** than the Excel for determining OFFs and Redemptions, producing incorrect results on specific holes and for teams with absent players.

| Team | Metric | Engine (Before Fix) | Excel | Engine (After Fix) |
|------|--------|---------------------|-------|--------------------|
| #1 Jeremy's | OFFs | 0 (all holes skipped — Ronnie absent) | 1 (hole 16) | 1 (hole 16) ✅ |
| #1 Jeremy's | Redemptions | 0 (all holes skipped) | 1 (hole 18) | 1 (hole 18) ✅ |
| #2 Sean's | OFFs | 1 (hole 14 — wrong hole) | 1 (hole 11) | 1 (hole 11) ✅ |
| #2 Sean's | Redemptions | 1 (hole 2 — wrong hole, wrong count) | 2 (holes 13, 16) | 2 (holes 13, 16) ✅ |
| #3-#5 | OFFs + Reds | 0 | 0 | 0 ✅ |

### Root Cause

The engine had three issues:

1. **Wrong comparison:** Checked `netScore > netPar` where `netPar = par - strokes`. Since strokes cancel, this simplified to `gross > par` — ignoring handicap strokes entirely. The Excel compares `net > par` (i.e., `gross - strokes > par`), correctly accounting for handicap allocation.

2. **Wrong aggregation:** Checked if ALL individual players were over/under par. The Excel uses `SMALL(allPlayerNets, N) > par` for OFFs (N = scoresCountingPerHole, the same number of counting scores used in best-ball). This means the OFF check is "are the team's counting scores all over par?" — not "is every player over par?"

3. **Absent player handling:** If any player had `gross = 0`, the entire hole was skipped. The Excel computes `net = 0 - strokes` for absent players (producing very negative values) which are naturally handled by the SMALL/MAX sort — absent players' extreme negative nets sort to the bottom and don't affect the Nth-smallest or maximum.

Additionally, the Excel's Redemption logic requires a **running off balance** — Redemptions (`MAX(allPlayerNets) ≤ par`) can only occur after a team has gone off, and each Redemption reduces the balance by `redemptionAmount / offAmount`.

### Fix Applied

**File:** `src/GolfMatchPro.Engine/Investments/InvestmentCalculator.cs`

Rewrote `Evaluate()` with three new parameters (`scoresCountingPerHole`, `offAmount`, `redemptionAmount`) and the Excel-matching logic:

```csharp
// OFF: N-th smallest net > par (team's counting scores all over par)
Array.Sort(nets); // ascending
if (nets[n - 1] > par) { /* OFF */ }

// Redemption: MAX net ≤ par AND running balance > 0
if (nets[^1] <= par && runningBalance > 0) { /* Redemption */ }
```

**File:** `src/GolfMatchPro.Engine/Teams/TeamBetCalculator.cs`  
Updated `Calculate()` to pass `config.ScoresCountingPerHole`, `config.InvestmentOffAmount`, and `config.RedemptionAmount` to `Evaluate()`.

### Excel Formula Chain (for reference)

The Excel's Foursomes sheet uses a multi-tier formula chain for investment checks:

| Tier | Formula | Result | Count |
|------|---------|--------|-------|
| 1 | `MIN(allPlayerNets) > par` | "2offs" (all players over par) | 2 |
| 2 | `SMALL(allPlayerNets, N) > par` | "OFF" (counting scores over par) | 1 |
| 3 | `MAX(allPlayerNets) ≤ par AND balance > 0` | "InvR" (redemption) | 1 |

The running balance increments by the OFF count and decrements by `redemptionAmount / offAmount` per Redemption. Redemptions can only occur when the balance is positive (team has outstanding OFFs).

> **Note:** The "2offs" tier (all players over par, counting double) is implemented in the engine but not triggered in the current test data — all observed OFFs are tier-2 (SMALL-based).

---

## Verified Calculations (All Passing)

### Handicap Calculations (15 tests)

All 16 players' handicap index → course handicap conversions verified against the Excel. Formula: `ROUND(Index × Slope / 113)`.

| Player | Index | Course Hdcp | Verified |
|--------|-------|-------------|----------|
| Jeremy Hymas | -1 | -1 | ✅ |
| Sean (Guest) | 2 | 2 | ✅ |
| Jedd Moss | 3 | 3 | ✅ |
| Brady Watkins | 5 | 5 | ✅ |
| Tom Stuart | 6 | 7 | ✅ |
| Tony / Brett / Judd | 7 / 7 / 8 | 8 / 8 / 9 | ✅ |
| Gose | 8 | 9 | ✅ |
| JD Moss | 9 | 10 | ✅ |
| Jensen | 12 | 13 | ✅ |
| Lance Hori | 13 | 14 | ✅ |
| Baugh / Ben | 14 / 14 | 15 / 15 | ✅ |
| Redd | 15 | 16 | ✅ |
| Baker | 17.5 | 19 | ✅ |
| Jack Watkins | 21 | 23 | ✅ |
| Ronnie | 25.5 | 28 | ✅ |

### Stroke Distribution (3 tests)

Verified hole-by-hole net scores for Jeremy (-1 hdcp), Gose (9), and JD (10) against the Excel's Foursomes sheet. All 54 individual hole net scores match exactly.

- **Jeremy (hdcp -1):** Gives 1 stroke on hole 13 (rank 18). Net total = 76.
- **Gose (hdcp 9):** Gets 1 stroke on 9 hardest holes. Net total = 77.
- **JD (hdcp 10):** Gets 1 stroke on 10 hardest holes. Net total = 78.

### Team Nassau Totals (5 tests)

Best-2-of-4 net scores per hole, summed for front/back/18. All 5 teams match Excel:

| Team | Front | Back | Total |
|------|-------|------|-------|
| #1 Jeremy's | 20 | 19 | 39 |
| #2 Sean's | 61 | 65 | 126 |
| #3 Jedd's | 21 | 22 | 43 |
| #4 Brady's | 62 | 63 | 125 |
| #5 Baugh's | -9 | -8 | -17 |

### Total Strokes (5 tests)

Sum of all players' net totals per team (with max cap of 82):

| Team | Total Net | Verified |
|------|-----------|----------|
| #1 | 203 | ✅ (Jeremy 76 + Gose 77 + JD 78 + Ronnie -28) |
| #2 | 290 | ✅ (Sean 70 + Brett 72 + Jensen 71 + Jack 77) |
| #3 | 201 | ✅ (Jedd 73 + Judd 76 + Lance 71 + Raff -19) |
| #4 | 289 | ✅ (Brady 70 + Tom 73 + Ben 72 + Baker 74) |
| #5 | 132 | ✅ (Baugh -14 + Tony 77 + Redd 72 + Brandon -3) |

### Pairwise Nassau Win/Loss (10 tests)

All 10 team pairs verified — who wins front, back, and 18-hole in medal play:

| Matchup | Front Winner | Back Winner | 18-Hole Winner |
|---------|-------------|-------------|----------------|
| T1 vs T2 | T1 | T1 | T1 |
| T1 vs T3 | T1 | T1 | T1 |
| T1 vs T4 | T1 | T1 | T1 |
| T1 vs T5 | T5 | T5 | T5 |
| T2 vs T3 | T3 | T3 | T3 |
| T2 vs T4 | T2 | T4 | T4 |
| T2 vs T5 | T5 | T5 | T5 |
| T3 vs T4 | T3 | T3 | T3 |
| T3 vs T5 | T5 | T5 | T5 |
| T4 vs T5 | T5 | T5 | T5 |

### Pairwise Dollar Totals (10 tests)

Full dollar amounts (Nassau + Strokes + Investment) for all 10 team pairs. All match the Excel's team-vs-team matrix:

| Matchup | $ for Team A | Components |
|---------|-------------|------------|
| T1 vs T2 | +$99 | $15 nassau + $87 strokes − $3 investment |
| T1 vs T3 | +$10 | $15 nassau − $2 strokes − $3 investment |
| T1 vs T4 | +$98 | $15 nassau + $86 strokes − $3 investment |
| T1 vs T5 | −$89 | −$15 nassau − $71 strokes − $3 investment |
| T2 vs T3 | −$104 | −$15 nassau − $89 strokes |
| T2 vs T4 | −$6 | −$5 nassau − $1 strokes |
| T2 vs T5 | −$173 | −$15 nassau − $158 strokes |
| T3 vs T4 | +$103 | $15 nassau + $88 strokes |
| T3 vs T5 | −$84 | −$15 nassau − $69 strokes |
| T4 vs T5 | −$172 | −$15 nassau − $157 strokes |

### Per-Team Grand Totals (5 tests)

Sum of all pairwise results per team. Matches Excel row 24:

| Team | Total Per Man | Status |
|------|--------------|--------|
| #1 Jeremy's | +$118 | Winner |
| #2 Sean's | −$382 | Loser |
| #3 Jedd's | +$113 | Winner |
| #4 Brady's | −$367 | Loser |
| #5 Baugh's | +$518 | Biggest winner |

### Individual Bets — Jack vs JD (8 tests)

Match Play: Jack Watkins (hdcp 23) vs JD Moss (hdcp 10). Jack gets 13 strokes.

| Check | Result | Verified |
|-------|--------|----------|
| Stroke difference | 13 | ✅ |
| Jack net scores (18 holes) | All match Excel | ✅ |
| JD net scores (0 strokes) | = gross scores | ✅ |
| Front 9 match play | Jack 2-up | ✅ |
| Back 9 match play | JD 1-up | ✅ |
| 18-hole match play | Jack 1-up | ✅ |
| Dollar result | Front +$5, Back −$10, 18 +$5 = $0 net | ✅ |
| Auto-presses | None triggered | ✅ |

---

## Test Coverage Summary

| Category | Tests | Status |
|----------|-------|--------|
| Handicap: Index → Course Hdcp | 15 | ✅ All pass |
| Handicap: Stroke Distribution | 3 | ✅ All pass |
| Team: Nassau Totals (best 2 of 4) | 5 | ✅ All pass |
| Team: Total Strokes | 5 | ✅ All pass |
| Team: Pairwise Nassau Win/Loss | 10 | ✅ All pass |
| Team: Pairwise Dollar Totals | 10 | ✅ All pass |
| Team: Per-Team Grand Totals | 5 | ✅ All pass |
| Team: Investment (Excel-validated) | 2 | ✅ All pass |
| Individual: Handicap & Strokes | 3 | ✅ All pass |
| Individual: Match Play Results | 2 | ✅ All pass |
| Individual: Dollar Amounts | 2 | ✅ All pass |
| Individual: Full Integration | 1 | ✅ All pass |
| **Total Cross-Validation** | **63** | **✅ All pass** |
| **Pre-existing Engine Tests** | **64** | **✅ All pass** |
| **API Tests** | **1** | **✅ All pass** |
| **Grand Total** | **128** | **✅ All pass** |

---

## Remaining Items for Future Work

1. **Investment Calculator:** Update to match Excel's best-N-ball approach and handle absent players gracefully (skip absent players, not entire holes).
2. **Dunn Bet:** Not yet implemented in the engine. Excel has a $5 Dunn investment bet.
3. **Expense Deduction:** The Excel applies deduction proportionally across winners (not a flat %). The current engine doesn't implement this deduction layer.
4. **ESC (Equitable Stroke Control):** Score capping for handicap reporting — not yet enforced at the engine level.
5. **Best Ball / Skins / Tournament:** No Excel test data available for cross-validation (sheets are config-only).
6. **Round Robin:** Computation of all C(n,2) pairings — not yet cross-validated.
