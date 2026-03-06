// lib/services/wifi_service.dart

import 'package:dio/dio.dart';
import 'package:wifi_scan/wifi_scan.dart';
import 'package:permission_handler/permission_handler.dart';

class AttendanceResult {
  final bool success;
  final String message;
  final double confidence;
  final String? attendanceId;

  AttendanceResult({
    required this.success,
    required this.message,
    this.confidence = 0.0,
    this.attendanceId,
  });
}

class ActiveSessionResult {
  final bool active;
  final String? sessionId;
  final String? classroom;

  ActiveSessionResult({
    required this.active,
    this.sessionId,
    this.classroom,
  });
}

class WifiService {
  final Dio _dio;

  WifiService({required String baseUrl, required String token})
      : _dio = Dio(
          BaseOptions(
            baseUrl: baseUrl,
            headers: {'Authorization': 'Bearer $token'},
            connectTimeout: const Duration(seconds: 10),
            receiveTimeout: const Duration(seconds: 10),
          ),
        );

  /// Derse ait aktif WiFi session var mı kontrol et
  Future<ActiveSessionResult> getActiveSession(String csLectureId) async {
    try {
      final response = await _dio.get('/wifi/sessions/active/$csLectureId');
      final data = response.data;
      return ActiveSessionResult(
        active: data['active'] ?? false,
        sessionId: data['session_id']?.toString(),
        classroom: data['classroom'],
      );
    } on DioException catch (e) {
      return ActiveSessionResult(active: false);
    }
  }

  /// WiFi izinlerini iste
  Future<bool> requestPermissions() async {
    final location = await Permission.location.request();
    final nearbyWifi = await Permission.nearbyWifiDevices.request();
    return location.isGranted &&
        (nearbyWifi.isGranted || nearbyWifi.isLimited);
  }

  /// WiFi ağlarını tara
  Future<List<WiFiAccessPoint>> scanWifi() async {
    final canScan =
        await WiFiScan.instance.canStartScan(askPermissions: true);
    if (canScan != CanStartScan.yes) {
      throw Exception('WiFi taraması yapılamıyor: $canScan');
    }

    await WiFiScan.instance.startScan();
    await Future.delayed(const Duration(seconds: 2));

    final canGet = await WiFiScan.instance
        .canGetScannedResults(askPermissions: true);
    if (canGet != CanGetScannedResults.yes) {
      throw Exception('WiFi sonuçları alınamıyor: $canGet');
    }

    return await WiFiScan.instance.getScannedResults();
  }

  /// WiFi taraması yapıp yoklama al
  Future<AttendanceResult> checkIn({
    required String sessionId,
    required String deviceInfo,
  }) async {
    // 1. İzinleri kontrol et
    final hasPermission = await requestPermissions();
    if (!hasPermission) {
      return AttendanceResult(
        success: false,
        message: 'WiFi taraması için konum izni gerekli',
      );
    }

    // 2. WiFi tara
    final List<WiFiAccessPoint> aps;
    try {
      aps = await scanWifi();
    } catch (e) {
      return AttendanceResult(
        success: false,
        message: 'WiFi taraması başarısız: $e',
      );
    }

    if (aps.isEmpty) {
      return AttendanceResult(
        success: false,
        message: 'Hiç WiFi ağı bulunamadı',
      );
    }

    // 3. FastAPI'ye gönder
    try {
      final response = await _dio.post(
        '/wifi/attendance/check-in',
        data: {
          'session_id': sessionId,
          'device_info': deviceInfo,
          'client_timestamp': DateTime.now().toUtc().toIso8601String(),
          'access_points': aps
              .where((ap) => ap.level != 0)
              .map((ap) => {
                    'bssid': ap.bssid,
                    'ssid': ap.ssid,
                    'rssi': ap.level,
                    'frequency': ap.frequency,
                  })
              .toList(),
        },
      );

      final data = response.data;
      return AttendanceResult(
        success: data['success'] ?? false,
        message: data['message'] ?? '',
        confidence: (data['confidence'] ?? 0.0).toDouble(),
        attendanceId: data['attendance_id']?.toString(),
      );
    } on DioException catch (e) {
      final detail = e.response?.data?['detail'] ?? e.message;
      return AttendanceResult(
        success: false,
        message: detail.toString(),
      );
    }
  }
}