import { useEffect, useState } from 'react'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { apiUrl, formatDate, getAccessToken } from '../session'
import './CompanionDashboard.css'

type ReportRow = {
  transferDate: string | null
  fullName: string
  states: string
  scheduleFrom: string | null
  scheduleTo: string | null
  costPerLead: number | null
  target: number
  achieved: number
  billable: number
  nonBillable: number
  totalAmount: number
}

type TransferDay = {
  transferDate: string | null
  total: number | null
  billable: number | null
  nonBillable: number | null
}

const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })

function addDays(date: Date, days: number) {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate() + days)
}

function toInputValue(date: Date) {
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${date.getFullYear()}-${month}-${day}`
}

function fromInputValue(value: string) {
  const [year, month, day] = value.split('-').map(Number)
  return new Date(year, month - 1, day)
}

function formatDay(date: Date) {
  return formatDate(date)
}

function reportingToday() {
  // LeadLogix's existing day reports use a fixed UTC-05:00 business date.
  return fromInputValue(new Date(Date.now() - 5 * 60 * 60 * 1000).toISOString().slice(0, 10))
}

function weekStart(date: Date) {
  return addDays(date, -((date.getDay() + 6) % 7))
}

function formatTime(value: string | null) {
  return value ? new Date(value).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }) : '—'
}

function useReport<T>(app: 'leadlogix' | 'leadventures', path: string) {
  const [attempt, setAttempt] = useState(0)
  const key = `${path}:${attempt}`
  const [result, setResult] = useState<{ key: string; data: T | null; error: string }>({ key: '', data: null, error: '' })

  useEffect(() => {
    const controller = new AbortController()
    const load = async () => {
      try {
        const response = await fetch(`${apiUrl}/${app}/dashboard/${path}`, {
          headers: { Authorization: `Bearer ${getAccessToken()}` },
          signal: controller.signal,
        })
        if (!response.ok) {
          if (response.status === 401) throw new Error(`Your session has expired. Sign in again to view ${app === 'leadlogix' ? 'LeadLogix' : 'LeadVentures'} reports.`)
          if (response.status === 403) throw new Error(`Administrator access is required to view ${app === 'leadlogix' ? 'LeadLogix' : 'LeadVentures'} reports.`)
          const problem: { detail?: string; title?: string; errors?: Record<string, string[]> } = response.headers.get('content-type')?.includes('json')
            ? await response.json()
            : {}
          throw new Error(Object.values(problem.errors ?? {}).flat().join(' ') || problem.detail || problem.title || `Unable to load the ${app === 'leadlogix' ? 'LeadLogix' : 'LeadVentures'} report.`)
        }
        const data: T = await response.json()
        if (!controller.signal.aborted) setResult({ key, data, error: '' })
      } catch (error) {
        if (!controller.signal.aborted) {
          setResult({ key, data: null, error: error instanceof Error ? error.message : `Unable to load the ${app === 'leadlogix' ? 'LeadLogix' : 'LeadVentures'} report.` })
        }
      }
    }
    void load()
    return () => controller.abort()
  }, [app, path, key])

  return {
    data: result.key === key ? result.data : null,
    error: result.key === key ? result.error : '',
    retry: () => setAttempt((value) => value + 1),
  }
}

function ReportStatus({ error, retry }: { error: string; retry: () => void }) {
  return error
    ? <div className="users-error" role="alert">{error} <button className="ll-button" onClick={retry}>Retry</button></div>
    : <div className="empty-members" role="status">Loading report...</div>
}

function sum(rows: ReportRow[], key: 'target' | 'achieved' | 'billable' | 'nonBillable' | 'totalAmount') {
  return rows.reduce((total, row) => total + row[key], 0)
}

function averageCostPerLead(rows: ReportRow[]) {
  const costs = rows.flatMap((row) => row.costPerLead === null ? [] : [row.costPerLead])
  return costs.length ? currency.format(costs.reduce((total, cost) => total + cost, 0) / costs.length) : '—'
}

function TransfersChart({ data, error, retry }: { data: TransferDay[] | null; error: string; retry: () => void }) {
  const max = Math.max(1, ...(data ?? []).map((item) => item.total ?? 0))
  const series = [
    { key: 'total', label: 'Transfers', className: 'll-bar-total' },
    { key: 'billable', label: 'Billable', className: 'll-bar-billable' },
    { key: 'nonBillable', label: 'Non Billable', className: 'll-bar-nonbillable' },
  ] as const

  return (
    <section className="panel ll-chart-panel">
      <div className="panel-heading table-heading">
        <div><p className="panel-kicker">Latest 7 recorded days</p><h2>Transfers (Day)</h2></div>
        <div className="ll-legend">
          {series.map((item) => <span key={item.key}><i className={item.className} />{item.label}</span>)}
        </div>
      </div>
      {!data ? <ReportStatus error={error} retry={retry} />
        : data.length === 0 ? <div className="empty-members">No transfer records found.</div>
        : <div className="ll-chart" role="img" aria-label="Transfers, billable and non billable for the latest seven recorded days">
        {data.map((item, index) => (
          <div className="ll-chart-group" key={item.transferDate ?? index}>
            <div className="ll-chart-bars">
              {series.map((serie) => (
                <div className="ll-bar-slot" key={serie.key}>
                  <span className="ll-bar-value">{item[serie.key] ?? '—'}</span>
                  <div className={`ll-bar ${serie.className}`} style={{ height: `${((item[serie.key] ?? 0) / max) * 100}%` }} />
                </div>
              ))}
            </div>
            <span className="ll-chart-label">{item.transferDate ? formatDay(fromInputValue(item.transferDate)) : 'Undated'}</span>
          </div>
        ))}
      </div>}
    </section>
  )
}

function ReportTable({ rows, showDate }: { rows: ReportRow[]; showDate?: Date }) {
  if (rows.length === 0) return <div className="empty-members">No report records found for the selected dates.</div>

  return (
    <div className="table-wrap">
      <table className="ll-report-table">
        <thead>
          <tr>
            {showDate && <th>Transfer Date</th>}
            <th>Full Name</th><th>States</th>
            {showDate && <th className="ll-center">Time Zones</th>}
            <th className="ll-center">Target</th><th className="ll-center">Achieved</th>
            <th className="ll-center">Billable</th><th className="ll-center">Non Billable</th>
            <th className="ll-center">Cost Per Lead</th><th className="ll-center">Total Amount</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row, index) => (
            <tr key={`${row.fullName}-${index}`}>
              {showDate && <td>{row.transferDate ? formatDay(fromInputValue(row.transferDate)) : '—'}</td>}
              <td><strong className="demo-record-name">{row.fullName}</strong></td>
              <td>{row.states}</td>
              {showDate && <td className="ll-center">{formatTime(row.scheduleFrom)} - {formatTime(row.scheduleTo)}</td>}
              <td className="ll-center">{row.target}</td>
              <td className="ll-center">{row.achieved}</td>
              <td className="ll-center">{row.billable}</td>
              <td className="ll-center">{row.nonBillable}</td>
              <td className="ll-center">{row.costPerLead === null ? '—' : currency.format(row.costPerLead)}</td>
              <td className="ll-center">{currency.format(row.totalAmount)}</td>
            </tr>
          ))}
        </tbody>
        <tfoot>
          <tr>
            <td colSpan={showDate ? 4 : 2}><b>Total</b></td>
            <td className="ll-center"><b>{sum(rows, 'target')}</b></td>
            <td className="ll-center"><b>{sum(rows, 'achieved')}</b></td>
            <td className="ll-center"><b>{sum(rows, 'billable')}</b></td>
            <td className="ll-center"><b>{sum(rows, 'nonBillable')}</b></td>
            <td className="ll-center"><b>{averageCostPerLead(rows)}</b></td>
            <td className="ll-center"><b>{currency.format(sum(rows, 'totalAmount'))}</b></td>
          </tr>
        </tfoot>
      </table>
    </div>
  )
}

function CompanionDashboard({ app }: { app: 'leadlogix' | 'leadventures' }) {
  const [dailyDateOverride, setDailyDateOverride] = useState<Date | null>(null)
  const [weekFromOverride, setWeekFromOverride] = useState<Date | null>(null)
  const [weekToOverride, setWeekToOverride] = useState<Date | null>(null)
  const [loadedRangeOverride, setLoadedRangeOverride] = useState<{ from: Date; to: Date } | null>(null)
  const transfersReport = useReport<TransferDay[]>(app, 'transfers')
  const latestTransferDate = transfersReport.data?.at(-1)?.transferDate
  const dailyDate = dailyDateOverride ?? (latestTransferDate ? fromInputValue(latestTransferDate) : reportingToday())
  const defaultWeekFrom = weekStart(dailyDate)
  const defaultWeekTo = addDays(defaultWeekFrom, 6)
  const weekFrom = weekFromOverride ?? defaultWeekFrom
  const weekTo = weekToOverride ?? defaultWeekTo
  const loadedRange = loadedRangeOverride ?? { from: defaultWeekFrom, to: defaultWeekTo }

  const dayReport = useReport<ReportRow[]>(app, `daily?date=${toInputValue(dailyDate)}`)
  const weeklyReport = useReport<ReportRow[]>(app, `weekly?from=${toInputValue(loadedRange.from)}&to=${toInputValue(loadedRange.to)}`)
  const invalidRange = weekFrom > weekTo

  const shiftWeek = (days: number) => {
    const from = addDays(weekFrom, days)
    const to = addDays(weekTo, days)
    setWeekFromOverride(from)
    setWeekToOverride(to)
    setLoadedRangeOverride({ from, to })
  }

  return (
    <>
      <TransfersChart data={transfersReport.data} error={transfersReport.error} retry={transfersReport.retry} />

      <section className="panel ll-report-panel">
        <div className="panel-heading table-heading">
          <div><p className="panel-kicker">Agent performance</p><h2>Day Report</h2></div>
        </div>
        <div className="ll-toolbar">
          <label>Select Date
            <input type="date" value={toInputValue(dailyDate)} onChange={(event) => {
              if (event.target.value) {
                setDailyDateOverride(fromInputValue(event.target.value))
              }
            }} />
          </label>
          <button className="ll-button" onClick={() => {
            setDailyDateOverride(addDays(dailyDate, -1))
          }}><ChevronLeft size={15} /> Previous</button>
          <button className="ll-button" onClick={() => {
            setDailyDateOverride(addDays(dailyDate, 1))
          }}>Next <ChevronRight size={15} /></button>
        </div>
        {dayReport.data
          ? <ReportTable rows={dayReport.data} showDate={dailyDate} />
          : <ReportStatus error={dayReport.error} retry={dayReport.retry} />}
      </section>

      <section className="panel ll-report-panel">
        <div className="panel-heading table-heading">
          <div><p className="panel-kicker">{formatDay(loadedRange.from)} – {formatDay(loadedRange.to)}</p><h2>Weekly Report</h2></div>
        </div>
        <div className="ll-toolbar">
          <label>From
            <input type="date" value={toInputValue(weekFrom)} onChange={(event) => {
              if (event.target.value) {
                setWeekFromOverride(fromInputValue(event.target.value))
              }
            }} />
          </label>
          <label>To
            <input type="date" value={toInputValue(weekTo)} onChange={(event) => {
              if (event.target.value) {
                setWeekToOverride(fromInputValue(event.target.value))
              }
            }} />
          </label>
          <button className="ll-button ll-button-primary" disabled={invalidRange} onClick={() => {
            if (weekFrom.getTime() === loadedRange.from.getTime() && weekTo.getTime() === loadedRange.to.getTime()) weeklyReport.retry()
            else setLoadedRangeOverride({ from: weekFrom, to: weekTo })
          }}>Load</button>
          <button className="ll-button" disabled={invalidRange} onClick={() => {
            shiftWeek(-7)
          }}><ChevronLeft size={15} /> Previous</button>
          <button className="ll-button" disabled={invalidRange} onClick={() => {
            shiftWeek(7)
          }}>Next <ChevronRight size={15} /></button>
        </div>
        {invalidRange && <div className="users-error" role="alert">The end date must be on or after the start date.</div>}
        {weeklyReport.data
          ? <ReportTable rows={weeklyReport.data} />
          : <ReportStatus error={weeklyReport.error} retry={weeklyReport.retry} />}
      </section>
    </>
  )
}

export default CompanionDashboard
