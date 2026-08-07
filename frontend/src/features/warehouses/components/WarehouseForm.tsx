import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { X } from 'lucide-react';
import type { WarehouseDto } from '../types';

const warehouseSchema = z.object({
  code: z.string()
    .min(1, 'Depo kodu gereklidir')
    .max(50, 'Depo kodu en fazla 50 karakter olmalıdır'),
  name: z.string()
    .min(1, 'Depo adı gereklidir')
    .max(100, 'Depo adı en fazla 100 karakter olmalıdır'),
  description: z.string()
    .max(500, 'Açıklama en fazla 500 karakter olmalıdır')
    .nullable()
    .optional(),
  isActive: z.boolean(),
  isDefault: z.boolean(),
});

export type WarehouseFormValues = z.infer<typeof warehouseSchema>;

interface WarehouseFormProps {
  isOpen: boolean;
  mode: 'create' | 'update';
  initialData?: WarehouseDto | null;
  onSubmit: (data: WarehouseFormValues) => void;
  onClose: () => void;
  isSubmitting?: boolean;
}

export const WarehouseForm: React.FC<WarehouseFormProps> = ({
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
  } = useForm<WarehouseFormValues>({
    resolver: zodResolver(warehouseSchema),
    defaultValues: {
      code: '',
      name: '',
      description: '',
      isActive: true,
      isDefault: false,
    },
  });

  useEffect(() => {
    if (isOpen) {
      if (mode === 'update' && initialData) {
        reset({
          code: initialData.code,
          name: initialData.name,
          description: initialData.description || '',
          isActive: initialData.isActive,
          isDefault: initialData.isDefault,
        });
      } else {
        reset({
          code: '',
          name: '',
          description: '',
          isActive: true,
          isDefault: false,
        });
      }
    }
  }, [isOpen, mode, initialData, reset]);

  if (!isOpen) return null;

  const title = mode === 'create' ? 'Yeni Depo Ekle' : 'Depo Düzenle';
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
                Depo Adı *
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

            <div className="sm:col-span-2">
              <label htmlFor="code" className="block text-sm font-medium text-gray-700">
                Depo Kodu *
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

            <div className="sm:col-span-2">
              <label htmlFor="description" className="block text-sm font-medium text-gray-700">
                Açıklama
              </label>
              <textarea
                id="description"
                rows={3}
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.description ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('description')}
              />
              {errors.description && <p className="mt-1 text-sm text-red-600">{errors.description.message}</p>}
            </div>

            <div className="sm:col-span-1">
              <div className="flex items-center mt-4">
                <input
                  id="isActive"
                  type="checkbox"
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                  {...register('isActive')}
                />
                <label htmlFor="isActive" className="ml-2 block text-sm text-gray-900">
                  Aktif
                </label>
              </div>
            </div>

            {mode === 'create' && (
              <div className="sm:col-span-1">
                <div className="flex items-center mt-4">
                  <input
                    id="isDefault"
                    type="checkbox"
                    className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                    {...register('isDefault')}
                  />
                  <label htmlFor="isDefault" className="ml-2 block text-sm text-gray-900">
                    Varsayılan Depo Olarak Ayarla
                  </label>
                </div>
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
