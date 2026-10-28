import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { ArrowUpRight, Check, CheckCircle2, Edit2, Eye, EyeOff, KeyRound, Menu, Plus, Search, Users, X } from 'lucide-react'
import { apiUrl, authenticatedFetch, formatDate, getAccessToken, getCurrentUser } from '../../session'
import type { DirectoryUser } from '../../session'
import AppSidebar from '../../Shared/AppSidebar'

type ApiError = {
  message?: string
  errors?: Record<string, string[]>
}

function getErrorMessage(error: ApiError, fallback: string) {
  return Object.values(error.errors ?? {}).flat().join(' ') || error.message || fallback
}

function UsersPage() {
  const [members, setMembers] = useState<DirectoryUser[]>([])
  const [searchTerm, setSearchTerm] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState('')
  const [isCreateOpen, setIsCreateOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [createError, setCreateError] = useState('')
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [showInitialPassword, setShowInitialPassword] = useState(false)
  const [passwordUser, setPasswordUser] = useState<DirectoryUser | null>(null)
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [showNewPassword, setShowNewPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)
  const [passwordError, setPasswordError] = useState('')
  const [isChangingPassword, setIsChangingPassword] = useState(false)
  const [editingUser, setEditingUser] = useState<DirectoryUser | null>(null)
  const [editFirstName, setEditFirstName] = useState('')
  const [editLastName, setEditLastName] = useState('')
  const [editError, setEditError] = useState('')
  const [isSavingEdit, setIsSavingEdit] = useState(false)
  const [toastMessage, setToastMessage] = useState('')
  const [sidebarOpen, setSidebarOpen] = useState(() => sessionStorage.getItem('sidebar-open') === 'true')
  const currentUser = getCurrentUser()
  const initials = currentUser
    ? `${currentUser.firstName[0] ?? ''}${currentUser.lastName[0] ?? ''}`.toUpperCase()
    : '?'
  const closeSidebar = () => {
    setSidebarOpen(false)
    sessionStorage.removeItem('sidebar-open')
  }
  const openSidebar = () => {
    setSidebarOpen(true)
    sessionStorage.setItem('sidebar-open', 'true')
  }

  useEffect(() => {
    if (!toastMessage) return
    const timeoutId = window.setTimeout(() => setToastMessage(''), 3500)
    return () => window.clearTimeout(timeoutId)
  }, [toastMessage])

  useEffect(() => {
    const loadMembers = async () => {
      try {
        const response = await authenticatedFetch(`${apiUrl}/users`, {
          headers: { Authorization: `Bearer ${getAccessToken()}` },
        })
        if (!response.ok) {
          const error: ApiError = await response.json()
          throw new Error(getErrorMessage(error, 'Unable to load users.'))
        }
        setMembers(await response.json() as DirectoryUser[])
      } catch (error) {
        setLoadError(error instanceof Error ? error.message : 'Unable to load users.')
      } finally {
        setIsLoading(false)
      }
    }

    void loadMembers()
  }, [])

  const filteredMembers = useMemo(
    () => members.filter((member) =>
      `${member.firstName} ${member.lastName} ${member.email ?? ''}`.toLowerCase().includes(searchTerm.toLowerCase())),
    [members, searchTerm],
  )
  const membersThisMonth = members.filter((member) => {
    const createdOn = new Date(member.createdOn)
    const now = new Date()
    return createdOn.getMonth() === now.getMonth() && createdOn.getFullYear() === now.getFullYear()
  }).length

  const closeCreateForm = () => {
    setIsCreateOpen(false)
    setCreateError('')
    setFirstName('')
    setLastName('')
    setEmail('')
    setPassword('')
    setShowInitialPassword(false)
  }

  const closePasswordForm = () => {
    setPasswordUser(null)
    setNewPassword('')
    setConfirmPassword('')
    setShowNewPassword(false)
    setShowConfirmPassword(false)
    setPasswordError('')
  }

  const openEditForm = (member: DirectoryUser) => {
    setEditingUser(member)
    setEditFirstName(member.firstName)
    setEditLastName(member.lastName)
    setEditError('')
  }

  const closeEditForm = () => {
    setEditingUser(null)
    setEditFirstName('')
    setEditLastName('')
    setEditError('')
  }

  const createUser = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setCreateError('')
    setIsSaving(true)

    try {
      const response = await authenticatedFetch(`${apiUrl}/users`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${getAccessToken()}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ firstName, lastName, email, password }),
      })

      if (!response.ok) {
        const error: ApiError = await response.json()
        throw new Error(getErrorMessage(error, 'Unable to create this user.'))
      }

      const createdUser: DirectoryUser = await response.json()
      setMembers((current) => [createdUser, ...current])
      closeCreateForm()
    } catch (error) {
      setCreateError(error instanceof Error ? error.message : 'Unable to create this user.')
    } finally {
      setIsSaving(false)
    }
  }

  const changeUserPassword = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!passwordUser) return

    if (newPassword !== confirmPassword) {
      setPasswordError('Passwords do not match.')
      return
    }

    setPasswordError('')
    setIsChangingPassword(true)
    try {
      const response = await authenticatedFetch(`${apiUrl}/users/${encodeURIComponent(passwordUser.id)}/password`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${getAccessToken()}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ newPassword }),
      })
      if (!response.ok) {
        const error: ApiError = await response.json()
        throw new Error(getErrorMessage(error, 'Unable to change this user password.'))
      }
      closePasswordForm()
    } catch (error) {
      setPasswordError(error instanceof Error ? error.message : 'Unable to change this user password.')
    } finally {
      setIsChangingPassword(false)
    }
  }

  const updateUserName = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!editingUser) return

    setEditError('')
    setIsSavingEdit(true)
    try {
      const response = await authenticatedFetch(`${apiUrl}/users/${encodeURIComponent(editingUser.id)}`, {
        method: 'PUT',
        headers: {
          Authorization: `Bearer ${getAccessToken()}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          firstName: editFirstName,
          lastName: editLastName,
          email: editingUser.email,
        }),
      })
      if (!response.ok) {
        const error: ApiError = await response.json()
        throw new Error(getErrorMessage(error, 'Unable to update this user.'))
      }

      const updatedUser: DirectoryUser = await response.json()
      setMembers((current) => current.map((member) => member.id === updatedUser.id ? updatedUser : member))
      setToastMessage(`Name updated for ${updatedUser.firstName} ${updatedUser.lastName}.`)
      closeEditForm()
    } catch (error) {
      setEditError(error instanceof Error ? error.message : 'Unable to update this user.')
    } finally {
      setIsSavingEdit(false)
    }
  }

  return (
    <div className="app-shell">
      <AppSidebar activePage="users" usersCount={members.length} isLoading={isLoading} isOpen={sidebarOpen} onClose={closeSidebar} />
      <main className="main-content users-page">
        <header className="topbar">
          <button className="icon-button menu-button" onClick={openSidebar} aria-label="Open navigation"><Menu size={20} /></button>
          <div className="breadcrumb"><span>Workspace</span><span className="breadcrumb-slash">/</span><strong>Users</strong></div>
          <div className="topbar-actions"><div className="top-profile">{initials}</div></div>
        </header>
        <div className="users-content">
        <section className="users-heading">
          <div>
            <p className="eyebrow">Workspace directory</p>
            <h1>Members</h1>
            <p className="welcome-copy">Manage user accounts from one place.</p>
          </div>
          <button className="primary-button" onClick={() => { setCreateError(''); setIsCreateOpen(true) }}>
            <Plus size={17} /> Create user
          </button>
        </section>
        <section className="user-stat-row">
          <div className="user-stat">
            <div className="user-stat-icon"><Users size={17} /></div>
            <div><span>Total users</span><strong>{members.length}</strong></div>
          </div>
          <div className="user-stat">
            <div className="user-stat-icon user-stat-green"><Check size={17} /></div>
            <div><span>Created this month</span><strong>{membersThisMonth}</strong></div>
          </div>
        </section>
        <section className="panel members-directory">
          <div className="directory-toolbar">
            <div><p className="panel-kicker">All users</p><h2>Workspace directory <span>{members.length} total</span></h2></div>
            <label className="directory-search">
              <Search size={16} />
              <input value={searchTerm} onChange={(event) => setSearchTerm(event.target.value)} placeholder="Search users..." aria-label="Search users" />
            </label>
          </div>
          {loadError && <div className="users-error" role="alert">{loadError}</div>}
          <div className="member-table-wrap">
            <table>
              <thead><tr><th>User</th><th>Email</th><th>Created</th><th>Actions</th></tr></thead>
              <tbody>
                {filteredMembers.map((member) => {
                  const name = `${member.firstName} ${member.lastName}`
                  const initials = `${member.firstName[0] ?? ''}${member.lastName[0] ?? ''}`.toUpperCase()
                  return (
                    <tr key={member.id}>
                      <td><div className="member-cell"><div className="member-avatar lilac">{initials}</div><div><strong>{name}</strong></div></div></td>
                      <td>{member.email}</td>
                      <td>{formatDate(member.createdOn)}</td>
                      <td className="user-row-actions">
                        <button className="row-menu" aria-label={`Edit name for ${name}`} onClick={() => openEditForm(member)}><Edit2 size={16} /></button>
                        <button className="row-menu" aria-label={`Change password for ${name}`} onClick={() => { setPasswordError(''); setPasswordUser(member) }}><KeyRound size={16} /></button>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
            {isLoading && <div className="empty-members">Loading users...</div>}
            {!isLoading && !loadError && filteredMembers.length === 0 && (
              <div className="empty-members">{searchTerm ? `No users found for “${searchTerm}”.` : 'No users yet.'}</div>
            )}
          </div>
        </section>
        <footer className="footer-note"><span><a href="/">Back to dashboard <ArrowUpRight size={13} /></a></span></footer>
        </div>

      {isCreateOpen && (
        <div className="user-modal-backdrop">
          <section className="user-modal" role="dialog" aria-modal="true" aria-labelledby="create-user-title">
            <div className="user-modal-heading">
              <div><p className="panel-kicker">Workspace directory</p><h2 id="create-user-title">Create user</h2></div>
              <button className="icon-button" type="button" onClick={closeCreateForm} aria-label="Close form"><X size={19} /></button>
            </div>
            <p className="user-modal-description">Create a standard account. The user can sign in with the email and password you set.</p>
            <form onSubmit={createUser}>
              <div className="auth-name-row">
                <label><span>First name</span><input required maxLength={100} value={firstName} onChange={(event) => setFirstName(event.target.value)} autoFocus /></label>
                <label><span>Last name</span><input required maxLength={100} value={lastName} onChange={(event) => setLastName(event.target.value)} /></label>
              </div>
              <label><span>Email address</span><input required type="email" maxLength={256} value={email} onChange={(event) => setEmail(event.target.value)} /></label>
              <label><span>Initial password</span><div className="user-password-field"><input required type={showInitialPassword ? 'text' : 'password'} minLength={8} maxLength={100} value={password} onChange={(event) => setPassword(event.target.value)} autoComplete="new-password" /><button className="password-toggle" type="button" onClick={() => setShowInitialPassword((visible) => !visible)} aria-label={showInitialPassword ? 'Hide initial password' : 'Show initial password'}>{showInitialPassword ? <EyeOff size={16} /> : <Eye size={16} />}</button></div></label>
              {createError && <div className="auth-error" role="alert">{createError}</div>}
              <div className="user-modal-actions">
                <button className="user-cancel-button" type="button" onClick={closeCreateForm} disabled={isSaving}>Cancel</button>
                <button className="primary-button" type="submit" disabled={isSaving}>{isSaving ? 'Creating...' : 'Create user'}</button>
              </div>
            </form>
          </section>
        </div>
      )}
      {passwordUser && (
        <div className="user-modal-backdrop">
          <section className="user-modal" role="dialog" aria-modal="true" aria-labelledby="change-password-title">
            <div className="user-modal-heading">
              <div><p className="panel-kicker">User account</p><h2 id="change-password-title">Change password</h2></div>
              <button className="icon-button" type="button" onClick={closePasswordForm} aria-label="Close form"><X size={19} /></button>
            </div>
            <p className="user-modal-description">Set a new password for {passwordUser.firstName} {passwordUser.lastName}.</p>
            <form onSubmit={changeUserPassword}>
              <label><span>New password</span><div className="user-password-field"><input required type={showNewPassword ? 'text' : 'password'} minLength={8} maxLength={100} value={newPassword} onChange={(event) => setNewPassword(event.target.value)} autoComplete="new-password" autoFocus /><button className="password-toggle" type="button" onClick={() => setShowNewPassword((visible) => !visible)} aria-label={showNewPassword ? 'Hide new password' : 'Show new password'}>{showNewPassword ? <EyeOff size={16} /> : <Eye size={16} />}</button></div></label>
              <label><span>Confirm new password</span><div className="user-password-field"><input required type={showConfirmPassword ? 'text' : 'password'} minLength={8} maxLength={100} value={confirmPassword} onChange={(event) => setConfirmPassword(event.target.value)} autoComplete="new-password" /><button className="password-toggle" type="button" onClick={() => setShowConfirmPassword((visible) => !visible)} aria-label={showConfirmPassword ? 'Hide confirmed password' : 'Show confirmed password'}>{showConfirmPassword ? <EyeOff size={16} /> : <Eye size={16} />}</button></div></label>
              {passwordError && <div className="auth-error" role="alert">{passwordError}</div>}
              <div className="user-modal-actions">
                <button className="user-cancel-button" type="button" onClick={closePasswordForm} disabled={isChangingPassword}>Cancel</button>
                <button className="primary-button" type="submit" disabled={isChangingPassword}>{isChangingPassword ? 'Saving...' : 'Change password'}</button>
              </div>
            </form>
          </section>
        </div>
      )}
      {editingUser && (
        <div className="user-modal-backdrop">
          <section className="user-modal" role="dialog" aria-modal="true" aria-labelledby="edit-user-title">
            <div className="user-modal-heading">
              <div><p className="panel-kicker">User account</p><h2 id="edit-user-title">Edit name</h2></div>
              <button className="icon-button" type="button" onClick={closeEditForm} aria-label="Close form"><X size={19} /></button>
            </div>
            <p className="user-modal-description">Update the name for {editingUser.email}. Email address stays the same.</p>
            <form onSubmit={updateUserName}>
              <div className="auth-name-row">
                <label><span>First name</span><input required maxLength={100} value={editFirstName} onChange={(event) => setEditFirstName(event.target.value)} autoFocus /></label>
                <label><span>Last name</span><input required maxLength={100} value={editLastName} onChange={(event) => setEditLastName(event.target.value)} /></label>
              </div>
              {editError && <div className="auth-error" role="alert">{editError}</div>}
              <div className="user-modal-actions">
                <button className="user-cancel-button" type="button" onClick={closeEditForm} disabled={isSavingEdit}>Cancel</button>
                <button className="primary-button" type="submit" disabled={isSavingEdit}>{isSavingEdit ? 'Saving...' : 'Save name'}</button>
              </div>
            </form>
          </section>
        </div>
      )}
      {toastMessage && (
        <div className="success-toast" role="status" aria-live="polite">
          <CheckCircle2 size={18} />
          <span>{toastMessage}</span>
          <button type="button" onClick={() => setToastMessage('')} aria-label="Dismiss notification"><X size={16} /></button>
        </div>
      )}
      </main>
    </div>
  )
}

export default UsersPage
