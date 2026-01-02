import 'package:application/screens/attended/attended_courses_state.dart';
import 'package:flutter/material.dart';

class AttendedCoursesList extends StatefulWidget {
  const AttendedCoursesList({Key? key}) : super(key: key);

  @override
  _AttendedCoursesListState createState() => _AttendedCoursesListState();
}

class _AttendedCoursesListState extends State<AttendedCoursesList> {
  final AttendedCoursesState _state = AttendedCoursesState();
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadAttendances();
  }

  Future<void> _loadAttendances() async {
    await _state.loadAttendances();
    setState(() {
      _isLoading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Center(child: CircularProgressIndicator());
    }

    final courses = _state.attendedCourses;

    return Scaffold(
      body:
          courses.isEmpty
              ? const Center(child: Text('No attended courses found.'))
              : ListView.builder(
                itemCount: courses.length,
                itemBuilder: (context, index) {
                  return ListTile(
                    title: Text(courses[index].courseName),
                    subtitle: Text(
                      'Marked at: ${courses[index].markedAtUtc.toLocal()}',
                    ),
                    leading: const Icon(Icons.check_circle, color: Colors.blue),
                  );
                },
              ),
    );
  }
}
