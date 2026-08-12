import { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import Select from 'react-select';
import { Filter, Calendar, FileText, ArrowRightLeft, CreditCard, ShoppingCart, Truck } from 'lucide-react';
import toast from 'react-hot-toast';

import { getCurrentAccounts, getStatement } from '../features/current-accounts/api';
import { CurrentAccountType, DocumentType } from '../features/current-accounts/types';
import { formatCurrency, formatDateTime } from '../lib/formatters';


export const CustomerStatement = () => {
  const [selectedAccountId, setSelectedAccountId] = useState<number | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');

  // Fetch accounts for the dropdown
  const { data: accountsData, isLoading: isAccountsLoading } = useQuery({
    queryKey: ['current-accounts'],
    queryFn: () => getCurrentAccounts(),
  });

  // Filter for Customer and Both
  const customerAccounts = useMemo(() => {
    if (!accountsData) return [];
    return accountsData.filter(
      (a) => a.type === CurrentAccountType.Customer || a.type === CurrentAccountType.Both
    );
  }, [accountsData]);

  const accountOptions = useMemo(() => {
    return customerAccounts.map((a) => ({
      value: a.id,
      label: `${a.code} - ${a.name} (Bakiye: ${formatCurrency(a.balance)})`,
      account: a,
    }));
  }, [customerAccounts]);

  const selectedAccount = useMemo(() => {
    return customerAccounts.find((a) => a.id === selectedAccountId) || null;
  }, [selectedAccountId, customerAccounts]);

  const handleSearch = () => {
    if (startDate && endDate && new Date(startDate) > new Date(endDate)) {
      toast.error('Başlangıç tarihi bitiş tarihinden büyük olamaz.');
      return;
    }
    setPage(1);
  };

  // Fetch statement data
  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ['customer-statement', selectedAccountId, page, pageSize, startDate, endDate],
    queryFn: () =>
      getStatement({
        id: selectedAccountId!,
        pageNumber: page,
        pageSize,
        startDate: startDate || undefined,
        endDate: endDate || undefined,
      }),
    enabled: selectedAccountId !== null,
  });

  const getDocumentTypeName = (type: DocumentType) => {
    switch (type) {
      case DocumentType.Sale:
        return 'Satış';
      case DocumentType.Payment:
        return 'Müşteri Tahsilatı';
      case DocumentType.Purchase:
        return 'Satın Alma';
      case DocumentType.SupplierPayment:
        return 'Tedarikçi Ödemesi';
      default:
        return 'Bilinmeyen İşlem';
    }
  };

  const getDocumentTypeIcon = (type: DocumentType) => {
    switch (type) {
      case DocumentType.Sale:
        return <ShoppingCart className="w-4 h-4 text-green-600" />;
      case DocumentType.Payment:
        return <CreditCard className="w-4 h-4 text-blue-600" />;
      case DocumentType.Purchase:
        return <Truck className="w-4 h-4 text-orange-600" />;
      case DocumentType.SupplierPayment:
        return <ArrowRightLeft className="w-4 h-4 text-purple-600" />;
      default:
        return <FileText className="w-4 h-4 text-gray-500" />;
    }
  };

  const getBadgeColor = (type: DocumentType) => {
    switch (type) {
      case DocumentType.Sale:
        return 'bg-green-100 text-green-800 border-green-200';
      case DocumentType.Payment:
        return 'bg-blue-100 text-blue-800 border-blue-200';
      case DocumentType.Purchase:
        return 'bg-orange-100 text-orange-800 border-orange-200';
      case DocumentType.SupplierPayment:
        return 'bg-purple-100 text-purple-800 border-purple-200';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  const customerBalance = data?.customerBalance ?? 0;
  const supplierBalance = selectedAccount?.type === CurrentAccountType.Both
    ? customerBalance - selectedAccount.balance
    : 0;
  const isBoth = selectedAccount?.type === CurrentAccountType.Both;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-bold text-gray-900">Cari Ekstre</h1>
      </div>

      {/* Arama ve Filtreleme */}
      <div className="bg-white p-6 border border-gray-100 rounded-xl shadow-sm space-y-6">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Müşteri Seçimi</label>
            <Select
              options={accountOptions}
              value={accountOptions.find((o) => o.value === selectedAccountId) || null}
              onChange={(val: any) => {
                setSelectedAccountId(val?.value || null);
                setPage(1);
              }}
              placeholder="Ekstresini görmek istediğiniz müşteriyi seçin..."
              noOptionsMessage={() => 'Müşteri bulunamadı'}
              isLoading={isAccountsLoading}
              className="react-select-container"
              classNamePrefix="react-select"
              isClearable
            />
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Başlangıç Tarihi</label>
              <div className="relative">
                <input
                  type="date"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                  className="w-full pl-10 pr-3 py-2 bg-gray-50 border border-gray-200 rounded-lg focus:bg-white focus:ring-2 focus:ring-primary-500 outline-none text-sm transition-colors"
                />
                <Calendar className="w-4 h-4 text-gray-400 absolute left-3 top-2.5" />
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Bitiş Tarihi</label>
              <div className="relative">
                <input
                  type="date"
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                  className="w-full pl-10 pr-3 py-2 bg-gray-50 border border-gray-200 rounded-lg focus:bg-white focus:ring-2 focus:ring-primary-500 outline-none text-sm transition-colors"
                />
                <Calendar className="w-4 h-4 text-gray-400 absolute left-3 top-2.5" />
              </div>
            </div>
          </div>
        </div>

        <div className="flex justify-end border-t border-gray-100 pt-4">
          <button
            onClick={handleSearch}
            disabled={!selectedAccountId}
            className="flex items-center gap-2 px-6 py-2.5 bg-gray-900 text-white text-sm font-medium rounded-lg hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <Filter className="w-4 h-4" />
            Uygula
          </button>
        </div>
      </div>

      {/* Özet Kartları */}
      {selectedAccount && (
        <div className={`grid grid-cols-1 md:grid-cols-2 gap-4 ${isBoth ? 'xl:grid-cols-5' : 'xl:grid-cols-3'}`}>
          <div className="bg-white p-6 rounded-xl border border-gray-100 shadow-sm flex flex-col justify-center">
            <p className="text-sm font-medium text-gray-500 mb-1">Cari Kodu</p>
            <p className="text-xl font-bold text-gray-900">{selectedAccount.code}</p>
          </div>
          <div className="bg-white p-6 rounded-xl border border-gray-100 shadow-sm flex flex-col justify-center">
            <p className="text-sm font-medium text-gray-500 mb-1">Cari Adı / Ünvanı</p>
            <p className="text-lg font-semibold text-gray-900 truncate" title={selectedAccount.name}>
              {selectedAccount.name}
            </p>
          </div>
          <div className="p-6 rounded-xl border border-emerald-100 bg-emerald-50 shadow-sm flex flex-col justify-center">
            <p className="text-sm font-medium mb-1 text-emerald-700">
              {customerBalance >= 0 ? 'Müşteriden Alacak' : 'Müşteriye Borç'}
            </p>
            <p className="text-2xl font-bold text-emerald-800">{formatCurrency(Math.abs(customerBalance))}</p>
          </div>
          {isBoth && (
            <>
              <div className="p-6 rounded-xl border border-rose-100 bg-rose-50 shadow-sm flex flex-col justify-center">
                <p className="text-sm font-medium mb-1 text-rose-700">
                  {supplierBalance >= 0 ? 'Tedarikçiye Borç' : 'Tedarikçiden Alacak'}
                </p>
                <p className="text-2xl font-bold text-rose-800">{formatCurrency(Math.abs(supplierBalance))}</p>
              </div>
              <div className="p-6 rounded-xl border border-blue-100 bg-blue-50 shadow-sm flex flex-col justify-center">
                <p className="text-sm font-medium mb-1 text-blue-700">Net Bakiye</p>
                <p className="text-2xl font-bold text-blue-800">
                  {formatCurrency(Math.abs(selectedAccount.balance))} {selectedAccount.balance >= 0 ? 'Alacak' : 'Borç'}
                </p>
              </div>
            </>
          )}
        </div>
      )}

      {/* Table Section */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        {/* State Management */}
        {!selectedAccountId ? (
          <div className="py-20 flex flex-col items-center justify-center text-center px-4">
            <div className="w-16 h-16 bg-gray-50 rounded-full flex items-center justify-center mb-4 border border-gray-100">
              <FileText className="w-8 h-8 text-gray-400" />
            </div>
            <h3 className="text-lg font-medium text-gray-900 mb-1">Cari Seçilmedi</h3>
            <p className="text-gray-500 max-w-sm">Ekstreyi görüntülemek için lütfen yukarıdan bir müşteri seçin.</p>
          </div>
        ) : isError ? (
          <div className="py-20 flex flex-col items-center justify-center text-center px-4">
            <p className="text-red-500 mb-4 font-medium">Ekstre yüklenirken bir hata oluştu.</p>
            <button
              onClick={() => refetch()}
              className="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors"
            >
              Tekrar Dene
            </button>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-gray-600">
              <thead className="text-xs text-gray-700 uppercase bg-gray-50/80 border-b border-gray-100">
                <tr>
                  <th className="px-6 py-4 font-semibold">Tarih</th>
                  <th className="px-6 py-4 font-semibold">Belge No</th>
                  <th className="px-6 py-4 font-semibold">İşlem Türü</th>
                  <th className="px-6 py-4 font-semibold">Açıklama</th>
                  <th className="px-6 py-4 font-semibold text-right">Borç</th>
                  <th className="px-6 py-4 font-semibold text-right">Alacak</th>
                  <th className="px-6 py-4 font-semibold text-right">Bakiye</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {isLoading ? (
                  <tr>
                    <td colSpan={7} className="px-6 py-12 text-center text-gray-500">
                      <div className="flex justify-center mb-2">
                        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
                      </div>
                      Yükleniyor...
                    </td>
                  </tr>
                ) : data?.items?.items?.length === 0 ? (
                  <tr>
                    <td colSpan={7} className="px-6 py-16 text-center text-gray-500">
                      <div className="flex flex-col items-center justify-center">
                        <div className="w-12 h-12 bg-gray-50 rounded-full flex items-center justify-center mb-3">
                          <Filter className="w-6 h-6 text-gray-400" />
                        </div>
                        <p className="font-medium text-gray-900 mb-1">Hareket Bulunamadı</p>
                        <p className="text-sm">Seçilen tarih aralığında herhangi bir hareket bulunmamaktadır.</p>
                      </div>
                    </td>
                  </tr>
                ) : (
                  data?.items?.items?.map((item, idx) => (
                    <tr key={idx} className="hover:bg-gray-50/50 transition-colors">
                      <td className="px-6 py-4 whitespace-nowrap">
                        {formatDateTime(item.date)}
                      </td>
                      <td className="px-6 py-4 font-medium text-gray-900">
                        #{item.documentId.toString().padStart(5, '0')}
                      </td>
                      <td className="px-6 py-4">
                        <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md text-xs font-medium border ${getBadgeColor(item.documentType)}`}>
                          {getDocumentTypeIcon(item.documentType)}
                          {getDocumentTypeName(item.documentType)}
                        </span>
                      </td>
                      <td className="px-6 py-4 max-w-xs truncate" title={item.description || '-'}>
                        {item.description || '-'}
                      </td>
                      <td className="px-6 py-4 text-right font-medium text-red-600">
                        {item.debit > 0 ? formatCurrency(item.debit) : '-'}
                      </td>
                      <td className="px-6 py-4 text-right font-medium text-green-600">
                        {item.credit > 0 ? formatCurrency(item.credit) : '-'}
                      </td>
                      <td className="px-6 py-4 text-right font-bold text-gray-900">
                        {formatCurrency(item.balanceAfterTransaction)}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}

        {/* Pagination */}
        {data && data.items.totalPages > 1 && (
          <div className="flex items-center justify-between px-6 py-4 border-t border-gray-100">
            <div className="text-sm text-gray-500">
              Toplam <span className="font-medium text-gray-900">{data.items.totalCount}</span> kayıttan{' '}
              <span className="font-medium text-gray-900">
                {(page - 1) * pageSize + 1}-
                {Math.min(page * pageSize, data.items.totalCount)}
              </span>{' '}
              arası gösteriliyor
            </div>
            <div className="flex gap-2">
              <button
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-3 py-1 text-sm border rounded hover:bg-gray-50 disabled:opacity-50"
              >
                Önceki
              </button>
              <button
                onClick={() => setPage(p => Math.min(data.items.totalPages, p + 1))}
                disabled={page === data.items.totalPages}
                className="px-3 py-1 text-sm border rounded hover:bg-gray-50 disabled:opacity-50"
              >
                Sonraki
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
