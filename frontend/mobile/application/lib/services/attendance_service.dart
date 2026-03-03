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

class WifiAccessPoint {
  final String bssid;
  final int rssi;

  WifiAccessPoint({required this.bssid, required this.rssi});

  Map<String, dynamic> toJson() => {'bssid': bssid, 'rssi': rssi};
}

class AttendanceService {
  final Dio _dio = ApiClient.instance.dio;
  final _storage = FlutterSecureStorage();

  Future<Response<dynamic>> sendAttendance({
    required int sessionId,
    required String tokenFromQR,
  }) async {
    try {
      final token = await _storage.read(key: 'jwt');
      if (token == null) {
        throw Exception('No JWT token found');
      }

      final response = await _dio.post(
        '/api/sessions/attend',
        data: {'sessionId': sessionId, 'token': tokenFromQR},
        options: Options(headers: {'Authorization': 'Bearer $token'}),
      );

      return response;
    } catch (_) {
      throw Exception('Error sending attendance');
    }
  }

  /// Wi‑Fi fingerprint ile yoklama isteği
  Future<Response<dynamic>> sendWifiScan({
    required int sessionId,
    required List<WifiAccessPoint> accessPoints,
  }) async {
    try {
      final token = await _storage.read(key: 'jwt');
      final userId = await _storage.read(key: 'user_id');
      if (token == null || userId == null) {
        throw Exception('User id or JWT token not found');
      }

      final payload = {
        'studentId': userId,
        'sessionId': sessionId,
        'scannedAtUtc': DateTime.now().toUtc().toIso8601String(),
        'accessPoints': accessPoints.map((e) => e.toJson()).toList(),
      };

      final response = await _dio.post(
        '/api/wifi/scans',
        data: payload,
        options: Options(headers: {'Authorization': 'Bearer $token'}),
      );

      return response;
    } catch (_) {
      throw Exception('Error sending wifi scan');
    }
  }

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
      throw Exception('API başarısız: ${decoded['message']}');
    }

    final List<dynamic> attendancesJson = decoded['data']?['data'] ?? [];

    return attendancesJson
        .map((json) => AttendanceRecord.fromJson(json))
        .toList();
  }
}
