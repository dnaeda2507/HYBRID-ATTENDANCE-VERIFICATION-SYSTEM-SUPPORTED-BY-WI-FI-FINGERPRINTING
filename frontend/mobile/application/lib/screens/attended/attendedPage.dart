import 'package:application/screens/attended/attend_lecture.dart';
import 'package:application/screens/attended/qr_view.dart';
import 'package:application/services/attendance_service.dart';
import 'package:application/services/lecture_service.dart';
import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:wifi_scan/wifi_scan.dart' as wifi_scan;
import 'package:permission_handler/permission_handler.dart';
import 'attended_courses_list.dart';

class AttendedPage extends StatefulWidget {
  const AttendedPage({super.key});

  @override
  State<AttendedPage> createState() => _AttendedPageState();
}

class _AttendedPageState extends State<AttendedPage> {
  int _selectedTab = 0; // 0: Attend course, 1: Attended courses
  List<CourseListingDTO> _attendedLectures = [];
  List<CourseListingDTO> _filteredLectures = [];
  final LectureService _lectureService = LectureService();
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _loadLectures();
  }

  Future<void> _loadLectures() async {
    try {
      final lectures = await _lectureService.getLectures();
      setState(() {
        _attendedLectures = lectures;
        _filteredLectures = lectures;
      });
    } catch (e) {
      print('Error loading lectures: $e');
    }
  }

  void _filterLectures(String query) {
    final filtered =
        _attendedLectures.where((lecture) {
          return lecture.lectureName.toLowerCase().contains(
            query.toLowerCase(),
          );
        }).toList();

    setState(() {
      _filteredLectures = filtered;
    });
  }

  void _onLectureTap(String lectureTitle, int lectureId) {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => AttendedLecture(lectureTitle: lectureTitle),
      ),
    );
  }

  void _showSuccessDialog(BuildContext context, Response<dynamic> response) {
    showDialog(
      context: context,
      builder:
          (_) => AlertDialog(
            title: const Text('Success'),
            content: Text(response.data['message']),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(),
                child: const Text('OK'),
              ),
            ],
          ),
    );
  }

  void _showErrorDialog(BuildContext context, Response<dynamic> response) {
    String errorMessage = response.data['message'] ?? 'An error occurred';
    if (response.data['errors'] != null) {
      errorMessage += '\n${response.data['errors'].join(', ')}';
    }
    showDialog(
      context: context,
      builder:
          (_) => AlertDialog(
            title: const Text('Error'),
            content: Text(errorMessage),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(),
                child: const Text('OK'),
              ),
            ],
          ),
    );
  }

  void _askForPassword(BuildContext context) {
    final TextEditingController passwordController = TextEditingController();

    showDialog(
      context: context,
      builder: (dialogContext) {
        return AlertDialog(
          title: const Text("Enter Attendance Password"),
          content: TextField(controller: passwordController),
          actions: [
            TextButton(
              onPressed: () async {
                Navigator.of(context).pop();

                final input = passwordController.text.trim();
                if (input.isEmpty || !input.contains(':')) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text("Invalid input format")),
                  );
                  return;
                }
                final parts = input.split(':');
                final sessionId = int.tryParse(parts[0]) ?? 0;
                final token = parts[1];

                final response = await AttendanceService().sendAttendance(
                  sessionId: sessionId,
                  tokenFromQR: token,
                );

                if (response.data['success'] == true) {
                  _showSuccessDialog(context, response);
                } else {
                  _showErrorDialog(context, response);
                }
              },
              child: const Text("OK"),
            ),
          ],
        );
      },
    );
  }

  void _openQRSystem(BuildContext context) async {
    final result = await Navigator.push(
      context,
      MaterialPageRoute(builder: (context) => QRViewPage()),
    );

    if (result != null && result.contains(':')) {
      final parts = result.split(':');
      final sessionId = int.tryParse(parts[0]);
      final token = parts[1];

      if (sessionId != null && token.isNotEmpty) {
        final response = await AttendanceService().sendAttendance(
          sessionId: sessionId,
          tokenFromQR: token,
        );

        if (response.data['success'] == true) {
          _showSuccessDialog(context, response);
        } else {
          _showErrorDialog(context, response);
        }
      } else {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(const SnackBar(content: Text("Invalid QR format")));
      }
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("QR code could not be read.")),
      );
    }
  }

  /// Perform a Wi‑Fi scan and return a list of app DTO `WifiAccessPoint`.
  Future<List<WifiAccessPoint>> _collectWifiAccessPoints() async {
    final permissionGranted = await _ensureLocationPermission();
    if (!permissionGranted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Location permission is required for Wi‑Fi scanning.'),
        ),
      );
      return <WifiAccessPoint>[];
    }

    if (kIsWeb) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Wi‑Fi scanning is not available on web.'),
        ),
      );
      return <WifiAccessPoint>[];
    }

    try {
      final canScan = await wifi_scan.WiFiScan.instance.canStartScan();
      if (canScan != wifi_scan.CanStartScan.yes) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('Cannot start scan: $canScan')));
        return <WifiAccessPoint>[];
      }

      await wifi_scan.WiFiScan.instance.startScan();
      await Future.delayed(const Duration(milliseconds: 800));
      final results = await wifi_scan.WiFiScan.instance.getScannedResults();

      if (results == null || results.isEmpty) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('No Wi‑Fi access points were found.')),
        );
        return <WifiAccessPoint>[];
      }

      return results.map((r) {
        final bssid = (r.bssid ?? '').toString();
        final level = r.level ?? 0;
        return WifiAccessPoint(bssid: bssid, rssi: level);
      }).toList();
    } catch (e) {
      print('Wi‑Fi scan error: $e');
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(const SnackBar(content: Text('Error during Wi‑Fi scan.')));
      return <WifiAccessPoint>[];
    }
  }

  Future<bool> _ensureLocationPermission() async {
    // permission_handler ile konum izinini kontrol et ve gerekirse iste
    try {
      final status = await Permission.location.status;
      if (status.isGranted) return true;

      final result = await Permission.location.request();
      if (result.isGranted) return true;

      // Android 12+ için ek izinler gerekebilir; burada basit davranıyoruz
      return false;
    } catch (e) {
      print('Permission check error: $e');
      return false;
    }
  }

  void _attendWithWifi(BuildContext context) {
    final TextEditingController codeController = TextEditingController();

    showDialog(
      context: context,
      builder: (dialogContext) {
        return AlertDialog(
          title: const Text("Enter Attendance Code"),
          content: TextField(
            controller: codeController,
            decoration: const InputDecoration(hintText: "sessionId:token"),
          ),
          actions: [
            TextButton(
              onPressed: () async {
                Navigator.of(context).pop();

                final input = codeController.text.trim();
                if (input.isEmpty || !input.contains(':')) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text("Invalid input format")),
                  );
                  return;
                }

                final parts = input.split(':');
                final sessionId = int.tryParse(parts[0]) ?? 0;
                if (sessionId == 0) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text("Invalid session id")),
                  );
                  return;
                }

                try {
                  final aps = await _collectWifiAccessPoints();
                  if (aps.isEmpty) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(
                        content: Text("No Wi‑Fi access points found."),
                      ),
                    );
                    return;
                  }

                  final response = await AttendanceService().sendWifiScan(
                    sessionId: sessionId,
                    accessPoints: aps,
                  );

                  if (response.data['success'] == true) {
                    _showSuccessDialog(context, response);
                  } else {
                    _showErrorDialog(context, response);
                  }
                } catch (_) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text("Error sending Wi‑Fi scan.")),
                  );
                }
              },
              child: const Text("OK"),
            ),
          ],
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16.0),
            child: GestureDetector(
              onTap: () {
                showSearch(
                  context: context,
                  delegate: CustomSearchDelegate(
                    lectures: _attendedLectures,
                    onSearch: _filterLectures,
                    onLectureTap: _onLectureTap,
                  ),
                );
              },
              child: Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 16.0,
                  vertical: 8.0,
                ),
                width: 220.0,
                height: 40.0,
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(12.0),
                  boxShadow: [BoxShadow(color: Colors.black, blurRadius: 4.0)],
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.start,
                  children: [
                    const Icon(Icons.search, color: Colors.black, size: 28.0),
                    const SizedBox(width: 8.0),
                    Expanded(
                      child: Text(
                        "Search",
                        style: TextStyle(color: Colors.black),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ],
      ),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.symmetric(
              vertical: 16.0,
              horizontal: 16.0,
            ),
            child: Container(
              height: 48,
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(30),
                border: Border.all(color: Colors.grey.shade400),
              ),
              child: Row(
                children: [
                  Expanded(
                    child: GestureDetector(
                      onTap: () {
                        setState(() {
                          _selectedTab = 0;
                        });
                      },
                      child: Container(
                        decoration: BoxDecoration(
                          color: _selectedTab == 0 ? Colors.blue : Colors.white,
                          borderRadius: BorderRadius.circular(30),
                        ),
                        child: Center(
                          child: Text(
                            'Attend Course',
                            style: TextStyle(
                              color:
                                  _selectedTab == 0
                                      ? Colors.white
                                      : Colors.black,
                              fontWeight: FontWeight.bold,
                              fontSize: 15,
                            ),
                          ),
                        ),
                      ),
                    ),
                  ),
                  Expanded(
                    child: GestureDetector(
                      onTap: () {
                        setState(() {
                          _selectedTab = 1;
                        });
                      },
                      child: Container(
                        decoration: BoxDecoration(
                          color: _selectedTab == 1 ? Colors.blue : Colors.white,
                          borderRadius: BorderRadius.circular(30),
                        ),
                        child: Center(
                          child: Row(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              if (_selectedTab == 1) SizedBox(width: 4),
                              Text(
                                'Attended Courses',
                                style: TextStyle(
                                  color:
                                      _selectedTab == 1
                                          ? Colors.white
                                          : Colors.black,
                                  fontWeight: FontWeight.bold,
                                  fontSize: 15,
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
          Expanded(
            child:
                _selectedTab == 0
                    ? Center(
                      child: Padding(
                        padding: const EdgeInsets.all(24.0),
                        child: Column(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            SizedBox(
                              width: 340,
                              height: 70,
                              child: ElevatedButton.icon(
                                onPressed: () => _askForPassword(context),
                                icon: const Icon(
                                  Icons.lock,
                                  color: Colors.blue,
                                ),
                                label: const Text(
                                  "Attend using a password",
                                  style: TextStyle(
                                    fontSize: 18,
                                    color: Colors.blue,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                                style: ElevatedButton.styleFrom(
                                  padding: const EdgeInsets.symmetric(
                                    horizontal: 24,
                                    vertical: 16,
                                  ),
                                  backgroundColor: Colors.white,
                                  side: const BorderSide(color: Colors.blue),
                                  shape: RoundedRectangleBorder(
                                    borderRadius: BorderRadius.circular(12),
                                  ),
                                ),
                              ),
                            ),
                            const SizedBox(height: 20),
                            SizedBox(
                              width: 340,
                              height: 70,
                              child: ElevatedButton.icon(
                                onPressed: () => _openQRSystem(context),
                                icon: const Icon(
                                  Icons.qr_code_scanner,
                                  color: Colors.blue,
                                ),
                                label: const Text(
                                  "Scan QR Code",
                                  style: TextStyle(
                                    fontSize: 18,
                                    color: Colors.blue,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                                style: ElevatedButton.styleFrom(
                                  padding: const EdgeInsets.symmetric(
                                    horizontal: 24,
                                    vertical: 16,
                                  ),
                                  backgroundColor: Colors.white,
                                  side: const BorderSide(color: Colors.blue),
                                  shape: RoundedRectangleBorder(
                                    borderRadius: BorderRadius.circular(12),
                                  ),
                                ),
                              ),
                            ),
                            const SizedBox(height: 20),
                            SizedBox(
                              width: 340,
                              height: 70,
                              child: ElevatedButton.icon(
                                onPressed: () => _attendWithWifi(context),
                                icon: const Icon(
                                  Icons.wifi,
                                  color: Colors.blue,
                                ),
                                label: const Text(
                                  "Attend via Wi‑Fi",
                                  style: TextStyle(
                                    fontSize: 18,
                                    color: Colors.blue,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                                style: ElevatedButton.styleFrom(
                                  padding: const EdgeInsets.symmetric(
                                    horizontal: 24,
                                    vertical: 16,
                                  ),
                                  backgroundColor: Colors.white,
                                  side: const BorderSide(color: Colors.blue),
                                  shape: RoundedRectangleBorder(
                                    borderRadius: BorderRadius.circular(12),
                                  ),
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    )
                    : const AttendedCoursesList(),
          ),
        ],
      ),
    );
  }
}

class CustomSearchDelegate extends SearchDelegate<String> {
  final List<CourseListingDTO> lectures;
  final Function(String) onSearch;
  final Function(String, int) onLectureTap;

  CustomSearchDelegate({
    required this.lectures,
    required this.onSearch,
    required this.onLectureTap,
  });

  @override
  List<Widget> buildActions(BuildContext context) {
    return [
      IconButton(
        icon: const Icon(Icons.clear),
        onPressed: () {
          query = '';
          onSearch(query);
        },
      ),
    ];
  }

  @override
  Widget buildLeading(BuildContext context) {
    return IconButton(
      icon: const Icon(Icons.arrow_back),
      onPressed: () {
        close(context, '');
      },
    );
  }

  @override
  Widget buildResults(BuildContext context) {
    final results =
        lectures.where((lecture) {
          return lecture.lectureName.toLowerCase().contains(
            query.toLowerCase(),
          );
        }).toList();

    return ListView(
      children:
          results.map((lecture) {
            return ListTile(
              title: Text(lecture.lectureName),
              onTap: () {
                onLectureTap(lecture.lectureName, lecture.id);
                close(context, lecture.lectureName);
              },
            );
          }).toList(),
    );
  }

  @override
  Widget buildSuggestions(BuildContext context) {
    final suggestions =
        lectures.where((lecture) {
          return lecture.lectureName.toLowerCase().contains(
            query.toLowerCase(),
          );
        }).toList();

    return ListView(
      children:
          suggestions.map((lecture) {
            return ListTile(
              title: Text(lecture.lectureName),
              onTap: () {
                query = lecture.lectureName;
                onSearch(query);
                onLectureTap(lecture.lectureName, lecture.id);
                showResults(context);
              },
            );
          }).toList(),
    );
  }
}
