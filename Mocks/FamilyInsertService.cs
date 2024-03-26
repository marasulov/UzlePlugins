using System.Diagnostics;
using System.Linq;
using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;

namespace Mocks
{
    public class FamilyInsertService : IFamilyInsertService
    {
        public void InsertFamily(AllHolesDto allHoles)
        {
            var newHoles = allHoles.NewFamiliesDtos.Where(i => i.IsInsert);
            var actualHoles = allHoles.ActualFamiliesDtos.Where(i => i.IsDelete).ToArray();
            var outdatedHoles = allHoles.OutdatedFamiliesDtos.Where(i => !i.IsDelete);


            if (newHoles.Count() > 0)
            {
                foreach (var newHolesDto in newHoles)
                {
                    Debug.WriteLine($"inserted {newHolesDto.Id}");
                }
            }

        }
    }
}
