import { useEffect } from 'react'
import AdminLoginPage from './Features/Auth/AdminLoginPage'
import OverviewPage from './Features/Overview/OverviewPage'
import UsersPage from './Features/Users/UsersPage'
import LeadsPage from './Features/Leads/LeadsPage'
import LeadStatusesPage from './Features/LeadStatuses/LeadStatusesPage'
import DncNumbersPage from './Features/DncNumbers/DncNumbersPage'
import DncNamesPage from './Features/DncNames/DncNamesPage'
import { getAccessToken, getCurrentUser } from './session'
import './App.css'

function App() {
  const hasToken = Boolean(getAccessToken())
  const path = window.location.pathname
  const isAdmin = getCurrentUser()?.roles.includes('Admin') ?? false

  useEffect(() => {
    const pageTitle = path === '/login' || !hasToken
      ? 'Login | BSM'
      : path === '/dnc' && isAdmin
        ? 'DNC Numbers | BSM'
        : path === '/dnc-names' && isAdmin
        ? 'DNC Names | BSM'
        : path === '/lead-statuses'
        ? 'Lead statuses | BSM'
        : path === '/leads' || !isAdmin
          ? 'Leads | BSM'
          : path === '/users' ? 'Users | BSM' : 'Overview | BSM'
    document.title = pageTitle
  }, [hasToken, path, isAdmin])

  if (path === '/login' || !hasToken) {
    return <AdminLoginPage />
  }

  if (path === '/dnc' && isAdmin) {
    return <DncNumbersPage />
  }

  if (path === '/dnc-names' && isAdmin) {
    return <DncNamesPage />
  }

  if (path === '/lead-statuses') {
    return <LeadStatusesPage />
  }

  if (path === '/leads' || !isAdmin) {
    return <LeadsPage />
  }

  if (path === '/users') {
    return <UsersPage />
  }

  return <OverviewPage />
}

export default App
