import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'api_client.dart';

class ChangePasswordService {
  final Dio _dio = ApiClient.instance.dio;
  final _storage = FlutterSecureStorage();

  Future<bool> changePassword(
    String oldPassword,
    String newPassword,
    String confirmPassword,
  ) async {
    try {
      final token = await _storage.read(key: 'jwt');
      final userId = await _storage.read(key: 'user_id');

      if (token == null) {
        print('Token not found');
        return false;
      }
      if (userId == null) {
        print('User ID not found');
        return false;
      }

      final res = await _dio.post(
        '/api/Account/change-password',
        data: {
          'userId': userId,
          'oldPassword': oldPassword,
          'newPassword': newPassword,
          'confirmPassword': confirmPassword,
        },
        options: Options(
          headers: {
            'Authorization': 'Bearer $token',
            'Content-Type': 'application/json',
          },
        ),
      );

      final responseData = res.data;
      if (responseData['success'] == true) {
        print('Password changed successfully: ${responseData['Message']}');
        return true;
      } else {
        print('Error: ${responseData['message']}');
        if (responseData['errors'] != null) {
          responseData['errors'].forEach((error) {
            print('Error details: $error');
          });
        }
        return false;
      }
    } on DioException catch (e) {
      print('Change password error: ${e.response?.statusCode} ${e.message}');
      if (e.response != null) {
        print('Response data: ${e.response?.data}');
      } else {
        print('No response data, possible network error.');
      }
      return false;
    } catch (e) {
      print('Unexpected error: $e');
      return false;
    }
  }
}
