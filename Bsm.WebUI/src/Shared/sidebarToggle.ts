const storageKey = 'bsm_sidebar_collapsed'
const desktopQuery = '(min-width: 761px)'

const applyCollapsed = (collapsed: boolean) => document.documentElement.classList.toggle('sidebar-collapsed', collapsed)

applyCollapsed(localStorage.getItem(storageKey) === 'true')

/** Collapses or expands the sidebar on desktop; on small screens it opens the slide-in drawer instead. */
export function toggleNavigation(openDrawer: () => void) {
  if (!window.matchMedia(desktopQuery).matches) {
    openDrawer()
    return
  }
  const collapsed = !document.documentElement.classList.contains('sidebar-collapsed')
  applyCollapsed(collapsed)
  localStorage.setItem(storageKey, String(collapsed))
}
