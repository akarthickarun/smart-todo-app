import { useQuery } from '@tanstack/react-query'
import { todoApi } from '@/api/todo-api'
import type { TodoItem } from '@/features/todos/schemas/todoSchemas'

/**
 * Hook to fetch a single todo by ID
 * @param id - Todo item ID
 * @returns Query result with single todo
 */
export function useGetTodoById(id: string) {
  return useQuery<TodoItem>({
    queryKey: ['todos', id],
    queryFn: () => todoApi.getById(id),
  })
}
