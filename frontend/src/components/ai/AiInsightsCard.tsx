import React, { useEffect, useState } from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeSanitize from 'rehype-sanitize';
import { Sparkles, Loader2, AlertCircle, RefreshCw } from 'lucide-react';
import api from '../../api/axios';

interface DashboardInsightDto {
  summary: string;
  riskExplanation: string;
  recommendedAction: string;
  warnings: string[];
}

export const AiInsightsCard: React.FC = () => {
  const [content, setContent] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchInsights = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await api.get<DashboardInsightDto>('/api/ai/insights/dashboard');
      const data = response.data;
      
      let md = `**Özet:** ${data.summary}\n\n`;
      if (data.riskExplanation) {
        md += `**Risk Analizi:** ${data.riskExplanation}\n\n`;
      }
      if (data.recommendedAction) {
        md += `**Öneri:** ${data.recommendedAction}\n\n`;
      }
      if (data.warnings && data.warnings.length > 0) {
        md += `**Uyarılar:**\n`;
        data.warnings.forEach(w => md += `- ${w}\n`);
      }
      
      setContent(md);
    } catch (err: any) {
      console.error('Failed to fetch AI insights:', err);
      setError('Yapay zeka özetleri yüklenirken bir hata oluştu.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    fetchInsights();
  }, []);

  if (error) {
    return (
      <div className="bg-red-50 border border-red-100 rounded-xl p-4 flex items-start gap-3">
        <AlertCircle className="text-red-500 mt-0.5 flex-shrink-0" size={20} />
        <div className="flex-1">
          <h4 className="text-red-800 font-medium text-sm">Mikro AI Hatası</h4>
          <p className="text-red-600 text-sm mt-1">{error}</p>
          <button 
            onClick={fetchInsights}
            className="mt-3 text-xs flex items-center gap-1.5 text-red-700 font-medium hover:text-red-900 bg-red-100/50 px-3 py-1.5 rounded-lg transition-colors"
          >
            <RefreshCw size={14} /> Tekrar Dene
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-gradient-to-br from-indigo-50 via-white to-purple-50 border border-indigo-100/50 rounded-2xl shadow-sm overflow-hidden mb-6 relative">
      {/* Decorative background elements */}
      <div className="absolute top-0 right-0 -mr-8 -mt-8 w-32 h-32 rounded-full bg-indigo-500/5 blur-2xl"></div>
      <div className="absolute bottom-0 left-0 -ml-8 -mb-8 w-32 h-32 rounded-full bg-purple-500/5 blur-2xl"></div>
      
      <div className="px-6 py-4 border-b border-indigo-100/50 flex items-center justify-between bg-white/50 backdrop-blur-sm relative z-10">
        <div className="flex items-center gap-2 text-indigo-700">
          <Sparkles size={18} className="animate-pulse" />
          <h3 className="font-semibold">Mikro AI Günlük Özet</h3>
        </div>
        
        {isLoading ? (
          <div className="flex items-center gap-2 text-xs text-indigo-500 font-medium bg-indigo-50 px-3 py-1.5 rounded-full">
            <Loader2 size={14} className="animate-spin" />
            <span>Analiz Ediliyor...</span>
          </div>
        ) : (
          <button 
            onClick={fetchInsights}
            className="text-gray-400 hover:text-indigo-600 transition-colors p-1"
            title="Özeti Yenile"
          >
            <RefreshCw size={16} />
          </button>
        )}
      </div>
      
      <div className="p-6 relative z-10 min-h-[120px]">
        {isLoading && !content ? (
          <div className="space-y-3 animate-pulse">
            <div className="h-4 bg-indigo-100/50 rounded w-3/4"></div>
            <div className="h-4 bg-indigo-100/50 rounded w-1/2"></div>
            <div className="h-4 bg-indigo-100/50 rounded w-5/6"></div>
            <div className="h-4 bg-indigo-100/50 rounded w-2/3"></div>
          </div>
        ) : (
          <div className="prose prose-sm max-w-none text-gray-700 prose-headings:text-indigo-900 prose-headings:font-semibold prose-a:text-indigo-600 prose-strong:text-indigo-800 prose-li:marker:text-indigo-400">
            {content ? (
              <ReactMarkdown remarkPlugins={[remarkGfm]} rehypePlugins={[rehypeSanitize]}>
                {content}
              </ReactMarkdown>
            ) : (
              <p className="text-gray-500 italic">Şu an gösterilecek bir özet bulunamadı.</p>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
