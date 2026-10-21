export type AppUser = {
  id: string
  email: string
  firstName: string
  lastName: string
  roles: string[]
}

export type DirectoryUser = {
  id: string
  firstName: string
  lastName: string
  email: string | null
  createdOn: string
}

export const apiUrl = import.meta.env.VITE_API_URL ?? 'http://localhost:5049/api'

export function formatDate(value: string | Date) {
  return new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  }).format(new Date(value))
}

export function getAccessToken() {
  return localStorage.getItem('bsmjoin_token') ?? ''
}

export function getCurrentUser(): AppUser | null {
  const storedUser = localStorage.getItem('bsmjoin_user')
  if (!storedUser) return null

  const user: unknown = JSON.parse(storedUser)
  if (typeof user !== 'object' || user === null) return null

  const candidate = user as Partial<AppUser>
  if (
    typeof candidate.id !== 'string'
    || typeof candidate.email !== 'string'
    || typeof candidate.firstName !== 'string'
    || typeof candidate.lastName !== 'string'
    || !Array.isArray(candidate.roles)
    || !candidate.roles.every((role) => typeof role === 'string')
  ) {
    return null
  }

  return candidate as AppUser
}
