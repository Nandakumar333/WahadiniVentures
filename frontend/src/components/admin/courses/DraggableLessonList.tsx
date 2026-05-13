import { useState } from 'react';
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
} from '@dnd-kit/core';
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  verticalListSortingStrategy,
  useSortable,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import type { Lesson } from '@/types/course.types';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { GripVertical, Edit, Trash2, Lock } from 'lucide-react';

interface DraggableLessonListProps {
  lessons: Lesson[];
  onReorder: (lessonOrderMap: Record<string, number>) => void;
  onEdit: (lesson: Lesson) => void;
  onDelete: (lessonId: string) => void;
  isReordering?: boolean;
}

interface SortableItemProps {
  lesson: Lesson;
  onEdit: (lesson: Lesson) => void;
  onDelete: (lessonId: string) => void;
}

/**
 * Sortable lesson item component
 */
function SortableItem({ lesson, onEdit, onDelete }: SortableItemProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: lesson.id });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
  };

  return (
    <Card
      ref={setNodeRef}
      style={style}
      className={`p-4 ${isDragging ? 'shadow-lg' : ''}`}
    >
      <div className="flex items-center gap-4">
        {/* Drag Handle */}
        <button
          type="button"
          {...attributes}
          {...listeners}
          className="cursor-grab active:cursor-grabbing text-muted-foreground hover:text-foreground focus:outline-none"
          aria-label="Drag to reorder"
        >
          <GripVertical className="h-5 w-5" />
        </button>

        {/* Order Index */}
        <div className="flex-shrink-0 w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center font-semibold text-primary text-sm">
          {lesson.orderIndex}
        </div>

        {/* Lesson Info */}
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 mb-1">
            <h4 className="font-medium truncate">{lesson.title}</h4>
            {lesson.isPremium && (
              <Badge variant="secondary" className="flex items-center gap-1">
                <Lock className="h-3 w-3" />
                Premium
              </Badge>
            )}
          </div>
          {lesson.description && (
            <p className="text-sm text-muted-foreground line-clamp-1">
              {lesson.description}
            </p>
          )}
          <div className="flex items-center gap-3 mt-2 text-xs text-muted-foreground">
            <span>Duration: {lesson.duration} min</span>
            <span>•</span>
            <span>Reward: {lesson.rewardPoints} pts</span>
            {lesson.youTubeVideoId && (
              <>
                <span>•</span>
                <span className="truncate max-w-[100px]">
                  Video ID: {lesson.youTubeVideoId}
                </span>
              </>
            )}
          </div>
        </div>

        {/* Actions */}
        <div className="flex items-center gap-2">
          <Button
            type="button"
            variant="ghost"
            size="sm"
            onClick={() => onEdit(lesson)}
            aria-label={`Edit ${lesson.title}`}
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button
            type="button"
            variant="ghost"
            size="sm"
            onClick={() => onDelete(lesson.id)}
            className="text-destructive hover:text-destructive"
            aria-label={`Delete ${lesson.title}`}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      </div>
    </Card>
  );
}

/**
 * Draggable Lesson List Component
 * Uses @dnd-kit for drag-and-drop reordering of lessons
 */
export function DraggableLessonList({
  lessons,
  onReorder,
  onEdit,
  onDelete,
  isReordering,
}: DraggableLessonListProps) {
  const [items, setItems] = useState<Lesson[]>(lessons);

  // Configure sensors for drag interactions
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 8, // Drag must move 8px before activating
      },
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  /**
   * Handle drag end event
   * Updates local state and calls onReorder with new order mapping
   */
  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;

    if (over && active.id !== over.id) {
      setItems((prevItems) => {
        const oldIndex = prevItems.findIndex((item) => item.id === active.id);
        const newIndex = prevItems.findIndex((item) => item.id === over.id);

        // Reorder array
        const reorderedItems = arrayMove(prevItems, oldIndex, newIndex);

        // Create order map: lessonId -> new orderIndex
        const lessonOrderMap: Record<string, number> = {};
        reorderedItems.forEach((lesson, index) => {
          lessonOrderMap[lesson.id] = index + 1; // orderIndex starts at 1
        });

        // Call parent callback with new order
        onReorder(lessonOrderMap);

        return reorderedItems;
      });
    }
  };

  // Update items when lessons prop changes
  if (items !== lessons && items.length !== lessons.length) {
    setItems(lessons);
  }

  if (items.length === 0) {
    return (
      <div className="text-center py-12 text-muted-foreground">
        <p>No lessons yet. Add your first lesson to get started.</p>
      </div>
    );
  }

  return (
    <DndContext
      sensors={sensors}
      collisionDetection={closestCenter}
      onDragEnd={handleDragEnd}
    >
      <SortableContext
        items={items.map((lesson) => lesson.id)}
        strategy={verticalListSortingStrategy}
        disabled={isReordering}
      >
        <div className="space-y-3">
          {items.map((lesson) => (
            <SortableItem
              key={lesson.id}
              lesson={lesson}
              onEdit={onEdit}
              onDelete={onDelete}
            />
          ))}
        </div>
      </SortableContext>
    </DndContext>
  );
}
