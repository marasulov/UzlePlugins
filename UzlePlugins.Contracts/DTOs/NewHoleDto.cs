using System.ComponentModel;
using System.Runtime.CompilerServices;
using UzlePlugins.Contracts.DTOs.BaseDtos;

namespace UzlePlugins.Contracts.DTOs
{
    /// <summary>
    /// Новое отверстие.
    /// </summary>
    public class NewHoleDto : HoleDto, INotifyPropertyChanged
    {
        private bool _isInsert;
        /// <summary>
        /// Конструктор для инициализации объекта NewHoleDto.
        /// </summary>
        /// <param name="id">Идентификатор нового отверстия.</param>
        /// <param name="intersection">Данные о пересечении.</param>
        /// <param name="hole">Данные об отверстии.</param>
        /// <param name="holeSource">Данные об источнике.</param>
        /// <param name="shape">Форма нового отверстия.</param>
        /// <param name="height">Высота нового отверстия.</param>
        /// <param name="width">Ширина нового отверстия.</param>
        /// <param name="isInsert">Флаг, указывающий на вставку нового отверстия.</param>
        
        public NewHoleDto(
            int id, 
            IntersectionData intersection, 
            HoleData hole, 
            HoleSourceData holeSource, 
            string shape, 
            double height,
            double width, 
            bool isInsert)
        {
            Id = id;
            Intersection = intersection;
            Hole = hole;
            HoleSource = holeSource;
            Shape = shape;
            Height = height;
            Width = width;
            IsInsert = isInsert;
        }

        /// <summary>
        /// Форма нового отверстия.
        /// </summary>
        public string Shape { get; set; }

        /// <summary>
        /// Высота нового отверстия.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Ширина нового отверстия.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Флаг, указывающий на вставку нового отверстия.
        /// </summary>
        public bool IsInsert
        {
            get => _isInsert;
            set
            {
                _isInsert = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
