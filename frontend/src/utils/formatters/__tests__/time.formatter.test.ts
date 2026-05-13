import { describe, it, expect } from 'vitest';
import { formatSeconds, formatSecondsVerbose, calculateWatchPercentage } from '../time.formatter';

describe('Time Formatter Utilities', () => {
  describe('formatSeconds', () => {
    it('should format seconds under a minute correctly', () => {
      expect(formatSeconds(0)).toBe('00:00');
      expect(formatSeconds(5)).toBe('00:05');
      expect(formatSeconds(30)).toBe('00:30');
      expect(formatSeconds(59)).toBe('00:59');
    });

    it('should format minutes and seconds correctly', () => {
      expect(formatSeconds(60)).toBe('01:00');
      expect(formatSeconds(65)).toBe('01:05');
      expect(formatSeconds(125)).toBe('02:05');
      expect(formatSeconds(599)).toBe('09:59');
    });

    it('should format hours, minutes, and seconds correctly', () => {
      expect(formatSeconds(3600)).toBe('1:00:00');
      expect(formatSeconds(3665)).toBe('1:01:05');
      expect(formatSeconds(7265)).toBe('2:01:05');
      expect(formatSeconds(36000)).toBe('10:00:00');
    });

    it('should handle decimal seconds by rounding down', () => {
      expect(formatSeconds(65.7)).toBe('01:05');
      expect(formatSeconds(125.99)).toBe('02:05');
    });

    it('should handle negative numbers by returning 00:00', () => {
      expect(formatSeconds(-1)).toBe('00:00');
      expect(formatSeconds(-100)).toBe('00:00');
    });

    it('should handle NaN by returning 00:00', () => {
      expect(formatSeconds(NaN)).toBe('00:00');
      expect(formatSeconds(Number.NaN)).toBe('00:00');
    });

    it('should handle Infinity by returning 00:00', () => {
      expect(formatSeconds(Infinity)).toBe('00:00');
      expect(formatSeconds(-Infinity)).toBe('00:00');
    });

    it('should handle invalid input types by returning 00:00', () => {
      expect(formatSeconds(undefined as any)).toBe('00:00');
      expect(formatSeconds(null as any)).toBe('00:00');
      expect(formatSeconds('123' as any)).toBe('00:00');
    });
  });

  describe('formatSecondsVerbose', () => {
    it('should format seconds only', () => {
      expect(formatSecondsVerbose(0)).toBe('0 seconds');
      expect(formatSecondsVerbose(1)).toBe('1 second');
      expect(formatSecondsVerbose(30)).toBe('30 seconds');
    });

    it('should format minutes and seconds', () => {
      expect(formatSecondsVerbose(60)).toBe('1 minute');
      expect(formatSecondsVerbose(61)).toBe('1 minute 1 second');
      expect(formatSecondsVerbose(90)).toBe('1 minute 30 seconds');
      expect(formatSecondsVerbose(150)).toBe('2 minutes 30 seconds');
    });

    it('should format hours, minutes, and seconds', () => {
      expect(formatSecondsVerbose(3600)).toBe('1 hour');
      expect(formatSecondsVerbose(3661)).toBe('1 hour 1 minute 1 second');
      expect(formatSecondsVerbose(7265)).toBe('2 hours 1 minute 5 seconds');
    });

    it('should handle invalid input', () => {
      expect(formatSecondsVerbose(-1)).toBe('0 seconds');
      expect(formatSecondsVerbose(NaN)).toBe('0 seconds');
      expect(formatSecondsVerbose(Infinity)).toBe('0 seconds');
    });
  });

  describe('calculateWatchPercentage', () => {
    it('should calculate percentage correctly', () => {
      expect(calculateWatchPercentage(0, 100)).toBe(0);
      expect(calculateWatchPercentage(50, 100)).toBe(50);
      expect(calculateWatchPercentage(100, 100)).toBe(100);
      expect(calculateWatchPercentage(25, 100)).toBe(25);
    });

    it('should handle decimal values', () => {
      expect(calculateWatchPercentage(33.33, 100)).toBeCloseTo(33.33, 2);
      expect(calculateWatchPercentage(66.66, 100)).toBeCloseTo(66.66, 2);
    });

    it('should clamp values between 0 and 100', () => {
      expect(calculateWatchPercentage(150, 100)).toBe(100);
      expect(calculateWatchPercentage(-10, 100)).toBe(0);
    });

    it('should handle edge cases', () => {
      expect(calculateWatchPercentage(50, 0)).toBe(0);
      expect(calculateWatchPercentage(50, -100)).toBe(0);
      expect(calculateWatchPercentage(NaN, 100)).toBe(0);
      expect(calculateWatchPercentage(50, NaN)).toBe(0);
    });

    it('should handle invalid types', () => {
      expect(calculateWatchPercentage('50' as any, 100)).toBe(0);
      expect(calculateWatchPercentage(50, '100' as any)).toBe(0);
    });
  });
});
