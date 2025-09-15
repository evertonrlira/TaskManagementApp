import { z } from 'zod';

// Task validation schemas - Google Tasks compatible limits
export const createTaskSchema = z.object({
  title: z.string()
    .min(1, 'Title is required')
    .refine(val => val.trim().length > 0, 'Title cannot contain only whitespace')
    .refine(val => val.trim().length <= 1024, 'Title cannot exceed 1024 characters')
    .transform(val => val.trim()),
  description: z.string()
    .max(4096, 'Description cannot exceed 4096 characters')
    .transform(val => val && val.trim() !== '' ? val.trim() : undefined)
    .optional(),
  userId: z.string().min(1, 'User ID is required'),
});

export const updateTaskSchema = z.object({
  title: z.string()
    .min(1, 'Title is required')
    .refine(val => val.trim().length > 0, 'Title cannot contain only whitespace')
    .refine(val => val.trim().length <= 1024, 'Title cannot exceed 1024 characters')
    .transform(val => val.trim()),
  description: z.string()
    .max(4096, 'Description cannot exceed 4096 characters')
    .transform(val => val && val.trim() !== '' ? val.trim() : undefined)
    .optional(),
});


// User validation schemas
export const userSelectSchema = z.object({
  id: z.string().min(1, 'User ID is required'),
  name: z.string().min(1, 'User name is required'),
});

// Type exports
export type CreateTaskFormData = z.infer<typeof createTaskSchema>;
export type UpdateTaskFormData = z.infer<typeof updateTaskSchema>;
export type UserSelectData = z.infer<typeof userSelectSchema>;
