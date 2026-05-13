import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import { BrowserRouter } from 'react-router-dom';
import { RegisterForm } from '../../../components/auth/RegisterForm';

// Mock the auth service
vi.mock('../../../services/authService', () => ({
  AuthService: {
    register: vi.fn(),
  },
}));

// Mock react-router-dom navigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

const renderWithRouter = (component: React.ReactElement) => {
  return render(
    <BrowserRouter>
      {component}
    </BrowserRouter>
  );
};

describe('RegisterForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render all form fields', () => {
    renderWithRouter(<RegisterForm />);
    
    expect(screen.getByLabelText(/first name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/last name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/email address/i)).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/create a strong password/i)).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/confirm your password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /create account/i })).toBeInTheDocument();
  });

  it('should show validation errors for empty fields on submit', async () => {
    const user = userEvent.setup();
    renderWithRouter(<RegisterForm />);

    const submitButton = screen.getByRole('button', { name: /create account/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/first name is required/i)).toBeInTheDocument();
      expect(screen.getByText(/last name is required/i)).toBeInTheDocument();
      expect(screen.getByText(/email is required/i)).toBeInTheDocument();
      expect(screen.getByText(/password must be at least/i)).toBeInTheDocument();
    });
  });

  it('should show password validation error for weak password', async () => {
    const user = userEvent.setup();
    renderWithRouter(<RegisterForm />);

    const passwordInput = screen.getByPlaceholderText(/create a strong password/i);
    await user.type(passwordInput, '123');

    const submitButton = screen.getByRole('button', { name: /create account/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/password must be at least 8 characters/i)).toBeInTheDocument();
    });
  });

  it('should show error when passwords do not match', async () => {
    const user = userEvent.setup();
    renderWithRouter(<RegisterForm />);

    const passwordInput = screen.getByPlaceholderText(/create a strong password/i);
    const confirmPasswordInput = screen.getByPlaceholderText(/confirm your password/i);
    
    await user.type(passwordInput, 'Password123!');
    await user.type(confirmPasswordInput, 'DifferentPassword123!');

    const submitButton = screen.getByRole('button', { name: /create account/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/passwords do not match/i)).toBeInTheDocument();
    });
  });

  it('should successfully submit valid form data', async () => {
    const user = userEvent.setup();
    const mockRegister = vi.fn().mockResolvedValue({
      user: {
        id: '123',
        email: 'test@example.com',
        firstName: 'John',
        lastName: 'Doe',
        emailConfirmed: false,
      },
      accessToken: 'mock-token',
      refreshToken: 'mock-refresh-token',
    });

    // Import and mock the auth service after the mock is set up
    const { AuthService } = await import('../../../services/authService');
    AuthService.register = mockRegister;

    renderWithRouter(<RegisterForm />);

    // Fill out the form with valid data
    await user.type(screen.getByLabelText(/first name/i), 'John');
    await user.type(screen.getByLabelText(/last name/i), 'Doe');
    await user.type(screen.getByLabelText(/email address/i), 'test@example.com');
    await user.type(screen.getByPlaceholderText(/create a strong password/i), 'Password123!');
    await user.type(screen.getByPlaceholderText(/confirm your password/i), 'Password123!');
    
    // Check the required terms checkbox
    const termsCheckbox = screen.getByLabelText(/i accept the terms of service/i);
    await user.click(termsCheckbox);

    const submitButton = screen.getByRole('button', { name: /create account/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockRegister).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'Password123!',
        confirmPassword: 'Password123!',
        firstName: 'John',
        lastName: 'Doe',
        acceptTerms: true,
        acceptMarketing: false,
      });
    });
  });

  it('should display password strength indicator', async () => {
    const user = userEvent.setup();
    renderWithRouter(<RegisterForm />);

    const passwordInput = screen.getByPlaceholderText(/create a strong password/i);

    // Type a weak password
    await user.type(passwordInput, '123');
    expect(screen.getByText(/weak/i)).toBeInTheDocument();

    // Clear and type a strong password
    await user.clear(passwordInput);
    await user.type(passwordInput, 'Password123!');
    expect(screen.getByText(/strong/i)).toBeInTheDocument();
  });
});