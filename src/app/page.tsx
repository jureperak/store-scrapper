import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
} from "@/components/ui/card";
import {
  Table,
  TableHeader,
  TableRow,
  TableHead,
  TableBody,
  TableCell,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuLabel,
  DropdownMenuItem,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";

export default function Component() {
  return (
    <div className="flex min-h-screen w-full bg-muted/40">
      <div className="flex flex-col sm:gap-4 sm:py-4 sm:pl-14 w-full">
        <header className="sticky top-0 z-30 flex h-14 items-center gap-4 border-b bg-background px-4 sm:static sm:h-auto sm:border-0 sm:bg-transparent sm:px-6">
          <h1 className="text-xl font-bold">Administracija</h1>
        </header>
        <main className="flex-1 p-4 sm:px-6 sm:py-0 md:gap-8">
          <div className="grid gap-8">
            <div className="grid gap-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="article">Artikl</Label>
                  <Input id="article" placeholder="Unesi ime artikla" />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="availabilityUrl">Availability URL</Label>
                  <Input
                    id="availabilityUrl"
                    placeholder="Unesi availability URL"
                    type="url"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="articleUrl">Link artikla</Label>
                  <Input
                    id="articleUrl"
                    placeholder="Unesi link artikla"
                    type="url"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="skuNeeded">Potreban SKU</Label>
                  <Input
                    id="skuNeeded"
                    placeholder="Unesi potreban SKU"
                    type="text"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="active">
                    <div className="flex items-center space-x-2">
                      <Checkbox id="active" />
                      <span>Aktivno</span>
                    </div>
                  </Label>
                </div>
              </div>
            </div>
            <Card>
              <CardHeader>
                <CardTitle>Stranice</CardTitle>
                <CardDescription>
                  Upravljaj stranicama i linkovima.
                </CardDescription>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Artikl</TableHead>
                      <TableHead>Potreban SKU</TableHead>
                      <TableHead>Status</TableHead>
                      <TableHead>
                        <span className="sr-only">Akcije</span>
                      </TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    <TableRow>
                      <TableCell className="font-medium">
                        kaput-od-manteco-vune-s-prorezima-zw
                      </TableCell>
                      <TableCell>322543084</TableCell>
                      <TableCell>
                        <Badge variant="default">Aktivno</Badge>
                      </TableCell>
                      <TableCell>
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button
                              aria-haspopup="true"
                              size="icon"
                              variant="ghost"
                            >
                              <MoveVerticalIcon className="h-4 w-4" />
                              <span className="sr-only">Toggle menu</span>
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuLabel>Akcije</DropdownMenuLabel>
                            <DropdownMenuItem>Uredi</DropdownMenuItem>
                            <DropdownMenuItem>Deaktiviraj</DropdownMenuItem>
                            <DropdownMenuItem>Obriši</DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          </div>
        </main>
      </div>
    </div>
  );
}

function MoveVerticalIcon(props: React.SVGProps<SVGSVGElement>) {
  return (
    <svg
      {...props}
      xmlns="http://www.w3.org/2000/svg"
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <polyline points="8 18 12 22 16 18" />
      <polyline points="8 6 12 2 16 6" />
      <line x1="12" x2="12" y1="2" y2="22" />
    </svg>
  );
}
