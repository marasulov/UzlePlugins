// See https://aka.ms/new-console-template for more information

using System.Collections.ObjectModel;
using UzlePlugins.Models;



var holes = new ObservableCollection<HoleModel>
{
    //new HoleModel("first","first description","circle", 25, "wall", "wall material", true, 10, true),
    //new HoleModel("second","second description","circle", 25, "wall", "wall material", true, 10, true),
    //new HoleModel("third","third description","circle", 25, "wall", "wall material", true, 10, true),
    //new HoleModel("fourth","fourth description","circle", 25, "wall", "wall material", true, 10, true)


};
//SettingsReader reader = new SettingsReader();
//reader.GetFamilyTypes();


var source = new List<string>() { "a", "b", "c" };
var compare = new List<string>() { "b", "c", "d" };
var result = source.Intersect(compare);

var old = source.Except(compare);
var newP = compare.Except(source);
Console.WriteLine("Hello, World!");

