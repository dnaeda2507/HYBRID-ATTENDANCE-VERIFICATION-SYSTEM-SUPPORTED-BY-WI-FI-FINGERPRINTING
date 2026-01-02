import 'package:application/models/lecture.dart';
import 'package:application/screens/lecture/lecture_details.dart';
import 'package:application/services/lecture_service.dart';
import 'package:application/services/lecture_details_service.dart';
import 'package:flutter/material.dart';

class LecturePage extends StatefulWidget {
  const LecturePage({super.key});

  @override
  State<LecturePage> createState() => _LecturePageState();
}

class _LecturePageState extends State<LecturePage> {
  List<CourseListingDTO> _lectures = [];
  List<CourseListingDTO> _filteredLectures = [];
  final LectureService _lectureService = LectureService();
  final LectureDetailsService _lectureDetailsService = LectureDetailsService();

  @override
  void initState() {
    super.initState();
    _loadLectures();
  }

  Future<void> _loadLectures() async {
    try {
      final lectures = await _lectureService.getLectures();
      setState(() {
        _lectures = lectures;
        _filteredLectures = lectures;
      });
    } catch (e) {
      print('Error loading lectures: $e');
    }
  }

  void _filterLectures(String query) {
    final filtered =
        _lectures.where((lecture) {
          return lecture.lectureName.toLowerCase().contains(
            query.toLowerCase(),
          );
        }).toList();

    setState(() {
      _filteredLectures = filtered;
    });
  }

  Future<void> _onLectureTap(String lectureTitle, int lectureId) async {
    try {
      final data = await _lectureDetailsService.getLectureDetailsById(
        lectureId,
      );
       print('Lecture details raw data: $data');
      
     final lectureDetails = LectureDetails.fromJson(data!['data']);

      Navigator.push(
        context,
        MaterialPageRoute(
          builder:
                (context) => LectureDetailsPage(lectureDetails: lectureDetails),
        ),
      );
    } catch (e) {
      print('Error fetching lecture details: $e');
    }
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
                    lectures: _lectures,
                    onSearch: _filterLectures,
                    onLectureTap: _onLectureTap, // _onLectureTap fonksiyonunu ekliyoruz
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
      body:
          _lectures.isEmpty
              ? const Center(child: CircularProgressIndicator())
              : GridView.builder(
                gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                  crossAxisCount: 1,
                  crossAxisSpacing: 10,
                  mainAxisSpacing: 10,
                  childAspectRatio: 12,
                ),
                itemCount: _filteredLectures.length,
                itemBuilder: (context, index) {
                  return GestureDetector(
                    onTap: () {
                      final lecture = _filteredLectures[index];
                      final lectureId = _filteredLectures[index].id;
                      _onLectureTap(lecture.lectureName, lectureId);
                    },
                    child: Container(
                      width: 480.0,
                      height: 25.0,
                      decoration: BoxDecoration(
                        color: const Color.fromARGB(255, 241, 242, 244),
                        borderRadius: BorderRadius.circular(15.0),
                      ),
                      child: Align(
                        alignment: Alignment.centerLeft,
                        child: Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 16.0),
                          child: Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Text(
                                _filteredLectures[index].lectureName,
                                style: const TextStyle(
                                  color: Colors.black,
                                  fontSize: 14,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                              Text(
                                "Details >",
                                style: TextStyle(
                                  color: Colors.blueGrey[700],
                                  fontSize: 12,
                                  fontWeight: FontWeight.w500,
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ),
                  );
                },
              ),
    );
  }
}

class CustomSearchDelegate extends SearchDelegate<String> {
  final List<CourseListingDTO> lectures;
  final Function(String) onSearch;
  final Function(String, int) onLectureTap; // _onLectureTap fonksiyonunu ekliyoruz

  CustomSearchDelegate({
    required this.lectures,
    required this.onSearch,
    required this.onLectureTap, // Constructor'a ekliyoruz
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
    final results = lectures.where((lecture) {
      return lecture.lectureName.toLowerCase().contains(
            query.toLowerCase(),
          );
    }).toList();

    return ListView(
      children: results.map((lecture) {
        return ListTile(
          title: Text(lecture.lectureName),
          onTap: () {
            onLectureTap(lecture.lectureName, lecture.id); // _onLectureTap'i çağır
            close(context, lecture.lectureName);
          },
        );
      }).toList(),
    );
  }

  @override
  Widget buildSuggestions(BuildContext context) {
    final suggestions = lectures.where((lecture) {
      return lecture.lectureName.toLowerCase().contains(
            query.toLowerCase(),
          );
    }).toList();

    return ListView(
      children: suggestions.map((lecture) {
        return ListTile(
          title: Text(lecture.lectureName),
          onTap: () {
            query = lecture.lectureName;
            onSearch(query);
            onLectureTap(lecture.lectureName, lecture.id); // _onLectureTap'i çağır
            showResults(context);
          },
        );
      }).toList(),
    );
  }
}