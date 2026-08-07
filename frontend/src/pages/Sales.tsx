import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Search, FileDown, Eye, AlertCircle, FileText, Ban, Receipt } from 'lucide-react';


import { getSales, cancelSale, exportSalesPdf, exportSalesExcel } from '../features/sales/api';
import { createSale } from '../features/sales/api';
import type { SaleDto } from '../features/sales/types';
import { useAuthStore } from '../stores/useAuthStore';

import { SaleForm } from '../features/sales/components/SaleForm';
import { SaleDetailsModal } from '../features/sales/components/SaleDetailsModal';
import { ConfirmDialog } from '../components/common/ConfirmDialog';

export const Sales = () => {
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');

  const [isFormOpen, setIsFormOpen] = useState(false);
  
  const [detailSaleId, setDetailSaleId] = useState<number | null>(null);
  const [isDetailOpen, setIsDetailOpen] = useState(false);

  const [cancelSaleId, setCancelSaleId] = useState<number | null>(null);

  const queryClient = useQueryClient();
  const { user } = useAuthStore();
  const isAdmin = user?.role === 1; // Assuming 1 is Admin

  // Debounce search
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(search);
      setPage(1); // Reset to first page on search
    }, 500);
    return () => clearTimeout(timer);
  }, [search]);

  const { data, isLoading, isError } = useQuery({
    queryKey: ['sales', page, pageSize, debouncedSearch],
    queryFn: () => getSales(page, pageSize, debouncedSearch),
  });

  const createMutation = useMutation({
    mutationFn: createSale,
    onSuccess: () => {
      alert('Satış başarıyla oluşturuldu');
      queryClient.invalidateQueries({ queryKey: ['sales'] });
      queryClient.invalidateQueries({ queryKey: ['product-stocks'] }); // Invalidate stocks too
      setIsFormOpen(false);
    },
    onError: (error: any) => {
      alert(error.response?.data?.message || error.message || 'Satış oluşturulurken bir hata oluştu');
    },
  });

  const cancelMutation = useMutation({
    mutationFn: cancelSale,
    onSuccess: () => {
      alert('Satış başarıyla iptal edildi');
      queryClient.invalidateQueries({ queryKey: ['sales'] });
      queryClient.invalidateQueries({ queryKey: ['product-stocks'] });
      setCancelSaleId(null);
    },
    onError: (error: any) => {
      alert(error.response?.data?.message || error.message || 'Satış iptal edilirken bir hata oluştu');
      setCancelSaleId(null);
    },
  });

  const handleExportPdf = async () => {
    try {
      const blob = await exportSalesPdf();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `satislar_${new Date().toISOString().split('T')[0]}.pdf`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch {
      alert('PDF dışa aktarılırken bir hata oluştu');
    }
  };

  const handleExportExcel = async () => {
    try {
      const blob = await exportSalesExcel();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `satislar_${new Date().toISOString().split('T')[0]}.xlsx`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch {
      alert('Excel dışa aktarılırken bir hata oluştu');
    }
  };

  const openDetail = (id: number) => {
    setDetailSaleId(id);
    setIsDetailOpen(true);
  };

  const salesItems = data?.items || [];

  return (
    <div className="space-y-6 animate-in fade-in duration-500">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white p-6 rounded-2xl shadow-sm border border-gray-100">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 tracking-tight flex items-center gap-2">
            <Receipt className="w-7 h-7 text-primary-600" />
            Satışlar
          </h1>
          <p className="text-sm text-gray-500 mt-1">Tüm satış işlemlerini buradan yönetebilirsiniz.</p>
        </div>
        <div className="flex items-center gap-3 w-full sm:w-auto">
          {isAdmin && (
            <div className="flex gap-2">
              <button
                onClick={handleExportExcel}
                className="px-4 py-2 bg-green-50 text-green-700 hover:bg-green-100 rounded-xl font-medium transition-colors flex items-center gap-2 border border-green-200"
                title="Excel İndir"
              >
                <FileDown className="w-4 h-4" />
                <span className="hidden sm:inline">Excel</span>
              </button>
              <button
                onClick={handleExportPdf}
                className="px-4 py-2 bg-red-50 text-red-700 hover:bg-red-100 rounded-xl font-medium transition-colors flex items-center gap-2 border border-red-200"
                title="PDF İndir"
              >
                <FileText className="w-4 h-4" />
                <span className="hidden sm:inline">PDF</span>
              </button>
            </div>
          )}
          {isAdmin && (
            <button
              onClick={() => setIsFormOpen(true)}
              className="w-full sm:w-auto px-5 py-2.5 bg-primary-600 text-white rounded-xl hover:bg-primary-700 transition-all font-semibold flex items-center justify-center gap-2 shadow-sm shadow-primary-600/20"
            >
              <Plus className="w-5 h-5" />
              Yeni Satış
            </button>
          )}
        </div>
      </div>

      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="p-4 border-b border-gray-100 flex flex-col sm:flex-row gap-4 justify-between items-center bg-gray-50/50">
          <div className="relative w-full sm:w-96">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5" />
            <input
              type="text"
              placeholder="Satış No, Cari, Depo ara..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full pl-10 pr-4 py-2.5 bg-white border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all"
            />
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-gray-50/80 border-b border-gray-100">
                <th className="p-4 text-xs font-bold text-gray-500 uppercase tracking-wider">Satış No</th>
                <th className="p-4 text-xs font-bold text-gray-500 uppercase tracking-wider">Tarih</th>
                <th className="p-4 text-xs font-bold text-gray-500 uppercase tracking-wider">Cari Hesap</th>
                <th className="p-4 text-xs font-bold text-gray-500 uppercase tracking-wider">Depo</th>
                <th className="p-4 text-xs font-bold text-gray-500 uppercase tracking-wider text-right">Tutar</th>
                <th className="p-4 text-xs font-bold text-gray-500 uppercase tracking-wider text-center">Durum</th>
                <th className="p-4 text-xs font-bold text-gray-500 uppercase tracking-wider text-center">İşlemler</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {isLoading ? (
                <tr>
                  <td colSpan={7} className="p-8 text-center">
                    <div className="flex flex-col items-center justify-center space-y-3">
                      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
                      <span className="text-gray-500 font-medium">Satışlar yükleniyor...</span>
                    </div>
                  </td>
                </tr>
              ) : isError ? (
                <tr>
                  <td colSpan={7} className="p-8 text-center">
                    <div className="flex flex-col items-center justify-center text-red-500 space-y-2">
                      <AlertCircle className="w-8 h-8" />
                      <span className="font-medium">Satışlar yüklenirken bir hata oluştu.</span>
                    </div>
                  </td>
                </tr>
              ) : salesItems.length === 0 ? (
                <tr>
                  <td colSpan={7} className="p-12 text-center">
                    <div className="flex flex-col items-center justify-center text-gray-400 space-y-3">
                      <Receipt className="w-12 h-12 text-gray-300" />
                      <span className="text-lg font-medium text-gray-900">Satış Kaydı Bulunamadı</span>
                      <p className="text-sm">Henüz bir satış yapılmamış veya arama kriterinize uygun sonuç yok.</p>
                    </div>
                  </td>
                </tr>
              ) : (
                salesItems.map((sale: SaleDto) => (
                  <tr key={sale.id} className={`hover:bg-gray-50/80 transition-colors ${sale.isDeleted ? 'opacity-60' : ''}`}>
                    <td className="p-4 align-middle">
                      <div className="font-bold text-gray-900">SAT-{sale.id.toString().padStart(4, '0')}</div>
                    </td>
                    <td className="p-4 align-middle">
                      <div className="text-sm font-medium text-gray-900">
                        {new Date(sale.saleDate).toLocaleDateString('tr-TR')}
                      </div>
                      <div className="text-xs text-gray-500">
                        {new Date(sale.saleDate).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}
                      </div>
                    </td>
                    <td className="p-4 align-middle">
                      <div className="text-sm font-bold text-gray-900">{sale.currentAccountName}</div>
                      <div className="text-xs text-gray-500">{sale.currentAccountCode}</div>
                    </td>
                    <td className="p-4 align-middle">
                      <span className="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium bg-blue-50 text-blue-700">
                        {sale.warehouseName}
                      </span>
                    </td>
                    <td className="p-4 align-middle text-right">
                      <div className="text-sm font-bold text-gray-900">
                        {new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(sale.grandTotal)}
                      </div>
                      <div className="text-xs text-gray-500">
                        KDV: {new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(sale.vatAmount)}
                      </div>
                    </td>
                    <td className="p-4 align-middle text-center">
                      <span
                        className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-bold ${
                          sale.isDeleted
                            ? 'bg-red-100 text-red-800'
                            : 'bg-green-100 text-green-800'
                        }`}
                      >
                        {sale.isDeleted ? 'İptal Edildi' : 'Onaylandı'}
                      </span>
                    </td>
                    <td className="p-4 align-middle text-center">
                      <div className="flex items-center justify-center gap-2">
                        <button
                          onClick={() => openDetail(sale.id)}
                          className="p-1.5 text-gray-400 hover:text-primary-600 transition-colors bg-white rounded-lg border border-gray-200 hover:border-primary-200 shadow-sm"
                          title="Detay Görüntüle"
                        >
                          <Eye className="w-4 h-4" />
                        </button>
                        {isAdmin && !sale.isDeleted && (
                          <button
                            onClick={() => setCancelSaleId(sale.id)}
                            className="p-1.5 text-gray-400 hover:text-red-600 transition-colors bg-white rounded-lg border border-gray-200 hover:border-red-200 shadow-sm"
                            title="İptal Et"
                          >
                            <Ban className="w-4 h-4" />
                          </button>
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
          <div className="p-4 border-t border-gray-100 flex items-center justify-between bg-gray-50/50">
            <span className="text-sm text-gray-500 font-medium">
              Toplam <span className="font-bold text-gray-900">{data.totalCount}</span> kayıttan{' '}
              <span className="font-bold text-gray-900">
                {(page - 1) * pageSize + 1}-{Math.min(page * pageSize, data.totalCount)}
              </span>{' '}
              arası gösteriliyor
            </span>
            <div className="flex gap-2">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-4 py-2 border border-gray-200 rounded-xl text-sm font-bold text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 transition-colors shadow-sm"
              >
                Önceki
              </button>
              <button
                onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
                disabled={page === data.totalPages}
                className="px-4 py-2 border border-gray-200 rounded-xl text-sm font-bold text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 transition-colors shadow-sm"
              >
                Sonraki
              </button>
            </div>
          </div>
        )}
      </div>

      <SaleForm
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        onSubmit={(data) => createMutation.mutate(data)}
        isSubmitting={createMutation.isPending}
      />

      <SaleDetailsModal
        isOpen={isDetailOpen}
        onClose={() => setIsDetailOpen(false)}
        saleId={detailSaleId}
      />

      <ConfirmDialog
        isOpen={cancelSaleId !== null}
        title="Satışı İptal Et"
        message="Bu satışı iptal etmek istediğinize emin misiniz? Bu işlem sonucunda stok miktarları ve cari hesap bakiyesi geri alınacaktır."
        confirmLabel="İptal Et"
        cancelLabel="Vazgeç"
        onConfirm={() => {
          if (cancelSaleId) cancelMutation.mutate(cancelSaleId);
        }}
        onCancel={() => setCancelSaleId(null)}
        isDestructive={true}
      />
    </div>
  );
};
