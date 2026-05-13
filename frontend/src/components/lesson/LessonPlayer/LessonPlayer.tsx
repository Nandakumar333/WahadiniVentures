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
            
            // Save progress every 5 seconds to respect rate limits
            if (Math.floor(currentTime) % 5 === 0) {
               saveProgress(currentTime);
            }
            
            // Log occasionally (every 10 seconds)
            if (Math.floor(currentTime) % 10 === 0) {
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


    if (playerState === 0) { // Ended
      // Immediately reset to start to block YouTube suggestions
      event.target.seekTo(0);
      event.target.pauseVideo();
      setCurrentPosition(0);
    }

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

  // Refs for cleanup and state tracking
  const currentPositionRef = useRef(currentPosition);
  const saveProgressRef = useRef(saveProgress);
  const hasPromptBeenShown = useRef(false);

  // Show resume prompt - only once per load
  useEffect(() => {
    if (!hasPromptBeenShown.current && loaded && savedProgress?.lastWatchedPosition && savedProgress.lastWatchedPosition > 5) {
      if (savedProgress.lastWatchedPosition <= videoDuration) {
        setShowResumePrompt(true);
        hasPromptBeenShown.current = true;
      }
    }
  }, [loaded, savedProgress, videoDuration]);

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
        <div className="aspect-video bg-red-50 dark:bg-red-900/20 border-2 border-red-200 dark:border-red-800 rounded-lg flex items-center justify-center p-8">
          <div className="text-center">
            <p className="text-red-700 dark:text-red-300 text-lg font-medium mb-4">{error}</p>
            <button
              onClick={() => window.location.reload()}
              className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
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
      iv_load_policy: 3, // Hide video annotations
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
            <div className="bg-black/90 px-6 py-3 rounded-lg shadow-lg border border-white/10 backdrop-blur-sm">
              <p className="text-white font-medium">Seeking...</p>
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

        {/* Transparent overlay to block YouTube interactions (suggestions on pause) */}
        <div 
          className="absolute inset-0 z-[1] cursor-pointer" 
          onClick={handlePlayPause}
        />

        {/* Custom Controls */}
        {loaded && (
          <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/90 via-black/60 to-transparent pt-12 pb-4 px-4 z-20 opacity-0 group-hover:opacity-100 transition-opacity duration-300">
            {/* Progress Bar */}
            <div 
              className="group/progress w-full h-1.5 bg-white/20 rounded-full cursor-pointer mb-4 hover:h-2.5 transition-all"
              onClick={handleSeek}
            >
              <div 
                className="h-full bg-gradient-to-r from-blue-500 to-purple-500 rounded-full relative"
                style={{ width: `${videoDuration > 0 ? (currentPosition / videoDuration) * 100 : 0}%` }}
              >
                <div className="absolute right-0 top-1/2 -translate-y-1/2 w-3 h-3 bg-white rounded-full shadow-lg scale-0 group-hover/progress:scale-100 transition-transform" />
              </div>
            </div>

            {/* Control Buttons */}
            <div className="flex items-center justify-between text-white">
              <div className="flex items-center gap-4">
                <button 
                  onClick={handlePlayPause} 
                  className="w-10 h-10 flex items-center justify-center rounded-full bg-white/10 hover:bg-white/20 backdrop-blur-sm transition-all hover:scale-105"
                >
                  {playing ? <Pause className="w-5 h-5 fill-current" /> : <Play className="w-5 h-5 fill-current ml-0.5" />}
                </button>

                <div className="flex items-center gap-2 group/volume">
                  <button onClick={handleVolumeToggle} className="hover:text-blue-400 transition-colors">
                    {muted ? <VolumeX className="w-5 h-5" /> : <Volume2 className="w-5 h-5" />}
                  </button>
                  <div className="w-0 overflow-hidden group-hover/volume:w-20 transition-all duration-300">
                    <div className="w-20 h-1 bg-white/30 rounded-full ml-2">
                      <div className="h-full bg-white rounded-full" style={{ width: `${muted ? 0 : volume * 100}%` }} />
                    </div>
                  </div>
                </div>

                <span className="text-sm font-medium font-mono text-gray-300">
                  {formatSeconds(currentPosition)} / {formatSeconds(videoDuration)}
                </span>
              </div>

              <div className="flex items-center gap-3">
                <div className="relative group">
                  <button className="px-3 py-1.5 text-sm font-medium bg-white/10 hover:bg-white/20 backdrop-blur-sm rounded-lg transition-all">
                    {playbackRate}x
                  </button>
                  <div className="absolute bottom-full right-0 mb-2 hidden group-hover:block bg-black/90 backdrop-blur-xl border border-white/10 rounded-xl shadow-2xl overflow-hidden p-1 min-w-[100px]">
                    {[0.5, 0.75, 1, 1.25, 1.5, 2].map((rate) => (
                      <button
                        key={rate}
                        onClick={() => handlePlaybackRateChange(rate)}
                        className={`block w-full px-4 py-2 text-left text-sm rounded-lg transition-colors ${
                          playbackRate === rate 
                            ? 'bg-blue-600 text-white' 
                            : 'text-gray-300 hover:bg-white/10'
                        }`}
                      >
                        {rate}x
                      </button>
                    ))}
                  </div>
                </div>

                <button 
                  onClick={handleFullscreen} 
                  className="p-2 hover:bg-white/10 rounded-lg transition-colors"
                >
                  <Maximize className="w-5 h-5" />
                </button>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Progress Info Below Video */}
      <div className="mt-4 flex flex-wrap items-center gap-3">
        <div className="flex items-center gap-2 px-3 py-1.5 bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-300 rounded-full text-sm font-medium border border-gray-200 dark:border-gray-700">
          <div className="w-2 h-2 rounded-full bg-blue-500 animate-pulse" />
          Watching: {formatSeconds(currentPosition)} / {formatSeconds(videoDuration)}
        </div>

        {savedProgress && savedProgress.completionPercentage > 0 && (
          <div className="flex items-center gap-2 px-3 py-1.5 bg-blue-50 dark:bg-blue-900/20 text-blue-700 dark:text-blue-300 rounded-full text-sm font-medium border border-blue-100 dark:border-blue-800">
            <div className="w-4 h-4 rounded-full border-2 border-current border-t-transparent animate-spin" style={{ animationDuration: '3s' }} />
            {savedProgress.completionPercentage.toFixed(0)}% Complete
          </div>
        )}

        {lastSavedPosition !== null && (
          <div className="flex items-center gap-2 px-3 py-1.5 bg-green-50 dark:bg-green-900/20 text-green-700 dark:text-green-300 rounded-full text-sm font-medium border border-green-100 dark:border-green-800">
            <Check className="w-3.5 h-3.5" />
            Saved at: {formatSeconds(lastSavedPosition)}
          </div>
        )}

        {savedProgress?.isCompleted && (
          <div className="flex items-center gap-2 px-3 py-1.5 bg-purple-50 dark:bg-purple-900/20 text-purple-700 dark:text-purple-300 rounded-full text-sm font-medium border border-purple-100 dark:border-purple-800 shadow-sm">
            <Check className="w-3.5 h-3.5" />
            Lesson Completed
          </div>
        )}
      </div>
    </div>
  );
}
export const LessonPlayer = memo(LessonPlayerComponent);
