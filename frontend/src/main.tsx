import "./index.css";
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider } from "./contexts/AuthContext";
import { LoginPage } from "./pages/LoginPage";
import { RegisterPage } from "./pages/RegisterPage";
import { DashboardPage } from "./pages/DashboardPage";
import { SessionsPage } from "./pages/SessionsPage";
import { CreateSessionPage } from "./pages/CreateSessionPage";
import { SessionDetailPage } from "./pages/SessionDetailPage";
import SongsPage from "./pages/SongsPage";
import { ProtectedRoute } from "@/layouts/protected-route";
import { DashboardLayout } from "@/layouts/dashboard-layout";
import { Toaster } from "sonner";

const queryClient = new QueryClient();

const Providers = ({ children }: { children: React.ReactNode }) => {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>{children}</AuthProvider>
    </QueryClientProvider>
  );
};

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <Providers>
      <BrowserRouter>
        <Routes>
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="login" element={<LoginPage />} />
          <Route path="register" element={<RegisterPage />} />
          <Route path="songs" element={<SongsPage />} />
          <Route element={<ProtectedRoute />}>
            <Route element={<DashboardLayout />}>
              <Route path="dashboard" element={<DashboardPage />} />
              <Route path="sessions">
                <Route index element={<SessionsPage />} />
                <Route path="create" element={<CreateSessionPage />} />
                <Route path=":id" element={<SessionDetailPage />} />
              </Route>
            </Route>
          </Route>
        </Routes>
      </BrowserRouter>
    </Providers>
    <Toaster position="top-right" />
  </StrictMode>
);
