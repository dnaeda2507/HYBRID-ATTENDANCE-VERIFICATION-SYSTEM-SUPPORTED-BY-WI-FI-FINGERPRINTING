import CoursesList from "@/components/courses/CoursesList";

export default function CoursesPage() {
  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Courses</h1>
      <CoursesList />
    </div>
  );
}