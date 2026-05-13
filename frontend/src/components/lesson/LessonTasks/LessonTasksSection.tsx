import { useState } from 'react';
import { Card, CardHeader, CardTitle, CardContent, CardFooter } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { CheckCircle2, Clock, XCircle, Trophy, ChevronDown, ChevronUp } from 'lucide-react';
import type { LearningTask } from '@/types/task';
import { useTaskSubmissionStatus } from '@/hooks/lesson/useTaskSubmissionStatus';

interface TaskCardWithStatusProps {
  task: LearningTask;
  onOpenSubmission: (task: LearningTask) => void;
}

function TaskCardWithStatus({ task, onOpenSubmission }: TaskCardWithStatusProps) {
  const { data: status, isLoading } = useTaskSubmissionStatus(task.id);

  const getStatusBadge = () => {
    if (isLoading) return <Badge variant="outline">Loading...</Badge>;
    if (!status?.hasSubmitted) return <Badge variant="outline">Not Started</Badge>;

    switch (status.status) {
      case 'Approved':
        return (
          <Badge className="bg-green-100 text-green-800 border-green-300">
            <CheckCircle2 className="w-3 h-3 mr-1" />
            Completed
          </Badge>
        );
      case 'Pending':
        return (
          <Badge className="bg-yellow-100 text-yellow-800 border-yellow-300">
            <Clock className="w-3 h-3 mr-1" />
            Pending Review
          </Badge>
        );
      case 'Rejected':
        return (
          <Badge className="bg-red-100 text-red-800 border-red-300">
            <XCircle className="w-3 h-3 mr-1" />
            Rejected
          </Badge>
        );
      default:
        return <Badge variant="outline">Not Started</Badge>;
    }
  };

  const getActionButton = () => {
    if (isLoading) {
      return <Button disabled className="w-full py-3">Loading...</Button>;
    }

    if (!status?.hasSubmitted) {
      return (
        <Button 
          onClick={() => onOpenSubmission(task)} 
          className="w-full py-3 bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 font-semibold"
        >
          Start Task
        </Button>
      );
    }

    switch (status.status) {
      case 'Approved':
        return (
          <Button disabled variant="secondary" className="w-full py-3 bg-green-100 text-green-800">
            <CheckCircle2 className="w-4 h-4 mr-2" />
            Completed
          </Button>
        );
      case 'Pending':
        return (
          <Button disabled variant="secondary" className="w-full py-3 bg-yellow-100 text-yellow-800">
            <Clock className="w-4 h-4 mr-2" />
            Under Review
          </Button>
        );
      case 'Rejected':
        return (
          <Button 
            onClick={() => onOpenSubmission(task)} 
            variant="default" 
            className="w-full py-3 bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 font-semibold"
          >
            Resubmit Task
          </Button>
        );
      default:
        return (
          <Button 
            onClick={() => onOpenSubmission(task)} 
            className="w-full py-3 bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 font-semibold"
          >
            Start Task
          </Button>
        );
    }
  };

  return (
    <Card className={`flex flex-col hover:shadow-xl transition-all duration-300 border bg-white dark:bg-gray-800 ${
      status?.status === 'Approved' ? 'border-green-200 dark:border-green-800 shadow-green-100 dark:shadow-none' : 
      status?.status === 'Pending' ? 'border-yellow-200 dark:border-yellow-800 shadow-yellow-100 dark:shadow-none' : 
      status?.status === 'Rejected' ? 'border-red-200 dark:border-red-800 shadow-red-100 dark:shadow-none' : 
      'border-gray-100 dark:border-gray-700 hover:border-blue-300 dark:hover:border-blue-700'
    }`}>
      <CardHeader className="pb-3">
        <div className="flex justify-between items-start gap-3 mb-3">
          <CardTitle className="text-lg font-bold text-gray-900 dark:text-white flex-1 break-words">{task.title}</CardTitle>
          {getStatusBadge()}
        </div>
        <div className="flex items-center gap-2 text-sm flex-wrap">
          <Badge variant="outline" className="text-xs bg-gray-50 dark:bg-gray-900 text-gray-600 dark:text-gray-400 border-gray-200 dark:border-gray-700">
            {task.taskType}
          </Badge>
          {task.isRequired && (
            <Badge variant="destructive" className="text-xs">
              Required
            </Badge>
          )}
        </div>
      </CardHeader>
      <CardContent className="flex-1 pb-3">
        <p className="text-sm text-gray-600 dark:text-gray-300 line-clamp-3 leading-relaxed">{task.description}</p>
        {status?.feedbackText && status.status === 'Rejected' && (
          <div className="mt-3 p-3 bg-red-50 dark:bg-red-900/20 border-l-4 border-red-400 rounded-r-md">
            <p className="text-sm text-red-900 dark:text-red-300">
              <strong className="font-semibold">Feedback:</strong> {status.feedbackText}
            </p>
          </div>
        )}
      </CardContent>
      <CardFooter className="flex flex-col gap-3 pt-3 border-t border-gray-100 dark:border-gray-700 mt-auto">
        <div className="flex items-center justify-center gap-2 w-full">
          <Trophy className="w-4 h-4 text-yellow-500" />
          <span className="font-bold text-base text-gray-900 dark:text-white">{task.rewardPoints}</span>
          <span className="text-sm text-gray-500 dark:text-gray-400">points</span>
        </div>
        <div className="w-full">
          {getActionButton()}
        </div>
      </CardFooter>
    </Card>
  );
}

interface LessonTasksSectionProps {
  tasks: LearningTask[];
  onOpenSubmission: (task: LearningTask) => void;
}

export function LessonTasksSection({ tasks, onOpenSubmission }: LessonTasksSectionProps) {
  const [isExpanded, setIsExpanded] = useState(true);

  if (!tasks || tasks.length === 0) {
    return null;
  }

  const totalPoints = tasks.reduce((sum, task) => sum + task.rewardPoints, 0);
  const requiredTasks = tasks.filter(t => t.isRequired).length;

  return (
    <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-sm border border-gray-100 dark:border-gray-700 overflow-hidden">
      <div
        className="p-5 bg-gradient-to-r from-blue-600 to-indigo-600 cursor-pointer hover:from-blue-700 hover:to-indigo-700 transition-all"
        onClick={() => setIsExpanded(!isExpanded)}
      >
        <div className="flex justify-between items-center">
          <div className="text-white">
            <h2 className="text-xl font-bold mb-1 flex items-center gap-2">
              <span>Tasks & Assignments</span>
              <Badge variant="secondary" className="bg-white/20 text-white hover:bg-white/30 border-none">
                {tasks.length}
              </Badge>
            </h2>
            <p className="text-xs text-blue-100 flex items-center gap-3">
              {requiredTasks > 0 && (
                <span>{requiredTasks} required</span>
              )}
              <span className="flex items-center gap-1">
                <Trophy className="w-3 h-3" /> {totalPoints} pts available
              </span>
            </p>
          </div>
          <Button variant="ghost" size="sm" className="text-white hover:bg-white/20 hover:text-white h-8 w-8 p-0 rounded-full">
            {isExpanded ? (
              <ChevronUp className="w-5 h-5" />
            ) : (
              <ChevronDown className="w-5 h-5" />
            )}
          </Button>
        </div>
      </div>

      {isExpanded && (
        <div className="p-4 bg-gray-50 dark:bg-gray-900/50">
          <div className="space-y-4">
            {tasks.map((task) => (
              <TaskCardWithStatus
                key={task.id}
                task={task}
                onOpenSubmission={onOpenSubmission}
              />
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
