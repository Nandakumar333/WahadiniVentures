import { useState, useEffect } from 'react';
import type { Course, Lesson, CreateCourseRequest, UpdateCourseRequest, UpdateLessonRequest } from '@/types/course.types';
import type { CreateCourseFormData, CreateLessonFormData, UpdateLessonFormData } from '@/services/validation/course.validation';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Button } from '@/components/ui/button';
import { CourseBasicInfoForm } from './CourseBasicInfoForm';
import { LessonEditor } from './LessonEditor';
import { DraggableLessonList } from './DraggableLessonList';
import { LessonDeleteDialog } from './LessonDeleteDialog';
import { CoursePreview } from './CoursePreview';
import { useCreateCourse } from '@/hooks/courses/useCreateCourse';
import { useUpdateCourse } from '@/hooks/courses/useUpdateCourse';
import { usePublishCourse } from '@/hooks/courses/usePublishCourse';
import { useCreateLesson } from '@/hooks/lessons/useCreateLesson';
import { useUpdateLesson } from '@/hooks/courses/useUpdateLesson';
import { useDeleteLesson } from '@/hooks/courses/useDeleteLesson';
import { useReorderLessons } from '@/hooks/lessons/useReorderLessons';
import { Save, Eye, Plus } from 'lucide-react';
import { toast } from '@/utils/toast';

interface CourseEditorProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  courseId?: string;
  initialCourse?: Partial<Course> & { id?: string; isPublished?: boolean };
  initialLessons?: Lesson[];
  mode?: 'create' | 'edit';
}

/**
 * Course Editor Component
 * Full course management with tabs for Basic Info, Lessons, and Preview
 */
