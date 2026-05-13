import { Crown } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import type { Lesson } from '../../../types/lesson';

export interface PremiumVideoGateProps {
  lesson: Lesson;
}

/**
 * PremiumVideoGate component that blocks access to premium content
 * for free users and displays an upgrade prompt
 */
export function PremiumVideoGate({ lesson }: PremiumVideoGateProps) {
  const navigate = useNavigate();

  const handleUpgrade = () => {
    navigate('/pricing');
  };

  return (
    <div className="relative w-full aspect-video bg-gradient-to-br from-yellow-400 via-yellow-500 to-yellow-600 rounded-lg shadow-2xl overflow-hidden flex items-center justify-center">
      {/* Blur Effect Background */}
      <div className="absolute inset-0 bg-black/20 backdrop-blur-sm"></div>

      {/* Content Container */}
      <div className="relative z-10 text-center px-6 py-8 max-w-2xl">
        {/* Crown Icon */}
        <div className="flex justify-center mb-6">
          <div className="relative">
            <div className="absolute inset-0 bg-yellow-300 rounded-full blur-xl opacity-50 animate-pulse"></div>
            <div className="relative bg-white rounded-full p-6 shadow-lg">
              <Crown className="w-16 h-16 text-yellow-500" strokeWidth={2} />
            </div>
          </div>
        </div>

        {/* Title */}
        <h2 className="text-3xl font-bold text-white mb-3 drop-shadow-lg">
          Premium Content
        </h2>

        {/* Lesson Title */}
        <p className="text-white/90 text-lg mb-4 font-medium">
          {lesson.title}
        </p>

        {/* Description */}
        <p className="text-white/80 text-base mb-8 leading-relaxed">
          This is premium content available only to our premium subscribers.
          Upgrade now to unlock this lesson and access our complete library of
          advanced courses and exclusive features.
        </p>

        {/* Upgrade Button */}
        <button
          onClick={handleUpgrade}
          className="inline-flex items-center gap-2 px-8 py-4 bg-white text-yellow-600 font-bold text-lg rounded-lg shadow-xl hover:bg-yellow-50 hover:scale-105 transform transition-all duration-200 focus:outline-none focus:ring-4 focus:ring-yellow-300"
        >
          <Crown className="w-6 h-6" />
          <span>Upgrade to Premium</span>
        </button>

        {/* Benefits List */}
        <div className="mt-8 text-white/80 text-sm">
          <p className="font-semibold mb-2">Premium Benefits:</p>
          <ul className="space-y-1 text-left inline-block">
            <li className="flex items-center gap-2">
              <span className="text-yellow-300">✓</span>
              <span>Access to all premium lessons</span>
            </li>
            <li className="flex items-center gap-2">
              <span className="text-yellow-300">✓</span>
              <span>Exclusive advanced courses</span>
            </li>
            <li className="flex items-center gap-2">
              <span className="text-yellow-300">✓</span>
              <span>Priority support</span>
            </li>
            <li className="flex items-center gap-2">
              <span className="text-yellow-300">✓</span>
              <span>Downloadable resources</span>
            </li>
          </ul>
        </div>
      </div>

      {/* Decorative Elements */}
      <div className="absolute top-4 left-4 text-white/20">
        <Crown className="w-24 h-24" />
      </div>
      <div className="absolute bottom-4 right-4 text-white/20">
        <Crown className="w-32 h-32" />
      </div>
    </div>
  );
}
