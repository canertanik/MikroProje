import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Eye, Edit, Trash2, Search, Filter } from 'lucide-react';
import { getPayments, createPayment, updatePayment, deletePayment } from '../features/payments/api';
import { PaymentForm } from '../features/payments/components/PaymentForm';
import type { PaymentFormValues } from '../features/payments/components/PaymentForm';
import { PaymentDetailModal } from '../features/payments/components/PaymentDetailModal';
import { ConfirmDialog } from '../components/common/ConfirmDialog';
import { formatCurrency, formatDateTime } from '../lib/formatters';
import { useAuthStore } from '../stores/useAuthStore';
import { toast } from 'react-hot-toast';
import type { PaymentDto } from '../features/payments/types';
import { PaymentType, PaymentMethod } from '../features/payments/types';

export const Payments = () => {
  const queryClient = useQueryClient();
  const user = useAuthStore((state) => state.user);
  const isAdmin = user?.role === 1;

  // Pagination state
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);

  // Modals state
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [formMode, setFormMode] = useState<'create' | 'update'>('create');
  const [selectedPayment, setSelectedPayment] = useState<PaymentDto | null>(null);

  const [isDetailOpen, setIsDetailOpen] = useState(false);

  const [deleteConfirmData, setDeleteConfirmData] = useState<{ isOpen: boolean; id: number | null }>({
    isOpen: false,
    id: null,
  });

  const [searchTerm, setSearchTerm] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(searchTerm);
      setPage(1);
    }, 500);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  const handleSearch = () => {
    setDebouncedSearch(searchTerm);
    setPage(1);
  };

  // Queries
  const { data, isLoading, isError } = useQuery({
    queryKey: ['payments', page, pageSize, debouncedSearch, startDate, endDate],
    queryFn: () => getPayments({
      pageNumber: page, 
      pageSize, 
      searchTerm: debouncedSearch || undefined,
      startDate: startDate || undefined,
      endDate: endDate || undefined
    }),
  });

  // Mutations
  const createMutation = useMutation({
    mutationFn: createPayment,
    onSuccess: () => {
      toast.success('Tahsilat başarıyla oluşturuldu.');
      queryClient.invalidateQueries({ queryKey: ['payments'] });
      queryClient.invalidateQueries({ queryKey: ['current-accounts'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      setIsFormOpen(false);
    },
    onError: (error: any) => {
      toast.error(error.message || 'Tahsilat oluşturulurken bir hata oluştu');
    }
  });

  const updateMutation = useMutation({
    mutationFn: updatePayment,
    onSuccess: () => {
      toast.success('Tahsilat başarıyla güncellendi.');
      queryClient.invalidateQueries({ queryKey: ['payments'] });
      queryClient.invalidateQueries({ queryKey: ['current-accounts'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      setIsFormOpen(false);
    },
    onError: (error: any) => {
      toast.error(error.message || 'Tahsilat güncellenirken bir hata oluştu');
    }
  });

  const deleteMutation = useMutation({
    mutationFn: deletePayment,
    onSuccess: () => {
      toast.success('Tahsilat başarıyla silindi.');
      queryClient.invalidateQueries({ queryKey: ['payments'] });
      queryClient.invalidateQueries({ queryKey: ['current-accounts'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      setDeleteConfirmData({ isOpen: false, id: null });
    },
    onError: (error: any) => {
      toast.error(error.message || 'Tahsilat silinirken bir hata oluştu');
      setDeleteConfirmData({ isOpen: false, id: null });
    }
  });

  // Handlers
  const handleCreate = () => {
    setFormMode('create');
    setSelectedPayment(null);
    setIsFormOpen(true);
  };

  const handleEdit = (payment: PaymentDto) => {
    setFormMode('update');
    setSelectedPayment(payment);
    setIsFormOpen(true);
  };

  const handleDeleteClick = (id: number) => {
    setDeleteConfirmData({ isOpen: true, id });
  };

  const handleViewDetails = (payment: PaymentDto) => {
    setSelectedPayment(payment);
    setIsDetailOpen(true);
  };

  const handleFormSubmit = (values: PaymentFormValues) => {
    const payload = {
      ...values,
      paymentMethod: values.paymentMethod as PaymentMethod,
      description: values.description || undefined,
      type: PaymentType.Collection // Tahsilat
    };

    if (formMode === 'create') {
      createMutation.mutate(payload);
    } else if (selectedPayment) {
      updateMutation.mutate({ id: selectedPayment.id, command: payload });
    }
  };

  if (isError) {
    return (
      <div className="flex flex-col items-center justify-center h-64">
        <p className="text-red-500 mb-4">Tahsilatlar yüklenirken bir hata oluştu.</p>
        <button 
          onClick={() => queryClient.invalidateQueries({ queryKey: ['payments'] })}
          className="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
        >
          Tekrar Dene
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-bold text-gray-900">Müşteri Tahsilatları</h1>
        {isAdmin && (
          <button
            onClick={handleCreate}
            className="flex items-center gap-2 px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 focus:ring-4 focus:ring-primary-100 transition-colors"
          >
            <Plus className="w-5 h-5" />
            Yeni Tahsilat Ekle
          </button>
        )}
      </div>

      {/* Arama ve Filtreleme */}
      <div className="bg-gray-50 p-4 border border-gray-100 rounded-xl space-y-4">
        <div className="flex items-center gap-2 text-gray-700 mb-2 font-medium">
          <Filter className="w-4 h-4" />
          Filtreler
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-1">Arama</label>
            <div className="relative">
              <input
                type="text"
                placeholder="Tahsilat No, Müşteri, Açıklama..."
                value={searchTerm}
                onChange={(e) => {
                  setSearchTerm(e.target.value);
                  if (e.target.value === '') {
                    setDebouncedSearch('');
                    setPage(1);
                  }
                }}
                onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                className="w-full pl-9 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 outline-none text-sm"
              />
              <Search className="w-4 h-4 text-gray-400 absolute left-3 top-2.5" />
            </div>
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-1">Başlangıç Tarihi</label>
            <input
              type="date"
              value={startDate}
              onChange={(e) => { setStartDate(e.target.value); setPage(1); }}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 outline-none text-sm"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-1">Bitiş Tarihi</label>
            <input
              type="date"
              value={endDate}
              onChange={(e) => { setEndDate(e.target.value); setPage(1); }}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 outline-none text-sm"
            />
          </div>
        </div>
        <div className="flex justify-end pt-2">
          <button
            onClick={handleSearch}
            className="px-4 py-2 bg-gray-900 text-white text-sm font-medium rounded-lg hover:bg-gray-800 transition-colors"
          >
            Ara / Filtrele
          </button>
        </div>
      </div>

      {/* Table Section */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm text-gray-500">
            <thead className="text-xs text-gray-700 uppercase bg-gray-50/50 border-b border-gray-100">
              <tr>
                <th className="px-6 py-4 font-medium">Tahsilat No</th>
                <th className="px-6 py-4 font-medium">Müşteri</th>
                <th className="px-6 py-4 font-medium">Tahsilat Tarihi</th>
                <th className="px-6 py-4 font-medium text-right">Tutar</th>
                <th className="px-6 py-4 font-medium">Ödeme Yöntemi</th>
                <th className="px-6 py-4 font-medium text-right">İşlemler</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {isLoading ? (
                <tr>
                  <td colSpan={6} className="px-6 py-8 text-center text-gray-500">
                    Yükleniyor...
                  </td>
                </tr>
              ) : data?.items.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-6 py-8 text-center text-gray-500">
                    Henüz tahsilat bulunmuyor.
                  </td>
                </tr>
              ) : (
                data?.items.map((payment) => (
                  <tr key={payment.id} className="hover:bg-gray-50/50 transition-colors">
                    <td className="px-6 py-4 font-medium text-gray-900">
                      #{payment.id.toString().padStart(5, '0')}
                    </td>
                    <td className="px-6 py-4 text-gray-900">
                      {payment.currentAccountName}
                    </td>
                    <td className="px-6 py-4">
                      {formatDateTime(payment.paymentDate)}
                    </td>
                    <td className="px-6 py-4 text-right font-medium text-gray-900">
                      {formatCurrency(payment.amount)}
                    </td>
                    <td className="px-6 py-4">
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                        {payment.paymentMethodName || payment.paymentMethod}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => handleViewDetails(payment)}
                          className="p-2 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-lg transition-colors"
                          title="Detaylar"
                        >
                          <Eye className="w-4 h-4" />
                        </button>
                        {isAdmin && (
                          <>
                            <button
                              onClick={() => handleEdit(payment)}
                              className="p-2 text-gray-400 hover:text-amber-600 hover:bg-amber-50 rounded-lg transition-colors"
                              title="Düzenle"
                            >
                              <Edit className="w-4 h-4" />
                            </button>
                            <button
                              onClick={() => handleDeleteClick(payment.id)}
                              className="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                              title="Sil"
                            >
                              <Trash2 className="w-4 h-4" />
                            </button>
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
        
        {/* Pagination */}
        {data && data.totalPages > 1 && (
          <div className="flex items-center justify-between px-6 py-4 border-t border-gray-100">
            <div className="text-sm text-gray-500">
              Toplam <span className="font-medium text-gray-900">{data.totalCount}</span> kayıttan{' '}
              <span className="font-medium text-gray-900">
                {(page - 1) * pageSize + 1}-
                {Math.min(page * pageSize, data.totalCount)}
              </span>{' '}
              arası gösteriliyor
            </div>
            <div className="flex gap-2">
              <button
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-3 py-1 text-sm border rounded hover:bg-gray-50 disabled:opacity-50"
              >
                Önceki
              </button>
              <button
                onClick={() => setPage(p => Math.min(data.totalPages, p + 1))}
                disabled={page === data.totalPages}
                className="px-3 py-1 text-sm border rounded hover:bg-gray-50 disabled:opacity-50"
              >
                Sonraki
              </button>
            </div>
          </div>
        )}
      </div>

      <PaymentForm
        isOpen={isFormOpen}
        mode={formMode}
        initialData={selectedPayment}
        onSubmit={handleFormSubmit}
        onClose={() => setIsFormOpen(false)}
        isSubmitting={createMutation.isPending || updateMutation.isPending}
      />

      <PaymentDetailModal
        isOpen={isDetailOpen}
        payment={selectedPayment}
        onClose={() => setIsDetailOpen(false)}
      />

      <ConfirmDialog
        isOpen={deleteConfirmData.isOpen}
        title="Tahsilatı Sil"
        message="Bu tahsilatı silmek istediğinize emin misiniz? Bu işlem sonucunda müşteri bakiyesi iptal edilen tahsilat kadar geri artırılacaktır."
        confirmLabel="Evet, Sil"
        cancelLabel="İptal"
        onConfirm={() => {
          if (deleteConfirmData.id) {
            deleteMutation.mutate(deleteConfirmData.id);
          }
        }}
        onCancel={() => setDeleteConfirmData({ isOpen: false, id: null })}
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
};
