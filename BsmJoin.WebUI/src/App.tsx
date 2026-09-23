import { useState } from 'react'
import { Activity, ArrowUpRight, Bell, ChevronDown, CircleDollarSign, Clock3, LayoutDashboard, LogOut, Menu, MoreHorizontal, Search, Settings, ShieldCheck, Sparkles, Users, X } from 'lucide-react'
import AdminLoginPage from './AdminLoginPage'
import UsersPage from './UsersPage'
import './App.css'

const navigation = [
  { label: 'Overview', icon: LayoutDashboard },
  { label: 'Members', icon: Users },
  { label: 'Activity', icon: Activity },
  { label: 'Security', icon: ShieldCheck },
]

const recentJoins = [
  { initials: 'AM', name: 'Ava Morgan', email: 'ava.morgan@email.com', plan: 'Pro', date: 'Today, 09:42', status: 'Active', tone: 'lilac' },
  { initials: 'JL', name: 'Jonas Lee', email: 'jonas.lee@email.com', plan: 'Team', date: 'Today, 08:18', status: 'Active', tone: 'mint' },
  { initials: 'SK', name: 'Sofia Khan', email: 'sofia.khan@email.com', plan: 'Starter', date: 'Yesterday, 16:04', status: 'Pending', tone: 'peach' },
  { initials: 'RB', name: 'Ravi Bhatt', email: 'ravi.bhatt@email.com', plan: 'Pro', date: 'Yesterday, 14:27', status: 'Active', tone: 'blue' },
]

