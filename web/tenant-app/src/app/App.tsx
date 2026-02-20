import { BrowserRouter } from 'react-router-dom';
import { AppProviders } from './providers';
import { AppRouter } from './router';
import { Toaster } from 'sonner';

export function App() {
  return (
    <BrowserRouter>
      <AppProviders>
        <AppRouter />
        <Toaster position="top-center" richColors closeButton />
      </AppProviders>
    </BrowserRouter>
  );
}
