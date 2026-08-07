import React from 'react';
import { X, Calendar, User, FileText, CreditCard, Clock } from 'lucide-react';
import type { PaymentDto } from '../types';
import { formatCurrency, formatDate } from '../../../lib/formatters';

interface PaymentDetailModalProps {
  isOpen: boolean;
  onClose: () => void;
  payment: PaymentDto | null;
}

export const PaymentDetailModal: React.FC<PaymentDetailModalProps> = ({
  isOpen,
  onClose,
  payment
}) => {
  if (!isOpen || !payment) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-gray-900/50 backdrop-blur-sm">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-2xl overflow-hidden animate-in fade-in zoom-in-95 duration-200">
        <div className="flex justify-between items-center p-6 border-b border-gray-100">
          <h2 className="text-xl font-semibold text-gray-900">Tahsilat Detayı</h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-500 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="p-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            
            {/* Left Column */}
            <div className="space-y-6">
              <div>
                <h3 className="text-sm font-medium text-gray-500 flex items-center gap-2 mb-2">
                  <User className="w-4 h-4" />
                  Müşteri Bilgileri
                </h3>
                <div className="bg-gray-50 rounded-lg p-4">
                  <p className="font-medium text-gray-900">{payment.currentAccountName}</p>
                  <p className="text-sm text-gray-500">Kod: {payment.currentAccountCode}</p>
                </div>
              </div>

              <div>
                <h3 className="text-sm font-medium text-gray-500 flex items-center gap-2 mb-2">
                  <CreditCard className="w-4 h-4" />
                  Ödeme Bilgileri
                </h3>
                <div className="bg-gray-50 rounded-lg p-4 space-y-3">
                  <div>
                    <p className="text-sm text-gray-500">Tutar</p>
                    <p className="text-xl font-bold text-primary-600">
                      {formatCurrency(payment.amount)}
                    </p>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <p className="text-sm text-gray-500">Ödeme Yöntemi</p>
                      <p className="font-medium text-gray-900">
                        {payment.paymentMethodName || payment.paymentMethod}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Right Column */}
            <div className="space-y-6">
              <div>
                <h3 className="text-sm font-medium text-gray-500 flex items-center gap-2 mb-2">
                  <Calendar className="w-4 h-4" />
                  Tarih Bilgileri
                </h3>
                <div className="bg-gray-50 rounded-lg p-4 space-y-3">
                  <div>
                    <p className="text-sm text-gray-500">Tahsilat Tarihi</p>
                    <p className="font-medium text-gray-900">{formatDate(payment.paymentDate)}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-500">Kayıt Tarihi</p>
                    <p className="font-medium text-gray-900 flex items-center gap-2">
                      <Clock className="w-4 h-4 text-gray-400" />
                      {formatDate(payment.createdDate)}
                    </p>
                  </div>
                  {payment.updatedDate && (
                    <div>
                      <p className="text-sm text-gray-500">Son Güncelleme</p>
                      <p className="font-medium text-gray-900 flex items-center gap-2">
                        <Clock className="w-4 h-4 text-gray-400" />
                        {formatDate(payment.updatedDate)}
                      </p>
                    </div>
                  )}
                </div>
              </div>

              {payment.description && (
                <div>
                  <h3 className="text-sm font-medium text-gray-500 flex items-center gap-2 mb-2">
                    <FileText className="w-4 h-4" />
                    Açıklama
                  </h3>
                  <div className="bg-gray-50 rounded-lg p-4">
                    <p className="text-sm text-gray-900 whitespace-pre-wrap">{payment.description}</p>
                  </div>
                </div>
              )}
            </div>

          </div>
        </div>

        <div className="flex justify-end p-6 bg-gray-50 border-t border-gray-100">
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
