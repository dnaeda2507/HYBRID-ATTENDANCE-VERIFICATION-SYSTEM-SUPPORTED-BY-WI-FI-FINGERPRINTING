import 'dart:convert';

import 'package:application/screens/attended/attended_courses_state.dart';
import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'api_client.dart';

class AttendanceRecord {
  final String courseName;
  final DateTime markedAtUtc;

  AttendanceRecord({required this.courseName, required this.markedAtUtc});

  factory AttendanceRecord.fromJson(Map<String, dynamic> json) {
    return AttendanceRecord(
      courseName: json['courseName'] ?? '',
      markedAtUtc: DateTime.parse(json['markedAtUtc']),
    );
  }
}

class AttendanceService {
  final Dio _dio = ApiClient.instance.dio;
  final _storage = FlutterSecureStorage();

  // Attendance methods
  static const int METHOD_QR = 0;
  static const int METHOD_PASSWORD = 1;
  static const int METHOD_WIFI = 2;

  String _extractErrorMessage(Object error) {
    if (error is DioException) {
      final data = error.response?.data;
      if (data is Map<String, dynamic>) {
        final message = data['message'];
        if (message is String && message.isNotEmpty) {
          return message;
        }
      }
      return error.message ?? 'Sunucu ile iletisimde hata olustu';
    }
    return 'Beklenmeyen bir hata olustu';
  }

  /// Sends attendance via QR code, Password, or WiFi
  /// [method]: 0 = QR, 1 = Password, 2 = WiFi
  Future<Response<dynamic>> sendAttendance({
    required int sessionId,
    String? token,  // Required for QR and Password, null for WiFi
    int method = METHOD_QR,  // Default: QR code
  }) async {
    try {
      final jwtToken = await _storage.read(key: 'jwt');
      if (jwtToken == null) {
        throw Exception('No JWT token found');
      }

      final Map<String, dynamic> data = {
        'sessionId': sessionId,
        'method': method,
      };

      // Token required for QR and Password
      if ((method == METHOD_QR || method == METHOD_PASSWORD) && token != null) {
        data['token'] = token;
      }

      final response = await _dio.post(
        '/api/sessions/attend',
        data: data,
        options: Options(headers: {'Authorization': 'Bearer $jwtToken'}),
      );

      return response;
    } catch (e) {
      throw Exception(_extractErrorMessage(e));
    }
  }

  Future<int?> getMyActiveSessionId() async {
    try {
      final jwtToken = await _storage.read(key: 'jwt');
      if (jwtToken == null || jwtToken.isEmpty) {
        throw Exception('No JWT token found');
      }

      final response = await _dio.get(
        '/api/sessions/my-active-session',
        options: Options(headers: {'Authorization': 'Bearer $jwtToken'}),
      );

      final data = response.data;
      if (data is Map<String, dynamic> && data['success'] == true) {
        final sessionId = data['data'];
        if (sessionId is int) {
          return sessionId;
        }
      }
      return null;
    } catch (e) {
      throw Exception(_extractErrorMessage(e));
    }
  }

  ({int sessionId, String token})? parseQrPayload(String payload) {
    final value = payload.trim();
    if (value.isEmpty) return null;

    if (value.contains(':')) {
      final parts = value.split(':');
      if (parts.length >= 2) {
        final sessionId = int.tryParse(parts[0].trim());
        final token = parts.sublist(1).join(':').trim();
        if (sessionId != null && token.isNotEmpty) {
          return (sessionId: sessionId, token: token);
        }
      }
    }

    try {
      final decoded = jsonDecode(value);
      if (decoded is Map<String, dynamic>) {
        final dynamic maybeId = decoded['sessionId'] ?? decoded['session_id'];
        final dynamic maybeToken = decoded['token'] ?? decoded['qrToken'];
        final sessionId = maybeId is int ? maybeId : int.tryParse('$maybeId');
        final token = maybeToken?.toString().trim() ?? '';
        if (sessionId != null && token.isNotEmpty) {
          return (sessionId: sessionId, token: token);
        }
      }
    } catch (_) {
      // Not a JSON payload, continue fallback.
    }

    return null;
  }

  /// QR Code ile yoklama al
  Future<Response<dynamic>> markAttendanceByQR({
    required int sessionId,
    required String qrToken,
  }) => sendAttendance(
    sessionId: sessionId,
    token: qrToken,
    method: METHOD_QR,
  );

  /// Şifre ile yoklama al
  Future<Response<dynamic>> markAttendanceByPassword({
    required int sessionId,
    required String password,
  }) => sendAttendance(
    sessionId: sessionId,
    token: password,
    method: METHOD_PASSWORD,
  );

  /// WiFi ile yoklama al
  Future<Response<dynamic>> markAttendanceByWiFi({
    required int sessionId,
  }) => sendAttendance(
    sessionId: sessionId,
    method: METHOD_WIFI,
  );

  Future<List<AttendanceRecord>> fetchUserAttendances({
    int pageNumber = 1,
    int pageSize = 10,
  }) async {
    final token = await _storage.read(key: 'jwt');
    if (token == null || token.isEmpty) {
      throw Exception('JWT token bulunamadı');
    }

    final response = await _dio.get(
      '/api/sessions/get-currentuser-attendances',
      queryParameters: {'pageNumber': pageNumber, 'pageSize': pageSize},
      options: Options(
        headers: {'Authorization': 'Bearer $token'},
        responseType: ResponseType.plain, // String olarak alıyoruz
      ),
    );

    if (response.statusCode != 200) {
      throw Exception('Sunucu hatası: ${response.statusCode}');
    }

    final decoded = jsonDecode(response.data);

    if (decoded['success'] != true) {
      final errorMessage = decoded['message'] ?? 'Katılım listesi alınamadı';
      throw Exception('API Hatası: $errorMessage');
    }

    // Success true, data'yı parse et
    final List<dynamic> attendancesJson = decoded['data']?['data'] ?? [];

    return attendancesJson
        .map((json) => AttendanceRecord.fromJson(json))
        .toList();
  }
}
