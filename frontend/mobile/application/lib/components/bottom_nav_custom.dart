import 'package:flutter/material.dart';

class BottomNavCustom extends StatelessWidget {
  final int selectedIndex;
  final Function(int) onItemTapped;
  final BuildContext context;

  BottomNavCustom({
    Key? key,
    required this.selectedIndex,
    required this.onItemTapped,
    required this.context,
  }) : super(key: key);

  final Color backgroundColorNav = Colors.white;
  final Color selectedColor = Colors.blue;  
  final List<NavigationItem> items = [
    NavigationItem(
      icon: Icon(Icons.home),
      title: Text('Home'),
      color: Color.fromRGBO(223, 215, 243, 1),
    ),
    NavigationItem(
      icon: Icon(Icons.check_circle),
      title: Text('Attended'),
      color: Color.fromRGBO(223, 215, 243, 1),
    ),
    NavigationItem(
      icon: Icon(Icons.menu_book),
      title: Text('Courses'),
      color: Color.fromRGBO(223, 215, 243, 1),
    ),
  ];

  Widget _buildItem(BuildContext context, NavigationItem item, bool isSelected, int index) {
    return GestureDetector(
      onTap: () => onItemTapped(index),
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 250),
        height: 50,
        width: isSelected ? MediaQuery.of(context).size.width / 3.5 : 50,
        padding: isSelected ? const EdgeInsets.symmetric(horizontal: 4) : null,
        decoration: isSelected
            ? BoxDecoration(
                color: item.color,
                borderRadius: const BorderRadius.all(Radius.circular(50)),
              )
            : null,
        child: Row(
          mainAxisAlignment: MainAxisAlignment.center,
          mainAxisSize: MainAxisSize.min,
          children: [
            IconTheme(
              data: IconThemeData(
                size: 24,
                color: isSelected ? selectedColor : Colors.black,
              ),
              child: item.icon,
            ),
            if (isSelected)
              Flexible(
                child: Padding(
                  padding: const EdgeInsets.only(left: 4),
                  child: DefaultTextStyle.merge(
                    style: TextStyle(
                      color: selectedColor,
                      fontSize: 12,
                    ),
                    overflow: TextOverflow.ellipsis,
                    child: item.title,
                  ),
                ),
              ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 56,
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: const BoxDecoration(
        color: Colors.white,
        boxShadow: [BoxShadow(color: Colors.black12, blurRadius: 4)],
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
        children: items.asMap().entries.map((entry) {
          final int idx = entry.key;
          final item = entry.value;
          return _buildItem(context, item, selectedIndex == idx, idx);
        }).toList(),
      ),
    );
  }
}

class NavigationItem {
  final Icon icon;
  final Text title;
  final Color color;

  const NavigationItem({
    required this.icon,
    required this.title,
    required this.color,
  });
}
