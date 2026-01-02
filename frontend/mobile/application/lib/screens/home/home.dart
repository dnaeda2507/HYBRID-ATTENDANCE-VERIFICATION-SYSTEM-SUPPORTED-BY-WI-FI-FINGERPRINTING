import 'package:application/components/flutter_search_delegate_sample.dart';
import 'package:application/screens/settings/security_page.dart';
import 'package:flutter/material.dart';
import 'package:hive_flutter/hive_flutter.dart';
import '../loginPage.dart';
import '../attended/attendedPage.dart';
import '../lecture/lecturePage.dart';


import '../../components/bottom_nav_custom.dart';
import '../../services/user_service.dart';
import '../../models/user.dart';

class Home extends StatefulWidget {
  final int initialIndex;

  const Home({super.key, this.initialIndex = 0});

  @override
  _HomeState createState() => _HomeState();
}

class _HomeState extends State<Home> {
  late int _selectedIndex;
  final UserService _userService = UserService();
  User? _user;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _selectedIndex = widget.initialIndex;
    _loadUserData();
  }

  Future<void> _loadUserData() async {
    try {
      final user = await _userService.getCurrentUser();
      if (mounted) {
        setState(() {
          _user = user;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Error loading user data: $e')),
        );
      }
    }
  }

  void _onLogout() {
    final box = Hive.box('login');
    box.clear();
    box.put('loginStatus', false);
    Navigator.pushReplacement(
      context,
      MaterialPageRoute(builder: (context) => const Login()),
    );
  }

  List<Widget> _pages() => [
  _user == null
      ? const Center(child: Text('No user data'))
      : HomePageContent(user: _user!), 
  const AttendedPage(),
  const LecturePage(),
];

  final List<String> _pageTitles = [
    " ",
    "Attended Courses",
    "Courses",
  ];

  void _onItemTapped(int index) {
    setState(() {
      _selectedIndex = index;
    });
  }

  Future<bool> _onWillPop() async {
    if (_selectedIndex != 0) {
      setState(() {
        _selectedIndex = 0;
      });
      return false;
    }
    return true;
  }

  @override
  Widget build(BuildContext context) {
    return WillPopScope(
      onWillPop: _onWillPop,
      child: Scaffold(
        appBar: AppBar(
          backgroundColor: Colors.blue,
          foregroundColor: Colors.white,
          iconTheme: const IconThemeData(color: Colors.white),

          leadingWidth: _selectedIndex == 0 ? 120 : 56, 

          leading: _selectedIndex == 0
              ? Row(
                  mainAxisAlignment: MainAxisAlignment.start,
                  children: [
                    const SizedBox(width: 8),
                    IconButton(
                      icon: const Icon(Icons.settings),
                      onPressed: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(builder: (context) => const SettingsPage()),
                        );
                      },
                    ),
                    
                  ],
                )
              : IconButton(
                  icon: const Icon(Icons.arrow_back),
                  onPressed: () {
                    setState(() {
                      _selectedIndex = 0;
                    });
                  },
                ),

          title: Text(
            _pageTitles[_selectedIndex],
            style: const TextStyle(color: Colors.white),
          ),
          actions: [
            if (_selectedIndex == 0)
              IconButton(
                icon: const Icon(Icons.exit_to_app),
                onPressed: _onLogout,
              ),
          ],
        ),
        body: _isLoading
            ? const Center(child: CircularProgressIndicator())
             : _pages()[_selectedIndex],
          
               
        bottomNavigationBar: BottomNavCustom(
          selectedIndex: _selectedIndex,
          onItemTapped: (index) {
            if (index == 3) {
              showSearch(
                context: context,
                delegate: Search([
                  'item 1',
                  'item 2',
                  'item 3',
                  'item 4',
                  'item 5',
                ]),
              );
            } else {
              _onItemTapped(index);
            }
          },
          context: context,
        ),
      ),
    );
  }

  Widget _buildInfoCard(String title, List<Widget> children) {
    return Card(
      elevation: 4,
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              title,
              style: Theme.of(context).textTheme.titleLarge,
            ),
            const Divider(),
            ...children,
          ],
        ),
      ),
    );
  }

  Widget _buildInfoRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8.0),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 100,
            child: Text(
              label,
              style: const TextStyle(
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
          Expanded(
            child: Text(value),
          ),
        ],
      ),
    );
  }
}

class HomePageContent extends StatelessWidget {
  final User user;

  const HomePageContent({super.key, required this.user});

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Container(
          color: Colors.blue,
          width: double.infinity,
          height: 220,
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const CircleAvatar(
                radius: 36,
                backgroundColor: Colors.white,
                child: Icon(
                  Icons.person,
                  size: 40,
                  color: Colors.blue,
                ),
              ),
              const SizedBox(height: 16),
           
              Text(
                '${user.firstName} ${user.lastName}',
                style: const TextStyle(
                  color: Colors.white,
                  fontWeight: FontWeight.bold,
                  fontSize: 18,
                ),
              ),
              const SizedBox(height: 4),
              Text(
                user.email,
                style: const TextStyle(
                  color: Colors.white,
                  fontWeight: FontWeight.w500,
                  fontSize: 13,
                ),
              ),
              const SizedBox(height: 2),
            ],
          ),
        ),
        Expanded(
          child: Container(
            width: double.infinity,
            color: Colors.transparent,
            child: Center(
              child: Container(
                width: 230,
                height: 230,
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(140),
                  image: const DecorationImage(
                   image: const AssetImage('assets/images/loginlogo.png'),
                    fit: BoxFit.cover, 
                  ),
                ),
              ),
            ),
          ),
        ),
      ],
    );
  }
}
