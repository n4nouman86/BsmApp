import { useEffect, useMemo, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { Menu, Pencil, Plus, Search, Trash2, X } from 'lucide-react'
import AppSidebar from '../../Shared/AppSidebar'
import { toggleNavigation } from '../../Shared/sidebarToggle'
import { showToast } from '../../Shared/toast'
import { apiUrl, authenticatedFetch, formatDate, getAccessToken } from '../../session'
import StateCombobox from '../Leads/StateCombobox'
import '../Leads/Leads.css'

type DncNumber = { id: number; phone: string; firstName: string | null; lastName: string | null; state: string | null; createdOn: string }
type FormValues = { phone: string; firstName: string; lastName: string; state: string }

const emptyForm: FormValues = { phone: '', firstName: '', lastName: '', state: '' }

async function responseError(response: Response, fallback: string) {
  if (response.headers.get('content-type')?.includes('json')) {
    const problem: { title?: string; detail?: string; errors?: Record<string, string[]> } = await response.json()
    return Object.values(problem.errors ?? {}).flat().join(' ') || problem.detail || problem.title || fallback
  }
  return `${fallback} (HTTP ${response.status})`
}

function DncNumbersPage() {
  const [entries, setEntries] = useState<DncNumber[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const [reload, setReload] = useState(0)
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const [editor, setEditor] = useState<{ entry?: DncNumber } | null>(null)
  const [deleting, setDeleting] = useState<DncNumber | null>(null)
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
        const response = await authenticatedFetch(`${apiUrl}/dncnumbers`, { headers: authHeader() })
        if (!response.ok) throw new Error(await responseError(response, 'Unable to load DNC phones.'))
        const data: DncNumber[] = await response.json()
        if (active) setEntries(data)
      } catch (loadError) {
        if (active) setError(loadError instanceof Error ? loadError.message : 'Unable to load DNC phones.')
      } finally {
        if (active) setIsLoading(false)
      }
    }
    void load()
    return () => { active = false }
  }, [reload])

  const filtered = useMemo(() => {
    const term = search.trim().toLowerCase()
    return entries.filter(entry => [entry.phone, entry.firstName, entry.lastName, entry.state]
      .some(value => value?.toLowerCase().includes(term)))
  }, [entries, search])

  const openEditor = (entry?: DncNumber) => {
    setValues(entry
      ? { phone: entry.phone, firstName: entry.firstName ?? '', lastName: entry.lastName ?? '', state: entry.state ?? '' }
      : emptyForm)
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
      const response = await authenticatedFetch(`${apiUrl}/dncnumbers${editing ? `/${editing.id}` : ''}`, {
        method: editing ? 'PUT' : 'POST',
        headers: { ...authHeader(), 'Content-Type': 'application/json' },
        body: JSON.stringify({
          phone: values.phone.trim(),
          firstName: values.firstName.trim() || null,
          lastName: values.lastName.trim() || null,
          state: values.state || null,
        }),
      })
      if (!response.ok) throw new Error(await responseError(response, 'Unable to save this number.'))
      const saved: DncNumber = await response.json()
      setEntries(current => editing ? current.map(entry => entry.id === saved.id ? saved : entry) : [saved, ...current])
      setEditor(null)
      showToast(editing ? 'DNC number updated.' : 'DNC number added.')
    } catch (saveError) {
      setFormError(saveError instanceof Error ? saveError.message : 'Unable to save this number.')
    } finally {
      setIsSaving(false)
    }
  }

  const deleteEntry = async () => {
    if (!deleting) return
    setIsSaving(true)
    setFormError('')
    try {
      const response = await authenticatedFetch(`${apiUrl}/dncnumbers/${deleting.id}`, { method: 'DELETE', headers: authHeader() })
      if (!response.ok) throw new Error(await responseError(response, 'Unable to delete this number.'))
      setEntries(current => current.filter(entry => entry.id !== deleting.id))
      setDeleting(null)
      showToast('DNC number deleted.')
    } catch (deleteError) {
      setFormError(deleteError instanceof Error ? deleteError.message : 'Unable to delete this number.')
    } finally {
      setIsSaving(false)
    }
  }

  const setField = (name: keyof FormValues, value: string) => setValues(current => ({ ...current, [name]: value }))

  return (
    <div className="app-shell">
      <AppSidebar activePage="dnc" isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />
      <main className="main-content">
        <header className="topbar">
          <button className="icon-button menu-button" onClick={() => toggleNavigation(() => setSidebarOpen(true))} aria-label="Toggle navigation"><Menu size={20} /></button>
          <div className="breadcrumb"><span>Workspace</span><span className="breadcrumb-slash">/</span><strong>DNC Numbers</strong></div>
        </header>
        <div className="page-content">
          <section className="welcome-row">
            <div><p className="eyebrow">Do not call</p><h1>DNC Numbers</h1><p className="welcome-copy">Numbers on this list are blocked when creating or editing leads.</p></div>
            <button className="primary-button" onClick={() => openEditor()}><Plus size={17} />Add number</button>
          </section>
          {error && <div className="users-error" role="alert">{error} <button type="button" onClick={() => { setError(''); setIsLoading(true); setReload(value => value + 1) }}>Retry</button></div>}
          <section className="panel members-directory">
            <div className="directory-toolbar">
              <div><p className="panel-kicker">DNC directory</p><h2>All numbers{!isLoading && !error && <span>{entries.length} total</span>}</h2></div>
              <label className="directory-search"><Search size={16} /><input aria-label="Search DNC numbers" placeholder="Search DNC numbers..." value={search} onChange={event => setSearch(event.target.value)} /></label>
            </div>
            <div className="member-table-wrap">
              <table>
                <thead><tr><th>Phone</th><th>Name</th><th>State</th><th>Created</th><th>Actions</th></tr></thead>
                <tbody>
                  {isLoading ? <tr><td colSpan={5} className="empty-members">Loading DNC numbers...</td></tr>
                    : error ? <tr><td colSpan={5} className="empty-members">DNC data is unavailable.</td></tr>
                      : filtered.length === 0 ? <tr><td colSpan={5} className="empty-members">{search ? 'No matching numbers.' : 'No DNC numbers yet.'}</td></tr>
                        : filtered.map(entry => (
                          <tr key={entry.id}>
                            <td><strong>{entry.phone}</strong></td>
                            <td>{[entry.firstName, entry.lastName].filter(Boolean).join(' ') || '-'}</td>
                            <td>{entry.state ?? '-'}</td>
                            <td>{formatDate(entry.createdOn)}</td>
                            <td className="user-row-actions">
                              <button className="row-menu" aria-label={`Edit ${entry.phone}`} onClick={() => openEditor(entry)}><Pencil size={16} /></button>
                              <button className="row-menu" aria-label={`Delete ${entry.phone}`} onClick={() => { setFormError(''); setDeleting(entry) }}><Trash2 size={16} /></button>
                            </td>
                          </tr>
                        ))}
                </tbody>
              </table>
            </div>
          </section>
        </div>
      </main>
      <dialog ref={dialogRef} className="user-modal" aria-labelledby="dnc-dialog-title" onCancel={event => { event.preventDefault(); closeModal() }}>
        <div className="user-modal-heading">
          <h2 id="dnc-dialog-title">{editor ? editor.entry ? 'Edit DNC number' : 'Add DNC number' : 'Delete DNC number'}</h2>
          <button className="icon-button" aria-label="Close dialog" disabled={isSaving} onClick={closeModal}><X size={20} /></button>
        </div>
        {formError && <p className="users-error" role="alert">{formError}</p>}
        {editor && (
          <form onSubmit={save}>
            <label><span>Phone number *</span><input name="phone" required maxLength={50} value={values.phone} disabled={isSaving} onChange={event => setField('phone', event.target.value)} /></label>
            <label><span>First name</span><input name="firstName" maxLength={50} value={values.firstName} disabled={isSaving} onChange={event => setField('firstName', event.target.value)} /></label>
            <label><span>Last name</span><input name="lastName" maxLength={50} value={values.lastName} disabled={isSaving} onChange={event => setField('lastName', event.target.value)} /></label>
            <label><span>State</span><StateCombobox name="state" required={false} value={values.state} disabled={isSaving} onChange={code => setField('state', code)} /></label>
            <div className="user-modal-actions"><button type="button" className="user-cancel-button" disabled={isSaving} onClick={closeModal}>Cancel</button><button className="primary-button" disabled={isSaving}>{isSaving ? 'Saving...' : 'Save number'}</button></div>
          </form>
        )}
        {deleting && <div><p className="user-modal-description">Remove <strong>{deleting.phone}</strong> from the DNC list? This cannot be undone.</p>
          <div className="user-modal-actions"><button className="user-cancel-button" disabled={isSaving} onClick={closeModal}>Cancel</button><button className="primary-button lead-delete-button" disabled={isSaving} onClick={() => void deleteEntry()}>{isSaving ? 'Deleting...' : 'Delete permanently'}</button></div>
        </div>}
      </dialog>
    </div>
  )
}

export default DncNumbersPage
