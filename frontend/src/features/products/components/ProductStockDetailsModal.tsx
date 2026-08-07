import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { X, Package, Info, AlertCircle } from 'lucide-react';
import { getProductStocks, getProductById } from '../api';

interface ProductStockDetailsModalProps {
  productId: number | null;
  isOpen: boolean;
  onClose: () => void;
}

export const ProductStockDetailsModal: React.FC<ProductStockDetailsModalProps> = ({
  productId,
  isOpen,
  onClose,
}) => {
  const { data: stocks, isLoading: isStocksLoading, isError: isStocksError } = useQuery({
    queryKey: ['product-stocks', productId],
    queryFn: () => getProductStocks(productId!),
    enabled: isOpen && productId !== null,
  });

  const { data: product, isLoading: isProductLoading } = useQuery({
    queryKey: ['product', productId],
    queryFn: () => getProductById(productId!),
    enabled: isOpen && productId !== null,
  });

  if (!isOpen) return null;

  const isLoading = isStocksLoading || isProductLoading;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-gray-900/50 backdrop-blur-sm">
      <div className="bg-white rounded-xl shadow-2xl w-full max-w-2xl overflow-hidden animate-in fade-in zoom-in-95 duration-200">
        <div className="flex justify-between items-center p-5 border-b border-gray-200">
          <h2 className="text-xl font-bold text-gray-800">Ürün Detayı</h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors p-1 rounded-full hover:bg-gray-100"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="p-6">
          {isLoading ? (
            <div className="flex justify-center items-center py-12">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
            </div>
          ) : product ? (
            <>
              <div className="flex items-start gap-6 mb-8">
                <div className="w-24 h-24 bg-gray-50 rounded-2xl flex items-center justify-center shrink-0 border border-gray-100">
                  <Package className="w-10 h-10 text-gray-300" />
                </div>
                <div className="flex-1">
                  <h3 className="text-xl font-bold text-gray-900 mb-4">{product.name}</h3>
                  
                  <div className="grid grid-cols-[100px_1fr] gap-y-3 items-center">
                    <span className="text-sm font-semibold text-gray-500">Ürün Kodu</span>
                    <span className="text-sm font-bold text-gray-900">{product.code}</span>
                    
                    <span className="text-sm font-semibold text-gray-500">Satış Fiyatı</span>
                    <span className="text-sm font-bold text-gray-900">
                      {new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(product.salePrice)}
                    </span>
                    
                    <span className="text-sm font-semibold text-gray-500">Toplam Stok</span>
                    <div>
                      <span className="inline-flex items-center justify-center px-2.5 py-0.5 rounded-full text-xs font-bold bg-green-100 text-green-700">
                        {product.stockQuantity}
                      </span>
                    </div>
                  </div>
                </div>
              </div>

              <div className="mb-4">
                <h4 className="text-base font-bold text-gray-800">Depolara Göre Stok Dağılımı</h4>
              </div>

              {isStocksError ? (
                <div className="flex flex-col items-center justify-center py-8 text-gray-500 bg-red-50 rounded-lg border border-red-100">
                  <AlertCircle className="w-10 h-10 text-red-400 mb-2" />
                  <p className="text-red-800 font-medium">Stok bilgileri yüklenemedi</p>
                </div>
              ) : stocks && stocks.length > 0 ? (
                <div className="border border-gray-100 rounded-xl overflow-hidden shadow-sm">
                  <table className="min-w-full divide-y divide-gray-100">
                    <thead className="bg-gray-50/50">
                      <tr>
                        <th scope="col" className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                          DEPO KODU
                        </th>
                        <th scope="col" className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                          DEPO ADI
                        </th>
                        <th scope="col" className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                          STOK MİKTARI
                        </th>
                      </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-100">
                      {stocks.map((stock) => (
                        <tr key={stock.warehouseId} className="hover:bg-gray-50/50 transition-colors">
                          <td className="px-6 py-4 whitespace-nowrap text-sm font-bold text-gray-900">
                            {stock.warehouseCode}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                            {stock.warehouseName}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <span className={`inline-flex items-center justify-center px-2.5 py-0.5 rounded-full text-xs font-bold ${
                              stock.quantity > 0 ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
                            }`}>
                              {stock.quantity}
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <div className="text-center py-8 text-gray-500 bg-gray-50 rounded-xl border border-gray-100">
                  Stok kaydı bulunamadı.
                </div>
              )}

              <div className="mt-6 flex items-center gap-2 p-4 bg-blue-50/50 rounded-xl border border-blue-100 text-blue-700">
                <Info className="w-5 h-5 shrink-0" />
                <span className="text-sm font-medium">Stok miktarları anlık olarak güncellenmektedir.</span>
              </div>
            </>
          ) : (
            <div className="text-center py-8 text-gray-500">Ürün bilgileri yüklenemedi.</div>
          )}
        </div>
        
        <div className="p-4 border-t border-gray-100 flex justify-end bg-gray-50/30">
          <button
            onClick={onClose}
            className="px-6 py-2.5 bg-white border border-gray-200 text-gray-700 font-bold rounded-lg hover:bg-gray-50 transition-colors shadow-sm"
          >
            Kapat
          </button>
        </div>
      </div>
    </div>
  );
};
