# Performance Dashboard Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement `PerforamncePage.razor` as a fully-functional daily performance dashboard showing KG/BSM transfer stats per 30-min slot with best-day comparison, hourly scores, and a summary header.

**Architecture:** Code-behind loads `Performance` + `PerformanceLine` entities for a selected date, builds a `List<PerformanceRowVM>` with all computed columns (running totals, hourly pairs, Score), and the Razor page renders a styled summary header + RadzenDataGrid with footer totals.

**Tech Stack:** Blazor Server (.NET 10), Radzen.Blazor 10.4.3, EF Core (IDbContextFactory), Bootstrap 5 (already in layout)

---

## File Map

| File | Action |
|---|---|
| `BlazorPortal/Models/PerformanceRowVM.cs` | **Create** — ViewModel + `BuildRows` static computation |
| `BlazorPortal/Components/Pages/DashboardPages/PerforamncePage.razor.cs` | **Modify** — code-behind (data loading, summary props) |
| `BlazorPortal/Components/Pages/DashboardPages/PerforamncePage.razor` | **Modify** — full UI (date picker, summary header, data table) |

---

## Task 1: PerformanceRowVM with BuildRows

**Files:**
- Create: `BlazorPortal/Models/PerformanceRowVM.cs`

- [ ] **Step 1: Create the file**

`BlazorPortal/Models/PerformanceRowVM.cs`:

```csharp
#nullable disable
using BlazorPortal.Entities;

namespace BlazorPortal.Models;

public class PerformanceRowVM
{
    public DateTime TransferTime { get; set; }

    // KG columns
    public int? KgBestDay { get; set; }       // cumulative from best-day lines
    public int KgTotal { get; set; }           // running cumulative (current day)
    public int KgLast30Min { get; set; }       // raw incremental
    public int? KgHourlyToday { get; set; }    // pair sum — null if no pair
    public int? KgHourlyBest { get; set; }     // best-day pair sum — null if no pair

    // BSM columns
    public int? BsmBestDay { get; set; }
    public int BsmTotal { get; set; }
    public int BsmLast30Min { get; set; }
    public int? BsmHourlyToday { get; set; }
    public int? BsmHourlyBest { get; set; }

    // Score columns
    public int? Score { get; set; }            // KgHourlyToday + BsmHourlyToday (same on both rows of pair)
    public int HourlyScore { get; set; }       // running cumulative of Score per row

    /// <summary>
    /// Builds computed rows from ordered PerformanceLine list.
    /// Lines are paired positionally (i=0,1), (i=2,3) etc. to form hourly groups.
    /// Best-day lines are matched by time-of-day (HH:mm).
    /// </summary>
    public static List<PerformanceRowVM> BuildRows(
        List<PerformanceLine> lines,
        Dictionary<TimeOnly, PerformanceLine> bestDayByTime)
    {
        var rows = new List<PerformanceRowVM>();
        int kgRunning = 0, bsmRunning = 0;
        int kgBestRunning = 0, bsmBestRunning = 0;
        int hourlyScoreRunning = 0;

        for (int i = 0; i < lines.Count; i += 2)
        {
            var first = lines[i];
            var second = i + 1 < lines.Count ? lines[i + 1] : null;

            var firstTime = TimeOnly.FromDateTime(first.TransferTime);
            bestDayByTime.TryGetValue(firstTime, out var bestFirst);

            PerformanceLine bestSecond = null;
            if (second != null)
            {
                var secondTime = TimeOnly.FromDateTime(second.TransferTime);
                bestDayByTime.TryGetValue(secondTime, out bestSecond);
            }

            int? kgHourly = second != null ? first.KgTransfers + second.KgTransfers : null;
            int? bsmHourly = second != null ? first.BsmTransfers + second.BsmTransfers : null;
            int? score = kgHourly.HasValue && bsmHourly.HasValue ? kgHourly + bsmHourly : null;
            int? kgBestHourly = bestFirst != null && bestSecond != null
                ? bestFirst.KgTransfers + bestSecond.KgTransfers : null;
            int? bsmBestHourly = bestFirst != null && bestSecond != null
                ? bestFirst.BsmTransfers + bestSecond.BsmTransfers : null;

            // First row of pair
            kgRunning += first.KgTransfers;
            bsmRunning += first.BsmTransfers;
            kgBestRunning += bestFirst?.KgTransfers ?? 0;
            bsmBestRunning += bestFirst?.BsmTransfers ?? 0;
            hourlyScoreRunning += score ?? 0;

            rows.Add(new PerformanceRowVM
            {
                TransferTime = first.TransferTime,
                KgBestDay = bestFirst != null ? kgBestRunning : null,
                KgTotal = kgRunning,
                KgLast30Min = first.KgTransfers,
                KgHourlyToday = kgHourly,
                KgHourlyBest = kgBestHourly,
                BsmBestDay = bestFirst != null ? bsmBestRunning : null,
                BsmTotal = bsmRunning,
                BsmLast30Min = first.BsmTransfers,
                BsmHourlyToday = bsmHourly,
                BsmHourlyBest = bsmBestHourly,
                Score = score,
                HourlyScore = hourlyScoreRunning
            });

            if (second == null) continue;

            // Second row of pair
            kgRunning += second.KgTransfers;
            bsmRunning += second.BsmTransfers;
            kgBestRunning += bestSecond?.KgTransfers ?? 0;
            bsmBestRunning += bestSecond?.BsmTransfers ?? 0;
            hourlyScoreRunning += score ?? 0;

            rows.Add(new PerformanceRowVM
            {
                TransferTime = second.TransferTime,
                KgBestDay = bestSecond != null ? kgBestRunning : null,
                KgTotal = kgRunning,
                KgLast30Min = second.KgTransfers,
                KgHourlyToday = kgHourly,
                KgHourlyBest = kgBestHourly,
                BsmBestDay = bestSecond != null ? bsmBestRunning : null,
                BsmTotal = bsmRunning,
                BsmLast30Min = second.BsmTransfers,
                BsmHourlyToday = bsmHourly,
                BsmHourlyBest = bsmBestHourly,
                Score = score,
                HourlyScore = hourlyScoreRunning
            });
        }

        return rows;
    }
}
```

