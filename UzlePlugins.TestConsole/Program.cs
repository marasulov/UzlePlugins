// See https://aka.ms/new-console-template for more information

using System.Collections.ObjectModel;
using UzlePlugins.Models;
using UzlePlugins.Views;
using UzlePlugins.Vm;

Console.WriteLine("Hello, World!");


var holes = new ObservableCollection<HoleModel>
{
    new HoleModel("first","first description","circle", 25, "wall", "wall material", true, 10, true),
    new HoleModel("second","second description","circle", 25, "wall", "wall material", true, 10, true),
    new HoleModel("third","third description","circle", 25, "wall", "wall material", true, 10, true),
    new HoleModel("fourth","fourth description","circle", 25, "wall", "wall material", true, 10, true)
};





