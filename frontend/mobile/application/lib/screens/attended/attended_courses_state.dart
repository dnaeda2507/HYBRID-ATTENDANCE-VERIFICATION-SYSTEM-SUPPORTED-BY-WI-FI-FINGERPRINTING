import 'package:application/services/attendance_service.dart';

class AttendedCourseClass {
  final String courseName;
  final DateTime markedAtUtc;

  AttendedCourseClass({required this.courseName, required this.markedAtUtc});
}

class AttendedCoursesState {
  final List<AttendedCourseClass> attendedCourses = [];

  Future<void> loadAttendances() async {
    try {
      final attendanceRecords =
          await AttendanceService().fetchUserAttendances();
      attendedCourses.clear();
      attendedCourses.addAll(
        attendanceRecords.map(
          (e) => AttendedCourseClass(
            courseName: e.courseName,
            markedAtUtc: e.markedAtUtc,
          ),
        ),
      );
    } catch (e) {
      print('Katılım listesi alınamadı: $e');
    }
  }

  void addCourse(AttendedCourseClass attended) {
    if (!attendedCourses.contains(attended)) {
      attendedCourses.add(attended);
    }
  }
}