- [ ] **Step 2: Build to verify no errors**

```
dotnet build BlazorPortal/BlazorPortal.csproj
```
Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Commit**

```
git add BlazorPortal/Models/PerformanceRowVM.cs
git commit -m "feat: add PerformanceRowVM with BuildRows computation"
```

---

## Task 2: Code-behind (PerforamncePage.razor.cs)

**Files:**
- Modify: `BlazorPortal/Components/Pages/DashboardPages/PerforamncePage.razor.cs`

- [ ] **Step 1: Replace the empty code-behind**

`BlazorPortal/Components/Pages/DashboardPages/PerforamncePage.razor.cs`:

```csharp
#nullable disable
using BlazorPortal.Entities;
using BlazorPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.DashboardPages;

public partial class PerforamncePage
{
    private DateTime SelectedDate { get; set; } = DateTime.Today;
    private Performance CurrentPerformance { get; set; }
    private List<PerformanceRowVM> Rows { get; set; }

    // Summary computed props — safe to call even when Rows/CurrentPerformance is null
    private int KgAchieved => Rows?.LastOrDefault()?.KgTotal ?? 0;
    private int BsmAchieved => Rows?.LastOrDefault()?.BsmTotal ?? 0;
    private int OverallTarget => (CurrentPerformance?.KGTarget ?? 0) + (CurrentPerformance?.BSMTarget ?? 0);
    private int KgRem => (CurrentPerformance?.KGTarget ?? 0) - KgAchieved;
    private int BsmRem => (CurrentPerformance?.BSMTarget ?? 0) - BsmAchieved;
    private int TotalAchieved => KgAchieved + BsmAchieved;
    private int TotalRemaining => OverallTarget - TotalAchieved;
    private int TotalReps => (CurrentPerformance?.KGReps ?? 0) + (CurrentPerformance?.BSMReps ?? 0);
    private int TotalVerifiers => (CurrentPerformance?.KGVerifiers ?? 0) + (CurrentPerformance?.BSMVerifiers ?? 0);

    // Footer totals for the data grid
    private int FooterKgLast30Min => Rows?.Sum(r => r.KgLast30Min) ?? 0;
    private int FooterBsmLast30Min => Rows?.Sum(r => r.BsmLast30Min) ?? 0;
    private int FooterHourlyScore => Rows?.LastOrDefault()?.HourlyScore ?? 0;




    protected override async Task OnInitializedAsync()
    {
        await LoadPerformance();
    }




    private async Task LoadPerformance()
    {
        using var context = contextFactory.CreateDbContext();

        CurrentPerformance = await context.Performances
            .FirstOrDefaultAsync(x => x.PerformancDate == SelectedDate.Date);

        if (CurrentPerformance is null)
        {
            Rows = null;
            return;
        }

        var lines = await context.PerformanceLines
            .Where(x => x.PerformanceId == CurrentPerformance.Id)
            .OrderBy(x => x.TransferTime)
            .ToListAsync();

        var bestDayByTime = new Dictionary<TimeOnly, PerformanceLine>();
        if (CurrentPerformance.BestDayId.HasValue)
        {
            var bestLines = await context.PerformanceLines
                .Where(x => x.PerformanceId == CurrentPerformance.BestDayId.Value)
                .OrderBy(x => x.TransferTime)
                .ToListAsync();

            foreach (var bl in bestLines)
                bestDayByTime[TimeOnly.FromDateTime(bl.TransferTime)] = bl;
        }

        Rows = PerformanceRowVM.BuildRows(lines, bestDayByTime);
    }
}
```

