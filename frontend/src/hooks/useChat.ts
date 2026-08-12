import { useState, useRef, useCallback } from 'react';
import { useAuthStore } from '../stores/useAuthStore';

export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
}

export const useChat = () => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const abortControllerRef = useRef<AbortController | null>(null);

  const sendMessage = useCallback(async (content: string) => {
    if (!content.trim()) return;

    // Add user message immediately
    const userMsgId = Date.now().toString();
    setMessages((prev) => [...prev, { id: userMsgId, role: 'user', content }]);
    
    // Add empty assistant message that will be populated
    const aiMsgId = (Date.now() + 1).toString();
    setMessages((prev) => [...prev, { id: aiMsgId, role: 'assistant', content: '' }]);
    
    setIsLoading(true);
    setError(null);

    // Cancel previous request if any
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    abortControllerRef.current = new AbortController();

    const token = useAuthStore.getState().accessToken;
    const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080';

    try {
      const history = messages
        .slice(-10) // Limit history to last 10 messages to prevent token budget explosion
        .map(msg => ({ role: msg.role, content: msg.content }));

      const response = await fetch(`${baseUrl}/api/ai/chat`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        },
        body: JSON.stringify({ message: content, history }),
        signal: abortControllerRef.current.signal
      });

      if (!response.ok) {
        throw new Error(`Error: ${response.status} ${response.statusText}`);
      }

      const reader = response.body?.getReader();
      const decoder = new TextDecoder('utf-8');

      if (!reader) {
        throw new Error('No readable stream available');
      }

      let buffer = '';

      while (true) {
        const { value, done } = await reader.read();
        if (done) break;
        
        buffer += decoder.decode(value, { stream: true });
        
        let boundaryIndex;
        while ((boundaryIndex = buffer.indexOf('\n\n')) >= 0) {
          const messageStr = buffer.slice(0, boundaryIndex).trim();
          buffer = buffer.slice(boundaryIndex + 2);
          
          if (!messageStr) continue;
          
          const lines = messageStr.split('\n');
          for (const line of lines) {
            if (line.startsWith('data: ')) {
              const dataStr = line.substring(6);
              if (dataStr === '[DONE]') continue;
              
              try {
                const data = JSON.parse(dataStr);
                if (data.type === 'text_delta' && data.content) {
                  setMessages((prev) => prev.map(msg => 
                    msg.id === aiMsgId 
                      ? { ...msg, content: msg.content + data.content }
                      : msg
                  ));
                }
              } catch (e) {
                console.error('Failed to parse SSE line:', line, e);
              }
            }
          }
        }
      }
    } catch (err: any) {
      if (err.name !== 'AbortError') {
        setError(err.message || 'An error occurred while fetching chat response');
      }
    } finally {
      setIsLoading(false);
    }
  }, [messages]);

  const clearChat = useCallback(() => {
    setMessages([]);
    setError(null);
  }, []);

  return {
    messages,
    isLoading,
    error,
    sendMessage,
    clearChat
  };
};
