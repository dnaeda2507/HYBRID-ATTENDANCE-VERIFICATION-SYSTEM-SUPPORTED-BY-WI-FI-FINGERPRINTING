import SessionCreateForm from "@/components/attendance/SessionCreateForm";

export default function AttendancePage() {
  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Attendance</h1>
      <SessionCreateForm />
    </div>
  );
}
