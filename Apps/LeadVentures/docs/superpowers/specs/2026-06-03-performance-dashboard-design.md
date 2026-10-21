# Performance Dashboard Page — Design Spec

**Date:** 2026-06-03  
**File:** `BlazorPortal/Components/Pages/DashboardPages/PerforamncePage.razor`  
**Route:** `/DashboardPerformance`  
**Role:** Super Admin only

---

## Goal

Fill in the currently empty `PerforamncePage.razor` to display a daily performance dashboard that mirrors the "Hourly Stats Daily" spreadsheet — showing KG and BSM transfer stats per 30-minute slot alongside best-day comparisons, hourly grouped scores, and a summary header.

---

## Entities

### Performance (one per day)
- `PerformancDate` — the date of this record
- `BestDayId` — FK to another `Performance` used as the "best day" reference
- `KGTarget`, `KGReps`, `KGVerifiers`
- `BSMTarget`, `BSMReps`, `BSMVerifiers`

### PerformanceLine (many per Performance)
- `PerformanceId` — FK to Performance
- `TransferTime` — timestamp of the 30-min slot
- `KgTransfers` — **incremental** KG transfers in this slot
- `BsmTransfers` — **incremental** BSM transfers in this slot

---

## Page Structure

### 1. Date selector
A `RadzenDatePicker` bound to a `SelectedDate` property. On change, load the `Performance` record for that date. If none exists, show a message: _"No performance record found for this date."_

### 2. Summary header
A `RadzenCard` with stat blocks laid out horizontally using `RadzenStack`:

| Block | Content |
|---|---|
| Target | `KGTarget + BSMTarget` |
| KG | Achieved / Target, computed as sum of all KgTransfers / KGTarget |
| BSM | Achieved / Target |
| KG Rem | `KGTarget − KG achieved` |
| BSM Rem | `BSMTarget − BSM achieved` |
| Achieved | `KG achieved + BSM achieved` |
| Remaining | `(KGTarget + BSMTarget) − (KG + BSM achieved)` |

Below that, a small inline table for headcount:

| | Reps | Verifiers |
|---|---|---|
| KG | KGReps | KGVerifiers |
| BSM | BSMReps | BSMVerifiers |
| Total | KGReps + BSMReps | KGVerifiers + BSMVerifiers |

### 3. Main data table

A `RadzenDataGrid` over a `List<PerformanceRowVM>`, no paging (all rows visible), with a footer TOTAL row.

**Columns:**

| # | Column | Value |
|---|---|---|
| 1 | Time | `TransferTime` formatted as `hh:mm tt` |
| 2 | KG Best Day | Cumulative KgTransfers from best-day lines matched by time-of-day |
| 3 | KG Total | Running cumulative KgTransfers (current day) |
| 4 | KG Last 30 Min | `PerformanceLine.KgTransfers` (raw incremental) |
| 5 | KG Hourly Today | Sum of two consecutive 30-min slots for this hour; same value shown on both rows; if only one slot exists for the hour (unpaired), shows `—` |
| 6 | KG Hourly Best | Best-day hourly pair value; same display rule; `—` if no best-day set or no matching slot |
| 7 | BSM Best Day | Cumulative BsmTransfers from best-day |
| 8 | BSM Total | Running cumulative BsmTransfers |
| 9 | BSM Last 30 Min | `PerformanceLine.BsmTransfers` |
| 10 | BSM Hourly Today | Same rule as KG Hourly Today |
| 11 | BSM Hourly Best | Same rule |
| 12 | Score (Inshaa Allah) | `KgHourlyToday + BsmHourlyToday` — same value on both rows of the hour pair |
| 13 | Hourly Score (Mashaa Allah) | Running cumulative of Score, incremented on every row |

**Footer TOTAL row:**
- KG Total / BSM Total → last cumulative value (= total achieved)
- KG Last 30 Min / BSM Last 30 Min → sum of all incremental values
- Score → last HourlyScore value (total combined achieved)

**Dialer Cost columns:** deferred — no entity fields exist yet.

---

## ViewModel

```csharp
public class PerformanceRowVM
{
    public DateTime TransferTime { get; set; }

    // KG columns
    public int? KgBestDay { get; set; }       // cumulative from best-day lines
    public int KgTotal { get; set; }           // running cumulative (current day)
    public int KgLast30Min { get; set; }       // raw incremental
    public int? KgHourlyToday { get; set; }    // hourly pair sum (null = show dash)
    public int? KgHourlyBest { get; set; }

    // BSM columns
    public int? BsmBestDay { get; set; }
    public int BsmTotal { get; set; }
    public int BsmLast30Min { get; set; }
    public int? BsmHourlyToday { get; set; }
    public int? BsmHourlyBest { get; set; }

    // Score columns
    public int? Score { get; set; }            // KgHourlyToday + BsmHourlyToday (same on both rows of pair)
    public int HourlyScore { get; set; }       // running cumulative of Score, per row
}
```

---

## Computation logic (code-behind)

1. Load `Performance` for `SelectedDate` (include `BestDayId`).
2. Load current day's `PerformanceLine` list ordered by `TransferTime` ascending.
3. If `BestDayId` is set, load best-day `PerformanceLine` list ordered by `TransferTime` ascending. Build a `Dictionary<TimeOnly, PerformanceLine>` keyed by `HH:mm` for O(1) lookup. If `BestDayId` is null, all best-day columns display `—`.
4. Iterate lines in order:
   - Accumulate `kgRunning` and `bsmRunning` for running totals.
   - Accumulate `kgBestRunning` and `bsmBestRunning` from matched best-day lines.
   - Detect hour pairs: group by `TransferTime.Hour`. For each completed pair (both :30 and :00 processed), compute `KgHourly = pair[0].KgTransfers + pair[1].KgTransfers` and similarly for BSM.
   - Assign `KgHourlyToday` and `BsmHourlyToday` to **both** rows of the pair.
   - `Score` = `KgHourlyToday + BsmHourlyToday` (assigned to both rows once pair is complete).
   - `HourlyScore` = running sum of `Score` (null Score contributes 0).
5. Compute summary totals from the final running values + `Performance` entity fields.

---

## Styling

- Column group headers (KG, BSM, Score) use distinct background colors matching the spreadsheet (dark for KG, green/teal for BSM, purple for Score).
- The TOTAL footer row is bold.
- Best-day columns use a muted/yellow background to visually distinguish them.
- Uses existing `RadzenCard`, `RadzenDataGrid`, `RadzenStack` components — no new UI dependencies.

---

## Out of scope

- Dialer Cost columns (no entity fields)
- Edit/delete of lines from this page (use existing PerformanceLineIndexPage)
- Export

---

## Files to create/edit

| File | Action |
|---|---|
| `PerforamncePage.razor` | Implement (currently empty shell) |
| `PerforamncePage.razor.cs` | Create (code-behind) |
