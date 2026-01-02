import 'package:application/models/user.dart' show User;
import 'package:application/services/user_service.dart';
import 'package:flutter/material.dart';

class AccountPage extends StatefulWidget {
  const AccountPage({Key? key}) : super(key: key);

  @override
  State<AccountPage> createState() => _AccountPageState();
}

class _AccountPageState extends State<AccountPage> {
  final _formKey = GlobalKey<FormState>();
  final UserService _userService = UserService();

  final TextEditingController nameController = TextEditingController();
  final TextEditingController surnameController = TextEditingController();
  final TextEditingController infoEmailController = TextEditingController();
  final TextEditingController phoneController = TextEditingController();
  final TextEditingController studentEmailController = TextEditingController();

  bool isSaved = false;
  bool isLoading = true;
  User? user;

  @override
  void initState() {
    super.initState();
    _loadUserData();
  }

  Future<void> _loadUserData() async {
    user = await _userService.getCurrentUser();
    if (user != null) {
      setState(() {
        print("Information Email: ${user!.informationMail}");
        print("Phone Number: ${user!.phoneNumber}");

        nameController.text = user!.firstName;
        surnameController.text = user!.lastName;
        infoEmailController.text = user!.informationMail;
        phoneController.text = user!.phoneNumber;
        studentEmailController.text = user!.email;
        isLoading = false;
      });
    } else {
      setState(() {
        isLoading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    if (isLoading) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    return Scaffold(
      appBar: AppBar(leading: const BackButton(), title: const Text("Account")),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          children: [
            const SizedBox(height: 16),
            Form(
              key: _formKey,
              child: Expanded(
                child: SingleChildScrollView(
                  child: Card(
                    elevation: 2,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(8),
                      side: const BorderSide(color: Colors.black12),
                    ),
                    child: Padding(
                      padding: const EdgeInsets.all(16.0),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Text(
                            'User Details',
                            style: TextStyle(
                              fontWeight: FontWeight.bold,
                              fontSize: 16,
                            ),
                          ),
                          const SizedBox(height: 16),
                          Row(
                            children: [
                              Expanded(
                                child: TextFormField(
                                  controller: nameController,
                                  enabled: false,
                                  decoration: const InputDecoration(
                                    labelText: 'Name',
                                    filled: true,
                                  ),
                                ),
                              ),
                              const SizedBox(width: 16),
                              Expanded(
                                child: TextFormField(
                                  controller: surnameController,
                                  enabled: false,
                                  decoration: const InputDecoration(
                                    labelText: 'Surname',
                                    filled: true,
                                  ),
                                ),
                              ),
                            ],
                          ),
                          const SizedBox(height: 16),
                          TextFormField(
                            controller: infoEmailController,
                            decoration: const InputDecoration(
                              labelText: 'Information Email',
                              filled: true,
                              hintText: 'Enter your information email',
                            ),
                          ),
                          const SizedBox(height: 16),
                          TextFormField(
                            controller: studentEmailController,
                            enabled: false,
                            decoration: const InputDecoration(
                              labelText: 'Student Email',
                              filled: true,
                            ),
                          ),
                          const SizedBox(height: 16),
                          TextFormField(
                            controller: phoneController,
                            decoration: const InputDecoration(
                              labelText: 'Phone Number',
                              filled: true,
                              hintText: 'Enter your phone number',
                            ),
                          ),
                          const SizedBox(height: 24),
                          SizedBox(
                            width: double.infinity,
                            child: ElevatedButton(
                              style: ElevatedButton.styleFrom(
                                backgroundColor: Colors.blue,
                              ),
                              onPressed: () async {
                                final success = await _userService.updateUser(
                                  firstName: nameController.text,
                                  lastName: surnameController.text,
                                  email: studentEmailController.text,
                                  informationMail: infoEmailController.text,
                                  phoneNumber: phoneController.text,
                                );

                                if (success) {
                                  setState(() {
                                    isSaved = true;
                                  });
                                } else {
                                  ScaffoldMessenger.of(context).showSnackBar(
                                    const SnackBar(
                                      content: Text("Failed to update profile"),
                                    ),
                                  );
                                }
                              },
                              child: const Text(
                                'Save',
                                style: TextStyle(color: Colors.white),
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              ),
            ),
            if (isSaved)
              Container(
                margin: const EdgeInsets.only(top: 16),
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: Colors.green.shade50,
                  border: Border.all(color: Colors.green.shade200),
                  borderRadius: BorderRadius.circular(4),
                ),
                child: Row(
                  children: const [
                    Icon(Icons.check_circle, color: Colors.green),
                    SizedBox(width: 8),
                    Expanded(
                      child: Text(
                        'Successfully Saved. Your profile settings have been saved.',
                      ),
                    ),
                    Icon(Icons.close, color: Colors.black45),
                  ],
                ),
              ),
          ],
        ),
      ),
    );
  }
}
