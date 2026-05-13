import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { createCourseSchema } from '@/services/validation/course.validation';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { DIFFICULTY_LEVELS, COURSE_LIMITS, REWARD_POINTS, VIDEO_DURATION } from '@/utils/constants';

// Infer the form data type from the schema
type CreateCourseFormData = z.infer<typeof createCourseSchema>;

interface CourseBasicInfoFormProps {
  initialData?: Partial<CreateCourseFormData>;
  onSubmit: (data: CreateCourseFormData) => void;
  isSubmitting?: boolean;
}

/**
 * Course Basic Info Form
 * React Hook Form with Zod validation for course creation/editing
 */
export function CourseBasicInfoForm({ initialData, onSubmit, isSubmitting }: CourseBasicInfoFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
  } = useForm<CreateCourseFormData>({
    resolver: zodResolver(createCourseSchema),
    defaultValues: initialData || {
      title: '',
      description: '',
      categoryId: '',
      difficultyLevel: DIFFICULTY_LEVELS.Beginner,
      isPremium: false,
      rewardPoints: REWARD_POINTS.DEFAULT,
      estimatedDuration: VIDEO_DURATION.DEFAULT,
      thumbnailUrl: null,
    },
  });

  const thumbnailUrl = watch('thumbnailUrl');

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      {/* Title */}
      <div className="space-y-2">
        <Label htmlFor="title">
          Course Title <span className="text-destructive">*</span>
        </Label>
        <Input
          id="title"
          {...register('title')}
          placeholder="e.g., Introduction to Crypto Airdrops"
          disabled={isSubmitting}
        />
        {errors.title && (
          <p className="text-sm text-destructive">{errors.title.message}</p>
        )}
        <p className="text-xs text-muted-foreground">
          {COURSE_LIMITS.TITLE_MIN}-{COURSE_LIMITS.TITLE_MAX} characters
        </p>
      </div>

      {/* Description */}
      <div className="space-y-2">
        <Label htmlFor="description">Description</Label>
        <textarea
          id="description"
          {...register('description')}
          placeholder="Describe what students will learn in this course..."
          rows={4}
          disabled={isSubmitting}
          className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
        />
        {errors.description && (
          <p className="text-sm text-destructive">{errors.description.message}</p>
        )}
        <p className="text-xs text-muted-foreground">
          Max {COURSE_LIMITS.DESCRIPTION_MAX} characters
        </p>
      </div>

      {/* Category Selection */}
      <div className="space-y-2">
        <Label htmlFor="categoryId">
          Category <span className="text-destructive">*</span>
        </Label>
        <select
          id="categoryId"
          {...register('categoryId')}
          disabled={isSubmitting}
          className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
        >
          <option value="">Select a category</option>
          {/* TODO: Load categories from API */}
          <option value="placeholder-1">Airdrops</option>
          <option value="placeholder-2">GameFi</option>
          <option value="placeholder-3">Task-to-Earn</option>
          <option value="placeholder-4">DeFi</option>
          <option value="placeholder-5">NFT Strategies</option>
        </select>
        {errors.categoryId && (
          <p className="text-sm text-destructive">{errors.categoryId.message}</p>
        )}
      </div>

      {/* Difficulty Level */}
      <div className="space-y-2">
        <Label htmlFor="difficultyLevel">
          Difficulty Level <span className="text-destructive">*</span>
        </Label>
        <select
          id="difficultyLevel"
          {...register('difficultyLevel', { valueAsNumber: true })}
          disabled={isSubmitting}
          className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
        >
          <option value={DIFFICULTY_LEVELS.Beginner}>Beginner</option>
          <option value={DIFFICULTY_LEVELS.Intermediate}>Intermediate</option>
          <option value={DIFFICULTY_LEVELS.Advanced}>Advanced</option>
          <option value={DIFFICULTY_LEVELS.Expert}>Expert</option>
        </select>
        {errors.difficultyLevel && (
          <p className="text-sm text-destructive">{errors.difficultyLevel.message}</p>
        )}
      </div>

      {/* Thumbnail URL */}
      <div className="space-y-2">
        <Label htmlFor="thumbnailUrl">Thumbnail URL</Label>
        <Input
          id="thumbnailUrl"
          {...register('thumbnailUrl')}
          type="url"
          placeholder="https://example.com/image.jpg"
          disabled={isSubmitting}
        />
        {errors.thumbnailUrl && (
          <p className="text-sm text-destructive">{errors.thumbnailUrl.message}</p>
        )}
        {thumbnailUrl && (
          <div className="mt-2">
            <img
              src={thumbnailUrl}
              alt="Thumbnail preview"
              className="h-32 w-auto rounded-md border object-cover"
              onError={(e) => {
                e.currentTarget.src = '/placeholder-course.png';
              }}
            />
          </div>
        )}
      </div>

      {/* Estimated Duration */}
      <div className="space-y-2">
        <Label htmlFor="estimatedDuration">
          Estimated Duration (minutes) <span className="text-destructive">*</span>
        </Label>
        <Input
          id="estimatedDuration"
          {...register('estimatedDuration', { valueAsNumber: true })}
          type="number"
          min={VIDEO_DURATION.MIN}
          max={VIDEO_DURATION.MAX}
          disabled={isSubmitting}
        />
        {errors.estimatedDuration && (
          <p className="text-sm text-destructive">{errors.estimatedDuration.message}</p>
        )}
        <p className="text-xs text-muted-foreground">
          {VIDEO_DURATION.MIN}-{VIDEO_DURATION.MAX} minutes
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
          Points awarded upon course completion ({REWARD_POINTS.MIN}-{REWARD_POINTS.MAX})
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
          Premium Course (requires subscription)
        </Label>
      </div>
      {errors.isPremium && (
        <p className="text-sm text-destructive">{errors.isPremium.message}</p>
      )}

      {/* Submit Button */}
      <div className="flex justify-end gap-3">
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Saving...' : 'Save Course'}
        </Button>
      </div>
    </form>
  );
}
