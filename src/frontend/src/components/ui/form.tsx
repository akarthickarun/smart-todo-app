import * as React from "react"
import { cn } from "@/lib/utils"

export interface FormProps extends React.FormHTMLAttributes<HTMLFormElement> {}

export function Form({ className, ...props }: FormProps) {
  return <form className={cn("space-y-6", className)} {...props} />
}

export function FormField({ children }: { children: React.ReactNode }) {
  return <div className="space-y-2">{children}</div>
}

export function FormItem({ children }: { children: React.ReactNode }) {
  return <div className="space-y-1">{children}</div>
}

export function FormLabel({ children }: { children: React.ReactNode }) {
  return <label className="block text-sm font-medium text-foreground">{children}</label>
}

export function FormControl({ children }: { children: React.ReactNode }) {
  return <div>{children}</div>
}

export function FormMessage({ children }: { children: React.ReactNode }) {
  return <p className="text-xs text-destructive mt-1">{children}</p>
}
