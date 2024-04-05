namespace UzlePlugins.Contracts.DTOs.BaseDtos
{
    /// <summary>
    /// Класс, представляющий данные об отверстии.
    /// </summary>
    public class HoleData
    {
        /// <summary>
        /// Конструктор для инициализации объекта HoleData.
        /// </summary>
        /// <param name="type">Тип отверстия.</param>
        /// <param name="offset">Смещение отверстия.</param>
        /// <param name="isRectangled">Флаг, указывающий, является ли отверстие прямоугольным.</param>
        public HoleData(string type, double offset, bool isRectangled)
        {
            Type = type;
            Offset = offset;
            IsRectangled = isRectangled;
        }

        /// <summary>
        /// Тип отверстия.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Смещение отверстия.
        /// </summary>
        public double Offset { get; set; }

        /// <summary>
        /// Флаг, указывающий, является ли отверстие прямоугольным.
        /// </summary>
        public bool IsRectangled { get; set; }
    }

}
