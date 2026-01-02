import 'package:flutter/material.dart';
import 'package:mobile_scanner/mobile_scanner.dart';

class QRViewPage extends StatelessWidget {
  const QRViewPage({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text("Scan QR Code")),
      body: MobileScanner(
  onDetect: (BarcodeCapture capture) {
    final List<Barcode> barcodes = capture.barcodes;
    for (final barcode in barcodes) {
     final code = barcode.rawValue;
            if (code != null) {
              Navigator.pop(context, code); 
              break; 
            }
    }
  },
)

    );
  }
}
