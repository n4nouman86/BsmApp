import { BriefcaseBusiness, ChevronDown, LayoutDashboard, LogOut, Rocket, Sparkles, Users, X } from 'lucide-react'
import { useState } from 'react'
import { getCurrentUser } from '../session'

export type ProductApp = 'bsmjoin' | 'leadlogix' | 'leadventures'

type AppSidebarProps = {
  activeApp: ProductApp
  activePage: 'overview' | 'users' | 'dashboard'
  usersCount: number
  isLoading: boolean
  isOpen: boolean
  onClose: () => void
}

const productApps: { id: ProductApp; name: string; href: string; icon: typeof Sparkles }[] = [
  { id: 'bsmjoin', name: 'BsmJoin', href: '/', icon: Sparkles },
  { id: 'leadlogix', name: 'LeadLogix', href: '/leadlogix', icon: BriefcaseBusiness },
  { id: 'leadventures', name: 'LeadVentures', href: '/leadventures', icon: Rocket },
]

function AppSidebar({ activeApp, activePage, usersCount, isLoading, isOpen, onClose }: AppSidebarProps) {
  const [isAppSwitcherOpen, setIsAppSwitcherOpen] = useState(false)
  const user = getCurrentUser()
  const displayName = user ? `${user.firstName} ${user.lastName}` : 'Signed-in user'
  const initials = user ? `${user.firstName[0] ?? ''}${user.lastName[0] ?? ''}`.toUpperCase() : '?'
  const selectedApp = productApps.find((app) => app.id === activeApp) ?? productApps[0]

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
          <span className="brand-name">bsmjoin</span>
          <button className="icon-button sidebar-close" onClick={onClose} aria-label="Close navigation"><X size={18} /></button>
        </div>
        <div className="app-switcher-wrap">
          <button className="workspace-switcher" aria-expanded={isAppSwitcherOpen} onClick={() => setIsAppSwitcherOpen((open) => !open)}>
            <div className="workspace-avatar"><selectedApp.icon size={17} /></div>
            <div className="workspace-copy"><strong>{selectedApp.name}</strong><span>{activeApp === 'bsmjoin' ? 'Central account' : 'Admin reports'}</span></div>
            <ChevronDown className={isAppSwitcherOpen ? 'switcher-chevron-open' : ''} size={16} />
          </button>
          {isAppSwitcherOpen && (
            <div className="app-switcher-menu" aria-label="Switch application">
              {productApps.map((app) => (
                <a className={`app-switcher-option ${app.id === activeApp ? 'app-switcher-option-active' : ''}`} href={app.href} key={app.id}>
                  <app.icon size={17} /><span>{app.name}</span>
                </a>
              ))}
            </div>
          )}
        </div>
        <nav className="main-nav" aria-label="Main navigation">
          {activeApp === 'bsmjoin'
            ? <>
              <span className="nav-label">Workspace</span>
              <a className={`nav-item ${activePage === 'overview' ? 'nav-item-active' : ''}`} href="/">
                <LayoutDashboard size={18} strokeWidth={1.8} /><span>Overview</span>
              </a>
              <a className={`nav-item ${activePage === 'users' ? 'nav-item-active' : ''}`} href="/users">
                <Users size={18} strokeWidth={1.8} /><span>Users</span>
                {!isLoading && <span className="nav-count">{usersCount}</span>}
              </a>
            </>
            : <>
              <span className="nav-label">Admin workspace</span>
              <a className={`nav-item ${activePage === 'dashboard' ? 'nav-item-active' : ''}`} href={`/${activeApp}`}>
                <LayoutDashboard size={18} strokeWidth={1.8} /><span>Dashboard</span>
              </a>
            </>}
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
