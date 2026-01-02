import 'package:flutter/material.dart';
import 'package:application/screens/home/home.dart';

void navigateToTab(BuildContext context, int index) {
  Navigator.pushReplacement(
    context,
    MaterialPageRoute(
      builder: (context) => Home(initialIndex: index),
    ),
  );
}