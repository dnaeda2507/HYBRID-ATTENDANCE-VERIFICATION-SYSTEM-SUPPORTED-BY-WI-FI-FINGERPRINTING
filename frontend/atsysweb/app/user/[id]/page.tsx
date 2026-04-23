import { UserForm } from "@/components/users/UserForm";

interface PageProps {
  params: Promise<{ id: string }>;
}

export default async function Page({ params }: PageProps) {
  const { id } = await params;
  return <UserForm userId={id} />;
}
