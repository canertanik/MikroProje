import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { 
  Users, DollarSign, TrendingUp, AlertCircle, ShoppingCart, CreditCard, 
  ArrowRightLeft, AlertTriangle, FileText, Factory, Clock 
} from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import { 
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, Legend, ResponsiveContainer 
} from 'recharts';
import { 
  getDashboardSummary, getDashboardTrends, getRecentActivities, getCriticalStock, getTopRecords 
} from '../features/dashboard/api';
import { format, subDays, startOfMonth, startOfYear, subMonths } from 'date-fns';
import { AiInsightsCard } from '../components/ai/AiInsightsCard';

export const Dashboard = () => {
  const navigate = useNavigate();
  const [dateRange, setDateRange] = useState('thisMonth');

  // Compute dates based on selection
  const getDates = () => {
    const today = new Date();
    let startDate: string | undefined;
    let endDate: string | undefined;

    switch (dateRange) {
      case 'today':
        startDate = format(today, 'yyyy-MM-dd');
        endDate = format(today, 'yyyy-MM-dd');
        break;
      case 'last7days':
        startDate = format(subDays(today, 7), 'yyyy-MM-dd');
        endDate = format(today, 'yyyy-MM-dd');
        break;
      case 'thisMonth':
        startDate = format(startOfMonth(today), 'yyyy-MM-dd');
        endDate = format(today, 'yyyy-MM-dd');
        break;
      case 'lastMonth': {
        const lastM = subMonths(today, 1);
        startDate = format(startOfMonth(lastM), 'yyyy-MM-dd');
        endDate = format(lastM, 'yyyy-MM-dd'); // approximate
        break;
      }
      case 'thisYear':
        startDate = format(startOfYear(today), 'yyyy-MM-dd');
        endDate = format(today, 'yyyy-MM-dd');
        break;
      default:
        break; // all time
    }
    return { startDate, endDate };
  };

  const { startDate, endDate } = getDates();

  const { data: summary, isLoading: loadingSummary, isError: isSummaryError } = useQuery({
    queryKey: ['dashboardSummary', startDate, endDate],
    queryFn: () => getDashboardSummary(startDate, endDate),
  });

  const { data: trends, isLoading: loadingTrends } = useQuery({
    queryKey: ['dashboardTrends', startDate, endDate],
    queryFn: () => getDashboardTrends(startDate, endDate),
  });

  const { data: activities, isLoading: loadingActivities } = useQuery({
    queryKey: ['dashboardActivities'],
    queryFn: getRecentActivities,
  });

  const { data: criticalStock, isLoading: loadingStock } = useQuery({
    queryKey: ['dashboardCriticalStock'],
    queryFn: getCriticalStock,
  });

  // Remove unused topRecords since we aren't displaying them in this basic UI version yet.
  const { isLoading: loadingTop } = useQuery({
    queryKey: ['dashboardTopRecords', startDate, endDate],
    queryFn: () => getTopRecords(startDate, endDate),
  });

  if (loadingSummary || loadingTrends || loadingActivities || loadingStock || loadingTop) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
        <span className="ml-3 text-gray-500">Dashboard verileri yükleniyor...</span>
      </div>
    );
  }

  if (isSummaryError) {
    return (
      <div className="bg-red-50 p-6 rounded-lg border border-red-200 flex items-start">
        <AlertCircle className="w-6 h-6 text-red-600 mr-3 mt-0.5" />
        <div>
          <h3 className="text-lg font-medium text-red-800">Veriler yüklenirken hata oluştu</h3>
          <p className="mt-2 text-sm text-red-600">
            Lütfen daha sonra tekrar deneyin veya bağlantınızı kontrol edin.
          </p>
        </div>
      </div>
    );
  }

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value || 0);
  };

  const formatDate = (dateString: string) => {
    return format(new Date(dateString), 'dd.MM.yyyy HH:mm');
  };

  const getActivityConfig = (type: string) => {
    switch (type) {
      case 'Sale': return { label: 'Satış', color: 'bg-green-100 text-green-800', icon: TrendingUp };
      case 'Payment': return { label: 'Tahsilat', color: 'bg-blue-100 text-blue-800', icon: DollarSign };
      case 'Purchase': return { label: 'Alım', color: 'bg-orange-100 text-orange-800', icon: ShoppingCart };
      case 'SupplierPayment': return { label: 'Ödeme', color: 'bg-red-100 text-red-800', icon: CreditCard };
      case 'StockTransfer': return { label: 'Transfer', color: 'bg-purple-100 text-purple-800', icon: ArrowRightLeft };
      default: return { label: 'Diğer', color: 'bg-gray-100 text-gray-800', icon: FileText };
    }
  };

  return (
    <div className="space-y-6">
      {/* Header and Filter */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <h1 className="text-2xl font-semibold text-gray-900">Kontrol Paneli</h1>
        <select
          value={dateRange}
          onChange={(e) => setDateRange(e.target.value)}
          className="block w-48 rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
        >
          <option value="today">Bugün</option>
          <option value="last7days">Son 7 Gün</option>
          <option value="thisMonth">Bu Ay</option>
          <option value="lastMonth">Geçen Ay</option>
          <option value="thisYear">Bu Yıl</option>
          <option value="all">Tüm Zamanlar</option>
        </select>
      </div>

      {/* AI Insights Card */}
      <AiInsightsCard />

      {/* Main KPIs Row 1 */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
        <div className="relative overflow-hidden rounded-lg bg-white p-5 shadow-sm border border-gray-100">
          <dt>
            <div className="absolute rounded-md bg-blue-50 p-3">
              <TrendingUp className="h-6 w-6 text-blue-600" aria-hidden="true" />
            </div>
            <p className="ml-16 min-h-10 text-sm font-medium leading-5 text-gray-500">Satışlar (Seçili Dönem)</p>
          </dt>
          <dd className="ml-16 flex items-baseline pb-1">
            <p className="text-2xl font-semibold text-gray-900">{formatCurrency(summary?.salesTotal || 0)}</p>
          </dd>
        </div>

        <div className="relative overflow-hidden rounded-lg bg-white p-5 shadow-sm border border-gray-100">
          <dt>
            <div className="absolute rounded-md bg-orange-50 p-3">
              <ShoppingCart className="h-6 w-6 text-orange-600" aria-hidden="true" />
            </div>
            <p className="ml-16 min-h-10 text-sm font-medium leading-5 text-gray-500">Satın Almalar (Seçili Dönem)</p>
          </dt>
          <dd className="ml-16 flex items-baseline pb-1">
            <p className="text-2xl font-semibold text-gray-900">{formatCurrency(summary?.purchasesTotal || 0)}</p>
          </dd>
        </div>

        <div className="relative overflow-hidden rounded-lg bg-white p-5 shadow-sm border border-gray-100">
          <dt>
            <div className="absolute rounded-md bg-green-50 p-3">
              <DollarSign className="h-6 w-6 text-green-600" aria-hidden="true" />
            </div>
            <p className="ml-16 min-h-10 text-sm font-medium leading-5 text-gray-500">Toplam Müşteri Alacağı</p>
          </dt>
          <dd className="ml-16 flex items-baseline pb-1">
            <p className="text-2xl font-semibold text-gray-900">{formatCurrency(summary?.totalCustomerReceivable || 0)}</p>
          </dd>
        </div>

        <div className="relative overflow-hidden rounded-lg bg-white p-5 shadow-sm border border-gray-100">
          <dt>
            <div className="absolute rounded-md bg-red-50 p-3">
              <CreditCard className="h-6 w-6 text-red-600" aria-hidden="true" />
            </div>
            <p className="ml-16 min-h-10 text-sm font-medium leading-5 text-gray-500">Toplam Tedarikçi Borcu</p>
          </dt>
          <dd className="ml-16 flex items-baseline pb-1">
            <p className="text-2xl font-semibold text-gray-900">{formatCurrency(Math.abs(summary?.totalSupplierPayable || 0))}</p>
          </dd>
        </div>
      </div>

      {/* Operational KPIs Row 2 */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
        <div 
          onClick={() => navigate('/purchases')}
          className="relative overflow-hidden rounded-lg bg-white p-5 shadow-sm border border-gray-100 hover:border-primary-300 cursor-pointer transition-colors"
        >
          <dt>
            <div className="absolute rounded-md bg-yellow-50 p-3">
              <Clock className="h-6 w-6 text-yellow-600" aria-hidden="true" />
            </div>
            <p className="ml-16 min-h-10 text-sm font-medium leading-5 text-gray-500">Bekleyen Alımlar</p>
          </dt>
          <dd className="ml-16 flex items-baseline pb-1">
            <p className="text-2xl font-semibold text-gray-900">{summary?.pendingPurchaseCount || 0}</p>
          </dd>
        </div>

        <div 
          onClick={() => navigate('/stock-transfers')}
          className="relative overflow-hidden rounded-lg bg-white p-5 shadow-sm border border-gray-100 hover:border-primary-300 cursor-pointer transition-colors"
        >
          <dt>
            <div className="absolute rounded-md bg-purple-50 p-3">
              <ArrowRightLeft className="h-6 w-6 text-purple-600" aria-hidden="true" />
            </div>
            <p className="ml-16 min-h-10 text-sm font-medium leading-5 text-gray-500">Draft Transferler</p>
          </dt>
          <dd className="ml-16 flex items-baseline pb-1">
            <p className="text-2xl font-semibold text-gray-900">{summary?.draftTransferCount || 0}</p>
          </dd>
        </div>

        <div className="relative overflow-hidden rounded-lg bg-white p-5 shadow-sm border border-gray-100">
          <dt>
            <div className="absolute rounded-md bg-indigo-50 p-3">
              <Users className="h-6 w-6 text-indigo-600" aria-hidden="true" />
            </div>
            <p className="ml-16 min-h-10 text-sm font-medium leading-5 text-gray-500">Borçlu Müşteriler</p>
          </dt>
          <dd className="ml-16 flex items-baseline pb-1">
            <p className="text-2xl font-semibold text-gray-900">{summary?.customerWithDebtCount || 0} / {summary?.totalCustomerCount || 0}</p>
          </dd>
        </div>

        <div className="relative overflow-hidden rounded-lg bg-white p-5 shadow-sm border border-gray-100">
          <dt>
            <div className="absolute rounded-md bg-pink-50 p-3">
              <Factory className="h-6 w-6 text-pink-600" aria-hidden="true" />
            </div>
            <p className="ml-16 min-h-10 text-sm font-medium leading-5 text-gray-500">Alacaklı Tedarikçiler</p>
          </dt>
          <dd className="ml-16 flex items-baseline pb-1">
            <p className="text-2xl font-semibold text-gray-900">{summary?.supplierWithDebtCount || 0} / {summary?.totalSupplierCount || 0}</p>
          </dd>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Chart Section */}
        <div className="lg:col-span-2 bg-white p-6 rounded-lg shadow-sm border border-gray-200">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Satış & Satın Alma Trendi</h3>
          <div className="h-72 w-full">
            {trends && trends.length > 0 ? (
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={trends} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} />
                  <XAxis dataKey="dateLabel" tick={{ fontSize: 12 }} />
                  <YAxis tickFormatter={(value) => `${(value / 1000).toFixed(0)}k`} />
                  <RechartsTooltip 
                    formatter={(value: any) => formatCurrency(value as number)}
                    labelStyle={{ color: '#374151', fontWeight: 600 }}
                  />
                  <Legend />
                  <Bar dataKey="salesTotal" name="Satışlar" fill="#3b82f6" radius={[4, 4, 0, 0]} />
                  <Bar dataKey="purchasesTotal" name="Satın Almalar" fill="#f97316" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <div className="flex justify-center items-center h-full text-gray-400">Veri bulunamadı</div>
            )}
          </div>
        </div>

        {/* Cash Flow Summary Section */}
        <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200 flex flex-col justify-between">
          <div>
            <h3 className="text-lg font-medium text-gray-900 mb-4">Nakit Akış Özeti</h3>
            <div className="space-y-4">
              <div className="flex justify-between items-center border-b pb-3">
                <span className="text-gray-600">Gelen (Tahsilat)</span>
                <span className="font-semibold text-green-600">{formatCurrency(summary?.customerPaymentTotal || 0)}</span>
              </div>
              <div className="flex justify-between items-center border-b pb-3">
                <span className="text-gray-600">Giden (Ödeme)</span>
                <span className="font-semibold text-red-600">{formatCurrency(summary?.supplierPaymentTotal || 0)}</span>
              </div>
            </div>
          </div>
          <div className="mt-6 pt-4 border-t-2 border-gray-100">
            <div className="flex justify-between items-center">
              <span className="text-gray-900 font-medium">Net Nakit Akışı</span>
              <span className={`font-bold text-xl ${(summary?.netCashFlow || 0) >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                {formatCurrency(summary?.netCashFlow || 0)}
              </span>
            </div>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Recent Activities */}
        <div className="lg:col-span-2 bg-white shadow-sm rounded-lg border border-gray-200 overflow-hidden">
          <div className="px-4 py-5 sm:px-6 border-b border-gray-200 flex justify-between items-center">
            <h3 className="text-lg leading-6 font-medium text-gray-900">Son Hareketler</h3>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">İşlem</th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">İlgili Kişi/Depo</th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tutar/Miktar</th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tarih</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {activities && activities.length > 0 ? (
                  activities.map((act, i) => {
                    const config = getActivityConfig(act.activityType);
                    const ActIcon = config.icon;
                    return (
                      <tr key={i} className="hover:bg-gray-50">
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="flex items-center">
                            <span className={`p-1.5 rounded-full ${config.color} mr-2`}>
                              <ActIcon className="w-4 h-4" />
                            </span>
                            <div>
                              <div className="text-sm font-medium text-gray-900">{config.label}</div>
                              <div className="text-xs text-gray-500">{act.documentNumber}</div>
                            </div>
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-700">
                          {act.relatedEntityName}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                          {act.activityType === 'StockTransfer' ? '-' : formatCurrency(act.amountOrQuantity)}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {formatDate(act.date)}
                        </td>
                      </tr>
                    );
                  })
                ) : (
                  <tr>
                    <td colSpan={4} className="px-6 py-8 text-center text-sm text-gray-500">
                      Son hareket bulunmuyor
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>

        {/* Critical Stock */}
        <div className="bg-white shadow-sm rounded-lg border border-gray-200 overflow-hidden flex flex-col">
          <div className="px-4 py-5 sm:px-6 border-b border-gray-200 flex justify-between items-center">
            <h3 className="text-lg leading-6 font-medium text-gray-900 flex items-center">
              <AlertTriangle className="w-5 h-5 text-red-500 mr-2" />
              Kritik Stok
            </h3>
            <Link to="/products" className="text-sm font-medium text-primary-600 hover:text-primary-700">Tümü</Link>
          </div>
          <div className="overflow-y-auto flex-1 p-0">
            <ul className="divide-y divide-gray-200">
              {criticalStock && criticalStock.length > 0 ? (
                criticalStock.map((prod) => (
                  <li key={prod.productId} className="px-4 py-4 hover:bg-gray-50 cursor-pointer" onClick={() => navigate(`/products`)}>
                    <div className="flex items-center justify-between">
                      <div className="truncate">
                        <p className="text-sm font-medium text-gray-900 truncate">{prod.productName}</p>
                        <p className="text-xs text-gray-500">{prod.productCode}</p>
                      </div>
                      <div className="flex flex-col items-end">
                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${prod.currentStock <= 0 ? 'bg-red-100 text-red-800' : 'bg-yellow-100 text-yellow-800'}`}>
                          Stok: {prod.currentStock}
                        </span>
                        <span className="text-xs text-gray-500 mt-1">Kritik: {prod.criticalStock}</span>
                      </div>
                    </div>
                  </li>
                ))
              ) : (
                <li className="px-4 py-8 text-center text-sm text-gray-500">
                  Kritik stokta ürün bulunmuyor.
                </li>
              )}
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
};
