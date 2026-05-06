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
  final AttendanceService _attendanceService = AttendanceService();

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
          title: const Text("Yoklama Şifresini Gir"),
          content: TextField(
            controller: passwordController,
            obscureText: true,
            decoration: const InputDecoration(hintText: "Şifre"),
          ),
          actions: [
            TextButton(
              onPressed: () async {
                Navigator.of(dialogContext).pop();
                final password = passwordController.text.trim();
                if (password.isEmpty) {
                  ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Şifre boş olamaz")));
                  return;
                }
                try {
                  final sessionId = await _attendanceService.getMyActiveSessionId();
                  if (sessionId == null) {
                    _showErrorDialog('Aktif yoklama oturumu bulunamadı');
                    return;
                  }

                  final response = await _attendanceService.markAttendanceByPassword(
                    sessionId: sessionId,
                    password: password,
                  );
                  if (response.data['success'] == true) {
                    _showSuccessDialog("Şifre ile yoklama başarılı!");
                  } else {
                    _showErrorDialog(response.data['message'] ?? 'Hata oluştu');
                  }
                } catch (e) {
                  _showErrorDialog('Hata: $e');
                }
              },
              child: const Text("Gönder"),
            ),
          ],
        );
      },
    );
  }

  void _openQRSystem() async {
    final result = await Navigator.push(context, MaterialPageRoute(builder: (context) => const QRViewPage()));
    if (result == null) return;

    final parsed = _attendanceService.parseQrPayload(result.toString());
    if (parsed == null) {
      _showErrorDialog('QR kod formati gecersiz');
      return;
    }

    try {
      final response = await _attendanceService.markAttendanceByQR(
        sessionId: parsed.sessionId,
        qrToken: parsed.token,
      );
      if (response.data['success'] == true) {
        _showSuccessDialog("QR ile yoklama başarılı!");
      } else {
        _showErrorDialog(response.data['message'] ?? 'Hata oluştu');
      }
    } catch (e) {
      _showErrorDialog('Hata: $e');
    }
  }

  Future<void> _wifiCheckIn() async {
    setState(() => _wifiLoading = true);
    try {
      // Önce active session'ı bul
      final sessionId = await WifiService().findActiveSessionId();
      if (sessionId == null) {
        _showErrorDialog('Aktif yoklama oturumu bulunamadı');
        return;
      }

      // WiFi ile yoklama yap
      final response = await _attendanceService.markAttendanceByWiFi(
        sessionId: sessionId,
      );
      if (response.data['success'] == true) {
        _showSuccessDialog("WiFi ile yoklama başarılı!");
      } else {
        _showErrorDialog(response.data['message'] ?? 'Hata oluştu');
      }
    } catch (e) {
      _showErrorDialog('Hata: $e');
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
