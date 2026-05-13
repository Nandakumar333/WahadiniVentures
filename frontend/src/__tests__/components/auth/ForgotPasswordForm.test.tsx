import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import { ForgotPasswordForm } from '../../../components/auth/ForgotPasswordForm';

// Mock the auth service
vi.mock('../../../services/authService', () => ({
  AuthService: {
    forgotPassword: vi.fn(),
  },
}));

describe('ForgotPasswordForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render form with email input field', () => {
    render(<ForgotPasswordForm />);
    
    expect(screen.getByLabelText(/email address/i)).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/enter your email address/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /send reset instructions/i })).toBeInTheDocument();
  });

  it('should show validation error for empty email on submit', async () => {
    const user = userEvent.setup();
    render(<ForgotPasswordForm />);

    const submitButton = screen.getByRole('button', { name: /send reset instructions/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/email is required/i)).toBeInTheDocument();
    });
  });

  it('should show validation error for invalid email format', async () => {
    const user = userEvent.setup();
    render(<ForgotPasswordForm />);

    const emailInput = screen.getByLabelText(/email address/i) as HTMLInputElement;
    
    // Clear the field first, then type invalid email
    await user.clear(emailInput);
    await user.type(emailInput, 'invalidemail');
    
    // Manually trigger form validation by setting value
    // This simulates react-hook-form validation
    const submitButton = screen.getByRole('button', { name: /send reset instructions/i });
    await user.click(submitButton);

    // Wait for Zod validation error to appear
    await waitFor(() => {
      expect(screen.getByText(/please enter a valid email address/i)).toBeInTheDocument();
    }, { timeout: 2000 });
  });

  it('should disable submit button during API call', async () => {
    const user = userEvent.setup();
    const mockForgotPassword = vi.fn().mockImplementation(() => {
      return new Promise(resolve => setTimeout(() => resolve({}), 100));
    });

    const { AuthService } = await import('../../../services/authService');
    AuthService.forgotPassword = mockForgotPassword;

    render(<ForgotPasswordForm />);

    const emailInput = screen.getByLabelText(/email address/i);
    await user.type(emailInput, 'test@example.com');

    const submitButton = screen.getByRole('button', { name: /send reset instructions/i });
    await user.click(submitButton);

    // Button should be disabled and show loading text
    await waitFor(() => {
      expect(submitButton).toBeDisabled();
      expect(screen.getByText(/sending/i)).toBeInTheDocument();
    });
  });

  it('should display success message after successful submission', async () => {
    const user = userEvent.setup();
    const mockForgotPassword = vi.fn().mockResolvedValue({});

    const { AuthService } = await import('../../../services/authService');
    AuthService.forgotPassword = mockForgotPassword;

    render(<ForgotPasswordForm />);

    const emailInput = screen.getByLabelText(/email address/i);
    await user.type(emailInput, 'test@example.com');

    const submitButton = screen.getByRole('button', { name: /send reset instructions/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/password reset instructions have been sent to your email address/i)).toBeInTheDocument();
      expect(mockForgotPassword).toHaveBeenCalledWith({ email: 'test@example.com' });
    });
  });

  it('should display error message on submission failure', async () => {
    const user = userEvent.setup();
    const mockForgotPassword = vi.fn().mockRejectedValue(new Error('Network error'));

    const { AuthService } = await import('../../../services/authService');
    AuthService.forgotPassword = mockForgotPassword;

    render(<ForgotPasswordForm />);

    const emailInput = screen.getByLabelText(/email address/i);
    await user.type(emailInput, 'test@example.com');

    const submitButton = screen.getByRole('button', { name: /send reset instructions/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/network error/i)).toBeInTheDocument();
    });
  });

  it('should call onSuccess callback with email after successful submission', async () => {
    const user = userEvent.setup();
    const mockForgotPassword = vi.fn().mockResolvedValue({});
    const mockOnSuccess = vi.fn();

    const { AuthService } = await import('../../../services/authService');
    AuthService.forgotPassword = mockForgotPassword;

    render(<ForgotPasswordForm onSuccess={mockOnSuccess} />);

    const emailInput = screen.getByLabelText(/email address/i);
    await user.type(emailInput, 'test@example.com');

    const submitButton = screen.getByRole('button', { name: /send reset instructions/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockOnSuccess).toHaveBeenCalledWith('test@example.com');
    });
  });

  it('should call onError callback on submission failure', async () => {
    const user = userEvent.setup();
    const errorMessage = 'Failed to send email';
    const mockForgotPassword = vi.fn().mockRejectedValue(new Error(errorMessage));
    const mockOnError = vi.fn();

    const { AuthService } = await import('../../../services/authService');
    AuthService.forgotPassword = mockForgotPassword;

    render(<ForgotPasswordForm onError={mockOnError} />);

    const emailInput = screen.getByLabelText(/email address/i);
    await user.type(emailInput, 'test@example.com');

    const submitButton = screen.getByRole('button', { name: /send reset instructions/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockOnError).toHaveBeenCalledWith(errorMessage);
    });
  });
});
