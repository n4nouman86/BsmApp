import { useEffect } from 'react'
import AdminLoginPage from './Features/Auth/AdminLoginPage'
import OverviewPage from './Features/Overview/OverviewPage'
import UsersPage from './Features/Users/UsersPage'
import LeadsPage from './Features/Leads/LeadsPage'
import { getAccessToken, getCurrentUser } from './session'
import './App.css'

function App() {
  const hasToken = Boolean(getAccessToken())
  const path = window.location.pathname
  const isAdmin = getCurrentUser()?.roles.includes('Admin') ?? false

  useEffect(() => {
    const pageTitle = path === '/login' || !hasToken
      ? 'Login | bsm'
      : path === '/leads' || !isAdmin
        ? 'Leads | bsm'
        : path === '/users' ? 'Users | bsm' : 'Overview | bsm'
    document.title = pageTitle
  }, [hasToken, path, isAdmin])

  if (path === '/login' || !hasToken) {
    return <AdminLoginPage />
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
