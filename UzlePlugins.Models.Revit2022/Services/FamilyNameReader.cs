using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UzlePlugins.Settings;

namespace UzlePlugins.Models.Revit2022.Services
{
    internal class FamilyNameReader
    {
        public FamilyNameReader()
        {
            var settingsReader = new SettingsReader();
            var recFamilyNames = settingsReader.GetFamilyTypes().FamilyTypes.Rectangled.FamilyNames;
            var circledFamilyNames = settingsReader.GetFamilyTypes().FamilyTypes.Circled.FamilyNames;
            
        }
    }
}
