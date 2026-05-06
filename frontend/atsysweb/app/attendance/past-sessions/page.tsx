import TeacherPastSessions from "@/components/attendance/TeacherPastSessions";

export default function PastSessionsPage() {
  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Past Attendance Sessions</h1>
      <TeacherPastSessions />
    </div>
  );
}
