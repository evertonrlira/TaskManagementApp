import { useTaskStatistics } from '../../lib/queries';

interface TaskStatisticsProps {
  userId: string;
}

export function TaskStatistics({ userId }: TaskStatisticsProps) {
  const { data: statistics, isLoading, error } = useTaskStatistics(userId);

  if (error) {
    return (
      <div className="flex flex-col items-center text-sm text-destructive">
        <span>Unable to load statistics</span>
      </div>
    );
  }

  if (isLoading || !statistics) {
    return (
      <div className="flex flex-col items-center text-sm">
        <div className="animate-pulse">
          <div className="space-y-1">
            <div className="h-4 bg-muted rounded w-24"></div>
            <div className="h-4 bg-muted rounded w-28"></div>
            <div className="h-4 bg-muted rounded w-20"></div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="flex flex-col items-center text-sm" data-testid="task-statistics">
      <div className="grid grid-cols-3 gap-4 text-center">
        {/* Pending Tasks */}
        <div className="flex flex-col items-center" data-testid="pending-tasks-stat">
          <div className="w-8 h-8 rounded-full bg-warning/20 flex items-center justify-center mb-1">
            <div className="w-3 h-3 rounded-full bg-warning"></div>
          </div>
          <span className="text-xs text-muted-foreground">Pending</span>
          <span className="font-bold text-warning" data-testid="pending-tasks-count">{statistics.pendingTasks}</span>
        </div>

        {/* Completed Tasks */}
        <div className="flex flex-col items-center" data-testid="completed-tasks-stat">
          <div className="w-8 h-8 rounded-full bg-success/20 flex items-center justify-center mb-1">
            <svg className="w-3 h-3 text-success" fill="currentColor" viewBox="0 0 20 20" aria-hidden="true">
              <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
            </svg>
          </div>
          <span className="text-xs text-muted-foreground">Completed</span>
          <span className="font-bold text-success" data-testid="completed-tasks-count">{statistics.completedTasks}</span>
        </div>

        {/* Total Tasks */}
        <div className="flex flex-col items-center" data-testid="total-tasks-stat">
          <div className="w-8 h-8 rounded-full bg-primary/20 flex items-center justify-center mb-1">
            <svg className="w-3 h-3 text-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
            </svg>
          </div>
          <span className="text-xs text-muted-foreground">Total</span>
          <span className="font-bold text-primary" data-testid="total-tasks-count">{statistics.totalTasks}</span>
        </div>
      </div>
    </div>
  );
}