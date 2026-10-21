import { useEffect } from 'react'
import AdminLoginPage from './Features/BsmJoin/Auth/AdminLoginPage'
import OverviewPage from './Features/BsmJoin/Overview/OverviewPage'
import UsersPage from './Features/BsmJoin/Users/UsersPage'
import LeadLogixDashboardPage from './Features/LeadLogix/Dashboard/LeadLogixDashboardPage'
import LeadVenturesDashboardPage from './Features/LeadVentures/Dashboard/LeadVenturesDashboardPage'
import { getAccessToken } from './session'
import './App.css'

function App() {
  const hasToken = Boolean(getAccessToken())
  const path = window.location.pathname

  useEffect(() => {
    const pageTitle = path === '/leadlogix'
      ? 'LeadLogix Dashboard | BsmJoin'
      : path === '/leadventures'
        ? 'LeadVentures Dashboard | BsmJoin'
        : path === '/users'
          ? 'Users | BsmJoin'
          : path === '/login' || !hasToken
            ? 'Admin Login | BsmJoin'
            : 'Overview | BsmJoin'
    document.title = pageTitle
  }, [hasToken, path])

  if (path === '/login' || !hasToken) {
    return <AdminLoginPage />
  }

  if (path === '/users') {
    return <UsersPage />
  }

  if (path === '/leadlogix') {
    return <LeadLogixDashboardPage />
  }

  if (path === '/leadventures') {
    return <LeadVenturesDashboardPage />
  }

  return <OverviewPage />
}

export default App
