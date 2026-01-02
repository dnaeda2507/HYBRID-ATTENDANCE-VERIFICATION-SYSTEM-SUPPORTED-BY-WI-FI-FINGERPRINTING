import 'package:flutter/material.dart';

class DynamicSizePage extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    // Ekranın genişlik ve yüksekliğini alıyoruz.
    double screenWidth = MediaQuery.of(context).size.width;
    double screenHeight = MediaQuery.of(context).size.height;

    return Scaffold(
      appBar: AppBar(
        title: const Text("Dynamic Size with MediaQuery"),
      ),
      body: Center(
        child: Container(
          // Ekranın genişliğine göre genişlik ayarlıyoruz (ekranın %80'i gibi).
          width: screenWidth * 0.8,  // %80 genişlik
          height: screenHeight * 0.3, // %30 yükseklik
          color: Colors.blue,
          child: const Center(
            child: Text(
              "Dynamically Sized Container",
              style: TextStyle(color: Colors.white, fontSize: 18),
            ),
          ),
        ),
      ),
    );
  }
}

void main() {
  runApp(MaterialApp(home: DynamicSizePage()));
}
