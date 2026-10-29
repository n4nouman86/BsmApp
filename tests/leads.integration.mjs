import assert from 'node:assert/strict'
import { randomUUID } from 'node:crypto'
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
  return { status: response.status, location: response.headers.get('location'), data: text ? JSON.parse(text) : null }
}

test('Leads CRUD, SQL field limits, and ownership isolation', async () => {
  const settings = JSON.parse(await readFile(new URL('../Bsm.Api/appsettings.Development.json', import.meta.url), 'utf8'))
  const adminLogin = await request('POST', '/api/auth/login', null, {
    email: settings.DevelopmentAdmin.Email,
    password: settings.DevelopmentAdmin.Password,
  })
  assert.equal(adminLogin.status, 200, 'Local admin login must succeed')
  const admin = adminLogin.data.accessToken
  const users = []
  const leads = new Set()
  const suffix = randomUUID()
  const statusIds = []
  const makeStatus = async name => {
    const result = await request('POST', '/api/leadstatuses', admin, { name: `IT ${name} ${suffix.slice(0, 8)}` })
    if (result.data?.id) statusIds.push(result.data.id)
    assert.equal(result.status, 201, 'Temporary status creation must succeed')
    return result.data
  }
  const status = await makeStatus('A')
  const otherStatus = await makeStatus('B')
  const payload = {
    statusId: status.id,
    phoneNumber: '2125550134', firstName: 'Integration', lastName: 'Lead',
    email: `lead@example.test`, dateOfBirth: '1990-05-20',
    streetAddress: '100 Example Street', city: 'Austin', state: 'TX', postalCode: '78701',
    haveSpouse: 'Yes', spouseName: 'Example Spouse', spouseDateOfBirth: '1991-06-21',
    currentAutoCarrier: 'Example Auto', anyLaspse: 'No', makeAndModel: 'Example sedan',
    accidents: 'None', tikets: 'None', homeOwner: 'Yes', hopeType: 'Single family',
    haveHomeClaims: 'No', currentHomeCarrier: 'Example Home', anyClaimInPast3Years: 'No',
  }
  const createLead = async (token, body) => {
    const result = await request('POST', '/api/leads', token, body)
    if (result.data?.id) leads.add(result.data.id)
    assert.equal(result.status, 201, 'Lead creation must return 201')
    return result
  }
  try {
    for (const index of [1, 2]) {
      const email = `leads-${index}-${suffix}@example.test`
      const password = `Test!${randomUUID()}Aa1`
      const created = await request('POST', '/api/users', admin, {
        firstName: 'Integration', lastName: `User${index}`, email, password,
      })
      if (created.data?.id) users.push({ id: created.data.id })
      assert.equal(created.status, 201, 'Temporary user creation must succeed')
      const login = await request('POST', '/api/auth/login', null, { email, password })
      assert.equal(login.status, 200)
      users[index - 1].token = login.data.accessToken
    }
    const [a, b] = users
    assert.equal((await request('GET', '/api/leads')).status, 401)
    assert.equal((await request('GET', '/api/users', a.token)).status, 403)
    const created = await createLead(a.token, payload)
    const id = created.data.id
    const path = `/api/leads/${id}`
    assert.ok(created.location, 'Creation must supply a Location header')
    assert.equal((await request('GET', created.location, a.token)).status, 200)
    assert.equal(created.data.appUserId, a.id)
    assert.ok(created.data.createdOn.endsWith('+00:00') || created.data.createdOn.endsWith('Z'))
    for (const [key, value] of Object.entries(payload)) assert.equal(created.data[key], value, key)
    assert.ok((await request('GET', '/api/leads', a.token)).data.some(lead => lead.id === id))
    assert.ok(!(await request('GET', '/api/leads', b.token)).data.some(lead => lead.id === id))
    for (const method of ['GET', 'PUT', 'DELETE']) {
      assert.equal((await request(method, path, b.token, method === 'PUT' ? payload : undefined)).status, 404)
    }
    assert.equal((await request('GET', path, admin)).status, 200)
    assert.ok((await request('GET', '/api/leads', admin)).data.some(lead => lead.id === id))
    for (const search of [
      `${payload.firstName} ${payload.lastName}`, payload.email, payload.phoneNumber,
      payload.city, payload.state, status.name,
    ]) {
      const searchPath = `/api/leads?search=${encodeURIComponent(`  ${search}  `)}`
      assert.ok((await request('GET', searchPath, a.token)).data.some(lead => lead.id === id))
      assert.ok((await request('GET', searchPath, admin)).data.some(lead => lead.id === id))
      assert.ok(!(await request('GET', searchPath, b.token)).data.some(lead => lead.id === id))
    }
    assert.deepEqual((await request('GET', `/api/leads?search=${randomUUID()}`, a.token)).data, [])
    const adminUpdate = await request('PUT', path, admin, { ...payload, firstName: 'Updated' })
    assert.equal(adminUpdate.status, 200)
    assert.equal(adminUpdate.data.appUserId, a.id)
    assert.equal(adminUpdate.data.createdOn, created.data.createdOn)
    const updatePayload = { ...payload, firstName: 'Owner update', appUserId: b.id, createdOn: '2000-01-01T00:00:00Z' }
    const updated = await request('PUT', path, a.token, updatePayload)
    assert.equal(updated.status, 200)
    assert.equal(updated.data.appUserId, a.id)
    assert.equal(updated.data.createdOn, created.data.createdOn)
    for (const key of Object.keys(payload)) assert.equal(updated.data[key], updatePayload[key], key)
    const optional = ['city', 'postalCode', 'spouseName', 'spouseDateOfBirth', 'hopeType', 'haveHomeClaims', 'currentHomeCarrier', 'anyClaimInPast3Years']
    const nullable = { ...payload }
    for (const key of optional) nullable[key] = null
    const nullResult = await request('PUT', path, a.token, nullable)
    assert.equal(nullResult.status, 200)
    for (const key of optional) assert.equal(nullResult.data[key], null, key)
    const limits = { ...payload, streetAddress: 'A'.repeat(256), makeAndModel: 'B'.repeat(1024) }
    const allowed = await request('PUT', path, a.token, limits)
    assert.equal(allowed.status, 200)
    assert.equal(allowed.data.streetAddress.length, 256)
    assert.equal(allowed.data.makeAndModel.length, 1024)
    const missingBirthDate = { ...payload }
    delete missingBirthDate.dateOfBirth
    assert.equal(allowed.data.statusName, status.name)
    const reassigned = await request('PUT', path, a.token, { ...payload, statusId: otherStatus.id })
    assert.equal(reassigned.status, 200)
    assert.equal(reassigned.data.statusId, otherStatus.id)
    assert.equal(reassigned.data.statusName, otherStatus.name)
    assert.equal((await request('GET', path, a.token)).data.statusName, otherStatus.name)
    assert.equal((await request('DELETE', `/api/leadstatuses/${otherStatus.id}`, admin)).status, 409, 'Statuses in use cannot be deleted')
    assert.equal((await request('POST', '/api/leads', a.token, { ...payload, statusId: 2147483647 })).status, 400)
    assert.equal((await request('PUT', path, a.token, { ...payload, statusId: 2147483647 })).status, 400)
    assert.equal((await request('POST', '/api/leads', a.token, { ...payload, phoneNumber: '555-0100' })).status, 400, 'Short phone numbers are rejected')
    assert.equal((await request('PUT', path, a.token, { ...payload, phoneNumber: '555-0100' })).status, 400)
    const formattedPhone = await request('PUT', path, a.token, { ...payload, phoneNumber: '(212) 555-0134' })
    assert.equal(formattedPhone.status, 200)
    assert.equal(formattedPhone.data.phoneNumber, '2125550134', 'Phone numbers are stored as 10 digits')
    const missingStatus = { ...payload }
    delete missingStatus.statusId
    const invalidPayloads = [
      missingStatus,
      missingBirthDate,
      { ...payload, dateOfBirth: null },
      { ...payload, dateOfBirth: '2026-02-30' },
      { ...payload, firstName: '' },
      { ...payload, email: 'not-an-email' },
      { ...payload, streetAddress: 'A'.repeat(257) },
      { ...payload, makeAndModel: 'B'.repeat(1025) },
    ]
    for (const [key, value] of Object.entries(payload)) {
      if (typeof value === 'string' && !['dateOfBirth', 'spouseDateOfBirth', 'streetAddress', 'makeAndModel'].includes(key)) {
        invalidPayloads.push({ ...payload, [key]: 'A'.repeat(51) })
      }
    }
    for (const invalid of invalidPayloads) {
      const result = await request('PUT', path, a.token, invalid)
      assert.equal(result.status, 400, 'Invalid field values must return 400')
      assert.ok(result.data.errors, 'Validation must include errors')
    }
    for (const method of ['GET', 'PUT', 'DELETE']) {
      assert.equal((await request(method, '/api/leads/2147483647', a.token, method === 'PUT' ? payload : undefined)).status, 404)
    }
    const adminLead = await createLead(admin, { ...payload, appUserId: a.id, createdOn: '2000-01-01T00:00:00Z' })
    assert.equal(adminLead.data.appUserId, adminLogin.data.user.id)
    assert.notEqual(adminLead.data.createdOn, '2000-01-01T00:00:00Z')
    assert.equal((await request('GET', `/api/leads/${adminLead.data.id}`, a.token)).status, 404)
    assert.equal((await request('DELETE', path, a.token)).status, 204)
    assert.equal((await request('GET', path, a.token)).status, 404)
    assert.equal((await request('DELETE', `/api/leads/${adminLead.data.id}`, admin)).status, 204)
  } finally {
    const failures = []
    for (const id of leads) {
      const result = await request('DELETE', `/api/leads/${id}`, admin)
      if (![204, 404].includes(result.status)) failures.push(`lead ${id}: HTTP ${result.status}`)
    }
    for (const id of statusIds) {
      const result = await request('DELETE', `/api/leadstatuses/${id}`, admin)
      if (![204, 404].includes(result.status)) failures.push(`status ${id}: HTTP ${result.status}`)
    }
    for (const { id } of users) {
      const result = await request('DELETE', `/api/users/${id}`, admin)
      if (![204, 404].includes(result.status)) failures.push(`test user cleanup: HTTP ${result.status}`)
    }
    assert.deepEqual(failures, [], 'Synthetic test records must be cleaned up')
  }
})
