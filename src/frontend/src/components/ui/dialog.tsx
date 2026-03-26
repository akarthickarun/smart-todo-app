import * as React from "react"


export interface DialogProps {
  open?: boolean
  onOpenChange?: (open: boolean) => void
  children: React.ReactNode
}

type DialogContextValue = {
  open: boolean
  setOpen: (open: boolean) => void
}

const DialogContext = React.createContext<DialogContextValue | undefined>(undefined)

export function Dialog({ open: openProp, onOpenChange, children }: DialogProps) {
  const [uncontrolledOpen, setUncontrolledOpen] = React.useState(false)

  const isControlled = openProp !== undefined
  const open = isControlled ? !!openProp : uncontrolledOpen

  const setOpen = React.useCallback(
    (nextOpen: boolean) => {
      if (!isControlled) {
        setUncontrolledOpen(nextOpen)
      }
      if (onOpenChange) {
        onOpenChange(nextOpen)
      }
    },
    [isControlled, onOpenChange]
  )

  const value = React.useMemo<DialogContextValue>(
    () => ({ open, setOpen }),
    [open, setOpen]
  )

  return <DialogContext.Provider value={value}>{children}</DialogContext.Provider>
}

export function DialogTrigger({ children }: { children: React.ReactNode }) {
  const context = React.useContext(DialogContext)

  // If used outside of a Dialog, render children as-is to avoid breaking existing usage.
  if (!context) {
    return <>{children}</>
  }

  const { setOpen } = context

  // If child is not a valid React element, wrap it in a button that opens the dialog.
  if (!React.isValidElement(children)) {
    return (
      <button type="button" onClick={() => setOpen(true)}>
        {children}
      </button>
    )
  }

  const child = children as React.ReactElement<any>

  const handleClick = (event: React.MouseEvent) => {
    if (typeof child.props.onClick === "function") {
      child.props.onClick(event)
    }
    if (!event.defaultPrevented) {
      setOpen(true)
    }
  }

  return React.cloneElement(child, {
    ...child.props,
    onClick: handleClick,
  })
}

export function DialogContent({ children }: { children: React.ReactNode }) {
  const context = React.useContext(DialogContext)

  // If used outside of a Dialog, render nothing to avoid incorrect behavior.
  if (!context) {
    return null
  }

  if (!context.open) {
    return null
  }

  return (
    <div className="bg-background p-6 rounded-lg shadow-lg w-full max-w-md mx-auto mt-10">
      {children}
    </div>
  )
}

export function DialogHeader({ children }: { children: React.ReactNode }) {
  return <div className="mb-4">{children}</div>
}

export function DialogTitle({ children }: { children: React.ReactNode }) {
  return <h2 className="text-lg font-semibold mb-2">{children}</h2>
}
