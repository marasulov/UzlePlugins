using Autodesk.Revit.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Electrical;
using System.Windows.Controls;

namespace UzlePlugins.Models.Revit2022.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class TestCommand : IExternalCommand
    {
        private static CableTray _tempCableTray;
        private static Element _defaultCableTrayType;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData
                .Application;
            var uiDocument = commandData
                .Application
                .ActiveUIDocument;

            var app = commandData
                .Application
                .Application;
            
            UIDocument activeUiDocument = uiDocument;
            Document document = activeUiDocument.Document;
            try
            {
                using (Transaction transaction = new Transaction(document, "TransactionNames.CreateCableTray"))
                {
                    transaction.Start();
                    CreateAndSelectTempCableTray(uiApp);
                    transaction.Commit();
                }
                RevitCommandId revitCommandId = RevitCommandId.LookupPostableCommandId((PostableCommand) 33441);
                activeUiDocument.Application.PostCommand(revitCommandId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ошибка");
            }
            finally
            {
                _tempCableTray = (CableTray) null;
                _defaultCableTrayType = (Element) null;
            }


            return Result.Succeeded;
        }

        private static void CreateAndSelectTempCableTray(UIApplication uiApp)
        {
            UIDocument activeUiDocument = uiApp.ActiveUIDocument;
            Document document = activeUiDocument.Document;
            _tempCableTray = CreateTempCableTray(activeUiDocument);
            if (_tempCableTray == null)
                return;
            ElementId defaultElementTypeId = document.GetDefaultElementTypeId((ElementTypeGroup)122);
            _defaultCableTrayType = document.GetElement(defaultElementTypeId);
            SetSizesCableTray();
            AddIdTemp(document);
            activeUiDocument.Selection.SetElementIds((ICollection<ElementId>)new List<ElementId>()
            {
                ((Element) _tempCableTray).Id
            });
        }

        private static void AddIdTemp(Document doc)
        {
            Parameter parameter = ((Element)doc.ProjectInformation)[IdTemp];
            int integerValue = ((Element)_tempCableTray).Id.IntegerValue;
            if (parameter.HasValue)
            {
                List<int> list = ((IEnumerable<string>)parameter.AsString().Split(';')).Select<string, int>((Func<string, int>)(n => StringExtension.FirstInt(n))).Where<int>((Func<int, bool>)(n => n > 0)).ToList<int>();
                list.Add(integerValue);
                string str = string.Join<int>(";", (IEnumerable<int>)list);
                parameter.Set(str);
            }
            else
                parameter.Set(integerValue.ToString());
        }

        private static CableTray CreateTempCableTray(UIDocument uiDoc)
        {
            Document document = uiDoc.Document;
            ElementId levelId = GetLevelId(uiDoc);
            if (levelId == ElementId.InvalidElementId)
                return (CableTray)null;
            Level element = (Level)document.GetElement(levelId);
            ElementId defaultElementTypeId = document.GetDefaultElementTypeId((ElementTypeGroup)122);
            double num = GetOffset(uiDoc.Application) + element.Elevation;
            XYZ xyz1 = new XYZ(0.0, 0.0, num);
            XYZ xyz2 = new XYZ(0.01, 0.0, num);
            return CableTray.Create(document, defaultElementTypeId, xyz1, xyz2, levelId);
        }

        private static double GetOffset(UIApplication uiApp)
        {
            RibbonPanel ribbonPanel = uiApp.GetRibbonPanels(Resources.RibbonName).FirstOrDefault<RibbonPanel>((Func<RibbonPanel, bool>)(n => n.Name == RibbonPanels.CreateCableTrays));
            TextBox textBox = ribbonPanel != null ? (TextBox)ribbonPanel.GetItems().FirstOrDefault<RibbonItem>((Func<RibbonItem, bool>)(n => n.Name == Resources.TextBoxOffset)) : (TextBox)null;
            return UnitConverter.Convert(textBox != null ? StringExtension.FirstDouble(textBox.Value.ToString(), ',') : 0.0, (ConvertMode)3);
        }

        private static ElementId GetLevelId(UIDocument uiDoc)
        {
            Document document = uiDoc.Document;
            ElementId levelId = ElementId.InvalidElementId;
            View activeView = uiDoc.ActiveView;
            if (activeView.GenLevel != null)
            {
                levelId = ((Element)activeView.GenLevel).Id;
            }
            else
            {
                List<Level> list = ((IEnumerable)new FilteredElementCollector(document).OfClass(typeof(Level)).WhereElementIsNotElementType()).OfType<Level>().ToList<Level>();
                if (list.Any<Level>())
                {
                    if (activeView.SketchPlane != null)
                    {
                        string nameSketchPlane = ((Element)activeView.SketchPlane).Name;
                        Level level = list.FirstOrDefault<Level>((Func<Level, bool>)(n => ((Element)n).Name == nameSketchPlane));
                        if (level != null)
                            levelId = ((Element)level).Id;
                    }
                    else
                        levelId = ((Element)list.First<Level>()).Id;
                }
            }
            return levelId;
        }

        private static void SetSizesCableTray()
        {
            if (_tempCableTray == null || _defaultCableTrayType == null)
                return;
            Parameter parameter1 = _defaultCableTrayType[Guids.CableTrayTypeWidth];
            Parameter parameter2 = _defaultCableTrayType[Guids.CableTrayTypeHeight];
            double num1 = parameter1 != null ? parameter1.AsDouble() : MathExtension.MmToFeet(200.0);
            double num2 = parameter2 != null ? parameter2.AsDouble() : MathExtension.MmToFeet(50.0);
            if (num1 <= 0.0 || num2 <= 0.0)
                return;
            ((Element)_tempCableTray)[(BuiltInParameter) - 1140122].Set(num1);
            ((Element)_tempCableTray)[(BuiltInParameter) - 1140121].Set(num2);
        }
    }
}
