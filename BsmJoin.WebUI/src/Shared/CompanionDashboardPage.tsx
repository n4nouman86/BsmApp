import { useState } from 'react'
import { BriefcaseBusiness, Menu, Rocket } from 'lucide-react'
import AppSidebar from './AppSidebar'
import CompanionDashboard from './CompanionDashboard'
import { getCurrentUser } from '../session'
import type { ProductApp } from './AppSidebar'

type CompanionDashboardPageProps = {
  app: Extract<ProductApp, 'leadlogix' | 'leadventures'>
}

function CompanionDashboardPage({ app }: CompanionDashboardPageProps) {
  const [sidebarOpen, setSidebarOpen] = useState(() => sessionStorage.getItem('sidebar-open') === 'true')
  const user = getCurrentUser()
  const initials = user ? `${user.firstName[0] ?? ''}${user.lastName[0] ?? ''}`.toUpperCase() : '?'
  const isLeadLogix = app === 'leadlogix'
  const appName = isLeadLogix ? 'LeadLogix' : 'LeadVentures'
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
      <AppSidebar
        activeApp={app}
        activePage="dashboard"
        usersCount={0}
        isLoading
        isOpen={sidebarOpen}
        onClose={closeSidebar}
      />
      <main className="main-content">
        <header className="topbar">
          <button className="icon-button menu-button" onClick={openSidebar} aria-label="Open navigation"><Menu size={20} /></button>
          <div className="breadcrumb"><span>Apps</span><span className="breadcrumb-slash">/</span><strong>{appName}</strong></div>
          <div className="topbar-actions"><span className="demo-topbar-tag">REPORTS</span><div className="top-profile">{initials}</div></div>
        </header>
        <div className="page-content demo-page">
          <div className="demo-notice"><span>DATABASE REPORTS</span> Read-only {appName} reports. Dates without records will show an empty report.</div>
          <section className="welcome-row">
            <div>
              <p className="eyebrow">Super Admin</p>
              <h1>Dashboard</h1>
              <p className="welcome-copy">Daily transfers, agent day report, and weekly performance.</p>
            </div>
            <div className="demo-brand-mark">
              {isLeadLogix ? <BriefcaseBusiness size={18} /> : <Rocket size={18} />}
              {appName}
            </div>
          </section>
          <CompanionDashboard app={app} />
          <footer className="footer-note"><span>{appName} reports <b>•</b> BsmJoin</span></footer>
        </div>
      </main>
    </div>
  )
}

export default CompanionDashboardPage
