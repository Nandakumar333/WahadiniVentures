import { AlertCircle } from 'lucide-react';

interface VideoErrorFallbackProps {
  error?: string;
  errorType?: 'deleted' | 'private' | 'invalid' | 'network';
}

const ERROR_MESSAGES = {
  deleted: 'This video has been removed from YouTube.',
  private: 'This video is private and cannot be accessed.',
  invalid: 'The video link is invalid or broken.',
  network: 'Unable to load the video. Please check your internet connection.',
};

export function VideoErrorFallback({ error, errorType = 'network' }: VideoErrorFallbackProps) {
  const message = error || ERROR_MESSAGES[errorType];

  return (
    <div className="w-full max-w-4xl mx-auto">
      <div className="aspect-video bg-red-50 border-2 border-red-200 rounded-lg flex items-center justify-center p-8">
        <div className="text-center max-w-md">
          <AlertCircle className="w-16 h-16 text-red-500 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-red-700 mb-2">
            Video Unavailable
          </h3>
          <p className="text-red-600 mb-6">{message}</p>
          <div className="flex gap-3 justify-center">
            <button
              onClick={() => window.location.reload()}
              className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition"
            >
              Retry
            </button>
            <a
              href="mailto:support@wahadinicryptoquest.com?subject=Video%20Issue"
              className="px-4 py-2 bg-white border-2 border-red-600 text-red-600 rounded-lg hover:bg-red-50 transition"
            >
              Report Issue
            </a>
          </div>
        </div>
      </div>
    </div>
  );
}
