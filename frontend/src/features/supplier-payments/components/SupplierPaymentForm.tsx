import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { X } from 'lucide-react';
import { useQuery } from '@tanstack/react-query';
import { getCurrentAccounts } from '../../current-accounts/api';
import { CurrentAccountType } from '../../current-accounts/types';
import { PaymentMethod } from '../types';
import { getLocalNow } from '../../../lib/formatters';

const supplierPaymentSchema = z.object({
  currentAccountId: z.coerce.number().min(1, 'Geçerli bir cari hesap seçilmelidir'),
  amount: z.coerce.number({ invalid_type_error: "Geçerli bir tutar girin" }).positive('Tutar 0\'dan büyük olmalıdır'),
  paymentMethod: z.coerce.number().refine(val => Object.values(PaymentMethod).includes(val as any), { message: 'Geçerli bir ödeme yöntemi seçilmelidir' }),
  referenceNumber: z.string().max(50, 'Referans numarası en fazla 50 karakter olabilir').nullable().optional(),
  description: z.string().max(500, 'Açıklama en fazla 500 karakter olabilir').nullable().optional(),
  paymentDate: z.string().min(1, 'Tarih boş olamaz'),
});

export type SupplierPaymentFormValues = z.infer<typeof supplierPaymentSchema>;

interface SupplierPaymentFormProps {
  isOpen: boolean;
  onSubmit: (data: SupplierPaymentFormValues) => void;
  onClose: () => void;
  isSubmitting?: boolean;
}

export const SupplierPaymentForm: React.FC<SupplierPaymentFormProps> = ({
  isOpen,
  onSubmit,
  onClose,
  isSubmitting = false,
}) => {
  const { data: currentAccounts = [], isLoading: accountsLoading } = useQuery({
    queryKey: ['current-accounts', 'all'],
    queryFn: getCurrentAccounts,
    enabled: isOpen
  });

  const validAccounts = currentAccounts.filter(
    (acc) => acc.type === CurrentAccountType.Supplier || acc.type === CurrentAccountType.Both
  );

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<SupplierPaymentFormValues>({
    resolver: zodResolver(supplierPaymentSchema),
    defaultValues: {
      currentAccountId: 0,
      amount: 0,
      paymentMethod: PaymentMethod.BankTransfer,
      referenceNumber: '',
      description: '',
      paymentDate: getLocalNow(),
    },
  });

  useEffect(() => {
    if (isOpen) {
      reset({
        currentAccountId: 0,
        amount: 0,
        paymentMethod: PaymentMethod.BankTransfer,
        referenceNumber: '',
        description: '',
        paymentDate: getLocalNow(),
      });
    }
  }, [isOpen, reset]);

  const handleLocalSubmit = (data: SupplierPaymentFormValues) => {
    const selectedAccount = validAccounts.find((a) => a.id === data.currentAccountId);
    if (selectedAccount) {
      if (data.amount > selectedAccount.balance) {
        setError('amount', {
          type: 'manual',
          message: `Ödeme tutarı tedarikçinin mevcut borcundan (${selectedAccount.balance.toLocaleString('tr-TR', {
            style: 'currency',
            currency: 'TRY',
          })}) büyük olamaz.`,
        });
        return;
      }
    }

    // Clean up empty strings to undefined
    const payload = {
      ...data,
      referenceNumber: data.referenceNumber || undefined,
      description: data.description || undefined,
    };

    onSubmit(payload);
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-gray-900/50 backdrop-blur-sm">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-2xl overflow-hidden animate-in fade-in zoom-in-95 duration-200">
        <div className="flex justify-between items-center p-6 border-b border-gray-100">
          <h2 className="text-xl font-semibold text-gray-900">Yeni Tedarikçi Ödemesi</h2>
          <button
            onClick={onClose}
            disabled={isSubmitting}
            className="text-gray-400 hover:text-gray-500 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit(handleLocalSubmit)}>
          <div className="p-6 grid grid-cols-1 sm:grid-cols-2 gap-6">
            
            <div className="sm:col-span-2">
              <label htmlFor="currentAccountId" className="block text-sm font-medium text-gray-700">
                Tedarikçi *
              </label>
              <select
                id="currentAccountId"
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.currentAccountId ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('currentAccountId')}
              >
                <option value={0}>Seçiniz...</option>
                {validAccounts.map((account) => (
                  <option key={account.id} value={account.id}>
                    {account.code} - {account.name} (Borç: {account.balance.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })})
                  </option>
                ))}
              </select>
              {accountsLoading && <p className="mt-1 text-xs text-gray-500">Tedarikçiler yükleniyor...</p>}
              {errors.currentAccountId && <p className="mt-1 text-sm text-red-600">{errors.currentAccountId.message}</p>}
            </div>

            <div>
              <label htmlFor="amount" className="block text-sm font-medium text-gray-700">
                Tutar *
              </label>
              <input
                id="amount"
                type="number"
                step="0.01"
                min="0.01"
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.amount ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('amount')}
              />
              {errors.amount && <p className="mt-1 text-sm text-red-600">{errors.amount.message}</p>}
            </div>

            <div>
              <label htmlFor="paymentMethod" className="block text-sm font-medium text-gray-700">
                Ödeme Yöntemi *
              </label>
              <select
                id="paymentMethod"
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.paymentMethod ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('paymentMethod')}
              >
                <option value={PaymentMethod.Cash}>Nakit</option>
                <option value={PaymentMethod.BankTransfer}>Havale/EFT</option>
                <option value={PaymentMethod.CreditCard}>Kredi Kartı</option>
              </select>
              {errors.paymentMethod && <p className="mt-1 text-sm text-red-600">{errors.paymentMethod.message}</p>}
            </div>
            
            <div className="sm:col-span-2">
              <label htmlFor="referenceNumber" className="block text-sm font-medium text-gray-700">
                Referans Numarası
              </label>
              <input
                id="referenceNumber"
                type="text"
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.referenceNumber ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('referenceNumber')}
              />
              {errors.referenceNumber && <p className="mt-1 text-sm text-red-600">{errors.referenceNumber.message}</p>}
            </div>

            <div className="sm:col-span-2">
              <label htmlFor="paymentDate" className="block text-sm font-medium text-gray-700">
                Ödeme Tarihi *
              </label>
              <input
                id="paymentDate"
                type="datetime-local"
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.paymentDate ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('paymentDate')}
              />
              {errors.paymentDate && <p className="mt-1 text-sm text-red-600">{errors.paymentDate.message}</p>}
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
              {isSubmitting ? 'Oluşturuluyor...' : 'Oluştur'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
