using System;
using System.Collections.Generic;
using System.Text;
using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;
using UzlePlugins.Vm.Commands;

namespace Mocks
{
    public class FindHolesService : IFindHoleService
    {
        public AllHolesDto FindHoles()
        {
            var newHolesDtos = new List<NewHolesDto>();
            var actualHoleModelDtos = new List<ActualHoleModelDto>();
            var outdatedFamilyDtos = new List<OutdatedFamilyDto>();

            for (int i = 0; i < 50; i++)
            {
                var newHoleDto = new NewHolesDto(i, new PointDTO(1, 2, 3), "Elementtype" + i, "AAA" + i, "324" + i, "holetype" + i, 3242 + i, "asdsad", "Circle", 20, true, 20, new PointDTO(i + 1, i + 2, i + 3), 50, "aaa",i,i);
                newHolesDtos.Add(newHoleDto);
            }

            for (int i = 0; i < 50; i++)
            {
                var actualHoles = new ActualHoleModelDto(i, new PointDTO(1, 2, 3), "AAA" + i, "324" + i, "32432" + i, 3242 + i, "asdsad", true, 20, true, 20, new PointDTO(i + 1, i + 2, i + 3), 50, "aaa");
                actualHoleModelDtos.Add(actualHoles);
            }

            for (int i = 0; i < 50; i++)
            {
                var outdatedFamilyDto = new OutdatedFamilyDto(i.ToString(), "AAA" + i, new PointDTO(1, 2, 3));
                outdatedFamilyDtos.Add(outdatedFamilyDto);
            }

            return new AllHolesDto(newHolesDtos, actualHoleModelDtos, outdatedFamilyDtos);
        }
    }
}
