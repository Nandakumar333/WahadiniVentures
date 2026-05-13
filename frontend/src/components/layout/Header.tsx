import { 
  Menu, 
  Moon, 
  Sun, 
  Bell, 
  User, 
  LogOut, 
  Settings, 
  Trophy,
  Coins,
  Zap,
  Crown,
  Search,
  Award
} from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { useTheme } from '@/providers/ThemeProvider';
import { useAuthStore } from '@/store/authStore';

interface HeaderProps {
  onMenuClick: () => void;
}

export const Header: React.FC<HeaderProps> = ({ onMenuClick }) => {
  const { theme, setTheme } = useTheme();
  const navigate = useNavigate();
  const { user, logout } = useAuthStore();

  const handleLogout = async () => {
    try {
      await logout();
      navigate('/login');
    } catch (error) {
      console.error('Logout error:', error);
      navigate('/login');
    }
  };

  const toggleTheme = () => {
    setTheme(theme === 'dark' ? 'light' : 'dark');
  };

  const getUserInitials = () => {
    if (!user) return 'U';
    return `${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}`.toUpperCase();
  };

  const getRoleBadgeContent = () => {
    if (!user) return null;
    switch (user.role) {
      case 2:
        return { icon: Crown, label: 'Admin', variant: 'destructive' as const };
      case 1:
        return { icon: Zap, label: 'Premium', variant: 'default' as const };
      default:
        return { icon: User, label: 'Free', variant: 'secondary' as const };
    }
  };

  const roleBadge = getRoleBadgeContent();

  return (
    <header className="sticky top-0 z-40 w-full border-b border-white/10 bg-background/80 backdrop-blur-xl supports-[backdrop-filter]:bg-background/60">
      <div className="container flex h-16 items-center justify-between px-4 sm:px-6 lg:px-8">
        {/* Left section - Menu and Logo */}
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="icon"
            className="lg:hidden hover:bg-muted"
            onClick={onMenuClick}
            aria-label="Toggle menu"
          >
            <Menu className="h-5 w-5" />
          </Button>
          
          <Link
            to="/dashboard"
            className="flex items-center gap-2 font-bold text-xl hover:opacity-80 transition-opacity group"
            aria-label="Go to dashboard"
          >
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-gradient-to-br from-purple-600 to-blue-600 shadow-lg shadow-blue-500/20 group-hover:scale-105 transition-transform duration-200">
              <Zap className="h-5 w-5 text-white" />
            </div>
            <span className="hidden sm:inline bg-gradient-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent font-extrabold tracking-tight">
              WahadiniCrypto
            </span>
            <span className="sm:hidden bg-gradient-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent font-extrabold">
              WC
            </span>
          </Link>
        </div>

        {/* Center section - Search (Desktop) */}
        <div className="hidden md:flex flex-1 max-w-md mx-8">
          <div className="relative w-full group">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground group-hover:text-primary transition-colors" />
            <Input 
              placeholder="Search courses, lessons..." 
              className="pl-10 bg-muted/50 border-transparent focus:bg-background focus:border-primary/20 transition-all duration-200 rounded-xl"
            />
          </div>
        </div>

        {/* Right section - Actions and User Menu */}
        <div className="flex items-center gap-2 sm:gap-4">
          {/* Points display (for logged-in users) */}
          {user && (
            <div className="hidden md:flex items-center gap-2 px-4 py-1.5 rounded-full bg-gradient-to-r from-yellow-500/10 to-orange-500/10 border border-yellow-500/20">
              <Coins className="h-4 w-4 text-yellow-600 dark:text-yellow-400" />
              <span className="text-sm font-bold text-yellow-700 dark:text-yellow-300">
                {user.totalPoints || 0}
              </span>
            </div>
          )}

          <div className="flex items-center gap-1 sm:gap-2">
            {/* Theme Toggle */}
            <Button
              variant="ghost"
              size="icon"
              onClick={toggleTheme}
              className="relative rounded-full hover:bg-muted"
              aria-label={`Switch to ${theme === 'dark' ? 'light' : 'dark'} mode`}
            >
              {theme === 'dark' ? (
                <Sun className="h-5 w-5 text-yellow-500 transition-all rotate-0 scale-100" />
              ) : (
                <Moon className="h-5 w-5 text-slate-700 transition-all rotate-0 scale-100" />
              )}
            </Button>

            {user ? (
              <>
                {/* Notifications */}
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => navigate('/notifications')}
                  className="relative rounded-full hover:bg-muted"
                  aria-label="Notifications"
                >
                  <Bell className="h-5 w-5" />
                  {/* Notification badge */}
                  <span className="absolute top-2 right-2 flex h-2.5 w-2.5">
                    <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-red-400 opacity-75"></span>
                    <span className="relative inline-flex rounded-full h-2.5 w-2.5 bg-red-500"></span>
                  </span>
                </Button>

                {/* User Menu */}
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button
                      variant="ghost"
                      className="relative h-10 w-10 rounded-full ring-2 ring-transparent hover:ring-primary/20 transition-all p-0 ml-1"
                      aria-label="User menu"
                    >
                      <Avatar className="h-9 w-9">
                        <AvatarImage src={user.avatar} alt={`${user.firstName} ${user.lastName}`} />
                        <AvatarFallback className="bg-gradient-to-br from-purple-600 to-blue-600 text-white font-bold text-sm">
                          {getUserInitials()}
                        </AvatarFallback>
                      </Avatar>
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end" className="w-72 p-2" sideOffset={8}>
                    <DropdownMenuLabel className="p-2">
                      <div className="flex items-center gap-3">
                        <Avatar className="h-10 w-10">
                          <AvatarImage src={user.avatar} alt={`${user.firstName} ${user.lastName}`} />
                          <AvatarFallback className="bg-gradient-to-br from-purple-600 to-blue-600 text-white font-bold">
                            {getUserInitials()}
                          </AvatarFallback>
                        </Avatar>
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-bold truncate">
                            {user.firstName} {user.lastName}
                          </p>
                          <p className="text-xs text-muted-foreground truncate">{user.email}</p>
                        </div>
                      </div>
                    </DropdownMenuLabel>

                    <div className="px-2 pb-2">
                      <div className="flex items-center gap-2 mt-1">
                        {roleBadge && (
                          <Badge variant={roleBadge.variant} className="text-xs">
                            <roleBadge.icon className="h-3 w-3 mr-1" />
                            {roleBadge.label}
                          </Badge>
                        )}
                      </div>
                    </div>

                    <DropdownMenuSeparator />

                    <DropdownMenuItem onClick={() => navigate('/dashboard')} className="cursor-pointer rounded-lg">
                      <User className="mr-2 h-4 w-4" />
                      <span>Dashboard</span>
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => navigate('/profile')} className="cursor-pointer rounded-lg">
                      <Settings className="mr-2 h-4 w-4" />
                      <span>Profile Settings</span>
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => navigate('/leaderboard')} className="cursor-pointer rounded-lg">
                      <Award className="mr-2 h-4 w-4" />
                      <span>Leaderboard</span>
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => navigate('/rewards')} className="cursor-pointer rounded-lg md:hidden">
                      <Trophy className="mr-2 h-4 w-4" />
                      <span>Rewards</span>
                    </DropdownMenuItem>
                    
                    {user.role !== 1 && (
                      <DropdownMenuItem onClick={() => navigate('/subscription')} className="cursor-pointer rounded-lg bg-gradient-to-r from-yellow-500/10 to-orange-500/10 text-yellow-700 dark:text-yellow-400 mt-1">
                        <Crown className="mr-2 h-4 w-4" />
                        <span>Upgrade to Premium</span>
                      </DropdownMenuItem>
                    )}

                    <DropdownMenuSeparator />

                    <DropdownMenuItem 
                      onClick={handleLogout} 
                      className="cursor-pointer rounded-lg text-red-600 dark:text-red-400 focus:text-red-600 dark:focus:text-red-400 focus:bg-red-50 dark:focus:bg-red-950/20"
                    >
                      <LogOut className="mr-2 h-4 w-4" />
                      <span>Logout</span>
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </>
            ) : (
              <div className="flex items-center gap-2">
                <Button 
                  variant="ghost" 
                  onClick={() => navigate('/login')}
                  className="hidden sm:inline-flex font-semibold"
                >
                  Login
                </Button>
                <Button 
                  onClick={() => navigate('/register')}
                  className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 shadow-lg shadow-blue-500/20 font-semibold"
                >
                  <Zap className="mr-2 h-4 w-4" />
                  Get Started
                </Button>
              </div>
            )}
          </div>
        </div>
      </div>
    </header>
  );
};

