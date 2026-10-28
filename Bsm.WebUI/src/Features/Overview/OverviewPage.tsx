import { useEffect, useMemo, useState } from 'react'
import { ArrowUpRight, Menu, Plus, Sparkles, Users } from 'lucide-react'
import AppSidebar from '../../Shared/AppSidebar'
import { toggleNavigation } from '../../Shared/sidebarToggle'
import { apiUrl, authenticatedFetch, formatDate, getAccessToken, getCurrentUser } from '../../session'
import type { DirectoryUser } from '../../session'

type ApiError = {
  message?: string
  errors?: Record<string, string[]>
}

function getErrorMessage(error: ApiError, fallback: string) {
  return Object.values(error.errors ?? {}).flat().join(' ') || error.message || fallback
}

function OverviewPage() {
  const [users, setUsers] = useState<DirectoryUser[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [sidebarOpen, setSidebarOpen] = useState(() => sessionStorage.getItem('sidebar-open') === 'true')
  const currentUser = getCurrentUser()
  const displayName = currentUser ? `${currentUser.firstName} ${currentUser.lastName}` : 'Signed-in user'
  const firstName = currentUser?.firstName ?? displayName
  const greeting = new Date().getHours() < 12 ? 'Good morning' : new Date().getHours() < 18 ? 'Good afternoon' : 'Good evening'

  useEffect(() => {
    const loadUsers = async () => {
      try {
        const response = await authenticatedFetch(`${apiUrl}/users`, {
          headers: { Authorization: `Bearer ${getAccessToken()}` },
        })
        if (!response.ok) {
          const apiError: ApiError = await response.json()
          throw new Error(getErrorMessage(apiError, 'Unable to load users.'))
        }
        setUsers(await response.json() as DirectoryUser[])
      } catch (loadError) {
        setError(loadError instanceof Error ? loadError.message : 'Unable to load users.')
      } finally {
        setIsLoading(false)
      }
    }

    void loadUsers()
  }, [])

  const usersThisMonth = useMemo(() => {
    const now = new Date()
    return users.filter((user) => {
      const createdOn = new Date(user.createdOn)
      return createdOn.getMonth() === now.getMonth() && createdOn.getFullYear() === now.getFullYear()
    }).length
  }, [users])
  const recentUsers = useMemo(
    () => [...users].sort((left, right) => Date.parse(right.createdOn) - Date.parse(left.createdOn)).slice(0, 5),
    [users],
  )

  const closeSidebar = () => {
    setSidebarOpen(false)
    sessionStorage.removeItem('sidebar-open')
  }
  const openSidebar = () => {
    setSidebarOpen(true)
    sessionStorage.setItem('sidebar-open', 'true')
  }

  return (
    <div className="app-shell">
      <AppSidebar activePage="overview" isOpen={sidebarOpen} onClose={closeSidebar} />
      <main className="main-content">
        <header className="topbar">
          <button className="icon-button menu-button" onClick={() => toggleNavigation(openSidebar)} aria-label="Toggle navigation"><Menu size={20} /></button>
          <div className="breadcrumb"><span>Workspace</span><span className="breadcrumb-slash">/</span><strong>Overview</strong></div>
        </header>
        <div className="page-content">
          <section className="welcome-row">
            <div>
              <p className="eyebrow">{formatDate(new Date())}</p>
              <h1>{greeting}, {firstName}</h1>
              <p className="welcome-copy">Here is your current user directory overview.</p>
            </div>
            <button className="primary-button" onClick={() => { window.location.href = '/users' }}><Plus size={17} /> Create user</button>
          </section>
          {error && <div className="users-error" role="alert">{error}</div>}
          <section className="metric-grid" aria-label="User metrics">
            <article className="metric-card metric-card-highlight">
              <div className="metric-top"><span>Total users</span><div className="metric-icon"><Users size={18} /></div></div>
              <strong>{isLoading ? '—' : users.length}</strong>
            </article>
            <article className="metric-card">
              <div className="metric-top"><span>Created this month</span><div className="metric-icon mint-icon"><Sparkles size={18} /></div></div>
              <strong>{isLoading ? '—' : usersThisMonth}</strong>
            </article>
          </section>
          <section className="panel table-panel">
            <div className="panel-heading table-heading">
              <div><p className="panel-kicker">User directory</p><h2>Recently created users</h2></div>
              <a className="text-button" href="/users">View all users <ArrowUpRight size={15} /></a>
            </div>
            {error
              ? <div className="empty-members">User data is unavailable.</div>
              : <div className="table-wrap">
                <table>
                  <thead><tr><th>User</th><th>Email</th><th>Created</th></tr></thead>
                  <tbody>{recentUsers.map((user) => {
                    const name = `${user.firstName} ${user.lastName}`
                    const userInitials = `${user.firstName[0] ?? ''}${user.lastName[0] ?? ''}`.toUpperCase()
                    return (
                      <tr key={user.id}>
                        <td><div className="member-cell"><div className="member-avatar lilac">{userInitials}</div><div><strong>{name}</strong></div></div></td>
                        <td>{user.email}</td>
                        <td>{formatDate(user.createdOn)}</td>
                      </tr>
                    )
                  })}</tbody>
                </table>
                {isLoading && <div className="empty-members">Loading users...</div>}
                {!isLoading && recentUsers.length === 0 && <div className="empty-members">No users have been created yet.</div>}
              </div>}
          </section>
        </div>
      </main>
    </div>
  )
}

export default OverviewPage
