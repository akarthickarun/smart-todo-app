import { z } from 'zod'

// Status enum matching backend TodoStatus
export const todoStatusEnum = z.enum(['Pending', 'Completed'])
export type TodoStatus = z.infer<typeof todoStatusEnum>

// Response schema matching backend TodoItemDto
export const todoItemSchema = z.object({
  id: z.string().uuid(),
  title: z.string(),
  description: z.string().nullable(),
  status: todoStatusEnum,
  dueDate: z.string().date().nullable(),
  createdAt: z.string().datetime(),
  updatedAt: z.string().datetime(),
})

// Create input schema with validation rules
export const createTodoSchema = z.object({
  title: z
    .string()
    .min(3, 'Title must be at least 3 characters')
    .max(200, 'Title must not exceed 200 characters'),
  description: z
    .string()
    .max(1000, 'Description must not exceed 1000 characters')
    .nullable()
    .optional(),
  dueDate: z.string().date().nullable().optional(),
})

// Update input schema with validation rules
export const updateTodoSchema = z.object({
  title: z
    .string()
    .min(3, 'Title must be at least 3 characters')
    .max(200, 'Title must not exceed 200 characters'),
  description: z
    .string()
    .max(1000, 'Description must not exceed 1000 characters')
    .nullable()
    .optional(),
  dueDate: z.string().date().nullable().optional(),
})

export type TodoItem = z.infer<typeof todoItemSchema>
export type CreateTodoInput = z.infer<typeof createTodoSchema>
export type UpdateTodoInput = z.infer<typeof updateTodoSchema>
