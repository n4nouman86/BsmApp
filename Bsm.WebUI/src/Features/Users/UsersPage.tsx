import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { ArrowUpRight, Ban, Check, CheckCircle2, Edit2, Eye, EyeOff, KeyRound, Menu, Plus, Search, ShieldCheck, Users, X } from 'lucide-react'
import { apiUrl, authenticatedFetch, formatDate, getAccessToken, getCurrentUser } from '../../session'
import type { DirectoryUser } from '../../session'
import AppSidebar from '../../Shared/AppSidebar'
import { toggleNavigation } from '../../Shared/sidebarToggle'

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
  const [searchRequest, setSearchRequest] = useState({ term: '', revision: 0 })
  const [searchResults, setSearchResults] = useState<DirectoryUser[] | null>(null)
  const [appliedSearch, setAppliedSearch] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState('')
  const [isCreateOpen, setIsCreateOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [createError, setCreateError] = useState('')
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [role, setRole] = useState<'Verifier' | 'Admin'>('Verifier')
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
  const [editRole, setEditRole] = useState<'Verifier' | 'Admin' | null>(null)
  const [editError, setEditError] = useState('')
  const [isSavingEdit, setIsSavingEdit] = useState(false)
  const [toastMessage, setToastMessage] = useState('')
  const [blockingUser, setBlockingUser] = useState<DirectoryUser | null>(null)
  const [blockError, setBlockError] = useState('')
  const [isUpdatingBlock, setIsUpdatingBlock] = useState(false)
  const currentUserId = getCurrentUser()?.id
  const [sidebarOpen, setSidebarOpen] = useState(() => sessionStorage.getItem('sidebar-open') === 'true')
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
    const controller = new AbortController()
    const loadMembers = async () => {
      setIsLoading(true)
      setLoadError('')
      try {
        const query = searchRequest.term ? `?${new URLSearchParams({ search: searchRequest.term })}` : ''
        const response = await authenticatedFetch(`${apiUrl}/users${query}`, {
          signal: controller.signal,
          headers: { Authorization: `Bearer ${getAccessToken()}` },
        })
        if (!response.ok) {
          const error: ApiError = await response.json()
          throw new Error(getErrorMessage(error, 'Unable to load users.'))
        }
        const users: DirectoryUser[] = await response.json()
        if (controller.signal.aborted) return
        if (searchRequest.term) {
          setSearchResults(users)
        } else {
          setMembers(users)
          setSearchResults(null)
        }
        setAppliedSearch(searchRequest.term)
      } catch (error) {
        if (controller.signal.aborted) return
        setLoadError(error instanceof Error ? error.message : 'Unable to load users.')
      } finally {
        if (!controller.signal.aborted) setIsLoading(false)
      }
    }

    void loadMembers()
    return () => controller.abort()
  }, [searchRequest])

  const filteredMembers = searchResults ?? members
  const submitSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setSearchRequest((current) => ({ term: searchTerm.trim(), revision: current.revision + 1 }))
  }
  const refreshSearch = () => {
    setSearchRequest((current) => ({ ...current, revision: current.revision + 1 }))
  }
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
    setRole('Verifier')
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
    setEditRole(member.roles.includes('Admin') ? 'Admin' : member.roles.includes('Verifier') ? 'Verifier' : null)
    setEditError('')
  }

  const closeEditForm = () => {
    setEditingUser(null)
    setEditFirstName('')
    setEditLastName('')
    setEditRole(null)
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
        body: JSON.stringify({ firstName, lastName, email, password, isVerifier: role === 'Verifier', isAdmin: role === 'Admin' }),
      })

      if (!response.ok) {
        const error: ApiError = await response.json()
        throw new Error(getErrorMessage(error, 'Unable to create this user.'))
      }

      const createdUser: DirectoryUser = await response.json()
      setMembers((current) => [createdUser, ...current])
      refreshSearch()
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

  const updateUser = async (event: FormEvent<HTMLFormElement>) => {
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
          isVerifier: editRole === 'Verifier',
          isAdmin: editRole === 'Admin',
        }),
      })
      if (!response.ok) {
        const error: ApiError = await response.json()
        throw new Error(getErrorMessage(error, 'Unable to update this user.'))
      }

      const updatedUser: DirectoryUser = await response.json()
      setMembers((current) => current.map((member) => member.id === updatedUser.id ? updatedUser : member))
      refreshSearch()
      setToastMessage(`User updated for ${updatedUser.firstName} ${updatedUser.lastName}.`)
      closeEditForm()
    } catch (error) {
      setEditError(error instanceof Error ? error.message : 'Unable to update this user.')
    } finally {
      setIsSavingEdit(false)
    }
  }

  const changeBlockedStatus = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!blockingUser) return

    setBlockError('')
    setIsUpdatingBlock(true)
    try {
      const response = await authenticatedFetch(`${apiUrl}/users/${encodeURIComponent(blockingUser.id)}/blocked`, {
        method: 'PUT',
        headers: {
          Authorization: `Bearer ${getAccessToken()}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ isBlocked: !blockingUser.isBlocked }),
      })
      if (!response.ok) {
        const error: ApiError = await response.json()
        throw new Error(getErrorMessage(error, 'Unable to change this user status.'))
      }

      const updatedUser: DirectoryUser = await response.json()
      setMembers((current) => current.map((member) => member.id === updatedUser.id ? updatedUser : member))
      refreshSearch()
      setToastMessage(`${updatedUser.firstName} ${updatedUser.lastName} ${updatedUser.isBlocked ? 'blocked' : 'unblocked'}.`)
      setBlockingUser(null)
    } catch (error) {
      setBlockError(error instanceof Error ? error.message : 'Unable to change this user status.')
    } finally {
      setIsUpdatingBlock(false)
    }
  }

  return (
    <div className="app-shell">
      <AppSidebar activePage="users" isOpen={sidebarOpen} onClose={closeSidebar} />
      <main className="main-content users-page">
        <header className="topbar">
          <button className="icon-button menu-button" onClick={() => toggleNavigation(openSidebar)} aria-label="Toggle navigation"><Menu size={20} /></button>
          <div className="breadcrumb"><span>Workspace</span><span className="breadcrumb-slash">/</span><strong>Users</strong></div>
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
            <div><p className="panel-kicker">{appliedSearch ? 'Search results' : 'All users'}</p><h2>Workspace directory <span>{appliedSearch ? `${filteredMembers.length} found` : `${members.length} total`}</span></h2></div>
            <form className="directory-search" role="search" onSubmit={submitSearch}>
              <Search size={16} />
              <input value={searchTerm} onChange={(event) => setSearchTerm(event.target.value)} placeholder="Search users..." aria-label="Search users" />
            </form>
          </div>
          {loadError && <div className="users-error" role="alert">{loadError}</div>}
          <div className="member-table-wrap" aria-busy={isLoading}>
            <table>
              <thead><tr><th>User</th><th>Email</th><th>Role</th><th>Status</th><th>Created</th><th>Actions</th></tr></thead>
              <tbody>
                {filteredMembers.map((member) => {
                  const name = `${member.firstName} ${member.lastName}`
                  const initials = `${member.firstName[0] ?? ''}${member.lastName[0] ?? ''}`.toUpperCase()
                  const roleLabel = member.roles?.length ? member.roles.join(', ') : 'User'
                  return (
                    <tr key={member.id}>
                      <td><div className="member-cell"><div className="member-avatar lilac">{initials}</div><div><strong>{name}</strong></div></div></td>
                      <td>{member.email}</td>
                      <td><span className="role-cell">{roleLabel}</span></td>
                      <td><span className={`user-status${member.isBlocked ? ' user-status-blocked' : ''}`}>{member.isBlocked ? 'Blocked' : 'Active'}</span></td>
                      <td>{formatDate(member.createdOn)}</td>
                      <td className="user-row-actions">
                        <button className="row-menu" aria-label={`Edit user ${name}`} onClick={() => openEditForm(member)}><Edit2 size={16} /></button>
                        <button className="row-menu" aria-label={`Change password for ${name}`} onClick={() => { setPasswordError(''); setPasswordUser(member) }}><KeyRound size={16} /></button>
                        <button className="row-menu" aria-label={`${member.isBlocked ? 'Unblock' : 'Block'} user ${name}`} title={member.id === currentUserId ? 'You cannot block your own account.' : `${member.isBlocked ? 'Unblock' : 'Block'} user`} disabled={member.id === currentUserId} onClick={() => { setBlockError(''); setBlockingUser(member) }}>{member.isBlocked ? <ShieldCheck size={16} /> : <Ban size={16} />}</button>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
            {isLoading && <div className="empty-members">Loading users...</div>}
            {!isLoading && !loadError && filteredMembers.length === 0 && (
              <div className="empty-members">{appliedSearch ? `No users found for “${appliedSearch}”.` : 'No users yet.'}</div>
            )}
          </div>
        </section>
        <footer className="footer-note"><span><a href="/">Back to dashboard <ArrowUpRight size={13} /></a></span></footer>
        </div>

      {blockingUser && (
        <div className="user-modal-backdrop">
          <section className="user-modal" role="dialog" aria-modal="true" aria-labelledby="block-user-title">
            <div className="user-modal-heading">
              <div><p className="panel-kicker">User access</p><h2 id="block-user-title">{blockingUser.isBlocked ? 'Unblock user' : 'Block user'}</h2></div>
              <button className="icon-button" type="button" disabled={isUpdatingBlock} onClick={() => setBlockingUser(null)} aria-label="Close form"><X size={19} /></button>
            </div>
            <p className="user-modal-description">
              {blockingUser.isBlocked
                ? `Allow ${blockingUser.firstName} ${blockingUser.lastName} to sign in again? A fresh login is required; old sessions remain invalid.`
                : `Block ${blockingUser.firstName} ${blockingUser.lastName}? Login will be disabled and existing sessions will lose access on their next API request.`}
            </p>
            <form onSubmit={changeBlockedStatus}>
              {blockError && <div className="auth-error" role="alert">{blockError}</div>}
              <div className="user-modal-actions">
                <button className="user-cancel-button" type="button" disabled={isUpdatingBlock} onClick={() => setBlockingUser(null)}>Cancel</button>
                <button className="primary-button" type="submit" disabled={isUpdatingBlock}>{isUpdatingBlock ? 'Saving...' : blockingUser.isBlocked ? 'Unblock user' : 'Block user'}</button>
              </div>
            </form>
          </section>
        </div>
      )}
      {isCreateOpen && (
        <div className="user-modal-backdrop">
          <section className="user-modal" role="dialog" aria-modal="true" aria-labelledby="create-user-title">
            <div className="user-modal-heading">
              <div><p className="panel-kicker">Workspace directory</p><h2 id="create-user-title">Create user</h2></div>
              <button className="icon-button" type="button" onClick={closeCreateForm} aria-label="Close form"><X size={19} /></button>
            </div>
            <p className="user-modal-description">Create an account and select its roles. Admin grants user-management access. The user can sign in with the email and password you set.</p>
            <form onSubmit={createUser}>
              <div className="auth-name-row">
                <label><span>First name</span><input required maxLength={50} value={firstName} onChange={(event) => setFirstName(event.target.value)} autoFocus /></label>
                <label><span>Last name</span><input required maxLength={50} value={lastName} onChange={(event) => setLastName(event.target.value)} /></label>
              </div>
              <label><span>Email address</span><input required type="email" maxLength={256} value={email} onChange={(event) => setEmail(event.target.value)} /></label>
              <label><span>Initial password</span><div className="user-password-field"><input required type={showInitialPassword ? 'text' : 'password'} minLength={6} value={password} onChange={(event) => setPassword(event.target.value)} autoComplete="new-password" /><button className="password-toggle" type="button" onClick={() => setShowInitialPassword((visible) => !visible)} aria-label={showInitialPassword ? 'Hide initial password' : 'Show initial password'}>{showInitialPassword ? <EyeOff size={16} /> : <Eye size={16} />}</button></div></label>
              <div className="user-role-options">
                <label className="user-role-option"><input type="radio" name="create-user-role" required checked={role === 'Verifier'} onChange={() => setRole('Verifier')} /><span>Verifier</span></label>
                <label className="user-role-option"><input type="radio" name="create-user-role" required checked={role === 'Admin'} onChange={() => setRole('Admin')} /><span>Admin</span></label>
              </div>
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
              <label><span>New password</span><div className="user-password-field"><input required type={showNewPassword ? 'text' : 'password'} minLength={6} value={newPassword} onChange={(event) => setNewPassword(event.target.value)} autoComplete="new-password" autoFocus /><button className="password-toggle" type="button" onClick={() => setShowNewPassword((visible) => !visible)} aria-label={showNewPassword ? 'Hide new password' : 'Show new password'}>{showNewPassword ? <EyeOff size={16} /> : <Eye size={16} />}</button></div></label>
              <label><span>Confirm new password</span><div className="user-password-field"><input required type={showConfirmPassword ? 'text' : 'password'} minLength={6} value={confirmPassword} onChange={(event) => setConfirmPassword(event.target.value)} autoComplete="new-password" /><button className="password-toggle" type="button" onClick={() => setShowConfirmPassword((visible) => !visible)} aria-label={showConfirmPassword ? 'Hide confirmed password' : 'Show confirmed password'}>{showConfirmPassword ? <EyeOff size={16} /> : <Eye size={16} />}</button></div></label>
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
              <div><p className="panel-kicker">User account</p><h2 id="edit-user-title">Edit user</h2></div>
              <button className="icon-button" type="button" onClick={closeEditForm} aria-label="Close form"><X size={19} /></button>
            </div>
            <p className="user-modal-description">Update the name and roles for {editingUser.email}. Admin grants user-management access. Email address stays the same.</p>
            <form onSubmit={updateUser}>
              <div className="auth-name-row">
                <label><span>First name</span><input required maxLength={50} value={editFirstName} onChange={(event) => setEditFirstName(event.target.value)} autoFocus /></label>
                <label><span>Last name</span><input required maxLength={50} value={editLastName} onChange={(event) => setEditLastName(event.target.value)} /></label>
              </div>
              <div className="user-role-options">
                <label className="user-role-option"><input type="radio" name="edit-user-role" required checked={editRole === 'Verifier'} onChange={() => setEditRole('Verifier')} /><span>Verifier</span></label>
                <label className="user-role-option"><input type="radio" name="edit-user-role" required checked={editRole === 'Admin'} onChange={() => setEditRole('Admin')} /><span>Admin</span></label>
              </div>
              {editError && <div className="auth-error" role="alert">{editError}</div>}
              <div className="user-modal-actions">
                <button className="user-cancel-button" type="button" onClick={closeEditForm} disabled={isSavingEdit}>Cancel</button>
                <button className="primary-button" type="submit" disabled={isSavingEdit}>{isSavingEdit ? 'Saving...' : 'Save changes'}</button>
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
