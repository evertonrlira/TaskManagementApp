import { useForm } from 'react-hook-form';
import { useRef, useEffect } from 'react';
import { zodResolver } from '@hookform/resolvers/zod';
import { createTaskSchema, updateTaskSchema, type CreateTaskFormData, type UpdateTaskFormData } from '../../lib/validation';

interface TaskFormBodyProps {
  mode: 'create' | 'edit';
  defaultValues?: Partial<CreateTaskFormData | UpdateTaskFormData>;
  onSubmit: (data: CreateTaskFormData | UpdateTaskFormData) => Promise<void>;
  isSubmitting: boolean;
  formError?: string | null;
  children: React.ReactNode; // For action buttons
  userId?: string; // Only needed for create mode
}

export function TaskFormBody({ 
  mode, 
  defaultValues, 
  onSubmit, 
  isSubmitting, 
  formError,
  children,
  userId 
}: TaskFormBodyProps) {
  const titleInputRef = useRef<HTMLInputElement>(null);
  const isEditMode = mode === 'edit';

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors }
  } = useForm<CreateTaskFormData | UpdateTaskFormData>({
    resolver: zodResolver(isEditMode ? updateTaskSchema : createTaskSchema),
    defaultValues: defaultValues || (isEditMode ? {} : { title: '', description: '', userId: userId || '' })
  });

  const { ref: titleRef, ...titleRegister } = register('title');

  // Watch form values for character counting
  const titleValue = watch('title') || '';
  const descriptionValue = watch('description') || '';
  
  useEffect(() => {
    if (!isEditMode && userId) {
      setValue('userId', userId);
    }
  }, [userId, setValue, isEditMode]);

  // Focus title input after successful create (only for create mode)
  useEffect(() => {
    if (!isSubmitting && !isEditMode && !formError) {
      // Small delay to ensure form is reset
      const timer = setTimeout(() => {
        titleInputRef.current?.focus();
      }, 100);
      return () => clearTimeout(timer);
    }
  }, [isSubmitting, isEditMode, formError]);

  const handleFormSubmit = async (data: CreateTaskFormData | UpdateTaskFormData) => {
    try {
      await onSubmit(data);
      if (!isEditMode) {
        reset(); // Only reset for create mode
      }
    } catch (error) {
      // Error is handled by parent component
    }
  };

  return (
    <>
      {formError && (
        <div className="bg-destructive/15 text-destructive p-3 rounded-md mb-4" role="alert" aria-live="polite">
          {formError}
        </div>
      )}

      <form onSubmit={handleSubmit(handleFormSubmit)} noValidate>
        {/* Hidden input to ensure userId is included in form data for create mode */}
        {!isEditMode && <input type="hidden" {...register('userId')} />}
        
        <div className="space-y-4">
          <div>
            <label htmlFor="title" className="block text-sm font-medium mb-1">
              Title *
            </label>
            <input
              id="title"
              type="text"
              {...titleRegister}
              ref={(e) => {
                titleRef(e);
                titleInputRef.current = e;
              }}
              className="w-full px-3 py-2 border border-border rounded-md focus:outline-none focus:ring-2 focus:ring-ring focus:border-transparent bg-background text-foreground"
              placeholder="Enter task title..."
              disabled={isSubmitting}
              aria-describedby="title-hint title-error"
              aria-invalid={errors.title ? 'true' : 'false'}
              data-testid="task-title-input"
            />
            {errors.title && (
              <div id="title-error" className="text-destructive text-sm mt-1" role="alert" data-testid="title-error">
                {errors.title.message}
              </div>
            )}
            <div id="title-hint" className="text-xs text-muted-foreground mt-1 flex justify-between">
              <span>Required. Maximum 1024 characters allowed.</span>
              <span className={titleValue.length > 1024 ? 'text-destructive' : ''}>
                {titleValue.length}/1024
              </span>
            </div>
          </div>

          <div>
            <label htmlFor="description" className="block text-sm font-medium mb-1">
              Description
            </label>
            <textarea
              id="description"
              {...register('description')}
              rows={3}
              className="w-full px-3 py-2 border border-border rounded-md focus:outline-none focus:ring-2 focus:ring-ring focus:border-transparent bg-background text-foreground resize-vertical"
              placeholder="Enter task description (optional)..."
              disabled={isSubmitting}
              aria-describedby="description-hint description-error"
              aria-invalid={errors.description ? 'true' : 'false'}
              data-testid="task-description-input"
            />
            {errors.description && (
              <div id="description-error" className="text-destructive text-sm mt-1" role="alert" data-testid="description-error">
                {errors.description.message}
              </div>
            )}
            <div id="description-hint" className="text-xs text-muted-foreground mt-1 flex justify-between">
              <span>Optional. Maximum 4096 characters allowed.</span>
              <span className={descriptionValue.length > 4096 ? 'text-destructive' : ''}>
                {descriptionValue.length}/4096
              </span>
            </div>
          </div>

          {children}
        </div>
      </form>
    </>
  );
}