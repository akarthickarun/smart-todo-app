import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { createTodoSchema } from '@/features/todos/schemas/todoSchemas';
import type { CreateTodoInput } from '@/features/todos/schemas/todoSchemas';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Form, FormControl, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { Controller } from 'react-hook-form';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { useCreateTodo } from '@/features/todos/hooks/useCreateTodo';

export function CreateTodoDialog() {
  const { mutate: createTodo, isPending } = useCreateTodo();

  const form = useForm<CreateTodoInput>({
    resolver: zodResolver(createTodoSchema),
    defaultValues: {
      title: '',
      description: '',
      dueDate: '',
    },
  });

  const onSubmit = (data: CreateTodoInput) => {
    createTodo(data, {
      onSuccess: () => {
        form.reset();
      },
    });
  };

  return (
    <Dialog>
      <DialogTrigger>
        <Button>Create Todo</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Create New Todo</DialogTitle>
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
              {isPending ? 'Creating...' : 'Create'}
            </Button>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