export function CourseEditor({
  open,
  onOpenChange,
  initialCourse,
  initialLessons = [],
  mode = 'create',
}: CourseEditorProps) {
  const [activeTab, setActiveTab] = useState('basic');
  const [currentCourse, setCurrentCourse] = useState<Partial<Course> & { id?: string; isPublished?: boolean }>(
    initialCourse || {}
  );
  const [lessons, setLessons] = useState<Lesson[]>(initialLessons);
  const [editingLesson, setEditingLesson] = useState<Lesson | null>(null);
  const [deletingLesson, setDeletingLesson] = useState<Lesson | null>(null);
  const [showLessonEditor, setShowLessonEditor] = useState(false);

  // Mutations
  const createCourseMutation = useCreateCourse();
  const updateCourseMutation = useUpdateCourse();
  const publishCourseMutation = usePublishCourse();
  const createLessonMutation = useCreateLesson();
  const updateLessonMutation = useUpdateLesson();
  const deleteLessonMutation = useDeleteLesson();
  const reorderLessonsMutation = useReorderLessons();

  // Handle successful course creation
  useEffect(() => {
    if (createCourseMutation.isSuccess && createCourseMutation.data) {
      setCurrentCourse(createCourseMutation.data);
      setActiveTab('lessons');
    }
  }, [createCourseMutation.isSuccess, createCourseMutation.data]);

  // Handle successful course publication
  useEffect(() => {
    if (publishCourseMutation.isSuccess && publishCourseMutation.data) {
      setCurrentCourse(publishCourseMutation.data);
      toast.success('Course published successfully!');
      onOpenChange(false);
    }
  }, [publishCourseMutation.isSuccess, publishCourseMutation.data, onOpenChange]);

  // Handle successful lesson creation
  useEffect(() => {
    if (createLessonMutation.isSuccess && createLessonMutation.data) {
      setLessons([...lessons, createLessonMutation.data]);
      setShowLessonEditor(false);
    }
  }, [createLessonMutation.isSuccess, createLessonMutation.data, lessons]);

  // Handle successful lesson update
  useEffect(() => {
    if (updateLessonMutation.isSuccess && updateLessonMutation.data) {
      setLessons(lessons.map((l) => (l.id === updateLessonMutation.data.id ? updateLessonMutation.data : l)));
      setEditingLesson(null);
      setShowLessonEditor(false);
    }
  }, [updateLessonMutation.isSuccess, updateLessonMutation.data, lessons]);

  // Handle successful lesson deletion
  useEffect(() => {
    if (deleteLessonMutation.isSuccess && deletingLesson) {
      setLessons(lessons.filter((l) => l.id !== deletingLesson.id));
      setDeletingLesson(null);
    }
  }, [deleteLessonMutation.isSuccess, deletingLesson, lessons]);

  // Update state when props change
  useEffect(() => {
    if (initialCourse) {
      setCurrentCourse(initialCourse);
    }
    if (initialLessons) {
      setLessons(initialLessons);
    }
  }, [initialCourse, initialLessons]);

  /**
   * Handle saving course basic info (draft)
   */
  const handleSaveCourse = (data: CreateCourseFormData) => {
    if (mode === 'create' || !currentCourse.id) {
      // Create new course (difficultyLevel is already number from form)
      createCourseMutation.mutate(data as CreateCourseRequest);
    } else {
      // Update existing course
      updateCourseMutation.mutate({
        courseId: currentCourse.id,
        data: {
          ...data,
          isPublished: currentCourse.isPublished || false,
        } as UpdateCourseRequest,
      });
    }
  };

  /**
   * Handle publishing course
   */
  const handlePublish = () => {
    if (!currentCourse.id) {
      toast.error('Please save the course before publishing');
      return;
    }

    if (lessons.length === 0) {
      toast.error('Cannot publish a course with zero lessons');
      return;
    }

    publishCourseMutation.mutate(currentCourse.id);
  };

  /**
   * Handle creating a lesson
   */
  const handleCreateLesson = (data: CreateLessonFormData) => {
    if (!currentCourse.id) {
      toast.error('Please save the course before adding lessons');
      return;
    }

    createLessonMutation.mutate({
      ...data,
      courseId: currentCourse.id,
      orderIndex: lessons.length + 1,
    });
  };

  /**
   * Handle updating a lesson
   */
  const handleUpdateLesson = (data: CreateLessonFormData | UpdateLessonFormData) => {
    if (!editingLesson) return;

    updateLessonMutation.mutate({
      lessonId: editingLesson.id,
      data: {
        ...data,
        id: editingLesson.id,
      } as UpdateLessonRequest,
    });
  };

  /**
   * Handle lesson reordering
   */
  const handleReorderLessons = (lessonOrderMap: Record<string, number>) => {
    if (!currentCourse.id) return;

    reorderLessonsMutation.mutate({
      courseId: currentCourse.id,
      lessonOrderMap,
    });
  };

  /**
   * Open lesson editor for creating
   */
  const handleAddLesson = () => {
    if (!currentCourse.id) {
      toast.error('Please save the course before adding lessons');
      setActiveTab('basic');
      return;
    }
    setEditingLesson(null);
    setShowLessonEditor(true);
  };

  /**
   * Open lesson editor for editing
   */
  const handleEditLesson = (lesson: Lesson) => {
    setEditingLesson(lesson);
    setShowLessonEditor(true);
  };

  /**
   * Open delete confirmation dialog
   */
  const handleDeleteLesson = (lessonId: string) => {
    const lesson = lessons.find((l) => l.id === lessonId);
    if (lesson) {
      setDeletingLesson(lesson);
    }
  };

  /**
   * Confirm lesson deletion
   */
  const confirmDeleteLesson = () => {
    if (deletingLesson) {
      deleteLessonMutation.mutate(deletingLesson.id);
    }
  };

  const isPublishDisabled = !currentCourse.id || lessons.length === 0 || currentCourse.isPublished;

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="max-w-5xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>
              {mode === 'create' ? 'Create New Course' : 'Edit Course'}
            </DialogTitle>
          </DialogHeader>

          <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
            <TabsList className="grid w-full grid-cols-3">
              <TabsTrigger value="basic">Basic Info</TabsTrigger>
              <TabsTrigger value="lessons">
                Lessons ({lessons.length})
              </TabsTrigger>
              <TabsTrigger value="preview">
                <Eye className="mr-2 h-4 w-4" />
                Preview
              </TabsTrigger>
            </TabsList>

            {/* Tab 1: Basic Info */}
            <TabsContent value="basic" className="space-y-4">
              <CourseBasicInfoForm
                initialData={currentCourse}
                onSubmit={handleSaveCourse}
                isSubmitting={createCourseMutation.isPending || updateCourseMutation.isPending}
              />
            </TabsContent>

            {/* Tab 2: Lessons */}
            <TabsContent value="lessons" className="space-y-4">
              <div className="flex justify-between items-center mb-4">
                <h3 className="text-lg font-semibold">Course Lessons</h3>
                <Button onClick={handleAddLesson} disabled={!currentCourse.id}>
                  <Plus className="mr-2 h-4 w-4" />
                  Add Lesson
                </Button>
              </div>

              {!currentCourse.id && (
                <div className="bg-muted/50 border border-dashed rounded-lg p-6 text-center">
                  <p className="text-muted-foreground">
                    Please save the course basic information first before adding lessons.
                  </p>
                  <Button
                    variant="link"
                    onClick={() => setActiveTab('basic')}
                    className="mt-2"
                  >
                    Go to Basic Info
                  </Button>
                </div>
              )}

              {currentCourse.id && (
                <DraggableLessonList
                  lessons={lessons}
                  onReorder={handleReorderLessons}
                  onEdit={handleEditLesson}
                  onDelete={handleDeleteLesson}
                  isReordering={reorderLessonsMutation.isPending}
                />
              )}
            </TabsContent>

            {/* Tab 3: Preview */}
            <TabsContent value="preview">
              <CoursePreview
                course={{
                  ...currentCourse,
                  isPublished: currentCourse.isPublished || false,
                }}
                lessonCount={lessons.length}
              />
            </TabsContent>
          </Tabs>

          {/* Footer Actions */}
          <div className="flex justify-between items-center pt-4 border-t">
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Close
            </Button>

            <div className="flex gap-2">
              {/* Save Draft Button (T127) */}
              {activeTab === 'basic' && (
                <Button
                  type="submit"
                  variant="secondary"
                  onClick={() => {
                    // Trigger form submission (handled in CourseBasicInfoForm)
                  }}
                  disabled={createCourseMutation.isPending || updateCourseMutation.isPending}
                >
                  <Save className="mr-2 h-4 w-4" />
                  {createCourseMutation.isPending || updateCourseMutation.isPending
                    ? 'Saving...'
                    : 'Save Draft'}
                </Button>
              )}

              {/* Publish Button (T127) */}
              <Button
                onClick={handlePublish}
                disabled={isPublishDisabled || publishCourseMutation.isPending}
                title={
                  !currentCourse.id
                    ? 'Save course before publishing'
                    : lessons.length === 0
                    ? 'Add at least one lesson before publishing'
                    : currentCourse.isPublished
                    ? 'Course is already published'
                    : 'Publish course'
                }
              >
                {publishCourseMutation.isPending ? 'Publishing...' : 'Publish Course'}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Lesson Editor Dialog */}
      <Dialog open={showLessonEditor} onOpenChange={setShowLessonEditor}>
        <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>
              {editingLesson ? 'Edit Lesson' : 'Create New Lesson'}
            </DialogTitle>
          </DialogHeader>
          <LessonEditor
            courseId={currentCourse.id || ''}
            initialData={editingLesson || undefined}
            mode={editingLesson ? 'edit' : 'create'}
            orderIndex={lessons.length + 1}
            onSubmit={editingLesson ? handleUpdateLesson : handleCreateLesson}
            onCancel={() => {
              setEditingLesson(null);
              setShowLessonEditor(false);
            }}
            isSubmitting={createLessonMutation.isPending || updateLessonMutation.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Delete Lesson Confirmation Dialog */}
      <LessonDeleteDialog
        lesson={deletingLesson}
        open={!!deletingLesson}
        onOpenChange={(open) => !open && setDeletingLesson(null)}
        onConfirm={confirmDeleteLesson}
        isDeleting={deleteLessonMutation.isPending}
      />
    </>
  );
}
