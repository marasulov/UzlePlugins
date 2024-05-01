using System.Collections.Generic;

namespace UzlePlugins.Contracts.DTOs
{
    public class AllHolesDto
    {
        public AllHolesDto(
            List<NewHoleDto> newFamiliesDtos, 
            List<ActualHoleDto> actualFamiliesDtos, 
            List<OutdatedFamilyDto> outdatedFamiliesDtos)
        {
            NewFamiliesDtos = newFamiliesDtos;
            ActualFamiliesDtos = actualFamiliesDtos;
            OutdatedFamiliesDtos = outdatedFamiliesDtos;
        }

        public List<NewHoleDto> NewFamiliesDtos { get; }
        public List<ActualHoleDto> ActualFamiliesDtos { get; }
        public List<OutdatedFamilyDto> OutdatedFamiliesDtos { get; }
    }
}
