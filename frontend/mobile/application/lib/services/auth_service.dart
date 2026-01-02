import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'api_client.dart';

class AuthService {
  final Dio _dio = ApiClient.instance.dio;
  final _storage = FlutterSecureStorage();

  Future<bool> login(String email, String password) async {
    try {
      final res = await _dio.post(
        '/api/Account/authenticate/mobile',
        data: {'email': email, 'password': password},
      );

      if (res.statusCode != 200) {
        print('Login HTTP ${res.statusCode}: ${res.data}');
        return false;
      }

      final raw = res.data;
      print('Login response raw data: $raw');

      if (raw is! Map<String, dynamic>) {
        print('Unexpected response format: ${raw.runtimeType}');
        return false;
      }

      final payload = raw['data'];
      if (payload is! Map<String, dynamic>) {
        print('Invalid or missing "data" field: $payload');
        return false;
      }

      final token = payload['jwToken'] as String?;
      final userId = payload['id']?.toString();

      print('Extracted token: $token');
      print('Extracted userId: $userId');

      if (token == null || token.isEmpty) {
        print('jwToken missing or empty in payload: $payload');
        return false;
      }

      if (userId == null || userId.isEmpty) {
        print('User ID missing or empty in payload: $payload');
        return false;
      }

      await _storage.write(key: 'jwt', value: token);
      await _storage.write(key: 'email', value: email);
      await _storage.write(key: 'user_id', value: userId);
      return true;
    } on DioException catch (e) {
      print('Login exception: ${e.response?.statusCode} ${e.message}');
      if (e.response != null) {
        print('Login error response data: ${e.response?.data}');
      }
      return false;
    }
  }

  Future<void> logout() async {
    await _storage.delete(key: 'jwt');
    await _storage.delete(key: 'email');
    await _storage.delete(key: 'user_id');
  }
}
