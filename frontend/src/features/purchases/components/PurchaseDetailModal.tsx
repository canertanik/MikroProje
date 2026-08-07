import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { X, Receipt, Building2, Calendar, FileText, MapPin, AlertCircle } from 'lucide-react';
import { getPurchaseById } from '../api';
import { formatCurrency, formatDate } from '../../../lib/formatters';

interface PurchaseDetailModalProps {
  isOpen: boolean;
  onClose: () => void;
  purchaseId: number | null;
}

export const PurchaseDetailModal: React.FC<PurchaseDetailModalProps> = ({
  isOpen,
  onClose,
  purchaseId,
}) => {
  const { data: response, isLoading, isError } = useQuery({
    queryKey: ['purchase', purchaseId],
    queryFn: () => getPurchaseById(purchaseId!),
    enabled: !!purchaseId && isOpen,
  });

  if (!isOpen) return null;

  const purchase = response?.data;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-gray-900/50 backdrop-blur-sm">
      <div className="bg-white rounded-xl shadow-2xl w-full max-w-4xl overflow-hidden animate-in fade-in zoom-in-95 duration-200 flex flex-col max-h-[90vh]">
        
        <div className="flex justify-between items-start p-6 border-b border-gray-200 shrink-0">
          <div className="flex items-start gap-4">
            <div className="w-10 h-10 bg-blue-50 rounded-lg flex items-center justify-center border border-blue-100 mt-1">
              <Receipt className="w-5 h-5 text-blue-600" />
            </div>
            {purchase && (
              <div>
                <h2 className="text-xl font-bold text-gray-800">Satın Alma Detayı</h2>
                <div className="flex items-center space-x-3 mt-1">
                  <p className="text-sm font-medium text-gray-500">
                    PUR-{purchase.id}
                  </p>
                  <div className="h-4 w-px bg-gray-300"></div>
                  {purchase.status === 1 && (
                    <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-yellow-100 text-yellow-800">
                      Bekliyor
                    </span>
                  )}
                  {purchase.status === 2 && (
                    <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-green-100 text-green-800">
                      Teslim Alındı
                    </span>
                  )}
                  {purchase.status === 3 && (
                    <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-red-100 text-red-800">
                      İptal Edildi
                    </span>
                  )}
                </div>
              </div>
            )}
          </div>
          <button
            onClick={onClose}
            className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="p-6 overflow-y-auto custom-scrollbar">
          {isLoading ? (
            <div className="flex flex-col items-center justify-center py-12">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
              <p className="mt-4 text-sm text-gray-500">Detaylar yükleniyor...</p>
            </div>
          ) : isError ? (
            <div className="flex flex-col items-center justify-center py-12 text-red-500">
              <AlertCircle className="w-12 h-12 mb-3 text-red-200" />
              <p>Detaylar yüklenirken bir hata oluştu.</p>
            </div>
          ) : purchase ? (
            <div className="space-y-8">
              
              {/* Üst Bilgiler */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="bg-gray-50 rounded-xl p-5 border border-gray-100 space-y-4">
                  <div className="flex items-start gap-3">
                    <Building2 className="w-5 h-5 text-gray-400 mt-0.5" />
                    <div>
                      <p className="text-xs font-medium text-gray-500 uppercase tracking-wider">Tedarikçi</p>
                      <p className="text-sm font-medium text-gray-900 mt-1">{purchase.currentAccountName}</p>
                    </div>
                  </div>
                  <div className="flex items-start gap-3">
                    <MapPin className="w-5 h-5 text-gray-400 mt-0.5" />
                    <div>
                      <p className="text-xs font-medium text-gray-500 uppercase tracking-wider">Giriş Deposu</p>
                      <p className="text-sm font-medium text-gray-900 mt-1">{purchase.warehouseName}</p>
                    </div>
                  </div>
                </div>

                <div className="bg-gray-50 rounded-xl p-5 border border-gray-100 space-y-4">
                  <div className="flex items-start gap-3">
                    <Calendar className="w-5 h-5 text-gray-400 mt-0.5" />
                    <div>
                      <p className="text-xs font-medium text-gray-500 uppercase tracking-wider">İşlem Tarihi</p>
                      <p className="text-sm font-medium text-gray-900 mt-1">{formatDate(purchase.purchaseDate)}</p>
                    </div>
                  </div>
                  {purchase.description && (
                    <div className="flex items-start gap-3">
                      <FileText className="w-5 h-5 text-gray-400 mt-0.5" />
                      <div>
                        <p className="text-xs font-medium text-gray-500 uppercase tracking-wider">Açıklama</p>
                        <p className="text-sm font-medium text-gray-900 mt-1">{purchase.description}</p>
                      </div>
                    </div>
                  )}
                </div>
              </div>

              {/* Kalemler */}
              <div>
                <h3 className="text-base font-bold text-gray-900 mb-4 flex items-center gap-2">
                  <Receipt className="w-5 h-5 text-gray-400" />
                  İşlem Kalemleri
                </h3>
                
                <div className="border border-gray-200 rounded-xl overflow-hidden shadow-sm">
                  <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-200">
                      <thead className="bg-gray-50">
                        <tr>
                          <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Ürün</th>
                          <th className="px-4 py-3 text-right text-xs font-semibold text-gray-500 uppercase">Miktar</th>
                          <th className="px-4 py-3 text-right text-xs font-semibold text-gray-500 uppercase">Birim Fiyat</th>
                          <th className="px-4 py-3 text-right text-xs font-semibold text-gray-500 uppercase">KDV %</th>
                          <th className="px-4 py-3 text-right text-xs font-semibold text-gray-500 uppercase">Toplam</th>
                        </tr>
                      </thead>
                      <tbody className="bg-white divide-y divide-gray-200">
                        {purchase.items?.map((item: any) => (
                          <tr key={item.id} className="hover:bg-gray-50/50 transition-colors">
                            <td className="px-4 py-3 text-sm font-medium text-gray-900">{item.productName}</td>
                            <td className="px-4 py-3 text-sm text-gray-700 text-right">{item.quantity}</td>
                            <td className="px-4 py-3 text-sm text-gray-700 text-right">{formatCurrency(item.unitPrice)}</td>
                            <td className="px-4 py-3 text-sm text-gray-700 text-right">{item.vatRate}</td>
                            <td className="px-4 py-3 text-sm font-medium text-gray-900 text-right">{formatCurrency(item.lineTotal)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                  
                  <div className="bg-gray-50 px-6 py-4 border-t border-gray-200">
                    <div className="flex flex-col items-end space-y-2 text-sm">
                      <div className="w-64 flex justify-between text-gray-600">
                        <span>Ara Toplam:</span>
                        <span className="font-medium text-gray-900">{formatCurrency(purchase.subtotal)}</span>
                      </div>
                      <div className="w-64 flex justify-between text-gray-600">
                        <span>Toplam KDV:</span>
                        <span className="font-medium text-gray-900">{formatCurrency(purchase.vatAmount)}</span>
                      </div>
                      <div className="w-64 flex justify-between text-base border-t border-gray-200 pt-2 mt-2">
                        <span className="font-bold text-gray-900">Genel Toplam:</span>
                        <span className="font-bold text-blue-600">{formatCurrency(purchase.grandTotal)}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          ) : null}
        </div>
      </div>
    </div>
  );
};
