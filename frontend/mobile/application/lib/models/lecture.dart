class LectureDetails {
  final int? id;
  final String lectureName;
  final String teacherName;
  final String departmentName;
  final String dayOfWeek;
  final String startTime;
  final String location;

  LectureDetails({
    required this.id,
    required this.lectureName,
    required this.teacherName,
    required this.departmentName,
    required this.dayOfWeek,
    required this.startTime,
    required this.location,
  });

  factory LectureDetails.fromJson(Map<String, dynamic> json) {
  print('JSON verisi: $json');
  return LectureDetails(
    id: json['id'] as int?,
    lectureName: json['lecture']?['name']?.toString() ?? '',
    teacherName:
        "${json['teacher']?['firstName']?.toString() ?? ''} ${json['teacher']?['lastName']?.toString() ?? ''}",
    departmentName: json['department']?['name']?.toString() ?? '',
    dayOfWeek: _parseDayOfWeek(json['dayOfWeek'] as int?),
    startTime: json['startTime']?.toString() ?? '',
    location: json['location']?.toString() ?? '',
  );
}
  static String _parseDayOfWeek(int? value) {
    const days = [
      'Pazartesi',
      'Salı',
      'Çarşamba',
      'Perşembe',
      'Cuma',
      'Cumartesi',
      'Pazar'
    ];
    if (value == null || value < 0 || value > 6) return '';
    return days[value];
  }
}
