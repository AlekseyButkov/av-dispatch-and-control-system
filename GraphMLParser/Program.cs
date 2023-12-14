// See https://aka.ms/new-console-template for more information

var graphmlFilePath = args[0];
var parser = new GraphMLParser.GraphMLParser();
var parseResults = parser.ParseFile(graphmlFilePath);
