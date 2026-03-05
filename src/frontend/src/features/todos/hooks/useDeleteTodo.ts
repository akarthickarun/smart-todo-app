import { useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { todoApi } from '@/api/todo-api'

interface UseDeleteTodoOptions {
  onSuccess?: () => void
  onError?: (error: Error) => void
}

/**
 * Hook to delete a todo
 * @param options - Optional callbacks for success/error
 * @returns Mutation result with delete function
 */
export function useDeleteTodo(options?: UseDeleteTodoOptions) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => todoApi.delete(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['todos'] })
      queryClient.invalidateQueries({ queryKey: ['todos', id] })
      toast.success('Todo deleted successfully')
      options?.onSuccess?.()
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to delete todo')
      options?.onError?.(error)
    },
  })
}
