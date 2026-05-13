import { 
  X, 
  Home, 
  CheckSquare, 
  Award, 
  Settings, 
  HelpCircle, 
  Shield,
  ChevronLeft,
  ChevronRight,
  Sparkles,
  Trophy,
  Coins,
  BookMarked,
  LogOut
} from 'lucide-react';
import { Link, useLocation } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { useAuthStore } from '@/store/authStore';
import { cn } from '@/lib/utils';
import { useState } from 'react';

interface SidebarProps {
  isOpen: boolean;
  onClose: () => void;
}

interface NavItem {
  label: string;
  path: string;
  icon: React.ElementType;
  badge?: string | number;
  adminOnly?: boolean;
  group?: 'main' | 'learning' | 'rewards' | 'settings';
}

const navigationItems: NavItem[] = [
  // Main Navigation
  { label: 'Dashboard', path: '/dashboard', icon: Home, group: 'main' },
  
  // Learning Section
  { label: 'Browse Courses', path: '/courses', icon: Sparkles, group: 'learning' },
  { label: 'My Courses', path: '/my-courses', icon: BookMarked, group: 'learning' },
  { label: 'Tasks', path: '/my-submissions', icon: CheckSquare, badge: 3, group: 'learning' },
  
  // Rewards Section
  { label: 'Rewards', path: '/rewards', icon: Trophy, group: 'rewards' },
  { label: 'Leaderboard', path: '/leaderboard', icon: Award, group: 'rewards' },
  
  // Admin Section
  { label: 'Admin Panel', path: '/admin/courses', icon: Shield, adminOnly: true, group: 'settings' },
  
  // Settings Section
  { label: 'Settings', path: '/settings', icon: Settings, group: 'settings' },
  { label: 'Help & Support', path: '/help', icon: HelpCircle, group: 'settings' },
];

const navGroups = {
  main: 'Main',
  learning: 'Learning',
  rewards: 'Rewards',
  settings: 'Settings'
};

