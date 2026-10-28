import { useSyncExternalStore } from 'react'

export type ToastKind = 'success' | 'error'
export type Toast = { id: number; message: string; kind: ToastKind }

const durationMs = 4000
const listeners = new Set<() => void>()
let toasts: Toast[] = []
let nextId = 1

const emit = () => listeners.forEach(listener => listener())

export function dismissToast(id: number) {
  toasts = toasts.filter(toast => toast.id !== id)
  emit()
}

export function showToast(message: string, kind: ToastKind = 'success') {
  const id = nextId++
  toasts = [...toasts, { id, message, kind }]
  emit()
  window.setTimeout(() => dismissToast(id), durationMs)
}

export function useToasts() {
  return useSyncExternalStore(
    listener => { listeners.add(listener); return () => { listeners.delete(listener) } },
    () => toasts,
  )
}
