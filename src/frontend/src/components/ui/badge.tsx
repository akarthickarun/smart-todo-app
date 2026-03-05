import * as React from "react"
import { cn } from "@/lib/utils"

export function Badge({ children, variant = "secondary", className }: { children: React.ReactNode; variant?: "secondary" | "success"; className?: string }) {
  const color = variant === "success" ? "bg-green-500 text-white" : "bg-muted text-foreground";
  return <span className={cn("inline-block px-2 py-0.5 rounded text-xs font-medium", color, className)}>{children}</span>;
}
