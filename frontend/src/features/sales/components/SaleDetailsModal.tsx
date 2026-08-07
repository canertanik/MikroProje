import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { X, Receipt, MapPin, Building2, Calendar, FileText, AlertCircle } from 'lucide-react';
import { getSaleById } from '../api';

interface SaleDetailsModalProps {
  saleId: number | null;
  isOpen: boolean;
  onClose: () => void;
}

export const SaleDetailsModal: React.FC<SaleDetailsModalProps> = ({
  saleId,
  isOpen,
  onClose,
}) => {
  const { data: sale, isLoading, isError } = useQuery({
    queryKey: ['sale', saleId],
    queryFn: () => getSaleById(saleId!),
    enabled: isOpen && saleId !== null,
  });

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-gray-900/50 backdrop-blur-sm">
      <div className="bg-white rounded-xl shadow-2xl w-full max-w-4xl overflow-hidden animate-in fade-in zoom-in-95 duration-200 flex flex-col max-h-[90vh]">
        
        <div className="flex justify-between items-center p-5 border-b border-gray-200 shrink-0">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-primary-50 rounded-lg flex items-center justify-center border border-primary-100">
              <Receipt className="w-5 h-5 text-primary-600" />
            </div>
            <div>
              <h2 className="text-xl font-bold text-gray-800">Satış Detayı</h2>
              {sale && <p className="text-sm font-medium text-gray-500 mt-0.5">SAT-{sale.id.toString().padStart(4, '0')}</p>}
            </div>
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors p-2 rounded-full hover:bg-gray-100"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="p-6 overflow-y-auto">
          {isLoading ? (
            <div className="flex justify-center items-center py-12">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
            </div>
          ) : isError ? (
            <div className="flex flex-col items-center justify-center py-12 text-gray-500 bg-red-50 rounded-lg border border-red-100">
              <AlertCircle className="w-12 h-12 text-red-400 mb-3" />
              <p className="text-red-800 font-medium">Satış bilgileri yüklenemedi</p>
            </div>
          ) : sale ? (
            <div className="space-y-6">
              {sale.isDeleted && (
                <div className="bg-red-50 text-red-700 p-4 rounded-lg border border-red-200 flex items-center gap-3">
                  <AlertCircle className="w-5 h-5" />
                  <span className="font-bold">Bu satış iptal edilmiştir.</span>
                </div>
              )}

              {/* Top Info Grid */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="bg-gray-50 p-4 rounded-xl border border-gray-100">
                  <div className="flex items-center gap-2 mb-3">
                    <Building2 className="w-4 h-4 text-gray-400" />
                    <span className="text-sm font-semibold text-gray-500 uppercase tracking-wider">Cari Hesap</span>
                  </div>
                  <div className="font-bold text-gray-900">{sale.currentAccountName}</div>
                  <div className="text-sm text-gray-500 mt-1">Kodu: {sale.currentAccountCode}</div>
                </div>
                
                <div className="bg-gray-50 p-4 rounded-xl border border-gray-100">
                  <div className="flex items-center gap-2 mb-3">
                    <MapPin className="w-4 h-4 text-gray-400" />
                    <span className="text-sm font-semibold text-gray-500 uppercase tracking-wider">Çıkış Deposu</span>
                  </div>
                  <div className="font-bold text-gray-900">{sale.warehouseName}</div>
                </div>

                <div className="bg-gray-50 p-4 rounded-xl border border-gray-100">
                  <div className="flex items-center gap-2 mb-3">
                    <Calendar className="w-4 h-4 text-gray-400" />
                    <span className="text-sm font-semibold text-gray-500 uppercase tracking-wider">Tarih Bilgileri</span>
                  </div>
                  <div className="font-bold text-gray-900">
                    {new Date(sale.saleDate).toLocaleDateString('tr-TR', { day: '2-digit', month: 'long', year: 'numeric' })}
                  </div>
                  <div className="text-sm text-gray-500 mt-1">
                    Saat: {new Date(sale.saleDate).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}
                  </div>
                </div>

                <div className="bg-gray-50 p-4 rounded-xl border border-gray-100">
                  <div className="flex items-center gap-2 mb-3">
                    <FileText className="w-4 h-4 text-gray-400" />
                    <span className="text-sm font-semibold text-gray-500 uppercase tracking-wider">Açıklama</span>
                  </div>
                  <div className="text-sm text-gray-700 whitespace-pre-wrap">
                    {sale.description || <span className="text-gray-400 italic">Açıklama girilmemiş</span>}
                  </div>
                </div>
              </div>

              {/* Items Table */}
              <div>
                <h3 className="text-lg font-bold text-gray-900 mb-4">Satış Kalemleri</h3>
                <div className="border border-gray-200 rounded-xl overflow-hidden shadow-sm">
                  <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-50">
                      <tr>
                        <th className="px-4 py-3 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Ürün</th>
                        <th className="px-4 py-3 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Miktar</th>
                        <th className="px-4 py-3 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Birim Fiyat</th>
                        <th className="px-4 py-3 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">İsk. (%)</th>
                        <th className="px-4 py-3 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">KDV (%)</th>
                        <th className="px-4 py-3 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Tutar</th>
                      </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                      {sale.items.map((item) => (
                        <tr key={item.id} className="hover:bg-gray-50 transition-colors">
                          <td className="px-4 py-3">
                            <div className="font-semibold text-gray-900 text-sm">{item.productName}</div>
                            <div className="text-xs text-gray-500 mt-0.5">{item.productCode}</div>
                          </td>
                          <td className="px-4 py-3 text-right text-sm font-bold text-gray-900">
                            {item.quantity}
                          </td>
                          <td className="px-4 py-3 text-right text-sm text-gray-700">
                            {new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(item.unitPrice)}
                          </td>
                          <td className="px-4 py-3 text-right text-sm text-gray-700">
                            {item.discount > 0 ? (
                              <span className="inline-flex px-2 py-0.5 rounded text-xs font-medium bg-red-50 text-red-700">
                                %{item.discount}
                              </span>
                            ) : '-'}
                          </td>
                          <td className="px-4 py-3 text-right text-sm text-gray-700">
                            %{item.vatRate}
                          </td>
                          <td className="px-4 py-3 text-right text-sm font-bold text-gray-900">
                            {new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(item.lineTotal)}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>

              {/* Totals Box */}
              <div className="flex justify-end mt-6">
                <div className="bg-gray-50 border border-gray-200 rounded-xl p-4 w-full md:w-80">
                  <div className="space-y-3">
                    <div className="flex justify-between text-sm font-medium text-gray-600">
                      <span>Ara Toplam:</span>
                      <span>{new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(sale.totalAmount)}</span>
                    </div>
                    <div className="flex justify-between text-sm font-medium text-gray-600 border-b border-gray-200 pb-3">
                      <span>Toplam KDV:</span>
                      <span>{new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(sale.vatAmount)}</span>
                    </div>
                    <div className="flex justify-between items-center text-lg font-bold text-gray-900 pt-1">
                      <span>Genel Toplam:</span>
                      <span className="text-primary-700">
                        {new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(sale.grandTotal)}
                      </span>
                    </div>
                  </div>
                </div>
              </div>

            </div>
          ) : null}
        </div>
        
        <div className="p-4 border-t border-gray-200 flex justify-end bg-gray-50 shrink-0">
          <button
            onClick={onClose}
            className="px-6 py-2.5 bg-white border border-gray-300 text-gray-700 font-bold rounded-lg hover:bg-gray-100 transition-colors shadow-sm"
          >
            Kapat
          </button>
        </div>

      </div>
    </div>
  );
};
