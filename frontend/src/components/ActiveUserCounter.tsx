import { Users } from "lucide-react";
import { useActiveUsers } from "../hooks/useActiveUsers";

export const ActiveUserCounter = () => {
  const { activeUserCount, isLoading, error } = useActiveUsers();

  if (isLoading) {
    return (
      <div className="flex items-center space-x-2 px-3 py-1 bg-gray-100 rounded-full">
        <Users className="w-4 h-4 text-gray-500" />
        <span className="text-sm text-gray-600">Loading...</span>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center space-x-2 px-3 py-1 bg-red-100 rounded-full">
        <Users className="w-4 h-4 text-red-500" />
        <span className="text-sm text-red-600">Error</span>
      </div>
    );
  }

  return (
    <div className="flex items-center space-x-2 px-3 py-1 bg-green-100 rounded-full">
      <Users className="w-4 h-4 text-green-600" />
      <span className="text-sm text-green-800 font-medium">
        {activeUserCount} active user{activeUserCount !== 1 ? 's' : ''}
      </span>
    </div>
  );
};