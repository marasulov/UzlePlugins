using System;
using System.Collections.Generic;
using System.Text;

namespace UzlePlugins.Contracts.DTOs.BaseDtos
{
    /// <summary>
    /// Класс, представляющий данные об отверстии.
    /// </summary>
    public class HoleDto
    {
        /// <summary>
        /// Идентификатор отверстия.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Данные о пересечении.
        /// </summary>
        public IntersectionData Intersection { get; set; }

        /// <summary>
        /// Данные об отверстии.
        /// </summary>
        public HoleData Hole { get; set; }

        /// <summary>
        /// Данные об источнике отверстия.
        /// </summary>
        public HoleSourceData HoleSource { get; set; }
    }
}
