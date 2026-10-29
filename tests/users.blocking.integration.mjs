import assert from 'node:assert/strict'
import { createHmac, randomUUID } from 'node:crypto'
import { readFile } from 'node:fs/promises'
import test from 'node:test'

const baseUrl = 'http://localhost:5049'

async function request(method, path, token, body) {
  const response = await fetch(new URL(path, baseUrl), {
    method,
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(body ? { 'Content-Type': 'application/json' } : {}),
    },
    ...(body ? { body: JSON.stringify(body) } : {}),
  })
  const text = await response.text()
  return {
    status: response.status,
    refreshedToken: response.headers.get('x-refreshed-token'),
    data: text ? JSON.parse(text) : null,
  }
}

test('Blocking revokes all sessions immediately and unblock requires fresh login', async () => {
  const settings = JSON.parse(await readFile(new URL('../Bsm.Api/appsettings.json', import.meta.url), 'utf8'))
  const development = JSON.parse(await readFile(new URL('../Bsm.Api/appsettings.Development.json', import.meta.url), 'utf8'))
  const jwt = { ...settings.Jwt, ...development.Jwt }
  const resign = (token, change) => {
    const [header, payload] = token.split('.')
    const claims = JSON.parse(Buffer.from(payload, 'base64url').toString())
    change(claims)
    const input = `${header}.${Buffer.from(JSON.stringify(claims)).toString('base64url')}`
    return `${input}.${createHmac('sha256', jwt.Key).update(input).digest('base64url')}`
  }
  const adminLogin = await request('POST', '/api/auth/login', null, {
    email: 'demo@bsmjoin.test', password: 'Demo@12345',
  })
  assert.equal(adminLogin.status, 200)
  const admin = adminLogin.data.accessToken
  const profile = {
    firstName: 'Blocking', lastName: 'Integration',
    email: `blocking-${randomUUID()}@example.test`, password: `Test!${randomUUID()}Aa1`,
    isVerifier: true,
  }
  let userId
  let temporaryAdminId
  try {
    const created = await request('POST', '/api/users', admin, profile)
    userId = created.data?.id
    assert.equal(created.status, 201)
    assert.equal(created.data.isBlocked, false)
    const path = `/api/users/${userId}/blocked`
    const login = async () => request('POST', '/api/auth/login', null, {
      email: profile.email, password: profile.password,
    })
    const first = await login()
    const second = await login()
    assert.equal(first.status, 200)
    assert.equal(second.status, 200)
    const token = first.data.accessToken
    const secondToken = second.data.accessToken
    const claims = JSON.parse(Buffer.from(token.split('.')[1], 'base64url').toString())
    assert.equal(claims.token_version, '0')
    const protectedPath = '/api/leadstatuses'
    assert.equal((await request('GET', protectedPath, token)).status, 200)

    // Exercise sliding refresh without waiting for half the real token lifetime.
    const olderToken = resign(token, claims => {
      claims.nbf = Math.floor(Date.now() / 1000) - 3600
      claims.exp = Math.floor(Date.now() / 1000) + 60
    })
    const sliding = await request('GET', protectedPath, olderToken)
    assert.equal(sliding.status, 200)
    assert.ok(sliding.refreshedToken)
    assert.equal((await request('GET', protectedPath, sliding.refreshedToken)).status, 200)

    for (const change of [
      claims => { delete claims.token_version },
      claims => { claims.token_version = 'invalid' },
      claims => { claims.token_version = '1' },
    ]) {
      assert.equal((await request('GET', protectedPath, resign(token, change))).status, 401)
    }

    assert.equal((await request('PUT', path, null, { isBlocked: true })).status, 401)
    assert.equal((await request('PUT', path, token, { isBlocked: true })).status, 403)
    assert.equal((await request('PUT', '/api/users/missing-user/blocked', admin, { isBlocked: true })).status, 404)
    assert.equal((await request('PUT', path, admin, {})).status, 400)
    assert.equal((await request('PUT', path, admin, { isBlocked: 'yes' })).status, 400)
    const selfBlock = await request('PUT', `/api/users/${adminLogin.data.user.id}/blocked`, admin, { isBlocked: true })
    assert.equal(selfBlock.status, 409)
    assert.match(selfBlock.data.message, /own account/)

    const blocked = await request('PUT', path, admin, { isBlocked: true })
    assert.equal(blocked.status, 200)
    assert.equal(blocked.data.isBlocked, true)
    assert.equal((await request('GET', `/api/users/${userId}`, admin)).data.isBlocked, true)
    const directory = await request('GET', '/api/users', admin)
    assert.equal(directory.data.find(user => user.id === userId).isBlocked, true)
    for (const session of [token, secondToken, olderToken, sliding.refreshedToken]) {
      const denied = await request('GET', protectedPath, session)
      assert.equal(denied.status, 401)
      assert.equal(denied.refreshedToken, null)
    }
    assert.equal((await login()).status, 401)
    assert.equal((await request('PUT', path, admin, { isBlocked: true })).status, 200)

    const unblocked = await request('PUT', path, admin, { isBlocked: false })
    assert.equal(unblocked.status, 200)
    assert.equal(unblocked.data.isBlocked, false)
    for (const session of [token, secondToken, olderToken, sliding.refreshedToken]) {
      assert.equal((await request('GET', protectedPath, session)).status, 401)
    }
    const fresh = await login()
    assert.equal(fresh.status, 200)
    const freshToken = fresh.data.accessToken
    const freshClaims = JSON.parse(Buffer.from(freshToken.split('.')[1], 'base64url').toString())
    assert.equal(freshClaims.token_version, '2')
    assert.equal((await request('GET', protectedPath, freshToken)).status, 200)
    assert.equal((await request('PUT', path, admin, { isBlocked: false })).status, 200)
    assert.equal((await request('GET', protectedPath, freshToken)).status, 200)

    const adminProfile = { ...profile, email: `blocking-admin-${randomUUID()}@example.test`, isAdmin: true, isVerifier: false }
    const createdAdmin = await request('POST', '/api/users', admin, adminProfile)
    temporaryAdminId = createdAdmin.data?.id
    assert.equal(createdAdmin.status, 201)
    const temporaryAdminLogin = await request('POST', '/api/auth/login', null, {
      email: adminProfile.email, password: adminProfile.password,
    })
    assert.equal(temporaryAdminLogin.status, 200)
    const temporaryAdminToken = temporaryAdminLogin.data.accessToken
    assert.equal((await request('GET', '/api/users', temporaryAdminToken)).status, 200)
    assert.equal((await request('PUT', `/api/users/${temporaryAdminId}/blocked`, admin, { isBlocked: true })).status, 200)
    assert.equal((await request('GET', '/api/users', temporaryAdminToken)).status, 401)
    assert.equal((await request('PUT', path, temporaryAdminToken, { isBlocked: true })).status, 401)
  } finally {
    if (temporaryAdminId) assert.equal((await request('DELETE', `/api/users/${temporaryAdminId}`, admin)).status, 204)
    if (userId) assert.equal((await request('DELETE', `/api/users/${userId}`, admin)).status, 204)
  }
})
