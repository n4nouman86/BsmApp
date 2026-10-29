import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { Eye, Menu, Pencil, Plus, Search, Trash2, X } from 'lucide-react'
import AppSidebar from '../../Shared/AppSidebar'
import { toggleNavigation } from '../../Shared/sidebarToggle'
import { showToast } from '../../Shared/toast'
import { apiUrl, authenticatedFetch, formatDate, getAccessToken, getCurrentUser } from '../../session'
import { leadFields, leadGroups, leadValues } from './leadFields'
import StateCombobox from './StateCombobox'
import type { Lead, LeadValues } from './leadFields'
import './Leads.css'

async function responseError(response: Response, fallback: string) {
  if (response.headers.get('content-type')?.includes('json')) {
    const problem: { title?: string; detail?: string; errors?: Record<string, string[]> } = await response.json()
    return Object.values(problem.errors ?? {}).flat().join(' ') || problem.detail || problem.title || fallback
  }
  return `${fallback} (HTTP ${response.status})`
}

const blockReasons: Record<string, string> = { tcpa: 'TCPA litigator', dnc_complainers: 'DNC complainer' }
const blockReason = (status: string) => {
  const lists = status.split(/[,|]/).map(item => item.trim()).filter(Boolean)
  return lists.map(item => blockReasons[item] ?? item.replace(/_/g, ' ')).join(', ')
}

type CheckState = { state: 'checking' | 'clean' | 'blocked' | 'error' | 'skipped'; message: string } | null

