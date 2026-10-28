export type LeadValues = {
  statusId: string
  phoneNumber: string
  firstName: string
  lastName: string
  email: string
  dateOfBirth: string
  streetAddress: string
  city: string
  state: string
  postalCode: string
  haveSpouse: string
  spouseName: string
  spouseDateOfBirth: string
  currentAutoCarrier: string
  anyLaspse: string
  makeAndModel: string
  accidents: string
  tikets: string
  homeOwner: string
  hopeType: string
  haveHomeClaims: string
  currentHomeCarrier: string
  anyClaimInPast3Years: string
}

export type Lead = Omit<LeadValues,
  'statusId' | 'city' | 'postalCode' | 'spouseName' | 'spouseDateOfBirth' | 'hopeType' |
  'haveHomeClaims' | 'currentHomeCarrier' | 'anyClaimInPast3Years'
> & {
  id: number
  statusId: number
  statusName: string
  appUserId: string | null
  createdOn: string
  city: string | null
  postalCode: string | null
  spouseName: string | null
  spouseDateOfBirth: string | null
  hopeType: string | null
  haveHomeClaims: string | null
  currentHomeCarrier: string | null
  anyClaimInPast3Years: string | null
}

type LeadField = {
  name: keyof LeadValues
  label: string
  group: 'Lead detail' | 'Spouse details' | 'Auto insurance' | 'Home insurance' | 'Status'
  required?: boolean
  type?: 'email' | 'tel' | 'date' | 'status' | 'state'
  maxLength?: number
}

export const leadFields: LeadField[] = [
  { name: 'phoneNumber', label: 'Phone number', type: 'tel', group: 'Lead detail', required: true },
  { name: 'firstName', label: 'First name', group: 'Lead detail', required: true },
  { name: 'lastName', label: 'Last name', group: 'Lead detail', required: true },
  { name: 'email', label: 'Email', type: 'email', group: 'Lead detail', required: true },
  { name: 'dateOfBirth', label: 'Date of birth', type: 'date', group: 'Lead detail', required: true },
  { name: 'streetAddress', label: 'Street address', maxLength: 256, group: 'Lead detail', required: true },
  { name: 'city', label: 'City', group: 'Lead detail' },
  { name: 'state', label: 'State', type: 'state', group: 'Lead detail', required: true },
  { name: 'postalCode', label: 'Postal code', group: 'Lead detail' },
  { name: 'haveSpouse', label: 'Have spouse', group: 'Spouse details', required: true },
  { name: 'spouseName', label: 'Spouse name', group: 'Spouse details' },
  { name: 'spouseDateOfBirth', label: 'Spouse date of birth', type: 'date', group: 'Spouse details' },
  { name: 'currentAutoCarrier', label: 'Current auto carrier', group: 'Auto insurance', required: true },
  { name: 'anyLaspse', label: 'Any coverage lapse', group: 'Auto insurance', required: true },
  { name: 'makeAndModel', label: 'Make and model', maxLength: 1024, group: 'Auto insurance', required: true },
  { name: 'accidents', label: 'Accidents', group: 'Auto insurance', required: true },
  { name: 'tikets', label: 'Tickets', group: 'Auto insurance', required: true },
  { name: 'homeOwner', label: 'Homeowner', group: 'Home insurance', required: true },
  { name: 'hopeType', label: 'Home type', group: 'Home insurance' },
  { name: 'haveHomeClaims', label: 'Have home claims', group: 'Home insurance' },
  { name: 'currentHomeCarrier', label: 'Current home carrier', group: 'Home insurance' },
  { name: 'anyClaimInPast3Years', label: 'Any claim in past 3 years', group: 'Home insurance' },
  { name: 'statusId', label: 'Status', type: 'status', group: 'Status', required: true },
]

