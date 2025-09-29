import { useAuth } from "@/contexts/auth";
import { Navigate, Outlet } from "react-router-dom";

export default function ProtectedLayout() {
  const auth = useAuth();

  console.log("ProtectedLayout auth:", auth);

  if (!auth.isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  return <Outlet />;
}
