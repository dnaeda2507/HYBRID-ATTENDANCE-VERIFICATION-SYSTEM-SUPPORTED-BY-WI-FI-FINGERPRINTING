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
