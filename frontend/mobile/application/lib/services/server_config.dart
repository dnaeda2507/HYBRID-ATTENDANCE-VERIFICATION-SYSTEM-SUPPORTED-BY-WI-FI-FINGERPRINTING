import 'package:hive_flutter/hive_flutter.dart';

class ServerConfig {
  static const _boxName = 'settings';
  static const _key = 'server_url';
  static const _defaultUrl = 'http://35.204.145.218:9001';

  static String get baseUrl {
    final box = Hive.box(_boxName);
    return box.get(_key, defaultValue: _defaultUrl) as String;
  }

  static Future<void> setBaseUrl(String url) async {
    final box = Hive.box(_boxName);
    await box.put(_key, url.trimRight().replaceAll(RegExp(r'/+$'), ''));
  }
}
