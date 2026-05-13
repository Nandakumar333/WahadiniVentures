import { useAuthStore } from '@/store/authStore';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Link } from 'react-router-dom';
import { 
  BookOpen, 
  Trophy, 
  Clock,
  Award,
  Target,
  Zap,
  ChevronRight,
  TrendingUp,
  Star,
  CheckCircle2
} from 'lucide-react';

export default function DashboardPage() {
  const { user } = useAuthStore();

  return (
    <div className="space-y-8 pb-10">
      {/* Welcome Header */}
      <div className="relative overflow-hidden rounded-3xl bg-gradient-to-r from-blue-600 to-indigo-600 p-8 sm:p-12 text-white shadow-xl">
        <div className="absolute top-0 right-0 -mt-10 -mr-10 h-64 w-64 rounded-full bg-white/10 blur-3xl" />
        <div className="absolute bottom-0 left-0 -mb-10 -ml-10 h-64 w-64 rounded-full bg-purple-500/20 blur-3xl" />
        
        <div className="relative z-10">
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-6">
            <div>
              <h1 className="text-3xl sm:text-4xl font-bold mb-2">
                Welcome back, {user?.firstName || 'Learner'}! 👋
              </h1>
              <p className="text-blue-100 text-lg max-w-xl">
                Ready to continue your crypto journey? You're making great progress!
              </p>
            </div>
            <div className="flex items-center gap-4 bg-white/10 backdrop-blur-md rounded-2xl p-4 border border-white/20">
              <div className="p-3 bg-yellow-400/20 rounded-xl">
                <Zap className="h-8 w-8 text-yellow-300" />
              </div>
              <div>
                <p className="text-sm text-blue-100 font-medium">Daily Streak</p>
                <div className="flex items-baseline gap-1">
                  <span className="text-2xl font-bold">0</span>
                  <span className="text-sm text-blue-200">days</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Stats Overview */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <Card className="border-none shadow-lg bg-white/50 backdrop-blur-sm hover:bg-white/80 transition-all duration-300 group">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between mb-4">
              <div className="p-3 bg-blue-100 text-blue-600 rounded-xl group-hover:scale-110 transition-transform duration-300">
                <BookOpen className="h-6 w-6" />
              </div>
              <Badge variant="secondary" className="bg-green-100 text-green-700">
                <TrendingUp className="h-3 w-3 mr-1" />
                +12%
              </Badge>
            </div>
            <div>
              <p className="text-sm font-medium text-gray-500">Courses Progress</p>
              <h3 className="text-2xl font-bold text-gray-900 mt-1">0%</h3>
            </div>
            <div className="mt-4">
              <div className="w-full bg-gray-100 rounded-full h-1.5 overflow-hidden">
                <div className="bg-blue-600 h-full rounded-full w-0 transition-all duration-1000" style={{ width: '0%' }} />
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-none shadow-lg bg-white/50 backdrop-blur-sm hover:bg-white/80 transition-all duration-300 group">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between mb-4">
              <div className="p-3 bg-yellow-100 text-yellow-600 rounded-xl group-hover:scale-110 transition-transform duration-300">
                <Trophy className="h-6 w-6" />
              </div>
              <Badge variant="secondary" className="bg-yellow-100 text-yellow-700">
                Level 1
              </Badge>
            </div>
            <div>
              <p className="text-sm font-medium text-gray-500">Achievements</p>
              <h3 className="text-2xl font-bold text-gray-900 mt-1">0</h3>
            </div>
            <p className="text-xs text-gray-400 mt-4">Next badge in 2 quests</p>
          </CardContent>
        </Card>

        <Card className="border-none shadow-lg bg-white/50 backdrop-blur-sm hover:bg-white/80 transition-all duration-300 group">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between mb-4">
              <div className="p-3 bg-purple-100 text-purple-600 rounded-xl group-hover:scale-110 transition-transform duration-300">
                <Star className="h-6 w-6" />
              </div>
              <Badge variant="secondary" className="bg-purple-100 text-purple-700">
                Rank #--
              </Badge>
            </div>
            <div>
              <p className="text-sm font-medium text-gray-500">Total Points</p>
              <h3 className="text-2xl font-bold text-gray-900 mt-1">{user?.totalPoints || 0}</h3>
            </div>
            <p className="text-xs text-gray-400 mt-4">Top 10% of learners</p>
          </CardContent>
        </Card>

        <Card className="border-none shadow-lg bg-white/50 backdrop-blur-sm hover:bg-white/80 transition-all duration-300 group">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between mb-4">
              <div className="p-3 bg-pink-100 text-pink-600 rounded-xl group-hover:scale-110 transition-transform duration-300">
                <Clock className="h-6 w-6" />
              </div>
              <Badge variant="secondary" className="bg-gray-100 text-gray-600">
                Total
              </Badge>
            </div>
            <div>
              <p className="text-sm font-medium text-gray-500">Hours Learned</p>
              <h3 className="text-2xl font-bold text-gray-900 mt-1">0h</h3>
            </div>
            <p className="text-xs text-gray-400 mt-4">Updated just now</p>
          </CardContent>
        </Card>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Quick Actions */}
        <div className="lg:col-span-2 space-y-6">
          <div className="flex items-center justify-between">
            <h2 className="text-xl font-bold text-gray-900">Quick Actions</h2>
          </div>
          
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <Link to="/courses" className="group">
              <div className="bg-white p-6 rounded-2xl shadow-sm border border-gray-100 hover:shadow-md hover:border-blue-200 transition-all duration-300 h-full">
                <div className="flex items-start justify-between">
                  <div className="p-3 bg-blue-50 rounded-xl group-hover:bg-blue-100 transition-colors">
                    <BookOpen className="h-6 w-6 text-blue-600" />
                  </div>
                  <div className="bg-gray-50 p-2 rounded-full group-hover:bg-blue-50 transition-colors">
                    <ChevronRight className="h-4 w-4 text-gray-400 group-hover:text-blue-600" />
                  </div>
                </div>
                <h3 className="font-bold text-gray-900 mt-4 mb-1">Browse Courses</h3>
                <p className="text-sm text-gray-500">Explore our catalog of crypto courses and start learning.</p>
              </div>
            </Link>

            <Link to="/my-courses" className="group">
              <div className="bg-white p-6 rounded-2xl shadow-sm border border-gray-100 hover:shadow-md hover:border-green-200 transition-all duration-300 h-full">
                <div className="flex items-start justify-between">
                  <div className="p-3 bg-green-50 rounded-xl group-hover:bg-green-100 transition-colors">
                    <Target className="h-6 w-6 text-green-600" />
                  </div>
                  <div className="bg-gray-50 p-2 rounded-full group-hover:bg-green-50 transition-colors">
                    <ChevronRight className="h-4 w-4 text-gray-400 group-hover:text-green-600" />
                  </div>
                </div>
                <h3 className="font-bold text-gray-900 mt-4 mb-1">My Learning</h3>
                <p className="text-sm text-gray-500">Continue where you left off and track your progress.</p>
              </div>
            </Link>

            <Link to="/rewards" className="group">
              <div className="bg-white p-6 rounded-2xl shadow-sm border border-gray-100 hover:shadow-md hover:border-yellow-200 transition-all duration-300 h-full">
                <div className="flex items-start justify-between">
                  <div className="p-3 bg-yellow-50 rounded-xl group-hover:bg-yellow-100 transition-colors">
                    <Award className="h-6 w-6 text-yellow-600" />
                  </div>
                  <div className="bg-gray-50 p-2 rounded-full group-hover:bg-yellow-50 transition-colors">
                    <ChevronRight className="h-4 w-4 text-gray-400 group-hover:text-yellow-600" />
                  </div>
                </div>
                <h3 className="font-bold text-gray-900 mt-4 mb-1">Achievements</h3>
                <p className="text-sm text-gray-500">View your badges, points, and leaderboard status.</p>
              </div>
            </Link>

            <Link to="/leaderboard" className="group">
              <div className="bg-white p-6 rounded-2xl shadow-sm border border-gray-100 hover:shadow-md hover:border-purple-200 transition-all duration-300 h-full">
                <div className="flex items-start justify-between">
                  <div className="p-3 bg-purple-50 rounded-xl group-hover:bg-purple-100 transition-colors">
                    <Trophy className="h-6 w-6 text-purple-600" />
                  </div>
                  <div className="bg-gray-50 p-2 rounded-full group-hover:bg-purple-50 transition-colors">
                    <ChevronRight className="h-4 w-4 text-gray-400 group-hover:text-purple-600" />
                  </div>
                </div>
                <h3 className="font-bold text-gray-900 mt-4 mb-1">Leaderboard</h3>
                <p className="text-sm text-gray-500">Compete with other learners and climb the ranks.</p>
              </div>
            </Link>
          </div>
        </div>

        {/* Sidebar Widgets */}
        <div className="space-y-6">
          {/* Current Goals */}
          <Card className="border-none shadow-lg bg-white/80 backdrop-blur-sm">
            <CardHeader>
              <CardTitle className="text-lg font-bold flex items-center gap-2">
                <Target className="h-5 w-5 text-indigo-600" />
                Current Goals
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {[
                { title: 'Complete your first course', desc: 'Start your crypto journey', done: false },
                { title: 'Earn your first badge', desc: 'Complete a quiz or challenge', done: false },
                { title: 'Build a 7-day streak', desc: 'Learn daily this week', done: false }
              ].map((goal, i) => (
                <div key={i} className="flex items-start gap-3 p-3 rounded-xl hover:bg-gray-50 transition-colors">
                  <div className={`mt-1 h-5 w-5 rounded-full border-2 flex items-center justify-center ${goal.done ? 'bg-green-500 border-green-500' : 'border-gray-300'}`}>
                    {goal.done && <CheckCircle2 className="h-3 w-3 text-white" />}
                  </div>
                  <div>
                    <p className="font-semibold text-gray-900">{goal.title}</p>
                    <p className="text-xs text-gray-500">{goal.desc}</p>
                  </div>
                </div>
              ))}
            </CardContent>
          </Card>

          {/* Account Status */}
          <div className="bg-gradient-to-br from-gray-900 to-gray-800 rounded-2xl p-6 text-white shadow-xl relative overflow-hidden">
            <div className="absolute top-0 right-0 -mt-4 -mr-4 h-24 w-24 rounded-full bg-white/10 blur-2xl" />
            
            <div className="relative z-10">
              <h3 className="font-bold text-lg mb-1">Account Status</h3>
              <div className="flex items-center gap-2 mb-4">
                <Badge variant="secondary" className="bg-white/20 text-white hover:bg-white/30 border-none">
                  {user?.role === 2 ? 'Admin' : user?.role === 1 ? 'Premium' : 'Free Plan'}
                </Badge>
                {user?.emailConfirmed && (
                  <Badge variant="outline" className="text-green-400 border-green-400/30 bg-green-400/10">
                    Verified
                  </Badge>
                )}
              </div>

              {user?.role === 0 && (
                <>
                  <p className="text-sm text-gray-300 mb-4">
                    Upgrade to Premium to unlock exclusive courses, advanced analytics, and more!
                  </p>
                  <Button className="w-full bg-gradient-to-r from-yellow-400 to-yellow-600 hover:from-yellow-500 hover:to-yellow-700 text-gray-900 font-bold border-none shadow-lg shadow-yellow-500/20">
                    Upgrade Now
                  </Button>
                </>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}