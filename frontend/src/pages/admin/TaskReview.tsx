import { useState } from 'react';
import { CheckCircle, XCircle, Search, Calendar, X } from 'lucide-react';
import TaskCard from '../../components/admin/TaskCard';
import { useTaskReview } from '../../hooks/useTaskReview';
import { SubmissionStatus, type PendingTaskDto, type TaskReviewRequestDto } from '../../types/admin.types';

/**
 * Admin page for reviewing pending task submissions
 * T052: US2 - Task Review Workflow
 */
const TaskReview = () => {
  const [filters, setFilters] = useState({
    dateFrom: '',
    dateTo: '',
    courseId: '',
    pageNumber: 1,
    pageSize: 20
  });

  const [selectedTask, setSelectedTask] = useState<PendingTaskDto | null>(null);
  const [reviewFeedback, setReviewFeedback] = useState('');
  const [showReviewModal, setShowReviewModal] = useState(false);

  const { tasks, isLoading, error, reviewTask, isReviewing } = useTaskReview(filters);

  const handleReviewClick = (submissionId: string) => {
    const task = tasks.find(t => t.submissionId === submissionId);
    if (task) {
      setSelectedTask(task);
      setReviewFeedback('');
      setShowReviewModal(true);
    }
  };

  const handleApprove = async () => {
    if (!selectedTask) return;

    const review: TaskReviewRequestDto = {
      status: SubmissionStatus.Approved,
      feedback: reviewFeedback || undefined
    };

    try {
      await reviewTask({ submissionId: selectedTask.submissionId, review });
      setShowReviewModal(false);
      setSelectedTask(null);
      setReviewFeedback('');
    } catch (err) {
      console.error('Failed to approve task:', err);
      alert('Failed to approve submission. Please try again.');
    }
  };

  const handleReject = async () => {
    if (!selectedTask) return;

    if (!reviewFeedback.trim()) {
      alert('Feedback is required when rejecting a submission.');
      return;
    }

    const review: TaskReviewRequestDto = {
      status: SubmissionStatus.Rejected,
      feedback: reviewFeedback
    };

    try {
      await reviewTask({ submissionId: selectedTask.submissionId, review });
      setShowReviewModal(false);
      setSelectedTask(null);
      setReviewFeedback('');
    } catch (err) {
      console.error('Failed to reject task:', err);
      alert('Failed to reject submission. Please try again.');
    }
  };

  const closeModal = () => {
    if (!isReviewing) {
      setShowReviewModal(false);
      setSelectedTask(null);
      setReviewFeedback('');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Task Review</h1>
        <p className="text-gray-600 mt-2">
          Review and approve student task submissions
        </p>
      </div>

      {/* Filters */}
      <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
        <div className="flex items-center gap-2 mb-4">
          <Search className="w-5 h-5 text-gray-500" />
          <h2 className="text-lg font-semibold text-gray-900">Filters</h2>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label htmlFor="filter-date-from" className="block text-sm font-medium text-gray-700 mb-1">
              From Date
            </label>
            <input
              id="filter-date-from"
              type="date"
              value={filters.dateFrom}
              onChange={(e) => setFilters({ ...filters, dateFrom: e.target.value })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-purple-500"
            />
          </div>

          <div>
            <label htmlFor="filter-date-to" className="block text-sm font-medium text-gray-700 mb-1">
              To Date
            </label>
            <input
              id="filter-date-to"
              type="date"
              value={filters.dateTo}
              onChange={(e) => setFilters({ ...filters, dateTo: e.target.value })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-purple-500"
            />
          </div>

          <div>
            <label htmlFor="filter-page-size" className="block text-sm font-medium text-gray-700 mb-1">
              Page Size
            </label>
            <select
              id="filter-page-size"
              value={filters.pageSize}
              onChange={(e) => setFilters({ ...filters, pageSize: Number(e.target.value) })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-purple-500"
            >
              <option value={10}>10</option>
              <option value={20}>20</option>
              <option value={50}>50</option>
            </select>
          </div>
        </div>
      </div>

      {/* Task List */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-red-800">
            Failed to load pending tasks. {error instanceof Error ? error.message : String(error)}
          </p>
        </div>
      )}

      {isLoading && (
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-purple-600 mx-auto"></div>
          <p className="text-gray-600 mt-4">Loading pending tasks...</p>
        </div>
      )}

      {!isLoading && !error && tasks.length === 0 && (
        <div className="bg-gray-50 rounded-lg p-12 text-center">
          <Calendar className="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-gray-700 mb-2">No pending tasks</h3>
          <p className="text-gray-600">All submissions have been reviewed.</p>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-6">
        {tasks.map((task) => (
          <TaskCard key={task.submissionId} task={task} onReview={handleReviewClick} />
        ))}
      </div>

      {/* Review Modal */}
      {showReviewModal && selectedTask && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            {/* Modal Header */}
            <div className="flex justify-between items-center p-6 border-b border-gray-200">
              <h2 className="text-2xl font-bold text-gray-900">Review Submission</h2>
              <button
                onClick={closeModal}
                disabled={isReviewing}
                className="text-gray-500 hover:text-gray-700 disabled:opacity-50"
              >
                <X className="w-6 h-6" />
              </button>
            </div>

            {/* Modal Content */}
            <div className="p-6 space-y-4">
              <div>
                <h3 className="font-semibold text-gray-900">{selectedTask.taskTitle}</h3>
                <p className="text-sm text-gray-600">{selectedTask.courseName}</p>
              </div>

              <div>
                <p className="text-sm text-gray-600">
                  <span className="font-medium">Submitted by:</span> {selectedTask.username}
                </p>
                <p className="text-sm text-gray-600">
                  <span className="font-medium">Reward:</span> {selectedTask.pointReward} points
                </p>
              </div>

              <div>
                <p className="block text-sm font-medium text-gray-700 mb-2">
                  Submission Data
                </p>
                <div className="bg-gray-50 rounded-md p-4 max-h-64 overflow-y-auto">
                  <pre className="text-sm text-gray-800 whitespace-pre-wrap">
                    {selectedTask.submissionData}
                  </pre>
                </div>
              </div>

              <div>
                <label htmlFor="review-feedback" className="block text-sm font-medium text-gray-700 mb-2">
                  Feedback (Optional for Approve, Required for Reject)
                </label>
                <textarea
                  id="review-feedback"
                  value={reviewFeedback}
                  onChange={(e) => setReviewFeedback(e.target.value)}
                  placeholder="Provide feedback to the student..."
                  rows={4}
                  disabled={isReviewing}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-purple-500 disabled:bg-gray-100"
                />
                <p className="text-xs text-gray-500 mt-1">Max 1000 characters</p>
              </div>
            </div>

            {/* Modal Actions */}
            <div className="flex gap-3 p-6 border-t border-gray-200">
              <button
                onClick={handleReject}
                disabled={isReviewing}
                className="flex-1 flex items-center justify-center gap-2 px-4 py-3 bg-red-600 hover:bg-red-700 text-white font-medium rounded-md transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <XCircle className="w-5 h-5" />
                {isReviewing ? 'Rejecting...' : 'Reject'}
              </button>

              <button
                onClick={handleApprove}
                disabled={isReviewing}
                className="flex-1 flex items-center justify-center gap-2 px-4 py-3 bg-green-600 hover:bg-green-700 text-white font-medium rounded-md transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <CheckCircle className="w-5 h-5" />
                {isReviewing ? 'Approving...' : 'Approve'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default TaskReview;
