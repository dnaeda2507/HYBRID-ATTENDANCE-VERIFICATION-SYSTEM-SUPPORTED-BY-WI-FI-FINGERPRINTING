import 'package:application/screens/loginPage.dart';
import 'package:application/components/theme_provider.dart';
import 'package:flutter/material.dart';
import 'package:hive_flutter/hive_flutter.dart';
import 'package:provider/provider.dart'; 
import 'screens/home/home.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await _initHive();

  final Box _boxLogin = Hive.box("login");
  bool isLoggedIn = _boxLogin.get("loginStatus") ?? false;

  runApp(
    ChangeNotifierProvider(
      create: (_) => ThemeProvider(),
      child: mainApp(isLoggedIn: isLoggedIn),
    ),
  );
}

Future<void> _initHive() async {
  await Hive.initFlutter();
  await Hive.openBox("login");
  await Hive.openBox("accounts");
}

class mainApp extends StatefulWidget {
  final bool isLoggedIn;
  const mainApp({super.key, required this.isLoggedIn});

  @override
  State<mainApp> createState() => _mainAppState();
}

class _mainAppState extends State<mainApp> {
  @override
  Widget build(BuildContext context) {
    final themeProvider = Provider.of<ThemeProvider>(context);

    return MaterialApp(
      debugShowCheckedModeBanner: false,
      theme: ThemeData.light(),
      darkTheme: ThemeData.dark(),
      themeMode: themeProvider.isDarkMode ? ThemeMode.dark : ThemeMode.light,
      home: widget.isLoggedIn ? const Home() : const Login(),
    );
  }
}

class MainScreen extends StatefulWidget {
  @override
  _MainScreenState createState() => _MainScreenState();
}

class _MainScreenState extends State<MainScreen> {
  int _selectedIndex = 0;

  void _onItemTapped(int index) {
    setState(() {
      _selectedIndex = index;
    });
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context); 

    return Scaffold(
      backgroundColor: theme.scaffoldBackgroundColor, 
      appBar: AppBar(
        backgroundColor: theme.appBarTheme.backgroundColor ?? theme.primaryColor,
        title: Text(
          'Ana Ekran',
          style: theme.textTheme.titleLarge?.copyWith(
            color: theme.appBarTheme.foregroundColor ?? theme.primaryTextTheme.titleLarge?.color,
          ),
        ),
      ),
      body: Center(
        child: Text(
          'Seçilen Sayfa: $_selectedIndex',
          style: theme.textTheme.bodyLarge, 
        ),
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _selectedIndex,
        onTap: _onItemTapped,
        items: const <BottomNavigationBarItem>[
          BottomNavigationBarItem(
            icon: Icon(Icons.home),
            label: 'Home',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.check_circle),
            label: 'Attended',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.menu_book),
            label: 'Lectures',
          ),
        ],
      ),
    );
  }
}
