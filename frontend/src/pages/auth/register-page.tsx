import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useAuth } from "@/contexts/auth";
import { Navigate } from "react-router-dom";

export default function RegisterPage() {
  const auth = useAuth();

  if (auth.isAuthenticated) {
    <Navigate to="/" replace />;
  }

  return (
    <div className="flex flex-col justify-center items-center gap-6 bg-muted p-6 md:p-10 min-h-svh">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle className="text-xl">Register</CardTitle>
        </CardHeader>
        <CardContent>
          <form>
            <div className="gap-6 grid">
              <div className="gap-3 grid">
                <Label htmlFor="email">Email</Label>
                <Input
                  id="email"
                  type="email"
                  placeholder="m@example.com"
                  required
                />
              </div>
              <div className="gap-3 grid">
                <div className="flex items-center">
                  <Label htmlFor="password">Password</Label>
                </div>
                <Input id="password" type="password" required />
              </div>
              <Button type="submit" className="w-full">
                Login
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
