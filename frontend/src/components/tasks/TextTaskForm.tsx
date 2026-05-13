import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { Textarea } from '@/components/ui/textarea'; // Assuming Textarea exists
import { submissionService } from '@/services/api/submissionService';
import { TaskType } from '@/types/task';
import type { Task } from '@/types/task';
import { useToast } from '@/components/ui/use-toast';

const textSchema = z.object({
  text: z.string().min(10, "Response too short")
});

interface TextTaskFormProps {
  task: Task;
  onSuccess: () => void;
}

export function TextTaskForm({ task, onSuccess }: TextTaskFormProps) {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [isSubmitting, setIsSubmitting] = React.useState(false);

  const form = useForm({
    resolver: zodResolver(textSchema),
    defaultValues: { text: '' }
  });

  const onSubmit = async (data: any) => {
    setIsSubmitting(true);
    try {
      const submissionData = JSON.stringify({ text: data.text });
      await submissionService.submitTask(task.id, {
        taskId: task.id,
        taskType: TaskType.TextSubmission,
        submissionData
      });
      await queryClient.invalidateQueries({ queryKey: ['task-submission-status', task.id] });
      await queryClient.invalidateQueries({ queryKey: ['my-submissions'] });
      toast({ title: 'Success', description: 'Response submitted for review', variant: 'default' });
      onSuccess();
    } catch (error: any) {
      const errorMessage = error?.response?.data?.message || error?.message || 'Failed to submit';
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
          name="text"
          render={({ field }) => (
            <FormItem>
              <FormLabel className="text-lg font-semibold text-gray-900">Your Response</FormLabel>
              <FormControl>
                <Textarea 
                  {...field} 
                  rows={8} 
                  className="resize-none text-base p-4 border-2 focus:border-blue-400 rounded-lg"
                  placeholder="Enter your detailed response here..."
                />
              </FormControl>
              <FormMessage />
              <p className="text-sm text-gray-500 mt-2">Minimum 10 characters required</p>
            </FormItem>
          )}
        />
        <Button 
          type="submit" 
          disabled={isSubmitting}
          className="w-full py-6 text-lg font-semibold bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700"
        >
          {isSubmitting ? 'Submitting...' : 'Submit Response'}
        </Button>
      </form>
    </Form>
  );
}
