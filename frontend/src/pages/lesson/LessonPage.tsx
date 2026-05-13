import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { LessonPlayer } from '@/components/lesson/LessonPlayer';
import { LessonTasksSection } from '@/components/lesson/LessonTasks';
import { TaskSubmissionModal } from '@/components/tasks/TaskSubmissionModal';
import { Loader2, AlertCircle, ArrowLeft, ChevronRight, BookOpen, Trophy } from 'lucide-react';
import { useLesson } from '@/hooks/lessons/useLesson';
import type { LearningTask } from '@/types/task';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';

export default function LessonPage() {
  const { lessonId } = useParams<{ lessonId: string }>();
  const navigate = useNavigate();
  const [selectedTask, setSelectedTask] = useState<LearningTask | null>(null);
  const [isSubmissionModalOpen, setIsSubmissionModalOpen] = useState(false);

  if (!lessonId) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-50 dark:bg-gray-900">
        <div className="text-center">
          <AlertCircle className="w-16 h-16 text-red-500 mx-auto mb-4" />
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">Lesson Not Found</h1>
          <p className="text-gray-600 dark:text-gray-400">The requested lesson could not be found.</p>
          <Button onClick={() => navigate('/courses')} className="mt-4">Back to Courses</Button>
        </div>
      </div>
    );
  }

  const { data: lesson, isLoading, error } = useLesson(lessonId, { includeTasks: true });

  const handleOpenSubmission = (task: LearningTask) => {
    setSelectedTask(task);
    setIsSubmissionModalOpen(true);
  };

  const handleCloseSubmission = () => {
    setSelectedTask(null);
    setIsSubmissionModalOpen(false);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-50 dark:bg-gray-900">
        <div className="text-center">
          <Loader2 className="w-12 h-12 text-blue-600 animate-spin mx-auto mb-4" />
          <p className="text-gray-600 dark:text-gray-400">Loading lesson...</p>
        </div>
      </div>
    );
  }

  if (error || !lesson) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-50 dark:bg-gray-900">
        <div className="text-center">
          <AlertCircle className="w-16 h-16 text-red-500 mx-auto mb-4" />
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">Error Loading Lesson</h1>
          <p className="text-gray-600 dark:text-gray-400">Unable to load the lesson. Please try again later.</p>
          <Button onClick={() => window.location.reload()} className="mt-4">Try Again</Button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 pb-16">
      {/* Header Navigation */}
      <div className="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 sticky top-0 z-20">
        <div className="max-w-[1600px] mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
          <div className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 overflow-hidden">
            <button 
              onClick={() => navigate(-1)} 
              className="hover:text-blue-600 dark:hover:text-blue-400 flex items-center gap-1 transition-colors whitespace-nowrap"
            >
              <ArrowLeft className="w-4 h-4" />
              Back
            </button>
            <ChevronRight className="w-4 h-4 text-gray-400 flex-shrink-0" />
            <span className="truncate font-medium text-gray-900 dark:text-white">{lesson.title}</span>
          </div>
          
          {lesson.rewardPoints > 0 && (
            <Badge variant="secondary" className="bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400 border-yellow-200 dark:border-yellow-800 flex items-center gap-1">
              <Trophy className="w-3.5 h-3.5" />
              {lesson.rewardPoints} XP
            </Badge>
          )}
        </div>
      </div>

      <div className="max-w-[1600px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Main Content Column (Video & Description) */}
          <div className="lg:col-span-2 space-y-8">
            {/* Video Player Section */}
            <div className="bg-black rounded-2xl overflow-hidden shadow-2xl ring-1 ring-gray-900/10">
              <LessonPlayer lesson={lesson} />
            </div>

            {/* Lesson Info */}
            <div className="bg-white dark:bg-gray-800 rounded-2xl p-6 shadow-sm border border-gray-100 dark:border-gray-700">
              <h1 className="text-2xl sm:text-3xl font-bold text-gray-900 dark:text-white mb-4">
                {lesson.title}
              </h1>
              
              <div className="prose dark:prose-invert max-w-none">
                <p className="text-gray-600 dark:text-gray-300 leading-relaxed">
                  {lesson.description}
                </p>
              </div>

              {/* Additional Metadata or Resources could go here */}
              <div className="mt-6 pt-6 border-t border-gray-100 dark:border-gray-700 flex items-center gap-4">
                <div className="flex items-center gap-2 text-sm text-gray-500 dark:text-gray-400">
                  <BookOpen className="w-4 h-4" />
                  <span>Lesson Content</span>
                </div>
                {/* Add more metadata items here if available */}
              </div>
            </div>
          </div>

          {/* Sidebar Column (Tasks) */}
          <div className="lg:col-span-1">
            <div className="sticky top-24 space-y-6">
              {lesson.tasks && lesson.tasks.length > 0 ? (
                <LessonTasksSection
                  tasks={lesson.tasks}
                  onOpenSubmission={handleOpenSubmission}
                />
              ) : (
                <div className="bg-white dark:bg-gray-800 rounded-xl p-6 shadow-sm border border-gray-100 dark:border-gray-700 text-center">
                  <div className="w-12 h-12 bg-blue-50 dark:bg-blue-900/20 rounded-full flex items-center justify-center mx-auto mb-3">
                    <BookOpen className="w-6 h-6 text-blue-500" />
                  </div>
                  <h3 className="font-semibold text-gray-900 dark:text-white mb-1">No Tasks</h3>
                  <p className="text-sm text-gray-500 dark:text-gray-400">
                    This lesson has no associated tasks. Just watch the video to complete it!
                  </p>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Task Submission Modal */}
      {selectedTask && (
        <TaskSubmissionModal
          isOpen={isSubmissionModalOpen}
          onClose={handleCloseSubmission}
          task={{
            id: selectedTask.id,
            title: selectedTask.title,
            description: selectedTask.description,
            taskType: selectedTask.taskType as any,
            rewardPoints: selectedTask.rewardPoints,
            taskData: selectedTask.taskData,
            isRequired: selectedTask.isRequired
          }}
        />
      )}
    </div>
  );
}
