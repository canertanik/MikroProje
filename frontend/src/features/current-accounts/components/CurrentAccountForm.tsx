import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { X } from 'lucide-react';
import { CurrentAccountType } from '../types';
import type { CurrentAccountDto } from '../types';

const currentAccountSchema = z.object({
  code: z.string().min(1, 'Cari kodu gereklidir'),
  name: z.string().min(1, 'Ünvan/Ad gereklidir'),
  type: z.nativeEnum(CurrentAccountType, {
    errorMap: () => ({ message: 'Geçerli bir cari tipi seçiniz' }),
  }),
  phone: z.string().nullable().optional(),
  email: z.union([z.literal(''), z.string().email('Geçerli bir e-posta adresi giriniz')]).nullable().optional(),
  taxNumber: z.string().refine((val) => {
    if (!val) return true;
    return /^\d{10,11}$/.test(val);
  }, 'Vergi No 10 haneli, T.C. Kimlik No 11 haneli olmalıdır').nullable().optional(),
});

export type CurrentAccountFormValues = z.infer<typeof currentAccountSchema>;

interface CurrentAccountFormProps {
  isOpen: boolean;
  mode: 'create' | 'update';
  initialData?: CurrentAccountDto | null;
  onSubmit: (data: CurrentAccountFormValues) => void;
  onClose: () => void;
  isSubmitting?: boolean;
}

export const CurrentAccountForm: React.FC<CurrentAccountFormProps> = ({
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
  } = useForm<CurrentAccountFormValues>({
    resolver: zodResolver(currentAccountSchema),
    defaultValues: {
      code: '',
      name: '',
      type: CurrentAccountType.Customer,
      phone: '',
      email: '',
      taxNumber: '',
    },
  });

  useEffect(() => {
    if (isOpen) {
      if (mode === 'update' && initialData) {
        reset({
          code: initialData.code,
          name: initialData.name,
          type: initialData.type,
          phone: initialData.phone || '',
          email: initialData.email || '',
          taxNumber: initialData.taxNumber || '',
        });
      } else {
        reset({
          code: '',
          name: '',
          type: CurrentAccountType.Customer,
          phone: '',
          email: '',
          taxNumber: '',
        });
      }
    }
  }, [isOpen, mode, initialData, reset]);

  if (!isOpen) return null;

  const title = mode === 'create' ? 'Yeni Cari Hesap Ekle' : 'Cari Hesap Düzenle';
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
                Ad / Ünvan *
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
                Cari Kodu *
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
              <label htmlFor="type" className="block text-sm font-medium text-gray-700">
                Cari Tipi *
              </label>
              <select
                id="type"
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.type ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('type', { valueAsNumber: true })}
              >
                <option value={CurrentAccountType.Customer}>Müşteri</option>
                <option value={CurrentAccountType.Supplier}>Tedarikçi</option>
                <option value={CurrentAccountType.Both}>Müşteri & Tedarikçi</option>
              </select>
              {errors.type && <p className="mt-1 text-sm text-red-600">{errors.type.message}</p>}
            </div>

            <div>
              <label htmlFor="phone" className="block text-sm font-medium text-gray-700">
                Telefon
              </label>
              <input
                id="phone"
                type="text"
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.phone ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('phone')}
              />
              {errors.phone && <p className="mt-1 text-sm text-red-600">{errors.phone.message}</p>}
            </div>

            <div>
              <label htmlFor="email" className="block text-sm font-medium text-gray-700">
                E-posta
              </label>
              <input
                id="email"
                type="email"
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.email ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('email')}
              />
              {errors.email && <p className="mt-1 text-sm text-red-600">{errors.email.message}</p>}
            </div>

            <div className="sm:col-span-2">
              <label htmlFor="taxNumber" className="block text-sm font-medium text-gray-700">
                Vergi Numarası / T.C. Kimlik No
              </label>
              <input
                id="taxNumber"
                type="text"
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.taxNumber ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('taxNumber')}
              />
              {errors.taxNumber && <p className="mt-1 text-sm text-red-600">{errors.taxNumber.message}</p>}
            </div>
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
