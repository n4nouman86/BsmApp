export type LeadValues = {
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
  'city' | 'postalCode' | 'spouseName' | 'spouseDateOfBirth' | 'hopeType' |
  'haveHomeClaims' | 'currentHomeCarrier' | 'anyClaimInPast3Years'
> & {
  id: number
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
  group: 'Lead detail' | 'Spouse details' | 'Auto insurance' | 'Home insurance'
  required?: boolean
  type?: 'email' | 'tel' | 'date'
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
  { name: 'state', label: 'State', group: 'Lead detail', required: true },
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
]

export const leadGroups = ['Lead detail', 'Spouse details', 'Auto insurance', 'Home insurance'] as const

export function leadValues(lead?: Lead): LeadValues {
  return {
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
