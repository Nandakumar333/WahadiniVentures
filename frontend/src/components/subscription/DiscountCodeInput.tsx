import { useState } from 'react';
import { Loader2, CheckCircle2, XCircle } from 'lucide-react';
import { apiClient } from '../../services/api/client';

interface DiscountCodeInputProps {
  tier: string;
  onDiscountValidated: (code: string, discountAmount: number) => void;
  onDiscountCleared: () => void;
}

export function DiscountCodeInput({ tier, onDiscountValidated, onDiscountCleared }: DiscountCodeInputProps) {
  const [code, setCode] = useState('');
  const [isValidating, setIsValidating] = useState(false);
  const [validationError, setValidationError] = useState<string | null>(null);
  const [validatedDiscount, setValidatedDiscount] = useState<number | null>(null);

  const validateDiscount = async (discountCode: string) => {
    if (!discountCode.trim()) {
      setValidationError(null);
      setValidatedDiscount(null);
      onDiscountCleared();
      return;
    }

    setIsValidating(true);
    setValidationError(null);
    setValidatedDiscount(null);

    try {
      // Using apiClient handles the Base URL and Auth token automatically
      // It also wraps the response in a standard format { success: true, data: ... }
      const response = await apiClient.post<any>('/subscriptions/validate-discount', {
        discountCode: discountCode.trim(),
        tier,
      });

      // Handle potential response wrapping by apiClient
      const data = response.data || response;

      if (data.isValid) {
        setValidatedDiscount(data.discountAmount);
        onDiscountValidated(discountCode.trim(), data.discountAmount);
      } else {
        setValidationError(data.errorMessage || 'Invalid discount code');
        onDiscountCleared();
      }
    } catch (error: any) {
      console.error('Error validating discount:', error);
      // apiClient throws error object with message
      const msg = error.message || 'Failed to validate discount code. Please try again.';
      setValidationError(msg);
      onDiscountCleared();
    } finally {
      setIsValidating(false);
    }
  };

  const handleCodeChange = (value: string) => {
    setCode(value);
    
    // Clear validation when user modifies the code
    if (validatedDiscount !== null || validationError !== null) {
      setValidatedDiscount(null);
      setValidationError(null);
      onDiscountCleared();
    }
  };

  const handleBlur = () => {
    if (code.trim()) {
      validateDiscount(code);
    }
  };

  const handleClear = () => {
    setCode('');
    setValidationError(null);
    setValidatedDiscount(null);
    onDiscountCleared();
  };

  return (
    <div className="space-y-2">
      <label htmlFor="discount-code" className="block text-sm font-medium text-gray-700">
        Discount Code (Optional)
      </label>
      
      <div className="relative">
        <input
          id="discount-code"
          type="text"
          value={code}
          onChange={(e) => handleCodeChange(e.target.value)}
          onBlur={handleBlur}
          placeholder="Enter discount code"
          className={`block w-full px-3 py-2 pr-10 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-2 ${
            validatedDiscount !== null
              ? 'border-green-500 focus:ring-green-500'
              : validationError
              ? 'border-red-500 focus:ring-red-500'
              : 'border-gray-300 focus:ring-indigo-500'
          }`}
          disabled={isValidating}
        />
        
        <div className="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
          {isValidating && <Loader2 className="h-5 w-5 text-gray-400 animate-spin" />}
          {!isValidating && validatedDiscount !== null && (
            <CheckCircle2 className="h-5 w-5 text-green-500" />
          )}
          {!isValidating && validationError && (
            <XCircle className="h-5 w-5 text-red-500" />
          )}
        </div>
      </div>

      {validatedDiscount !== null && (
        <div className="flex items-center justify-between p-2 bg-green-50 border border-green-200 rounded-md">
          <div className="flex items-center space-x-2">
            <CheckCircle2 className="h-4 w-4 text-green-600" />
            <span className="text-sm text-green-800">
              {validatedDiscount}% discount applied
            </span>
          </div>
          <button
            type="button"
            onClick={handleClear}
            className="text-sm text-green-700 hover:text-green-900 font-medium"
          >
            Remove
          </button>
        </div>
      )}

      {validationError && (
        <div className="flex items-center space-x-2 p-2 bg-red-50 border border-red-200 rounded-md">
          <XCircle className="h-4 w-4 text-red-600" />
          <span className="text-sm text-red-800">{validationError}</span>
        </div>
      )}

      {validatedDiscount !== null && (
        <p className="text-xs text-gray-500">
          Your discount will be applied at checkout
        </p>
      )}
    </div>
  );
}
