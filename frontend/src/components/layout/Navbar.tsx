import { Menu, User, LogOut } from 'lucide-react';
import { useAuthStore } from '../../stores/useAuthStore';
import { useNavigate } from 'react-router-dom';

interface NavbarProps {
  onMenuClick: () => void;
}

export const Navbar = ({ onMenuClick }: NavbarProps) => {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <header className="bg-white border-b border-gray-200 h-16 flex items-center justify-between px-4 sm:px-6 lg:px-8">
      <div className="flex items-center">
        <button
          type="button"
          className="text-gray-500 hover:text-gray-700 focus:outline-none focus:ring-2 focus:ring-primary-500 lg:hidden p-2 -ml-2 rounded-md"
          onClick={onMenuClick}
        >
          <span className="sr-only">Menüyü aç</span>
          <Menu className="w-6 h-6" />
        </button>
      </div>

      <div className="flex items-center space-x-4">
        <div className="flex items-center space-x-2 text-sm">
          <div className="w-8 h-8 bg-primary-100 text-primary-700 rounded-full flex items-center justify-center font-bold">
            {user?.firstName?.charAt(0) || <User className="w-4 h-4" />}
          </div>
          <span className="hidden sm:block font-medium text-gray-700">
            {user ? `${user.firstName} ${user.lastName}` : 'Kullanıcı'}
          </span>
        </div>
        
        <button
          onClick={handleLogout}
          className="text-gray-400 hover:text-red-600 p-2 rounded-md transition-colors"
          title="Çıkış Yap"
        >
          <LogOut className="w-5 h-5" />
        </button>
      </div>
    </header>
  );
};
