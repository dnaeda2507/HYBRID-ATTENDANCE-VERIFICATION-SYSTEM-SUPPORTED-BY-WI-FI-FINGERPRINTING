import 'package:application/models/lecture.dart';
import 'package:application/services/api_client.dart';
import 'package:dio/dio.dart';

class CourseListingDTO {
  final int id;
  final String lectureName;
  final String teacherName;
  final String departmentName;
  final String dayOfWeek;
  final String startTime;
  final String location;

  CourseListingDTO({
    required this.id,
    required this.lectureName,
    required this.teacherName,
    required this.departmentName,
    required this.dayOfWeek,
    required this.startTime,
    required this.location,
  });

  factory CourseListingDTO.fromJson(Map<String, dynamic> json) {
    return CourseListingDTO(
      id: json['id'] as int,
      lectureName: json['lectureName'] as String? ?? '',
      teacherName: json['teacherName'] as String? ?? '',
      departmentName: json['departmentName'] as String? ?? '',
      dayOfWeek: json['dayOfWeek']?.toString() ?? '',
      startTime: json['startTime']?.toString() ?? '',
      location: json['location'] as String? ?? '',
    );
  }
}

class LectureService {
  final Dio _dio = ApiClient.instance.dio;
  Future<LectureDetails> getLectureDetails(
    String version,
    String userId,
  ) async {
    try {
      final response = await _dio.get('/api/v1/Course/get-by-user/$userId');
      if (response.statusCode != 200) {
        throw Exception('Server error: ${response.statusCode}');
      }

      final raw = response.data;
      if (raw is Map<String, dynamic>) {
        final data = raw['data'];
        if (data != null && data is Map<String, dynamic>) {
          return LectureDetails.fromJson(data);
        } else {
          throw Exception('No data found in response');
        }
      } else {
        throw Exception('Unexpected response structure');
      }
    } on DioException catch (dioErr) {
      throw Exception('Network error: ${dioErr.message}');
    } catch (e) {
      throw Exception('Error loading lecture details: $e');
    }
  }

  Future<List<CourseListingDTO>> getLectures() async {
    try {
      final response = await _dio.get(
        '/api/v1/Course/get-for-current-user',
        queryParameters: {'pageNumber': 1, 'pageSize': 100},
      );

      if (response.statusCode != 200) {
        throw Exception('Server error: ${response.statusCode}');
      }

      final raw = response.data;
      print('API response: $raw');

      late final Map<String, dynamic> envelope;
      if (raw is List && raw.isNotEmpty && raw[0] is Map<String, dynamic>) {
        envelope = raw[0] as Map<String, dynamic>;
      } else if (raw is Map<String, dynamic>) {
        envelope = raw;
      } else {
        throw Exception('Unexpected root JSON structure');
      }

      final page = envelope['data'];
      if (page is! Map<String, dynamic>) {
        throw Exception('Missing `data` object');
      }

      final listJson = page['data'];
      if (listJson is! List) {
        throw Exception('Missing inner `data` array');
      }

      return listJson
          .map((e) => CourseListingDTO.fromJson(e as Map<String, dynamic>))
          .toList();
    } on DioException catch (dioErr) {
      throw Exception('Network error: ${dioErr.message}');
    } catch (e) {
      throw Exception('Error loading lectures: $e');
    }
  }
}

class LectureStateService {
  static final LectureStateService _instance = LectureStateService._internal();
  factory LectureStateService() => _instance;
  LectureStateService._internal();

  int? selectedLectureId;
}
