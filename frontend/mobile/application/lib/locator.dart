import 'package:get_it/get_it.dart';
import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import 'package:application/services/token_service.dart';

final GetIt getIt = GetIt.instance;

void setupLocator() {

  getIt.registerLazySingleton(() => Dio());
  getIt.registerLazySingleton(() => FlutterSecureStorage());
  getIt.registerLazySingleton(() => TokenService(getIt<FlutterSecureStorage>()));
}
