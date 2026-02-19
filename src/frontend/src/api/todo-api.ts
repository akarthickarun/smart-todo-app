import apiClient from '@/api/axios-instance'
import type {
  TodoItem,
  CreateTodoInput,
  UpdateTodoInput,
} from '@/features/todos/schemas/todoSchemas'
import { todoItemSchema } from '@/features/todos/schemas/todoSchemas'
import { z } from 'zod'

export const todoApi = {
  getAll: async (status?: string): Promise<TodoItem[]> => {
    const params = status !== undefined ? { status } : {}
    const response = await apiClient.get('/todoitems', { params })
    const data = z.array(todoItemSchema).parse(response.data)
    return data
  },

  getById: async (id: string): Promise<TodoItem> => {
    const response = await apiClient.get(`/todoitems/${id}`)
    return todoItemSchema.parse(response.data)
  },

  create: async (data: CreateTodoInput): Promise<string> => {
    const response = await apiClient.post<string>('/todoitems', data)
    return response.data
  },

  update: async (id: string, data: UpdateTodoInput): Promise<void> => {
    await apiClient.put(`/todoitems/${id}`, data)
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/todoitems/${id}`)
  },

  toggleComplete: async (id: string): Promise<void> => {
    await apiClient.patch(`/todoitems/${id}/complete`)
  },
}
