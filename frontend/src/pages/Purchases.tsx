import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, FileDown, Eye, FileText, XCircle, PackageCheck, Trash2, Search, Filter } from 'lucide-react';
import toast from 'react-hot-toast';

import { getPurchases, createPurchase, exportPurchasesPdf, exportPurchasesExcel, receivePurchase, cancelPurchase, deletePurchase } from '../features/purchases/api';
import { useAuthStore } from '../stores/useAuthStore';
import { PurchaseForm } from '../features/purchases/components/PurchaseForm';
import { PurchaseDetailModal } from '../features/purchases/components/PurchaseDetailModal';
import { ConfirmDialog } from '../components/common/ConfirmDialog';
import { formatCurrency, formatDateTime } from '../lib/formatters';
import { PurchaseStatus, type PurchaseListDto } from '../features/purchases/types';

export const Purchases = () => {
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [detailPurchaseId, setDetailPurchaseId] = useState<number | null>(null);
  
  const [receiveConfirmData, setReceiveConfirmData] = useState<{ isOpen: boolean; id: number | null }>({ isOpen: false, id: null });
  const [cancelConfirmData, setCancelConfirmData] = useState<{ isOpen: boolean; id: number | null }>({ isOpen: false, id: null });
  const [deleteConfirmData, setDeleteConfirmData] = useState<{ isOpen: boolean; id: number | null }>({ isOpen: false, id: null });

  const queryClient = useQueryClient();
  const { user } = useAuthStore();
  const isAdmin = user?.role === 1; // Assuming 1 is Admin

  const [searchTerm, setSearchTerm] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

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

  const { data, isLoading, isError } = useQuery({
    queryKey: ['purchases', page, pageSize, debouncedSearch, startDate, endDate, statusFilter],
    queryFn: () => getPurchases({
      pageNumber: page, 
      pageSize, 
      searchTerm: debouncedSearch || undefined,
      startDate: startDate || undefined,
      endDate: endDate || undefined,
      status: statusFilter ? Number(statusFilter) : undefined
    }),
  });

  const createMutation = useMutation({
    mutationFn: createPurchase,
    onSuccess: () => {
      toast.success('Satın alma kaydı oluşturuldu. Depoya giriş işlemi bekleniyor.');
      queryClient.invalidateQueries({ queryKey: ['purchases'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      setIsFormOpen(false);
    },
    onError: (error: any) => {
      const msg = error.response?.data?.message || error.message || 'Satın alma oluşturulurken bir hata oluştu';
      toast.error(msg);
    },
  });

  const receiveMutation = useMutation({
    mutationFn: receivePurchase,
    onSuccess: () => {
      toast.success('Satın alma depoya başarıyla alındı. Stoklar güncellendi.');
      queryClient.invalidateQueries({ queryKey: ['purchases'] });
      queryClient.invalidateQueries({ queryKey: ['products'] });
      queryClient.invalidateQueries({ queryKey: ['warehouse-stocks'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      setReceiveConfirmData({ isOpen: false, id: null });
    },
    onError: (error: any) => {
      const msg = error.response?.data?.message || error.message || 'Depoya alma işlemi başarısız oldu';
      toast.error(msg);
      setReceiveConfirmData({ isOpen: false, id: null });
    },
  });

  const cancelMutation = useMutation({
    mutationFn: cancelPurchase,
    onSuccess: () => {
      toast.success('Satın alma başarıyla iptal edildi.');
      queryClient.invalidateQueries({ queryKey: ['purchases'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      setCancelConfirmData({ isOpen: false, id: null });
    },
    onError: (error: any) => {
      const msg = error.response?.data?.message || error.message || 'İptal işlemi başarısız oldu';
      toast.error(msg);
      setCancelConfirmData({ isOpen: false, id: null });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: deletePurchase,
    onSuccess: () => {
      toast.success('Satın alma başarıyla silindi.');
      queryClient.invalidateQueries({ queryKey: ['purchases'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      setDeleteConfirmData({ isOpen: false, id: null });
    },
    onError: (error: any) => {
      const msg = error.response?.data?.message || error.message || 'Silme işlemi başarısız oldu';
      toast.error(msg);
      setDeleteConfirmData({ isOpen: false, id: null });
    },
  });

  const handleExportExcel = async () => {
    try {
      const blob = await exportPurchasesExcel();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `satin-almalar-${new Date().toISOString().split('T')[0]}.xlsx`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      toast.success('Excel başarıyla indirildi');
    } catch {
      toast.error('Excel indirme başarısız oldu');
    }
  };

  const handleExportPdf = async () => {
    try {
      const blob = await exportPurchasesPdf();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `satin-almalar-${new Date().toISOString().split('T')[0]}.pdf`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      toast.success('PDF başarıyla indirildi');
    } catch {
      toast.error('PDF indirme başarısız oldu');
    }
  };

  // Find the selected purchase for details
  const selectedPurchase = detailPurchaseId 
    ? data?.data?.items.find((p: PurchaseListDto) => p.id === detailPurchaseId) || null
    : null;

  return (
    <div className="space-y-6">
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div className="flex justify-between items-center mb-6">
          <div className="flex items-center space-x-3">
            <div className="bg-blue-100 p-2 rounded-lg text-blue-600">
              <FileText className="w-6 h-6" />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Satın Almalar</h1>
              <p className="text-sm text-gray-500">Tüm satın alma işlemlerini buradan yönetebilirsiniz.</p>
            </div>
          </div>
          
          <div className="flex space-x-3">
            {isAdmin && (
              <button
                onClick={handleExportExcel}
                className="inline-flex items-center px-4 py-2 bg-green-50 text-green-700 font-medium rounded-lg hover:bg-green-100 transition-colors border border-green-200"
              >
                <FileDown className="w-5 h-5 mr-2" />
                Excel
              </button>
            )}
            <button
              onClick={handleExportPdf}
              className="inline-flex items-center px-4 py-2 bg-red-50 text-red-700 font-medium rounded-lg hover:bg-red-100 transition-colors border border-red-200"
            >
              <FileDown className="w-5 h-5 mr-2" />
              PDF
            </button>
            {isAdmin && (
              <button
                onClick={() => setIsFormOpen(true)}
                className="inline-flex items-center px-4 py-2 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition-colors shadow-sm"
              >
                <Plus className="w-5 h-5 mr-2" />
                Yeni Satın Alma
              </button>
            )}
          </div>
        </div>

        {/* Arama ve Filtreleme */}
        <div className="mb-6 p-4 bg-gray-50 border border-gray-100 rounded-xl space-y-4">
          <div className="flex items-center gap-2 text-gray-700 mb-2 font-medium">
            <Filter className="w-4 h-4" />
            Filtreler
          </div>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1">Arama</label>
              <div className="relative">
                <input
                  type="text"
                  placeholder="Satın Alma No, Tedarikçi..."
                  value={searchTerm}
                  onChange={(e) => {
                    setSearchTerm(e.target.value);
                    if (e.target.value === '') {
                      setDebouncedSearch('');
                      setPage(1);
                    }
                  }}
                  onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                  className="w-full pl-9 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none text-sm"
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
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none text-sm"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1">Bitiş Tarihi</label>
              <input
                type="date"
                value={endDate}
                onChange={(e) => { setEndDate(e.target.value); setPage(1); }}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none text-sm"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1">Durum</label>
              <select
                value={statusFilter}
                onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none text-sm"
              >
                <option value="">Tümü</option>
                <option value={PurchaseStatus.Pending.toString()}>Bekliyor</option>
                <option value={PurchaseStatus.Received.toString()}>Teslim Alındı</option>
                <option value={PurchaseStatus.Cancelled.toString()}>İptal Edildi</option>
              </select>
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

        <div className="border border-gray-200 rounded-lg overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm text-left">
              <thead className="bg-gray-50 text-gray-600 font-medium border-b border-gray-200">
                <tr>
                  <th className="px-6 py-4">SATIN ALMA NO</th>
                  <th className="px-6 py-4">TARİH</th>
                  <th className="px-6 py-4">TEDARİKÇİ</th>
                  <th className="px-6 py-4 text-right">TUTAR</th>
                  <th className="px-6 py-4 text-center">DURUM</th>
                  <th className="px-6 py-4 text-center">İŞLEMLER</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200 bg-white">
                {isLoading ? (
                  <tr>
                    <td colSpan={5} className="px-6 py-12 text-center text-gray-500">
                      Yükleniyor...
                    </td>
                  </tr>
                ) : isError ? (
                  <tr>
                    <td colSpan={5} className="px-6 py-12 text-center text-red-500">
                      Veriler yüklenirken bir hata oluştu.
                    </td>
                  </tr>
                ) : data?.data?.items?.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="px-6 py-12 text-center">
                      <div className="flex flex-col items-center">
                        <FileText className="w-12 h-12 text-gray-300 mb-3" />
                        <h3 className="text-lg font-medium text-gray-900">Satın Alma Bulunamadı</h3>
                        <p className="text-gray-500">Henüz bir satın alma yapılmamış.</p>
                      </div>
                    </td>
                  </tr>
                ) : (
                  data?.data?.items.map((purchase: PurchaseListDto) => (
                    <tr key={purchase.id} className="hover:bg-gray-50 transition-colors">
                      <td className="px-6 py-4">
                        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                          PUR-{purchase.id}
                        </span>
                      </td>
                      <td className="px-6 py-4">
                      {formatDateTime(purchase.purchaseDate)}
                    </td>
                      <td className="px-6 py-4 font-medium text-gray-900">
                        {purchase.currentAccountName}
                      </td>
                      <td className="px-6 py-4 text-right font-medium text-gray-900">
                        {formatCurrency(purchase.grandTotal)}
                      </td>
                      <td className="px-6 py-4 text-center">
                        {purchase.status === PurchaseStatus.Pending && (
                          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800 border border-yellow-200">
                            Bekliyor
                          </span>
                        )}
                        {purchase.status === PurchaseStatus.Received && (
                          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800 border border-green-200">
                            Teslim Alındı
                          </span>
                        )}
                        {purchase.status === PurchaseStatus.Cancelled && (
                          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800 border border-red-200">
                            İptal Edildi
                          </span>
                        )}
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex items-center justify-center space-x-2">
                          <button
                            onClick={() => setDetailPurchaseId(purchase.id)}
                            className="p-1 text-gray-500 hover:text-blue-600 hover:bg-blue-50 rounded transition-colors"
                            title="Detay Görüntüle"
                          >
                            <Eye className="w-5 h-5" />
                          </button>
                          {isAdmin && (
                            <button
                              onClick={() => setDeleteConfirmData({ isOpen: true, id: purchase.id })}
                              className="p-1 text-gray-500 hover:text-red-600 hover:bg-red-50 rounded transition-colors"
                              title="Sil"
                            >
                              <Trash2 className="w-5 h-5" />
                            </button>
                          )}
                          {isAdmin && purchase.status === PurchaseStatus.Pending && (
                            <>
                              <button
                                onClick={() => setReceiveConfirmData({ isOpen: true, id: purchase.id })}
                                className="p-1 text-gray-500 hover:text-green-600 hover:bg-green-50 rounded transition-colors"
                                title="Depoya Giriş Yap"
                              >
                                <PackageCheck className="w-5 h-5" />
                              </button>
                              <button
                                onClick={() => setCancelConfirmData({ isOpen: true, id: purchase.id })}
                                className="p-1 text-gray-500 hover:text-red-600 hover:bg-red-50 rounded transition-colors"
                                title="İptal Et"
                              >
                                <XCircle className="w-5 h-5" />
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

          {data?.data && data.data.totalPages > 1 && (
            <div className="px-6 py-4 border-t border-gray-200 bg-gray-50 flex items-center justify-between">
              <span className="text-sm text-gray-600">
                Toplam <span className="font-medium text-gray-900">{data.data.totalCount}</span> kayıttan{' '}
                <span className="font-medium text-gray-900">
                  {(page - 1) * pageSize + 1}-{Math.min(page * pageSize, data.data.totalCount)}
                </span>{' '}
                arası gösteriliyor
              </span>
              <div className="flex space-x-2">
                <button
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                  className="px-3 py-1 border border-gray-300 rounded text-sm font-medium hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                >
                  Önceki
                </button>
                <button
                  onClick={() => setPage((p) => p + 1)}
                  disabled={page === data.data.totalPages}
                  className="px-3 py-1 border border-gray-300 rounded text-sm font-medium hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                >
                  Sonraki
                </button>
              </div>
            </div>
          )}
        </div>
      </div>

      {isFormOpen && (
        <PurchaseForm
          isOpen={isFormOpen}
          onClose={() => setIsFormOpen(false)}
          onSubmit={(data) => createMutation.mutate(data)}
          isSubmitting={createMutation.isPending}
        />
      )}

      {selectedPurchase && (
        <PurchaseDetailModal
          isOpen={!!detailPurchaseId}
          onClose={() => setDetailPurchaseId(null)}
          purchaseId={detailPurchaseId}
        />
      )}

      <ConfirmDialog
        isOpen={receiveConfirmData.isOpen}
        title="Depoya Giriş Yap"
        message="Bu satın almayı depoya girmek istediğinize emin misiniz? Bu işlem sonucunda ürün stokları artırılacak ve tedarikçi bakiyesi güncellenecektir."
        confirmLabel="Evet, Teslim Al"
        cancelLabel="Vazgeç"
        onConfirm={() => {
          if (receiveConfirmData.id) {
            receiveMutation.mutate(receiveConfirmData.id);
          }
        }}
        onCancel={() => setReceiveConfirmData({ isOpen: false, id: null })}
        isLoading={receiveMutation.isPending}
      />

      <ConfirmDialog
        isOpen={cancelConfirmData.isOpen}
        title="Satın Almayı İptal Et"
        message="Bu satın alma işlemini iptal etmek istediğinize emin misiniz? (İptal işlemi geri alınamaz)"
        confirmLabel="Evet, İptal Et"
        cancelLabel="Vazgeç"
        onConfirm={() => {
          if (cancelConfirmData.id) {
            cancelMutation.mutate(cancelConfirmData.id);
          }
        }}
        onCancel={() => setCancelConfirmData({ isOpen: false, id: null })}
        isLoading={cancelMutation.isPending}
      />

      <ConfirmDialog
        isOpen={deleteConfirmData.isOpen}
        title="Satın Almayı Sil"
        message="Bu satın alma işlemini silmek istediğinize emin misiniz? Teslim alınmış bir satın almayı silmek, stok hareketlerini ve cari hesap bakiyesini geri alacaktır. (Bu işlem geri alınamaz)"
        confirmLabel="Evet, Sil"
        cancelLabel="Vazgeç"
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
