import { Card, CardHeader, CardTitle, CardContent, CardFooter } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Trophy, FileText, Link as LinkIcon, Image, Wallet, CheckCircle2, Clock, AlertCircle } from 'lucide-react';
import type { Task } from '@/types/task';
import { TaskType } from '@/types/task';

interface TaskCardProps {
  task: Task;
  onOpenSubmission: (task: Task) => void;
  submissionStatus?: 'Pending' | 'Approved' | 'Rejected';
}

export function TaskCard({ task, onOpenSubmission, submissionStatus }: TaskCardProps) {
  const isCompleted = submissionStatus === 'Approved';
  const isPending = submissionStatus === 'Pending';
  const isRejected = submissionStatus === 'Rejected';

  const getTaskTypeIcon = () => {
    switch (task.taskType) {
      case TaskType.Quiz:
        return <FileText className="w-4 h-4" />;
      case TaskType.ExternalLink:
        return <LinkIcon className="w-4 h-4" />;
      case TaskType.Screenshot:
        return <Image className="w-4 h-4" />;
      case TaskType.WalletVerification:
        return <Wallet className="w-4 h-4" />;
      default:
        return <FileText className="w-4 h-4" />;
    }
  };

  const getStatusBadge = () => {
    if (isCompleted) {
      return (
        <Badge className="bg-green-100 text-green-800 border-green-300 flex items-center gap-1">
          <CheckCircle2 className="w-3 h-3" />
          Completed
        </Badge>
      );
    }
    if (isPending) {
      return (
        <Badge className="bg-yellow-100 text-yellow-800 border-yellow-300 flex items-center gap-1">
          <Clock className="w-3 h-3" />
          Pending
        </Badge>
      );
    }
    if (isRejected) {
      return (
        <Badge className="bg-red-100 text-red-800 border-red-300 flex items-center gap-1">
          <AlertCircle className="w-3 h-3" />
          Rejected
        </Badge>
      );
    }
    return null;
  };

  return (
    <Card className={`w-full hover:shadow-xl transition-all duration-300 ${
      isCompleted ? 'border-green-300 bg-green-50/30' : 
      isPending ? 'border-yellow-300 bg-yellow-50/30' : 
      isRejected ? 'border-red-300 bg-red-50/30' : 
      'border-gray-200 hover:border-blue-400'
    }`}>
      <CardHeader>
        <div className="flex justify-between items-start mb-3">
          <CardTitle className="text-xl font-bold text-gray-900 flex-1">{task.title}</CardTitle>
          {getStatusBadge()}
        </div>
        <div className="flex items-center gap-2">
          <Badge variant="outline" className="flex items-center gap-1">
            {getTaskTypeIcon()}
            <span className="text-xs">{task.taskType}</span>
          </Badge>
          {task.isRequired && (
            <Badge variant="destructive" className="text-xs">Required</Badge>
          )}
        </div>
      </CardHeader>
      <CardContent>
        <p className="text-sm text-gray-700 line-clamp-3 leading-relaxed">{task.description}</p>
      </CardContent>
      <CardFooter className="flex justify-between items-center pt-4 border-t">
        <div className="flex items-center gap-2">
          <Trophy className="w-5 h-5 text-yellow-600" />
          <span className="font-bold text-lg text-gray-900">{task.rewardPoints}</span>
          <span className="text-sm text-gray-600">points</span>
        </div>
        {isCompleted ? (
          <Button disabled variant="secondary" className="bg-green-100 text-green-800 hover:bg-green-200">
            <CheckCircle2 className="w-4 h-4 mr-2" />
            Completed
          </Button>
        ) : isPending ? (
          <Button disabled variant="secondary" className="bg-yellow-100 text-yellow-800">
            <Clock className="w-4 h-4 mr-2" />
            Under Review
          </Button>
        ) : (
          <Button 
            onClick={() => onOpenSubmission(task)} 
            className="bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 font-semibold"
          >
            {isRejected ? 'Resubmit Task' : 'Start Task'}
          </Button>
        )}
      </CardFooter>
    </Card>
  );
}
