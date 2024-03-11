using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using UzlePlugins.Settings;

namespace UzlePlugins.RevitCore.Services
{
    public class FamilyTypeFinder
    {
        private HoleFamilyTypes _familyTypes;

        public FamilyTypeFinder()
        {
            SettingsReader reader = new SettingsReader();
            _familyTypes = reader.GetFamilyTypes();

        }

        public List<string> FamilyParameters { get; set; }

        public string FamilyName { get; set; }
        //public FamilySymbol FamilySymbol { get; set; }

        public string GetFamilyType(BuiltInCategory builtInCategory, bool isHoleCircled)
        {
            FamilyParameters = isHoleCircled ? _familyTypes.FamilyTypes.Circled.FamilyParameters : _familyTypes.FamilyTypes.Rectangled.FamilyParameters;

            var familyName = string.Empty;

            if (builtInCategory == BuiltInCategory.OST_Floors & isHoleCircled)
            {
                familyName = _familyTypes.FamilyTypes.Circled.FamilyNames.FloorType;
            }
            else if (builtInCategory == BuiltInCategory.OST_Floors & !isHoleCircled)
            {
                familyName = _familyTypes.FamilyTypes.Rectangled.FamilyNames.FloorType;
            }
            else if (builtInCategory == BuiltInCategory.OST_Walls & isHoleCircled)
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
