import { useUsers } from '../../lib/queries';

interface UserSelectProps {
  value: string | null;
  onChange: (userId: string | null) => void;
}

export function UserSelect({ value, onChange }: UserSelectProps) {
  const { data: users = [], isLoading, error, refetch } = useUsers();

  const handleUserChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const userId = event.target.value;
    onChange(userId || null);
  };

  const handleRetry = () => {
    refetch();
  };

  if (error) {
    return (
      <div className="flex items-center gap-2">
        <div className="text-destructive text-sm">
          {(error as Error)?.message?.includes('fetch') || (error as Error)?.message?.includes('Network') 
            ? 'Backend unavailable' 
            : 'Failed to load users'}
        </div>
        <button
          onClick={handleRetry}
          className="text-xs bg-secondary hover:bg-secondary/80 text-secondary-foreground px-2 py-1 rounded transition-colors"
          title="Retry loading users"
        >
          Retry
        </button>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="flex items-center gap-2">
        <div className="animate-spin w-4 h-4 border-2 border-primary border-t-transparent rounded-full"></div>
        <span className="text-sm text-muted-foreground">Loading users...</span>
      </div>
    );
  }

  return (
    <select
      value={value ?? ''}
      onChange={handleUserChange}
      data-testid="user-select"
      className="bg-background text-foreground border border-input dark:border-gray-600 rounded px-3 py-2 text-sm hover:border-input focus:ring-2 focus:ring-ring focus:outline-none transition-colors duration-200"
    >
      <option value="">Select User</option>
      {users.map(user => (
        <option key={user.id} value={user.id}>
          {user.name}
        </option>
      ))}
    </select>
  );
}
