import { useEffect, useMemo, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { Menu, Pencil, Plus, Search, Trash2, X } from 'lucide-react'
import AppSidebar from '../../Shared/AppSidebar'
import { apiUrl, authenticatedFetch, formatDate, getAccessToken, getCurrentUser } from '../../session'
import '../Leads/Leads.css'

type LeadStatus = { id: number; name: string; createdOn: string }

async function responseError(response: Response, fallback: string) {
  if (response.headers.get('content-type')?.includes('json')) {
    const problem: { title?: string; detail?: string; errors?: Record<string, string[]> } = await response.json()
    return Object.values(problem.errors ?? {}).flat().join(' ') || problem.detail || problem.title || fallback
  }
  return `${fallback} (HTTP ${response.status})`
}

function LeadStatusesPage() {
  const [statuses, setStatuses] = useState<LeadStatus[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const [reload, setReload] = useState(0)
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const [editor, setEditor] = useState<{ status?: LeadStatus } | null>(null)
  const [deleting, setDeleting] = useState<LeadStatus | null>(null)
  const [name, setName] = useState('')
  const [isSaving, setIsSaving] = useState(false)
  const [formError, setFormError] = useState('')
  const [notice, setNotice] = useState('')
  const dialogRef = useRef<HTMLDialogElement>(null)
  const isAdmin = getCurrentUser()?.roles.includes('Admin') ?? false
  const modalOpen = editor !== null || deleting !== null
  const authHeader = () => ({ Authorization: `Bearer ${getAccessToken()}` })

  useEffect(() => {
    if (modalOpen) dialogRef.current?.showModal()
    else dialogRef.current?.close()
  }, [modalOpen])

  useEffect(() => {
    let active = true
    const load = async () => {
      try {
        const response = await authenticatedFetch(`${apiUrl}/leadstatuses`, { headers: authHeader() })
        if (!response.ok) throw new Error(await responseError(response, 'Unable to load statuses.'))
        const data: LeadStatus[] = await response.json()
        if (active) setStatuses(data)
      } catch (loadError) {
        if (active) setError(loadError instanceof Error ? loadError.message : 'Unable to load statuses.')
      } finally {
        if (active) setIsLoading(false)
      }
    }
    void load()
    return () => { active = false }
  }, [reload])

  const filtered = useMemo(() => statuses.filter(status =>
    status.name.toLowerCase().includes(search.toLowerCase())), [statuses, search])

  const openEditor = (status?: LeadStatus) => {
    setName(status?.name ?? '')
    setFormError('')
    setNotice('')
    setEditor({ status })
  }

  const closeModal = () => {
    if (isSaving) return
    setEditor(null)
    setDeleting(null)
    setFormError('')
  }

  const save = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!editor) return
    setIsSaving(true)
    setFormError('')
    const editing = editor.status
    try {
      const response = await authenticatedFetch(`${apiUrl}/leadstatuses${editing ? `/${editing.id}` : ''}`, {
        method: editing ? 'PUT' : 'POST',
        headers: { ...authHeader(), 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: name.trim() }),
      })
      if (!response.ok) throw new Error(await responseError(response, 'Unable to save this status.'))
      const saved: LeadStatus = await response.json()
      setStatuses(current => (editing ? current.map(status => status.id === saved.id ? saved : status) : [...current, saved])
        .sort((a, b) => a.name.localeCompare(b.name)))
      setEditor(null)
      setNotice(editing ? 'Status updated.' : 'Status created.')
    } catch (saveError) {
      setFormError(saveError instanceof Error ? saveError.message : 'Unable to save this status.')
    } finally {
      setIsSaving(false)
    }
  }

  const deleteStatus = async () => {
    if (!deleting) return
    setIsSaving(true)
    setFormError('')
    try {
      const response = await authenticatedFetch(`${apiUrl}/leadstatuses/${deleting.id}`, { method: 'DELETE', headers: authHeader() })
      if (!response.ok) throw new Error(await responseError(response, 'Unable to delete this status.'))
      setStatuses(current => current.filter(status => status.id !== deleting.id))
      setDeleting(null)
      setNotice('Status deleted.')
    } catch (deleteError) {
      setFormError(deleteError instanceof Error ? deleteError.message : 'Unable to delete this status.')
    } finally {
      setIsSaving(false)
    }
  }

  const columns = isAdmin ? 3 : 2

  return (
    <div className="app-shell">
      <AppSidebar activePage="leadstatuses" isLoading={isLoading} isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />
      <main className="main-content">
        <header className="topbar">
          <button className="icon-button menu-button" onClick={() => setSidebarOpen(true)} aria-label="Open navigation"><Menu size={20} /></button>
          <div className="breadcrumb"><span>Workspace</span><span className="breadcrumb-slash">/</span><strong>Lead statuses</strong></div>
        </header>
        <div className="page-content">
          <section className="welcome-row">
            <div><p className="eyebrow">Lead statuses</p><h1>Lead statuses</h1><p className="welcome-copy">{isAdmin ? 'Manage the statuses available for leads.' : 'Statuses available for leads.'}</p></div>
            {isAdmin && <button className="primary-button" onClick={() => openEditor()}><Plus size={17} />Create status</button>}
          </section>
          {notice && <p className="lead-notice" role="status">{notice}</p>}
          {error && <div className="users-error" role="alert">{error} <button type="button" onClick={() => { setError(''); setIsLoading(true); setReload(value => value + 1) }}>Retry</button></div>}
          <section className="panel members-directory">
            <div className="directory-toolbar">
              <div><p className="panel-kicker">Status directory</p><h2>All statuses{!isLoading && !error && <span>{statuses.length} total</span>}</h2></div>
              <label className="directory-search"><Search size={16} /><input aria-label="Search statuses" placeholder="Search statuses..." value={search} onChange={event => setSearch(event.target.value)} /></label>
            </div>
            <div className="member-table-wrap">
              <table>
                <thead><tr><th>Name</th><th>Created</th>{isAdmin && <th>Actions</th>}</tr></thead>
                <tbody>
                  {isLoading ? <tr><td colSpan={columns} className="empty-members">Loading statuses...</td></tr>
                    : error ? <tr><td colSpan={columns} className="empty-members">Status data is unavailable.</td></tr>
                      : filtered.length === 0 ? <tr><td colSpan={columns} className="empty-members">{search ? 'No matching statuses.' : 'No statuses yet.'}</td></tr>
                        : filtered.map(status => (
                          <tr key={status.id}>
                            <td><strong>{status.name}</strong></td><td>{formatDate(status.createdOn)}</td>
                            {isAdmin && <td className="user-row-actions">
                              <button className="row-menu" aria-label={`Edit status ${status.name}`} onClick={() => openEditor(status)}><Pencil size={16} /></button>
                              <button className="row-menu" aria-label={`Delete status ${status.name}`} onClick={() => { setFormError(''); setNotice(''); setDeleting(status) }}><Trash2 size={16} /></button>
                            </td>}
                          </tr>
                        ))}
                </tbody>
              </table>
            </div>
          </section>
        </div>
      </main>
      <dialog ref={dialogRef} className="user-modal" aria-labelledby="status-dialog-title" onCancel={event => { event.preventDefault(); closeModal() }}>
        <div className="user-modal-heading">
          <h2 id="status-dialog-title">{editor ? editor.status ? 'Edit status' : 'Create status' : 'Delete status'}</h2>
          <button className="icon-button" aria-label="Close dialog" disabled={isSaving} onClick={closeModal}><X size={20} /></button>
        </div>
        {formError && <p className="users-error" role="alert">{formError}</p>}
        {editor && (
          <form onSubmit={save}>
            <label><span>Name *</span><input name="name" required maxLength={50} value={name} disabled={isSaving} onChange={event => setName(event.target.value)} /></label>
            <div className="user-modal-actions"><button type="button" className="user-cancel-button" disabled={isSaving} onClick={closeModal}>Cancel</button><button className="primary-button" disabled={isSaving}>{isSaving ? 'Saving...' : 'Save status'}</button></div>
          </form>
        )}
        {deleting && <div><p className="user-modal-description">Permanently delete the status <strong>{deleting.name}</strong>? This cannot be undone.</p>
          <div className="user-modal-actions"><button className="user-cancel-button" disabled={isSaving} onClick={closeModal}>Cancel</button><button className="primary-button lead-delete-button" disabled={isSaving} onClick={() => void deleteStatus()}>{isSaving ? 'Deleting...' : 'Delete permanently'}</button></div>
        </div>}
      </dialog>
    </div>
  )
}

export default LeadStatusesPage
