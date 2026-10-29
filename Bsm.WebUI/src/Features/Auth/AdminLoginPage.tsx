import { useState } from 'react'
import type { FormEvent } from 'react'
import { ArrowRight, Eye, EyeOff, LockKeyhole, Mail, Sparkles } from 'lucide-react'

const apiUrl = import.meta.env.VITE_API_URL ?? 'http://localhost:5049/api'

function AdminLoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [error, setError] = useState(() => new URLSearchParams(window.location.search).get('session') === 'expired'
    ? 'Your session has expired. Please sign in again.'
    : '')
  const [isLoading, setIsLoading] = useState(false)

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError('')
    setIsLoading(true)

    try {
      const response = await fetch(`${apiUrl}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      })
      const data = await response.json()
      if (!response.ok) throw new Error(data.message ?? 'Invalid email or password.')

      localStorage.setItem('bsmjoin_token', data.accessToken)
      localStorage.setItem('bsmjoin_user', JSON.stringify(data.user))
      window.location.href = '/'
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Unable to sign in.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <main className="auth-page">
      <section className="auth-art"><div className="auth-brand"><span><Sparkles size={17} /></span> BSM</div><div className="auth-art-content"><p className="eyebrow">One workspace. Everything aligned.</p><h1>Bring your people<br /><em>into focus.</em></h1><p>Manage members, access and activity from one calm, connected workspace.</p></div><div className="auth-art-footer"><span>Secure workspace access</span><span>© 2026 BSM</span></div></section>
      <section className="auth-form-side">
        <div className="auth-form-wrap">
          <div className="mobile-auth-brand"><span><Sparkles size={16} /></span> BSM</div>
          <div className="auth-heading"><p className="eyebrow">Workspace access</p><h2>Welcome back</h2><p>Sign in to manage your BSM workspace.</p></div>
          <form onSubmit={submit}>
            <label>
              <span>Email address</span>
              <div className="auth-input">
                <Mail size={16} />
                <input required type="email" autoComplete="username" value={email} onChange={(event) => setEmail(event.target.value)} placeholder="admin@company.com" />
              </div>
            </label>
            <label>
              <span>Password</span>
              <div className="auth-input">
                <LockKeyhole size={16} />
                <input required type={showPassword ? 'text' : 'password'} autoComplete="current-password" value={password} onChange={(event) => setPassword(event.target.value)} placeholder="Enter your password" />
                <button type="button" className="password-toggle" onClick={() => setShowPassword((value) => !value)} aria-label={showPassword ? 'Hide password' : 'Show password'}>{showPassword ? <EyeOff size={16} /> : <Eye size={16} />}</button>
              </div>
            </label>
            {error && <div className="auth-error" role="alert">{error}</div>}
            <button className="auth-submit" type="submit" disabled={isLoading}>{isLoading ? 'Signing in...' : 'Sign in'}{!isLoading && <ArrowRight size={17} />}</button>
          </form>
          <p className="auth-switch">Manage your leads. Administrators can also manage user accounts.</p>
        </div>
      </section>
    </main>
  )
}

export default AdminLoginPage