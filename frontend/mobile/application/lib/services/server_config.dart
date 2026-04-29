import 'package:hive_flutter/hive_flutter.dart';

class ServerConfig {
  static const _boxName = 'settings';
  static const _key = 'server_url';
  // Development: localhost:5000 (iOS sim) veya 10.0.2.2:5000 (Android emulator)
  // Production: http://34.38.191.19:9001
  static const _defaultUrl = 'http://localhost:5000';

  static String get baseUrl {
    final box = Hive.box(_boxName);
    return box.get(_key, defaultValue: _defaultUrl) as String;
  }

  static Future<void> setBaseUrl(String url) async {
    final box = Hive.box(_boxName);
    await box.put(_key, url.trimRight().replaceAll(RegExp(r'/+$'), ''));
  }
}