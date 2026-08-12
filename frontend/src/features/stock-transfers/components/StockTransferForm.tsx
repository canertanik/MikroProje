import React, { useEffect, useMemo } from 'react';
import { useForm, useFieldArray, Controller, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { X, Plus, Trash2, AlertCircle } from 'lucide-react';
import { useQuery } from '@tanstack/react-query';
import Select from 'react-select';
import { getWarehouses } from '../../warehouses/api';
import { getProducts } from '../../products/api';

const itemSchema = z.object({
  productId: z.number().min(1, 'Geçerli bir ürün seçilmelidir'),
  quantity: z.number({ invalid_type_error: 'Miktar giriniz' }).min(1, 'Miktar sıfırdan büyük olmalıdır'),
});

const formSchema = z.object({
  sourceWarehouseId: z.number().min(1, 'Kaynak depo seçilmelidir'),
  destinationWarehouseId: z.number().min(1, 'Hedef depo seçilmelidir'),
  description: z.string().max(500, 'Açıklama en fazla 500 karakter olmalıdır').nullable().optional(),
  items: z.array(itemSchema).min(1, 'Transfer en az bir kalem içermelidir'),
}).refine((data) => data.sourceWarehouseId !== data.destinationWarehouseId, {
  message: 'Kaynak ve hedef depo aynı olamaz',
  path: ['destinationWarehouseId'],
}).refine((data) => {
  const productIds = data.items.map(i => i.productId);
  const uniqueProductIds = new Set(productIds.filter(id => id > 0));
  return uniqueProductIds.size === productIds.filter(id => id > 0).length;
}, {
  message: 'Aynı ürün birden fazla kez eklenemez',
  path: ['items'],
});

export type StockTransferFormValues = z.infer<typeof formSchema>;

interface StockTransferFormProps {
  isOpen: boolean;
  onSubmit: (data: StockTransferFormValues) => void;
  onClose: () => void;
  isSubmitting?: boolean;
}

export const StockTransferForm: React.FC<StockTransferFormProps> = ({
  isOpen,
  onSubmit,
  onClose,
  isSubmitting = false,
}) => {
  const { data: warehousesData, isLoading: isWarehousesLoading } = useQuery({
    queryKey: ['warehouses', 1, 100, '', true, 'All'],
    queryFn: () => getWarehouses(1, 100, undefined, true, undefined),
    enabled: isOpen,
  });

  const { data: productsData, isLoading: isProductsLoading } = useQuery({
    queryKey: ['products', 1, 100, ''],
    queryFn: () => getProducts(1, 100),
    enabled: isOpen,
  });


  const warehouseOptions = useMemo(() => {
    return (warehousesData?.items || []).map(w => ({ value: w.id, label: `${w.code} - ${w.name}` }));
  }, [warehousesData]);

  const productOptions = useMemo(() => {
    return (productsData?.items || []).map(p => ({ value: p.id, label: `${p.code} - ${p.name}` }));
  }, [productsData]);

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
    menu: (base: any) => ({ ...base, zIndex: 9999 })
  });

  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<StockTransferFormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      sourceWarehouseId: 0,
      destinationWarehouseId: 0,
      description: '',
      items: [{ productId: 0, quantity: 1 }],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'items',
  });

  const watchItems = useWatch({ control, name: 'items' }) || [];

  useEffect(() => {
    if (isOpen) {
      reset({
        sourceWarehouseId: 0,
        destinationWarehouseId: 0,
        description: '',
        items: [{ productId: 0, quantity: 1 }],
      });
    }
  }, [isOpen, reset]);

  const productStocks = useMemo(() => {
    const map = new Map<number, number>();
    const products = productsData?.items || [];
    products.forEach(p => map.set(p.id, p.stockQuantity));
    return map;
  }, [productsData]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex justify-center items-start p-4 bg-gray-900/50 backdrop-blur-sm overflow-y-auto">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-4xl my-8 flex flex-col animate-in fade-in zoom-in-95 duration-200">
        <div className="flex justify-between items-center p-6 border-b border-gray-100 shrink-0">
          <h2 className="text-xl font-semibold text-gray-900">Yeni Stok Transferi</h2>
          <button
            onClick={onClose}
            disabled={isSubmitting}
            className="text-gray-400 hover:text-gray-500 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="p-6">
          <form id="transfer-form" onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Kaynak Depo *</label>
                <Controller
                  name="sourceWarehouseId"
                  control={control}
                  render={({ field }) => (
                    <Select
                      options={warehouseOptions}
                      placeholder="Depo Seçiniz"
                      noOptionsMessage={() => "Depo bulunamadı"}
                      styles={selectStyles(!!errors.sourceWarehouseId)}
                      value={warehouseOptions.find(c => c.value === field.value) || null}
                      onChange={(val) => field.onChange(val?.value || 0)}
                      isClearable
                      menuPosition="fixed"
                      menuPortalTarget={document.body}
                    />
                  )}
                />
                {errors.sourceWarehouseId && (
                  <p className="mt-1 text-sm text-red-600">{errors.sourceWarehouseId.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Hedef Depo *</label>
                <Controller
                  name="destinationWarehouseId"
                  control={control}
                  render={({ field }) => (
                    <Select
                      options={warehouseOptions}
                      placeholder="Depo Seçiniz"
                      noOptionsMessage={() => "Depo bulunamadı"}
                      styles={selectStyles(!!errors.destinationWarehouseId)}
                      value={warehouseOptions.find(c => c.value === field.value) || null}
                      onChange={(val) => field.onChange(val?.value || 0)}
                      isClearable
                      menuPosition="fixed"
                      menuPortalTarget={document.body}
                    />
                  )}
                />
                {errors.destinationWarehouseId && (
                  <p className="mt-1 text-sm text-red-600">{errors.destinationWarehouseId.message}</p>
                )}
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Açıklama</label>
              <textarea
                rows={2}
                className={`mt-1 block w-full px-3 py-2 border ${
                  errors.description ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                {...register('description')}
              />
              {errors.description && (
                <p className="mt-1 text-sm text-red-600">{errors.description.message}</p>
              )}
            </div>

            <div className="pt-4 border-t border-gray-200">
              <div className="flex justify-between items-center mb-4">
                <h3 className="text-lg font-medium text-gray-900">Transfer Kalemleri *</h3>
                <button
                  type="button"
                  onClick={() => append({ productId: 0, quantity: 1 })}
                  className="inline-flex items-center px-3 py-1.5 border border-transparent text-xs font-medium rounded-md text-primary-700 bg-primary-100 hover:bg-primary-200"
                >
                  <Plus className="w-4 h-4 mr-1" /> Satır Ekle
                </button>
              </div>

              {errors.items?.root && (
                <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-md flex items-center text-red-800 text-sm">
                  <AlertCircle className="w-5 h-5 mr-2 shrink-0" />
                  {errors.items.root.message}
                </div>
              )}

              <div className="space-y-4">
                {fields.map((field, index) => {
                  const currentProductId = watchItems?.[index]?.productId;
                  const currentQty = watchItems?.[index]?.quantity || 0;
                  const currentStock = currentProductId ? productStocks.get(currentProductId) || 0 : null;
                  const stockWarning = currentStock !== null && currentQty > currentStock;

                  return (
                    <div key={field.id} className="flex gap-4 items-start p-4 bg-gray-50 rounded-lg border border-gray-200">
                      <div className="flex-1">
                        <label className="block text-xs font-medium text-gray-700 mb-1">Ürün</label>
                        <Controller
                          name={`items.${index}.productId` as const}
                          control={control}
                          render={({ field }) => (
                            <Select
                              options={productOptions}
                              placeholder="Ürün Seçiniz"
                              noOptionsMessage={() => "Ürün bulunamadı"}
                              styles={selectStyles(!!errors.items?.[index]?.productId)}
                              value={productOptions.find(c => c.value === field.value) || null}
                              onChange={(val) => field.onChange(val?.value || 0)}
                              isClearable
                              menuPosition="fixed"
                              menuPortalTarget={document.body}
                            />
                          )}
                        />
                        {errors.items?.[index]?.productId && (
                          <p className="mt-1 text-xs text-red-600">{errors.items[index]?.productId?.message}</p>
                        )}
                        {currentStock !== null && (
                          <p className={`mt-1 text-xs ${stockWarning ? 'text-red-600 font-medium' : 'text-gray-500'}`}>
                            Mevcut Stok: {currentStock}
                          </p>
                        )}
                      </div>

                      <div className="w-32">
                        <label className="block text-xs font-medium text-gray-700 mb-1">Miktar</label>
                        <input
                          type="number"
                          min="1"
                          className={`block w-full px-3 py-2 border ${
                            errors.items?.[index]?.quantity ? 'border-red-300' : 'border-gray-300'
                          } rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                          {...register(`items.${index}.quantity` as const, { valueAsNumber: true })}
                        />
                        {errors.items?.[index]?.quantity && (
                          <p className="mt-1 text-xs text-red-600">{errors.items[index]?.quantity?.message}</p>
                        )}
                      </div>

                      <div className="pt-6">
                        <button
                          type="button"
                          onClick={() => remove(index)}
                          disabled={fields.length === 1}
                          className="p-2 text-red-600 hover:text-red-800 disabled:opacity-30 disabled:hover:text-red-600 transition-colors rounded-md hover:bg-red-50"
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

        <div className="flex justify-end gap-3 p-6 bg-gray-50 border-t border-gray-100 shrink-0">
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
            form="transfer-form"
            disabled={isSubmitting || isWarehousesLoading || isProductsLoading}
            className="px-4 py-2 text-sm font-medium text-white bg-primary-600 border border-transparent rounded-lg hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50"
          >
            {isSubmitting ? 'İşleniyor...' : 'Transferi Oluştur'}
          </button>
        </div>
      </div>
    </div>
  );
};
