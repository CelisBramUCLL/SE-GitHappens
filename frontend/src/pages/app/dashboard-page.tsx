import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
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
            <CardHeader className="relative">
              <CardTitle>Party {i + 1}</CardTitle>
              <CardDescription>
                <span>Public party</span>
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex -space-x-2 *:data-[slot=avatar]:grayscale pr-4 pb-4 *:data-[slot=avatar]:ring-2 *:data-[slot=avatar]:ring-background">
                <Avatar>
                  <AvatarImage
                    src="https://github.com/shadcn.png"
                    alt="@shadcn"
                  />
                  <AvatarFallback>CN</AvatarFallback>
                </Avatar>
                <Avatar>
                  <AvatarImage
                    src="https://github.com/maxleiter.png"
                    alt="@maxleiter"
                  />
                  <AvatarFallback>LR</AvatarFallback>
                </Avatar>
                <Avatar>
                  <AvatarImage
                    src="https://github.com/evilrabbit.png"
                    alt="@evilrabbit"
                  />
                  <AvatarFallback>ER</AvatarFallback>
                </Avatar>
              </div>
              <Button>Join</Button>
            </CardContent>
          </Card>
        ))}
      </div>
    </main>
  );
}
