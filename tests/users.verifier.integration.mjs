import assert from 'node:assert/strict'
import { randomUUID } from 'node:crypto'
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
    location: response.headers.get('location'),
    data: text ? JSON.parse(text) : null,
  }
}

test('Admins can create, assign and remove Verifier without changing other roles', async () => {
  const login = await request('POST', '/api/auth/login', null, {
    email: 'demo@bsmjoin.test',
    password: 'Demo@12345',
  })
  assert.equal(login.status, 200)
  const admin = login.data.accessToken
  const ids = []
  const suffix = randomUUID()
  const password = `Test!${randomUUID()}Aa1`
  const profile = {
    firstName: 'Integration',
    lastName: 'Verifier',
    email: `verifier-${suffix}@example.test`,
  }
  try {
    const created = await request('POST', '/api/users', admin, {
      ...profile, password, isVerifier: true,
    })
    if (created.data?.id) ids.push(created.data.id)
    assert.equal(created.status, 201)
    assert.ok(created.location)
    assert.match(created.data.createdOn, /(?:\+00:00|Z)$/)
    assert.deepEqual(created.data.roles, ['Verifier'])
    const path = `/api/users/${created.data.id}`
    assert.deepEqual((await request('GET', path, admin)).data.roles, ['Verifier'])

    const verifierLogin = await request('POST', '/api/auth/login', null, {
      email: profile.email, password,
    })
    assert.equal(verifierLogin.status, 200)
    assert.deepEqual(verifierLogin.data.user.roles, ['Verifier'])
    const verifier = verifierLogin.data.accessToken
    assert.equal((await request('GET', '/api/users', verifier)).status, 403)
    assert.equal((await request('PUT', path, verifier, {
      ...profile, isVerifier: false,
    })).status, 403)
    assert.equal((await request('POST', '/api/users', verifier, {
      ...profile, email: `forbidden-${suffix}@example.test`, password, isVerifier: true,
    })).status, 403)

    const legacyUpdate = await request('PUT', path, admin, profile)
    assert.equal(legacyUpdate.status, 200)
    assert.deepEqual(legacyUpdate.data.roles, ['Verifier'])
    for (const isVerifier of [true, false, false, true]) {
      const updated = await request('PUT', path, admin, { ...profile, isVerifier })
      assert.equal(updated.status, 200)
      assert.deepEqual(updated.data.roles, isVerifier ? ['Verifier'] : [])
      assert.deepEqual((await request('GET', path, admin)).data.roles, updated.data.roles)
    }

    const standardProfile = { ...profile, email: `standard-${suffix}@example.test` }
    const standard = await request('POST', '/api/users', admin, {
      ...standardProfile, password,
    })
    if (standard.data?.id) ids.push(standard.data.id)
    assert.equal(standard.status, 201)
    assert.deepEqual(standard.data.roles, [])

    const conflict = await request('PUT', path, admin, {
      ...profile, firstName: 'Should roll back', email: standardProfile.email, isVerifier: false,
    })
    assert.equal(conflict.status, 409)
    const unchanged = await request('GET', path, admin)
    assert.equal(unchanged.data.firstName, profile.firstName)
    assert.equal(unchanged.data.email, profile.email)
    assert.deepEqual(unchanged.data.roles, ['Verifier'])

    const invalid = await request('PUT', path, admin, { ...profile, isVerifier: 'invalid' })
    assert.equal(invalid.status, 400)
    const anonymous = await request('PUT', path, null, { ...profile, isVerifier: false })
    assert.equal(anonymous.status, 401)

    const directory = await request('GET', '/api/users', admin)
    assert.equal(directory.status, 200)
    assert.deepEqual(directory.data.find(user => user.id === created.data.id).roles, ['Verifier'])
    assert.deepEqual(directory.data.find(user => user.id === standard.data.id).roles, [])
    assert.ok(directory.data.find(user => user.id === login.data.user.id).roles.includes('Admin'))

    for (const search of [profile.email, `${profile.firstName} ${profile.lastName}`]) {
      const found = await request('GET', `/api/users?search=${encodeURIComponent(`  ${search}  `)}`, admin)
      assert.equal(found.status, 200)
      assert.ok(found.data.some(user => user.id === created.data.id))
      assert.ok(found.data.every(user =>
        `${user.firstName} ${user.lastName} ${user.email}`.includes(search)))
    }
    const noMatches = await request('GET', `/api/users?search=${randomUUID()}`, admin)
    assert.equal(noMatches.status, 200)
    assert.deepEqual(noMatches.data, [])
    const allUsers = await request('GET', '/api/users?search=%20%20', admin)
    assert.equal(allUsers.status, 200)
    assert.equal(allUsers.data.length, directory.data.length)
    assert.equal((await request('GET', '/api/users?search=Demo', verifier)).status, 403)

    const boundaryProfile = { ...standardProfile, firstName: 'F'.repeat(50), lastName: 'L'.repeat(50) }
    const boundaryPath = `/api/users/${standard.data.id}`
    const boundaryUpdate = await request('PUT', boundaryPath, admin, boundaryProfile)
    assert.equal(boundaryUpdate.status, 200)
    assert.equal(boundaryUpdate.data.firstName, boundaryProfile.firstName)
    assert.equal(boundaryUpdate.data.lastName, boundaryProfile.lastName)
    for (const field of ['firstName', 'lastName']) {
      const tooLong = { ...boundaryProfile, [field]: 'X'.repeat(51) }
      assert.equal((await request('PUT', boundaryPath, admin, tooLong)).status, 400)
      assert.equal((await request('POST', '/api/users', admin, {
        ...tooLong, email: `invalid-${field}-${suffix}@example.test`, password,
      })).status, 400)
    }
    const boundaryCreate = await request('POST', '/api/users', admin, {
      ...boundaryProfile, email: `boundary-${suffix}@example.test`, password,
    })
    if (boundaryCreate.data?.id) ids.push(boundaryCreate.data.id)
    assert.equal(boundaryCreate.status, 201)
    assert.equal(boundaryCreate.data.firstName.length, 50)
    assert.equal(boundaryCreate.data.lastName.length, 50)

    const adminProfile = { ...profile, email: `admin-${suffix}@example.test` }
    assert.equal((await request('POST', '/api/users', admin, {
      ...adminProfile, password, isAdmin: true, isVerifier: true,
    })).status, 400)
    const createdAdmin = await request('POST', '/api/users', admin, {
      ...adminProfile, password, isAdmin: true, isVerifier: false,
    })
    if (createdAdmin.data?.id) ids.push(createdAdmin.data.id)
    assert.equal(createdAdmin.status, 201)
    assert.deepEqual(createdAdmin.data.roles, ['Admin'])
    const adminPath = `/api/users/${createdAdmin.data.id}`
    const preserved = await request('PUT', adminPath, admin, adminProfile)
    assert.equal(preserved.status, 200)
    assert.deepEqual(preserved.data.roles, ['Admin'])
    for (const flags of [{ isVerifier: true }, { isVerifier: true, isAdmin: true }]) {
      assert.equal((await request('PUT', adminPath, admin, { ...adminProfile, ...flags })).status, 400)
      assert.deepEqual((await request('GET', adminPath, admin)).data.roles, ['Admin'])
    }
    const removedAdmin = await request('PUT', adminPath, admin, {
      ...adminProfile, isAdmin: false, isVerifier: true,
    })
    assert.equal(removedAdmin.status, 200)
    assert.deepEqual(removedAdmin.data.roles, ['Verifier'])
    const restoredAdmin = await request('PUT', adminPath, admin, {
      ...adminProfile, isAdmin: true, isVerifier: false,
    })
    assert.equal(restoredAdmin.status, 200)
    assert.deepEqual(restoredAdmin.data.roles, ['Admin'])
    const adminSignIn = await request('POST', '/api/auth/login', null, {
      email: adminProfile.email, password,
    })
    assert.equal(adminSignIn.status, 200)
    assert.deepEqual(adminSignIn.data.user.roles, ['Admin'])
    assert.equal((await request('GET', '/api/users', adminSignIn.data.accessToken)).status, 200)
    assert.equal((await request('PUT', path, verifier, {
      ...profile, isAdmin: true,
    })).status, 403)
  } finally {
    for (const id of ids.reverse()) {
      assert.equal((await request('DELETE', `/api/users/${id}`, admin)).status, 204)
    }
  }
})
