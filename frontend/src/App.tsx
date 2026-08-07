import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

import { AdminLayout } from './layouts/AdminLayout';
import ProtectedRoute from './routes/ProtectedRoute';
import { Login } from './pages/Login';
import { Register } from './pages/Register';
import { Dashboard } from './pages/Dashboard';
import { CurrentAccounts } from './pages/CurrentAccounts';
import { CustomerStatement } from './pages/CustomerStatement';
import { SupplierStatement } from './pages/SupplierStatement';
import { Products } from './pages/Products';
import { Sales } from './pages/Sales';
import { Warehouses } from './pages/Warehouses';
import { StockTransfers } from './pages/StockTransfers';
import { Purchases } from './pages/Purchases';
import { Payments } from './pages/Payments';
import { SupplierPayments } from './pages/SupplierPayments';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: false,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Router>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          
          <Route element={<ProtectedRoute />}>
            <Route element={<AdminLayout />}>
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              <Route path="/dashboard" element={<Dashboard />} />
              <Route path="/customers" element={<CurrentAccounts />} />
              <Route path="/products" element={<Products />} />
              <Route path="/warehouses" element={<Warehouses />} />
              <Route path="/transfers" element={<StockTransfers />} />
              <Route path="/sales" element={<Sales />} />
              <Route path="/purchases" element={<Purchases />} />
              <Route path="/collections" element={<Payments />} />
              <Route path="/payments" element={<SupplierPayments />} />
              <Route path="/customer-statement" element={<CustomerStatement />} />
              <Route path="/supplier-statement" element={<SupplierStatement />} />
            </Route>
          </Route>
          
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </Router>
    </QueryClientProvider>
  );
}

export default App;
