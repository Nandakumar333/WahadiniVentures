import { Clock, FileText, Award, User } from 'lucide-react';
import type { PendingTaskDto } from '../../types/admin.types';

interface TaskCardProps {
  task: PendingTaskDto;
  onReview: (submissionId: string) => void;
}

/**
 * Task submission card component for admin review
 * T051: US2 - Task Review Workflow
 */
export default function TaskCard({ task, onReview }: Readonly<TaskCardProps>) {
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const parseSubmissionData = (data: string) => {
    try {
      const parsed = JSON.parse(data);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return data;
    }
  };

  return (
    <div className="bg-white border border-gray-200 rounded-lg shadow-sm hover:shadow-md transition-shadow p-6">
      {/* Header */}
      <div className="flex justify-between items-start mb-4">
        <div className="flex-1">
          <h3 className="text-lg font-semibold text-gray-900 mb-1">
            {task.taskTitle}
          </h3>
          <p className="text-sm text-gray-600">{task.courseName}</p>
        </div>
        <div className="flex items-center gap-2 px-3 py-1 bg-purple-100 text-purple-700 rounded-full">
          <Award className="w-4 h-4" />
          <span className="font-semibold">{task.pointReward} pts</span>
        </div>
      </div>

      {/* User & Time Info */}
      <div className="flex items-center gap-4 mb-4 text-sm text-gray-600">
        <div className="flex items-center gap-1">
          <User className="w-4 h-4" />
          <span>{task.username}</span>
        </div>
        <div className="flex items-center gap-1">
          <Clock className="w-4 h-4" />
          <span>{formatDate(task.submittedAt)}</span>
        </div>
      </div>

      {/* Submission Data */}
      <div className="mb-4">
        <div className="flex items-center gap-2 mb-2">
          <FileText className="w-4 h-4 text-gray-500" />
          <span className="text-sm font-medium text-gray-700">Submission</span>
        </div>
        <div className="bg-gray-50 rounded-md p-3 max-h-32 overflow-y-auto">
          <pre className="text-xs text-gray-800 whitespace-pre-wrap font-mono">
            {parseSubmissionData(task.submissionData)}
          </pre>
        </div>
      </div>

      {/* Action Button */}
      <button
        onClick={() => onReview(task.submissionId)}
        className="w-full px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white font-medium rounded-md transition-colors"
      >
        Review Submission
      </button>
    </div>
  );
}
