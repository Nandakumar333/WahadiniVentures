import { useEffect, useState } from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Checkbox } from '@/components/ui/checkbox';
import { Button } from "@/components/ui/button";
import { adminSubmissionService } from '@/services/api/adminSubmissionService';
import type { UserTaskSubmission } from '@/types/task';
import { SubmissionPreviewModal } from './SubmissionPreviewModal';
import { useToast } from '@/components/ui/use-toast';

export function SubmissionsTable() {
  const { toast } = useToast();
  const [submissions, setSubmissions] = useState<UserTaskSubmission[]>([]);
  const [selectedSubmission, setSelectedSubmission] = useState<UserTaskSubmission | null>(null);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [isLoading, setIsLoading] = useState(true);

  const fetchSubmissions = async () => {
    setIsLoading(true);
    try {
      const result = await adminSubmissionService.getPendingSubmissions();
      setSubmissions(result?.items || []);
      setSelectedIds(new Set()); // Reset selection on refresh
    } catch (error) {
      console.error("Failed to fetch submissions", error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchSubmissions();
  }, []);

  const handleReviewComplete = () => {
    setSelectedSubmission(null);
    fetchSubmissions();
  };

  const toggleSelection = (id: string) => {
    const newSelected = new Set(selectedIds);
    if (newSelected.has(id)) {
      newSelected.delete(id);
    } else {
      newSelected.add(id);
    }
    setSelectedIds(newSelected);
  };

  const toggleAll = () => {
    if (selectedIds.size === submissions.length) {
      setSelectedIds(new Set());
    } else {
      setSelectedIds(new Set(submissions.map(s => s.id)));
    }
  };

  const handleBulkAction = async (approve: boolean) => {
    if (selectedIds.size === 0) return;
    try {
      await adminSubmissionService.bulkReview(
        Array.from(selectedIds), 
        approve, 
        approve ? "Bulk Approved" : "Bulk Rejected", 
        approve ? 10 : 0
      );
      toast({ title: "Success", description: `Bulk action completed` });
      fetchSubmissions();
    } catch (error) {
      toast({ title: "Error", description: "Bulk action failed", variant: "destructive" });
    }
  };

  if (isLoading) return <div>Loading...</div>;

  return (
    <>
      <div className="mb-4 flex justify-between items-center">
        <div className="space-x-2">
          <Button 
            variant="outline" 
            disabled={selectedIds.size === 0}
            onClick={() => handleBulkAction(true)}
          >
            Approve Selected ({selectedIds.size})
          </Button>
          <Button 
            variant="destructive" 
            disabled={selectedIds.size === 0}
            onClick={() => handleBulkAction(false)}
          >
            Reject Selected ({selectedIds.size})
          </Button>
        </div>
      </div>

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-[50px]">
                <Checkbox 
                  checked={submissions.length > 0 && selectedIds.size === submissions.length}
                  onCheckedChange={toggleAll}
                />
              </TableHead>
              <TableHead>Submitted At</TableHead>
              <TableHead>User ID</TableHead>
              <TableHead>Task ID</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {submissions.map((submission) => (
              <TableRow key={submission.id}>
                <TableCell>
                  <Checkbox 
                    checked={selectedIds.has(submission.id)}
                    onCheckedChange={() => toggleSelection(submission.id)}
                  />
                </TableCell>
                <TableCell>{new Date(submission.submittedAt).toLocaleString()}</TableCell>
                <TableCell>{submission.userId}</TableCell>
                <TableCell>{submission.taskId}</TableCell>
                <TableCell>{submission.status}</TableCell>
                <TableCell className="text-right">
                  <Button variant="outline" size="sm" onClick={() => setSelectedSubmission(submission)}>
                    Review
                  </Button>
                </TableCell>
              </TableRow>
            ))}
            {submissions.length === 0 && (
              <TableRow>
                <TableCell colSpan={6} className="text-center">No pending submissions</TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>

      {selectedSubmission && (
        <SubmissionPreviewModal
          submission={selectedSubmission}
          isOpen={!!selectedSubmission}
          onClose={() => setSelectedSubmission(null)}
          onReviewComplete={handleReviewComplete}
        />
      )}
    </>
  );
}