- [ ] **Step 2: Build to verify no errors**

```
dotnet build BlazorPortal/BlazorPortal.csproj
```
Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Commit**

```
git add BlazorPortal/Components/Pages/DashboardPages/PerforamncePage.razor.cs
git commit -m "feat: add PerforamncePage code-behind with data loading"
```

---

## Task 3: Razor UI (PerforamncePage.razor)

**Files:**
- Modify: `BlazorPortal/Components/Pages/DashboardPages/PerforamncePage.razor`

- [ ] **Step 1: Replace the empty razor file**

`BlazorPortal/Components/Pages/DashboardPages/PerforamncePage.razor`:

```razor
@page "/DashboardPerformance"
@attribute [Authorize(Roles = "Super Admin")]
<PageTitle>Performance Dashboard</PageTitle>

@inject IDbContextFactory<ApplicationDbContext> contextFactory

<PageHeading HeadingText="Performance Dashboard" />

@* Date selector *@
<RadzenCard class="mb-3">
    <RadzenStack Orientation="Orientation.Horizontal" AlignItems="AlignItems.Center" Gap="1rem">
        <RadzenLabel Text="Select Date" />
        <RadzenDatePicker @bind-Value="SelectedDate"
                          DateFormat="dd MMM yyyy"
                          ShowTime="false"
                          Change="LoadPerformance" />
    </RadzenStack>
</RadzenCard>

@if (CurrentPerformance is null)
{
    <div class="alert alert-warning">No performance record found for @SelectedDate.ToString("dd MMM yyyy").</div>
}
else
{
    @* ===== SUMMARY HEADER ===== *@
    <RadzenCard class="mb-3 p-3">
        <div class="d-flex flex-wrap gap-2 align-items-stretch mb-3">

            @* Overall Target *@
            <div class="perf-block" style="background:#111;color:#fff;">
                <div class="perf-val">@OverallTarget</div>
                <div class="perf-lbl">Target</div>
            </div>

            @* KG *@
            <div class="perf-block" style="background:#1a1a2e;color:#fff;">
                <div class="perf-section-title">KG</div>
                <div class="perf-val">@KgAchieved</div>
                <div class="perf-lbl">/ @CurrentPerformance!.KGTarget</div>
            </div>

            @* BSM *@
            <div class="perf-block" style="background:#1a2e1a;color:#fff;">
                <div class="perf-section-title">BSM</div>
                <div class="perf-val">@BsmAchieved</div>
                <div class="perf-lbl">/ @CurrentPerformance!.BSMTarget</div>
            </div>

            @* KG Rem *@
            <div class="perf-block" style="background:#6b0000;color:#fff;">
                <div class="perf-lbl">KG Rem</div>
                <div class="perf-val">@KgRem</div>
            </div>

            @* BSM Rem *@
            <div class="perf-block" style="background:#6b3300;color:#fff;">
                <div class="perf-lbl">BSM Rem</div>
                <div class="perf-val">@BsmRem</div>
            </div>

            @* Achieved *@
            <div class="perf-block" style="background:#1a3a1a;color:#fff;">
                <div class="perf-lbl">Achieved</div>
                <div class="perf-val">@TotalAchieved</div>
            </div>

            @* Remaining *@
            <div class="perf-block" style="background:#3a1a1a;color:#fff;">
                <div class="perf-lbl">Remaining</div>
                <div class="perf-val">@TotalRemaining</div>
            </div>

        </div>

        @* Headcount table *@
        <table class="table table-sm table-bordered mb-0" style="max-width:320px;">
            <thead class="table-dark">
                <tr>
                    <th></th>
                    <th class="text-center">Reps</th>
                    <th class="text-center">Verifiers</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td><strong>KG</strong></td>
                    <td class="text-center">@CurrentPerformance!.KGReps</td>
                    <td class="text-center">@CurrentPerformance!.KGVerifiers</td>
                </tr>
                <tr>
                    <td><strong>BSM</strong></td>
                    <td class="text-center">@CurrentPerformance!.BSMReps</td>
                    <td class="text-center">@CurrentPerformance!.BSMVerifiers</td>
                </tr>
                <tr class="table-secondary fw-bold">
                    <td><strong>Total</strong></td>
                    <td class="text-center">@TotalReps</td>
                    <td class="text-center">@TotalVerifiers</td>
                </tr>
            </tbody>
        </table>
    </RadzenCard>

    @* ===== MAIN DATA TABLE ===== *@
    @if (Rows is null || Rows.Count == 0)
    {
        <div class="alert alert-info">No performance lines recorded for this date.</div>
    }
    else
    {
        <RadzenCard>
            <RadzenDataGrid Data="@Rows" AllowPaging="false" AllowSorting="false" AllowColumnResize="true">
                <Columns>

                    @* Time *@
                    <RadzenDataGridColumn Title="Time" Width="90px" TextAlign="TextAlign.Center">
                        <Template Context="row">
                            @row.TransferTime.ToString("hh:mm tt")
                        </Template>
                        <FooterTemplate>
                            <b>TOTAL</b>
                        </FooterTemplate>
                    </RadzenDataGridColumn>

                    @* ===== KG SECTION ===== *@
                    <RadzenDataGridColumn Title="KG Best Day" Width="105px" TextAlign="TextAlign.Center"
                                          HeaderCssClass="perf-header-kg">
                        <Template Context="row">
                            @(row.KgBestDay?.ToString() ?? "—")
                        </Template>
                        <FooterTemplate>
                            <b>@(Rows.LastOrDefault()?.KgBestDay?.ToString() ?? "—")</b>
                        </FooterTemplate>
                    </RadzenDataGridColumn>

                    <RadzenDataGridColumn Title="KG Total" Width="90px" TextAlign="TextAlign.Center"
                                          HeaderCssClass="perf-header-kg">
                        <Template Context="row">
                            @row.KgTotal
                        </Template>
                        <FooterTemplate>
                            <b>@KgAchieved</b>
                        </FooterTemplate>
                    </RadzenDataGridColumn>

                    <RadzenDataGridColumn Title="KG Last 30 Min" Width="115px" TextAlign="TextAlign.Center"
                                          HeaderCssClass="perf-header-kg">
                        <Template Context="row">
                            @row.KgLast30Min
                        </Template>
                        <FooterTemplate>
                            <b>@FooterKgLast30Min</b>
                        </FooterTemplate>
                    </RadzenDataGridColumn>

                    <RadzenDataGridColumn Title="KG Hourly Today" Width="120px" TextAlign="TextAlign.Center"
                                          HeaderCssClass="perf-header-kg-hourly">
                        <Template Context="row">
                            @(row.KgHourlyToday?.ToString() ?? "—")
                        </Template>
                        <FooterTemplate>
                            <b>@FooterKgLast30Min</b>
                        </FooterTemplate>
                    </RadzenDataGridColumn>

                    <RadzenDataGridColumn Title="KG Hourly Best" Width="115px" TextAlign="TextAlign.Center"
                                          HeaderCssClass="perf-header-kg-hourly">
                        <Template Context="row">
                            @(row.KgHourlyBest?.ToString() ?? "—")
                        </Template>
                        <FooterTemplate>
                            <b>—</b>
                        </FooterTemplate>
                    </RadzenDataGridColumn>

                    @* ===== BSM SECTION ===== *@
                    <RadzenDataGridColumn Title="BSM Best Day" Width="110px" TextAlign="TextAlign.Center"
                                          HeaderCssClass="perf-header-bsm">
                        <Template Context="row">
                            @(row.BsmBestDay?.ToString() ?? "—")
                        </Template>
                        <FooterTemplate>
                            <b>@(Rows.LastOrDefault()?.BsmBestDay?.ToString() ?? "—")</b>
                        </FooterTemplate>
                    </RadzenDataGridColumn>

                    <RadzenDataGridColumn Title="BSM Total" Width="95px" TextAlign="TextAlign.Center"
                                          HeaderCssClass="perf-header-bsm">
                        <Template Context="row">
                            @row.BsmTotal
                        </Template>
                        <FooterTemplate>
                            <b>@BsmAchieved</b>
                        </FooterTemplate>
                    </RadzenDataGridColumn>

                    <RadzenDataGridColumn Title="BSM Last 30 Min" Width="120px" TextAlign="TextAlign.Center"
                                          HeaderCssClass="perf-header-bsm">
                        <Template Context="row">
                            @row.BsmLast30Min
                        </Template>
                        <FooterTemplate>
                            <b>@FooterBsmLast30Min</b>
                        </FooterTemplate>
                    </RadzenDataGridColumn>

                    <RadzenDataGridColumn Title="BSM Hourly Today" Width="125px" TextAlign="TextAlign.Center"
                                          HeaderCssClass="perf-header-bsm-hourly">
                        <Template Context="row">
                            @(row.BsmHourlyToday?.ToString() ?? "—")
                        </Template>
                        <FooterTemplate>
                            <b>@FooterBsmLast30Min</b>
                        </FooterTemplate>
                    </RadzenDataGridColumn>

                    <RadzenDataGridColumn Title="BSM Hourly Best" Width="120px" TextAlign="TextAlign.Center"
                                          HeaderCssClass="perf-header-bsm-hourly">
                        <Template Context="row">
                            @(row.BsmHourlyBest?.ToString() ?? "—")
                        </Template>
                        <FooterTemplate>
                            <b>—</b>
                        </FooterTemplate>
                    </RadzenDataGridColumn>

                    @* ===== SCORE SECTION ===== *@
                    <RadzenDataGridColumn Title="Score (Inshaa Allah)" Width="140px" TextAlign="TextAlign.Center"
                                          HeaderCssClass="perf-header-score">
                        <Template Context="row">
                            @(row.Score?.ToString() ?? "—")
                        </Template>
                        <FooterTemplate>
                            <b>@TotalAchieved</b>
                        </FooterTemplate>
                    </RadzenDataGridColumn>

                    <RadzenDataGridColumn Title="Hourly Score (Mashaa Allah)" Width="170px" TextAlign="TextAlign.Center"
                                          HeaderCssClass="perf-header-score">
                        <Template Context="row">
                            @row.HourlyScore
                        </Template>
                        <FooterTemplate>
                            <b>@FooterHourlyScore</b>
                        </FooterTemplate>
                    </RadzenDataGridColumn>

                </Columns>
            </RadzenDataGrid>
        </RadzenCard>
    }
}

<style>
    .perf-block {
        padding: 8px 16px;
        border-radius: 4px;
        text-align: center;
        min-width: 90px;
    }

    .perf-val {
        font-size: 1.8rem;
        font-weight: 700;
        line-height: 1.1;
    }

    .perf-lbl {
        font-size: 0.75rem;
        opacity: 0.85;
    }

    .perf-section-title {
        font-size: 0.8rem;
        font-weight: 600;
        opacity: 0.75;
    }

    .perf-header-kg {
        background-color: #1a1a2e !important;
        color: #fff !important;
    }

    .perf-header-kg-hourly {
        background-color: #ffc107 !important;
        color: #000 !important;
    }

    .perf-header-bsm {
        background-color: #155724 !important;
        color: #fff !important;
    }

    .perf-header-bsm-hourly {
        background-color: #856404 !important;
        color: #fff !important;
    }

    .perf-header-score {
        background-color: #4a0080 !important;
        color: #fff !important;
    }
</style>
```

