import { useId, useMemo, useState } from 'react'
import type { KeyboardEvent } from 'react'
import { usStates } from './leadFields'

type StateComboboxProps = {
  name: string
  value: string
  required?: boolean
  disabled?: boolean
  onChange: (code: string) => void
}

function StateCombobox({ name, value, required, disabled, onChange }: StateComboboxProps) {
  const listId = useId()
  const [query, setQuery] = useState('')
  const [open, setOpen] = useState(false)
  const [active, setActive] = useState(0)

  const selected = usStates.find(state => state.code === value)
  const label = selected ? `${selected.name} (${selected.code})` : value
  const matches = useMemo(() => {
    const term = query.trim().toLowerCase()
    return term ? usStates.filter(state => state.name.toLowerCase().includes(term) || state.code.toLowerCase() === term) : usStates
  }, [query])

  const choose = (code: string) => {
    onChange(code)
    setOpen(false)
    setQuery('')
  }

  const onKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'ArrowDown') {
      event.preventDefault()
      setOpen(true)
      setActive(index => Math.min(index + 1, matches.length - 1))
    } else if (event.key === 'ArrowUp') {
      event.preventDefault()
      setActive(index => Math.max(index - 1, 0))
    } else if (event.key === 'Enter' && open) {
      event.preventDefault()
      if (matches[active]) choose(matches[active].code)
    } else if (event.key === 'Escape' && open) {
      // Close the list only, without triggering the dialog's own cancel handling.
      event.preventDefault()
      setOpen(false)
      setQuery('')
    }
  }

  return (
    <div className="state-combobox">
      <input
        type="text"
        role="combobox"
        aria-expanded={open}
        aria-controls={listId}
        aria-autocomplete="list"
        autoComplete="off"
        placeholder="Search state..."
        disabled={disabled}
        value={open ? query : label}
        onFocus={() => { setOpen(true); setQuery(''); setActive(0) }}
        onBlur={() => { setOpen(false); setQuery('') }}
        onChange={event => { setQuery(event.target.value); setActive(0); setOpen(true) }}
        onKeyDown={onKeyDown}
      />
      <input className="state-combobox-value" name={name} tabIndex={-1} aria-hidden="true" readOnly required={required} value={value} />
      {open && (
        <ul id={listId} role="listbox" className="state-combobox-list">
          {matches.length === 0 && <li className="state-combobox-empty">No matching states</li>}
          {matches.map((state, index) => (
            <li
              key={state.code}
              role="option"
              aria-selected={state.code === value}
              className={index === active ? 'active' : undefined}
              onMouseDown={event => { event.preventDefault(); choose(state.code) }}
              onMouseEnter={() => setActive(index)}
            >{state.name} ({state.code})</li>
          ))}
        </ul>
      )}
    </div>
  )
}

export default StateCombobox
