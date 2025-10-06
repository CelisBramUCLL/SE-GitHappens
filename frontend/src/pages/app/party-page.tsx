import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

export default function DashboardPage() {
  return (
    <main className="flex flex-col gap-6 p-6">
      <Card className="@container/card">
        <CardHeader>
          <CardTitle>Parties</CardTitle>
          <CardDescription>
            <span>
              Create, manage, and invite your friends to join your party.
            </span>
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex gap-2">
            <Button>Create a party</Button>
            <Button>Join a party</Button>
          </div>
        </CardContent>
      </Card>
      <div className="gap-6 grid grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <Card key={i} className="@container/card">
            <CardHeader>
              <CardTitle>Party {i + 1}</CardTitle>
              <CardDescription>
                <span>Public party</span>
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Button>Join</Button>
            </CardContent>
          </Card>
        ))}
      </div>
    </main>
  );
}