- [ ] **Step 2: Build to verify no errors**

```
dotnet build BlazorPortal/BlazorPortal.csproj
```
Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Commit**

```
git add BlazorPortal/Components/Pages/DashboardPages/PerforamncePage.razor
git commit -m "feat: implement performance dashboard UI"
```

---

## Task 4: Verify in Browser

- [ ] **Step 1: Run the app**

```
dotnet run --project BlazorPortal/BlazorPortal.csproj
```

- [ ] **Step 2: Navigate to `/DashboardPerformance`**

Log in as Super Admin, go to `/DashboardPerformance`.

- [ ] **Step 3: Verify date picker and no-record message**

Pick a date with no Performance record → should show the yellow "No performance record found" alert.

- [ ] **Step 4: Verify with a real record**

Pick a date that has a Performance record (check `/PerformanceIndex` for existing dates) → should show:
- Summary header with colored blocks showing target, achieved, remaining, rems
- Headcount table with KG/BSM reps and verifiers
- Data table with all 13 columns populated
- Footer TOTAL row in bold

- [ ] **Step 5: Verify with a record that has BestDayId set**

Best-day columns (`KG Best Day`, `BSM Best Day`, `KG Hourly Best`, `BSM Hourly Best`) should show values; without BestDayId set, they should all show `—`.

- [ ] **Step 6: Commit if any minor fixes were needed**

```
git add -A
git commit -m "fix: minor corrections to performance dashboard"
```

