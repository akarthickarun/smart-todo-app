import type { TodoItem, TodoStatus } from '@/features/todos/schemas/todoSchemas';
import { TodoItem as TodoItemCard } from './TodoItem';
import { Button } from '@/components/ui/button';

interface TodoListProps {
  todos: TodoItem[];
  onEdit: (todo: TodoItem) => void;
  onDelete: (todo: TodoItem) => void;
  onStatusFilter: (status: TodoStatus | null) => void;
  statusFilter: TodoStatus | null;
  loading?: boolean;
}

const statusFilters: { label: string; value: TodoStatus | null }[] = [
  { label: 'All', value: null },
  { label: 'Pending', value: 'Pending' },
  { label: 'Completed', value: 'Completed' },
];

export function TodoList({
  todos,
  onEdit,
  onDelete,
  onStatusFilter,
  statusFilter,
  loading,
}: TodoListProps) {
  return (
    <div>
      <div className="flex gap-2 mb-4">
        {statusFilters.map((filter) => (
          <Button
            key={filter.label}
            variant={statusFilter === filter.value ? 'default' : 'outline'}
            onClick={() => onStatusFilter(filter.value)}
            size="sm"
          >
            {filter.label}
          </Button>
        ))}
      </div>
      {loading ? (
        <div className="text-center py-8">Loading...</div>
      ) : todos.length === 0 ? (
        <div className="text-center py-8 text-muted-foreground">No todos found.</div>
      ) : (
        <div>
          {todos.map((todo) => (
            <TodoItemCard
              key={todo.id}
              todo={todo}
              onEdit={onEdit}
              onDelete={onDelete}
            />
          ))}
        </div>
      )}
    </div>
  );
}
