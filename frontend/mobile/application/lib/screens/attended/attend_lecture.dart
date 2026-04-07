import 'package:application/screens/attended/qr_view.dart';
import 'package:application/services/attendance_service.dart';
import 'package:application/services/wifi_service.dart';
import 'package:flutter/material.dart';

class AttendedLecture extends StatefulWidget {
  final String lectureTitle;
  final int courseId;

  const AttendedLecture({
    Key? key,
    required this.lectureTitle,
    required this.courseId,
  }) : super(key: key);

  @override
  State<AttendedLecture> createState() => _AttendedLectureState();
}

class _AttendedLectureState extends State<AttendedLecture> {
  bool _wifiLoading = false;

  void _showSuccessDialog(String message) {
    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Başarılı ✓'),
        content: Text(message),
        actions: [TextButton(onPressed: () => Navigator.of(context).pop(), child: const Text('Tamam'))],
      ),
    );
  }

  void _showErrorDialog(String message) {
    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Hata'),
        content: Text(message),
        actions: [TextButton(onPressed: () => Navigator.of(context).pop(), child: const Text('Tamam'))],
      ),
    );
  }

  void _askForPassword() {
    final TextEditingController passwordController = TextEditingController();
    showDialog(
      context: context,
      builder: (dialogContext) {
        return AlertDialog(
          title: const Text("Enter Attendance Password"),
          content: TextField(controller: passwordController),
          actions: [
            TextButton(
              onPressed: () async {
                Navigator.of(context).pop();
                final input = passwordController.text.trim();
                if (input.isEmpty || !input.contains(':')) {
                  ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Invalid input format")));
                  return;
                }
                final parts = input.split(':');
                final sessionId = int.tryParse(parts[0]) ?? 0;
                final token = parts[1];
                final response = await AttendanceService().sendAttendance(sessionId: sessionId, tokenFromQR: token);
                if (response.data['success'] == true) {
                  _showSuccessDialog(response.data['message']);
                } else {
                  _showErrorDialog(response.data['message'] ?? 'Hata oluştu');
                }
              },
              child: const Text("OK"),
            ),
          ],
        );
      },
    );
  }

  void _openQRSystem() async {
    final result = await Navigator.push(context, MaterialPageRoute(builder: (context) => const QRViewPage()));
    if (result != null && result.contains(':')) {
      final parts = result.split(':');
      final sessionId = int.tryParse(parts[0]);
      final token = parts[1];
      if (sessionId != null && token.isNotEmpty) {
        final response = await AttendanceService().sendAttendance(sessionId: sessionId, tokenFromQR: token);
        if (response.data['success'] == true) {
          _showSuccessDialog(response.data['message']);
        } else {
          _showErrorDialog(response.data['message'] ?? 'Hata oluştu');
        }
      }
    }
  }

  Future<void> _wifiCheckIn() async {
    setState(() => _wifiLoading = true);
    try {
      final result = await WifiService().checkIn();
      if (result.success) {
        _showSuccessDialog(result.message);
      } else {
        _showErrorDialog(result.message);
      }
    } catch (e) {
      _showErrorDialog('Beklenmeyen hata: $e');
    } finally {
      setState(() => _wifiLoading = false);
    }
  }

  Widget _buildButton({required VoidCallback? onPressed, required IconData icon, required String label, bool isLoading = false}) {
    return SizedBox(
      width: 340,
      height: 70,
      child: ElevatedButton.icon(
        onPressed: onPressed,
        icon: isLoading
            ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.blue))
            : Icon(icon, color: Colors.blue),
        label: Text(label, style: const TextStyle(fontSize: 18, color: Colors.blue, fontWeight: FontWeight.bold)),
        style: ElevatedButton.styleFrom(
          backgroundColor: Colors.white,
          side: const BorderSide(color: Colors.blue),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(widget.lectureTitle)),
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              _buildButton(onPressed: _askForPassword, icon: Icons.lock, label: "Attend using a password"),
              const SizedBox(height: 20),
              _buildButton(onPressed: _openQRSystem, icon: Icons.qr_code_scanner, label: "Scan QR Code"),
              const SizedBox(height: 20),
              _buildButton(onPressed: _wifiLoading ? null : _wifiCheckIn, icon: Icons.wifi, label: "Attend using WiFi", isLoading: _wifiLoading),
            ],
          ),
        ),
      ),
    );
  }
}
