import { useQuery } from '@tanstack/react-query'
import { todoApi } from '@/api/todo-api'
import type { TodoItem } from '@/features/todos/schemas/todoSchemas'

/**
 * Hook to fetch all todos with optional status filter
 * @param status - Optional status filter (Pending | Completed)
 * @returns Query result with todos list
 */
export function useTodos(status?: string) {
  return useQuery<TodoItem[]>({
    queryKey: ['todos', status],
    queryFn: () => todoApi.getAll(status),
  })
}
