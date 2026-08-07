import React from 'react';
import { X, Calendar, MapPin, Hash, Package, FileText } from 'lucide-react';
import type { StockTransferDto } from '../types';
import { StockTransferStatus } from '../types';
import { formatDate } from '../../../lib/formatters';

interface StockTransferDetailsModalProps {
  isOpen: boolean;
  transfer: StockTransferDto | null;
  onClose: () => void;
}

export const StockTransferDetailsModal: React.FC<StockTransferDetailsModalProps> = ({
  isOpen,
  transfer,
  onClose,
}) => {
  if (!isOpen || !transfer) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-gray-900/50 backdrop-blur-sm">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-3xl max-h-[90vh] flex flex-col animate-in fade-in zoom-in-95 duration-200">
        <div className="flex justify-between items-center p-6 border-b border-gray-100 shrink-0">
          <h2 className="text-xl font-semibold text-gray-900">Transfer Detayı</h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-500 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="p-6 overflow-y-auto space-y-6">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="flex items-start">
              <Hash className="w-5 h-5 text-gray-400 mt-0.5 mr-3 shrink-0" />
              <div>
                <p className="text-sm font-medium text-gray-500">Transfer No</p>
                <p className="text-base text-gray-900 font-medium">{transfer.transferNumber}</p>
              </div>
            </div>
            
            <div className="flex items-start">
              <Calendar className="w-5 h-5 text-gray-400 mt-0.5 mr-3 shrink-0" />
              <div>
                <p className="text-sm font-medium text-gray-500">Transfer Tarihi</p>
                <p className="text-base text-gray-900">{formatDate(transfer.transferDate)}</p>
              </div>
            </div>

            <div className="flex items-start">
              <MapPin className="w-5 h-5 text-gray-400 mt-0.5 mr-3 shrink-0" />
              <div>
                <p className="text-sm font-medium text-gray-500">Kaynak Depo</p>
                <p className="text-base text-gray-900">{transfer.sourceWarehouseCode} - {transfer.sourceWarehouseName}</p>
              </div>
            </div>

            <div className="flex items-start">
              <MapPin className="w-5 h-5 text-gray-400 mt-0.5 mr-3 shrink-0" />
              <div>
                <p className="text-sm font-medium text-gray-500">Hedef Depo</p>
                <p className="text-base text-gray-900">{transfer.destinationWarehouseCode} - {transfer.destinationWarehouseName}</p>
              </div>
            </div>

            <div className="flex items-start">
              <div className="w-5 h-5 flex items-center justify-center text-gray-400 mt-0.5 mr-3 shrink-0">
                <div className={`w-3 h-3 rounded-full ${
                  transfer.status === StockTransferStatus.Draft ? 'bg-gray-400' :
                  transfer.status === StockTransferStatus.Completed ? 'bg-green-500' : 'bg-red-500'
                }`} />
              </div>
              <div>
                <p className="text-sm font-medium text-gray-500">Durum</p>
                <p className="text-base text-gray-900">
                  {transfer.status === StockTransferStatus.Draft ? 'Taslak' :
                   transfer.status === StockTransferStatus.Completed ? 'Tamamlandı' : 'İptal Edildi'}
                </p>
              </div>
            </div>
          </div>

          {transfer.description && (
            <div className="flex items-start pt-2">
              <FileText className="w-5 h-5 text-gray-400 mt-0.5 mr-3 shrink-0" />
              <div>
                <p className="text-sm font-medium text-gray-500">Açıklama</p>
                <p className="text-base text-gray-900">{transfer.description}</p>
              </div>
            </div>
          )}

          <div className="mt-8">
            <div className="flex items-center mb-4">
              <Package className="w-5 h-5 text-gray-400 mr-2" />
              <h3 className="text-lg font-medium text-gray-900">Transfer Kalemleri</h3>
            </div>
            
            <div className="bg-white border border-gray-200 rounded-lg overflow-hidden">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Ürün Kodu
                    </th>
                    <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Ürün Adı
                    </th>
                    <th scope="col" className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Miktar
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {transfer.items.map((item) => (
                    <tr key={item.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                        {item.productCode}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {item.productName}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900 font-medium text-right">
                        {item.quantity}
                      </td>
                    </tr>
                  ))}
                  {transfer.items.length === 0 && (
                    <tr>
                      <td colSpan={3} className="px-6 py-8 text-center text-sm text-gray-500">
                        Bu transferde herhangi bir kalem bulunmuyor.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div className="flex justify-end p-6 bg-gray-50 border-t border-gray-100 shrink-0">
          <button
            onClick={onClose}
            className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
          >
            Kapat
          </button>
        </div>
      </div>
    </div>
  );
};
