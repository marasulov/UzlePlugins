using System.ComponentModel;
using System.Runtime.CompilerServices;
using UzlePlugins.Contracts.DTOs.BaseDtos;

namespace UzlePlugins.Contracts.DTOs
{
    /// <summary>
    /// Класс, представляющий фактическое отверстие.
    /// </summary>
    public class ActualHoleDto :HoleDto, INotifyPropertyChanged, IHoleModel

    {
        private bool _isDelete;

        /// <summary>
        /// Конструктор для инициализации объекта ActualHoleDto.
        /// </summary>
        /// <param name="id">Идентификатор отверстия.</param>
        /// <param name="intersection">Данные о пересечении.</param>
        /// <param name="hole">Данные об отверстии.</param>
        /// <param name="holeSource">Данные об источнике отверстия.</param>
        /// <param name="isDelete">Флаг, указывающий на удаление отверстия.</param>
        public ActualHoleDto(
            int id, 
            IntersectionData intersection, 
            HoleData hole, 
            HoleSourceData holeSource, 
            bool isDelete)
        {
            Id = id;
            Intersection = intersection;
            Hole = hole;
            HoleSource = holeSource;
            IsDelete = isDelete;
        }

        
        public bool IsDelete
        {
            get => _isDelete;
            set
            {
                _isDelete = value;
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
