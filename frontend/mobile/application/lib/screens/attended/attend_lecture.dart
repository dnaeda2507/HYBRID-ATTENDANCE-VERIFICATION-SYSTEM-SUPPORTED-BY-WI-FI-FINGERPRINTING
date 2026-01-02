import 'package:application/screens/attended/qr_view.dart';
import 'package:application/services/attendance_service.dart';
import 'package:dio/dio.dart';
import 'package:flutter/material.dart';

class AttendedLecture extends StatelessWidget {
  final String lectureTitle;

  const AttendedLecture({Key? key, required this.lectureTitle})
    : super(key: key);

  void _showSuccessDialog(BuildContext context, Response<dynamic> response) {
    showDialog(
      context: context,
      builder:
          (_) => AlertDialog(
            title: const Text('Success'),
            content: Text(response.data['message']),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(),
                child: const Text('OK'),
              ),
            ],
          ),
    );
  }

  void _showErrorDialog(BuildContext context, Response<dynamic> response) {
    String errorMessage = response.data['message'] ?? 'An error occurred';
    if (response.data['errors'] != null) {
      errorMessage += '\n${response.data['errors'].join(', ')}';
    }
    showDialog(
      context: context,
      builder:
          (_) => AlertDialog(
            title: const Text('Error'),
            content: Text(errorMessage),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(),
                child: const Text('OK'),
              ),
            ],
          ),
    );
  }

  void _askForPassword(BuildContext context) {
    final TextEditingController passwordController = TextEditingController();

    showDialog(
      context: context,
      builder: (dialogContext) {
        return AlertDialog(
          title: Text("Enter Attendance Password"),
          content: TextField(controller: passwordController),
          actions: [
            TextButton(
              onPressed: () async {
                Navigator.of(context).pop();

                final input = passwordController.text.trim();
                if (input.isEmpty || !input.contains(':')) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text("Invalid input format")),
                  );
                  return;
                }
                final parts = input.split(':');
                final sessionId = int.tryParse(parts[0]) ?? 0;
                final token = parts[1];

                final response = await AttendanceService().sendAttendance(
                  sessionId: sessionId,
                  tokenFromQR: token,
                );

                if (response.data['success'] == true) {
                  _showSuccessDialog(context, response);
                } else {
                  _showErrorDialog(context, response);
                }
              },
              child: Text("OK"),
            ),
          ],
        );
      },
    );
  }

  void _openQRSystem(BuildContext context) async {
    final result = await Navigator.push(
      context,
      MaterialPageRoute(builder: (context) => QRViewPage()),
    );

    if (result != null && result.contains(':')) {
      final parts = result.split(':');
      final sessionId = int.tryParse(parts[0]);
      final token = parts[1];

      if (sessionId != null && token.isNotEmpty) {
        final response = await AttendanceService().sendAttendance(
          sessionId: sessionId,
          tokenFromQR: token,
        );

        if (response.data['success'] == true) {
          _showSuccessDialog(context, response);
        } else {
          _showErrorDialog(context, response);
        }
      } else {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(const SnackBar(content: Text("Invalid QR format")));
      }
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("QR code could not be read.")),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(lectureTitle)),
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              SizedBox(
                width: 340,
                height: 70,
                child: ElevatedButton.icon(
                  onPressed: () => _askForPassword(context),
                  icon: Icon(Icons.lock, color: Colors.blue),
                  label: Text(
                    "Attend using a password",
                    style: TextStyle(
                      fontSize: 18,
                      color: Colors.blue,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  style: ElevatedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 24,
                      vertical: 16,
                    ),
                    backgroundColor: Colors.white,
                    side: BorderSide(color: Colors.blue),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 20),
              SizedBox(
                width: 340,
                height: 70,
                child: ElevatedButton.icon(
                  onPressed: () => _openQRSystem(context),
                  icon: Icon(Icons.qr_code_scanner, color: Colors.blue),
                  label: Text(
                    "Scan QR Code",
                    style: TextStyle(
                      fontSize: 18,
                      color: Colors.blue,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  style: ElevatedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 24,
                      vertical: 16,
                    ),
                    backgroundColor: Colors.white,
                    side: BorderSide(color: Colors.blue),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
