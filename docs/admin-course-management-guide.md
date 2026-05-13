# Admin Course Management Guide

**WahadiniCryptoQuest Platform** - Course & Lesson Management System  
**For**: Platform Administrators  
**Last Updated**: November 15, 2025

---

## Overview

This guide provides step-by-step instructions for platform administrators to create, manage, and publish courses in the WahadiniCryptoQuest crypto learning platform. Courses are organized by category (Airdrops, GameFi, Task-to-Earn, DeFi, NFT Strategies) and can include multiple video lessons with YouTube integration.

**Prerequisites**:
- Admin account with role permissions
- Access to admin dashboard at `/admin/courses`
- YouTube video URLs ready for lesson content

---

## Quick Start

### 1. Access Admin Dashboard

1. Navigate to `/admin/courses` or click **Admin** in the main navigation
2. You'll see a table of all courses (published and drafts)
3. Click **Create Course** button in the top-right corner

### 2. Create a New Course

**Step 1: Basic Information**

1. In the Course Editor modal, fill in the **Basic Info** tab:
   - **Title** (required, max 200 characters): Clear, descriptive course name
   - **Description** (required, max 2000 characters): Detailed course overview with learning objectives
   - **Category** (required): Select from dropdown (Airdrops, GameFi, Task-to-Earn, DeFi, NFT Strategies)
   - **Difficulty Level** (required): Beginner, Intermediate, or Advanced
   - **Premium Course**: Toggle ON if course requires premium subscription
   - **Thumbnail URL** (optional): Direct URL to course thumbnail image (recommended size: 1280x720)
   - **Reward Points** (optional): Points users earn upon course completion (default: 0)
   - **Estimated Duration** (required, minutes): Total time to complete course

2. Click **Next** or select **Lessons** tab

**Example**:
```
Title: Airdrop Hunting Strategies for Beginners
Description: Learn how to discover and participate in cryptocurrency airdrops safely. This course covers wallet setup, risk assessment, and maximizing airdrop rewards.
Category: Airdrops
Difficulty: Beginner
Premium: OFF
Reward Points: 100
Estimated Duration: 45 minutes
```

**Step 2: Add Lessons**

1. In the **Lessons** tab, click **Add Lesson** button
2. Fill in lesson details:
   - **Title** (required, max 200 characters): Specific lesson topic
   - **Description** (optional, max 1000 characters): Lesson content overview
   - **YouTube Video URL** (required): Full YouTube URL or video ID
     - Supported formats:
       - `https://www.youtube.com/watch?v=VIDEO_ID`
       - `https://youtu.be/VIDEO_ID`
       - `VIDEO_ID` (11 characters)
   - **Duration** (required, minutes): Video length
   - **Reward Points** (optional): Points earned after completing lesson
   - **Premium Lesson**: Toggle ON if lesson requires premium subscription

3. Click **Save Lesson**
4. Repeat for all lessons (minimum 1 lesson required to publish)

**Example Lesson**:
```
Title: Setting Up a Secure Airdrop Wallet
Description: Step-by-step guide to creating a dedicated wallet for airdrop participation
YouTube URL: https://www.youtube.com/watch?v=dQw4w9WgXcQ
Duration: 12 minutes
Reward Points: 20
Premium: OFF
```

**Step 3: Reorder Lessons**

1. In the Lessons tab, drag and drop lessons to change order
2. Lessons are numbered automatically (1, 2, 3...)
3. Users will see lessons in this order on the course detail page
4. Changes are saved immediately

**Step 4: Preview and Publish**

1. Click **Preview** tab to see how the course appears to users
2. Review course details, lesson list, and layout
3. Once satisfied, click **Save as Draft** to save without publishing
4. To make course visible to users, click **Publish Course**
   - ⚠️ **Publish Requirements**:
     - At least 1 lesson must be added
     - All required fields must be filled
     - YouTube video IDs must be valid

5. Published courses appear immediately in the course catalog

---

## Managing Existing Courses

### Edit a Course

