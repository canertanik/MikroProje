import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { 
  Plus, 
  Search, 
  Edit, 
  Trash2, 
  ChevronLeft,
  ChevronRight,
  Filter,
  CheckCircle,
  Star
} from 'lucide-react';
import { 
  getWarehouses, 
  createWarehouse, 
  updateWarehouse, 
  deleteWarehouse,
  getWarehouseById,
  setDefaultWarehouse
} from '../features/warehouses/api';
import type { WarehouseListDto, WarehouseDto } from '../features/warehouses/types';
import { WarehouseForm } from '../features/warehouses/components/WarehouseForm';
import type { WarehouseFormValues } from '../features/warehouses/components/WarehouseForm';
import { ConfirmDialog } from '../components/common/ConfirmDialog';
import { useAuthStore } from '../stores/useAuthStore';

export const Warehouses = () => {
  const queryClient = useQueryClient();
  const { user } = useAuthStore();
  const isAdmin = user?.role === 1;
  
  // State for pagination & filtering
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [activeFilter, setActiveFilter] = useState<'All' | 'Active' | 'Inactive'>('All');
  const [defaultFilter, setDefaultFilter] = useState<'All' | 'Default' | 'NotDefault'>('All');
  
  // Modals state
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [formMode, setFormMode] = useState<'create' | 'update'>('create');
  const [selectedWarehouse, setSelectedWarehouse] = useState<WarehouseDto | null>(null);
  
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [warehouseToDelete, setWarehouseToDelete] = useState<WarehouseListDto | null>(null);

  // Fetching data
  const { data: pagedData, isLoading, isError, error } = useQuery({
    queryKey: [
      'warehouses', 
      page, 
      pageSize, 
      searchTerm, 
      activeFilter, 
      defaultFilter
    ],
    queryFn: () => getWarehouses(
      page, 
      pageSize, 
      searchTerm || undefined, 
      activeFilter === 'All' ? undefined : activeFilter === 'Active',
      defaultFilter === 'All' ? undefined : defaultFilter === 'Default'
    ),
  });

  // Derived pagination info
  const items = pagedData?.items || [];
  const totalCount = pagedData?.totalCount || 0;
  const totalPages = pagedData?.totalPages || 0;

  // Mutations
  const createMutation = useMutation({
    mutationFn: createWarehouse,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['warehouses'] });
      setIsFormOpen(false);
    },
  });

  const updateMutation = useMutation({
    mutationFn: updateWarehouse,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['warehouses'] });
      setIsFormOpen(false);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: deleteWarehouse,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['warehouses'] });
      setIsDeleteOpen(false);
      
      // If we delete the last item on the current page, go back one page
      if (items.length === 1 && page > 1) {
        setPage(page - 1);
      }
    },
    onError: (err: Error) => {
      alert(err.message || 'Depo silinirken bir hata oluştu.');
      setIsDeleteOpen(false);
    }
  });

  const setDefaultMutation = useMutation({
    mutationFn: setDefaultWarehouse,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['warehouses'] });
    },
    onError: (err: Error) => {
      alert(err.message || 'Varsayılan depo ayarlanırken bir hata oluştu.');
    }
  });

  // Handlers
  const handleCreate = () => {
    setFormMode('create');
    setSelectedWarehouse(null);
    setIsFormOpen(true);
  };

  const handleEdit = async (item: WarehouseListDto) => {
    try {
      const fullData = await getWarehouseById(item.id);
      setSelectedWarehouse(fullData);
      setFormMode('update');
      setIsFormOpen(true);
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Kayıt detayları getirilemedi.');
    }
  };

  const handleDelete = (item: WarehouseListDto) => {
    setWarehouseToDelete(item);
    setIsDeleteOpen(true);
  };

  const handleSetDefault = (item: WarehouseListDto) => {
    if (!item.isDefault) {
      setDefaultMutation.mutate(item.id);
    }
  };

  const handleFormSubmit = (values: WarehouseFormValues) => {
    if (formMode === 'create') {
      createMutation.mutate({
        ...values,
        description: values.description || null,
      });
    } else if (selectedWarehouse) {
      updateMutation.mutate({
        id: selectedWarehouse.id,
        command: {
          ...values,
          id: selectedWarehouse.id,
          description: values.description || null,
          rowVersion: selectedWarehouse.rowVersion
        },
      });
    }
  };

  const confirmDelete = () => {
    if (warehouseToDelete) {
      deleteMutation.mutate(warehouseToDelete.id);
    }
  };



  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-semibold text-gray-900">Depolar</h1>
        {isAdmin && (
          <button
            onClick={handleCreate}
            className="inline-flex items-center px-4 py-2 border border-transparent rounded-lg shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
          >
            <Plus className="w-4 h-4 mr-2" />
            Yeni Depo Ekle
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
            placeholder="Depo adı veya kodu ile ara..."
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
            value={activeFilter}
            onChange={(e) => {
              setActiveFilter(e.target.value as any);
              setPage(1);
            }}
          >
            <option value="All">Tüm Durumlar</option>
            <option value="Active">Sadece Aktifler</option>
            <option value="Inactive">Sadece Pasifler</option>
          </select>
        </div>

        <div className="relative w-full sm:w-auto">
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <Star className="h-5 w-5 text-gray-400" />
          </div>
          <select
            className="block w-full pl-10 pr-10 py-2 border border-gray-300 rounded-lg leading-5 bg-white focus:outline-none focus:ring-1 focus:ring-primary-500 focus:border-primary-500 sm:text-sm appearance-none"
            value={defaultFilter}
            onChange={(e) => {
              setDefaultFilter(e.target.value as any);
              setPage(1);
            }}
          >
            <option value="All">Tümü</option>
            <option value="Default">Varsayılan Depolar</option>
            <option value="NotDefault">Diğerleri</option>
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
        ) : items.length === 0 ? (
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
                    Depo Adı
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Durum
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Varsayılan
                  </th>
                  {isAdmin && (
                    <th scope="col" className="relative px-6 py-3">
                      <span className="sr-only">İşlemler</span>
                    </th>
                  )}
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {items.map((item) => (
                  <tr key={item.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {item.code}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {item.name}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        item.isActive 
                          ? 'bg-green-100 text-green-800' 
                          : 'bg-red-100 text-red-800'
                      }`}>
                        {item.isActive ? 'Aktif' : 'Pasif'}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {item.isDefault ? (
                        <CheckCircle className="w-5 h-5 text-green-500" />
                      ) : (
                        <span className="text-gray-400">-</span>
                      )}
                    </td>
                    {isAdmin && (
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-3">
                        {!item.isDefault && (
                          <button
                            onClick={() => handleSetDefault(item)}
                            disabled={setDefaultMutation.isPending}
                            className="text-blue-600 hover:text-blue-900"
                            title="Varsayılan Yap"
                          >
                            <Star className="w-4 h-4" />
                          </button>
                        )}
                        <button
                          onClick={() => handleEdit(item)}
                          className="text-primary-600 hover:text-primary-900"
                          title="Düzenle"
                        >
                          <Edit className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => handleDelete(item)}
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

        {/* Server-Side Pagination */}
        {totalPages > 1 && (
          <div className="bg-white px-4 py-3 border-t border-gray-200 flex items-center justify-between sm:px-6">
            <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
              <div>
                <p className="text-sm text-gray-700">
                  Toplam <span className="font-medium">{totalCount}</span> kayıttan{' '}
                  <span className="font-medium">{(page - 1) * pageSize + 1}</span> -{' '}
                  <span className="font-medium">
                    {Math.min(page * pageSize, totalCount)}
                  </span>{' '}
                  arası gösteriliyor
                </p>
              </div>
              <div>
                <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
                  <button
                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                    disabled={page <= 1}
                    className="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                  >
                    <span className="sr-only">Önceki</span>
                    <ChevronLeft className="h-5 w-5" aria-hidden="true" />
                  </button>
                  <button
                    onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                    disabled={page >= totalPages}
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

      <WarehouseForm
        isOpen={isFormOpen}
        mode={formMode}
        initialData={selectedWarehouse}
        onClose={() => setIsFormOpen(false)}
        onSubmit={handleFormSubmit}
        isSubmitting={createMutation.isPending || updateMutation.isPending}
      />

      <ConfirmDialog
        isOpen={isDeleteOpen}
        title="Depoyu Sil"
        message={`"${warehouseToDelete?.name}" isimli depoyu silmek istediğinize emin misiniz? Bu işlem geri alınamaz.`}
        confirmLabel="Sil"
        onCancel={() => setIsDeleteOpen(false)}
        onConfirm={confirmDelete}
        isLoading={deleteMutation.isPending}
        isDestructive={true}
      />
    </div>
  );
};
