import { useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { todoApi } from '@/api/todo-api'
import type { CreateTodoInput } from '@/features/todos/schemas/todoSchemas'

interface UseCreateTodoOptions {
  onSuccess?: (id: string) => void
  onError?: (error: Error) => void
}

/**
 * Hook to create a new todo
 * @param options - Optional callbacks for success/error
 * @returns Mutation result with create function
 */
export function useCreateTodo(options?: UseCreateTodoOptions) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateTodoInput) => todoApi.create(data),
    onSuccess: (id) => {
      queryClient.invalidateQueries({ queryKey: ['todos'] })
      toast.success('Todo created successfully')
      options?.onSuccess?.(id)
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to create todo')
      options?.onError?.(error)
    },
  })
}