export const usStates = [
  { code: 'AL', name: 'Alabama' },
  { code: 'AK', name: 'Alaska' },
  { code: 'AZ', name: 'Arizona' },
  { code: 'AR', name: 'Arkansas' },
  { code: 'CA', name: 'California' },
  { code: 'CO', name: 'Colorado' },
  { code: 'CT', name: 'Connecticut' },
  { code: 'DE', name: 'Delaware' },
  { code: 'DC', name: 'District of Columbia' },
  { code: 'FL', name: 'Florida' },
  { code: 'GA', name: 'Georgia' },
  { code: 'HI', name: 'Hawaii' },
  { code: 'ID', name: 'Idaho' },
  { code: 'IL', name: 'Illinois' },
  { code: 'IN', name: 'Indiana' },
  { code: 'IA', name: 'Iowa' },
  { code: 'KS', name: 'Kansas' },
  { code: 'KY', name: 'Kentucky' },
  { code: 'LA', name: 'Louisiana' },
  { code: 'ME', name: 'Maine' },
  { code: 'MD', name: 'Maryland' },
  { code: 'MA', name: 'Massachusetts' },
  { code: 'MI', name: 'Michigan' },
  { code: 'MN', name: 'Minnesota' },
  { code: 'MS', name: 'Mississippi' },
  { code: 'MO', name: 'Missouri' },
  { code: 'MT', name: 'Montana' },
  { code: 'NE', name: 'Nebraska' },
  { code: 'NV', name: 'Nevada' },
  { code: 'NH', name: 'New Hampshire' },
  { code: 'NJ', name: 'New Jersey' },
  { code: 'NM', name: 'New Mexico' },
  { code: 'NY', name: 'New York' },
  { code: 'NC', name: 'North Carolina' },
  { code: 'ND', name: 'North Dakota' },
  { code: 'OH', name: 'Ohio' },
  { code: 'OK', name: 'Oklahoma' },
  { code: 'OR', name: 'Oregon' },
  { code: 'PA', name: 'Pennsylvania' },
  { code: 'RI', name: 'Rhode Island' },
  { code: 'SC', name: 'South Carolina' },
  { code: 'SD', name: 'South Dakota' },
  { code: 'TN', name: 'Tennessee' },
  { code: 'TX', name: 'Texas' },
  { code: 'UT', name: 'Utah' },
  { code: 'VT', name: 'Vermont' },
  { code: 'VA', name: 'Virginia' },
  { code: 'WA', name: 'Washington' },
  { code: 'WV', name: 'West Virginia' },
  { code: 'WI', name: 'Wisconsin' },
  { code: 'WY', name: 'Wyoming' },
] as const

export const leadGroups = ['Lead detail', 'Spouse details', 'Auto insurance', 'Home insurance', 'Status'] as const

export function leadValues(lead?: Lead): LeadValues {
  return {
    statusId: lead ? String(lead.statusId) : '',
    phoneNumber: lead?.phoneNumber ?? '',
    firstName: lead?.firstName ?? '',
    lastName: lead?.lastName ?? '',
    email: lead?.email ?? '',
    dateOfBirth: lead?.dateOfBirth ?? '',
    streetAddress: lead?.streetAddress ?? '',
    city: lead?.city ?? '',
    state: lead?.state ?? '',
    postalCode: lead?.postalCode ?? '',
    haveSpouse: lead?.haveSpouse ?? '',
    spouseName: lead?.spouseName ?? '',
    spouseDateOfBirth: lead?.spouseDateOfBirth ?? '',
    currentAutoCarrier: lead?.currentAutoCarrier ?? '',
    anyLaspse: lead?.anyLaspse ?? '',
    makeAndModel: lead?.makeAndModel ?? '',
    accidents: lead?.accidents ?? '',
    tikets: lead?.tikets ?? '',
    homeOwner: lead?.homeOwner ?? '',
    hopeType: lead?.hopeType ?? '',
    haveHomeClaims: lead?.haveHomeClaims ?? '',
    currentHomeCarrier: lead?.currentHomeCarrier ?? '',
    anyClaimInPast3Years: lead?.anyClaimInPast3Years ?? '',
  }
}