1. In the Admin Courses table, click **Edit** button on any course row
2. Make changes in the Course Editor (same tabs as creation)
3. Click **Save Draft** or **Publish** to save changes
4. **Note**: Editing a published course does not unpublish it

### Delete a Course

1. Click **Delete** button on course row
2. Confirm deletion in the dialog
3. Course is soft-deleted (marked inactive, not permanently removed)
4. Enrolled users retain access to deleted courses until completion
5. Deleted courses are hidden from public course catalog

### Unpublish a Course

1. Edit the course
2. Change status from Published to Draft
3. Course is removed from public catalog but remains in admin view
4. Enrolled users can still access unpublished courses

---

## Lesson Management

### Edit a Lesson

1. Open course in Course Editor
2. In Lessons tab, click **Edit** icon on lesson card
3. Update lesson details
4. Click **Save Lesson**

### Delete a Lesson

1. In Lessons tab, click **Delete** icon on lesson card
2. Confirm deletion
3. Lesson is soft-deleted (inactive)
4. Enrolled users lose access to deleted lessons
5. ⚠️ **Warning**: If all lessons are deleted, course cannot be published

### Add Lesson to Existing Course

1. Edit the course
2. In Lessons tab, click **Add Lesson**
3. Fill in lesson details (same as Step 2 in course creation)
4. New lessons are added to the end of the list
5. Drag-drop to reorder if needed

---

## YouTube Video Integration

### Extracting Video IDs

**Method 1: Copy Full URL**
- Navigate to YouTube video
- Copy URL from browser address bar: `https://www.youtube.com/watch?v=dQw4w9WgXcQ`
- Paste into **YouTube Video URL** field
- System automatically extracts video ID: `dQw4w9WgXcQ`

**Method 2: Use Share Button**
- Click **Share** button below YouTube video
- Copy shortened URL: `https://youtu.be/dQw4w9WgXcQ`
- Paste into field
- System extracts video ID

**Method 3: Manual Entry**
- Locate 11-character video ID in URL
- Enter ID directly: `dQw4w9WgXcQ`
- System validates format (alphanumeric, 11 characters)

**Validation Rules**:
- Must be exactly 11 characters
- Only letters (a-z, A-Z), numbers (0-9), hyphens (-), underscores (_) allowed
- Invalid examples: `abc123` (too short), `123-456-7890-xyz` (too long)

### Video Playback

- Videos are embedded using react-player component
- Users can play, pause, seek, adjust volume
- Progress is tracked automatically
- Lessons are marked complete when video reaches 90% watched

---

## Premium Course Management

### Setting Course as Premium

1. Toggle **Premium Course** in Basic Info tab
2. Only users with active premium subscriptions can enroll
3. Free users see "Upgrade to Premium" prompt on course detail page

### Setting Individual Lessons as Premium

1. Toggle **Premium Lesson** when creating/editing lesson
2. Free users enrolled in free course can view free lessons only
3. Premium lessons show lock icon 🔒 for free users

**Use Cases**:
- **Freemium Model**: First 3 lessons free, remaining premium
- **Bonus Content**: Advanced lessons marked premium
- **Full Premium**: All lessons premium, entire course premium

---

## Course Categories

Select category based on course content:

| Category | Description | Example Topics |
|----------|-------------|----------------|
| **Airdrops** | Free token distributions, wallet setup, safety | Airdrop hunting, wallet management, scam detection |
| **GameFi** | Blockchain gaming, play-to-earn, NFT games | Axie Infinity, The Sandbox, game strategies |
| **Task-to-Earn** | Completing tasks for crypto rewards | Bounties, social media campaigns, testing |
| **DeFi** | Decentralized finance, lending, yield farming | Uniswap, Aave, liquidity pools, impermanent loss |
| **NFT Strategies** | Non-fungible tokens, minting, trading | OpenSea, NFT art, flipping strategies |

---

## Best Practices

### Course Creation

