import api from '../../api/axios';
import type {
  DashboardSummaryDto,
  DashboardTrendsDto,
  DashboardRecentActivityDto,
  DashboardCriticalStockDto,
  DashboardTopRecordsDto
} from './types';

export const getDashboardSummary = async (startDate?: string, endDate?: string): Promise<DashboardSummaryDto> => {
  const response = await api.get('/api/dashboard/summary', { params: { startDate, endDate } });
  return response.data.data;
};

export const getDashboardTrends = async (startDate?: string, endDate?: string): Promise<DashboardTrendsDto[]> => {
  const response = await api.get('/api/dashboard/trends', { params: { startDate, endDate } });
  return response.data.data;
};

export const getRecentActivities = async (): Promise<DashboardRecentActivityDto[]> => {
  const response = await api.get('/api/dashboard/recent-activities');
  return response.data.data;
};

export const getCriticalStock = async (): Promise<DashboardCriticalStockDto[]> => {
  const response = await api.get('/api/dashboard/critical-stock');
  return response.data.data;
};

export const getTopRecords = async (startDate?: string, endDate?: string): Promise<DashboardTopRecordsDto> => {
  const response = await api.get('/api/dashboard/top-records', { params: { startDate, endDate } });
  return response.data.data;
};
