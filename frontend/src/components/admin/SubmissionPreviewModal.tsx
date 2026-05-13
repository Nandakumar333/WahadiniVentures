import { useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import type { UserTaskSubmission } from '@/types/task';
import { adminSubmissionService } from '@/services/api/adminSubmissionService';
import { useToast } from '@/components/ui/use-toast';

interface SubmissionPreviewModalProps {
  submission: UserTaskSubmission;
  isOpen: boolean;
  onClose: () => void;
  onReviewComplete: () => void;
}

export function SubmissionPreviewModal({ submission, isOpen, onClose, onReviewComplete }: SubmissionPreviewModalProps) {
  const { toast } = useToast();
  const [feedback, setFeedback] = useState('');
  const [points, setPoints] = useState(10); // Default points, should come from Task definition ideally
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleReview = async (approved: boolean) => {
    setIsSubmitting(true);
    try {
      // Version needs to be passed from backend to frontend to support optimistic locking
      // Assuming submission object has version field (add to type definition if missing)
      await adminSubmissionService.reviewSubmission(submission.id, {
        isApproved: approved,
        feedback,
        pointsAwarded: approved ? points : 0,
        version: (submission as any).version || '' 
      });
      toast({ title: "Success", description: `Submission ${approved ? 'approved' : 'rejected'}` });
      onReviewComplete();
    } catch (error: any) {
        if (error.response?.status === 409) {
            toast({ title: "Error", description: "This submission has been modified by another admin.", variant: "destructive" });
        } else {
            toast({ title: "Error", description: "Review failed", variant: "destructive" });
        }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle>Review Submission</DialogTitle>
        </DialogHeader>
        <div className="py-4 space-y-4">
          <div className="bg-slate-100 p-4 rounded-md overflow-auto max-h-[200px]">
            <pre className="text-xs whitespace-pre-wrap">{JSON.stringify(JSON.parse(submission.submissionData), null, 2)}</pre>
          </div>
          
          <div className="space-y-2">
            <label className="text-sm font-medium">Feedback</label>
            <Textarea 
              placeholder="Enter feedback for the user..." 
              value={feedback}
              onChange={(e) => setFeedback(e.target.value)}
            />
          </div>

          <div className="space-y-2">
             <label className="text-sm font-medium">Points to Award</label>
             <input 
                type="number" 
                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                value={points}
                onChange={(e) => setPoints(parseInt(e.target.value))}
             />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={onClose} disabled={isSubmitting}>Cancel</Button>
          <Button variant="destructive" onClick={() => handleReview(false)} disabled={isSubmitting}>Reject</Button>
          <Button onClick={() => handleReview(true)} disabled={isSubmitting}>Approve</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
