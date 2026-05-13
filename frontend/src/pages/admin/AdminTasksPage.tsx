// import React from 'react';
// import { Layout } from '@/components/layout/Layout'; // Layout not used yet
import { SubmissionsTable } from '@/components/admin/SubmissionsTable';

export function AdminTasksPage() {
  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold mb-6">Task Review Queue</h1>
      <SubmissionsTable />
    </div>
  );
}
