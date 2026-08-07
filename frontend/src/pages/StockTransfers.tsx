import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { 
  Plus, 
  Search, 
  Eye, 
  CheckCircle,
  XCircle,
  ChevronLeft,
  ChevronRight,
  Filter
} from 'lucide-react';
import { 
  getStockTransfers, 
  createStockTransfer, 
  completeStockTransfer, 
  cancelStockTransfer,
  getStockTransferById
} from '../features/stock-transfers/api';
import type { StockTransferListDto, StockTransferDto } from '../features/stock-transfers/types';
import { StockTransferStatus } from '../features/stock-transfers/types';
import { StockTransferForm } from '../features/stock-transfers/components/StockTransferForm';
import type { StockTransferFormValues } from '../features/stock-transfers/components/StockTransferForm';
import { StockTransferDetailsModal } from '../features/stock-transfers/components/StockTransferDetailsModal';
import { ConfirmDialog } from '../components/common/ConfirmDialog';
import { useAuthStore } from '../stores/useAuthStore';
import { formatDate } from '../lib/formatters';

export const StockTransfers = () => {
  const queryClient = useQueryClient();
  const { user } = useAuthStore();
  const isAdmin = user?.role === 1;
  
  // State for pagination & filtering
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<'All' | StockTransferStatus>('All');
  
  // Modals state
  const [isFormOpen, setIsFormOpen] = useState(false);
  
  const [isDetailsOpen, setIsDetailsOpen] = useState(false);
  const [selectedTransfer, setSelectedTransfer] = useState<StockTransferDto | null>(null);
  
  const [isCompleteOpen, setIsCompleteOpen] = useState(false);
  const [isCancelOpen, setIsCancelOpen] = useState(false);
  const [actionTransfer, setActionTransfer] = useState<StockTransferListDto | null>(null);

  // Fetching data
  const { data: pagedData, isLoading, isError, error } = useQuery({
    queryKey: ['stock-transfers', page, pageSize, searchTerm, statusFilter],
    queryFn: () => getStockTransfers(
      page, 
      pageSize, 
      searchTerm || undefined, 
      undefined,
      undefined,
      statusFilter === 'All' ? undefined : statusFilter,
      undefined,
      undefined
    ),
  });

  // Derived pagination info
  const items = pagedData?.items || [];
  const totalCount = pagedData?.totalCount || 0;
  const totalPages = pagedData?.totalPages || 0;

  // Mutations
  const createMutation = useMutation({
    mutationFn: createStockTransfer,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['stock-transfers'] });
      setIsFormOpen(false);
    },
    onError: (err: Error) => {
      alert(err.message || 'Transfer oluşturulurken bir hata oluştu.');
    }
  });

  const completeMutation = useMutation({
    mutationFn: completeStockTransfer,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['stock-transfers'] });
      setIsCompleteOpen(false);
    },
    onError: (err: Error) => {
      alert(err.message || 'Transfer tamamlanırken bir hata oluştu.');
      setIsCompleteOpen(false);
    }
  });

  const cancelMutation = useMutation({
    mutationFn: cancelStockTransfer,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['stock-transfers'] });
      setIsCancelOpen(false);
    },
    onError: (err: Error) => {
      alert(err.message || 'Transfer iptal edilirken bir hata oluştu.');
      setIsCancelOpen(false);
    }
  });

  // Handlers
  const handleCreate = () => {
    setIsFormOpen(true);
  };

  const handleViewDetails = async (item: StockTransferListDto) => {
    try {
      const fullData = await getStockTransferById(item.id);
      setSelectedTransfer(fullData);
      setIsDetailsOpen(true);
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Kayıt detayları getirilemedi.');
    }
  };

  const handleComplete = (item: StockTransferListDto) => {
    setActionTransfer(item);
    setIsCompleteOpen(true);
  };

  const handleCancel = (item: StockTransferListDto) => {
    setActionTransfer(item);
    setIsCancelOpen(true);
  };

  const handleFormSubmit = (values: StockTransferFormValues) => {
    createMutation.mutate({
      ...values,
      description: values.description || null,
      transferDate: new Date().toISOString(),
    });
  };

  const confirmComplete = () => {
    if (actionTransfer) {
      completeMutation.mutate({ id: actionTransfer.id, rowVersion: actionTransfer.rowVersion });
    }
  };

  const confirmCancel = () => {
    if (actionTransfer) {
      cancelMutation.mutate({ id: actionTransfer.id, rowVersion: actionTransfer.rowVersion });
    }
  };

  // Status Badge Helper
  const getStatusBadge = (status: StockTransferStatus) => {
    switch (status) {
      case StockTransferStatus.Draft:
        return <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">Taslak</span>;
      case StockTransferStatus.Completed:
        return <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">Tamamlandı</span>;
      case StockTransferStatus.Cancelled:
        return <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">İptal Edildi</span>;
      default:
        return null;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-semibold text-gray-900">Stok Transferleri</h1>
        {isAdmin && (
          <button
            onClick={handleCreate}
            className="inline-flex items-center px-4 py-2 border border-transparent rounded-lg shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
          >
            <Plus className="w-4 h-4 mr-2" />
            Yeni Transfer
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
            placeholder="Transfer no ile ara..."
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
            value={statusFilter}
            onChange={(e) => {
              const val = e.target.value;
              setStatusFilter(val === 'All' ? 'All' : Number(val) as StockTransferStatus);
              setPage(1);
            }}
          >
            <option value="All">Tüm Durumlar</option>
            <option value={StockTransferStatus.Draft}>Taslak</option>
            <option value={StockTransferStatus.Completed}>Tamamlandı</option>
            <option value={StockTransferStatus.Cancelled}>İptal Edildi</option>
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
                    Transfer No
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Kaynak Depo
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Hedef Depo
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Tarih
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Durum
                  </th>
                  <th scope="col" className="relative px-6 py-3">
                    <span className="sr-only">İşlemler</span>
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {items.map((item) => (
                  <tr key={item.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {item.transferNumber}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {item.sourceWarehouseCode}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {item.destinationWarehouseCode}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {formatDate(item.transferDate)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {getStatusBadge(item.status)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-3">
                      <button
                        onClick={() => handleViewDetails(item)}
                        className="text-blue-600 hover:text-blue-900"
                        title="Detayları Gör"
                      >
                        <Eye className="w-5 h-5 inline-block" />
                      </button>
                      
                      {isAdmin && item.status === StockTransferStatus.Draft && (
                        <>
                          <button
                            onClick={() => handleComplete(item)}
                            className="text-green-600 hover:text-green-900"
                            title="Tamamla"
                          >
                            <CheckCircle className="w-5 h-5 inline-block" />
                          </button>
                          <button
                            onClick={() => handleCancel(item)}
                            className="text-red-600 hover:text-red-900"
                            title="İptal Et"
                          >
                            <XCircle className="w-5 h-5 inline-block" />
                          </button>
                        </>
                      )}
                    </td>
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

      <StockTransferForm
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        onSubmit={handleFormSubmit}
        isSubmitting={createMutation.isPending}
      />

      <StockTransferDetailsModal
        isOpen={isDetailsOpen}
        transfer={selectedTransfer}
        onClose={() => setIsDetailsOpen(false)}
      />

      <ConfirmDialog
        isOpen={isCompleteOpen}
        title="Transferi Tamamla"
        message={`"${actionTransfer?.transferNumber}" numaralı stok transferini tamamlamak istediğinize emin misiniz? Bu işlem geri alınamaz.`}
        confirmLabel="Tamamla"
        onCancel={() => setIsCompleteOpen(false)}
        onConfirm={confirmComplete}
        isLoading={completeMutation.isPending}
        isDestructive={false}
      />

      <ConfirmDialog
        isOpen={isCancelOpen}
        title="Transferi İptal Et"
        message={`"${actionTransfer?.transferNumber}" numaralı stok transferini iptal etmek istediğinize emin misiniz? Bu işlem geri alınamaz.`}
        confirmLabel="İptal Et"
        onCancel={() => setIsCancelOpen(false)}
        onConfirm={confirmCancel}
        isLoading={cancelMutation.isPending}
        isDestructive={true}
      />
    </div>
  );
};
