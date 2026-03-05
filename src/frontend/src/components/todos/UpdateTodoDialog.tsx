import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { updateTodoSchema } from '@/features/todos/schemas/todoSchemas';
import type { UpdateTodoInput, TodoItem } from '@/features/todos/schemas/todoSchemas';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Form, FormControl, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { Controller } from 'react-hook-form';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { useUpdateTodo } from '@/features/todos/hooks/useUpdateTodo';
import { useEffect } from 'react';

interface UpdateTodoDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  todo: TodoItem | null;
}

export function UpdateTodoDialog({ open, onOpenChange, todo }: UpdateTodoDialogProps) {
  const { mutate: updateTodo, isPending } = useUpdateTodo();

  const form = useForm<UpdateTodoInput>({
    resolver: zodResolver(updateTodoSchema),
    defaultValues: {
      title: '',
      description: '',
      dueDate: '',
    },
  });

  useEffect(() => {
    if (todo) {
      form.reset({
        title: todo.title,
        description: todo.description ?? '',
        dueDate: todo.dueDate ? todo.dueDate.split('T')[0] : '',
      });
    }
  }, [todo, form]);

  const onSubmit = (data: UpdateTodoInput) => {
    if (!todo) return;
    updateTodo({ id: todo.id, data }, {
      onSuccess: () => {
        onOpenChange(false);
      },
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Edit Todo</DialogTitle>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormItem>
              <FormLabel>Title</FormLabel>
              <FormControl>
                <Controller
                  name="title"
                  control={form.control}
                  render={({ field }) => (
                    <Input placeholder="Enter todo title" {...field} value={field.value ?? ''} />
                  )}
                />
              </FormControl>
              <FormMessage>{form.formState.errors.title?.message as string}</FormMessage>
            </FormItem>
            <FormItem>
              <FormLabel>Description</FormLabel>
              <FormControl>
                <Controller
                  name="description"
                  control={form.control}
                  render={({ field }) => (
                    <Textarea placeholder="Enter description (optional)" {...field} value={field.value ?? ''} />
                  )}
                />
              </FormControl>
              <FormMessage>{form.formState.errors.description?.message as string}</FormMessage>
            </FormItem>
            <FormItem>
              <FormLabel>Due Date</FormLabel>
              <FormControl>
                <Controller
                  name="dueDate"
                  control={form.control}
                  render={({ field }) => (
                    <Input type="date" {...field} value={field.value ?? ''} />
                  )}
                />
              </FormControl>
              <FormMessage>{form.formState.errors.dueDate?.message as string}</FormMessage>
            </FormItem>
            <Button type="submit" disabled={isPending}>
              {isPending ? 'Saving...' : 'Save Changes'}
            </Button>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
