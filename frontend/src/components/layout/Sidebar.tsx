import { Link, useLocation } from 'react-router-dom';
import { 
  LayoutDashboard, 
  Users, 
  Package, 
  Warehouse, 
  ArrowRightLeft, 
  ShoppingCart, 
  ShoppingBag, 
  Banknote, 
  CreditCard, 
  FileText, 
  Receipt 
} from 'lucide-react';

const navItems = [
  { name: 'Dashboard', path: '/dashboard', icon: LayoutDashboard },
  { name: 'Cari Hesaplar', path: '/customers', icon: Users },
  { name: 'Ürünler', path: '/products', icon: Package },
  { name: 'Depolar', path: '/warehouses', icon: Warehouse },
  { name: 'Stok Transferleri', path: '/transfers', icon: ArrowRightLeft },
  { name: 'Satışlar', path: '/sales', icon: ShoppingCart },
  { name: 'Satın Almalar', path: '/purchases', icon: ShoppingBag },
  { name: 'Müşteri Tahsilatları', path: '/collections', icon: Banknote },
  { name: 'Tedarikçi Ödemeleri', path: '/payments', icon: CreditCard },
  { name: 'Cari Ekstre', path: '/customer-statement', icon: FileText },
  { name: 'Tedarikçi Ekstresi', path: '/supplier-statement', icon: Receipt },
];

interface SidebarProps {
  isOpen: boolean;
  setIsOpen: (isOpen: boolean) => void;
}

export const Sidebar = ({ isOpen, setIsOpen }: SidebarProps) => {
  const location = useLocation();

  return (
    <>
      {/* Mobile overlay */}
      {isOpen && (
        <div 
          className="fixed inset-0 z-40 bg-gray-900/50 lg:hidden"
          onClick={() => setIsOpen(false)}
        />
      )}

      {/* Sidebar */}
      <div className={`fixed inset-y-0 left-0 z-50 w-64 bg-white border-r border-gray-200 transform transition-transform duration-200 ease-in-out lg:translate-x-0 lg:static lg:inset-0 ${isOpen ? 'translate-x-0' : '-translate-x-full'}`}>
        <div className="flex items-center justify-center h-16 border-b border-gray-200">
          <span className="-translate-x-2 text-xl font-bold text-primary-600">Nexora ERP</span>
        </div>
        
        <div className="overflow-y-auto h-[calc(100vh-4rem)] p-4 space-y-1">
          {navItems.map((item) => {
            const Icon = item.icon;
            const isActive = location.pathname === item.path || (item.path !== '/dashboard' && location.pathname.startsWith(item.path));
            
            return (
              <Link
                key={item.path}
                to={item.path}
                className={`flex items-center px-4 py-2.5 text-sm font-medium rounded-lg transition-colors ${
                  isActive 
                    ? 'bg-primary-50 text-primary-700' 
                    : 'text-gray-700 hover:bg-gray-100'
                }`}
                onClick={() => setIsOpen(false)}
              >
                <Icon className={`w-5 h-5 mr-3 ${isActive ? 'text-primary-600' : 'text-gray-400'}`} />
                {item.name}
              </Link>
            );
          })}
        </div>
      </div>
    </>
  );
};
