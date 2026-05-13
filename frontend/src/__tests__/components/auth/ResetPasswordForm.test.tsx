import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import { ResetPasswordForm } from '../../../components/auth/ResetPasswordForm';

// Mock the auth service
vi.mock('../../../services/authService', () => ({
  AuthService: {
    resetPassword: vi.fn(),
  },
}));

const defaultProps = {
  token: 'mock-reset-token',
  email: 'test@example.com',
};

describe('ResetPasswordForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render form with password fields', () => {
    render(<ResetPasswordForm {...defaultProps} />);
    
    expect(screen.getByPlaceholderText(/enter your new password/i)).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/confirm your new password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /reset password/i })).toBeInTheDocument();
  });

  it('should display password requirements checklist', () => {
    render(<ResetPasswordForm {...defaultProps} />);
    
    expect(screen.getByText(/password must contain:/i)).toBeInTheDocument();
    expect(screen.getByText(/at least 8 characters/i)).toBeInTheDocument();
    expect(screen.getByText(/one uppercase letter/i)).toBeInTheDocument();
    expect(screen.getByText(/one lowercase letter/i)).toBeInTheDocument();
    expect(screen.getByText(/one number/i)).toBeInTheDocument();
    expect(screen.getByText(/one special character/i)).toBeInTheDocument();
  });

  it('should show password strength meter when typing', async () => {
    const user = userEvent.setup();
    render(<ResetPasswordForm {...defaultProps} />);

    const passwordInput = screen.getByPlaceholderText(/enter your new password/i);

    // Type weak password
    await user.type(passwordInput, '123');
    
    await waitFor(() => {
      expect(screen.getByText(/password strength:/i)).toBeInTheDocument();
      expect(screen.getByText(/weak/i)).toBeInTheDocument();
    });

    // Clear and type strong password
    await user.clear(passwordInput);
    await user.type(passwordInput, 'Password123!');
    
    await waitFor(() => {
      expect(screen.getByText(/very strong/i)).toBeInTheDocument();
    });
  });

  it('should toggle password visibility', async () => {
    const user = userEvent.setup();
    render(<ResetPasswordForm {...defaultProps} />);

    const passwordInput = screen.getByPlaceholderText(/enter your new password/i) as HTMLInputElement;
    const toggleButtons = screen.getAllByRole('button', { hidden: true });
    const toggleButton = toggleButtons[0]; // First toggle button (for password field)

    // Initially password should be hidden
    expect(passwordInput.type).toBe('password');

    // Click toggle to show password
    await user.click(toggleButton);
    expect(passwordInput.type).toBe('text');

    // Click again to hide
    await user.click(toggleButton);
    expect(passwordInput.type).toBe('password');
  });

  it('should show validation error when passwords do not match', async () => {
    const user = userEvent.setup();
    render(<ResetPasswordForm {...defaultProps} />);

    const passwordInput = screen.getByPlaceholderText(/enter your new password/i);
    const confirmPasswordInput = screen.getByPlaceholderText(/confirm your new password/i);

    await user.type(passwordInput, 'Password123!');
    await user.type(confirmPasswordInput, 'DifferentPassword123!');

    const submitButton = screen.getByRole('button', { name: /reset password/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/passwords do not match/i)).toBeInTheDocument();
    });
  });

  it('should disable submit button during API call', async () => {
    const user = userEvent.setup();
    const mockResetPassword = vi.fn().mockImplementation(() => {
      return new Promise(resolve => setTimeout(() => resolve({}), 100));
    });

    const { AuthService } = await import('../../../services/authService');
    AuthService.resetPassword = mockResetPassword;

    render(<ResetPasswordForm {...defaultProps} />);

    const passwordInput = screen.getByPlaceholderText(/enter your new password/i);
    const confirmPasswordInput = screen.getByPlaceholderText(/confirm your new password/i);

    await user.type(passwordInput, 'Password123!');
    await user.type(confirmPasswordInput, 'Password123!');

    const submitButton = screen.getByRole('button', { name: /reset password/i });
    await user.click(submitButton);

    // Button should be disabled and show loading text
    await waitFor(() => {
      expect(submitButton).toBeDisabled();
      expect(screen.getByText(/resetting password/i)).toBeInTheDocument();
    });
  });

  it('should successfully submit valid form data', async () => {
    const user = userEvent.setup();
    const mockResetPassword = vi.fn().mockResolvedValue({});
    const mockOnSuccess = vi.fn();

    const { AuthService } = await import('../../../services/authService');
    AuthService.resetPassword = mockResetPassword;

    render(<ResetPasswordForm {...defaultProps} onSuccess={mockOnSuccess} />);

    const passwordInput = screen.getByPlaceholderText(/enter your new password/i);
    const confirmPasswordInput = screen.getByPlaceholderText(/confirm your new password/i);

    await user.type(passwordInput, 'Password123!');
    await user.type(confirmPasswordInput, 'Password123!');

    const submitButton = screen.getByRole('button', { name: /reset password/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockResetPassword).toHaveBeenCalledWith({
        email: 'test@example.com',
        token: 'mock-reset-token',
        newPassword: 'Password123!',
        confirmNewPassword: 'Password123!',
      });
      expect(mockOnSuccess).toHaveBeenCalled();
    });
  });

  it('should display error message on submission failure', async () => {
    const user = userEvent.setup();
    const errorMessage = 'Token expired';
    const mockResetPassword = vi.fn().mockRejectedValue(new Error(errorMessage));

    const { AuthService } = await import('../../../services/authService');
    AuthService.resetPassword = mockResetPassword;

    render(<ResetPasswordForm {...defaultProps} />);

    const passwordInput = screen.getByPlaceholderText(/enter your new password/i);
    const confirmPasswordInput = screen.getByPlaceholderText(/confirm your new password/i);

    await user.type(passwordInput, 'Password123!');
    await user.type(confirmPasswordInput, 'Password123!');

    const submitButton = screen.getByRole('button', { name: /reset password/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/token expired/i)).toBeInTheDocument();
    });
  });

  it('should call onError callback on submission failure', async () => {
    const user = userEvent.setup();
    const errorMessage = 'Invalid reset token';
    const mockResetPassword = vi.fn().mockRejectedValue(new Error(errorMessage));
    const mockOnError = vi.fn();

    const { AuthService } = await import('../../../services/authService');
    AuthService.resetPassword = mockResetPassword;

    render(<ResetPasswordForm {...defaultProps} onError={mockOnError} />);

    const passwordInput = screen.getByPlaceholderText(/enter your new password/i);
    const confirmPasswordInput = screen.getByPlaceholderText(/confirm your new password/i);

    await user.type(passwordInput, 'Password123!');
    await user.type(confirmPasswordInput, 'Password123!');

    const submitButton = screen.getByRole('button', { name: /reset password/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockOnError).toHaveBeenCalledWith(errorMessage);
    });
  });
});
