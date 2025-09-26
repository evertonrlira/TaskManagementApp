import { TaskFormBody } from './TaskFormBody';
import { useCreateTask } from '../../lib/queries';
import { useRequiredUser } from '../../contexts/UserProvider';
import type { CreateTaskFormData, UpdateTaskFormData } from '../../lib/validation';

interface CreateTaskFormProps {
  onTaskCreated: () => void;
}

export function CreateTaskForm({ onTaskCreated }: CreateTaskFormProps) {
  const { currentUserId } = useRequiredUser();
  const createTaskMutation = useCreateTask();

  const handleSubmit = async (data: CreateTaskFormData | UpdateTaskFormData) => {
    const createData = data as CreateTaskFormData;
    await createTaskMutation.mutateAsync(createData);
    onTaskCreated();
  };

  const hasFormError = createTaskMutation.isError;
  const formError = hasFormError ? 'Failed to create task. Please try again.' : null;

  return (
    <section className="p-4 bg-card rounded-lg shadow border border-border dark:border-gray-600" aria-labelledby="add-task-heading">
      <h2 id="add-task-heading" className="text-xl font-semibold mb-4">
        Add New Task
      </h2>

      <TaskFormBody
        mode="create"
        onSubmit={handleSubmit}
        isSubmitting={createTaskMutation.isPending}
        formError={formError}
        userId={currentUserId}
      >
        <button
          type="submit"
          disabled={createTaskMutation.isPending}
          data-testid="add-task-button"
          className="w-full bg-secondary text-secondary-foreground py-2 rounded-md hover:bg-secondary/80 transition-colors disabled:opacity-50 border border-border flex items-center justify-center gap-2"
          aria-describedby="submit-button-hint"
        >
          {createTaskMutation.isPending ? (
            <>
              <svg className="animate-spin w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
              </svg>
              <span aria-live="polite">Creating...</span>
            </>
          ) : (
            <>
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              Create Task
            </>
          )}
        </button>
      </TaskFormBody>
    </section>
  );
}