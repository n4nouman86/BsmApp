import { ArrowLeft, ArrowUpRight, Check, Download, Mail, MoreHorizontal, Plus, Search, ShieldCheck, UserRound, Users } from 'lucide-react'
import { useMemo, useState } from 'react'

type Member = {
  initials: string
  name: string
  email: string
  role: string
  joined: string
  status: 'Active' | 'Pending'
  tone: string
}

const members: Member[] = [
  { initials: 'AM', name: 'Ava Morgan', email: 'ava.morgan@email.com', role: 'Administrator', joined: 'Sep 24, 2026', status: 'Active', tone: 'lilac' },
  { initials: 'JL', name: 'Jonas Lee', email: 'jonas.lee@email.com', role: 'Member', joined: 'Sep 22, 2026', status: 'Active', tone: 'mint' },
  { initials: 'SK', name: 'Sofia Khan', email: 'sofia.khan@email.com', role: 'Member', joined: 'Sep 21, 2026', status: 'Pending', tone: 'peach' },
  { initials: 'RB', name: 'Ravi Bhatt', email: 'ravi.bhatt@email.com', role: 'Manager', joined: 'Sep 18, 2026', status: 'Active', tone: 'blue' },
  { initials: 'LT', name: 'Lina Torres', email: 'lina.torres@email.com', role: 'Member', joined: 'Sep 14, 2026', status: 'Active', tone: 'rose' },
]

function UsersPage() {
  const [searchTerm, setSearchTerm] = useState('')
  const filteredMembers = useMemo(() => members.filter((member) => `${member.name} ${member.email} ${member.role}`.toLowerCase().includes(searchTerm.toLowerCase())), [searchTerm])

  return (
    <div className="users-page">
      <header className="users-topbar"><a className="back-link" href="/"><ArrowLeft size={16} /> Overview</a><div className="users-top-actions"><button className="icon-button" aria-label="Export members"><Download size={18} /></button><div className="top-profile">NM</div></div></header>
      <main className="users-content">
        <div className="users-breadcrumb">Workspace <span>/</span> Members</div>
        <section className="users-heading"><div><p className="eyebrow">Workspace directory</p><h1>Members</h1><p className="welcome-copy">Manage access, roles and member accounts from one place.</p></div><button className="primary-button"><Plus size={17} /> Invite member</button></section>
        <section className="user-stat-row"><div className="user-stat"><div className="user-stat-icon"><Users size={17} /></div><div><span>Total members</span><strong>248</strong></div></div><div className="user-stat"><div className="user-stat-icon user-stat-green"><Check size={17} /></div><div><span>Active members</span><strong>232</strong></div></div><div className="user-stat"><div className="user-stat-icon user-stat-orange"><Mail size={17} /></div><div><span>Pending invites</span><strong>16</strong></div></div><div className="user-stat"><div className="user-stat-icon user-stat-blue"><ShieldCheck size={17} /></div><div><span>Admins</span><strong>4</strong></div></div></section>
        <section className="panel members-directory"><div className="directory-toolbar"><div><p className="panel-kicker">All members</p><h2>Workspace directory <span>248 total</span></h2></div><label className="directory-search"><Search size={16} /><input value={searchTerm} onChange={(event) => setSearchTerm(event.target.value)} placeholder="Search members..." aria-label="Search members" /></label></div><div className="member-table-wrap"><table><thead><tr><th>Member</th><th>Role</th><th>Joined</th><th>Status</th><th aria-label="Actions" /></tr></thead><tbody>{filteredMembers.map((member) => <tr key={member.email}><td><div className="member-cell"><div className={`member-avatar ${member.tone}`}>{member.initials}</div><div><strong>{member.name}</strong><span>{member.email}</span></div></div></td><td><span className="role-cell"><UserRound size={14} />{member.role}</span></td><td>{member.joined}</td><td><span className={`status ${member.status.toLowerCase()}`}><i />{member.status}</span></td><td><button className="row-menu" aria-label={`More options for ${member.name}`}><MoreHorizontal size={18} /></button></td></tr>)}</tbody></table>{filteredMembers.length === 0 && <div className="empty-members">No members found for “{searchTerm}”.</div>}</div></section>
        <footer className="footer-note"><span><span className="status-dot" /> All systems operational</span><span><a href="/">Back to dashboard <ArrowUpRight size={13} /></a></span></footer>
      </main>
    </div>
  )
}

export default UsersPage