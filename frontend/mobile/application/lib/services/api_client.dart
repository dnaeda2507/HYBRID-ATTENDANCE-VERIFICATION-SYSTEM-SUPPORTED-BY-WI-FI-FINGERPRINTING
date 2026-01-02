import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart' show kIsWeb;
import 'dart:io' show Platform;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class ApiClient {
  static final ApiClient instance = ApiClient._();

  late final Dio dio;
  final _storage = FlutterSecureStorage();

  ApiClient._() {
    final host =
        kIsWeb
            ? 'localhost'
            : Platform.isAndroid
            ? '10.0.2.2'
            : '127.0.0.1';

    dio = Dio(BaseOptions(baseUrl: 'https://$host:9001'))
      ..interceptors.add(_authInterceptor());
  }

  // Save user ID to secure storage
  Future<void> saveUserId(String userId) async {
    await _storage.write(key: 'user_id', value: userId);
  }

  // Get user ID from secure storage
  Future<String?> getUserId() async {
    return await _storage.read(key: 'user_id');
  }

  // Clear user ID from secure storage (for logout)
  Future<void> clearUserId() async {
    await _storage.delete(key: 'user_id');
  }

  Interceptor _authInterceptor() {
    return InterceptorsWrapper(
      onRequest: (options, handler) async {
        final token = await _storage.read(key: 'jwt');
        if (token != null) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        handler.next(options);
      },
    );
  }
}
