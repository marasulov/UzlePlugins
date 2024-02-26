// See https://aka.ms/new-console-template for more information

using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
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

// Create a list of parts.
List<Part> parts = new List<Part>();

// Add parts to the list.
parts.Add(new Part() { PartName = "crank arm", PartId = 1234 });
parts.Add(new Part() { PartName = "chain ring", PartId = 1334 });
parts.Add(new Part() { PartName = "regular seat", PartId = 1434 });
parts.Add(new Part() { PartName = "banana seat", PartId = 1444 });
parts.Add(new Part() { PartName = "cassette", PartId = 1534 });
parts.Add(new Part() { PartName = "shift lever", PartId = 1634 }); ;

// Write out the parts in the list. This will call the overridden ToString method
// in the Part class.
Console.WriteLine();
foreach (Part aPart in parts)
{
    Console.WriteLine(aPart);
}

// Check the list for part #1734. This calls the IEquatable.Equals method
// of the Part class, which checks the PartId for equality.
Console.WriteLine("\nContains: Part with Id=1734: {0}",
    parts.Contains(new Part { PartId = 1234, PartName = "crank arm" }));

// Find items where name contains "seat".
Console.WriteLine("\nFind: Part where name contains \"seat\": {0}",
    parts.Find(x => x.PartName.Contains("seat")));

// Check if an item with Id 1444 exists.
Console.WriteLine("\nExists: Part with Id=1444: {0}",
    parts.Exists(x => x.PartId == 1444));


List<Point3D> exitList = new List<Point3D>();

exitList.Add(new (55,10,5));
exitList.Add(new (10,12,6));
exitList.Add(new (5,1,6));
exitList.Add(new (1,56,133));
exitList.Add(new (3,85,6565));

Console.WriteLine("\nContains: POint: {0}",
    exitList.Contains(new (1,56,133) ));


var source = new List<string>() { "a", "b", "c" };
var compare = new List<string>() { "b", "c", "d" };
var result = source.Intersect(compare);

var old = source.Except(compare);
var newP = compare.Except(source);
Console.WriteLine("Hello, World!");

public class Part : IEquatable<Part>
{
    public string PartName { get; set; }
    public int PartId { get; set; }

    public override string ToString()
    {
        return "ID: " + PartId + "   Name: " + PartName;
    }
    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        Part objAsPart = obj as Part;
        if (objAsPart == null) return false;
        else return Equals(objAsPart);
    }
    public override int GetHashCode()
    {
        return PartId;
    }
    public bool Equals(Part other)
    {
        if (other == null) return false;
        return (this.PartId.Equals(other.PartId));
    }
    // Should also override == and != operators.
}
public class Example
{
    public static void Main()
    {
        // Create a list of parts.
        List<Part> parts = new List<Part>();

        // Add parts to the list.
        parts.Add(new Part() { PartName = "crank arm", PartId = 1234 });
        parts.Add(new Part() { PartName = "chain ring", PartId = 1334 });
        parts.Add(new Part() { PartName = "regular seat", PartId = 1434 });
        parts.Add(new Part() { PartName = "banana seat", PartId = 1444 });
        parts.Add(new Part() { PartName = "cassette", PartId = 1534 });
        parts.Add(new Part() { PartName = "shift lever", PartId = 1634 }); ;

        // Write out the parts in the list. This will call the overridden ToString method
        // in the Part class.
        Console.WriteLine();
        foreach (Part aPart in parts)
        {
            Console.WriteLine(aPart);
        }

        // Check the list for part #1734. This calls the IEquatable.Equals method
        // of the Part class, which checks the PartId for equality.
        Console.WriteLine("\nContains: Part with Id=1734: {0}",
            parts.Contains(new Part { PartId = 1734, PartName = "" }));

        // Find items where name contains "seat".
        Console.WriteLine("\nFind: Part where name contains \"seat\": {0}",
            parts.Find(x => x.PartName.Contains("seat")));

        // Check if an item with Id 1444 exists.
        Console.WriteLine("\nExists: Part with Id=1444: {0}",
            parts.Exists(x => x.PartId == 1444));

        /*This code example produces the following output:

        ID: 1234   Name: crank arm
        ID: 1334   Name: chain ring
        ID: 1434   Name: regular seat
        ID: 1444   Name: banana seat
        ID: 1534   Name: cassette
        ID: 1634   Name: shift lever

        Contains: Part with Id=1734: False

        Find: Part where name contains "seat": ID: 1434   Name: regular seat

        Exists: Part with Id=1444: True
         */
    }


}