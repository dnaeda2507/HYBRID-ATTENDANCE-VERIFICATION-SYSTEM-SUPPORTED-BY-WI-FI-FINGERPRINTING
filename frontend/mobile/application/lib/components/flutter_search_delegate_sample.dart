import 'package:flutter/material.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Flutter Demo',
      themeMode: ThemeMode.dark,
      darkTheme: ThemeData.dark(),
      theme: ThemeData(
        primarySwatch: Colors.blue,
        visualDensity: VisualDensity.adaptivePlatformDensity,
      ),
      home: Scaffold(
        appBar: AppBar(
          title: const Text('Search demo'),
          actions: [
            Builder(builder: (context) {
              return IconButton(
                onPressed: () {
                  showSearch(
                      context: context,
                      delegate: Search([
                        'item 1',
                        'item 2',
                        'item 3',
                        'item 4',
                        'item 5',
                        'item 6',
                        'item 7',
                        'item 8',
                        'item 9',
                        'item 10',
                      ]));
                },
                icon: const Icon(Icons.search),
              );
            }),
          ],
        ),
        body: Center(
          child: ListView.builder(
            itemCount: 100,
            itemBuilder: (context, index) => ListTile(
              title: Text('item $index'),
            ),
          ),
        ),
      ),
    );
  }
}

class Search extends SearchDelegate {
  String selectedResult = "";
  final List<String> listData;
  List<String> recentList = ["Recent 1", "Recent 2"];

  Search(this.listData);

  @override
  List<Widget> buildActions(BuildContext context) {
    return <Widget>[
      IconButton(
        icon: const Icon(Icons.close),
        onPressed: () {
          query = "";
        },
      ),
    ];
  }

  @override
  Widget buildLeading(BuildContext context) {
    return IconButton(
      icon: const Icon(Icons.arrow_back),
      onPressed: () {
        Navigator.pop(context);
      },
    );
  }

  @override
  Widget buildResults(BuildContext context) {
    return Center(
      child: Text(selectedResult),
    );
  }

  @override
  Widget buildSuggestions(BuildContext context) {
    List<String> suggestionList = [];
   
    query.isEmpty
        ? suggestionList = recentList
        : suggestionList.addAll(listData.where(
            (element) => element.toLowerCase().contains(query.toLowerCase()), // Case-insensitive search
          ));

    return ListView.builder(
      itemCount: suggestionList.length,
      itemBuilder: (context, index) {
        return ListTile(
          title: Text(
            suggestionList[index],
          ),
          leading: query.isEmpty ? const Icon(Icons.access_time) : const SizedBox.shrink(),
          onTap: () {
            selectedResult = suggestionList[index];
            showResults(context);
          },
        );
      },
    );
  }
}
