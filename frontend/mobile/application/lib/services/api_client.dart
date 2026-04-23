import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'server_config.dart';

class ApiClient {
  static final ApiClient _instance = ApiClient._();
  static ApiClient get instance => _instance;

  late Dio dio;
  final _storage = FlutterSecureStorage();

  ApiClient._() {
    _buildDio();
  }

  void _buildDio() {
    dio = Dio(BaseOptions(baseUrl: ServerConfig.baseUrl))
      ..interceptors.add(_authInterceptor());
  }

  /// Call this after changing the server URL in settings.
  void reload() => _buildDio();

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
