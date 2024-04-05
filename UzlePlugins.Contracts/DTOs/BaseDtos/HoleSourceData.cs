using System;
using System.Collections.Generic;
using System.Text;

namespace UzlePlugins.Contracts.DTOs
{
    /// <summary>
    /// Класс, представляющий данные об источнике отверстия.
    /// </summary>
    public class HoleSourceData
    {
        /// <summary>
        /// Конструктор для инициализации объекта HoleSourceData.
        /// </summary>
        /// <param name="sourceType">Тип источника отверстия.</param>
        /// <param name="sourceThickness">Толщина источника отверстия.</param>
        /// <param name="sourceWidth">Ширина источника отверстия.</param>
        /// <param name="sourceName">Имя источника отверстия.</param>
        public HoleSourceData(string sourceType, double sourceThickness, double sourceWidth, string sourceName)
        {
            SourceType = sourceType;
            SourceThickness = sourceThickness;
            SourceWidth = sourceWidth;
            SourceName = sourceName;
        }

        /// <summary>
        /// Тип источника отверстия.
        /// </summary>
        public string SourceType { get; set; }

        /// <summary>
        /// Толщина источника отверстия.
        /// </summary>
        public double SourceThickness { get; set; }

        /// <summary>
        /// Ширина источника отверстия.
        /// </summary>
        public double SourceWidth { get; set; }

        /// <summary>
        /// Имя источника отверстия.
        /// </summary>
        public string SourceName { get; set; }
    }

}
