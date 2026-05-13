import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import type { Lesson } from '@/types/course.types';
import { AlertTriangle } from 'lucide-react';

interface LessonDeleteDialogProps {
  lesson: Lesson | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
  isDeleting?: boolean;
}

/**
 * Lesson Delete Confirmation Dialog
 * Shows lesson details and asks for confirmation before deletion
 */
export function LessonDeleteDialog({
  lesson,
  open,
  onOpenChange,
  onConfirm,
  isDeleting,
}: LessonDeleteDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <div className="flex items-center gap-2 mb-2">
            <AlertTriangle className="h-5 w-5 text-destructive" />
            <DialogTitle>Delete Lesson</DialogTitle>
          </div>
          <DialogDescription>
            Are you sure you want to delete this lesson? This action cannot be undone.
          </DialogDescription>
        </DialogHeader>

        {lesson && (
          <div className="py-4">
            <div className="rounded-lg border bg-muted/50 p-4 space-y-2">
              <div>
                <span className="text-sm font-medium">Title:</span>
                <p className="text-sm text-muted-foreground">{lesson.title}</p>
              </div>
              {lesson.description && (
                <div>
                  <span className="text-sm font-medium">Description:</span>
                  <p className="text-sm text-muted-foreground line-clamp-2">
                    {lesson.description}
                  </p>
                </div>
              )}
              <div className="flex items-center gap-4 text-sm">
                <span>
                  <span className="font-medium">Duration:</span>{' '}
                  <span className="text-muted-foreground">{lesson.duration} min</span>
                </span>
                <span>
                  <span className="font-medium">Order:</span>{' '}
                  <span className="text-muted-foreground">#{lesson.orderIndex}</span>
                </span>
              </div>
              {lesson.isPremium && (
                <p className="text-sm text-amber-600 dark:text-amber-400">
                  This is a premium lesson
                </p>
              )}
            </div>
          </div>
        )}

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={isDeleting}
          >
            Cancel
          </Button>
          <Button
            type="button"
            variant="destructive"
            onClick={onConfirm}
            disabled={isDeleting}
          >
            {isDeleting ? 'Deleting...' : 'Delete Lesson'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
