import { useEffect, useMemo, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { Eye, Menu, Pencil, Plus, Search, Trash2, X } from 'lucide-react'
import AppSidebar from '../../Shared/AppSidebar'
import { apiUrl, authenticatedFetch, formatDate, getAccessToken, getCurrentUser } from '../../session'
import { leadFields, leadGroups, leadValues } from './leadFields'
import type { Lead, LeadValues } from './leadFields'
import './Leads.css'

async function responseError(response: Response, fallback: string) {
  if (response.headers.get('content-type')?.includes('json')) {
    const problem: { title?: string; detail?: string; errors?: Record<string, string[]> } = await response.json()
    return Object.values(problem.errors ?? {}).flat().join(' ') || problem.detail || problem.title || fallback
  }
  return `${fallback} (HTTP ${response.status})`
}

function LeadsPage() {
  const [leads, setLeads] = useState<Lead[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const [reload, setReload] = useState(0)
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const [editor, setEditor] = useState<{ lead?: Lead } | null>(null)
  const [details, setDetails] = useState<Lead | null>(null)
  const [deleting, setDeleting] = useState<Lead | null>(null)
  const [values, setValues] = useState<LeadValues>(() => leadValues())
  const [isSaving, setIsSaving] = useState(false)
  const [formError, setFormError] = useState('')
  const [notice, setNotice] = useState('')
  const dialogRef = useRef<HTMLDialogElement>(null)
  const user = getCurrentUser()
  const isAdmin = user?.roles.includes('Admin') ?? false
  const modalOpen = editor !== null || details !== null || deleting !== null

  useEffect(() => {
    if (modalOpen) dialogRef.current?.showModal()
    else dialogRef.current?.close()
  }, [modalOpen])

  useEffect(() => {
    let active = true
    const load = async () => {
      try {
        const response = await authenticatedFetch(`${apiUrl}/leads`, {
          headers: { Authorization: `Bearer ${getAccessToken()}` },
        })
        if (!response.ok) throw new Error(await responseError(response, 'Unable to load leads.'))
        const data: Lead[] = await response.json()
        if (active) setLeads(data)
      } catch (loadError) {
        if (active) setError(loadError instanceof Error ? loadError.message : 'Unable to load leads.')
      } finally {
        if (active) setIsLoading(false)
      }
    }
    void load()
    return () => { active = false }
  }, [reload])

  const filtered = useMemo(() => leads.filter(lead =>
    `${lead.firstName} ${lead.lastName} ${lead.email} ${lead.phoneNumber} ${lead.city ?? ''} ${lead.state}`
      .toLowerCase().includes(search.toLowerCase())), [leads, search])

  const openEditor = (lead?: Lead) => {
    setValues(leadValues(lead))
    setFormError('')
    setNotice('')
    setEditor({ lead })
  }

  const closeModal = () => {
    if (isSaving) return
    setEditor(null)
    setDetails(null)
    setDeleting(null)
    setFormError('')
  }

  const save = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!editor) return
    setIsSaving(true)
    setFormError('')
    const editing = editor.lead
    try {
      const body = Object.fromEntries(leadFields.map(field =>
        [field.name, !field.required && !values[field.name].trim() ? null : values[field.name].trim()]))
      const response = await authenticatedFetch(`${apiUrl}/leads${editing ? `/${editing.id}` : ''}`, {
        method: editing ? 'PUT' : 'POST',
        headers: { Authorization: `Bearer ${getAccessToken()}`, 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
      })
      if (!response.ok) throw new Error(await responseError(response, 'Unable to save this lead.'))
      const saved: Lead = await response.json()
      setLeads(current => editing ? current.map(lead => lead.id === saved.id ? saved : lead) : [saved, ...current])
      setEditor(null)
      setNotice(editing ? 'Lead updated.' : 'Lead created.')
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
      setDeleting(null)
      setNotice('Lead deleted.')
    } catch (deleteError) {
      setFormError(deleteError instanceof Error ? deleteError.message : 'Unable to delete this lead.')
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <div className="app-shell">
      <AppSidebar activePage="leads" isLoading={isLoading} isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />
      <main className="main-content">
        <header className="topbar">
          <button className="icon-button menu-button" onClick={() => setSidebarOpen(true)} aria-label="Open navigation"><Menu size={20} /></button>
          <div className="breadcrumb"><span>Workspace</span><span className="breadcrumb-slash">/</span><strong>Leads</strong></div>
        </header>
        <div className="page-content">
          <section className="welcome-row">
            <div><p className="eyebrow">{isAdmin ? 'All leads' : 'My leads'}</p><h1>Leads</h1><p className="welcome-copy">Manage contact details and insurance information.</p></div>
            <button className="primary-button" onClick={() => openEditor()}><Plus size={17} />Create lead</button>
          </section>
          {notice && <p className="lead-notice" role="status">{notice}</p>}
          {error && <div className="users-error" role="alert">{error} <button type="button" onClick={() => { setError(''); setIsLoading(true); setReload(value => value + 1) }}>Retry</button></div>}
          <section className="panel members-directory">
            <div className="directory-toolbar">
              <div><p className="panel-kicker">Lead directory</p><h2>{isAdmin ? 'All leads' : 'My leads'}{!isLoading && !error && <span>{leads.length} total</span>}</h2></div>
              <label className="directory-search"><Search size={16} /><input aria-label="Search leads" placeholder="Search leads..." value={search} onChange={event => setSearch(event.target.value)} /></label>
            </div>
            <div className="member-table-wrap">
              <table>
                <thead><tr><th>Name</th><th>Phone</th><th>Email</th><th>State</th><th>Created</th><th>Actions</th></tr></thead>
                <tbody>
                  {isLoading ? <tr><td colSpan={6} className="empty-members">Loading leads...</td></tr>
                    : error ? <tr><td colSpan={6} className="empty-members">Lead data is unavailable.</td></tr>
                      : filtered.length === 0 ? <tr><td colSpan={6} className="empty-members">{search ? 'No matching leads.' : 'No leads yet. Create your first lead.'}</td></tr>
                        : filtered.map(lead => (
                          <tr key={lead.id}>
                            <td><strong>{lead.firstName} {lead.lastName}</strong></td><td>{lead.phoneNumber}</td><td>{lead.email}</td><td>{lead.state}</td><td>{formatDate(lead.createdOn)}</td>
                            <td className="user-row-actions">
                              <button className="row-menu" aria-label={`View lead ${lead.id}`} onClick={() => setDetails(lead)}><Eye size={16} /></button>
                              <button className="row-menu" aria-label={`Edit lead ${lead.id}`} onClick={() => openEditor(lead)}><Pencil size={16} /></button>
                              <button className="row-menu" aria-label={`Delete lead ${lead.id}`} onClick={() => { setFormError(''); setNotice(''); setDeleting(lead) }}><Trash2 size={16} /></button>
                            </td>
                          </tr>
                        ))}
                </tbody>
              </table>
            </div>
          </section>
        </div>
      </main>
      <dialog ref={dialogRef} className="user-modal lead-dialog" aria-labelledby="lead-dialog-title" onCancel={event => { event.preventDefault(); closeModal() }}>
        <div className="user-modal-heading">
          <h2 id="lead-dialog-title">{editor ? editor.lead ? 'Edit lead' : 'Create lead' : deleting ? 'Delete lead' : 'Lead details'}</h2>
          <button className="icon-button" aria-label="Close dialog" disabled={isSaving} onClick={closeModal}><X size={20} /></button>
        </div>
        {formError && <p className="users-error" role="alert">{formError}</p>}
        {editor && (
          <form onSubmit={save}>
            {leadGroups.map(group => <fieldset className="lead-fieldset" key={group}><legend>{group}</legend>
              <div className="lead-fields">{leadFields.filter(field => field.group === group).map(field => (
                <label key={field.name}><span>{field.label}{field.required ? ' *' : ''}</span>
                  <input name={field.name} type={field.type ?? 'text'} required={field.required} maxLength={field.type === 'date' ? undefined : field.maxLength ?? 50}
                    value={values[field.name]} disabled={isSaving} onChange={event => setValues(current => ({ ...current, [field.name]: event.target.value }))} />
                </label>
              ))}</div>
            </fieldset>)}
            <div className="user-modal-actions"><button type="button" className="user-cancel-button" disabled={isSaving} onClick={closeModal}>Cancel</button><button className="primary-button" disabled={isSaving}>{isSaving ? 'Saving...' : 'Save lead'}</button></div>
          </form>
        )}
        {details && <div className="lead-details">
          <p>Lead #{details.id} · Created {formatDate(details.createdOn)}</p>
          {leadGroups.map(group => <section key={group}><h3>{group}</h3><dl>{leadFields.filter(field => field.group === group).map(field => <div key={field.name}><dt>{field.label}</dt><dd>{details[field.name] || 'Not provided'}</dd></div>)}</dl></section>)}
        </div>}
        {deleting && <div><p className="user-modal-description">Permanently delete the lead for <strong>{deleting.firstName} {deleting.lastName}</strong>? This cannot be undone.</p>
          <div className="user-modal-actions"><button className="user-cancel-button" disabled={isSaving} onClick={closeModal}>Cancel</button><button className="primary-button lead-delete-button" disabled={isSaving} onClick={() => void deleteLead()}>{isSaving ? 'Deleting...' : 'Delete permanently'}</button></div>
        </div>}
      </dialog>
    </div>
  )
}

export default LeadsPage
