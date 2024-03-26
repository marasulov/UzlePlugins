using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using UzlePlugins.Settings;

namespace UzlePlugins.RevitCore.Services
{
    public class FamilyTypeFinderService
    {
        private HoleFamilyTypes _familyTypes;

        public FamilyTypeFinderService()
        {
            SettingsReader reader = new SettingsReader();
            _familyTypes = reader.GetFamilyTypes();
        }

        public List<string> FamilyParameters { get; set; }

        public string FamilyName { get; set; }
        //public FamilySymbol FamilySymbol { get; set; }

        public string GetFamilyType(BuiltInCategory builtInCategory, string shape)
        {
            FamilyParameters = shape != "Circle" ? _familyTypes.FamilyTypes.Circled.FamilyParameters : _familyTypes.FamilyTypes.Rectangled.FamilyParameters;

            string familyName;

            if (builtInCategory == BuiltInCategory.OST_Floors & shape == "Circle")
            {
                familyName = _familyTypes.FamilyTypes.Circled.FamilyNames.FloorType;
            }
            else if (builtInCategory == BuiltInCategory.OST_Floors & shape == "Square")
            {
                familyName = _familyTypes.FamilyTypes.Rectangled.FamilyNames.FloorType;
            }
            else if (builtInCategory == BuiltInCategory.OST_Walls & shape == "Circle")
            {
                familyName = _familyTypes.FamilyTypes.Circled.FamilyNames.WallType;
            }
            else
            {
                familyName = _familyTypes.FamilyTypes.Rectangled.FamilyNames.WallType;
            }

            return familyName;
        }

        public FamilySymbol GetFamilySymbolToPlace(Document doc, string familyName/*, string familyTypeName*/)
        {
            FamilySymbol symbol = null;
            foreach (FamilySymbol fSym in new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
                         .Cast<FamilySymbol>())
            {
                if (fSym.FamilyName != familyName) continue;
                symbol = fSym;
                break;
            }
            return symbol;
        }
    }


}
