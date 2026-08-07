import React from 'react';
import { X, Calendar, DollarSign, Building, FileText, Hash } from 'lucide-react';
import type { SupplierPaymentDto } from '../types';
import { PaymentMethod } from '../types';

interface SupplierPaymentDetailModalProps {
  payment: SupplierPaymentDto | null;
  isOpen: boolean;
  onClose: () => void;
}

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('tr-TR', {
    style: 'currency',
    currency: 'TRY',
  }).format(amount);
};

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('tr-TR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
};

const getPaymentMethodBadge = (method: PaymentMethod) => {
  switch (method) {
    case PaymentMethod.Cash:
      return <span className="px-2 py-1 text-xs font-medium rounded-full bg-green-100 text-green-800">Nakit</span>;
    case PaymentMethod.BankTransfer:
      return <span className="px-2 py-1 text-xs font-medium rounded-full bg-blue-100 text-blue-800">Havale/EFT</span>;
    case PaymentMethod.CreditCard:
      return <span className="px-2 py-1 text-xs font-medium rounded-full bg-purple-100 text-purple-800">Kredi Kartı</span>;
    default:
      return <span className="px-2 py-1 text-xs font-medium rounded-full bg-gray-100 text-gray-800">Bilinmiyor</span>;
  }
};

export const SupplierPaymentDetailModal: React.FC<SupplierPaymentDetailModalProps> = ({
  payment,
  isOpen,
  onClose,
}) => {
  if (!isOpen || !payment) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-gray-900/50 backdrop-blur-sm">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-2xl overflow-hidden animate-in fade-in zoom-in-95 duration-200">
        <div className="flex justify-between items-center p-6 border-b border-gray-100 bg-gray-50/50">
          <div>
            <h2 className="text-xl font-semibold text-gray-900">Ödeme Detayı</h2>
            <p className="text-sm text-gray-500 mt-1">
              Ödeme No: #{payment.id.toString().padStart(5, '0')}
            </p>
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-500 transition-colors p-2 hover:bg-gray-100 rounded-full"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="p-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div className="space-y-6">
              <div>
                <h3 className="text-sm font-medium text-gray-500 flex items-center gap-2 mb-2">
                  <Building className="w-4 h-4" />
                  Tedarikçi Bilgileri
                </h3>
                <div className="bg-gray-50 rounded-lg p-4 border border-gray-100">
                  <p className="font-medium text-gray-900">{payment.currentAccountName}</p>
                </div>
              </div>

              <div>
                <h3 className="text-sm font-medium text-gray-500 flex items-center gap-2 mb-2">
                  <DollarSign className="w-4 h-4" />
                  Tutar ve Yöntem
                </h3>
                <div className="bg-gray-50 rounded-lg p-4 border border-gray-100 space-y-3">
                  <div>
                    <span className="text-sm text-gray-500 block mb-1">Ödenen Tutar</span>
                    <span className="text-2xl font-bold text-gray-900">{formatCurrency(payment.amount)}</span>
                  </div>
                  <div>
                    <span className="text-sm text-gray-500 block mb-1">Ödeme Yöntemi</span>
                    {getPaymentMethodBadge(payment.paymentMethod)}
                  </div>
                </div>
              </div>
            </div>

            <div className="space-y-6">
              <div>
                <h3 className="text-sm font-medium text-gray-500 flex items-center gap-2 mb-2">
                  <Calendar className="w-4 h-4" />
                  Tarih Bilgileri
                </h3>
                <div className="bg-gray-50 rounded-lg p-4 border border-gray-100 space-y-3">
                  <div className="flex justify-between items-center">
                    <span className="text-sm text-gray-500">Ödeme Tarihi</span>
                    <span className="text-sm font-medium text-gray-900">{formatDate(payment.paymentDate)}</span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-sm text-gray-500">Oluşturulma</span>
                    <span className="text-sm font-medium text-gray-900">{formatDate(payment.createdDate)}</span>
                  </div>
                </div>
              </div>

              {payment.referenceNumber && (
                <div>
                  <h3 className="text-sm font-medium text-gray-500 flex items-center gap-2 mb-2">
                    <Hash className="w-4 h-4" />
                    Referans No
                  </h3>
                  <div className="bg-gray-50 rounded-lg p-4 border border-gray-100">
                    <p className="text-sm text-gray-900">{payment.referenceNumber}</p>
                  </div>
                </div>
              )}

              {payment.description && (
                <div>
                  <h3 className="text-sm font-medium text-gray-500 flex items-center gap-2 mb-2">
                    <FileText className="w-4 h-4" />
                    Açıklama
                  </h3>
                  <div className="bg-gray-50 rounded-lg p-4 border border-gray-100">
                    <p className="text-sm text-gray-900 whitespace-pre-wrap">{payment.description}</p>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>

        <div className="p-6 bg-gray-50 border-t border-gray-100 flex justify-end">
          <button
            onClick={onClose}
            className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
          >
            Kapat
          </button>
        </div>
      </div>
    </div>
  );
};
