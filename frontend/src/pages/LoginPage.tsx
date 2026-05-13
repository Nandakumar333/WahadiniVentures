import { useNavigate, Link } from 'react-router-dom';
import { LoginForm } from '@/components/auth/LoginForm';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Sparkles, Shield, TrendingUp } from 'lucide-react';

export default function LoginPage() {
  const navigate = useNavigate();

  const handleLoginSuccess = () => {
    // Redirect to dashboard or home page after successful login
    navigate('/dashboard');
  };

  const handleRegisterClick = () => {
    navigate('/register');
  };

  const handleForgotPasswordClick = () => {
    navigate('/forgot-password');
  };

  return (
    <div className="min-h-screen flex">
      {/* Left Panel - Branding & Info */}
      <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-indigo-600 via-purple-600 to-pink-600 p-12 relative overflow-hidden">
        {/* Animated background elements */}
        <div className="absolute inset-0 opacity-10">
          <div className="absolute top-20 left-20 w-72 h-72 bg-white rounded-full blur-3xl animate-pulse"></div>
          <div className="absolute bottom-20 right-20 w-96 h-96 bg-white rounded-full blur-3xl animate-pulse" style={{ animationDelay: '1s' }}></div>
        </div>
        
        <div className="relative z-10 flex flex-col justify-between w-full text-white">
          <div>
            <Link to="/" className="flex items-center space-x-2 mb-12">
              <div className="w-10 h-10 bg-white/20 backdrop-blur-sm rounded-xl flex items-center justify-center">
                <Sparkles className="w-6 h-6" />
              </div>
              <span className="text-2xl font-bold">WahadiniCryptoQuest</span>
            </Link>
            
            <h1 className="text-5xl font-bold mb-6 leading-tight">
              Welcome back to your
              <span className="block bg-gradient-to-r from-yellow-200 to-pink-200 bg-clip-text text-transparent">
                Crypto Journey
              </span>
            </h1>
            
            <p className="text-xl text-white/90 mb-12 leading-relaxed">
              Continue learning, earning, and mastering the world of cryptocurrency through interactive quests and challenges.
            </p>
          </div>
          
          <div className="space-y-6">
            <div className="flex items-start space-x-4">
              <div className="w-12 h-12 bg-white/10 backdrop-blur-sm rounded-xl flex items-center justify-center flex-shrink-0">
                <Shield className="w-6 h-6" />
              </div>
              <div>
                <h3 className="font-semibold text-lg mb-1">Secure & Protected</h3>
                <p className="text-white/80 text-sm">Your data is encrypted and secured with industry-standard protocols</p>
              </div>
            </div>
            
            <div className="flex items-start space-x-4">
              <div className="w-12 h-12 bg-white/10 backdrop-blur-sm rounded-xl flex items-center justify-center flex-shrink-0">
                <TrendingUp className="w-6 h-6" />
              </div>
              <div>
                <h3 className="font-semibold text-lg mb-1">Track Your Progress</h3>
                <p className="text-white/80 text-sm">Monitor your learning journey and watch your skills grow</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Right Panel - Login Form */}
      <div className="flex-1 flex items-center justify-center p-8 bg-gradient-to-br from-gray-50 to-gray-100">
        <div className="w-full max-w-md">
          {/* Mobile Logo */}
          <div className="lg:hidden text-center mb-8">
            <Link to="/" className="inline-flex items-center space-x-2 mb-4">
              <div className="w-10 h-10 bg-gradient-to-br from-indigo-600 to-purple-600 rounded-xl flex items-center justify-center">
                <Sparkles className="w-6 h-6 text-white" />
              </div>
              <span className="text-2xl font-bold bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent">
                WahadiniCryptoQuest
              </span>
            </Link>
          </div>

          <Card className="shadow-2xl border-0 backdrop-blur-sm bg-white/80">
            <CardHeader className="space-y-1 pb-6">
              <CardTitle className="text-3xl font-bold text-center bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent">
                Welcome Back
              </CardTitle>
              <CardDescription className="text-center text-base">
                Sign in to continue your crypto learning adventure
              </CardDescription>
            </CardHeader>
            <CardContent className="pt-0">
              <LoginForm
                onSuccess={handleLoginSuccess}
                onRegisterClick={handleRegisterClick}
                onForgotPasswordClick={handleForgotPasswordClick}
              />
            </CardContent>
          </Card>

          <div className="mt-8 text-center">
            <p className="text-sm text-gray-600">
              New to WahadiniCryptoQuest?{' '}
              <Link 
                to="/register" 
                className="font-semibold text-indigo-600 hover:text-indigo-700 transition-colors"
              >
                Create an account
              </Link>
            </p>
          </div>

          <div className="mt-8 text-center text-xs text-gray-500">
            <p>Protected by industry-standard encryption</p>
          </div>
        </div>
      </div>
    </div>
  );
}