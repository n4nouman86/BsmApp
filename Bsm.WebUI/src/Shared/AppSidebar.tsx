import { Contact, LayoutDashboard, LogOut, Sparkles, Users, X } from 'lucide-react'
import { getCurrentUser } from '../session'

type AppSidebarProps = {
  activePage: 'overview' | 'users' | 'leads'
  usersCount?: number
  isLoading: boolean
  isOpen: boolean
  onClose: () => void
}

function AppSidebar({ activePage, usersCount, isLoading, isOpen, onClose }: AppSidebarProps) {
  const user = getCurrentUser()
  const displayName = user ? `${user.firstName} ${user.lastName}` : 'Signed-in user'
  const initials = user ? `${user.firstName[0] ?? ''}${user.lastName[0] ?? ''}`.toUpperCase() : '?'

  const signOut = () => {
    localStorage.removeItem('bsmjoin_token')
    localStorage.removeItem('bsmjoin_user')
    sessionStorage.removeItem('sidebar-open')
    window.location.href = '/login'
  }

  return (
    <>
      <aside className={`sidebar ${isOpen ? 'sidebar-open' : ''}`}>
        <div className="brand-row">
          <div className="brand-mark"><Sparkles size={17} strokeWidth={2.5} /></div>
          <span className="brand-name">bsm</span>
          <button className="icon-button sidebar-close" onClick={onClose} aria-label="Close navigation"><X size={18} /></button>
        </div>
        <nav className="main-nav" aria-label="Main navigation">
          <span className="nav-label">Workspace</span>
          {user?.roles.includes('Admin') && <a className={`nav-item ${activePage === 'overview' ? 'nav-item-active' : ''}`} href="/">
            <LayoutDashboard size={18} strokeWidth={1.8} /><span>Overview</span>
          </a>}
          <a className={`nav-item ${activePage === 'leads' ? 'nav-item-active' : ''}`} href="/leads">
            <Contact size={18} strokeWidth={1.8} /><span>Leads</span>
          </a>
          {user?.roles.includes('Admin') && <a className={`nav-item ${activePage === 'users' ? 'nav-item-active' : ''}`} href="/users">
            <Users size={18} strokeWidth={1.8} /><span>Users</span>
            {!isLoading && usersCount !== undefined && <span className="nav-count">{usersCount}</span>}
          </a>}
        </nav>
        <div className="sidebar-footer">
          <div className="profile-row">
            <div className="profile-avatar">{initials}</div>
            <div><strong>{displayName}</strong><span>{user?.roles.includes('Admin') ? 'Administrator' : 'Signed-in user'}</span></div>
          </div>
          <button className="sidebar-signout" onClick={signOut}><LogOut size={15} /> Sign out</button>
        </div>
      </aside>
      {isOpen && <button className="scrim" aria-label="Close navigation" onClick={onClose} />}
    </>
  )
}

export default AppSidebar
