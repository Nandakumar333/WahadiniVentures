import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { Trophy, FileText, Link as LinkIcon, Image, Wallet, MessageSquare } from 'lucide-react';
import { TaskType } from '@/types/task';
import type { Task } from '@/types/task';
import { QuizTaskForm } from './QuizTaskForm';
import { TextTaskForm } from './TextTaskForm';
import { ExternalLinkTaskForm } from './ExternalLinkTaskForm';
import { ScreenshotTaskForm } from './ScreenshotTaskForm';
import { WalletTaskForm } from './WalletTaskForm';

interface TaskSubmissionModalProps {
  task: Task | null;
  isOpen: boolean;
  onClose: () => void;
}

export function TaskSubmissionModal({ task, isOpen, onClose }: TaskSubmissionModalProps) {
  if (!task) return null;

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
      case TaskType.TextSubmission:
        return <MessageSquare className="w-4 h-4" />;
      default:
        return <FileText className="w-4 h-4" />;
    }
  };

  const renderForm = () => {
    switch (task.taskType) {
      case TaskType.Quiz:
        return <QuizTaskForm task={task} onSuccess={onClose} />;
      case TaskType.TextSubmission:
        return <TextTaskForm task={task} onSuccess={onClose} />;
      case TaskType.ExternalLink:
        return <ExternalLinkTaskForm task={task} onSuccess={onClose} />;
      case TaskType.WalletVerification:
        return <WalletTaskForm task={task} onSuccess={onClose} />;
      case TaskType.Screenshot:
        return <ScreenshotTaskForm task={task} onSuccess={onClose} />;
      default:
        return <p className="text-center text-gray-500">Unknown Task Type</p>;
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose} modal>
      <DialogContent className="!fixed !left-1/2 !top-1/2 !-translate-x-1/2 !-translate-y-1/2 max-w-[90vw] sm:max-w-[600px] md:max-w-[700px] max-h-[calc(100vh-3rem)] overflow-hidden flex flex-col p-0 gap-0 bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
        {/* Header Section - Fixed */}
        <div className="px-6 pt-6 pb-4 border-b border-gray-200 dark:border-gray-700 bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-blue-900/20 dark:to-indigo-900/20 flex-shrink-0">
          <DialogHeader>
            <div className="pr-10">
              <DialogTitle className="text-xl font-bold text-gray-900 dark:text-white mb-2 leading-tight">
                {task.title}
              </DialogTitle>
              <DialogDescription className="text-sm text-gray-600 dark:text-gray-300 leading-relaxed">
                {task.description}
              </DialogDescription>
            </div>
          </DialogHeader>
          
          <div className="flex items-center gap-2 mt-4 flex-wrap">
            <Badge variant="outline" className="flex items-center gap-1 text-xs bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 border-gray-200 dark:border-gray-700">
              {getTaskTypeIcon()}
              <span>{task.taskType}</span>
            </Badge>
            <Badge className="flex items-center gap-1 bg-yellow-100 text-yellow-800 border-yellow-300 dark:bg-yellow-900/30 dark:text-yellow-400 dark:border-yellow-800 text-xs">
              <Trophy className="w-3 h-3" />
              <span>{task.rewardPoints} points</span>
            </Badge>
            {task.isRequired && (
              <Badge variant="destructive" className="text-xs">Required</Badge>
            )}
          </div>
        </div>
        
        {/* Scrollable Content Section */}
        <div className="overflow-y-auto flex-1 px-6 py-6">
          {renderForm()}
        </div>
      </DialogContent>
    </Dialog>
  );
}
