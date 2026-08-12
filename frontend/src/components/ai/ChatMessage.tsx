import React from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeSanitize from 'rehype-sanitize';
import { User, Bot } from 'lucide-react';
import type { ChatMessage as ChatMessageType } from '../../hooks/useChat';

interface Props {
  message: ChatMessageType;
}

export const ChatMessage: React.FC<Props> = ({ message }) => {
  const isUser = message.role === 'user';

  return (
    <div className={`flex w-full ${isUser ? 'justify-end' : 'justify-start'} mb-4`}>
      <div className={`flex max-w-[85%] ${isUser ? 'flex-row-reverse' : 'flex-row'} items-end gap-2`}>
        
        {/* Avatar */}
        <div className={`flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center ${isUser ? 'bg-indigo-100 text-indigo-600' : 'bg-gray-800 text-white'}`}>
          {isUser ? <User size={16} /> : <Bot size={16} />}
        </div>
        
        {/* Message Bubble */}
        <div 
          className={`px-4 py-3 rounded-2xl ${
            isUser 
              ? 'bg-indigo-600 text-white rounded-br-sm' 
              : 'bg-white border border-gray-200 text-gray-800 rounded-bl-sm shadow-sm'
          }`}
        >
          {isUser ? (
            <div className="whitespace-pre-wrap text-sm">{message.content}</div>
          ) : (
            <div className="prose prose-sm max-w-none prose-p:leading-relaxed prose-pre:bg-gray-900 prose-pre:text-gray-100">
              <ReactMarkdown 
                remarkPlugins={[remarkGfm]}
                rehypePlugins={[rehypeSanitize]}
                components={{
                  h1: ({ children, ...props }) => <h1 className="text-xl font-bold mt-4 mb-2 text-gray-800" {...props}>{children}</h1>,
                  h2: ({ children, ...props }) => <h2 className="text-lg font-bold mt-3 mb-2 text-gray-800" {...props}>{children}</h2>,
                  h3: ({ children, ...props }) => <h3 className="text-md font-bold mt-2 mb-1 text-gray-800" {...props}>{children}</h3>,
                  p: ({ children, ...props }) => <p className="mb-2 leading-relaxed" {...props}>{children}</p>,
                  ul: ({ children, ...props }) => <ul className="list-disc pl-5 mb-2 space-y-1" {...props}>{children}</ul>,
                  ol: ({ children, ...props }) => <ol className="list-decimal pl-5 mb-2 space-y-1" {...props}>{children}</ol>,
                  li: ({ children, ...props }) => <li className="text-gray-700" {...props}>{children}</li>,
                  a: ({ children, ...props }) => <a className="text-blue-600 hover:underline" target="_blank" rel="noopener noreferrer" {...props}>{children}</a>,
                  blockquote: ({ children, ...props }) => <blockquote className="border-l-4 border-gray-300 pl-4 italic text-gray-600 my-2" {...props}>{children}</blockquote>,
                  table: ({...props}) => (
                    <div className="overflow-x-auto my-4 rounded-lg border border-gray-200">
                      <table className="min-w-full divide-y divide-gray-200 m-0" {...props} />
                    </div>
                  ),
                  thead: ({...props}) => <thead className="bg-gray-50" {...props} />,
                  th: ({...props}) => <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-b border-gray-200" {...props} />,
                  td: ({...props}) => <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-900 border-b border-gray-100" {...props} />,
                  tr: ({...props}) => <tr className="hover:bg-gray-50 transition-colors" {...props} />
                }}
              >
                {message.content}
              </ReactMarkdown>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
