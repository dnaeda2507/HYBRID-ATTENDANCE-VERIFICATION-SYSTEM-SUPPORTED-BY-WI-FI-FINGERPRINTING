import 'package:application/models/lecture.dart';
import 'package:application/services/api_client.dart';
import 'package:application/services/lecture_service.dart';
import 'package:dio/dio.dart' show Dio, DioException;

class LectureDetailsService {
  final Dio _dio = ApiClient.instance.dio;

  Future<Map<String, dynamic>> getLectureDetailsFromStoredId() async {
    final id = LectureStateService().selectedLectureId;

    if (id == null) {
      throw Exception('No lecture ID stored in LectureStateService.');
    }

    try {
      final response = await _dio.get('/api/v1/Course/get-by-id/$id');
      if (response.statusCode != 200) {
        throw Exception('Server error: ${response.statusCode}');
      }

      final data = response.data;
      if (data is Map<String, dynamic>) {
        return data;
      } else {
        throw Exception('Unexpected response format: ${data.runtimeType}');
      }
    } on DioException catch (dioErr) {
      throw Exception('Network error: ${dioErr.message}');
    } catch (e) {
      throw Exception('Error fetching lecture details: $e');
    }
  }

    Future<Map<String, dynamic>?> getLectureDetailsById(int lectureId) async {
    try {
      final response = await _dio.get('/api/v1/Course/get-by-id/$lectureId');
      if (response.statusCode == 200) {
        final data = response.data;
        if (data is Map<String, dynamic>) {
          return data;
        } else {
          throw Exception('Unexpected response format: ${data.runtimeType}');
        }
      } else {
        throw Exception('Server error: ${response.statusCode}');
      }
    } on DioException catch (dioErr) {
      throw Exception('Network error: ${dioErr.message}');
    } catch (e) {
      throw Exception('Error fetching lecture details: $e');
    }
  }


  Future<LectureDetails> getLectureDetailsModelById(int lectureId) async {
    final json = await getLectureDetailsById(lectureId);
    if (json == null) {
      throw Exception('No data found for lecture id $lectureId');
    }
    return LectureDetails.fromJson(json);
  }
}
