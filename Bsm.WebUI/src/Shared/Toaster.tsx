import { useEffect, useRef } from 'react'
import { dismissToast, useToasts } from './toast'
import './Toaster.css'

function Toaster() {
  const toasts = useToasts()
  const ref = useRef<HTMLDivElement>(null)

  // A popover lives in the top layer, so toasts stay visible above open modal dialogs.
  useEffect(() => {
    const element = ref.current
    if (!element) return
    if (element.matches(':popover-open')) element.hidePopover()
    element.showPopover()
  }, [toasts])

  return (
    <div ref={ref} popover="manual" className="toaster" aria-live="polite">
      {toasts.map(toast => (
        <div key={toast.id} className={`toast toast-${toast.kind}`} role={toast.kind === 'error' ? 'alert' : 'status'} onClick={() => dismissToast(toast.id)}>
          {toast.message}
        </div>
      ))}
    </div>
  )
}

export default Toaster
