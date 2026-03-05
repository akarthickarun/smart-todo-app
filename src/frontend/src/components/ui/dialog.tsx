import * as React from "react"


export interface DialogProps {
  open?: boolean
  onOpenChange?: (open: boolean) => void
  children: React.ReactNode
}


export function Dialog({ children }: DialogProps) {
  // This is a simple wrapper for demonstration; replace with shadcn/ui Dialog in real app
  return <div>{children}</div>;
}

export function DialogTrigger({ children }: { children: React.ReactNode }) {
  return <>{children}</>;
}



export function DialogContent({ children }: { children: React.ReactNode }) {
  return <div className="bg-background p-6 rounded-lg shadow-lg w-full max-w-md mx-auto mt-10">{children}</div>
}

export function DialogHeader({ children }: { children: React.ReactNode }) {
  return <div className="mb-4">{children}</div>
}

export function DialogTitle({ children }: { children: React.ReactNode }) {
  return <h2 className="text-lg font-semibold mb-2">{children}</h2>
}
