import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { submissionService } from '@/services/api/submissionService';
import { TaskType, type TaskSubmissionResponse } from '@/types/task';
import type { Task } from '@/types/task';
import { useToast } from '@/components/ui/use-toast';

const quizSchema = z.object({
  answers: z.record(z.string(), z.number())
});

interface QuizTaskFormProps {
  task: Task;
  onSuccess: () => void;
}

export function QuizTaskForm({ task, onSuccess }: QuizTaskFormProps) {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [isSubmitting, setIsSubmitting] = React.useState(false);
  const taskData = typeof task.taskData === 'string' ? JSON.parse(task.taskData) : task.taskData;
  const questions = taskData.questions || [];

  const form = useForm({
    resolver: zodResolver(quizSchema),
    defaultValues: {
      answers: {}
    }
  });

  const onSubmit = async (data: any) => {
    setIsSubmitting(true);
    try {
      // Convert string keys "0", "1" to numbers for DTO
      const formattedAnswers: Record<number, number> = {};
      Object.keys(data.answers).forEach(key => {
        formattedAnswers[parseInt(key)] = data.answers[key];
      });

      const submissionData = JSON.stringify({ answers: formattedAnswers });
      
      const result: TaskSubmissionResponse = await submissionService.submitTask(task.id, {
        taskId: task.id,
        taskType: TaskType.Quiz,
        submissionData
      }) as TaskSubmissionResponse;

      // Invalidate queries to refresh UI
      await queryClient.invalidateQueries({ queryKey: ['task-submission-status', task.id] });
      await queryClient.invalidateQueries({ queryKey: ['my-submissions'] });

      if (result?.status === 'Approved') {
        toast({ title: 'Success', description: `Quiz passed! +${result.pointsAwarded} points`, variant: 'default' });
      } else if (result?.status === 'Rejected') {
        toast({ title: 'Failed', description: result?.message || 'Quiz failed. Please try again.', variant: 'destructive' });
      } else {
        toast({ title: 'Submitted', description: result?.message || 'Quiz submitted for review', variant: 'default' });
      }
      onSuccess();
    } catch (error: any) {
      const errorMessage = error?.response?.data?.message || error?.message || 'Failed to submit quiz';
      toast({ title: 'Error', description: errorMessage, variant: 'destructive' });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8">
        <div className="space-y-6">
          {questions.map((q: any, index: number) => (
            <FormField
              key={index}
              control={form.control}
              name={`answers.${index}`}
              render={({ field }) => (
                <FormItem className="space-y-4 p-5 bg-white border-2 border-gray-200 rounded-xl hover:border-blue-300 transition-colors">
                  <FormLabel className="text-lg font-semibold text-gray-900">
                    <span className="inline-flex items-center justify-center w-7 h-7 rounded-full bg-blue-500 text-white text-sm mr-3">
                      {index + 1}
                    </span>
                    {q.question}
                  </FormLabel>
                  <FormControl>
                    <RadioGroup
                      onValueChange={(val) => field.onChange(parseInt(val))}
                      value={field.value?.toString()}
                      className="flex flex-col space-y-3 mt-4"
                    >
                      {q.options.map((opt: string, optIndex: number) => (
                        <FormItem 
                          key={optIndex} 
                          className="flex items-center space-x-3 space-y-0 p-4 rounded-lg border-2 border-gray-200 hover:border-blue-400 hover:bg-blue-50 transition-all cursor-pointer"
                        >
                          <FormControl>
                            <RadioGroupItem value={optIndex.toString()} />
                          </FormControl>
                          <FormLabel className="font-normal text-base cursor-pointer flex-1">{opt}</FormLabel>
                        </FormItem>
                      ))}
                    </RadioGroup>
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          ))}
        </div>
        <div className="flex gap-3 pt-4">
          <Button 
            type="submit" 
            disabled={isSubmitting}
            className="flex-1 py-6 text-lg font-semibold bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700"
          >
            {isSubmitting ? 'Submitting...' : 'Submit Quiz'}
          </Button>
        </div>
      </form>
    </Form>
  );
}
