using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UzlePlugins.Settings;

namespace UzlePlugins.Models.Revit2022
{
    public class Utils
    {

        public static HoleFamilyTypes GetFamiliesName()
        {
            var settingsReader = new SettingsReader();
            return settingsReader.GetFamilyTypes();
        }
    }
}
