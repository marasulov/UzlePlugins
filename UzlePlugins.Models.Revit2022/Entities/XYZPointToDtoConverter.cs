using Autodesk.Revit.DB;
using UzlePlugins.Contracts.DTOs;

namespace UzlePlugins.Models.Revit2022.Entities
{
    public class XYZPointToDtoConverter
    {
        /// <summary>
        /// Метод для конвертации из PointDTO в XYZ
        /// </summary>
        /// <param name="pointDto"></param>
        /// <returns></returns>
        public static XYZ ConvertToPointXYZ(PointDTO pointDto)
        {
            return new XYZ ( pointDto.X, pointDto.Y, pointDto.Z );
        }

        /// <summary>
        /// Метод для конвертации из XYZ в PointDTO
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public static PointDTO ConvertToDTO(XYZ xyz)
        {
            return new PointDTO(xyz.X, xyz.Y, xyz.Z);
        }
    }
}
