import React, { useState, useRef, useEffect } from 'react';
import { Send, Loader2 } from 'lucide-react';

interface Props {
  onSend: (message: string) => void;
  disabled?: boolean;
}

export const ChatInput: React.FC<Props> = ({ onSend, disabled }) => {
  const [input, setInput] = useState('');
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  useEffect(() => {
    if (textareaRef.current) {
      textareaRef.current.style.height = 'auto';
      textareaRef.current.style.height = `${Math.min(textareaRef.current.scrollHeight, 120)}px`;
    }
  }, [input]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (input.trim() && !disabled) {
      onSend(input);
      setInput('');
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSubmit(e);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="relative flex items-end w-full gap-2 bg-white p-3 border-t border-gray-100">
      <div className="relative flex-1 bg-gray-50 rounded-2xl border border-gray-200 focus-within:border-indigo-500 focus-within:ring-1 focus-within:ring-indigo-500 transition-all">
        <textarea
          ref={textareaRef}
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Mikro AI'ye bir soru sorun..."
          className="w-full max-h-[120px] bg-transparent resize-none py-3 px-4 outline-none text-sm text-gray-800 placeholder-gray-400"
          rows={1}
          disabled={disabled}
        />
      </div>
      
      <button
        type="submit"
        disabled={disabled || !input.trim()}
        className={`flex-shrink-0 flex items-center justify-center w-11 h-11 rounded-full mb-0.5 transition-colors ${
          disabled || !input.trim()
            ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
            : 'bg-indigo-600 text-white hover:bg-indigo-700 shadow-sm hover:shadow'
        }`}
      >
        {disabled ? (
          <Loader2 size={18} className="animate-spin" />
        ) : (
          <Send size={18} className="ml-0.5" />
        )}
      </button>
    </form>
  );
};
