using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UzlePlugins.Models.Revit2022.Entities
{
    /// <summary>
    /// Параметры источника
    /// </summary>
    public class SourceParameters
    {
        /// <summary>
        /// Имя источника
        /// </summary>
        public string SourceName { get; }
    
        /// <summary>
        /// Тип источника
        /// </summary>
        public string SourceType { get; }

        /// <summary>
        /// Толщина источника
        /// </summary>
        public double SourceThickness { get; }

        public SourceParameters(string sourceName, string sourceType, double sourceThickness)
        {
            SourceName = sourceName;
            SourceType = sourceType;
            SourceThickness = sourceThickness;
        }
    }

}
