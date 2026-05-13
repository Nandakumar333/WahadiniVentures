import { toast } from "sonner"

// Simple wrapper around sonner to match the useToast API expected by components
export function useToast() {
  return {
    toast: (props: { title?: string; description?: string; variant?: "default" | "destructive" | "success" }) => {
        if (props.variant === 'destructive') {
            toast.error(props.title, { description: props.description });
        } else if (props.variant === 'success') {
            toast.success(props.title, { description: props.description });
        } else {
            toast.message(props.title, { description: props.description });
        }
    },
    dismiss: (id?: string) => toast.dismiss(id)
  }
}
