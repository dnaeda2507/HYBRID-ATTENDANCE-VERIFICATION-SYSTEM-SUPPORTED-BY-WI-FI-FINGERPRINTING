
import 'package:application/screens/attended/attendedPage.dart';
import 'package:application/screens/lecture/lecturePage.dart';
import 'package:flutter/material.dart';
import 'screens/home/home.dart';
import 'screens/loginPage.dart';

class AppWidget extends StatelessWidget {
   final bool isLoggedIn;
   const AppWidget({Key? key, required this.isLoggedIn}) : super(key: key);


  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      routes: {
    '/home': (context) => Home(),
    '/attended': (context) => AttendedPage(),
    '/courses': (context) => LecturePage(),
  },

      debugShowCheckedModeBanner: false,
      theme: ThemeData.from(
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color.fromRGBO(32, 63, 129, 1.0),
        ),
      ),
      home: isLoggedIn ? Home() : const Login(),
    );
  }
}