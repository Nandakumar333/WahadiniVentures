import { Link } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { BookOpen, Trophy, Users, Zap, Shield, TrendingUp, Star, CheckCircle2 } from 'lucide-react';

export default function HomePage() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-indigo-50 to-purple-50">
      {/* Navigation */}
      <nav className="bg-white/80 backdrop-blur-md shadow-sm sticky top-0 z-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <Zap className="h-8 w-8 text-indigo-600 mr-2" />
              <h1 className="text-xl font-bold bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent">
                WahadiniCryptoQuest
              </h1>
            </div>
            <div className="flex items-center space-x-4">
              <Link to="/login">
                <Button variant="ghost" className="text-gray-700 hover:text-indigo-600">
                  Sign in
                </Button>
              </Link>
              <Link to="/register">
                <Button className="bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700">
                  Get Started Free
                </Button>
              </Link>
            </div>
          </div>
        </div>
      </nav>

      {/* Hero Section */}
      <main>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pt-16 pb-12">
          <div className="text-center">
            <Badge className="mb-4 bg-indigo-100 text-indigo-700 hover:bg-indigo-200">
              🚀 Join 10,000+ Learners
            </Badge>
            <h1 className="text-5xl tracking-tight font-extrabold text-gray-900 sm:text-6xl md:text-7xl">
              <span className="block">Master Cryptocurrency</span>
              <span className="block bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent">
                Through Interactive Learning
              </span>
            </h1>
            <p className="mt-6 max-w-2xl mx-auto text-xl text-gray-600 leading-relaxed">
              Join WahadiniCryptoQuest and embark on an exciting journey to understand blockchain technology, 
              cryptocurrencies, and digital finance through gamified learning experiences. Earn rewards while you learn!
            </p>
            <div className="mt-10 flex flex-col sm:flex-row gap-4 justify-center items-center">
              <Link to="/register">
                <Button size="lg" className="bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 text-lg px-8 py-6">
                  <Zap className="mr-2 h-5 w-5" />
                  Start Learning Free
                </Button>
              </Link>
              <Link to="/login">
                <Button size="lg" variant="outline" className="text-lg px-8 py-6 border-2">
                  Sign In
                </Button>
              </Link>
            </div>
            
            {/* Stats */}
            <div className="mt-12 grid grid-cols-3 gap-4 max-w-2xl mx-auto">
              <div className="text-center">
                <div className="text-3xl font-bold text-indigo-600">50+</div>
                <div className="text-sm text-gray-600">Courses</div>
              </div>
              <div className="text-center">
                <div className="text-3xl font-bold text-purple-600">10K+</div>
                <div className="text-sm text-gray-600">Students</div>
              </div>
              <div className="text-center">
                <div className="text-3xl font-bold text-pink-600">4.8★</div>
                <div className="text-sm text-gray-600">Rating</div>
              </div>
            </div>
          </div>
        </div>

        {/* Features Section */}
        <div className="py-20 bg-white">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="text-center mb-16">
              <h2 className="text-4xl font-extrabold text-gray-900">
                Why Choose WahadiniCryptoQuest?
              </h2>
              <p className="mt-4 text-xl text-gray-600">
                The complete platform for crypto education and rewards
              </p>
            </div>

            <div className="grid grid-cols-1 gap-8 sm:grid-cols-2 lg:grid-cols-3">
              <Card className="border-2 hover:border-indigo-300 transition-all hover:shadow-lg">
                <CardContent className="pt-6">
                  <div className="flex items-center justify-center h-16 w-16 rounded-2xl bg-gradient-to-br from-indigo-500 to-purple-500 text-white mx-auto mb-4">
                    <BookOpen className="h-8 w-8" />
                  </div>
                  <h3 className="text-xl font-semibold text-gray-900 text-center mb-3">Interactive Courses</h3>
                  <p className="text-base text-gray-600 text-center leading-relaxed">
                    Master crypto with engaging video lessons, quizzes, and hands-on projects. Learn at your own pace.
                  </p>
                </CardContent>
              </Card>

              <Card className="border-2 hover:border-purple-300 transition-all hover:shadow-lg">
                <CardContent className="pt-6">
                  <div className="flex items-center justify-center h-16 w-16 rounded-2xl bg-gradient-to-br from-purple-500 to-pink-500 text-white mx-auto mb-4">
                    <Trophy className="h-8 w-8" />
                  </div>
                  <h3 className="text-xl font-semibold text-gray-900 text-center mb-3">Earn Rewards</h3>
                  <p className="text-base text-gray-600 text-center leading-relaxed">
                    Complete quests, earn badges, and unlock achievements. Get rewarded for your learning progress.
                  </p>
                </CardContent>
              </Card>

              <Card className="border-2 hover:border-pink-300 transition-all hover:shadow-lg">
                <CardContent className="pt-6">
                  <div className="flex items-center justify-center h-16 w-16 rounded-2xl bg-gradient-to-br from-pink-500 to-red-500 text-white mx-auto mb-4">
                    <Users className="h-8 w-8" />
                  </div>
                  <h3 className="text-xl font-semibold text-gray-900 text-center mb-3">Community</h3>
                  <p className="text-base text-gray-600 text-center leading-relaxed">
                    Join thousands of learners. Share insights, collaborate, and grow together in our vibrant community.
                  </p>
                </CardContent>
              </Card>

              <Card className="border-2 hover:border-green-300 transition-all hover:shadow-lg">
                <CardContent className="pt-6">
                  <div className="flex items-center justify-center h-16 w-16 rounded-2xl bg-gradient-to-br from-green-500 to-emerald-500 text-white mx-auto mb-4">
                    <Shield className="h-8 w-8" />
                  </div>
                  <h3 className="text-xl font-semibold text-gray-900 text-center mb-3">Secure & Safe</h3>
                  <p className="text-base text-gray-600 text-center leading-relaxed">
                    Your data is protected with enterprise-grade security. Learn with confidence and peace of mind.
                  </p>
                </CardContent>
              </Card>

              <Card className="border-2 hover:border-blue-300 transition-all hover:shadow-lg">
                <CardContent className="pt-6">
                  <div className="flex items-center justify-center h-16 w-16 rounded-2xl bg-gradient-to-br from-blue-500 to-cyan-500 text-white mx-auto mb-4">
                    <TrendingUp className="h-8 w-8" />
                  </div>
                  <h3 className="text-xl font-semibold text-gray-900 text-center mb-3">Track Progress</h3>
                  <p className="text-base text-gray-600 text-center leading-relaxed">
                    Monitor your learning journey with detailed analytics, progress tracking, and performance insights.
                  </p>
                </CardContent>
              </Card>

              <Card className="border-2 hover:border-yellow-300 transition-all hover:shadow-lg">
                <CardContent className="pt-6">
                  <div className="flex items-center justify-center h-16 w-16 rounded-2xl bg-gradient-to-br from-yellow-500 to-orange-500 text-white mx-auto mb-4">
                    <Star className="h-8 w-8" />
                  </div>
                  <h3 className="text-xl font-semibold text-gray-900 text-center mb-3">Expert Content</h3>
                  <p className="text-base text-gray-600 text-center leading-relaxed">
                    Learn from industry professionals and crypto experts. Content designed for all skill levels.
                  </p>
                </CardContent>
              </Card>
            </div>
          </div>
        </div>

        {/* How It Works Section */}
        <div className="py-20 bg-gradient-to-br from-indigo-50 to-purple-50">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="text-center mb-16">
              <h2 className="text-4xl font-extrabold text-gray-900">
                Start Your Journey in 3 Simple Steps
              </h2>
            </div>
            
            <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
              <div className="text-center">
                <div className="inline-flex items-center justify-center h-20 w-20 rounded-full bg-gradient-to-br from-indigo-600 to-purple-600 text-white text-2xl font-bold mb-4">
                  1
                </div>
                <h3 className="text-xl font-semibold text-gray-900 mb-3">Create Account</h3>
                <p className="text-gray-600">Sign up in seconds and start your crypto education journey today.</p>
              </div>
              
              <div className="text-center">
                <div className="inline-flex items-center justify-center h-20 w-20 rounded-full bg-gradient-to-br from-purple-600 to-pink-600 text-white text-2xl font-bold mb-4">
                  2
                </div>
                <h3 className="text-xl font-semibold text-gray-900 mb-3">Choose Your Path</h3>
                <p className="text-gray-600">Select courses that match your interests and skill level.</p>
              </div>
              
              <div className="text-center">
                <div className="inline-flex items-center justify-center h-20 w-20 rounded-full bg-gradient-to-br from-pink-600 to-red-600 text-white text-2xl font-bold mb-4">
                  3
                </div>
                <h3 className="text-xl font-semibold text-gray-900 mb-3">Learn & Earn</h3>
                <p className="text-gray-600">Complete quests, earn rewards, and become a crypto expert.</p>
              </div>
            </div>
          </div>
        </div>

        {/* CTA Section */}
        <div className="py-20 bg-gradient-to-r from-indigo-600 to-purple-600">
          <div className="max-w-4xl mx-auto text-center px-4">
            <h2 className="text-4xl font-extrabold text-white mb-6">
              Ready to Master Cryptocurrency?
            </h2>
            <p className="text-xl text-indigo-100 mb-10">
              Join thousands of students learning crypto the fun way. Start free today!
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link to="/register">
                <Button size="lg" className="bg-white text-indigo-600 hover:bg-gray-100 text-lg px-10 py-6">
                  <CheckCircle2 className="mr-2 h-5 w-5" />
                  Get Started Free
                </Button>
              </Link>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}