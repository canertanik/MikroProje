import React, { useMemo } from 'react';
import { useForm, useFieldArray, Controller, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { X, Plus, Trash2, Calculator, AlertCircle } from 'lucide-react';
import { useQuery } from '@tanstack/react-query';
import Select from 'react-select';

import { getWarehouses } from '../../warehouses/api';
import { getProducts } from '../../products/api';
import { getCurrentAccounts } from '../../current-accounts/api';
import type { CreatePurchaseCommand } from '../types';
import { CurrentAccountType } from '../../current-accounts/types';
import { formatCurrency, getLocalNow } from '../../../lib/formatters';

const itemSchema = z.object({
  productId: z.number().min(1, 'Ürün seçilmelidir'),
  quantity: z.number({ invalid_type_error: 'Miktar giriniz' }).min(1, 'Miktar sıfırdan büyük olmalıdır'),
  unitPrice: z.number({ invalid_type_error: 'Fiyat giriniz' }).min(0, 'Fiyat sıfırdan küçük olamaz'),
});

const formSchema = z.object({
  currentAccountId: z.number().min(1, 'Tedarikçi seçilmelidir'),
  warehouseId: z.number().min(1, 'Giriş deposu seçilmelidir'),
  purchaseDate: z.string().min(1, 'Tarih girilmelidir'),
  description: z.string().max(500, 'Açıklama en fazla 500 karakter olmalıdır').nullable().optional(),
  items: z.array(itemSchema).min(1, 'Satın alma en az bir kalem içermelidir'),
}).refine((data) => {
  const productIds = data.items.map(i => i.productId);
  const uniqueProductIds = new Set(productIds.filter(id => id > 0));
  return uniqueProductIds.size === productIds.filter(id => id > 0).length;
}, {
  message: 'Aynı ürün birden fazla kez eklenemez. Miktarını artırabilirsiniz.',
  path: ['items'],
});

export type PurchaseFormValues = z.infer<typeof formSchema>;

interface PurchaseFormProps {
  isOpen: boolean;
  onSubmit: (data: CreatePurchaseCommand) => void;
  onClose: () => void;
  isSubmitting?: boolean;
}

export const PurchaseForm: React.FC<PurchaseFormProps> = ({
  isOpen,
  onSubmit,
  onClose,
  isSubmitting = false,
}) => {
  const { data: accountsData, isLoading: isAccountsLoading } = useQuery({
    queryKey: ['current-accounts'],
    queryFn: () => getCurrentAccounts(),
    enabled: isOpen,
  });

  const { data: warehousesData } = useQuery({
    queryKey: ['warehouses', 1, 1000],
    queryFn: () => getWarehouses(1, 1000, undefined, true, undefined),
    enabled: isOpen,
  });

  const { data: productsData, isLoading: isProductsLoading } = useQuery({
    queryKey: ['products', 1, 100],
    queryFn: () => getProducts(1, 100),
    enabled: isOpen,
  });

  // Filter only suppliers and both
  const accountOptions = useMemo(() => (accountsData || []).filter(a => a.type === CurrentAccountType.Supplier || a.type === CurrentAccountType.Both).map((a: any) => ({ value: a.id, label: `${a.code} - ${a.name}` })), [accountsData]);
  const warehouseOptions = useMemo(() => (warehousesData?.items || []).map((w: any) => ({ value: w.id, label: `${w.code} - ${w.name}` })), [warehousesData]);
  const productOptions = useMemo(() => (productsData?.items || []).map((p: any) => ({ value: p.id, label: `${p.code} - ${p.name}` })), [productsData]);

  const selectStyles = (isError: boolean) => ({
    control: (base: any, state: any) => ({
      ...base,
      minHeight: '42px',
      borderRadius: '0.375rem',
      borderColor: isError ? '#fca5a5' : (state.isFocused ? '#0ea5e9' : '#d1d5db'),
      boxShadow: state.isFocused ? (isError ? '0 0 0 1px #fca5a5' : '0 0 0 1px #0ea5e9') : 'none',
      '&:hover': {
        borderColor: state.isFocused ? '#0ea5e9' : '#9ca3af'
      }
    }),
    menuPortal: (base: any) => ({ ...base, zIndex: 9999 }),
  });

  const {
    register,
    control,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<PurchaseFormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      currentAccountId: 0,
      warehouseId: 0,
      purchaseDate: getLocalNow(),
      description: '',
      items: [{ productId: 0, quantity: 1, unitPrice: 0 }],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'items',
  });

  const watchItemsRaw = useWatch({ control, name: 'items' });

  // Dinamik olarak hesaplamalar (Sadece UX için, backend kendi hesaplıyor)
  const totals = useMemo(() => {
    let subtotal = 0;
    let vatAmount = 0;
    const products = productsData?.items || [];
    const items = watchItemsRaw || [];

    items.forEach(item => {
      if (item.productId && item.quantity > 0) {
        const product = products.find(p => p.id === item.productId);
        if (product) {
          const lineSubtotal = (item.unitPrice || 0) * item.quantity;
          const lineVat = lineSubtotal * ((product.vatRate || 0) / 100);
          
          subtotal += lineSubtotal;
          vatAmount += lineVat;
        }
      }
    });

    return {
      subtotal,
      vatAmount,
      grandTotal: subtotal + vatAmount
    };
  }, [watchItemsRaw, productsData]);

  // Ürün seçildiğinde fiyatin otomatik dolması
  const handleProductChange = (index: number, productId: number) => {
    const products = productsData?.items || [];
    const product = products.find(p => p.id === productId);
    if (product) {
      setValue(`items.${index}.unitPrice`, product.purchasePrice || 0);
    }
  };

  const submitForm = (data: PurchaseFormValues) => {
    onSubmit({
      currentAccountId: data.currentAccountId,
      warehouseId: data.warehouseId,
      description: data.description || undefined,
      items: data.items.map(item => ({
        productId: item.productId,
        quantity: item.quantity,
        unitPrice: item.unitPrice,
      })),
    });
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex justify-center items-start p-4 bg-gray-900/50 backdrop-blur-sm overflow-y-auto">
      <div className="bg-white rounded-xl shadow-2xl w-full max-w-4xl my-8 flex flex-col animate-in fade-in zoom-in-95 duration-200">
        <div className="flex justify-between items-center p-6 border-b border-gray-100 shrink-0 bg-gray-50/50">
          <h2 className="text-xl font-bold text-gray-900">Yeni Satın Alma</h2>
          <button
            type="button"
            className="text-gray-400 hover:text-gray-600 transition-colors bg-white p-2 rounded-full border border-gray-200 shadow-sm"
            onClick={onClose}
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="p-6">
          <form id="purchase-form" onSubmit={handleSubmit(submitForm)}>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Tedarikçi *</label>
                  <Controller
                    name="currentAccountId"
                    control={control}
                    render={({ field }) => (
                      <Select
                        {...field}
                        options={accountOptions}
                        value={accountOptions.find(o => o.value === field.value) || null}
                        onChange={(val: any) => field.onChange(val?.value || 0)}
                        placeholder="Tedarikçi seçin..."
                        noOptionsMessage={() => "Tedarikçi bulunamadı"}
                        isLoading={isAccountsLoading}
                        styles={selectStyles(!!errors.currentAccountId)}
                        menuPortalTarget={document.body}
                      />
                    )}
                  />
                  {errors.currentAccountId && (
                    <p className="mt-1 text-sm text-red-600">{errors.currentAccountId.message}</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Giriş Deposu *</label>
                  <Controller
                    name="warehouseId"
                    control={control}
                    render={({ field }) => (
                      <Select
                        {...field}
                        options={warehouseOptions}
                        value={warehouseOptions.find(o => o.value === field.value) || null}
                        onChange={(val: any) => field.onChange(val?.value || 0)}
                        placeholder="Depo seçin..."
                        noOptionsMessage={() => "Aktif depo bulunamadı"}
                        styles={selectStyles(!!errors.warehouseId)}
                        menuPortalTarget={document.body}
                      />
                    )}
                  />
                  {errors.warehouseId && (
                    <p className="mt-1 text-sm text-red-600">{errors.warehouseId.message}</p>
                  )}
                </div>

                <div className="md:col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">Tarih *</label>
                  <input
                    type="datetime-local"
                    {...register('purchaseDate')}
                    className="w-full h-[42px] px-3 py-2 bg-white border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500 sm:text-sm transition-colors"
                  />
                  {errors.purchaseDate && (
                    <p className="mt-1 text-sm text-red-600">{errors.purchaseDate.message}</p>
                  )}
                </div>

                <div className="md:col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">Açıklama</label>
                  <input
                    type="text"
                    {...register('description')}
                    className="w-full h-[42px] px-3 py-2 bg-white border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500 sm:text-sm transition-colors"
                    placeholder="Satın alma ile ilgili notlar..."
                  />
                  {errors.description && (
                    <p className="mt-1 text-sm text-red-600">{errors.description.message}</p>
                  )}
                </div>
              </div>

              <div className="mb-4 flex items-center justify-between">
                <h4 className="text-md font-medium text-gray-900">Satın Alma Kalemleri</h4>
                <div className="flex items-center text-sm text-blue-600 bg-blue-50 px-3 py-1.5 rounded-md">
                  <AlertCircle className="w-4 h-4 mr-1.5" />
                  Kayıt tamamlandığında stoklar anında eklenecektir
                </div>
              </div>

              {errors.items?.root && (
                <div className="mb-4 p-3 bg-red-50 text-red-700 rounded-md text-sm border border-red-200 flex items-center">
                  <AlertCircle className="w-5 h-5 mr-2" />
                  {errors.items.root.message}
                </div>
              )}

              <div className="border border-gray-200 rounded-lg overflow-hidden bg-gray-50">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-100">
                    <tr>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider w-2/5">Ürün</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider w-1/5">Miktar</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider w-1/5">B.Fiyat</th>
                      <th className="px-4 py-3 text-right text-xs font-semibold text-gray-600 uppercase tracking-wider w-1/5">Tutar</th>
                      <th className="px-4 py-3 w-10"></th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-100">
                    {fields.map((field, index) => {
                      const items = watchItemsRaw || [];
                      const productId = items[index]?.productId;
                      const products = productsData?.items || [];
                      const product = products.find(p => p.id === productId);
                      const qty = items[index]?.quantity || 0;
                      const price = items[index]?.unitPrice || 0;
                      
                      const lineTotal = qty * price;
                      
                      return (
                        <tr key={field.id} className="hover:bg-gray-50/50 transition-colors">
                          <td className="px-4 py-3">
                            <Controller
                              name={`items.${index}.productId`}
                              control={control}
                              render={({ field }) => (
                                <Select
                                  {...field}
                                  options={productOptions}
                                  value={productOptions.find(o => o.value === field.value) || null}
                                  onChange={(val: any) => {
                                    field.onChange(val?.value || 0);
                                    handleProductChange(index, val?.value || 0);
                                  }}
                                  placeholder="Ürün seçin..."
                                  isLoading={isProductsLoading}
                                  styles={selectStyles(!!errors.items?.[index]?.productId)}
                                  menuPortalTarget={document.body}
                                />
                              )}
                            />
                            {errors.items?.[index]?.productId && (
                              <p className="mt-1 text-xs text-red-600">{errors.items[index]?.productId?.message}</p>
                            )}
                          </td>
                          <td className="px-4 py-3">
                            <input
                              type="number"
                              {...register(`items.${index}.quantity`, { valueAsNumber: true })}
                              className="w-full px-3 py-2 bg-white border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                              min="1"
                            />
                            {errors.items?.[index]?.quantity && (
                              <p className="mt-1 text-xs text-red-600">{errors.items[index]?.quantity?.message}</p>
                            )}
                          </td>
                          <td className="px-4 py-3">
                            <input
                              type="number"
                              step="0.01"
                              {...register(`items.${index}.unitPrice`, { valueAsNumber: true })}
                              className="w-full px-3 py-2 bg-white border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                            />
                            {errors.items?.[index]?.unitPrice && (
                              <p className="mt-1 text-xs text-red-600">{errors.items[index]?.unitPrice?.message}</p>
                            )}
                          </td>
                          <td className="px-4 py-3 text-right">
                            <div className="text-sm text-gray-900 font-medium">
                              {formatCurrency(lineTotal)}
                            </div>
                            {product && (
                              <div className="text-xs text-gray-500 mt-1">
                                +%{(product.vatRate || 0)} KDV
                              </div>
                            )}
                          </td>
                          <td className="px-4 py-3 text-right">
                            <button
                              type="button"
                              onClick={() => remove(index)}
                              className="text-gray-400 hover:text-red-600 p-1 rounded-md hover:bg-red-50 transition-colors"
                              title="Satırı sil"
                            >
                              <Trash2 className="w-5 h-5" />
                            </button>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
                <div className="bg-gray-50 px-4 py-3 border-t border-gray-200">
                  <button
                    type="button"
                    onClick={() => append({ productId: 0, quantity: 1, unitPrice: 0 })}
                    className="flex items-center text-sm font-medium text-blue-600 hover:text-blue-700"
                  >
                    <Plus className="w-4 h-4 mr-1" />
                    Yeni Satır Ekle
                  </button>
                </div>
              </div>

              {/* Toplamlar Bölümü */}
              <div className="mt-6 flex flex-col items-end">
                <div className="w-full sm:w-80 bg-gray-50 rounded-lg border border-gray-200 p-4 space-y-3">
                  <div className="flex items-center text-blue-600 mb-2 pb-2 border-b border-blue-100">
                    <Calculator className="w-5 h-5 mr-2" />
                    <span className="font-medium">Tahmini Toplamlar</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-500">Ara Toplam:</span>
                    <span className="font-medium text-gray-900">{formatCurrency(totals.subtotal)}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-500">Toplam KDV:</span>
                    <span className="font-medium text-gray-900">{formatCurrency(totals.vatAmount)}</span>
                  </div>
                  <div className="pt-3 border-t border-gray-200 flex justify-between">
                    <span className="text-base font-bold text-gray-900">Genel Toplam:</span>
                    <span className="text-lg font-bold text-blue-600">{formatCurrency(totals.grandTotal)}</span>
                  </div>
                  <div className="text-xs text-gray-400 text-center mt-2 italic">
                    * Kesin toplamlar backend tarafından hesaplanır.
                  </div>
                </div>
              </div>
            </form>
          </div>
          
          <div className="bg-gray-50 px-4 py-4 sm:px-6 sm:flex sm:flex-row-reverse border-t border-gray-100">
            <button
              type="submit"
              form="purchase-form"
              disabled={isSubmitting}
              className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-blue-600 text-base font-medium text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 sm:ml-3 sm:w-auto sm:text-sm disabled:bg-blue-400 disabled:cursor-not-allowed transition-colors"
            >
              {isSubmitting ? 'Kaydediliyor...' : 'Satın Almayı Kaydet'}
            </button>
            <button
              type="button"
              onClick={onClose}
              disabled={isSubmitting}
              className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-200 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm transition-colors"
            >
              İptal
            </button>
          </div>
        </div>
      </div>
  );
};
