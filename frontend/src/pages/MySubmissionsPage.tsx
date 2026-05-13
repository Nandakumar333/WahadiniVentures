import { useEffect, useState } from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Badge } from '@/components/ui/badge';
import { Loader2, AlertCircle, RefreshCw, Calendar, FileText, CheckCircle, XCircle, Clock, Trophy, ArrowRight } from 'lucide-react';
import { submissionService } from '@/services/api/submissionService';
import { SubmissionStatus } from '@/types/task';
import type { UserTaskSubmission } from '@/types/task';
import { TaskSubmissionModal } from '@/components/tasks/TaskSubmissionModal';
import type { Task } from '@/types/task';

export function MySubmissionsPage() {
  const [submissions, setSubmissions] = useState<UserTaskSubmission[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [resubmitTask, setResubmitTask] = useState<Task | null>(null);

  const fetchSubmissions = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await submissionService.getMySubmissions();
      setSubmissions(result || []);
    } catch (error: any) {
      console.error("Failed to fetch submissions", error);
      setError(error?.message || 'Failed to load submissions');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchSubmissions();
  }, []);

  const formatDateTime = (dateString: string) => {
    const date = new Date(dateString);
    return {
      date: date.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' }),
      time: date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
    };
  };

  const handleResubmit = (submission: UserTaskSubmission) => {
    if (submission.task) {
      setResubmitTask(submission.task);
    } else {
      console.error('Task details not available for resubmission');
    }
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Approved':
        return (
          <Badge className="bg-green-100 text-green-700 hover:bg-green-200 border-green-200 dark:bg-green-900/30 dark:text-green-400 dark:border-green-800">
            <CheckCircle className="w-3 h-3 mr-1" /> Approved
          </Badge>
        );
      case 'Rejected':
        return (
          <Badge className="bg-red-100 text-red-700 hover:bg-red-200 border-red-200 dark:bg-red-900/30 dark:text-red-400 dark:border-red-800">
            <XCircle className="w-3 h-3 mr-1" /> Rejected
          </Badge>
        );
      default:
        return (
          <Badge className="bg-yellow-100 text-yellow-700 hover:bg-yellow-200 border-yellow-200 dark:bg-yellow-900/30 dark:text-yellow-400 dark:border-yellow-800">
            <Clock className="w-3 h-3 mr-1" /> Pending
          </Badge>
        );
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 pb-16">
      {/* Hero Header */}
      <div className="bg-gradient-to-r from-blue-600 via-indigo-600 to-purple-600 pt-24 pb-32 relative overflow-hidden">
        <div className="absolute inset-0 bg-[url('/grid.svg')] bg-center [mask-image:linear-gradient(180deg,white,rgba(255,255,255,0))]" />
        <div className="container mx-auto px-4 sm:px-6 lg:px-8 max-w-[1600px] relative z-10">
          <h1 className="text-4xl font-extrabold text-white mb-4 tracking-tight">
            My Submissions
          </h1>
          <p className="text-blue-100 text-lg max-w-2xl">
            View and manage your task submissions, track your rewards, and check feedback from instructors.
          </p>
        </div>
      </div>

      <div className="container mx-auto px-4 sm:px-6 lg:px-8 max-w-[1600px] -mt-16 relative z-10">
        <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-xl border border-gray-100 dark:border-gray-700 overflow-hidden">
          {/* Toolbar */}
          <div className="p-6 border-b border-gray-100 dark:border-gray-700 flex justify-between items-center bg-gray-50/50 dark:bg-gray-800/50">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white flex items-center gap-2">
              <FileText className="w-5 h-5 text-blue-600" />
              Submission History
            </h2>
            <Button onClick={fetchSubmissions} variant="outline" size="sm" className="hover:bg-white dark:hover:bg-gray-700">
              <RefreshCw className={`w-4 h-4 mr-2 ${isLoading ? 'animate-spin' : ''}`} />
              Refresh
            </Button>
          </div>

          {/* Content */}
          {isLoading ? (
            <div className="flex flex-col items-center justify-center py-20">
              <Loader2 className="w-10 h-10 text-blue-600 animate-spin mb-4" />
              <p className="text-gray-500 dark:text-gray-400">Loading submissions...</p>
            </div>
          ) : error ? (
            <div className="p-8 text-center">
              <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-red-50 dark:bg-red-900/20 mb-6">
                <AlertCircle className="w-8 h-8 text-red-500" />
              </div>
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">Error Loading Submissions</h3>
              <p className="text-gray-500 dark:text-gray-400 mb-6 max-w-md mx-auto">{error}</p>
              <Button onClick={fetchSubmissions} variant="outline">
                Try Again
              </Button>
            </div>
          ) : submissions.length === 0 ? (
            <div className="p-16 text-center">
              <div className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-blue-50 dark:bg-blue-900/20 mb-6">
                <FileText className="w-10 h-10 text-blue-500" />
              </div>
              <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">No Submissions Yet</h3>
              <p className="text-gray-500 dark:text-gray-400 mb-8 max-w-md mx-auto">
                You haven't submitted any tasks yet. Start a course and complete tasks to see them here.
              </p>
              <Button asChild className="bg-blue-600 hover:bg-blue-700 text-white">
                <a href="/courses">Browse Courses</a>
              </Button>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow className="bg-gray-50/50 dark:bg-gray-900/50 hover:bg-gray-50/50 dark:hover:bg-gray-900/50">
                    <TableHead className="w-[200px]">Date Submitted</TableHead>
                    <TableHead className="w-[250px]">Task</TableHead>
                    <TableHead className="w-[150px]">Status</TableHead>
                    <TableHead className="w-[100px]">Points</TableHead>
                    <TableHead>Feedback</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {submissions.map((submission) => {
                    const dateTime = formatDateTime(submission.submittedAt);
                    return (
                      <TableRow key={submission.id} className="hover:bg-blue-50/30 dark:hover:bg-blue-900/10 transition-colors">
                        <TableCell>
                          <div className="flex items-center gap-3">
                            <div className="p-2 bg-gray-100 dark:bg-gray-800 rounded-lg text-gray-500">
                              <Calendar className="w-4 h-4" />
                            </div>
                            <div>
                              <div className="font-medium text-gray-900 dark:text-white">{dateTime.date}</div>
                              <div className="text-xs text-gray-500">{dateTime.time}</div>
                            </div>
                          </div>
                        </TableCell>
                        <TableCell>
                          <div>
                            <div className="font-medium text-gray-900 dark:text-white line-clamp-1" title={submission.task?.title}>
                              {submission.task?.title || 'Unknown Task'}
                            </div>
                            <div className="text-xs text-blue-600 dark:text-blue-400 font-medium mt-0.5">
                              {submission.task?.taskType || 'Task'}
                            </div>
                          </div>
                        </TableCell>
                        <TableCell>
                          {getStatusBadge(submission.status)}
                        </TableCell>
                        <TableCell>
                          <div className={`font-bold flex items-center gap-1 ${submission.rewardPointsAwarded > 0 ? 'text-yellow-600 dark:text-yellow-400' : 'text-gray-400'}`}>
                            {submission.rewardPointsAwarded > 0 && <Trophy className="w-3.5 h-3.5" />}
                            {submission.rewardPointsAwarded > 0 ? `+${submission.rewardPointsAwarded}` : '-'}
                          </div>
                        </TableCell>
                        <TableCell>
                          <p className="text-sm text-gray-600 dark:text-gray-300 max-w-[300px] truncate" title={submission.feedbackText || ''}>
                            {submission.feedbackText || <span className="text-gray-400 italic">No feedback yet</span>}
                          </p>
                        </TableCell>
                        <TableCell className="text-right">
                          {submission.status === SubmissionStatus.Rejected && submission.task && (
                            <Button 
                              variant="outline" 
                              size="sm" 
                              onClick={() => handleResubmit(submission)}
                              className="text-blue-600 hover:text-blue-700 border-blue-200 hover:bg-blue-50"
                            >
                              Resubmit <ArrowRight className="w-3 h-3 ml-1" />
                            </Button>
                          )}
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </div>
          )}
        </div>
      </div>
      
      {resubmitTask && (
        <TaskSubmissionModal 
            task={resubmitTask} 
            isOpen={!!resubmitTask} 
            onClose={() => { setResubmitTask(null); fetchSubmissions(); }} 
        />
      )}
    </div>
  );
}
