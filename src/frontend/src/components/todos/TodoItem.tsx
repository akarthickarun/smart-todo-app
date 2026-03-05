import type { TodoItem } from '@/features/todos/schemas/todoSchemas';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';

interface TodoItemProps {
  todo: TodoItem;
  onEdit: (todo: TodoItem) => void;
  onDelete: (todo: TodoItem) => void;
}

export function TodoItem({ todo, onEdit, onDelete }: TodoItemProps) {
  return (
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
            <Button size="sm" variant="destructive" onClick={() => onDelete(todo)}>
              Delete
            </Button>
          </div>
        </div>
      </CardHeader>
      {todo.description && (
        <CardContent>
          <p className="text-sm text-muted-foreground">{todo.description}</p>
        </CardContent>
      )}
      {todo.dueDate && (
        <CardContent>
          <span className="text-xs text-accent-foreground">Due: {new Date(todo.dueDate).toLocaleDateString()}</span>
        </CardContent>
      )}
    </Card>
  );
}
