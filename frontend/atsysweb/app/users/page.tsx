import UserList from "@/components/users/UserList";

export default function UsersPage() {
  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold text-gray-800">Users</h1>
      <UserList />
    </div>
  );
}
