import { useEffect } from 'react'
import AdminLoginPage from './Features/Auth/AdminLoginPage'
import OverviewPage from './Features/Overview/OverviewPage'
import UsersPage from './Features/Users/UsersPage'
import { getAccessToken } from './session'
import './App.css'

function App() {
  const hasToken = Boolean(getAccessToken())
  const path = window.location.pathname

  useEffect(() => {
    const pageTitle = path === '/users'
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

  return <OverviewPage />
}

export default App
