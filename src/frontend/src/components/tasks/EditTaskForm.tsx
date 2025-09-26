import { TaskFormBody } from './TaskFormBody';
import { useUpdateTask } from '../../lib/queries';
import type { Task } from '../../lib/types';
import type { CreateTaskFormData, UpdateTaskFormData } from '../../lib/validation';

interface EditTaskFormProps {
  task: Task;
  onTaskUpdated: () => void;
  onCancel: () => void;
}

export function EditTaskForm({ task, onTaskUpdated, onCancel }: EditTaskFormProps) {
  const updateTaskMutation = useUpdateTask();

  const handleSubmit = async (data: CreateTaskFormData | UpdateTaskFormData) => {
    const updateData = data as UpdateTaskFormData;
    await updateTaskMutation.mutateAsync({ id: task.id, data: updateData });
    onTaskUpdated();
  };

  const hasFormError = updateTaskMutation.isError;
  const formError = hasFormError ? 'Failed to update task. Please try again.' : null;

  const defaultValues = {
    title: task.title,
    description: task.description || ''
  };

  return (
    <section className="p-4 bg-card rounded-lg shadow border border-border dark:border-gray-600" aria-labelledby="edit-task-heading">
      <h2 id="edit-task-heading" className="text-xl font-semibold mb-4">
        Edit Task
      </h2>

      <TaskFormBody
        mode="edit"
        defaultValues={defaultValues}
        onSubmit={handleSubmit}
        isSubmitting={updateTaskMutation.isPending}
        formError={formError}
      >
        <div className="flex gap-2">
          <button
            type="button"
            onClick={onCancel}
            disabled={updateTaskMutation.isPending}
            data-testid="cancel-edit-button"
            className="px-4 py-2 bg-transparent border border-border text-muted-foreground rounded-md hover:bg-secondary/50 transition-colors disabled:opacity-50 flex items-center gap-2"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h10a8 8 0 018 8v2M3 10l6 6m-6-6l6-6" />
            </svg>
            Cancel
          </button>
          
          <button
            type="submit"
            disabled={updateTaskMutation.isPending}
            data-testid="update-task-button"
            className="flex-1 bg-secondary text-secondary-foreground py-2 rounded-md hover:bg-secondary/80 transition-colors disabled:opacity-50 border border-border flex items-center justify-center gap-2"
            aria-describedby="submit-button-hint"
          >
            {updateTaskMutation.isPending ? (
              <>
                <svg className="animate-spin w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg>
                <span aria-live="polite">Updating...</span>
              </>
            ) : (
              <>
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                </svg>
                Update Task
              </>
            )}
          </button>
        </div>
      </TaskFormBody>
    </section>
  );
}