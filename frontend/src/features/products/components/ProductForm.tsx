import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { X } from 'lucide-react';
import type { ProductDto } from '../types';

const productSchema = z.object({
  code: z.string().min(1, 'Ürün kodu gereklidir'),
  name: z.string().min(1, 'Ürün adı gereklidir'),
  barcode: z.string().nullable().optional(),
  purchasePrice: z.number().min(0, 'Alış fiyatı 0 veya daha büyük olmalıdır'),
  salePrice: z.number().min(0, 'Satış fiyatı 0 veya daha büyük olmalıdır'),
  vatRate: z.number().min(0, 'KDV oranı 0 veya daha büyük olmalıdır').max(100, 'KDV oranı 100\'den büyük olamaz'),
  criticalStockQuantity: z.number().min(0, 'Kritik stok miktarı 0 veya daha büyük olmalıdır'),
  initialStockQuantity: z.number().min(0, 'Başlangıç stok miktarı 0 veya daha büyük olmalıdır').optional(),
});

export type ProductFormValues = z.infer<typeof productSchema>;

interface ProductFormProps {
  isOpen: boolean;
  mode: 'create' | 'update';
  initialData?: ProductDto | null;
  onSubmit: (data: ProductFormValues) => void;
  onClose: () => void;
  isSubmitting?: boolean;
}

export const ProductForm: React.FC<ProductFormProps> = ({
  isOpen,
  mode,
  initialData,
  onSubmit,
  onClose,
  isSubmitting = false,
}) => {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ProductFormValues>({
    resolver: zodResolver(productSchema),
    defaultValues: {
      code: '',
      name: '',
      barcode: '',
      purchasePrice: 0,
      salePrice: 0,
      vatRate: 20,
      criticalStockQuantity: 10,
      initialStockQuantity: 0,
    },
  });

  useEffect(() => {
    if (isOpen) {
      if (mode === 'update' && initialData) {
        reset({
          code: initialData.code,
          name: initialData.name,
          barcode: initialData.barcode || '',
          purchasePrice: initialData.purchasePrice,
          salePrice: initialData.salePrice,
          vatRate: initialData.vatRate,
          criticalStockQuantity: initialData.criticalStockQuantity,
          initialStockQuantity: 0, // Not used in update
        });
      } else {
        reset({
          code: '',
          name: '',
          barcode: '',
          purchasePrice: 0,
          salePrice: 0,
          vatRate: 20,
          criticalStockQuantity: 10,
          initialStockQuantity: 0,
        });
      }
    }
  }, [isOpen, mode, initialData, reset]);

  if (!isOpen) return null;

  const title = mode === 'create' ? 'Yeni Ürün Ekle' : 'Ürün Düzenle';
  const submitLabel = mode === 'create' ? 'Oluştur' : 'Güncelle';

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-gray-900/50 backdrop-blur-sm">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-2xl overflow-hidden animate-in fade-in zoom-in-95 duration-200">
        <div className="flex justify-between items-center p-6 border-b border-gray-100">
          <h2 className="text-xl font-semibold text-gray-900">{title}</h2>
          <button
            onClick={onClose}
            disabled={isSubmitting}
            className="text-gray-400 hover:text-gray-500 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="p-6 grid grid-cols-1 sm:grid-cols-2 gap-6">
            <div className="sm:col-span-2">
              <label htmlFor="name" className="block text-sm font-medium text-gray-700">
                Ürün Adı *
              </label>
              <input
                id="name"
                type="text"
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.name ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('name')}
              />
              {errors.name && <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>}
            </div>

            <div>
              <label htmlFor="code" className="block text-sm font-medium text-gray-700">
                Ürün Kodu *
              </label>
              <input
                id="code"
                type="text"
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.code ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('code')}
              />
              {errors.code && <p className="mt-1 text-sm text-red-600">{errors.code.message}</p>}
            </div>

            <div>
              <label htmlFor="barcode" className="block text-sm font-medium text-gray-700">
                Barkod
              </label>
              <input
                id="barcode"
                type="text"
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.barcode ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('barcode')}
              />
              {errors.barcode && <p className="mt-1 text-sm text-red-600">{errors.barcode.message}</p>}
            </div>

            <div>
              <label htmlFor="purchasePrice" className="block text-sm font-medium text-gray-700">
                Alış Fiyatı *
              </label>
              <input
                id="purchasePrice"
                type="number"
                step="0.01"
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.purchasePrice ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('purchasePrice', { valueAsNumber: true })}
              />
              {errors.purchasePrice && <p className="mt-1 text-sm text-red-600">{errors.purchasePrice.message}</p>}
            </div>

            <div>
              <label htmlFor="salePrice" className="block text-sm font-medium text-gray-700">
                Satış Fiyatı *
              </label>
              <input
                id="salePrice"
                type="number"
                step="0.01"
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.salePrice ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('salePrice', { valueAsNumber: true })}
              />
              {errors.salePrice && <p className="mt-1 text-sm text-red-600">{errors.salePrice.message}</p>}
            </div>

            <div>
              <label htmlFor="vatRate" className="block text-sm font-medium text-gray-700">
                KDV Oranı (%) *
              </label>
              <input
                id="vatRate"
                type="number"
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.vatRate ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('vatRate', { valueAsNumber: true })}
              />
              {errors.vatRate && <p className="mt-1 text-sm text-red-600">{errors.vatRate.message}</p>}
            </div>

            <div>
              <label htmlFor="criticalStockQuantity" className="block text-sm font-medium text-gray-700">
                Kritik Stok Miktarı *
              </label>
              <input
                id="criticalStockQuantity"
                type="number"
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.criticalStockQuantity ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('criticalStockQuantity', { valueAsNumber: true })}
              />
              {errors.criticalStockQuantity && <p className="mt-1 text-sm text-red-600">{errors.criticalStockQuantity.message}</p>}
            </div>

            {mode === 'create' && (
              <div className="sm:col-span-2">
                <label htmlFor="initialStockQuantity" className="block text-sm font-medium text-gray-700">
                  Başlangıç Stok Miktarı (İsteğe Bağlı)
                </label>
                <input
                  id="initialStockQuantity"
                  type="number"
                  className={`mt-1 block w-full px-3 py-2 border ${
                    errors.initialStockQuantity ? 'border-red-300' : 'border-gray-300'
                  } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                  {...register('initialStockQuantity', { valueAsNumber: true })}
                />
                <p className="mt-1 text-xs text-gray-500">
                  Ürünü oluştururken doğrudan stoğa miktar eklemek isterseniz kullanabilirsiniz.
                </p>
                {errors.initialStockQuantity && <p className="mt-1 text-sm text-red-600">{errors.initialStockQuantity.message}</p>}
              </div>
            )}
          </div>

          <div className="flex justify-end gap-3 p-6 bg-gray-50 border-t border-gray-100">
            <button
              type="button"
              onClick={onClose}
              disabled={isSubmitting}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50"
            >
              İptal
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="px-4 py-2 text-sm font-medium text-white bg-primary-600 border border-transparent rounded-lg hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50"
            >
              {isSubmitting ? 'İşleniyor...' : submitLabel}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
