import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import type { ChartPointDto } from '../../types/admin.types';

interface TrendChartProps {
  data: ChartPointDto[];
  title: string;
  color?: string;
  valuePrefix?: string;
  loading?: boolean;
}

/**
 * Trend Chart component for displaying 30-day time series data
 * T035: Admin Dashboard trend visualization using Recharts
 */
export default function TrendChart({ 
  data, 
  title, 
  color = '#8b5cf6', 
  valuePrefix = '',
  loading 
}: Readonly<TrendChartProps>) {
  if (loading) {
    return (
      <div className="bg-white rounded-lg shadow p-6">
        <div className="h-6 bg-gray-200 rounded w-48 mb-4 animate-pulse"></div>
        <div className="h-64 bg-gray-100 rounded animate-pulse"></div>
      </div>
    );
  }

  // Transform data for Recharts (expects objects with string keys)
  const chartData = data.map(point => ({
    date: new Date(point.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
    value: Number(point.value)
  }));

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h3 className="text-lg font-semibold text-gray-900 mb-4">{title}</h3>
      
      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={chartData} margin={{ top: 5, right: 20, left: 10, bottom: 5 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
          <XAxis 
            dataKey="date" 
            tick={{ fontSize: 12 }}
            stroke="#6b7280"
          />
          <YAxis 
            tick={{ fontSize: 12 }}
            stroke="#6b7280"
          />
          <Tooltip
            contentStyle={{
              backgroundColor: '#fff',
              border: '1px solid #e5e7eb',
              borderRadius: '0.5rem',
              padding: '8px 12px'
            }}
            formatter={(value: any) => {
              const formatted = `${valuePrefix}${Number(value).toLocaleString()}`;
              return formatted;
            }}
            labelStyle={{ fontWeight: 600, marginBottom: '4px' }}
          />
          <Line 
            type="monotone" 
            dataKey="value" 
            stroke={color}
            strokeWidth={2}
            dot={{ fill: color, r: 3 }}
            activeDot={{ r: 5 }}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
