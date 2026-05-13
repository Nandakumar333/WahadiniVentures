import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { LoginForm } from '@/components/auth/LoginForm';
import { useAuthStore } from '@/store/authStore';

// Mock the auth store
vi.mock('@/store/authStore');

describe('LoginForm', () => {
  const mockLogin = vi.fn();
  const mockSetError = vi.fn();
  const mockOnSuccess = vi.fn();
  const mockOnRegisterClick = vi.fn();
  const mockOnForgotPasswordClick = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    
    // Setup default mock implementation
    (useAuthStore as unknown as ReturnType<typeof vi.fn>).mockReturnValue({
      login: mockLogin,
      isLoading: false,
      error: null,
      setError: mockSetError,
    });
  });

  describe('Form Rendering', () => {
    it('should render with email and password fields', () => {
      render(<LoginForm />);
      
      expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
      expect(screen.getByPlaceholderText(/enter your password/i)).toBeInTheDocument();
    });

    it('should render Remember Me checkbox', () => {
      render(<LoginForm />);
      
      expect(screen.getByRole('checkbox', { name: /remember me/i })).toBeInTheDocument();
    });

    it('should render submit button', () => {
      render(<LoginForm />);
      
      expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
    });

    it('should render forgot password link when callback provided', () => {
      render(<LoginForm onForgotPasswordClick={mockOnForgotPasswordClick} />);
      
      expect(screen.getByText(/forgot your password/i)).toBeInTheDocument();
    });

    it('should render register link when callback provided', () => {
      render(<LoginForm onRegisterClick={mockOnRegisterClick} />);
      
      expect(screen.getByText(/don't have an account/i)).toBeInTheDocument();
    });
  });

  describe('Field Validation', () => {
    it('should show error for empty email field on submit', async () => {
      const user = userEvent.setup();
      render(<LoginForm />);
      
      // Fill in password but leave email empty
      const passwordInput = screen.getByPlaceholderText(/enter your password/i);
      await user.type(passwordInput, 'Password123!');
      
      const submitButton = screen.getByRole('button', { name: /sign in/i });
      await user.click(submitButton);
      
      await waitFor(() => {
        expect(screen.getByText(/email is required/i)).toBeInTheDocument();
      });
    });

    it('should show error for empty password field on submit', async () => {
      const user = userEvent.setup();
      render(<LoginForm />);
      
      const emailInput = screen.getByLabelText(/email address/i);
      await user.type(emailInput, 'test@example.com');
      
      const submitButton = screen.getByRole('button', { name: /sign in/i });
      await user.click(submitButton);
      
      await waitFor(() => {
        expect(screen.getByText(/password is required/i)).toBeInTheDocument();
      });
    });
  });

  describe('Password Visibility Toggle', () => {
    it('should have password field with show/hide toggle', () => {
      render(<LoginForm />);
      
      const passwordInput = screen.getByPlaceholderText(/enter your password/i);
      expect(passwordInput).toHaveAttribute('type', 'password');
      
      // Check for toggle button
      const toggleButton = screen.getByRole('button', { name: /show password/i });
      expect(toggleButton).toBeInTheDocument();
    });

    it('should toggle password visibility when clicking the eye icon', async () => {
      const user = userEvent.setup();
      render(<LoginForm />);
      
      const passwordInput = screen.getByPlaceholderText(/enter your password/i);
      const toggleButton = screen.getByRole('button', { name: /show password/i });
      
      // Initially password is hidden
      expect(passwordInput).toHaveAttribute('type', 'password');
      
      // Click to show password
      await user.click(toggleButton);
      await waitFor(() => {
        expect(passwordInput).toHaveAttribute('type', 'text');
      });
      
      // Check button text changed
      const hideButton = screen.getByRole('button', { name: /hide password/i });
      expect(hideButton).toBeInTheDocument();
      
      // Click again to hide password
      await user.click(hideButton);
      await waitFor(() => {
        expect(passwordInput).toHaveAttribute('type', 'password');
      });
    });
  });

  describe('Form Submission', () => {
    it('should call login function with form data when submitted', async () => {
      const user = userEvent.setup();
      mockLogin.mockResolvedValueOnce(true);
      
      render(<LoginForm onSuccess={mockOnSuccess} />);
      
      const emailInput = screen.getByLabelText(/email address/i);
      const passwordInput = screen.getByPlaceholderText(/enter your password/i);
      
      await user.type(emailInput, 'test@example.com');
      await user.type(passwordInput, 'Password123!');
      
      const submitButton = screen.getByRole('button', { name: /sign in/i });
      await user.click(submitButton);
      
      await waitFor(() => {
        expect(mockLogin).toHaveBeenCalledWith({
          email: 'test@example.com',
          password: 'Password123!',
          rememberMe: false,
        });
      });
    });

    it('should call onSuccess callback after successful login', async () => {
      const user = userEvent.setup();
      mockLogin.mockResolvedValueOnce(true);
      
      render(<LoginForm onSuccess={mockOnSuccess} />);
      
      const emailInput = screen.getByLabelText(/email address/i);
      const passwordInput = screen.getByPlaceholderText(/enter your password/i);
      
      await user.type(emailInput, 'test@example.com');
      await user.type(passwordInput, 'Password123!');
      
      const submitButton = screen.getByRole('button', { name: /sign in/i });
      await user.click(submitButton);
      
      await waitFor(() => {
        expect(mockOnSuccess).toHaveBeenCalled();
      });
    });

    it('should submit form when pressing Enter key in password field', async () => {
      const user = userEvent.setup();
      mockLogin.mockResolvedValueOnce(true);
      
      render(<LoginForm />);
      
      const emailInput = screen.getByLabelText(/email address/i);
      const passwordInput = screen.getByPlaceholderText(/enter your password/i);
      
      await user.type(emailInput, 'test@example.com');
      await user.type(passwordInput, 'Password123!{Enter}');
      
      await waitFor(() => {
        expect(mockLogin).toHaveBeenCalled();
      });
    });
  });

  describe('Loading States', () => {
    it('should show loading spinner during submission', async () => {
      (useAuthStore as unknown as ReturnType<typeof vi.fn>).mockReturnValue({
        login: mockLogin,
        isLoading: true,
        error: null,
        setError: mockSetError,
      });
      
      render(<LoginForm />);
      
      // Should show loading spinner
      const submitButton = screen.getByRole('button', { name: /sign in/i });
      expect(submitButton).toHaveAttribute('disabled');
      expect(submitButton).toHaveAttribute('aria-busy', 'true');
    });

    it('should disable form fields during loading', async () => {
      (useAuthStore as unknown as ReturnType<typeof vi.fn>).mockReturnValue({
        login: mockLogin,
        isLoading: true,
        error: null,
        setError: mockSetError,
      });
      
      render(<LoginForm />);
      
      const emailInput = screen.getByLabelText(/email address/i);
      const passwordInput = screen.getByPlaceholderText(/enter your password/i);
      const submitButton = screen.getByRole('button', { name: /sign in/i });
      
      expect(emailInput).toBeDisabled();
      expect(passwordInput).toBeDisabled();
      expect(submitButton).toBeDisabled();
    });
  });

  describe('Error Handling', () => {
    it('should show error message from auth store', () => {
      (useAuthStore as unknown as ReturnType<typeof vi.fn>).mockReturnValue({
        login: mockLogin,
        isLoading: false,
        error: 'Invalid credentials',
        setError: mockSetError,
      });
      
      render(<LoginForm />);
      
      expect(screen.getByText(/invalid credentials/i)).toBeInTheDocument();
    });

    it('should clear error when setError is called', async () => {
      const user = userEvent.setup();
      (useAuthStore as unknown as ReturnType<typeof vi.fn>).mockReturnValue({
        login: mockLogin,
        isLoading: false,
        error: 'Previous error',
        setError: mockSetError,
      });
      
      render(<LoginForm />);
      
      const emailInput = screen.getByLabelText(/email address/i);
      const passwordInput = screen.getByPlaceholderText(/enter your password/i);
      
      await user.type(emailInput, 'test@example.com');
      await user.type(passwordInput, 'Password123!');
      
      const submitButton = screen.getByRole('button', { name: /sign in/i });
      await user.click(submitButton);
      
      await waitFor(() => {
        expect(mockSetError).toHaveBeenCalledWith(null);
      });
    });
  });

  describe('Accessibility', () => {
    it('should have proper ARIA attributes on form fields', () => {
      render(<LoginForm />);
      
      const emailInput = screen.getByLabelText(/email address/i);
      const passwordInput = screen.getByPlaceholderText(/enter your password/i);
      
      expect(emailInput).toHaveAttribute('aria-invalid', 'false');
      expect(passwordInput).toHaveAttribute('aria-invalid', 'false');
    });

    it('should set aria-invalid to true when field has error', async () => {
      const user = userEvent.setup();
      render(<LoginForm />);
      
      const submitButton = screen.getByRole('button', { name: /sign in/i });
      await user.click(submitButton);
      
      await waitFor(() => {
        const emailInput = screen.getByLabelText(/email address/i);
        expect(emailInput).toHaveAttribute('aria-invalid', 'true');
        expect(emailInput).toHaveAttribute('aria-describedby', 'email-error');
      });
    });

    it('should support keyboard navigation through form', async () => {
      const user = userEvent.setup();
      render(<LoginForm onForgotPasswordClick={mockOnForgotPasswordClick} onRegisterClick={mockOnRegisterClick} />);
      
      // Start at first element
      await user.tab();
      expect(screen.getByLabelText(/email address/i)).toHaveFocus();
      
      await user.tab();
      expect(screen.getByPlaceholderText(/enter your password/i)).toHaveFocus();
    });

    it('should announce errors to screen readers', async () => {
      const user = userEvent.setup();
      render(<LoginForm />);
      
      const submitButton = screen.getByRole('button', { name: /sign in/i });
      await user.click(submitButton);
      
      await waitFor(() => {
        const errorMessage = screen.getByText(/email is required/i);
        expect(errorMessage).toHaveAttribute('role', 'alert');
        expect(errorMessage).toHaveAttribute('aria-live', 'polite');
      });
    });

    it('should have aria-busy attribute during loading', () => {
      (useAuthStore as unknown as ReturnType<typeof vi.fn>).mockReturnValue({
        login: mockLogin,
        isLoading: true,
        error: null,
        setError: mockSetError,
      });
      
      render(<LoginForm />);
      
      const submitButton = screen.getByRole('button', { name: /sign in/i });
      expect(submitButton).toHaveAttribute('aria-busy', 'true');
    });
  });
});
