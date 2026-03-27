import * as React from "react"
import { FormProvider } from "react-hook-form"
import type { FieldValues, UseFormReturn } from "react-hook-form"

export interface FormProps<TFieldValues extends FieldValues = FieldValues> extends UseFormReturn<TFieldValues> {
  children: React.ReactNode
}

export function Form<TFieldValues extends FieldValues = FieldValues>({ children, ...form }: FormProps<TFieldValues>) {
  return <FormProvider {...(form as UseFormReturn<TFieldValues>)}>{children}</FormProvider>
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

export function FormMessage({ children }: { children?: React.ReactNode }) {
  if (!children) {
    return null
  }
  return <p className="text-xs text-destructive mt-1">{children}</p>
}
