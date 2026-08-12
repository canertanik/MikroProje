import React, { useState, useEffect, useRef } from 'react';
import { MessageSquare, X, Maximize2, Minimize2, Trash2 } from 'lucide-react';
import { useChat } from '../../hooks/useChat';
import { ChatMessage } from './ChatMessage';
import { ChatInput } from './ChatInput';

export const ChatWidget: React.FC = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [isExpanded, setIsExpanded] = useState(false);
  const { messages, isLoading, error, sendMessage, clearChat } = useChat();
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    if (isOpen) {
      scrollToBottom();
    }
  }, [messages, isOpen, isLoading]);

  if (!isOpen) {
    return (
      <button
        onClick={() => setIsOpen(true)}
        className="fixed bottom-6 right-6 w-14 h-14 bg-indigo-600 text-white rounded-full shadow-lg hover:shadow-xl hover:bg-indigo-700 hover:scale-105 transition-all flex items-center justify-center z-50 group"
        aria-label="Mikro AI'yi Aç"
      >
        <MessageSquare size={24} className="group-hover:animate-pulse" />
        
        {/* Unread indicator could go here if we implement background polling */}
      </button>
    );
  }

  return (
    <div 
      className={`fixed z-50 bg-white flex flex-col shadow-2xl transition-all duration-300 ease-in-out border border-gray-200 overflow-hidden ${
        isExpanded 
          ? 'inset-4 sm:inset-10 rounded-2xl' 
          : 'bottom-0 right-0 sm:bottom-6 sm:right-6 w-full h-full sm:w-[400px] sm:h-[600px] sm:rounded-2xl'
      }`}
    >
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3 bg-indigo-600 text-white shadow-md z-10">
        <div className="flex items-center gap-2">
          <MessageSquare size={20} />
          <div>
            <h3 className="font-semibold text-sm">Mikro AI Asistanı</h3>
            <p className="text-xs text-indigo-200">ERP verinizle konuşun</p>
          </div>
        </div>
        
        <div className="flex items-center gap-1">
          <button 
            onClick={clearChat}
            className="p-1.5 hover:bg-indigo-700 rounded transition-colors text-indigo-100 hover:text-white"
            title="Sohbeti Temizle"
          >
            <Trash2 size={16} />
          </button>
          
          <button 
            onClick={() => setIsExpanded(!isExpanded)}
            className="p-1.5 hover:bg-indigo-700 rounded transition-colors hidden sm:block text-indigo-100 hover:text-white"
            title={isExpanded ? "Küçült" : "Genişlet"}
          >
            {isExpanded ? <Minimize2 size={16} /> : <Maximize2 size={16} />}
          </button>
          
          <button 
            onClick={() => setIsOpen(false)}
            className="p-1.5 hover:bg-indigo-700 rounded transition-colors text-indigo-100 hover:text-white"
            title="Kapat"
          >
            <X size={18} />
          </button>
        </div>
      </div>

      {/* Error Banner */}
      {error && (
        <div className="bg-red-50 p-2 px-4 border-b border-red-100 text-red-600 text-xs flex items-center justify-between">
          <span>{error}</span>
          <button onClick={clearChat} className="text-red-800 hover:underline">Gizle</button>
        </div>
      )}

      {/* Messages Area */}
      <div className="flex-1 overflow-y-auto p-4 bg-gray-50/50">
        {messages.length === 0 ? (
          <div className="h-full flex flex-col items-center justify-center text-center px-6 opacity-60">
            <MessageSquare size={48} className="text-indigo-300 mb-4" />
            <p className="text-gray-600 font-medium">Merhaba, ben Mikro AI!</p>
            <p className="text-sm text-gray-500 mt-2">
              Güncel satışları, stok riskini öğrenebilir veya belirli bir ürün için yapay zeka destekli tahmin alabilirsiniz.
            </p>
            <div className="mt-6 flex flex-col gap-2 w-full">
              <button 
                onClick={() => sendMessage("Güncel satış ve stok özetini getir")}
                className="text-xs bg-white border border-gray-200 px-3 py-2 rounded-lg hover:bg-indigo-50 hover:border-indigo-200 transition-colors text-left"
              >
                "Güncel satış ve stok özetini getir"
              </button>
              <button 
                onClick={() => sendMessage("Hangi ürünlerin stoğu kritik seviyede?")}
                className="text-xs bg-white border border-gray-200 px-3 py-2 rounded-lg hover:bg-indigo-50 hover:border-indigo-200 transition-colors text-left"
              >
                "Hangi ürünlerin stoğu kritik seviyede?"
              </button>
            </div>
          </div>
        ) : (
          <div className="flex flex-col">
            {messages.map((msg) => (
              <ChatMessage key={msg.id} message={msg} />
            ))}
            {isLoading && messages[messages.length - 1]?.role === 'user' && (
              <div className="flex justify-start mb-4">
                <div className="flex items-center gap-2">
                  <div className="w-8 h-8 rounded-full bg-gray-800 text-white flex items-center justify-center animate-pulse">
                    <MessageSquare size={14} />
                  </div>
                  <div className="bg-white border border-gray-200 px-4 py-2 rounded-2xl rounded-bl-sm shadow-sm flex items-center gap-1">
                    <span className="w-1.5 h-1.5 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '0ms' }}></span>
                    <span className="w-1.5 h-1.5 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '150ms' }}></span>
                    <span className="w-1.5 h-1.5 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '300ms' }}></span>
                  </div>
                </div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>
        )}
      </div>

      {/* Input Area */}
      <ChatInput onSend={sendMessage} disabled={isLoading} />
    </div>
  );
};
