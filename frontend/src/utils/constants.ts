/**
 * Application constants for WahadiniCryptoQuest
 * Course categories, difficulty levels, and other enums
 */

/**
 * Course difficulty levels (must match backend DifficultyLevel enum)
 */
export const DIFFICULTY_LEVELS = {
  Beginner: 0,
  Intermediate: 1,
  Advanced: 2,
  Expert: 3,
} as const;

export type DifficultyLevel = (typeof DIFFICULTY_LEVELS)[keyof typeof DIFFICULTY_LEVELS];

/**
 * Difficulty level display names
 */
export const DIFFICULTY_LABELS: Record<number, string> = {
  [DIFFICULTY_LEVELS.Beginner]: 'Beginner',
  [DIFFICULTY_LEVELS.Intermediate]: 'Intermediate',
  [DIFFICULTY_LEVELS.Advanced]: 'Advanced',
  [DIFFICULTY_LEVELS.Expert]: 'Expert',
};

/**
 * Difficulty level colors for badges
 */
export const DIFFICULTY_COLORS: Record<number, string> = {
  [DIFFICULTY_LEVELS.Beginner]: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  [DIFFICULTY_LEVELS.Intermediate]: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
  [DIFFICULTY_LEVELS.Advanced]: 'bg-orange-100 text-orange-800 dark:bg-orange-900/30 dark:text-orange-400',
  [DIFFICULTY_LEVELS.Expert]: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
};

/**
 * Course categories (seeded in backend)
 * Note: Actual category IDs come from database, these are display names
 */
export const COURSE_CATEGORIES = {
  AIRDROPS: 'Airdrops',
  GAMEFI: 'GameFi',
  TASK_TO_EARN: 'Task-to-Earn',
  DEFI: 'DeFi',
  NFT_STRATEGIES: 'NFT Strategies',
} as const;

/**
 * Category icons (if using icon library)
 */
export const CATEGORY_ICONS: Record<string, string> = {
  'Airdrops': '🎁',
  'GameFi': '🎮',
  'Task-to-Earn': '💼',
  'DeFi': '💰',
  'NFT Strategies': '🖼️',
};

/**
 * Course status for admin panel
 */
export const COURSE_STATUS = {
  DRAFT: 'Draft',
  PUBLISHED: 'Published',
} as const;

/**
 * Course status colors
 */
export const STATUS_COLORS: Record<string, string> = {
  Draft: 'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300',
  Published: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
};

/**
 * Pagination defaults
 */
export const PAGINATION_DEFAULTS = {
  PAGE_SIZE: 12,
  PAGE_SIZE_OPTIONS: [6, 12, 24, 48],
} as const;

/**
 * Video duration limits (in minutes)
 */
export const VIDEO_DURATION = {
  MIN: 1,
  MAX: 180, // 3 hours
  DEFAULT: 10,
} as const;

/**
 * Reward points ranges
 */
export const REWARD_POINTS = {
  MIN: 0,
  MAX: 1000,
  DEFAULT: 10,
} as const;

/**
 * Course validation limits
 */
export const COURSE_LIMITS = {
  TITLE_MIN: 3,
  TITLE_MAX: 200,
  DESCRIPTION_MIN: 10,
  DESCRIPTION_MAX: 2000,
  MIN_LESSONS_TO_PUBLISH: 1,
} as const;

/**
 * Lesson validation limits
 */
export const LESSON_LIMITS = {
  TITLE_MIN: 3,
  TITLE_MAX: 200,
  DESCRIPTION_MIN: 10,
  DESCRIPTION_MAX: 1000,
} as const;
