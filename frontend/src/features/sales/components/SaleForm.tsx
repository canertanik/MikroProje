import React, { useEffect, useMemo, useState } from 'react';
import { useForm, useFieldArray, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { X, Plus, Trash2, AlertCircle, Calculator } from 'lucide-react';
import { useQuery } from '@tanstack/react-query';
import Select from 'react-select';

import { getWarehouses } from '../../warehouses/api';
import { getProducts, getProductStocks } from '../../products/api';
import { getCurrentAccounts } from '../../current-accounts/api';
import type { CreateSaleCommand } from '../types';
import { CurrentAccountType } from '../../current-accounts/types';
import { getLocalNow } from '../../../lib/formatters';

const itemSchema = z.object({
  productId: z.number().min(1, 'Ürün seçilmelidir'),
  quantity: z.number({ invalid_type_error: 'Miktar giriniz' }).min(1, 'Miktar sıfırdan büyük olmalıdır'),
  discount: z.number({ invalid_type_error: 'İskonto giriniz' }).min(0, 'İskonto negatif olamaz').max(100, 'İskonto %100\'ü geçemez'),
  unitPrice: z.number({ invalid_type_error: 'Fiyat giriniz' }).min(0.01, 'Fiyat sıfırdan büyük olmalıdır'),
});

const formSchema = z.object({
  currentAccountId: z.number().min(1, 'Cari hesap seçilmelidir'),
  warehouseId: z.number().min(1, 'Çıkış deposu seçilmelidir'),
  saleDate: z.string().min(1, 'Satış tarihi seçilmelidir'),
  description: z.string().max(500, 'Açıklama en fazla 500 karakter olmalıdır').nullable().optional(),
  items: z.array(itemSchema).min(1, 'Satış en az bir kalem içermelidir'),
}).refine((data) => {
  const productIds = data.items.map(i => i.productId);
  const uniqueProductIds = new Set(productIds.filter(id => id > 0));
  return uniqueProductIds.size === productIds.filter(id => id > 0).length;
}, {
  message: 'Aynı ürün birden fazla kez eklenemez. Miktarını artırabilirsiniz.',
  path: ['items'],
});

export type SaleFormValues = z.infer<typeof formSchema>;

interface SaleFormProps {
  isOpen: boolean;
  onSubmit: (data: CreateSaleCommand) => void;
  onClose: () => void;
  isSubmitting?: boolean;
}

export const SaleForm: React.FC<SaleFormProps> = ({
  isOpen,
  onSubmit,
  onClose,
  isSubmitting = false,
}) => {
  const { data: accountsData, isLoading: isAccountsLoading } = useQuery({
    queryKey: ['current-accounts', 1, 1000],
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

  const accounts: any[] = useMemo(() => (accountsData || []).filter(a => a.type === CurrentAccountType.Customer || a.type === CurrentAccountType.Both), [accountsData]);
  const warehouses = warehousesData?.items || [];
  const products = productsData?.items || [];

  const accountOptions = useMemo(() => accounts.map((a: any) => ({ value: a.id, label: `${a.code} - ${a.name}` })), [accounts]);
  const warehouseOptions = useMemo(() => warehouses.map((w: any) => ({ value: w.id, label: `${w.code} - ${w.name}` })), [warehouses]);
  const productOptions = useMemo(() => products.map((p: any) => ({ value: p.id, label: `${p.code} - ${p.name}` })), [products]);

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
    })
  });

  const {
    register,
    control,
    handleSubmit,
    reset,
    watch,
    setValue,
    formState: { errors },
  } = useForm<SaleFormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      currentAccountId: 0,
      warehouseId: 0,
      saleDate: getLocalNow(),
      description: '',
      items: [{ productId: 0, quantity: 1, discount: 0, unitPrice: 0 }],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'items',
  });

  const watchItems = watch('items');
  const watchWarehouseId = watch('warehouseId');

  useEffect(() => {
    if (isOpen) {
      reset({
        currentAccountId: 0,
        warehouseId: 0,
        description: '',
        items: [{ productId: 0, quantity: 1, discount: 0, unitPrice: 0 }],
      });
    }
  }, [isOpen, reset]);

  // Handle Product Selection to auto-fill price
  const handleProductChange = (index: number, productId: number) => {
    setValue(`items.${index}.productId`, productId);
    const product = products.find(p => p.id === productId);
    if (product) {
      setValue(`items.${index}.unitPrice`, product.salePrice, { shouldValidate: true });
    }
  };

  // State for Stocks
  const [stockMap, setStockMap] = useState<Record<number, number>>({});

  // Fetch stocks when warehouse or products change
  useEffect(() => {
    if (watchWarehouseId > 0 && watchItems.length > 0) {
      const fetchStocks = async () => {
        const productIds = watchItems.map(i => i.productId).filter(id => id > 0);
        if (productIds.length === 0) return;
        
        const newStockMap = { ...stockMap };
        for (const pid of productIds) {
          try {
            const stocks = await getProductStocks(pid);
            const wStock = stocks.find(s => s.warehouseId === watchWarehouseId);
            newStockMap[pid] = wStock ? wStock.quantity : 0;
          } catch {
            newStockMap[pid] = 0;
          }
        }
        setStockMap(newStockMap);
      };
      fetchStocks();
    }
  }, [watchWarehouseId, JSON.stringify(watchItems.map(i => i.productId))]);

  // Live Calculations
  const totals = useMemo(() => {
    let subTotal = 0;
    let totalDiscount = 0;
    let totalVat = 0;
    let grandTotal = 0;

    watchItems.forEach(item => {
      if (item.productId > 0 && item.quantity > 0 && item.unitPrice > 0) {
        const product = products.find(p => p.id === item.productId);
        const vatRate = product ? product.vatRate : 0;

        const rawLineTotal = item.quantity * item.unitPrice;
        const lineDiscount = rawLineTotal * ((item.discount || 0) / 100);
        const discountedLineTotal = rawLineTotal - lineDiscount;
        const lineVat = discountedLineTotal * (vatRate / 100);
        const lineGrandTotal = discountedLineTotal + lineVat;

        subTotal += rawLineTotal;
        totalDiscount += lineDiscount;
        totalVat += lineVat;
        grandTotal += lineGrandTotal;
      }
    });

    return { subTotal, totalDiscount, totalVat, grandTotal };
  }, [watchItems, products]);

  const handleFormSubmit = (data: SaleFormValues) => {
    // Convert to Command
    const command: CreateSaleCommand = {
      currentAccountId: data.currentAccountId,
      warehouseId: data.warehouseId,
      saleDate: data.saleDate,
      description: data.description,
      items: data.items.map(i => ({
        productId: i.productId,
        quantity: i.quantity,
        discount: i.discount,
        unitPrice: i.unitPrice
      }))
    };
    onSubmit(command);
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex justify-center items-start p-4 bg-gray-900/50 backdrop-blur-sm overflow-y-auto">
      <div className="bg-white rounded-xl shadow-2xl w-full max-w-6xl my-8 flex flex-col animate-in fade-in zoom-in-95 duration-200">
        <div className="flex justify-between items-center p-6 border-b border-gray-100 shrink-0 bg-gray-50/50">
          <h2 className="text-xl font-bold text-gray-900">Yeni Satış Faturası</h2>
          <button
            onClick={onClose}
            disabled={isSubmitting}
            className="text-gray-400 hover:text-gray-600 transition-colors bg-white p-2 rounded-full border border-gray-200 shadow-sm"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="p-6">
          <form id="sale-form" onSubmit={handleSubmit(handleFormSubmit)} className="space-y-8">
            
            {/* Header Section */}
            <div className="bg-gray-50 p-6 rounded-xl border border-gray-200 grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="lg:col-span-1">
                <label className="block text-sm font-semibold text-gray-700 mb-2">Cari Hesap *</label>
                <Controller
                  name="currentAccountId"
                  control={control}
                  render={({ field }) => (
                    <Select
                      options={accountOptions}
                      isLoading={isAccountsLoading}
                      placeholder="Müşteri Seçiniz"
                      noOptionsMessage={() => "Cari bulunamadı"}
                      loadingMessage={() => "Yükleniyor..."}
                      styles={selectStyles(!!errors.currentAccountId)}
                      value={accountOptions.find((c: any) => c.value === field.value) || null}
                      onChange={(val) => field.onChange(val?.value || 0)}
                      isClearable
                    />
                  )}
                />
                {errors.currentAccountId && (
                  <p className="mt-1.5 text-sm text-red-600 font-medium">{errors.currentAccountId.message}</p>
                )}
              </div>

              <div className="lg:col-span-1">
                <label className="block text-sm font-semibold text-gray-700 mb-2">Çıkış Deposu *</label>
                <Controller
                  name="warehouseId"
                  control={control}
                  render={({ field }) => (
                    <Select
                      options={warehouseOptions}
                      placeholder="Depo Seçiniz"
                      noOptionsMessage={() => "Depo bulunamadı"}
                      loadingMessage={() => "Yükleniyor..."}
                      styles={selectStyles(!!errors.warehouseId)}
                      value={warehouseOptions.find((c: any) => c.value === field.value) || null}
                      onChange={(val) => field.onChange(val?.value || 0)}
                      isClearable
                    />
                  )}
                />
                {errors.warehouseId && (
                  <p className="mt-1.5 text-sm text-red-600 font-medium">{errors.warehouseId.message}</p>
                )}
              </div>

              <div className="lg:col-span-1">
                <label className="block text-sm font-semibold text-gray-700 mb-2">Tarih & Saat *</label>
                <input
                  type="datetime-local"
                  className={`block w-full px-4 py-2.5 border ${
                    errors.saleDate ? 'border-red-300' : 'border-gray-300'
                  } rounded-lg shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm transition-colors`}
                  {...register('saleDate')}
                />
                {errors.saleDate && (
                  <p className="mt-1.5 text-sm text-red-600 font-medium">{errors.saleDate.message}</p>
                )}
              </div>

              <div className="lg:col-span-1">
                <label className="block text-sm font-semibold text-gray-700 mb-2">Açıklama</label>
                <input
                  type="text"
                  className={`block w-full px-4 py-2.5 border ${
                    errors.description ? 'border-red-300' : 'border-gray-300'
                  } rounded-lg shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm transition-colors`}
                  placeholder="Satış notu..."
                  {...register('description')}
                />
                {errors.description && (
                  <p className="mt-1.5 text-sm text-red-600 font-medium">{errors.description.message}</p>
                )}
              </div>
            </div>

            {/* Items Section */}
            <div>
              <div className="flex justify-between items-center mb-4">
                <h3 className="text-lg font-bold text-gray-900 flex items-center gap-2">
                  Satış Kalemleri
                </h3>
                <button
                  type="button"
                  onClick={() => append({ productId: 0, quantity: 1, discount: 0, unitPrice: 0 })}
                  className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-bold rounded-lg text-primary-700 bg-primary-100 hover:bg-primary-200 transition-colors shadow-sm"
                >
                  <Plus className="w-4 h-4 mr-1.5" /> Satır Ekle
                </button>
              </div>

              {errors.items?.root && (
                <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg flex items-center text-red-800 text-sm font-medium shadow-sm">
                  <AlertCircle className="w-5 h-5 mr-2 shrink-0 text-red-500" />
                  {errors.items.root.message}
                </div>
              )}

              <div className="space-y-3">
                {fields.map((field, index) => {
                  const currentProductId = watchItems?.[index]?.productId;
                  const currentQty = watchItems?.[index]?.quantity || 0;
                  const currentStock = currentProductId && watchWarehouseId > 0 ? stockMap[currentProductId] ?? null : null;
                  const stockWarning = currentStock !== null && currentQty > currentStock;
                  
                  const product = products.find(p => p.id === currentProductId);
                  const vatRate = product ? product.vatRate : 0;
                  
                  const uPrice = watchItems?.[index]?.unitPrice || 0;
                  const disc = watchItems?.[index]?.discount || 0;
                  
                  const raw = currentQty * uPrice;
                  const discAmt = raw * (disc / 100);
                  const afterDisc = raw - discAmt;
                  const vatAmt = afterDisc * (vatRate / 100);
                  const lineTot = afterDisc + vatAmt;

                  return (
                    <div key={field.id} className="flex flex-col lg:flex-row gap-4 items-start p-4 bg-white rounded-xl border border-gray-200 shadow-sm hover:border-primary-300 transition-colors relative">
                      
                      <div className="flex-1 w-full lg:w-auto">
                        <label className="block text-xs font-bold text-gray-600 mb-1">Ürün * ({products.length})</label>
                        <Controller
                          name={`items.${index}.productId` as const}
                          control={control}
                          render={({ field }) => (
                            <Select
                              options={productOptions}
                              isLoading={isProductsLoading}
                              placeholder="Ürün Seçiniz"
                              noOptionsMessage={() => "Kayıt bulunamadı!"}
                              loadingMessage={() => "Yükleniyor..."}
                              styles={selectStyles(!!errors.items?.[index]?.productId)}
                              value={productOptions.find((c: any) => c.value === field.value) || null}
                              onChange={(val) => handleProductChange(index, val?.value || 0)}
                              isClearable
                            />
                          )}
                        />
                        {errors.items?.[index]?.productId && (
                          <p className="mt-1 text-xs text-red-600 font-medium">{errors.items[index]?.productId?.message}</p>
                        )}
                        {currentStock !== null && (
                          <p className={`mt-1 text-xs font-bold ${stockWarning ? 'text-red-600' : 'text-green-600'}`}>
                            Stok: {currentStock} Adet
                          </p>
                        )}
                      </div>

                      <div className="w-full lg:w-24 shrink-0">
                        <label className="block text-xs font-bold text-gray-600 mb-1">Miktar *</label>
                        <input
                          type="number"
                          min="1"
                          className={`block w-full px-3 py-2.5 border ${
                            errors.items?.[index]?.quantity || stockWarning ? 'border-red-300 focus:ring-red-500 focus:border-red-500' : 'border-gray-300 focus:ring-primary-500 focus:border-primary-500'
                          } rounded-lg shadow-sm sm:text-sm font-semibold`}
                          {...register(`items.${index}.quantity` as const, { valueAsNumber: true })}
                        />
                        {errors.items?.[index]?.quantity && (
                          <p className="mt-1 text-xs text-red-600 font-medium">{errors.items[index]?.quantity?.message}</p>
                        )}
                      </div>

                      <div className="w-full lg:w-32 shrink-0">
                        <label className="block text-xs font-bold text-gray-600 mb-1">Birim Fiyat (₺) *</label>
                        <input
                          type="number"
                          step="0.01"
                          min="0.01"
                          className={`block w-full px-3 py-2.5 border ${
                            errors.items?.[index]?.unitPrice ? 'border-red-300' : 'border-gray-300'
                          } rounded-lg shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm font-semibold`}
                          {...register(`items.${index}.unitPrice` as const, { valueAsNumber: true })}
                        />
                        {errors.items?.[index]?.unitPrice && (
                          <p className="mt-1 text-xs text-red-600 font-medium">{errors.items[index]?.unitPrice?.message}</p>
                        )}
                      </div>

                      <div className="w-full lg:w-24 shrink-0">
                        <label className="block text-xs font-bold text-gray-600 mb-1">İskonto (%)</label>
                        <input
                          type="number"
                          min="0"
                          max="100"
                          className={`block w-full px-3 py-2.5 border ${
                            errors.items?.[index]?.discount ? 'border-red-300' : 'border-gray-300'
                          } rounded-lg shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm font-semibold`}
                          {...register(`items.${index}.discount` as const, { valueAsNumber: true })}
                        />
                        {errors.items?.[index]?.discount && (
                          <p className="mt-1 text-xs text-red-600 font-medium">{errors.items[index]?.discount?.message}</p>
                        )}
                      </div>

                      <div className="w-full lg:w-16 shrink-0 pt-2 lg:pt-0 text-center lg:text-left">
                        <label className="block text-xs font-bold text-gray-600 mb-1">KDV</label>
                        <div className="py-2.5 text-sm font-bold text-gray-700 bg-gray-50 rounded-lg border border-gray-100 text-center">
                          %{vatRate}
                        </div>
                      </div>

                      <div className="w-full lg:w-32 shrink-0 pt-2 lg:pt-0 text-right">
                        <label className="block text-xs font-bold text-gray-600 mb-1">Satır Toplamı</label>
                        <div className="py-2.5 text-sm font-bold text-gray-900 bg-gray-50 rounded-lg border border-gray-100 px-3">
                          {new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(lineTot)}
                        </div>
                      </div>

                      <div className="pt-2 lg:pt-6">
                        <button
                          type="button"
                          onClick={() => remove(index)}
                          disabled={fields.length === 1}
                          className="p-2.5 text-red-600 hover:text-white disabled:opacity-30 transition-colors rounded-lg bg-red-50 hover:bg-red-500 border border-red-100 disabled:hover:bg-red-50 disabled:hover:text-red-600 w-full lg:w-auto flex justify-center"
                        >
                          <Trash2 className="w-5 h-5" />
                        </button>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>

          </form>
        </div>

        {/* Footer with Totals */}
        <div className="p-6 bg-gray-50 border-t border-gray-200 shrink-0 flex flex-col md:flex-row justify-between items-center gap-6 rounded-b-xl">
          
          <div className="flex gap-4 items-center bg-white p-4 rounded-xl shadow-sm border border-gray-100 flex-wrap justify-center w-full md:w-auto">
            <div className="text-center px-4 border-r border-gray-100">
              <div className="text-xs font-bold text-gray-500 mb-1">ARA TOPLAM</div>
              <div className="text-sm font-bold text-gray-900">{new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(totals.subTotal)}</div>
            </div>
            <div className="text-center px-4 border-r border-gray-100">
              <div className="text-xs font-bold text-gray-500 mb-1">İNDİRİM</div>
              <div className="text-sm font-bold text-red-600">-{new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(totals.totalDiscount)}</div>
            </div>
            <div className="text-center px-4 border-r border-gray-100">
              <div className="text-xs font-bold text-gray-500 mb-1">KDV</div>
              <div className="text-sm font-bold text-gray-900">{new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(totals.totalVat)}</div>
            </div>
            <div className="text-center px-4">
              <div className="text-xs font-bold text-gray-500 mb-1">GENEL TOPLAM</div>
              <div className="text-lg font-black text-primary-700">{new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(totals.grandTotal)}</div>
            </div>
          </div>

          <div className="flex gap-3 w-full md:w-auto justify-end">
            <button
              type="button"
              onClick={onClose}
              disabled={isSubmitting}
              className="px-6 py-3 text-sm font-bold text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 shadow-sm transition-colors"
            >
              İptal
            </button>
            <button
              type="submit"
              form="sale-form"
              disabled={isSubmitting}
              className="px-6 py-3 text-sm font-bold text-white bg-primary-600 border border-transparent rounded-lg hover:bg-primary-700 shadow-sm transition-colors flex items-center gap-2"
            >
              <Calculator className="w-5 h-5" />
              {isSubmitting ? 'Kaydediliyor...' : 'Satışı Kaydet'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
