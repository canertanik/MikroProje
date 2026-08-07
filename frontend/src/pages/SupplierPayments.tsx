import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Eye, ChevronLeft, ChevronRight, Search, Filter } from 'lucide-react';
import { getSupplierPayments, createSupplierPayment, getSupplierPaymentById } from '../features/supplier-payments/api';
import { SupplierPaymentForm } from '../features/supplier-payments/components/SupplierPaymentForm';
import type { SupplierPaymentFormValues } from '../features/supplier-payments/components/SupplierPaymentForm';
import { SupplierPaymentDetailModal } from '../features/supplier-payments/components/SupplierPaymentDetailModal';
import { formatCurrency, formatDateTime } from '../lib/formatters';
import { useAuthStore } from '../stores/useAuthStore';
import { toast } from 'react-hot-toast';
import { PaymentMethod } from '../features/supplier-payments/types';
import type { SupplierPaymentListDto } from '../features/supplier-payments/types';

export const SupplierPayments = () => {
  const queryClient = useQueryClient();
  const { user } = useAuthStore();
  const isAdmin = user?.role === 1;

  const [page, setPage] = useState(1);
  const pageSize = 20;

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [selectedPaymentId, setSelectedPaymentId] = useState<number | null>(null);
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);

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
    queryKey: ['supplier-payments', page, pageSize, debouncedSearch, startDate, endDate],
    queryFn: () => getSupplierPayments({ 
      pageNumber: page, 
      pageSize,
      searchTerm: debouncedSearch || undefined,
      startDate: startDate || undefined,
      endDate: endDate || undefined
    }),
  });

  const { data: detailPayment } = useQuery({
    queryKey: ['supplier-payments', selectedPaymentId],
    queryFn: () => getSupplierPaymentById(selectedPaymentId!),
    enabled: selectedPaymentId !== null && isDetailModalOpen,
  });

  // Mutations
  const createMutation = useMutation({
    mutationFn: createSupplierPayment,
    onSuccess: () => {
      toast.success('Tedarikçi ödemesi başarıyla eklendi');
      setIsFormOpen(false);
      queryClient.invalidateQueries({ queryKey: ['supplier-payments'] });
      queryClient.invalidateQueries({ queryKey: ['current-accounts'] });
      queryClient.invalidateQueries({ queryKey: ['supplier-statements'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Ödeme eklenirken bir hata oluştu');
    },
  });

  // Handlers
  const handleCreate = () => {
    setIsFormOpen(true);
  };

  const handleFormSubmit = (values: SupplierPaymentFormValues) => {
    createMutation.mutate({
      ...values,
      paymentMethod: values.paymentMethod as PaymentMethod,
      description: values.description || undefined,
      referenceNumber: values.referenceNumber || undefined,
    });
  };

  const handleViewDetails = (id: number) => {
    setSelectedPaymentId(id);
    setIsDetailModalOpen(true);
  };

  const getPaymentMethodBadge = (method: PaymentMethod) => {
    switch (method) {
      case PaymentMethod.Cash:
        return <span className="px-2 py-1 text-xs font-medium rounded-full bg-green-100 text-green-800">Nakit</span>;
      case PaymentMethod.BankTransfer:
        return <span className="px-2 py-1 text-xs font-medium rounded-full bg-blue-100 text-blue-800">Havale/EFT</span>;
      case PaymentMethod.CreditCard:
        return <span className="px-2 py-1 text-xs font-medium rounded-full bg-purple-100 text-purple-800">Kredi Kartı</span>;
      default:
        return <span className="px-2 py-1 text-xs font-medium rounded-full bg-gray-100 text-gray-800">Bilinmiyor</span>;
    }
  };

  if (isError) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[400px] text-red-500">
        <p className="text-lg font-medium">Ödemeler yüklenirken bir hata oluştu.</p>
        <button 
          onClick={() => queryClient.invalidateQueries({ queryKey: ['supplier-payments'] })}
          className="mt-4 px-4 py-2 bg-red-100 text-red-700 rounded-lg hover:bg-red-200"
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
        <h1 className="text-2xl font-bold text-gray-900">Tedarikçi Ödemeleri</h1>
        {isAdmin && (
          <button
            onClick={handleCreate}
            className="flex items-center gap-2 px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 focus:ring-4 focus:ring-primary-100 transition-colors"
          >
            <Plus className="w-5 h-5" />
            Yeni Ödeme Ekle
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
                placeholder="Ödeme No, Tedarikçi, Açıklama, Referans..."
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
                <th className="px-6 py-4 font-medium">Ödeme No</th>
                <th className="px-6 py-4 font-medium">Tedarikçi</th>
                <th className="px-6 py-4 font-medium">Ödeme Tarihi</th>
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
                    Henüz tedarikçi ödemesi bulunmuyor.
                  </td>
                </tr>
              ) : (
                data?.items.map((payment: SupplierPaymentListDto) => (
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
                    <td className="px-6 py-4 font-medium text-gray-900 text-right">
                      {formatCurrency(payment.amount)}
                    </td>
                    <td className="px-6 py-4">
                      {getPaymentMethodBadge(payment.paymentMethod)}
                    </td>
                    <td className="px-6 py-4 text-right">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => handleViewDetails(payment.id)}
                          className="p-2 text-gray-400 hover:text-primary-600 transition-colors"
                          title="Detay"
                        >
                          <Eye className="w-4 h-4" />
                        </button>
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
          <div className="flex items-center justify-between px-6 py-4 border-t border-gray-100 bg-gray-50/50">
            <div className="text-sm text-gray-500">
              Toplam <span className="font-medium text-gray-900">{data.totalCount}</span> kayıttan{' '}
              <span className="font-medium text-gray-900">
                {(page - 1) * pageSize + 1}
              </span>
              -
              <span className="font-medium text-gray-900">
                {Math.min(page * pageSize, data.totalCount)}
              </span>{' '}
              arası gösteriliyor
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={!data.hasPreviousPage}
                className="p-2 border border-gray-200 rounded-lg text-gray-500 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                <ChevronLeft className="w-4 h-4" />
              </button>
              <button
                onClick={() => setPage(p => p + 1)}
                disabled={!data.hasNextPage}
                className="p-2 border border-gray-200 rounded-lg text-gray-500 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                <ChevronRight className="w-4 h-4" />
              </button>
            </div>
          </div>
        )}
      </div>

      <SupplierPaymentForm
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        onSubmit={handleFormSubmit}
        isSubmitting={createMutation.isPending}
      />

      <SupplierPaymentDetailModal
        isOpen={isDetailModalOpen}
        onClose={() => {
          setIsDetailModalOpen(false);
          setSelectedPaymentId(null);
        }}
        payment={detailPayment || null}
      />
    </div>
  );
};
