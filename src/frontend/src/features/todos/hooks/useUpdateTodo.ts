import { useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { todoApi } from '@/api/todo-api'
import type { UpdateTodoInput } from '@/features/todos/schemas/todoSchemas'

interface UseUpdateTodoOptions {
  onSuccess?: () => void
  onError?: (error: Error) => void
}

interface UpdateTodoData {
  id: string
  data: UpdateTodoInput
}

/**
 * Hook to update an existing todo
 * @param options - Optional callbacks for success/error
 * @returns Mutation result with update function
 */
export function useUpdateTodo(options?: UseUpdateTodoOptions) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: UpdateTodoData) => todoApi.update(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['todos'] })
      queryClient.invalidateQueries({ queryKey: ['todos', id] })
      toast.success('Todo updated successfully')
      options?.onSuccess?.()
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to update todo')
      options?.onError?.(error)
    },
  })
}
