import { RouterProvider } from 'react-router-dom';
import { Toaster } from 'sonner';
import { router } from './routes/AppRoutes';
import { QueryProvider } from './providers/QueryProvider';
import { ThemeProvider } from './providers/ThemeProvider';
import './App.css';

function App() {
  return (
    <ThemeProvider>
      <QueryProvider>
        <Toaster 
          position="top-right" 
          richColors 
          closeButton
          toastOptions={{
            duration: 4000,
          }}
        />
        <RouterProvider router={router} />
      </QueryProvider>
    </ThemeProvider>
  );
}

export default App;
