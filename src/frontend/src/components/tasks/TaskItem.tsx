import { useState, useRef, useEffect, memo } from 'react';
import type { Task } from '../../lib/types';
import { useToggleTaskCompletion, useDeleteTask } from '../../lib/queries';
import { Portal } from '../ui/Portal';
import { EditTaskForm } from './EditTaskForm';

interface TaskItemProps {
  task: Task;
  onStatusChange: () => void;
  onDelete?: () => void;
}

export const TaskItem = memo(function TaskItem({ task, onStatusChange, onDelete }: TaskItemProps) {
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [isEditing, setIsEditing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isTitleExpanded, setIsTitleExpanded] = useState(false);
  const [isDescriptionExpanded, setIsDescriptionExpanded] = useState(false);
  const [needsTitleTruncation, setNeedsTitleTruncation] = useState(false);
  const [needsDescriptionTruncation, setNeedsDescriptionTruncation] = useState(false);
  
  const titleRef = useRef<HTMLDivElement>(null);
  const descriptionRef = useRef<HTMLDivElement>(null);

  const toggleTaskMutation = useToggleTaskCompletion();
  const deleteTaskMutation = useDeleteTask();

  const isUpdating = toggleTaskMutation.isPending;
  const isDeleting = deleteTaskMutation.isPending;

  // Format date to a more readable format
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  // Get status display text and styling based on completion date
  const getStatusDisplay = () => {
    const isCompleted = task.completedAt !== null;
    return {
      text: isCompleted ? 'COMPLETED' : 'TO-DO',
      styles: isCompleted 
        ? 'bg-success/10 text-success border border-success/20' 
        : 'bg-primary/10 text-primary border border-primary/20'
    };
  };

  // Get task card styling based on completion status
  const getCardStyles = () => {
    const isCompleted = task.completedAt !== null;
    return isCompleted
      ? 'p-4 bg-card rounded-lg shadow border border-border dark:border-gray-600 opacity-75 bg-gray-50 dark:bg-gray-800/50'
      : 'p-4 bg-card rounded-lg shadow border border-border dark:border-gray-600';
  };

  // Get title styling based on completion status
  const getTitleStyles = () => {
    const isCompleted = task.completedAt !== null;
    const baseStyles = 'font-semibold text-lg break-words';
    return isCompleted
      ? `${baseStyles} line-through text-muted-foreground`
      : baseStyles;
  };

  // Check if text is overflowing and needs truncation
  const checkTextOverflow = () => {
    if (titleRef.current) {
      const element = titleRef.current;
      const isOverflowing = element.scrollHeight > element.clientHeight;
      setNeedsTitleTruncation(isOverflowing);
    }
    
    if (descriptionRef.current) {
      const element = descriptionRef.current;
      const isOverflowing = element.scrollHeight > element.clientHeight;
      setNeedsDescriptionTruncation(isOverflowing);
    }
  };

  // Check overflow on mount and window resize
  useEffect(() => {
    checkTextOverflow();
    
    const handleResize = () => {
      checkTextOverflow();
    };
    
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, [task.title, task.description]);

  // Also check after font loading (if applicable)
  useEffect(() => {
    const timer = setTimeout(checkTextOverflow, 100);
    return () => clearTimeout(timer);
  }, [task.title, task.description]);

  // Render expandable text component
  const renderExpandableText = (
    text: string,
    isExpanded: boolean,
    setIsExpanded: (expanded: boolean) => void,
    truncateClass: string,
    needsTruncation: boolean,
    ref: React.RefObject<HTMLDivElement | null>,
    testId?: string
  ) => {
    return (
      <div className="relative">
        <div 
          ref={ref}
          className={isExpanded ? '' : truncateClass}
          aria-expanded={needsTruncation ? isExpanded : undefined}
          data-testid={testId}
        >
          {text}
        </div>
        {needsTruncation && (
          <button
            onClick={() => setIsExpanded(!isExpanded)}
            className="text-primary hover:text-primary/80 text-xs mt-1 font-medium"
            aria-expanded={isExpanded}
            aria-label={isExpanded ? 'Show less text' : 'Show more text'}
          >
            {isExpanded ? '- Show less' : '+ Show more'}
          </button>
        )}
      </div>
    );
  };

  // Handle status update
  const handleStatusUpdate = async () => {
    setError(null);

    try {
      await toggleTaskMutation.mutateAsync({ id: task.id });
      onStatusChange(); // Trigger refresh in parent component
    } catch (err) {
      console.error('Error updating task status:', err);
      setError('Failed to update status');
    }
  };

  // Handle delete confirmation
  const handleDeleteClick = () => {
    setShowDeleteConfirm(true);
  };

  // Handle delete cancellation
  const handleDeleteCancel = () => {
    setShowDeleteConfirm(false);
  };

  // Handle delete confirmation
  const handleDeleteConfirm = async () => {
    setError(null);

    try {
      await deleteTaskMutation.mutateAsync(task.id);
      setShowDeleteConfirm(false);
      if (onDelete) {
        onDelete(); // Trigger refresh in parent component
      } else {
        onStatusChange(); // Fallback to status change handler
      }
    } catch (err) {
      console.error('Error deleting task:', err);
      setError('Failed to delete task');
      setShowDeleteConfirm(false);
    }
  };

  // Handle edit mode
  const handleEditClick = () => {
    setIsEditing(true);
  };

  const handleEditCancel = () => {
    setIsEditing(false);
  };

  const handleEditSuccess = () => {
    setIsEditing(false);
    onStatusChange(); // Refresh the task list
  };

  // Handle escape key to close modal
  useEffect(() => {
    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && showDeleteConfirm) {
        handleDeleteCancel();
      }
    };

    if (showDeleteConfirm) {
      document.addEventListener('keydown', handleEscape);
      // Focus trap - focus first button
      const modalElement = document.querySelector('[role="dialog"]');
      const firstButton = modalElement?.querySelector('button');
      firstButton?.focus();
    }

    return () => {
      document.removeEventListener('keydown', handleEscape);
    };
  }, [showDeleteConfirm]);

  return (
    <article className={getCardStyles()} role="article" aria-labelledby={`task-title-${task.id}`} data-testid="task-item">
      {isEditing ? (
        <div className="p-2">
          <EditTaskForm 
            task={task}
            onTaskUpdated={handleEditSuccess}
            onCancel={handleEditCancel}
          />
        </div>
      ) : (
        <div className="flex flex-col gap-2">
        <div className="flex justify-between items-start gap-3">
          <div className="flex-1 min-w-0 flex items-start gap-2">
            {/* Completion status icon */}
            <div className="flex-shrink-0 mt-1">
              {task.completedAt ? (
                <div 
                  className="w-5 h-5 rounded-full bg-success flex items-center justify-center"
                  aria-label="Task completed"
                  role="img"
                >
                  <svg className="w-3 h-3 text-success-foreground" fill="currentColor" viewBox="0 0 20 20" aria-hidden="true">
                    <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                  </svg>
                </div>
              ) : (
                <div 
                  className="w-5 h-5 rounded-full border-2 border-muted-foreground/40"
                  aria-label="Task pending"
                  role="img"
                ></div>
              )}
            </div>
            
            <div className="flex-1 min-w-0">
              <h3 id={`task-title-${task.id}`} className="sr-only">Task: {task.title}</h3>
              {renderExpandableText(
                task.title,
                isTitleExpanded,
                setIsTitleExpanded,
                `line-clamp-1 ${getTitleStyles()}`,
                needsTitleTruncation,
                titleRef,
                'task-title'
              )}
            </div>
          </div>
          <span 
            className={`text-xs px-2 py-1 rounded-full whitespace-nowrap flex-shrink-0 ${getStatusDisplay().styles}`}
            role="status"
            aria-label={`Task status: ${getStatusDisplay().text}`}
          >
            {getStatusDisplay().text}
          </span>
        </div>
        
        {task.description && (
          <div className={`${task.completedAt ? 'text-muted-foreground/70' : 'text-muted-foreground'}`}>
            {renderExpandableText(
              task.description,
              isDescriptionExpanded,
              setIsDescriptionExpanded,
              'line-clamp-2 break-words',
              needsDescriptionTruncation,
              descriptionRef,
              'task-description'
            )}
          </div>
        )}

          <div className="flex justify-between items-center">
            <div className="text-xs text-muted-foreground">
              Created: {formatDate(task.createdAt)}
              {task.completedAt && (
                <> • Completed: {formatDate(task.completedAt)}</>
              )}
            </div>
            <div className="flex gap-2 ml-4">
              <button
                onClick={handleDeleteClick}
                disabled={isDeleting}
                data-testid="delete-task-button"
                className="bg-destructive/90 text-destructive-foreground hover:bg-destructive px-3 py-1 rounded-md text-sm transition-colors flex items-center gap-1"
                title="Delete task"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
                {isDeleting ? 'Deleting...' : 'Delete'}
              </button>
              <button
                onClick={handleEditClick}
                disabled={isUpdating || isDeleting}
                data-testid="edit-task-button"
                className="bg-blue-500/90 text-white hover:bg-blue-500 px-3 py-1 rounded-md text-sm transition-colors flex items-center gap-1"
                title="Edit task"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                </svg>
                Edit
              </button>
              <button
                onClick={handleStatusUpdate}
                disabled={isUpdating}
                data-testid="toggle-completion-button"
                className="bg-secondary text-secondary-foreground hover:bg-secondary/80 px-3 py-1 rounded-md text-sm transition-colors disabled:opacity-50 border border-border flex items-center gap-2"
              >
                {isUpdating ? (
                  <>
                    <svg className="animate-spin w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                    </svg>
                    Updating...
                  </>
                ) : task.completedAt === null ? (
                  <>
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                    </svg>
                    Complete Task
                  </>
                ) : (
                  <>
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h10a8 8 0 018 8v2M3 10l6 6m-6-6l6-6" />
                    </svg>
                    Mark as To-Do
                  </>
                )}
              </button>
            </div>
          </div>
          {error && (
            <div className="text-xs text-destructive">{error}</div>
          )}
          
          {/* Delete Confirmation Dialog */}
          {showDeleteConfirm && (
            <Portal>
              <div className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-[9999]">
                <div className="bg-card p-6 rounded-lg shadow-xl max-w-md w-full mx-4 relative z-[10000] border border-border">
                  <h3 className="text-lg font-semibold mb-4 text-card-foreground">
                    Confirm Delete
                  </h3>
                  <p className="text-muted-foreground">
                    Are you sure you want to delete this task?
                  </p>
                  <p className="text-muted-foreground mb-6 underline">
                    This action cannot be undone
                  </p>
                  <div className="flex justify-end gap-3">
                    <button
                      onClick={handleDeleteCancel}
                      className="px-4 py-2 text-sm bg-secondary text-secondary-foreground border border-border rounded-md hover:bg-secondary/80 transition-colors"
                    >
                      Cancel
                    </button>
                    <button
                      onClick={handleDeleteConfirm}
                      disabled={isDeleting}
                      className="px-4 py-2 text-sm bg-destructive text-destructive-foreground rounded-md hover:bg-destructive/90 disabled:opacity-50 transition-colors"
                    >
                      {isDeleting ? 'Deleting...' : 'Delete'}
                    </button>
                  </div>
                </div>
              </div>
            </Portal>
          )}
        </div>
      )}
    </article>
  );
});
