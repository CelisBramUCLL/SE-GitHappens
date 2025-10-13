import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { Button } from "../components/ui/button";
import { Music } from "lucide-react";
import type { CreateUserDTO } from "../types";
import { tryCatch } from "@/lib/utils";

export const RegisterPage: React.FC = () => {
  const [formData, setFormData] = useState<CreateUserDTO>({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    username: "",
    role: "User",
  });
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");

  const { register } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError("");

    if (formData.password !== confirmPassword) {
      setError("Passwords do not match");
      setIsLoading(false);
      return;
    }

    const { error: registerError } = await tryCatch(register(formData));

    setIsLoading(false);

    if (registerError) {
      setError(registerError.message ?? "Registration failed");
      return;
    }

    await register(formData);
    navigate("/dashboard");
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prev) => ({
      ...prev,
      [e.target.name]: e.target.value,
    }));
  };

  return (
    <div className="flex justify-center items-center bg-gray-50 px-4 sm:px-6 lg:px-8 py-12 min-h-screen">
      <div className="space-y-8 w-full max-w-md">
        <div>
          <div className="flex justify-center items-center mx-auto w-12 h-12">
            <Music className="w-12 h-12 text-blue-600" />
          </div>
          <h2 className="mt-6 font-extrabold text-gray-900 text-3xl text-center">
            Create your account
          </h2>
          <p className="mt-2 text-gray-600 text-sm text-center">
            Or{" "}
            <Link
              to="/login"
              className="font-medium text-blue-600 hover:text-blue-500"
            >
              sign in to your existing account
            </Link>
          </p>
        </div>
        <form className="space-y-6 mt-8" onSubmit={handleSubmit}>
          <div className="space-y-4">
            <div className="flex space-x-4">
              <div className="flex-1">
                <label
                  htmlFor="firstName"
                  className="block font-medium text-gray-700 text-sm"
                >
                  First Name
                </label>
                <input
                  id="firstName"
                  name="firstName"
                  type="text"
                  required
                  className="block shadow-sm mt-1 px-3 py-2 border-gray-300 focus:border-blue-500 rounded-md focus:ring-blue-500 w-full"
                  value={formData.firstName}
                  onChange={handleChange}
                />
              </div>
              <div className="flex-1">
                <label
                  htmlFor="lastName"
                  className="block font-medium text-gray-700 text-sm"
                >
                  Last Name
                </label>
                <input
                  id="lastName"
                  name="lastName"
                  type="text"
                  required
                  className="block shadow-sm mt-1 px-3 py-2 border-gray-300 focus:border-blue-500 rounded-md focus:ring-blue-500 w-full"
                  value={formData.lastName}
                  onChange={handleChange}
                />
              </div>
            </div>

            <div>
              <label
                htmlFor="username"
                className="block font-medium text-gray-700 text-sm"
              >
                Username
              </label>
              <input
                id="username"
                name="username"
                type="text"
                required
                className="block shadow-sm mt-1 px-3 py-2 border-gray-300 focus:border-blue-500 rounded-md focus:ring-blue-500 w-full"
                value={formData.username}
                onChange={handleChange}
              />
            </div>

            <div>
              <label
                htmlFor="email"
                className="block font-medium text-gray-700 text-sm"
              >
                Email
              </label>
              <input
                id="email"
                name="email"
                type="email"
                required
                className="block shadow-sm mt-1 px-3 py-2 border-gray-300 focus:border-blue-500 rounded-md focus:ring-blue-500 w-full"
                value={formData.email}
                onChange={handleChange}
              />
            </div>

            <div>
              <label
                htmlFor="password"
                className="block font-medium text-gray-700 text-sm"
              >
                Password
              </label>
              <input
                id="password"
                name="password"
                type="password"
                required
                className="block shadow-sm mt-1 px-3 py-2 border-gray-300 focus:border-blue-500 rounded-md focus:ring-blue-500 w-full"
                value={formData.password}
                onChange={handleChange}
              />
            </div>

            <div>
              <label
                htmlFor="confirmPassword"
                className="block font-medium text-gray-700 text-sm"
              >
                Confirm Password
              </label>
              <input
                id="confirmPassword"
                name="confirmPassword"
                type="password"
                required
                className="block shadow-sm mt-1 px-3 py-2 border-gray-300 focus:border-blue-500 rounded-md focus:ring-blue-500 w-full"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
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
              {isLoading ? "Creating account..." : "Create account"}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
