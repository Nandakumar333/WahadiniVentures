import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { submissionService } from '@/services/api/submissionService';
import { TaskType } from '@/types/task';
import type { Task } from '@/types/task';
import { useToast } from '@/components/ui/use-toast';

const screenshotSchema = z.object({
  file: z.instanceof(FileList).refine((files) => files.length === 1, "File required")
        .refine((files) => files[0]?.size <= 5 * 1024 * 1024, "Max 5MB")
        .refine((files) => ['image/jpeg', 'image/png', 'image/gif'].includes(files[0]?.type), "Images only")
});

interface ScreenshotTaskFormProps {
  task: Task;
  onSuccess: () => void;
}

export function ScreenshotTaskForm({ task, onSuccess }: ScreenshotTaskFormProps) {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [isSubmitting, setIsSubmitting] = React.useState(false);

  const form = useForm({
    resolver: zodResolver(screenshotSchema),
  });

  const onSubmit = async (data: any) => {
    setIsSubmitting(true);
    try {
      const file = data.file[0];
      // We pass the File object directly; service/client must handle FormData conversion
      // Assuming submissionService needs update or special handling for files
      
      // Actually submissionService.submitTask takes TaskSubmissionRequest
      // We need to update submissionService to handle FormData if it's multipart
      // OR assume client handles it.
      // The current submissionService uses JSON. We need to adapt it or the service.
      
      // Let's invoke a specific method for file upload if needed, or construct FormData here.
      // But `submissionService.submitTask` signature expects `TaskSubmissionRequest` object.
      // We can extend it to accept `file`.
      
      await submissionService.submitTaskWithFile(task.id, {
        taskId: task.id,
        taskType: TaskType.Screenshot,
        submissionData: "" // Will be ignored/filled by backend
      }, file);

      await queryClient.invalidateQueries({ queryKey: ['task-submission-status', task.id] });
      await queryClient.invalidateQueries({ queryKey: ['my-submissions'] });
      toast({ title: 'Success', description: 'Screenshot uploaded', variant: 'default' });
      onSuccess();
    } catch (error: any) {
      const errorMessage = error?.response?.data?.message || error?.message || 'Upload failed';
      toast({ title: 'Error', description: errorMessage, variant: 'destructive' });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <FormField
          control={form.control}
          name="file"
          render={({ field: { onChange, value, ...field } }) => (
            <FormItem>
              <FormLabel className="text-lg font-semibold text-gray-900">Upload Screenshot</FormLabel>
              <FormControl>
                <div className="border-2 border-dashed border-gray-300 rounded-lg p-8 hover:border-blue-400 transition-colors">
                  <Input 
                    type="file" 
                    accept="image/*"
                    onChange={(e) => onChange(e.target.files)}
                    className="cursor-pointer"
                    {...field} 
                  />
                </div>
              </FormControl>
              <FormMessage />
              <p className="text-sm text-gray-500 mt-2">Accepted formats: JPG, PNG, GIF • Max size: 5MB</p>
            </FormItem>
          )}
        />
        <Button 
          type="submit" 
          disabled={isSubmitting}
          className="w-full py-6 text-lg font-semibold bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700"
        >
          {isSubmitting ? 'Uploading...' : 'Upload & Submit'}
        </Button>
      </form>
    </Form>
  );
}
