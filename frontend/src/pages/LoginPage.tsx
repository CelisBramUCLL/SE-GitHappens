import React, { useState } from "react";
import { Link, useNavigate, useLocation } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { Button } from "../components/ui/button";
import { Music } from "lucide-react";
import { tryCatch } from "@/lib/utils";

export const LoginPage: React.FC = () => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");

  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const from = location.state?.from?.pathname || "/dashboard";

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError("");

    const { error: loginError } = await tryCatch(login({ username, password }));

    setIsLoading(false);

    if (loginError) {
      setError(loginError.message ?? "Login failed");
      return;
    }

    await login({ username, password });
    navigate(from, { replace: true });
  };

  return (
    <div className="flex justify-center items-center bg-gray-50 px-4 sm:px-6 lg:px-8 py-12 min-h-screen">
      <div className="space-y-8 w-full max-w-md">
        <div>
          <div className="flex justify-center items-center mx-auto w-12 h-12">
            <Music className="w-12 h-12 text-blue-600" />
          </div>
          <h2 className="mt-6 font-extrabold text-gray-900 text-3xl text-center">
            Sign in to GitHappens
          </h2>
          <p className="mt-2 text-gray-600 text-sm text-center">
            Or{" "}
            <Link
              to="/register"
              className="font-medium text-blue-600 hover:text-blue-500"
            >
              create a new account
            </Link>
          </p>
        </div>
        <form className="space-y-6 mt-8" onSubmit={handleSubmit}>
          <div className="-space-y-px shadow-sm rounded-md">
            <div>
              <label htmlFor="username" className="sr-only">
                Username
              </label>
              <input
                id="username"
                name="username"
                type="text"
                required
                className="block focus:z-10 relative px-3 py-1.5 border-0 rounded-t-md ring-1 ring-gray-300 focus:ring-2 focus:ring-blue-600 ring-inset focus:ring-inset w-full text-gray-900 placeholder:text-gray-400 sm:text-sm sm:leading-6"
                placeholder="Username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
              />
            </div>
            <div>
              <label htmlFor="password" className="sr-only">
                Password
              </label>
              <input
                id="password"
                name="password"
                type="password"
                required
                className="block focus:z-10 relative px-3 py-1.5 border-0 rounded-b-md ring-1 ring-gray-300 focus:ring-2 focus:ring-blue-600 ring-inset focus:ring-inset w-full text-gray-900 placeholder:text-gray-400 sm:text-sm sm:leading-6"
                placeholder="Password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </div>
          </div>

          {error && (
            <div className="bg-red-50 p-4 rounded-md">
              <div className="text-red-700 text-sm">{error}</div>
            </div>
          )}

          <div>
            <Button
              type="submit"
              disabled={isLoading}
              className="group relative flex justify-center bg-blue-600 hover:bg-blue-700 disabled:opacity-50 px-4 py-2 border border-transparent rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 w-full font-medium text-white text-sm"
            >
              {isLoading ? "Signing in..." : "Sign in"}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
