import 'dart:convert';
import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'api_client.dart';
import '../models/user.dart';

class UserService {
  final Dio _dio = ApiClient.instance.dio;
  final _storage = FlutterSecureStorage();

  Future<User?> getCurrentUser() async {
    try {
      final userId = await _storage.read(key: 'user_id');
      final token = await _storage.read(key: 'jwt');

      if (token == null || userId == null) {
        print('Missing user ID or token');
        return null;
      }

      final response = await _dio.get(
        '/api/v1/User/get-current-user',
        options: Options(
          headers: {
            'Authorization': 'Bearer $token',
            'Accept': 'application/json',
          },
        ),
      );

      if (response.statusCode == 200 && response.data != null) {
        final raw = response.data;

        print('Raw response: $raw');

        if (raw is Map<String, dynamic>) {
          final userData = raw['data'];
          if (userData is Map<String, dynamic>) {
            return User.fromJson(userData);
          } else {
            print('Invalid or missing "data" field in response: $userData');
          }
        } else {
          print('Unexpected response type: ${raw.runtimeType}');
        }
      } else {
        print(
          'Failed to fetch user: ${response.statusCode} - ${response.data}',
        );
      }
      return null;
    } on DioException catch (e) {
      print('DioException: ${e.message}');
      if (e.response != null) {
        print('Status: ${e.response?.statusCode}');
        print('Data: ${e.response?.data}');
        if (e.response?.data is Map<String, dynamic>) {
          var errorMessage = e.response?.data['message'] ?? 'No message';
          print('Error Message: $errorMessage');
        }
      }
      return null;
    } catch (e) {
      print('Unexpected error: $e');
      return null;
    }
  }

  Future<bool> updateUser({
    required String firstName,
    required String lastName,
    required String email,
    required String informationMail,
    required String phoneNumber,
  }) async {
    try {
      final token = await _storage.read(key: 'jwt');
      if (token == null) {
        print('JWT token not found');
        return false;
      }

      final response = await _dio.put(
        '/api/v1/User/update-profile',
        data: {
          'firstName': firstName,
          'lastName': lastName,
          'email': email,
          'informationMail': informationMail,
          'phoneNumber': phoneNumber,
        },
        options: Options(headers: {'Authorization': 'Bearer $token'}),
      );

      if (response.statusCode == 200 || response.statusCode == 204) {
        print("User updated successfully");
        return true;
      } else {
        print("Update failed: ${response.statusCode} - ${response.data}");
        return false;
      }
    } on DioException catch (e) {
      print('DioException: ${e.message}');
      if (e.response != null) {
        print('Status: ${e.response?.statusCode}');
        print('Data: ${e.response?.data}');
      }
      return false;
    } catch (e) {
      print('Unexpected error: $e');
      return false;
    }
  }
}
