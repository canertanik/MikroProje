import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { 
  Plus, 
  Search, 
  Edit, 
  Trash2, 
  ChevronLeft,
  ChevronRight,
  Filter
} from 'lucide-react';
import { 
  getCurrentAccounts, 
  createCurrentAccount, 
  updateCurrentAccount, 
  deleteCurrentAccount 
} from '../features/current-accounts/api';
import { CurrentAccountType } from '../features/current-accounts/types';
import type { CurrentAccountDto } from '../features/current-accounts/types';
import { CurrentAccountForm } from '../features/current-accounts/components/CurrentAccountForm';
import type { CurrentAccountFormValues } from '../features/current-accounts/components/CurrentAccountForm';
import { ConfirmDialog } from '../components/common/ConfirmDialog';
import { formatCurrency } from '../lib/formatters';
import { useAuthStore } from '../stores/useAuthStore';

export const CurrentAccounts = () => {
  const queryClient = useQueryClient();
  const { user } = useAuthStore();
  const isAdmin = user?.role === 1;
  
  // State
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [typeFilter, setTypeFilter] = useState<CurrentAccountType | 'All'>('All');
  
  // Modals state
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [formMode, setFormMode] = useState<'create' | 'update'>('create');
  const [selectedAccount, setSelectedAccount] = useState<CurrentAccountDto | null>(null);
  
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [accountToDelete, setAccountToDelete] = useState<CurrentAccountDto | null>(null);

  // Queries
  const { data: allAccounts, isLoading, isError, error } = useQuery({
    queryKey: ['current-accounts'],
    queryFn: getCurrentAccounts,
  });

  // Client-side filtering and pagination
  const { paginatedData, totalPages, totalCount, safePage } = useMemo(() => {
    if (!allAccounts) return { paginatedData: [], totalPages: 0, totalCount: 0, safePage: 1 };
    
    let filtered = allAccounts;

    // Filter by type
    if (typeFilter !== 'All') {
      filtered = filtered.filter(a => a.type === typeFilter);
    }

    // Search by code or name
    if (searchTerm.trim()) {
      const lowerSearch = searchTerm.toLowerCase();
      filtered = filtered.filter(a => 
        a.name.toLowerCase().includes(lowerSearch) || 
        a.code.toLowerCase().includes(lowerSearch)
      );
    }

    const totalCount = filtered.length;
    const totalPages = Math.ceil(totalCount / pageSize);
    
    // Calculate safe page to prevent out of bounds when data changes
    const safePage = Math.min(page, Math.max(1, totalPages));

    const start = (safePage - 1) * pageSize;
    const paginatedData = filtered.slice(start, start + pageSize);

    return { paginatedData, totalPages, totalCount, safePage };
  }, [allAccounts, searchTerm, typeFilter, page, pageSize]);

  // Mutations
  const createMutation = useMutation({
    mutationFn: createCurrentAccount,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['current-accounts'] });
      setIsFormOpen(false);
    },
  });

  const updateMutation = useMutation({
    mutationFn: updateCurrentAccount,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['current-accounts'] });
      setIsFormOpen(false);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: deleteCurrentAccount,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['current-accounts'] });
      setIsDeleteOpen(false);
    },
  });

  // Handlers
  const handleCreate = () => {
    setFormMode('create');
    setSelectedAccount(null);
    setIsFormOpen(true);
  };

  const handleEdit = (account: CurrentAccountDto) => {
    setFormMode('update');
    setSelectedAccount(account);
    setIsFormOpen(true);
  };

  const handleDelete = (account: CurrentAccountDto) => {
    setAccountToDelete(account);
    setIsDeleteOpen(true);
  };

  const handleFormSubmit = (values: CurrentAccountFormValues) => {
    if (formMode === 'create') {
      createMutation.mutate({
        ...values,
        taxNumber: values.taxNumber || null,
        phone: values.phone || null,
        email: values.email || null,
      });
    } else if (selectedAccount) {
      updateMutation.mutate({
        id: selectedAccount.id,
        command: {
          ...values,
          id: selectedAccount.id,
          taxNumber: values.taxNumber || null,
          phone: values.phone || null,
          email: values.email || null,
        },
      });
    }
  };

  const confirmDelete = () => {
    if (accountToDelete) {
      deleteMutation.mutate(accountToDelete.id);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-semibold text-gray-900">Cari Hesaplar</h1>
        {isAdmin && (
          <button
            onClick={handleCreate}
            className="inline-flex items-center px-4 py-2 border border-transparent rounded-lg shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
          >
            <Plus className="w-4 h-4 mr-2" />
            Yeni Cari Hesap Ekle
          </button>
        )}
      </div>

      {/* Search and Filters */}
      <div className="bg-white p-4 rounded-xl shadow-sm border border-gray-100 flex flex-col sm:flex-row items-center gap-4">
        <div className="relative flex-1 w-full max-w-md">
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <Search className="h-5 w-5 text-gray-400" />
          </div>
          <input
            type="text"
            className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg leading-5 bg-white placeholder-gray-500 focus:outline-none focus:placeholder-gray-400 focus:ring-1 focus:ring-primary-500 focus:border-primary-500 sm:text-sm"
            placeholder="Cari adı veya kodu ile ara..."
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value);
              setPage(1);
            }}
          />
        </div>
        <div className="relative w-full sm:w-auto">
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <Filter className="h-5 w-5 text-gray-400" />
          </div>
          <select
            className="block w-full pl-10 pr-10 py-2 border border-gray-300 rounded-lg leading-5 bg-white focus:outline-none focus:ring-1 focus:ring-primary-500 focus:border-primary-500 sm:text-sm appearance-none"
            value={typeFilter}
            onChange={(e) => {
              const val = e.target.value;
              setTypeFilter(val === 'All' ? 'All' : Number(val) as CurrentAccountType);
              setPage(1);
            }}
          >
            <option value="All">Tüm Tipler</option>
            <option value={CurrentAccountType.Customer}>Müşteri</option>
            <option value={CurrentAccountType.Supplier}>Tedarikçi</option>
            <option value={CurrentAccountType.Both}>Müşteri & Tedarikçi</option>
          </select>
        </div>
      </div>

      {/* Main Content */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        {isLoading ? (
          <div className="p-8 text-center text-gray-500">Yükleniyor...</div>
        ) : isError ? (
          <div className="p-8 text-center text-red-500">
            {error instanceof Error ? error.message : 'Bir hata oluştu'}
          </div>
        ) : paginatedData.length === 0 ? (
          <div className="p-8 text-center text-gray-500">Kayıt bulunamadı.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Kod
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Ad / Ünvan
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Tip
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    İletişim
                  </th>
                  <th scope="col" className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Bakiye
                  </th>
                  {isAdmin && (
                    <th scope="col" className="relative px-6 py-3">
                      <span className="sr-only">İşlemler</span>
                    </th>
                  )}
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {paginatedData.map((account) => (
                  <tr key={account.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {account.code}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {account.name}
                      {account.taxNumber && (
                        <div className="text-xs text-gray-500">VN: {account.taxNumber}</div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        account.type === CurrentAccountType.Customer 
                          ? 'bg-blue-100 text-blue-800' 
                          : account.type === CurrentAccountType.Supplier
                          ? 'bg-purple-100 text-purple-800'
                          : 'bg-teal-100 text-teal-800'
                      }`}>
                        {account.type === CurrentAccountType.Customer ? 'Müşteri' : account.type === CurrentAccountType.Supplier ? 'Tedarikçi' : 'Müşteri & Tedarikçi'}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      <div>{account.phone || '-'}</div>
                      <div className="text-xs">{account.email || ''}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-right font-medium">
                      <span className={account.balance < 0 ? 'text-red-600' : 'text-green-600'}>
                        {formatCurrency(account.balance)}
                      </span>
                    </td>
                    {isAdmin && (
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <button
                          onClick={() => handleEdit(account)}
                          className="text-primary-600 hover:text-primary-900 mr-4"
                          title="Düzenle"
                        >
                          <Edit className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => handleDelete(account)}
                          className="text-red-600 hover:text-red-900"
                          title="Sil"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="bg-white px-4 py-3 border-t border-gray-200 flex items-center justify-between sm:px-6">
            <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
              <div>
                <p className="text-sm text-gray-700">
                  Toplam <span className="font-medium">{totalCount}</span> kayıttan{' '}
                  <span className="font-medium">{(safePage - 1) * pageSize + 1}</span> -{' '}
                  <span className="font-medium">
                    {Math.min(safePage * pageSize, totalCount)}
                  </span>{' '}
                  arası gösteriliyor
                </p>
              </div>
              <div>
                <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
                  <button
                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                    disabled={safePage <= 1}
                    className="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                  >
                    <span className="sr-only">Önceki</span>
                    <ChevronLeft className="h-5 w-5" aria-hidden="true" />
                  </button>
                  <button
                    onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                    disabled={safePage >= totalPages}
                    className="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                  >
                    <span className="sr-only">Sonraki</span>
                    <ChevronRight className="h-5 w-5" aria-hidden="true" />
                  </button>
                </nav>
              </div>
            </div>
          </div>
        )}
      </div>

      <CurrentAccountForm
        isOpen={isFormOpen}
        mode={formMode}
        initialData={selectedAccount}
        onClose={() => setIsFormOpen(false)}
        onSubmit={handleFormSubmit}
        isSubmitting={createMutation.isPending || updateMutation.isPending}
      />

      <ConfirmDialog
        isOpen={isDeleteOpen}
        title="Cari Hesabı Sil"
        message={`"${accountToDelete?.name}" isimli cari hesabı silmek istediğinize emin misiniz? Bu işlem geri alınamaz.`}
        confirmLabel="Sil"
        onCancel={() => setIsDeleteOpen(false)}
        onConfirm={confirmDelete}
        isLoading={deleteMutation.isPending}
        isDestructive={true}
      />
    </div>
  );
};
