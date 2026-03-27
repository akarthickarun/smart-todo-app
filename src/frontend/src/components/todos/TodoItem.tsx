import { useState } from 'react';
import type { TodoItem } from '@/features/todos/schemas/todoSchemas';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from '@/components/ui/dialog';

interface TodoItemProps {
  todo: TodoItem;
  onEdit: (todo: TodoItem) => void;
  onDelete: (todo: TodoItem) => void;
}

export function TodoItem({ todo, onEdit, onDelete }: TodoItemProps) {
  const [confirmOpen, setConfirmOpen] = useState(false);

  return (
    <>
      <Card className="mb-4">
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <span className={todo.status === 'Completed' ? 'line-through text-muted-foreground' : ''}>
                {todo.title}
              </span>
              <Badge variant={todo.status === 'Completed' ? 'success' : 'secondary'}>
                {todo.status}
              </Badge>
            </CardTitle>
            <div className="flex gap-2">
              <Button size="sm" variant="outline" onClick={() => onEdit(todo)}>
                Edit
              </Button>
              <Button size="sm" variant="destructive" onClick={() => setConfirmOpen(true)}>
                Delete
              </Button>
            </div>
          </div>
        </CardHeader>
        {(todo.description || todo.dueDate) && (
          <CardContent>
            {todo.description && (
              <p className="text-sm text-muted-foreground">{todo.description}</p>
            )}
            {todo.dueDate && (
              <span className="mt-2 block text-xs text-accent-foreground">
                Due: {new Date(todo.dueDate).toLocaleDateString()}
              </span>
            )}
          </CardContent>
        )}
      </Card>

      <Dialog open={confirmOpen} onOpenChange={setConfirmOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Todo</DialogTitle>
          </DialogHeader>
          <DialogDescription>
            Are you sure you want to delete "{todo.title}"? This action cannot be undone.
          </DialogDescription>
          <div className="flex justify-end gap-2 mt-4">
            <Button variant="outline" autoFocus onClick={() => setConfirmOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={() => {
                onDelete(todo);
                setConfirmOpen(false);
              }}
            >
              Delete
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}
