import { useState } from "react";
import { Button } from "../components/ui/button";
import { useQuery } from "@tanstack/react-query";

type Todo = {
  id: string;
  description: string;
};

function App() {
  const [count, setCount] = useState(0);

  const todosQuery = useQuery<Todo[]>({
    queryKey: ["todos"], // unique cache key
    queryFn: async () => {
      const response = await fetch("/api/v1/todos");
      if (!response.ok) {
        throw new Error("Failed to fetch todos");
      }
      return response.json();
    },
  });

  return (
    <>
      <h1>GitHappens</h1>
      <div>
        <Button onClick={() => setCount((count) => count + 1)}>
          count is {count}
        </Button>
      </div>
      {todosQuery.data?.map((todo) => {
        return <div key={todo.id}>{todo.description}</div>;
      })}
    </>
  );
}

export default App;
