import 'package:flutter/material.dart';
import 'package:application/models/lecture.dart';



class LectureDetailsPage extends StatelessWidget {
  final LectureDetails lectureDetails;

  const LectureDetailsPage({Key? key, required this.lectureDetails}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text("Lecture Details"),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _buildDetailCard(
              icon: Icons.book,
              label: 'Lecture Name',
              value: lectureDetails.lectureName,
            ),
            _buildDetailCard(
              icon: Icons.person,
              label: 'Teacher',
              value: lectureDetails.teacherName,
            ),
            _buildDetailCard(
              icon: Icons.account_tree_outlined,
              label: 'Department',
              value: lectureDetails.departmentName,
            ),
            _buildDetailCard(
              icon: Icons.calendar_today,
              label: 'Day',
              value: lectureDetails.dayOfWeek,
            ),
            _buildDetailCard(
              icon: Icons.access_time,
              label: 'Start Time',
              value: lectureDetails.startTime,
            ),
            _buildDetailCard(
              icon: Icons.location_on,
              label: 'Location',
              value: lectureDetails.location,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildDetailCard({
    required IconData icon,
    required String label,
    required String value,
  }) {
    return Card(
      margin: const EdgeInsets.symmetric(vertical: 8.0),
      elevation: 3,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: ListTile(
        leading: Icon(icon, color: Colors.blueAccent),
        title: Text(
          label,
          style: const TextStyle(
            fontWeight: FontWeight.bold,
            fontSize: 14,
          ),
        ),
        subtitle: Text(
          value,
          style: const TextStyle(fontSize: 16),
        ),
      ),
    );
  }
}