import { useEffect, useMemo, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { Menu, Pencil, Plus, Search, Trash2, X } from 'lucide-react'
import AppSidebar from '../../Shared/AppSidebar'
import { toggleNavigation } from '../../Shared/sidebarToggle'
import { showToast } from '../../Shared/toast'
import { apiUrl, authenticatedFetch, getAccessToken } from '../../session'
import StateCombobox from '../Leads/StateCombobox'
import '../Leads/Leads.css'

type DncName = { id: number; firstName: string; lastName: string; phone: string | null; state: string | null }
type FormValues = { firstName: string; lastName: string; phone: string; state: string }

const emptyForm: FormValues = { firstName: '', lastName: '', phone: '', state: '' }

async function responseError(response: Response, fallback: string) {
  if (response.headers.get('content-type')?.includes('json')) {
    const problem: { title?: string; detail?: string; errors?: Record<string, string[]> } = await response.json()
    return Object.values(problem.errors ?? {}).flat().join(' ') || problem.detail || problem.title || fallback
  }
  return `${fallback} (HTTP ${response.status})`
}

function DncNamesPage() {
  const [entries, setEntries] = useState<DncName[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const [reload, setReload] = useState(0)
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const [editor, setEditor] = useState<{ entry?: DncName } | null>(null)
  const [deleting, setDeleting] = useState<DncName | null>(null)
  const [values, setValues] = useState<FormValues>(emptyForm)
  const [isSaving, setIsSaving] = useState(false)
  const [formError, setFormError] = useState('')
  const dialogRef = useRef<HTMLDialogElement>(null)
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
        const response = await authenticatedFetch(`${apiUrl}/dncnames`, { headers: authHeader() })
        if (!response.ok) throw new Error(await responseError(response, 'Unable to load DNC names.'))
        const data: DncName[] = await response.json()
        if (active) setEntries(data)
      } catch (loadError) {
        if (active) setError(loadError instanceof Error ? loadError.message : 'Unable to load DNC names.')
      } finally {
        if (active) setIsLoading(false)
      }
    }
    void load()
    return () => { active = false }
  }, [reload])

  const filtered = useMemo(() => {
    const term = search.trim().toLowerCase()
    return entries.filter(entry => [entry.firstName, entry.lastName, entry.phone, entry.state]
      .some(value => value?.toLowerCase().includes(term)))
  }, [entries, search])

  const openEditor = (entry?: DncName) => {
    setValues(entry ? { firstName: entry.firstName, lastName: entry.lastName, phone: entry.phone ?? '', state: entry.state ?? '' } : emptyForm)
    setFormError('')
    setEditor({ entry })
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
    const editing = editor.entry
    try {
      const response = await authenticatedFetch(`${apiUrl}/dncnames${editing ? `/${editing.id}` : ''}`, {
        method: editing ? 'PUT' : 'POST',
        headers: { ...authHeader(), 'Content-Type': 'application/json' },
        body: JSON.stringify({
          firstName: values.firstName.trim(),
          lastName: values.lastName.trim(),
          phone: values.phone.trim() || null,
          state: values.state || null,
        }),
      })
      if (!response.ok) throw new Error(await responseError(response, 'Unable to save this name.'))
      const saved: DncName = await response.json()
      setEntries(current => editing ? current.map(entry => entry.id === saved.id ? saved : entry) : [saved, ...current])
      setEditor(null)
      showToast(editing ? 'DNC name updated.' : 'DNC name added.')
    } catch (saveError) {
      setFormError(saveError instanceof Error ? saveError.message : 'Unable to save this name.')
    } finally {
      setIsSaving(false)
    }
  }

  const deleteEntry = async () => {
    if (!deleting) return
    setIsSaving(true)
    setFormError('')
    try {
      const response = await authenticatedFetch(`${apiUrl}/dncnames/${deleting.id}`, { method: 'DELETE', headers: authHeader() })
      if (!response.ok) throw new Error(await responseError(response, 'Unable to delete this name.'))
      setEntries(current => current.filter(entry => entry.id !== deleting.id))
      setDeleting(null)
      showToast('DNC name deleted.')
    } catch (deleteError) {
      setFormError(deleteError instanceof Error ? deleteError.message : 'Unable to delete this name.')
    } finally {
      setIsSaving(false)
    }
  }

  const setField = (name: keyof FormValues, value: string) => setValues(current => ({ ...current, [name]: value }))

  return (
    <div className="app-shell">
      <AppSidebar activePage="dncnames" isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />
      <main className="main-content">
        <header className="topbar">
          <button className="icon-button menu-button" onClick={() => toggleNavigation(() => setSidebarOpen(true))} aria-label="Toggle navigation"><Menu size={20} /></button>
          <div className="breadcrumb"><span>Workspace</span><span className="breadcrumb-slash">/</span><strong>DNC Names</strong></div>
        </header>
        <div className="page-content">
          <section className="welcome-row">
            <div><p className="eyebrow">Do not call</p><h1>DNC Names</h1><p className="welcome-copy">Names on the internal do-not-call list.</p></div>
            <button className="primary-button" onClick={() => openEditor()}><Plus size={17} />Add name</button>
          </section>
          {error && <div className="users-error" role="alert">{error} <button type="button" onClick={() => { setError(''); setIsLoading(true); setReload(value => value + 1) }}>Retry</button></div>}
          <section className="panel members-directory">
            <div className="directory-toolbar">
              <div><p className="panel-kicker">DNC directory</p><h2>All names{!isLoading && !error && <span>{entries.length} total</span>}</h2></div>
              <label className="directory-search"><Search size={16} /><input aria-label="Search DNC names" placeholder="Search DNC names..." value={search} onChange={event => setSearch(event.target.value)} /></label>
            </div>
            <div className="member-table-wrap">
              <table>
                <thead><tr><th>Name</th><th>Phone</th><th>State</th><th>Actions</th></tr></thead>
                <tbody>
                  {isLoading ? <tr><td colSpan={4} className="empty-members">Loading DNC names...</td></tr>
                    : error ? <tr><td colSpan={4} className="empty-members">DNC data is unavailable.</td></tr>
                      : filtered.length === 0 ? <tr><td colSpan={4} className="empty-members">{search ? 'No matching names.' : 'No DNC names yet.'}</td></tr>
                        : filtered.map(entry => (
                          <tr key={entry.id}>
                            <td><strong>{entry.firstName} {entry.lastName}</strong></td>
                            <td>{entry.phone ?? '-'}</td>
                            <td>{entry.state ?? '-'}</td>
                            <td className="user-row-actions">
                              <button className="row-menu" aria-label={`Edit ${entry.firstName} ${entry.lastName}`} onClick={() => openEditor(entry)}><Pencil size={16} /></button>
                              <button className="row-menu" aria-label={`Delete ${entry.firstName} ${entry.lastName}`} onClick={() => { setFormError(''); setDeleting(entry) }}><Trash2 size={16} /></button>
                            </td>
                          </tr>
                        ))}
                </tbody>
              </table>
            </div>
          </section>
        </div>
      </main>
      <dialog ref={dialogRef} className="user-modal" aria-labelledby="dnc-name-dialog-title" onCancel={event => { event.preventDefault(); closeModal() }}>
        <div className="user-modal-heading">
          <h2 id="dnc-name-dialog-title">{editor ? editor.entry ? 'Edit DNC name' : 'Add DNC name' : 'Delete DNC name'}</h2>
          <button className="icon-button" aria-label="Close dialog" disabled={isSaving} onClick={closeModal}><X size={20} /></button>
        </div>
        {formError && <p className="users-error" role="alert">{formError}</p>}
        {editor && (
          <form onSubmit={save}>
            <label><span>First name *</span><input name="firstName" required maxLength={50} value={values.firstName} disabled={isSaving} onChange={event => setField('firstName', event.target.value)} /></label>
            <label><span>Last name *</span><input name="lastName" required maxLength={50} value={values.lastName} disabled={isSaving} onChange={event => setField('lastName', event.target.value)} /></label>
            <label><span>Phone number</span><input name="phone" maxLength={50} value={values.phone} disabled={isSaving} onChange={event => setField('phone', event.target.value)} /></label>
            <label><span>State</span><StateCombobox name="state" required={false} value={values.state} disabled={isSaving} onChange={code => setField('state', code)} /></label>
            <div className="user-modal-actions"><button type="button" className="user-cancel-button" disabled={isSaving} onClick={closeModal}>Cancel</button><button className="primary-button" disabled={isSaving}>{isSaving ? 'Saving...' : 'Save name'}</button></div>
          </form>
        )}
        {deleting && <div><p className="user-modal-description">Remove <strong>{deleting.firstName} {deleting.lastName}</strong> from the DNC list? This cannot be undone.</p>
          <div className="user-modal-actions"><button className="user-cancel-button" disabled={isSaving} onClick={closeModal}>Cancel</button><button className="primary-button lead-delete-button" disabled={isSaving} onClick={() => void deleteEntry()}>{isSaving ? 'Deleting...' : 'Delete permanently'}</button></div>
        </div>}
      </dialog>
    </div>
  )
}

export default DncNamesPage