export const Sidebar: React.FC<SidebarProps> = ({ isOpen, onClose }) => {
  const location = useLocation();
  const { user, logout } = useAuthStore();
  const [isCollapsed, setIsCollapsed] = useState(false);

  const isActivePath = (path: string) => location.pathname === path;

  // Get user initials
  const getUserInitials = () => {
    if (!user) return 'U';
    return `${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}`.toUpperCase();
  };

  // Get role display
  const getRoleDisplay = () => {
    if (!user) return 'Guest';
    switch (user.role) {
      case 2: return 'Admin';
      case 1: return 'Premium';
      default: return 'Free';
    }
  };

  // Get role color
  const getRoleBadgeVariant = () => {
    if (!user) return 'secondary';
    switch (user.role) {
      case 2: return 'destructive' as const;
      case 1: return 'default' as const;
      default: return 'secondary' as const;
    }
  };

  // Group navigation items
  const groupedItems = navigationItems.reduce((acc, item) => {
    const group = item.group || 'main';
    if (!acc[group]) acc[group] = [];
    acc[group].push(item);
    return acc;
  }, {} as Record<string, NavItem[]>);

  return (
    <>
      {/* Overlay for mobile */}
      {isOpen && (
        <div
          className="fixed inset-0 z-40 bg-black/50 backdrop-blur-sm lg:hidden"
          onClick={onClose}
          aria-hidden="true"
        />
      )}

      {/* Sidebar */}
      <aside
        className={cn(
          'fixed top-16 left-0 z-50 h-[calc(100vh-4rem)] border-r border-white/10 bg-background/80 backdrop-blur-xl transition-all duration-300 lg:relative lg:top-0 lg:translate-x-0',
          isOpen ? 'translate-x-0' : '-translate-x-full',
          isCollapsed ? 'w-20' : 'w-72'
        )}
        role="navigation"
        aria-label="Sidebar navigation"
      >
        <div className="flex h-full flex-col">
          {/* Collapse toggle (desktop only) */}
          <div className="hidden lg:flex items-center justify-end p-4">
            <Button
              variant="ghost"
              size="icon"
              onClick={() => setIsCollapsed(!isCollapsed)}
              className="h-8 w-8 rounded-full hover:bg-muted"
              aria-label={isCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
            >
              {isCollapsed ? (
                <ChevronRight className="h-4 w-4" />
              ) : (
                <ChevronLeft className="h-4 w-4" />
              )}
            </Button>
          </div>

          {/* Close button (mobile only) */}
          <div className="flex items-center justify-between p-4 lg:hidden">
            <h2 className="text-lg font-bold bg-gradient-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent">Menu</h2>
            <Button
              variant="ghost"
              size="icon"
              onClick={onClose}
              aria-label="Close sidebar"
            >
              <X className="h-5 w-5" />
            </Button>
          </div>

          <Separator className="lg:hidden" />

          {/* Navigation Links */}
          <nav className="flex-1 overflow-y-auto scrollbar-thin scrollbar-thumb-muted scrollbar-track-transparent py-4">
            <div className="space-y-6 px-3">
              {Object.entries(groupedItems).map(([groupKey, items]) => {
                const filteredItems = items.filter(
                  item => !item.adminOnly || user?.role === 2
                );

                if (filteredItems.length === 0) return null;

                return (
                  <div key={groupKey} className="space-y-1">
                    {!isCollapsed && (
                      <h3 className="px-3 py-2 text-xs font-bold text-muted-foreground uppercase tracking-wider">
                        {navGroups[groupKey as keyof typeof navGroups]}
                      </h3>
                    )}
                    {filteredItems.map((item) => {
                      const Icon = item.icon;
                      const isActive = isActivePath(item.path);

                      return (
                        <Link
                          key={item.path}
                          to={item.path}
                          onClick={onClose}
                          className={cn(
                            'group flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-all duration-200 relative overflow-hidden',
                            isActive
                              ? 'text-white shadow-lg shadow-blue-500/20'
                              : 'text-muted-foreground hover:text-foreground hover:bg-muted/50',
                            isCollapsed && 'justify-center px-2'
                          )}
                          aria-current={isActive ? 'page' : undefined}
                          title={isCollapsed ? item.label : undefined}
                        >
                          {isActive && (
                            <div className="absolute inset-0 bg-gradient-to-r from-blue-600 to-indigo-600 opacity-100" />
                          )}
                          
                          <Icon className={cn(
                            'h-5 w-5 shrink-0 relative z-10 transition-transform duration-200 group-hover:scale-110',
                            isActive ? 'text-white' : 'text-muted-foreground group-hover:text-foreground'
                          )} />
                          
                          {!isCollapsed && (
                            <>
                              <span className="flex-1 truncate relative z-10">{item.label}</span>
                              {item.badge && (
                                <Badge 
                                  variant="secondary" 
                                  className={cn(
                                    'ml-auto h-5 min-w-[20px] px-1.5 text-xs relative z-10',
                                    isActive ? 'bg-white/20 text-white border-none' : ''
                                  )}
                                >
                                  {item.badge}
                                </Badge>
                              )}
                            </>
                          )}
                          
                          {isCollapsed && item.badge && (
                            <span className="absolute -right-1 -top-1 flex h-4 w-4 items-center justify-center rounded-full bg-blue-600 text-[10px] text-white ring-2 ring-background">
                              {item.badge}
                            </span>
                          )}
                        </Link>
                      );
                    })}
                  </div>
                );
              })}
            </div>
          </nav>

          {/* User Info Card */}
          {user && (
            <div className={cn('p-4 mt-auto', isCollapsed && 'p-2')}>
              <div className={cn(
                'rounded-2xl bg-gradient-to-br from-gray-900 to-gray-800 p-4 border border-white/10 shadow-xl relative overflow-hidden group',
                isCollapsed && 'p-2 bg-none border-none shadow-none'
              )}>
                {!isCollapsed && (
                  <div className="absolute inset-0 bg-gradient-to-r from-blue-600/10 to-purple-600/10 opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
                )}
                
                <div className={cn('flex items-center gap-3 relative z-10', isCollapsed && 'flex-col gap-2')}>
                  <Avatar className={cn('h-10 w-10 ring-2 ring-white/20', isCollapsed && 'h-8 w-8')}>
                    <AvatarImage src={user.avatar} alt={`${user.firstName} ${user.lastName}`} />
                    <AvatarFallback className="bg-gradient-to-br from-blue-500 to-purple-500 text-white font-bold">
                      {getUserInitials()}
                    </AvatarFallback>
                  </Avatar>
                  
                  {!isCollapsed && (
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-bold text-white truncate">
                        {user.firstName} {user.lastName}
                      </p>
                      <div className="flex items-center gap-2 mt-1">
                        <Badge variant={getRoleBadgeVariant()} className="h-5 text-[10px] px-1.5">
                          {getRoleDisplay()}
                        </Badge>
                        <div className="flex items-center gap-1 text-xs text-yellow-400">
                          <Coins className="h-3 w-3" />
                          <span className="font-medium">{user.totalPoints || 0}</span>
                        </div>
                      </div>
                    </div>
                  )}
                  
                  {!isCollapsed && (
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => logout()}
                      className="h-8 w-8 text-gray-400 hover:text-white hover:bg-white/10 rounded-full"
                    >
                      <LogOut className="h-4 w-4" />
                    </Button>
                  )}
                </div>
              </div>
            </div>
          )}
        </div>
      </aside>
    </>
  );
};

