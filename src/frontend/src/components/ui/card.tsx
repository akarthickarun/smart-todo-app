import * as React from "react"
import { cn } from "@/lib/utils"

export function Card({ children, className }: { children: React.ReactNode; className?: string }) {
  return <div className={cn("rounded-lg border bg-card text-card-foreground shadow-sm", className)}>{children}</div>
}

export function CardHeader({ children }: { children: React.ReactNode }) {
  return <div className="p-4 border-b">{children}</div>
}

export function CardTitle({ children, className }: { children: React.ReactNode; className?: string }) {
  return <h3 className={cn("text-lg font-semibold", className)}>{children}</h3>
}

export function CardContent({ children }: { children: React.ReactNode }) {
  return <div className="p-4 pt-0">{children}</div>
}
