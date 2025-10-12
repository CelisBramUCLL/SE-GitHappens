import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from '../contexts/AuthContext';
import { ToastProvider } from '../contexts/ToastContext';
import { ProtectedRoute } from '../components/ProtectedRoute';
import { LoginPage } from './LoginPage';
import { RegisterPage } from './RegisterPage';
import { DashboardPage } from './DashboardPage';
import { SessionsPage } from './SessionsPage';
import { CreateSessionPage } from './CreateSessionPage';
import { SessionDetailPage } from './SessionDetailPage';
import SongsPage from './SongsPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1, 						// If API call fails, try 1 more time
      refetchOnWindowFocus: false, 		// Don't reload data when switching browser tabs
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}> 		{/* API data management */}
      <AuthProvider> 								{/* Login/logout state */}
        <ToastProvider> 							{/* Notifications */}
          <Router> 									{/* Navigation */}
            <Routes> 								{/* Route definitions */}
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route
                path="/dashboard"
                element={
                  <ProtectedRoute>
                    <DashboardPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/sessions"
                element={
                  <ProtectedRoute>
                    <SessionsPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/sessions/create"
                element={
                  <ProtectedRoute>
                    <CreateSessionPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/sessions/:id"
                element={
                  <ProtectedRoute>
                    <SessionDetailPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/songs"
                element={
                  <ProtectedRoute>
                    <SongsPage />
                  </ProtectedRoute>
                }
              />
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
            </Routes>
          </Router>
        </ToastProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
