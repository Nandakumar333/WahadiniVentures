import { useState, useEffect, useRef, memo, useCallback, useMemo } from 'react';
import YouTube from 'react-youtube';
import { Loader2, Play, Pause, Check, Volume2, VolumeX, Maximize } from 'lucide-react';
import type { Lesson } from '@/types/lesson';
import { useVideoProgress } from '@/hooks/lesson/useVideoProgress';
import { ResumePrompt } from '@/components/lesson/ResumePrompt/ResumePrompt';
import { PointsNotification } from '@/components/reward/PointsNotification/PointsNotification';
import { PremiumVideoGate } from '@/components/subscription/PremiumVideoGate/PremiumVideoGate';
import { useAuthStore } from '@/store/authStore';
import { formatSeconds } from '@/utils/formatters/time.formatter';
import { sanitizeYouTubeVideoId } from '@/utils/validation/urlSanitizer';

interface LessonPlayerProps {
  lesson: Lesson;
}

function LessonPlayerComponent({ lesson }: LessonPlayerProps) {
  const [playing, setPlaying] = useState(false);
  const [loaded, setLoaded] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [currentPosition, setCurrentPosition] = useState(0);
  const [duration, setDuration] = useState(0);
  const [showResumePrompt, setShowResumePrompt] = useState(false);
  const [isSeeking, setIsSeeking] = useState(false);
  const [showPointsNotification, setShowPointsNotification] = useState(false);
  const [pointsEarned, setPointsEarned] = useState(0);
  const [playbackRate, setPlaybackRate] = useState(1);
  const [volume] = useState(1);
  const [muted, setMuted] = useState(false);
  
  // Reference to the YouTube player instance
  const playerRef = useRef<any>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  const { isPremium } = useAuthStore();
  const sanitizedVideoId = sanitizeYouTubeVideoId(lesson.youTubeVideoId);
  const hasAccess = !lesson.isPremium || isPremium();

  const handleComplete = useCallback((pointsAwarded: number) => {
    setPointsEarned(pointsAwarded);
    setShowPointsNotification(true);
  }, []);

  // Progress tracking hook
  const { saveProgress, lastSavedPosition, savedProgress } = useVideoProgress({
    lessonId: lesson.id,
    videoDuration: lesson.videoDuration || lesson.duration * 60,
    onComplete: handleComplete,
  });

  const videoDuration = duration || lesson.videoDuration || lesson.duration * 60;
  // Poll for progress updates since react-youtube doesn't have onProgress
  useEffect(() => {
    let interval: NodeJS.Timeout;

    if (playing && playerRef.current && !isSeeking) {
      interval = setInterval(() => {
        try {
          const currentTime = playerRef.current.getCurrentTime();
          if (typeof currentTime === 'number') {
            setCurrentPosition(currentTime);
            saveProgress(currentTime);
            
            // Log occasionally
            if (Math.floor(currentTime) % 5 === 0) {
               console.log(`[Player] Time: ${currentTime.toFixed(1)}s, Duration: ${videoDuration}s`);
            }
          }
        } catch (e) {
          console.error('[Player] Error getting time:', e);
        }
      }, 1000);
    }

    return () => {
      if (interval) clearInterval(interval);
    };
  }, [playing, isSeeking, saveProgress, videoDuration]);

  // Handle player ready
  const onPlayerReady = useCallback((event: any) => {
    playerRef.current = event.target;
    setLoaded(true);
    setError(null);
    
    const dur = event.target.getDuration();
    if (dur) {
      setDuration(dur);
      console.log(`[Player] Duration set: ${dur}s`);
    }

    // Apply initial settings
    event.target.setVolume(volume * 100);
    if (muted) event.target.mute();
    event.target.setPlaybackRate(playbackRate);
  }, [volume, muted, playbackRate]);

  // Handle player state change
  const onPlayerStateChange = useCallback((event: any) => {
    // Player states: -1 (unstarted), 0 (ended), 1 (playing), 2 (paused), 3 (buffering), 5 (video cued)
    const playerState = event.data;
    const isPlaying = playerState === 1;
    setPlaying(isPlaying);

    if (playerState === 2) { // Paused
      if (currentPosition > 0) {
        saveProgress(currentPosition, true);
      }
    }
  }, [currentPosition, saveProgress]);

  const onPlayerError = useCallback((event: any) => {
    console.error('[Player] Video error:', event.data);
    setError('Video unavailable. Please try again later.');
    setLoaded(true);
  }, []);

  const handlePlayPause = () => {
    if (playerRef.current) {
      if (playing) {
        playerRef.current.pauseVideo();
      } else {
        playerRef.current.playVideo();
      }
    }
  };



  const handleSeek = (e: React.MouseEvent<HTMLDivElement>) => {
    if (!playerRef.current || !videoDuration) return;
    
    const bounds = e.currentTarget.getBoundingClientRect();
    const percent = (e.clientX - bounds.left) / bounds.width;
    const seekTo = percent * videoDuration;
    
    setCurrentPosition(seekTo);
    playerRef.current.seekTo(seekTo, true);
  };

  const handleVolumeToggle = () => {
    const newMuted = !muted;
    setMuted(newMuted);
    if (playerRef.current) {
      if (newMuted) {
        playerRef.current.mute();
      } else {
        playerRef.current.unMute();
      }
    }
  };

  const handleFullscreen = () => {
    if (containerRef.current?.requestFullscreen) {
      containerRef.current.requestFullscreen();
    }
  };

  const handlePlaybackRateChange = (rate: number) => {
    setPlaybackRate(rate);
    localStorage.setItem('videoPlaybackSpeed', rate.toString());
    if (playerRef.current) {
      playerRef.current.setPlaybackRate(rate);
    }
  };

  const handleResume = () => {
    if (playerRef.current && savedProgress?.lastWatchedPosition) {
      setIsSeeking(true);
      playerRef.current.seekTo(savedProgress.lastWatchedPosition, true);
      setShowResumePrompt(false);
      playerRef.current.playVideo();
      setTimeout(() => setIsSeeking(false), 500);
    }
  };

  const handleStartFromBeginning = () => {
    if (playerRef.current) {
      setIsSeeking(true);
      playerRef.current.seekTo(0, true);
      setShowResumePrompt(false);
      playerRef.current.playVideo();
      setTimeout(() => setIsSeeking(false), 500);
    }
  };

  // Show resume prompt
  useEffect(() => {
    if (loaded && savedProgress?.lastWatchedPosition && savedProgress.lastWatchedPosition > 5) {
      if (savedProgress.lastWatchedPosition <= videoDuration) {
        setShowResumePrompt(true);
      }
    }
  }, [loaded, savedProgress, videoDuration]);

  // Refs for cleanup
  const currentPositionRef = useRef(currentPosition);
  const saveProgressRef = useRef(saveProgress);

  useEffect(() => {
    currentPositionRef.current = currentPosition;
  }, [currentPosition]);

  useEffect(() => {
    saveProgressRef.current = saveProgress;
  }, [saveProgress]);

  // Save on unmount
  useEffect(() => {
    return () => {
      if (currentPositionRef.current > 0) {
        saveProgressRef.current(currentPositionRef.current, true);
      }
    };
  }, []);

  // Save on tab switch
  useEffect(() => {
    const handleVisibilityChange = () => {
      if (document.hidden && currentPositionRef.current > 0) {
        saveProgressRef.current(currentPositionRef.current, true);
      }
    };
    document.addEventListener('visibilitychange', handleVisibilityChange);
    return () => document.removeEventListener('visibilitychange', handleVisibilityChange);
  }, []);

  // Load saved playback speed
  useEffect(() => {
    const saved = localStorage.getItem('videoPlaybackSpeed');
    if (saved) setPlaybackRate(parseFloat(saved));
  }, []);

  if (!sanitizedVideoId) {
    return (
      <div className="w-full aspect-video bg-gray-900 rounded-lg flex items-center justify-center">
        <div className="text-center text-white p-8">
          <p className="text-xl mb-2">Invalid video</p>
          <p className="text-gray-400">The video ID is not valid or may contain unsafe content.</p>
        </div>
      </div>
    );
  }

  if (!hasAccess) {
    return <PremiumVideoGate lesson={lesson} />;
  }

  if (error) {
    return (
      <div className="w-full px-4 sm:px-6 lg:px-0 sm:max-w-2xl lg:max-w-4xl mx-auto">
        <div className="aspect-video bg-red-50 border-2 border-red-200 rounded-lg flex items-center justify-center p-8">
          <div className="text-center">
            <p className="text-red-700 text-lg font-medium mb-4">{error}</p>
            <button
              onClick={() => window.location.reload()}
              className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
            >
              Retry
            </button>
          </div>
        </div>
      </div>
    );
  }

  const opts: any = useMemo(() => ({
    height: '100%',
    width: '100%',
    playerVars: {
      autoplay: 0,
      controls: 0, // Hide default controls
      modestbranding: 1,
      rel: 0,
      fs: 0, // Disable fullscreen button in player (we have custom one)
      disablekb: 1, // Disable keyboard controls to prevent conflict
    },
  }), []);

  return (
    <div className="w-full px-4 sm:px-6 lg:px-0 sm:max-w-2xl lg:max-w-4xl mx-auto">
      <PointsNotification
        points={pointsEarned}
        show={showPointsNotification}
        onClose={() => setShowPointsNotification(false)}
      />

      <div 
        ref={containerRef}
        className="relative w-full aspect-video bg-black rounded-lg overflow-hidden shadow-lg"
      >
        {!loaded && (
          <div className="absolute inset-0 flex items-center justify-center bg-black/80 z-10">
            <Loader2 className="w-12 h-12 text-white animate-spin" />
          </div>
        )}

        {isSeeking && (
          <div className="absolute inset-0 flex items-center justify-center bg-black/50 z-15">
            <div className="bg-white/90 px-6 py-3 rounded-lg shadow-lg">
              <p className="text-gray-900 font-medium">Seeking...</p>
            </div>
          </div>
        )}

        {showResumePrompt && savedProgress && (
          <ResumePrompt
            resumePosition={savedProgress.lastWatchedPosition}
            videoDuration={videoDuration}
            onResume={handleResume}
            onStartFromBeginning={handleStartFromBeginning}
          />
        )}
        
        <YouTube
          videoId={sanitizedVideoId}
          opts={opts}
          onReady={onPlayerReady}
          onStateChange={onPlayerStateChange}
          onError={onPlayerError}
          className="w-full h-full"
          iframeClassName="w-full h-full"
        />

        {/* Custom Controls */}
        {loaded && (
          <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/80 to-transparent p-4 z-20 pointer-events-none">
            {/* Progress Bar */}
            <div 
              className="w-full h-1 bg-gray-600 rounded-full cursor-pointer mb-3 hover:h-2 transition-all pointer-events-auto"
              onClick={handleSeek}
            >
              <div 
                className="h-full bg-blue-500 rounded-full transition-all"
                style={{ width: `${videoDuration > 0 ? (currentPosition / videoDuration) * 100 : 0}%` }}
              />
            </div>

            {/* Control Buttons */}
            <div className="flex items-center justify-between text-white pointer-events-auto">
              <div className="flex items-center gap-3">
                <button onClick={handlePlayPause} className="hover:text-blue-400 transition-colors">
                  {playing ? <Pause className="w-6 h-6" /> : <Play className="w-6 h-6" />}
                </button>

                <button onClick={handleVolumeToggle} className="hover:text-blue-400 transition-colors">
                  {muted ? <VolumeX className="w-5 h-5" /> : <Volume2 className="w-5 h-5" />}
                </button>

                <span className="text-sm font-medium bg-black/50 px-2 py-1 rounded">
                  {formatSeconds(currentPosition)} / {formatSeconds(videoDuration)}
                </span>
              </div>

              <div className="flex items-center gap-3">
                <div className="relative group">
                  <button className="px-2 py-1 text-sm hover:text-blue-400 transition-colors">
                    {playbackRate}x
                  </button>
                  <div className="absolute bottom-full right-0 mb-2 hidden group-hover:block bg-gray-900 rounded-lg shadow-lg overflow-hidden">
                    {[0.5, 0.75, 1, 1.25, 1.5, 2].map((rate) => (
                      <button
                        key={rate}
                        onClick={() => handlePlaybackRateChange(rate)}
                        className={`block w-full px-4 py-2 text-left text-sm hover:bg-gray-700 transition ${
                          playbackRate === rate ? 'bg-gray-700 font-bold' : ''
                        }`}
                      >
                        {rate}x
                      </button>
                    ))}
                  </div>
                </div>

                <button onClick={handleFullscreen} className="hover:text-blue-400 transition-colors">
                  <Maximize className="w-5 h-5" />
                </button>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Progress Info Below Video */}
      <div className="mt-4 flex flex-wrap items-center gap-2 text-xs sm:text-sm text-gray-600">
        <div className="px-3 py-1 bg-gray-100 text-gray-800 rounded-full">
          Watching: {formatSeconds(currentPosition)} / {formatSeconds(videoDuration)}
        </div>

        {savedProgress && savedProgress.completionPercentage > 0 && (
          <div className="px-3 py-1 bg-blue-100 text-blue-800 rounded-full">
            {savedProgress.completionPercentage.toFixed(0)}% Complete
          </div>
        )}

        {lastSavedPosition !== null && (
          <div className="flex items-center gap-2 px-3 py-1 bg-green-100 text-green-800 rounded-full">
            <Check className="w-4 h-4" />
            Saved at: {formatSeconds(lastSavedPosition)}
          </div>
        )}

        {savedProgress?.isCompleted && (
          <div className="flex items-center gap-2 px-3 py-1 bg-purple-100 text-purple-800 rounded-full">
            <Check className="w-4 h-4" />
            Completed ✓
          </div>
        )}

        {lesson.rewardPoints > 0 && !savedProgress?.isCompleted && (
          <span className="px-3 py-1 bg-yellow-100 text-yellow-800 rounded-full">
            🏆 {lesson.rewardPoints} points
          </span>
        )}
      </div>
    </div>
  );
}

export const LessonPlayer = memo(LessonPlayerComponent);