✅ **Do**:
- Write clear, concise titles (avoid clickbait)
- Include learning objectives in description
- Use high-quality thumbnail images (1280x720)
- Set appropriate difficulty level
- Add reward points to incentivize completion
- Preview course before publishing

❌ **Don't**:
- Use copyrighted content without permission
- Publish courses with zero lessons
- Set unrealistic reward points (inflates leaderboard)
- Use broken YouTube video links
- Forget to set premium flag if content is restricted

### Lesson Organization

✅ **Do**:
- Order lessons logically (beginner → advanced)
- Keep lessons focused (10-15 minutes ideal)
- Use descriptive lesson titles
- Add reward points for each lesson
- Test video playback before publishing

❌ **Don't**:
- Create single-lesson courses (split into multiple courses)
- Mix difficulty levels within one course
- Use extremely long videos (>60 minutes)
- Skip lesson descriptions (helps SEO and user discovery)

### Content Quality

✅ **Do**:
- Verify YouTube videos are public (not private/unlisted)
- Check audio quality and clarity
- Ensure content is accurate and up-to-date
- Update courses when information becomes outdated
- Respond to user feedback in course comments

❌ **Don't**:
- Use AI-generated content without review
- Publish placeholder/incomplete lessons
- Ignore broken video links
- Leave courses unpublished for extended periods

---

## Troubleshooting

### "Cannot publish course with zero lessons"

**Problem**: Publish button is disabled or error appears  
**Solution**: Add at least 1 lesson to the course before publishing

### "Invalid YouTube video ID"

**Problem**: Lesson save fails with validation error  
**Solution**: Verify video ID is exactly 11 characters, alphanumeric only. Test video URL in YouTube to confirm it's valid.

### "Video not playing in preview"

**Problem**: YouTube player shows error or black screen  
**Solution**: 
- Check if video is public (not private or unlisted)
- Verify video hasn't been deleted by creator
- Try a different video to test embed functionality
- Clear browser cache and refresh

### "Lessons not reordering"

**Problem**: Drag-drop doesn't update lesson order  
**Solution**:
- Ensure you're in Edit mode (not Preview)
- Try refreshing the page
- Check for JavaScript errors in browser console
- Contact technical support if issue persists

### "Premium users can't access premium course"

**Problem**: User reports access denied despite premium subscription  
**Solution**:
- Verify user's subscription status in User Management
- Check course premium flag is set correctly
- Confirm lesson premium flags match course settings
- Review subscription payment status with Stripe dashboard

---

## API Endpoints for Advanced Users

Administrators with API access can manage courses programmatically:

**Create Course**
```http
POST /api/courses
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "title": "Course Title",
  "description": "Course description",
  "categoryId": "uuid",
  "difficultyLevel": "Beginner",
  "isPremium": false,
  "rewardPoints": 100,
  "estimatedDuration": 45
}
```

**Add Lesson**
```http
POST /api/courses/{courseId}/lessons
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "title": "Lesson Title",
  "description": "Lesson description",
  "youtubeVideoId": "dQw4w9WgXcQ",
  "duration": 12,
  "rewardPoints": 20,
  "isPremium": false
}
```

**Reorder Lessons**
```http
PUT /api/lessons/reorder
Authorization: Bearer {admin_token}
Content-Type: application/json

[
  {"lessonId": "uuid1", "orderIndex": 1},
  {"lessonId": "uuid2", "orderIndex": 2},
  {"lessonId": "uuid3", "orderIndex": 3}
]
```

**Publish Course**
```http
PUT /api/courses/{id}/publish
Authorization: Bearer {admin_token}
```

Full API documentation available at `/swagger` when logged in as admin.

---

## Support

**Questions?**
- Contact: admin@wahadinicryptoquest.com
- Slack Channel: #admin-support
- Documentation: `/docs`

**Found a bug?**
- GitHub Issues: https://github.com/wahadinicryptoquest/platform/issues
- Tag with `admin-dashboard` label

---

## Changelog

### Version 1.0 (November 2025)
- Initial admin course management guide
- YouTube video integration documented
- Premium course management added
- Lesson reordering instructions included
