import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { createLessonSchema, type CreateLessonFormData, type UpdateLessonFormData } from '@/services/validation/course.validation';
import { extractYouTubeVideoId, isValidYouTubeVideoId, buildYouTubeThumbnailUrl } from '@/utils/youtube.helpers';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { LESSON_LIMITS, REWARD_POINTS, VIDEO_DURATION } from '@/utils/constants';
import { AlertCircle, CheckCircle2, X } from 'lucide-react';

interface LessonEditorProps {
  courseId: string;
  initialData?: Partial<CreateLessonFormData>;
  mode?: 'create' | 'edit';
  orderIndex?: number;
  onSubmit: (data: CreateLessonFormData | UpdateLessonFormData) => void;
  onCancel?: () => void;
  isSubmitting?: boolean;
}

/**
 * Lesson Editor Form
 * Create or edit lessons with YouTube video integration
 */
export function LessonEditor({
  courseId,
  initialData,
  mode = 'create',
  orderIndex = 1,
  onSubmit,
  onCancel,
  isSubmitting,
}: LessonEditorProps) {
  const [youtubeUrl, setYoutubeUrl] = useState('');
  const [videoIdError, setVideoIdError] = useState<string | null>(null);

  // Use createLessonSchema for both modes, we'll handle the id in the onSubmit callback
  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
  } = useForm<CreateLessonFormData>({
    resolver: zodResolver(createLessonSchema),
    defaultValues: {
      courseId,
      title: '',
      description: '',
      youTubeVideoId: '',
      duration: VIDEO_DURATION.DEFAULT,
      orderIndex: orderIndex,
      isPremium: false,
      rewardPoints: REWARD_POINTS.DEFAULT,
      contentMarkdown: null,
      ...initialData,
    },
  });

  const youTubeVideoId = watch('youTubeVideoId');

  const handleYouTubeUrlChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const url = e.target.value;
    setYoutubeUrl(url);
    setVideoIdError(null);

    if (url.trim()) {
      const videoId = extractYouTubeVideoId(url);
      if (videoId && isValidYouTubeVideoId(videoId)) {
        setValue('youTubeVideoId', videoId, { shouldValidate: true });
        setVideoIdError(null);
      } else {
        setVideoIdError('Invalid YouTube URL. Please enter a valid YouTube video link.');
        setValue('youTubeVideoId', '', { shouldValidate: false });
      }
    } else {
      setValue('youTubeVideoId', '', { shouldValidate: false });
    }
  };

  const handleVideoIdChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const videoId = e.target.value;
    setValue('youTubeVideoId', videoId, { shouldValidate: true });
    
    if (videoId && !isValidYouTubeVideoId(videoId)) {
      setVideoIdError('Invalid YouTube Video ID format.');
    } else {
      setVideoIdError(null);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      {/* Title */}
      <div className="space-y-2">
        <Label htmlFor="title">
          Lesson Title <span className="text-destructive">*</span>
        </Label>
        <Input
          id="title"
          {...register('title')}
          placeholder="e.g., Understanding Airdrop Mechanics"
          disabled={isSubmitting}
        />
        {errors.title && (
          <p className="text-sm text-destructive">{errors.title.message}</p>
        )}
        <p className="text-xs text-muted-foreground">
          {LESSON_LIMITS.TITLE_MIN}-{LESSON_LIMITS.TITLE_MAX} characters
        </p>
      </div>

      {/* Description */}
      <div className="space-y-2">
        <Label htmlFor="description">Description</Label>
        <textarea
          id="description"
          {...register('description')}
          placeholder="Brief description of what this lesson covers..."
          rows={3}
          disabled={isSubmitting}
          className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
        />
        {errors.description && (
          <p className="text-sm text-destructive">{errors.description.message}</p>
        )}
        <p className="text-xs text-muted-foreground">
          Max {LESSON_LIMITS.DESCRIPTION_MAX} characters
        </p>
      </div>

      {/* YouTube URL Input with Extraction Helper */}
      <div className="space-y-2">
        <Label htmlFor="youtubeUrl">
          YouTube Video URL <span className="text-destructive">*</span>
        </Label>
        <Input
          id="youtubeUrl"
          value={youtubeUrl}
          onChange={handleYouTubeUrlChange}
          placeholder="https://www.youtube.com/watch?v=dQw4w9WgXcQ"
          disabled={isSubmitting}
        />
        {videoIdError && (
          <div className="flex items-center gap-2 text-sm text-destructive">
            <AlertCircle className="h-4 w-4" />
            <span>{videoIdError}</span>
          </div>
        )}
        {youTubeVideoId && !videoIdError && (
          <div className="flex items-center gap-2 text-sm text-green-600 dark:text-green-400">
            <CheckCircle2 className="h-4 w-4" />
            <span>Video ID extracted successfully</span>
          </div>
        )}
        <p className="text-xs text-muted-foreground">
          Paste any YouTube URL (watch, embed, short link, etc.)
        </p>
      </div>

      {/* YouTube Video ID (Manual Entry or Display) */}
      <div className="space-y-2">
        <Label htmlFor="youTubeVideoId">
          YouTube Video ID <span className="text-destructive">*</span>
        </Label>
        <Input
          id="youTubeVideoId"
          value={youTubeVideoId || ''}
          onChange={handleVideoIdChange}
          placeholder="dQw4w9WgXcQ"
          disabled={isSubmitting}
          maxLength={11}
        />
        {errors.youTubeVideoId && (
          <p className="text-sm text-destructive">{errors.youTubeVideoId.message}</p>
        )}
        <p className="text-xs text-muted-foreground">
          11-character alphanumeric ID (auto-extracted from URL above)
        </p>
      </div>

      {/* Video Preview Thumbnail */}
      {youTubeVideoId && isValidYouTubeVideoId(youTubeVideoId) && (
        <div className="space-y-2">
          <Label>Video Preview</Label>
          <div className="relative aspect-video w-full max-w-md overflow-hidden rounded-md border bg-muted">
            <img
              src={buildYouTubeThumbnailUrl(youTubeVideoId, 'high')}
              alt="YouTube video thumbnail"
              className="h-full w-full object-cover"
              onError={(e) => {
                e.currentTarget.src = '/placeholder-video.png';
              }}
            />
            <div className="absolute inset-0 flex items-center justify-center bg-black/30">
              <div className="h-16 w-16 rounded-full bg-red-600 flex items-center justify-center">
                <div className="ml-1 h-0 w-0 border-l-[16px] border-l-white border-t-[10px] border-t-transparent border-b-[10px] border-b-transparent" />
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Duration */}
      <div className="space-y-2">
        <Label htmlFor="duration">
          Duration (minutes) <span className="text-destructive">*</span>
        </Label>
        <Input
          id="duration"
          {...register('duration', { valueAsNumber: true })}
          type="number"
          min={VIDEO_DURATION.MIN}
          max={VIDEO_DURATION.MAX}
          disabled={isSubmitting}
        />
        {errors.duration && (
          <p className="text-sm text-destructive">{errors.duration.message}</p>
        )}
        <p className="text-xs text-muted-foreground">
          {VIDEO_DURATION.MIN}-{VIDEO_DURATION.MAX} minutes
        </p>
      </div>

      {/* Order Index */}
      <div className="space-y-2">
        <Label htmlFor="orderIndex">
          Order Index <span className="text-destructive">*</span>
        </Label>
        <Input
          id="orderIndex"
          {...register('orderIndex', { valueAsNumber: true })}
          type="number"
          min={1}
          disabled={isSubmitting}
        />
        {errors.orderIndex && (
          <p className="text-sm text-destructive">{errors.orderIndex.message}</p>
        )}
        <p className="text-xs text-muted-foreground">
          Position of this lesson in the course (can be reordered via drag-drop)
        </p>
      </div>

      {/* Reward Points */}
      <div className="space-y-2">
        <Label htmlFor="rewardPoints">Reward Points</Label>
        <Input
          id="rewardPoints"
          {...register('rewardPoints', { valueAsNumber: true })}
          type="number"
          min={REWARD_POINTS.MIN}
          max={REWARD_POINTS.MAX}
          disabled={isSubmitting}
        />
        {errors.rewardPoints && (
          <p className="text-sm text-destructive">{errors.rewardPoints.message}</p>
        )}
        <p className="text-xs text-muted-foreground">
          Points awarded upon lesson completion ({REWARD_POINTS.MIN}-{REWARD_POINTS.MAX})
        </p>
      </div>

      {/* Premium Toggle */}
      <div className="flex items-center space-x-2">
        <Checkbox
          id="isPremium"
          {...register('isPremium')}
          disabled={isSubmitting}
        />
        <Label
          htmlFor="isPremium"
          className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
        >
          Premium Lesson (requires subscription)
        </Label>
      </div>
      {errors.isPremium && (
        <p className="text-sm text-destructive">{errors.isPremium.message}</p>
      )}

      {/* Content Markdown (Optional) */}
      <div className="space-y-2">
        <Label htmlFor="contentMarkdown">
          Additional Content (Markdown)
        </Label>
        <textarea
          id="contentMarkdown"
          {...register('contentMarkdown')}
          placeholder="Optional markdown content for supplementary materials..."
          rows={6}
          disabled={isSubmitting}
          className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm font-mono ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
        />
        {errors.contentMarkdown && (
          <p className="text-sm text-destructive">{errors.contentMarkdown.message}</p>
        )}
        <p className="text-xs text-muted-foreground">
          Optional supplementary content in Markdown format
        </p>
      </div>

      {/* Form Actions */}
      <div className="flex justify-end gap-3 pt-4 border-t">
        {onCancel && (
          <Button type="button" variant="outline" onClick={onCancel} disabled={isSubmitting}>
            <X className="mr-2 h-4 w-4" />
            Cancel
          </Button>
        )}
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Saving...' : mode === 'edit' ? 'Update Lesson' : 'Create Lesson'}
        </Button>
      </div>
    </form>
  );
}
