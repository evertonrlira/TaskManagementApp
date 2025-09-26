import { useState, useEffect } from 'react';
import { useTasks } from '../../lib/queries';
import { TaskItem } from './TaskItem';
import { TaskListSkeleton } from '../ui/Loading';
import { PaginationControlsBasic } from '../ui/PaginationControlsBasic';
import { PaginationControlsEnhanced } from '../ui/PaginationControlsEnhanced';
import { TaskStatistics } from './TaskStatistics';
import { useRequiredUser } from '../../contexts/UserProvider';

export function TaskList() {
  const { currentUserId } = useRequiredUser();
  const [currentPage, setCurrentPage] = useState(1);
  const { data: tasksResponse, isLoading, error, refetch } = useTasks(currentUserId, currentPage);

  // Reset pagination when user changes
  useEffect(() => {
    setCurrentPage(1);
  }, [currentUserId]);

  // Function to refresh the task list
  const refreshTasks = () => {
    refetch();
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    // The query will automatically refetch when currentPage changes due to the queryKey dependency
  };

  if (isLoading) {
    return <TaskListSkeleton />;
  }

  if (error) {
    return (
      <div className="bg-destructive/15 text-destructive p-4 rounded-lg" role="alert" data-testid="error-message">
        <h3 className="font-semibold mb-2">Error loading tasks</h3>
        <p className="mb-3">{(error as Error).message}</p>
        <button 
          onClick={() => refetch()}
          className="bg-destructive text-destructive-foreground px-4 py-2 rounded hover:bg-destructive/80"
        >
          Try Again
        </button>
      </div>
    );
  }

  const tasks = tasksResponse?.items || [];
  const pagination = tasksResponse ? {
    pageNumber: currentPage, // Use the state for current page
    pageSize: tasksResponse.pageSize,
    totalCount: tasksResponse.totalCount,
    totalPages: tasksResponse.totalPages,
    hasNextPage: tasksResponse.hasNextPage,
    hasPreviousPage: tasksResponse.hasPreviousPage
  } : null;

  if (tasks.length === 0 && !isLoading) {
    return (
      <div className="p-8 text-center text-muted-foreground">
        <p>No tasks found. Create your first task using the form.</p>
      </div>
    );
  }

  return (
    <div data-testid="task-list">
      {/* Header and top pagination */}
      <div className="space-y-4 mb-2">
        <div className="flex justify-between items-center">
          <h2 className="text-xl font-semibold">Your Tasks</h2>
          {pagination && pagination.totalPages > 1 && (
            <PaginationControlsBasic
              pagination={pagination}
              onPageChange={handlePageChange}
            />
          )}
          {pagination && (
            <TaskStatistics
              userId={currentUserId}
            />
          )}
        </div>
      </div>

      {/* Task list */}
      <div className="space-y-4 mb-6 transition-opacity duration-300 ease-in-out opacity-100 animate-fadeIn">
        {tasks.map(task => (
          <TaskItem
            key={task.id}
            task={task}
            onStatusChange={refreshTasks}
            onDelete={refreshTasks}
          />
        ))}
      </div>

      {/* Bottom pagination - full version */}
      {pagination && pagination.totalPages > 1 && (
        <PaginationControlsEnhanced
          pagination={pagination}
          onPageChange={handlePageChange}
        />
      )}
    </div>
  );
}
