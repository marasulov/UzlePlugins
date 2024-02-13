using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UzlePlugins.RevitCore.Services
{
    public class HoleFamilyCreator
    {
        public HoleFamilyCreator(Document doc, Reference reference, FamilySymbol symbol)
        {
            //ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, reference);

            //List<XYZ> intersectPoints = refFinder.GetIntersectionsPoint();

            //if (intersectPoints.Count > 0)
            //{
            //    foreach (XYZ intersectPoint in intersectPoints)
            //    {
            //        FamilyInstance fi = doc.Create.NewFamilyInstance(intersectPoint, symbol, StructuralType.NonStructural);
            //        var basisY = fi.GetTransform().BasisY;
            //        var angle = basisY.AngleTo(refFinder.Normal);

            //        Line axis = Line.CreateBound(intersectPoint, intersectPoint + XYZ.BasisZ);
                    
            //        ElementTransformUtils.RotateElement(doc, fi.Id, axis, -angle);
            //    }
                
            //}

            
        }
    }
}
