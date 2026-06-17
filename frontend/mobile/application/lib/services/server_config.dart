import 'dart:io' show Platform;

import 'package:flutter/foundation.dart';
import 'package:hive_flutter/hive_flutter.dart';

class ServerConfig {
  static const _boxName = 'settings';
  static const _key = 'server_url';

  /// Android emulator: 10.0.2.2 = bilgisayarınızın localhost'u
  /// iOS simulator: localhost çalışır
  /// Fiziksel telefon: Settings'ten bilgisayarınızın yerel IP'sini girin (ör. 192.168.1.42:9001)
  /// Canlı sunucu: http://34.38.191.19:9001
  ///
  /// Not: baseUrl /api içermez; servisler path'te /api/... kullanır.
  static String get _defaultUrl {
    if (!kIsWeb && Platform.isAndroid) {
      return 'http://10.0.2.2:9001';
    }
    return 'http://localhost:9001';
  }

  /// Eski kayıtlarda veya kullanıcı girişinde /api suffix'i olabilir — kaldır.
  static String _normalize(String url) {
    var normalized = url.trim().replaceAll(RegExp(r'/+$'), '');
    if (normalized.endsWith('/api')) {
      normalized = normalized.substring(0, normalized.length - 4);
    }
    return normalized;
  }

  static String get baseUrl {
    final box = Hive.box(_boxName);
    final stored = box.get(_key) as String?;
    if (stored == null || stored.isEmpty) {
      return _defaultUrl;
    }

    var url = _normalize(stored);

    // Eski kayıtlı localhost adresini Android emulator'de düzelt
    if (!kIsWeb && Platform.isAndroid && url.contains('localhost')) {
      url = url.replaceFirst('localhost', '10.0.2.2');
    }

    // Hive'daki eski /api suffix'li değeri bir kez düzelt
    if (stored != url) {
      box.put(_key, url);
    }

    return url;
  }

  static Future<void> setBaseUrl(String url) async {
    final box = Hive.box(_boxName);
    final normalized = _normalize(url);
    await box.put(_key, normalized);
  }
}
