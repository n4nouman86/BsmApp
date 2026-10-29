import assert from 'node:assert/strict'
import { randomUUID } from 'node:crypto'
import test from 'node:test'

async function request(method, path, token, body) {
  const response = await fetch(`http://localhost:5049/api${path}`, {
    method,
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(body ? { 'Content-Type': 'application/json' } : {}),
    },
    ...(body ? { body: JSON.stringify(body) } : {}),
  })
  const text = await response.text()
  return { status: response.status, data: text ? JSON.parse(text) : null }
}

test('Create and reset passwords require only six characters', async () => {
  const login = await request('POST', '/auth/login', null, {
    email: 'demo@bsmjoin.test', password: 'Demo@12345',
  })
  assert.equal(login.status, 200)
  const admin = login.data.accessToken
  const profile = {
    firstName: 'Password', lastName: 'Test',
    email: `password-${randomUUID()}@example.test`,
  }
  let id
  try {
    for (const password of ['', 'aaaaa']) {
      assert.equal((await request('POST', '/users', admin, { ...profile, password })).status, 400)
    }
    const created = await request('POST', '/users', admin, { ...profile, password: 'aaaaaa' })
    id = created.data?.id
    assert.equal(created.status, 201)
    assert.equal((await request('POST', '/auth/login', null, {
      email: profile.email, password: 'aaaaaa',
    })).status, 200)
    for (const newPassword of ['', '12345']) {
      assert.equal((await request('POST', `/users/${id}/password`, admin, { newPassword })).status, 400)
    }
    for (const newPassword of ['123456', 'AAAAAA', '!!!!!!', 'a'.repeat(101)]) {
      assert.equal((await request('POST', `/users/${id}/password`, admin, { newPassword })).status, 204)
      assert.equal((await request('POST', '/auth/login', null, {
        email: profile.email, password: newPassword,
      })).status, 200)
    }
  } finally {
    if (id) assert.equal((await request('DELETE', `/users/${id}`, admin)).status, 204)
  }
})