function LeadsPage() {
  const [leads, setLeads] = useState<Lead[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const [submittedSearch, setSubmittedSearch] = useState('')
  const [reload, setReload] = useState(0)
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const [editor, setEditor] = useState<{ lead?: Lead } | null>(null)
  const [details, setDetails] = useState<Lead | null>(null)
  const [deleting, setDeleting] = useState<Lead | null>(null)
  const [values, setValues] = useState<LeadValues>(() => leadValues())
  const [isSaving, setIsSaving] = useState(false)
  const [formError, setFormError] = useState('')
  const [statuses, setStatuses] = useState<{ id: number; name: string }[]>([])
  const [phoneCheck, setPhoneCheck] = useState<CheckState>(null)
  const [dncCheck, setDncCheck] = useState<CheckState>(null)
  const [confirm, setConfirm] = useState<'save' | 'discard' | null>(null)
  const dialogRef = useRef<HTMLDialogElement>(null)
  const confirmRef = useRef<HTMLDialogElement>(null)
  const user = getCurrentUser()
  const isAdmin = user?.roles.includes('Admin') ?? false
  const modalOpen = editor !== null || details !== null || deleting !== null

  useEffect(() => {
    if (modalOpen) dialogRef.current?.showModal()
    else dialogRef.current?.close()
  }, [modalOpen])

  useEffect(() => {
    if (confirm) confirmRef.current?.showModal()
    else confirmRef.current?.close()
  }, [confirm])

  useEffect(() => {
    const controller = new AbortController()
    const load = async () => {
      setIsLoading(true)
      setError('')
      try {
        const query = submittedSearch ? `?${new URLSearchParams({ search: submittedSearch })}` : ''
        const [response, statusResponse] = await Promise.all([
          authenticatedFetch(`${apiUrl}/leads${query}`, { signal: controller.signal, headers: { Authorization: `Bearer ${getAccessToken()}` } }),
          authenticatedFetch(`${apiUrl}/leadstatuses`, { signal: controller.signal, headers: { Authorization: `Bearer ${getAccessToken()}` } }),
        ])
        if (!response.ok) throw new Error(await responseError(response, 'Unable to load leads.'))
        if (!statusResponse.ok) throw new Error(await responseError(statusResponse, 'Unable to load lead statuses.'))
        const data: Lead[] = await response.json()
        const statusData: { id: number; name: string }[] = await statusResponse.json()
        if (!controller.signal.aborted) { setLeads(data); setStatuses(statusData) }
      } catch (loadError) {
        if (!controller.signal.aborted) setError(loadError instanceof Error ? loadError.message : 'Unable to load leads.')
      } finally {
        if (!controller.signal.aborted) setIsLoading(false)
      }
    }
    void load()
    return () => controller.abort()
  }, [reload, submittedSearch])

  const submitSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setSubmittedSearch(search.trim())
    setReload(current => current + 1)
  }

  const openEditor = (lead?: Lead) => {
    setValues(leadValues(lead))
    setFormError('')
    setPhoneCheck(null)
    setEditor({ lead })
  }

  const closeModal = () => {
    if (isSaving) return
    setEditor(null)
    setDetails(null)
    setDeleting(null)
    setFormError('')
  }

  const checkPhone = async () => {
    const phone = values.phoneNumber.trim()
    if (!phone || phone === editor?.lead?.phoneNumber) { setPhoneCheck(null); setDncCheck(null); return }
    setPhoneCheck({ state: 'checking', message: 'Checking...' })
    setDncCheck({ state: 'checking', message: 'Checking...' })
    try {
      const dncResponse = await authenticatedFetch(`${apiUrl}/tcpa/dnc-check?phoneNumber=${encodeURIComponent(phone)}`, {
        headers: { Authorization: `Bearer ${getAccessToken()}` },
      })
      if (dncResponse.status === 400) {
        setDncCheck({ state: 'error', message: 'Invalid phone number' })
        setPhoneCheck({ state: 'error', message: 'Invalid phone number' })
        return
      }
      if (!dncResponse.ok) throw new Error('DNC check failed')
      const dnc: { phoneNumber: string; isListed: boolean } = await dncResponse.json()
      if (dnc.isListed) {
        setDncCheck({ state: 'blocked', message: 'On DNC list' })
        setPhoneCheck({ state: 'skipped', message: 'Skipped' })
        return
      }
      setDncCheck({ state: 'clean', message: 'Clear' })

      const response = await authenticatedFetch(`${apiUrl}/tcpa/check?phoneNumber=${encodeURIComponent(phone)}`, {
        headers: { Authorization: `Bearer ${getAccessToken()}` },
      })
      if (response.status === 400) { setPhoneCheck({ state: 'error', message: 'Invalid phone number' }); return }
      if (!response.ok) { setPhoneCheck({ state: 'error', message: 'Check unavailable, retry' }); return }
      const result: { phoneNumber: string; isClean: boolean; status: string } = await response.json()
      if (result.isClean) {
        setValues(current => ({ ...current, phoneNumber: result.phoneNumber }))
        setPhoneCheck({ state: 'clean', message: 'Clear' })
      } else {
        setPhoneCheck({ state: 'blocked', message: blockReason(result.status) })
      }
    } catch {
      setDncCheck(current => current?.state === 'clean' ? current : { state: 'error', message: 'Check unavailable, retry' })
      setPhoneCheck({ state: 'error', message: 'Check unavailable, retry' })
    }
  }

  const requestClose = () => {
    if (isSaving) return
    if (editor) setConfirm('discard')
    else closeModal()
  }

  const submitForm = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setConfirm('save')
  }

  const save = async () => {
    setConfirm(null)
    if (!editor) return
    setIsSaving(true)
    setFormError('')
    const editing = editor.lead
    try {
      const body = Object.fromEntries(leadFields.map(field =>
        [field.name, field.type === 'status' ? Number(values.statusId) : !field.required && !values[field.name].trim() ? null : values[field.name].trim()]))
      const response = await authenticatedFetch(`${apiUrl}/leads${editing ? `/${editing.id}` : ''}`, {
        method: editing ? 'PUT' : 'POST',
        headers: { Authorization: `Bearer ${getAccessToken()}`, 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
      })
      if (!response.ok) throw new Error(await responseError(response, 'Unable to save this lead.'))
      const saved: Lead = await response.json()
      setLeads(current => editing ? current.map(lead => lead.id === saved.id ? saved : lead) : [saved, ...current])
      setReload(current => current + 1)
      setEditor(null)
      showToast(editing ? 'Lead updated.' : 'Lead created.')
    } catch (saveError) {
      setFormError(saveError instanceof Error ? saveError.message : 'Unable to save this lead.')
    } finally {
      setIsSaving(false)
    }
  }

  const deleteLead = async () => {
    if (!deleting) return
    setIsSaving(true)
    setFormError('')
    try {
      const response = await authenticatedFetch(`${apiUrl}/leads/${deleting.id}`, {
        method: 'DELETE', headers: { Authorization: `Bearer ${getAccessToken()}` },
      })
      if (!response.ok) throw new Error(await responseError(response, 'Unable to delete this lead.'))
      setLeads(current => current.filter(lead => lead.id !== deleting.id))
      setReload(current => current + 1)
      setDeleting(null)
      showToast('Lead deleted.')
    } catch (deleteError) {
      setFormError(deleteError instanceof Error ? deleteError.message : 'Unable to delete this lead.')
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <div className="app-shell">
      <AppSidebar activePage="leads" isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />
      <main className="main-content">
        <header className="topbar">
          <button className="icon-button menu-button" onClick={() => toggleNavigation(() => setSidebarOpen(true))} aria-label="Toggle navigation"><Menu size={20} /></button>
          <div className="breadcrumb"><span>Workspace</span><span className="breadcrumb-slash">/</span><strong>Leads</strong></div>
        </header>
        <div className="page-content">
          <section className="welcome-row">
            <div><p className="eyebrow">{isAdmin ? 'All leads' : 'My leads'}</p><h1>Leads</h1><p className="welcome-copy">Manage contact details and insurance information.</p></div>
            <button className="primary-button" onClick={() => openEditor()}><Plus size={17} />Add lead</button>
          </section>
          {error && <div className="users-error" role="alert">{error} <button type="button" onClick={() => { setError(''); setIsLoading(true); setReload(value => value + 1) }}>Retry</button></div>}
          <section className="panel members-directory">
            <div className="directory-toolbar">
              <div><p className="panel-kicker">Lead directory</p><h2>{isAdmin ? 'All leads' : 'My leads'}{!isLoading && !error && <span>{leads.length} {submittedSearch ? 'found' : 'total'}</span>}</h2></div>
              <form className="directory-search" role="search" onSubmit={submitSearch}><Search size={16} /><input aria-label="Search leads" placeholder="Search leads..." value={search} onChange={event => setSearch(event.target.value)} /></form>
            </div>
            <div className="member-table-wrap">
              <table>
                <thead><tr><th>Name</th><th>Phone</th><th>Email</th><th>State</th><th>Status</th><th>Created</th><th>Actions</th></tr></thead>
                <tbody>
                  {isLoading ? <tr><td colSpan={7} className="empty-members">Loading leads...</td></tr>
                    : error ? <tr><td colSpan={7} className="empty-members">Lead data is unavailable.</td></tr>
                      : leads.length === 0 ? <tr><td colSpan={7} className="empty-members">{submittedSearch ? 'No matching leads.' : 'No leads yet. Add your first lead.'}</td></tr>
                        : leads.map(lead => (
                          <tr key={lead.id}>
                            <td><strong>{lead.firstName} {lead.lastName}</strong></td><td>{lead.phoneNumber}</td><td>{lead.email}</td><td>{lead.state}</td><td>{lead.statusName}</td><td>{formatDate(lead.createdOn)}</td>
                            <td className="user-row-actions">
                              <button className="row-menu" aria-label={`View lead ${lead.id}`} onClick={() => setDetails(lead)}><Eye size={16} /></button>
                              <button className="row-menu" aria-label={`Edit lead ${lead.id}`} onClick={() => openEditor(lead)}><Pencil size={16} /></button>
                              <button className="row-menu" aria-label={`Delete lead ${lead.id}`} onClick={() => { setFormError(''); setDeleting(lead) }}><Trash2 size={16} /></button>
                            </td>
                          </tr>
                        ))}
                </tbody>
              </table>
            </div>
          </section>
        </div>
      </main>
      <dialog ref={dialogRef} className="user-modal lead-dialog" aria-labelledby="lead-dialog-title" onCancel={event => { event.preventDefault(); requestClose() }}>
        <div className="user-modal-heading">
          <h2 id="lead-dialog-title">{editor ? editor.lead ? 'Edit lead' : 'Add lead' : deleting ? 'Delete lead' : 'Lead details'}</h2>
          {details && <button className="icon-button" aria-label="Close dialog" onClick={requestClose}><X size={20} /></button>}
        </div>
        {formError && <p className="users-error" role="alert">{formError}</p>}
        {editor && (
          <form onSubmit={submitForm}>
            {leadGroups.map(group => <fieldset className="lead-fieldset" key={group}><legend>{group}</legend>
              <div className="lead-fields">{leadFields.filter(field => field.group === group).map(field => (
                <label key={field.name} className={`lead-field-${field.name}`}><span>{field.label}{field.required ? ' *' : ''}</span>
                  {field.type === 'status' ? <select name={field.name} required value={values.statusId} disabled={isSaving} onChange={event => setValues(current => ({ ...current, statusId: event.target.value }))}>
                    <option value="">Select a status</option>
                    {statuses.map(status => <option key={status.id} value={status.id}>{status.name}</option>)}
                  </select> : field.type === 'state' ? <StateCombobox name={field.name} required={field.required} value={values.state} disabled={isSaving} onChange={code => setValues(current => ({ ...current, state: code }))} /> : <input name={field.name} type={field.type ?? 'text'} required={field.required} maxLength={field.type === 'date' ? undefined : field.maxLength ?? 50}
                    value={values[field.name]} disabled={isSaving} onChange={event => { if (field.name === 'phoneNumber') { setPhoneCheck(null); setDncCheck(null) } setValues(current => ({ ...current, [field.name]: event.target.value })) }}
                    onBlur={field.name === 'phoneNumber' ? () => void checkPhone() : undefined} />}
                  {field.name === 'phoneNumber' && (
                    <span className="lead-checks">
                      <span className="lead-tcpa">
                        <span className="lead-tcpa-title">TCPA check</span>
                        <small className={`lead-phone-check lead-phone-${phoneCheck?.state ?? 'idle'}`} role="status">{phoneCheck?.message ?? 'Not checked yet'}</small>
                      </span>
                      <span className="lead-tcpa">
                        <span className="lead-tcpa-title">DNC check</span>
                        <small className={`lead-phone-check lead-phone-${dncCheck?.state ?? 'idle'}`} role="status">{dncCheck?.message ?? 'Not checked yet'}</small>
                      </span>
                    </span>
                  )}
                </label>
              ))}</div>
            </fieldset>)}
            <div className="user-modal-actions"><button type="button" className="user-cancel-button" disabled={isSaving} onClick={requestClose}>Cancel</button><button className={`primary-button${phoneCheck?.state === 'blocked' || dncCheck?.state === 'blocked' ? ' save-blocked' : ''}`} disabled={isSaving || phoneCheck?.state === 'blocked' || dncCheck?.state === 'blocked'} title={dncCheck?.state === 'blocked' ? 'Phone number is on the DNC list' : phoneCheck?.state === 'blocked' ? 'Phone number is blocked by TCPA' : undefined}>{isSaving ? 'Saving...' : 'Save lead'}</button></div>
          </form>
        )}
        {details && <div className="lead-details">
          <p>Lead #{details.id} · Created {formatDate(details.createdOn)}</p>
          {leadGroups.map(group => <section key={group}><h3>{group}</h3><dl>{leadFields.filter(field => field.group === group).map(field => <div key={field.name}><dt>{field.label}</dt><dd>{field.type === 'status' ? details.statusName : details[field.name as keyof Lead] || 'Not provided'}</dd></div>)}</dl></section>)}
        </div>}
        {deleting && <div><p className="user-modal-description">Permanently delete the lead for <strong>{deleting.firstName} {deleting.lastName}</strong>? This cannot be undone.</p>
          <div className="user-modal-actions"><button className="user-cancel-button" disabled={isSaving} onClick={closeModal}>Cancel</button><button className="primary-button lead-delete-button" disabled={isSaving} onClick={() => void deleteLead()}>{isSaving ? 'Deleting...' : 'Delete permanently'}</button></div>
        </div>}
      </dialog>
      <dialog ref={confirmRef} className="user-modal confirm-dialog" aria-labelledby="confirm-title" onCancel={event => { event.preventDefault(); setConfirm(null) }}>
        <h2 id="confirm-title">{confirm === 'save' ? (editor?.lead ? 'Save changes?' : 'Create this lead?') : 'Discard changes?'}</h2>
        <p className="user-modal-description">{confirm === 'save' ? 'Please confirm that you want to save this lead.' : 'Your unsaved changes will be lost.'}</p>
        <div className="user-modal-actions">
          <button type="button" className="user-cancel-button" onClick={() => setConfirm(null)}>{confirm === 'save' ? 'Go back' : 'Keep editing'}</button>
          <button type="button" className={confirm === 'save' ? 'primary-button' : 'primary-button lead-delete-button'} onClick={() => { if (confirm === 'save') void save(); else { setConfirm(null); closeModal() } }}>{confirm === 'save' ? 'Yes, save' : 'Yes, discard'}</button>
        </div>
      </dialog>
    </div>
  )
}

export default LeadsPage