function App() {
  const [activeSection, setActiveSection] = useState('Overview')
  const [sidebarOpen, setSidebarOpen] = useState(false)

  if (window.location.pathname === '/login' || !localStorage.getItem('bsmjoin_token')) {
    return <AdminLoginPage />
  }

  if (window.location.pathname === '/users') {
    return <UsersPage />
  }

  return (
    <div className="app-shell">
      <button className="logout-floating" onClick={() => { localStorage.removeItem('bsmjoin_token'); localStorage.removeItem('bsmjoin_user'); window.location.href = '/login' }}><LogOut size={16} /> Sign out</button>
      <aside className={`sidebar ${sidebarOpen ? 'sidebar-open' : ''}`}>
        <div className="brand-row"><div className="brand-mark"><Sparkles size={17} strokeWidth={2.5} /></div><span className="brand-name">bsmjoin</span><button className="icon-button sidebar-close" onClick={() => setSidebarOpen(false)} aria-label="Close navigation"><X size={18} /></button></div>
        <div className="workspace-switcher"><div className="workspace-avatar">B</div><div className="workspace-copy"><strong>BsmJoin App</strong><span>Workspace</span></div><ChevronDown size={16} /></div>
        <nav className="main-nav" aria-label="Main navigation"><span className="nav-label">Workspace</span>{navigation.map(({ label, icon: Icon }) => <button className={`nav-item ${activeSection === label ? 'nav-item-active' : ''}`} key={label} onClick={() => { if (label === 'Members') { window.location.href = '/users'; return }; setActiveSection(label); setSidebarOpen(false) }}><Icon size={18} strokeWidth={1.8} /><span>{label}</span>{label === 'Members' && <span className="nav-count">248</span>}</button>)}<span className="nav-label nav-label-spaced">Manage</span><button className="nav-item" onClick={() => setActiveSection('Settings')}><Settings size={18} strokeWidth={1.8} /><span>Settings</span></button></nav>
        <div className="sidebar-footer"><div className="usage-head"><span>Monthly usage</span><span>68%</span></div><div className="usage-bar"><span /></div><p>680 of 1,000 member seats used</p><div className="profile-row"><div className="profile-avatar">NM</div><div><strong>Nadia Malik</strong><span>Administrator</span></div><MoreHorizontal size={18} /></div></div>
      </aside>
      {sidebarOpen && <button className="scrim" aria-label="Close navigation" onClick={() => setSidebarOpen(false)} />}
      <main className="main-content">
        <header className="topbar"><button className="icon-button menu-button" onClick={() => setSidebarOpen(true)} aria-label="Open navigation"><Menu size={20} /></button><div className="breadcrumb"><span>Workspace</span><span className="breadcrumb-slash">/</span><strong>{activeSection}</strong></div><div className="topbar-actions"><label className="search-box"><Search size={17} /><input placeholder="Search anything..." aria-label="Search anything" /></label><button className="icon-button notification-button" aria-label="Notifications"><Bell size={19} /><span /></button><div className="top-profile">NM</div></div></header>
        <div className="page-content">
          <section className="welcome-row"><div><p className="eyebrow">Wednesday, September 24, 2026</p><h1>Good morning, Nadia <span>✦</span></h1><p className="welcome-copy">Here is what is happening across your workspace today.</p></div><button className="primary-button"><Users size={17} /> Invite member</button></section>
          <section className="metric-grid" aria-label="Workspace metrics"><article className="metric-card metric-card-highlight"><div className="metric-top"><span>Total members</span><div className="metric-icon"><Users size={18} /></div></div><strong>248</strong><div className="metric-trend positive"><ArrowUpRight size={14} /> 12.8% <span>vs last month</span></div></article><article className="metric-card"><div className="metric-top"><span>Active this month</span><div className="metric-icon mint-icon"><Activity size={18} /></div></div><strong>186</strong><div className="metric-trend positive"><ArrowUpRight size={14} /> 8.4% <span>vs last month</span></div></article><article className="metric-card"><div className="metric-top"><span>New this week</span><div className="metric-icon peach-icon"><Sparkles size={18} /></div></div><strong>32</strong><div className="metric-trend positive"><ArrowUpRight size={14} /> 24.1% <span>vs last week</span></div></article><article className="metric-card"><div className="metric-top"><span>Revenue this month</span><div className="metric-icon blue-icon"><CircleDollarSign size={18} /></div></div><strong>$18,420</strong><div className="metric-trend positive"><ArrowUpRight size={14} /> 16.2% <span>vs last month</span></div></article></section>
          <section className="content-grid"><article className="panel chart-panel"><div className="panel-heading"><div><p className="panel-kicker">Member growth</p><h2>Workspace overview</h2></div><button className="period-select">Last 30 days <ChevronDown size={15} /></button></div><div className="chart-summary"><strong>+34.6%</strong><span>member growth</span></div><div className="chart"><div className="chart-grid"><span /><span /><span /><span /></div><svg viewBox="0 0 700 210" preserveAspectRatio="none" role="img" aria-label="Member growth chart"><defs><linearGradient id="chartFill" x1="0" x2="0" y1="0" y2="1"><stop offset="0%" stopColor="#b4a1ff" stopOpacity=".42" /><stop offset="100%" stopColor="#b4a1ff" stopOpacity="0" /></linearGradient></defs><path d="M0 178 C45 170 52 145 92 153 S137 147 166 126 S217 141 250 120 S294 114 324 126 S370 106 406 92 S450 98 478 74 S523 91 550 63 S598 79 625 42 S674 54 700 21 V210 H0Z" fill="url(#chartFill)" /><path d="M0 178 C45 170 52 145 92 153 S137 147 166 126 S217 141 250 120 S294 114 324 126 S370 106 406 92 S450 98 478 74 S523 91 550 63 S598 79 625 42 S674 54 700 21" fill="none" stroke="#927cff" strokeWidth="3" /></svg><div className="chart-labels"><span>Aug 26</span><span>Sep 02</span><span>Sep 09</span><span>Sep 16</span><span>Sep 24</span></div></div></article><article className="panel activity-panel"><div className="panel-heading"><div><p className="panel-kicker">Live feed</p><h2>Recent activity</h2></div><button className="text-button">View all <ArrowUpRight size={15} /></button></div><div className="activity-list"><div className="activity-item"><div className="activity-dot violet"><Users size={15} /></div><div><p><strong>Ava Morgan</strong> joined the workspace</p><span>2 minutes ago</span></div></div><div className="activity-item"><div className="activity-dot green"><ShieldCheck size={15} /></div><div><p>Security settings were updated</p><span>18 minutes ago</span></div></div><div className="activity-item"><div className="activity-dot orange"><CircleDollarSign size={15} /></div><div><p><strong>Pro plan</strong> payment received</p><span>46 minutes ago</span></div></div><div className="activity-item"><div className="activity-dot blue"><Settings size={15} /></div><div><p>Workspace preferences changed</p><span>1 hour ago</span></div></div></div></article></section>
          <section className="panel table-panel"><div className="panel-heading table-heading"><div><p className="panel-kicker">Your workspace</p><h2>Recent members</h2></div><button className="text-button">View all members <ArrowUpRight size={15} /></button></div><div className="table-wrap"><table><thead><tr><th>Member</th><th>Plan</th><th>Joined</th><th>Status</th><th aria-label="Actions" /></tr></thead><tbody>{recentJoins.map((member) => <tr key={member.email}><td><div className="member-cell"><div className={`member-avatar ${member.tone}`}>{member.initials}</div><div><strong>{member.name}</strong><span>{member.email}</span></div></div></td><td><span className="plan-text">{member.plan}</span></td><td><span className="joined-date"><Clock3 size={14} />{member.date}</span></td><td><span className={`status ${member.status.toLowerCase()}`}><i />{member.status}</span></td><td><button className="row-menu" aria-label={`More options for ${member.name}`}><MoreHorizontal size={18} /></button></td></tr>)}</tbody></table></div></section>
          <footer className="footer-note"><span><span className="status-dot" /> All systems operational</span><span>BsmJoin App <b>•</b> v1.0.0</span></footer>
        </div>
      </main>
    </div>
  )
}

export default App
