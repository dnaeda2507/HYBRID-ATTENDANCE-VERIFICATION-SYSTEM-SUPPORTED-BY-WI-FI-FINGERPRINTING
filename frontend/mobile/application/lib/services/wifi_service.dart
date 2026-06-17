import 'package:application/services/api_client.dart';
import 'package:application/services/server_config.dart';
import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:wifi_scan/wifi_scan.dart';
import 'package:permission_handler/permission_handler.dart';

class AttendanceResult {
  final bool success;
  final String message;
  final double confidence;

  AttendanceResult({required this.success, required this.message, this.confidence = 0.0});
}

class WifiService {
  final Dio _dio = ApiClient.instance.dio;
  final _storage = const FlutterSecureStorage();

  Future<String?> _getToken() => _storage.read(key: 'jwt');

  Future<bool> requestPermissions() async {
    final location = await Permission.location.request();
    final nearbyWifi = await Permission.nearbyWifiDevices.request();
    return location.isGranted && (nearbyWifi.isGranted || nearbyWifi.isLimited);
  }

  Future<List<WiFiAccessPoint>> scanWifi() async {
    final canScan = await WiFiScan.instance.canStartScan(askPermissions: true);
    if (canScan != CanStartScan.yes) throw Exception('WiFi taraması yapılamıyor: $canScan');
    await WiFiScan.instance.startScan();
    await Future.delayed(const Duration(seconds: 2));
    final canGet = await WiFiScan.instance.canGetScannedResults(askPermissions: true);
    if (canGet != CanGetScannedResults.yes) throw Exception('WiFi sonuçları alınamıyor');
    return await WiFiScan.instance.getScannedResults();
  }

  /// Öğrencinin derslerini al, aktif session olan ilkini bul
  Future<int?> findActiveSessionId() async {
    try {
      final token = await _getToken();

      // 1. Öğrencinin derslerini al
      final coursesResp = await _dio.get(
        '/api/v1/Course/get-for-current-user',
        queryParameters: {'pageNumber': 1, 'pageSize': 100},
        options: Options(headers: {'Authorization': 'Bearer $token'}),
      );

      final listJson = coursesResp.data['data']['data'] as List;
      final courseIds = listJson.map((e) => e['id'] as int).toList();

      // 2. Her ders için aktif session var mı kontrol et
      for (final courseId in courseIds) {
        final resp = await _dio.get(
          '/api/sessions/active-by-course/$courseId',
          options: Options(headers: {'Authorization': 'Bearer $token'}),
        );
        final sessionId = resp.data['data'] as int?;
        if (sessionId != null) return sessionId;
      }
      return null;
    } catch (e) {
      return null;
    }
  }

  Future<AttendanceResult> checkIn() async {
    // 1. Aktif session bul
    final sessionId = await findActiveSessionId();
    if (sessionId == null) {
      return AttendanceResult(success: false, message: 'Şu an aktif bir dersiniz bulunamadı');
    }

    // 2. İzin kontrolü
    final hasPermission = await requestPermissions();
    if (!hasPermission) {
      return AttendanceResult(success: false, message: 'Konum izni gerekli');
    }

    // 3. WiFi tara
    final List<WiFiAccessPoint> aps;
    try {
      aps = await scanWifi();
    } catch (e) {
      return AttendanceResult(success: false, message: 'WiFi taraması başarısız: $e');
    }

    if (aps.isEmpty) {
      return AttendanceResult(success: false, message: 'Hiç WiFi ağı bulunamadı');
    }

    // 4. C# backend'e gönder — tüm görünen AP'leri gönder (eduroam filtresi kaldırıldı)
    try {
      final token = await _getToken();
      final accessPoints = aps
          .where((ap) => ap.level != 0)
          .toList()
        ..sort((a, b) => b.level.compareTo(a.level));

      if (accessPoints.isEmpty) {
        return AttendanceResult(
          success: false,
          message: 'Geçerli sinyal seviyesinde WiFi ağı bulunamadı',
        );
      }

      await _dio.post(
        '/api/Wifi/scans',
        data: {
          'studentId': null,
          'sessionId': sessionId,
          'scannedAtUtc': DateTime.now().toUtc().toIso8601String(),
          'accessPoints': accessPoints
              .take(20)
              .map((ap) => {'bssid': ap.bssid, 'rssi': ap.level})
              .toList(),
        },
        options: Options(headers: {'Authorization': 'Bearer $token'}),
      );

      return AttendanceResult(success: true, message: 'WiFi taraması gönderildi ✓');
    } on DioException catch (e) {
      if (e.type == DioExceptionType.connectionError ||
          e.type == DioExceptionType.connectionTimeout) {
        return AttendanceResult(
          success: false,
          message: 'Sunucuya bağlanılamadı (${ServerConfig.baseUrl}). '
              'Settings > Server Address adresini kontrol edin. '
              'Android emulator: http://10.0.2.2:9001 | '
              'Fiziksel telefon: http://BILGISAYAR_IP:9001',
        );
      }
      final detail = e.response?.data?['errors']?.first ??
          e.response?.data?['message'] ??
          e.message;
      return AttendanceResult(success: false, message: detail.toString());
    }
  }
}
