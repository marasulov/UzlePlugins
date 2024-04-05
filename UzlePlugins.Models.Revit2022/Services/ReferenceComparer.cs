using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.DB;

namespace UzlePlugins.Models.Revit2022.Services
{
    public class ReferenceComparer : IEqualityComparer<Reference>
    {
        public bool Equals(Reference x, Reference y)
        {
            Debug.Print($"{x.GlobalPoint} - {y.GlobalPoint}\n");
            if (x.ElementId != y.ElementId) return false;
            return x.LinkedElementId == y.LinkedElementId;
        }

        public int GetHashCode(Reference obj)
        {
            int hashName = obj.ElementId.GetHashCode();
            int hashId = obj.LinkedElementId.GetHashCode();
            return hashId ^ hashId;
        }
    }
}
