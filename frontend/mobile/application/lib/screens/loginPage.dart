
import 'package:application/screens/forget_passwrd.dart';
import 'package:flutter/material.dart';
import 'package:hive_flutter/hive_flutter.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'home/home.dart';

import 'package:application/services/auth_service.dart';

class Login extends StatefulWidget {
  const Login({Key? key}) : super(key: key);

  @override
  State<Login> createState() => _LoginState();
}

class _LoginState extends State<Login> {
  final GlobalKey<FormState> _formKey = GlobalKey();
  final FocusNode _focusNodePassword = FocusNode();
  final TextEditingController _controllerUsername = TextEditingController();
  final TextEditingController _controllerPassword = TextEditingController();

  final Box _boxLogin = Hive.box("login");

  final AuthService _authService = AuthService();

  bool _obscurePassword = true;
  bool isChecked = false;
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _loadRemember();
    if (_boxLogin.get("loginStatus") ?? false) {
      Future.microtask(() {
        Navigator.pushReplacement(
          context,
          MaterialPageRoute(builder: (_) => const Home()),
        );
      });
    }
  }

  Future<void> _loadRemember() async {
    final prefs = await SharedPreferences.getInstance();
    setState(() {
      isChecked = prefs.getBool('remember') ?? false;
      if (isChecked) {
        _controllerUsername.text = prefs.getString('email') ?? "";
        _controllerPassword.text = prefs.getString('password') ?? "";
      }
    });
  }

  Future<void> _saveRemember() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool('remember', isChecked);
    if (isChecked) {
      await prefs.setString('email', _controllerUsername.text);
      await prefs.setString('password', _controllerPassword.text);
    } else {
      await prefs.remove('email');
      await prefs.remove('password');
    }
  }

  Future<void> _login() async {
    if (!(_formKey.currentState?.validate() ?? false)) return;

    setState(() => _isLoading = true);
    await _saveRemember();

    final email = _controllerUsername.text.trim();
    final password = _controllerPassword.text.trim();

    try {
      final success = await _authService.login(email, password);
      if (success) {
        _boxLogin.put("loginStatus", true);
        _boxLogin.put("userName", email);
        Navigator.pushReplacement(
          context,
          MaterialPageRoute(builder: (_) => const Home()),
        );
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Login failed.")),
        );
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("An error occurred: $e")),
      );
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_boxLogin.get("loginStatus") ?? false) {
      return const Home();
    }

    return Scaffold(
      backgroundColor: Colors.white,
      body: Stack(
        children: [
          _buildForm(),
          if (_isLoading)
            Container(
              color: Colors.black54,
              child: const Center(child: CircularProgressIndicator()),
            ),
        ],
      ),
    );
  }

  Widget _buildForm() {
    return Form(
      key: _formKey,
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(30.0),
        child: Column(
          children: [
            const SizedBox(height: 50),
            Image.asset('assets/images/loginlogo.png', height: 100.0),
            const SizedBox(height: 30),
            Text("Welcome", style: Theme.of(context).textTheme.headlineLarge),
            const SizedBox(height: 10),
            Text("Login to your account", style: Theme.of(context).textTheme.bodyMedium),
            const SizedBox(height: 60),
            TextFormField(
              controller: _controllerUsername,
              keyboardType: TextInputType.name,
              decoration: InputDecoration(
                labelText: "Email",
                prefixIcon: const Icon(Icons.email),
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(10)),
                enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(10)),
              ),
              onEditingComplete: () => _focusNodePassword.requestFocus(),
              validator: (String? value) {
                if (value == null || value.isEmpty) {
                  return "Please enter email.";
                }
                return null;
              },
            ),
            const SizedBox(height: 10),
            TextFormField(
              controller: _controllerPassword,
              focusNode: _focusNodePassword,
              obscureText: _obscurePassword,
              keyboardType: TextInputType.visiblePassword,
              decoration: InputDecoration(
                labelText: "Password",
                prefixIcon: const Icon(Icons.lock),
                suffixIcon: IconButton(
                  onPressed: () => setState(() => _obscurePassword = !_obscurePassword),
                  icon: Icon(
                    _obscurePassword ? Icons.visibility_outlined : Icons.visibility_off_outlined
                  ),
                ),
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(8)),
                enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(8)),
              ),
              validator: (String? value) {
                if (value == null || value.isEmpty) {
                  return "Please enter password.";
                }
                return null;
              },
            ),
            const SizedBox(height: 10),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Row(
                  children: [
                    Checkbox(
                      value: isChecked,
                      activeColor: Colors.blue,
                      onChanged: (bool? value) => setState(() => isChecked = value!),
                    ),
                    const Text("Remember me", style: TextStyle(fontSize: 14)),
                  ],
                ),
                TextButton(
                  onPressed: () => Navigator.push(
                    context,
                    MaterialPageRoute(builder: (_) => const ForgotPassword()),
                  ),
                  child: const Text("Forgot Password?", style: TextStyle(fontSize: 14, color: Colors.blue)),
                ),
              ],
            ),
            const SizedBox(height: 40),
            ElevatedButton(
              style: ElevatedButton.styleFrom(
                foregroundColor: Colors.white,
                minimumSize: const Size.fromHeight(50),
                backgroundColor: Colors.blue,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
              ),
              onPressed: _login,
              child: const Text("Login"),
            ),
          ],
        ),
      ),
    );
  }

  @override
  void dispose() {
    _focusNodePassword.dispose();
    _controllerUsername.dispose();
    _controllerPassword.dispose();
    super.dispose();
  }
}