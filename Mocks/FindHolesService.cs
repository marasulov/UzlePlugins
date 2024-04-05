using System.Collections.Generic;
using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;
using UzlePlugins.Contracts.DTOs.BaseDtos;

namespace Mocks
{
    public class FindHolesService : IFindHoleService
    {
        public FindHolesService()
        {
        }

        public AllHolesDto FindHoles()
        {
            var newHolesDtos = new List<NewHoleDto>();
            var actualHoleModelDtos = new List<ActualHoleDto>();
            var outdatedFamilyDtos = new List<OutdatedFamilyDto>();

            for (int i = 0; i < 50; i++)
            {
                var newIntData = new IntersectionData(new PointDTO(1, 2, 3),"ElementName" + i, "ElementType" + i, i, new PointDTO(1, 2, 3), "shape"+i);
                var holeData = new HoleData("type"+i, i+50, true);
                var soursaData = new HoleSourceData("sourceType" + i, i + 20, 30 + i, "sourceName" + i);
                var newHoleDto = new NewHoleDto(i, newIntData, holeData, soursaData,"shape"+i, i+50, 50+i, true );
                newHolesDtos.Add(newHoleDto);
            }

            //for (int i = 0; i < 50; i++)
            //{
            //    var actualHoles = new ActualHoleDto(i, new PointDTO(1, 2, 3), "AAA" + i, "324" + i, "32432" + i, 3242 + i, "asdsad", true, 20, true, 20, new PointDTO(i + 1, i + 2, i + 3), 50, "aaa");
            //    actualHoleModelDtos.Add(actualHoles);
            //}

            for (int i = 0; i < 50; i++)
            {
                var outdatedFamilyDto = new OutdatedFamilyDto(i.ToString(), "AAA" + i, new PointDTO(1, 2, 3));
                outdatedFamilyDtos.Add(outdatedFamilyDto);
            }

            return new AllHolesDto(newHolesDtos, actualHoleModelDtos, outdatedFamilyDtos);
        }
    }
}
