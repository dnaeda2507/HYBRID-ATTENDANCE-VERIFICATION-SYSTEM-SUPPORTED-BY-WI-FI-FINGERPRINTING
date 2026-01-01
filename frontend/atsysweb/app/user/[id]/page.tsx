import { UserForm } from "@/components/users/UserForm";
import React from "react";

interface Props {
  params: { id: string };
}

export default function EditUserPage({ params: { id } }: Props) {
  return <UserForm userId={id} />;
}
