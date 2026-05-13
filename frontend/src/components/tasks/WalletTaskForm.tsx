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

const walletSchema = z.object({
  walletAddress: z.string().min(10, "Invalid address")
});

interface WalletTaskFormProps {
  task: Task;
  onSuccess: () => void;
}

export function WalletTaskForm({ task, onSuccess }: WalletTaskFormProps) {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [isSubmitting, setIsSubmitting] = React.useState(false);

  const form = useForm({
    resolver: zodResolver(walletSchema),
    defaultValues: { walletAddress: '' }
  });

  const onSubmit = async (data: any) => {
    setIsSubmitting(true);
    try {
      const submissionData = JSON.stringify({ walletAddress: data.walletAddress });
      await submissionService.submitTask(task.id, {
        taskId: task.id,
        taskType: TaskType.WalletVerification,
        submissionData
      });
      await queryClient.invalidateQueries({ queryKey: ['task-submission-status', task.id] });
      await queryClient.invalidateQueries({ queryKey: ['my-submissions'] });
      toast({ title: 'Success', description: 'Wallet submitted for review', variant: 'default' });
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
          name="walletAddress"
          render={({ field }) => (
            <FormItem>
              <FormLabel className="text-lg font-semibold text-gray-900">Wallet Address</FormLabel>
              <FormControl>
                <Input 
                  {...field} 
                  placeholder="0x1234567890abcdef..." 
                  className="text-base p-6 border-2 focus:border-blue-400 rounded-lg font-mono"
                />
              </FormControl>
              <FormMessage />
              <p className="text-sm text-gray-500 mt-2">Enter your cryptocurrency wallet address for verification</p>
            </FormItem>
          )}
        />
        <Button 
          type="submit" 
          disabled={isSubmitting}
          className="w-full py-6 text-lg font-semibold bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700"
        >
          {isSubmitting ? 'Verifying...' : 'Verify Wallet'}
        </Button>
      </form>
    </Form>
  );
}
