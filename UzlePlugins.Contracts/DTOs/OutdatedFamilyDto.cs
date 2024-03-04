using UzlePlugins.Vm;

namespace UzlePlugins.Contracts.DTOs
{
    public class OutdatedFamilyDto : IOutdatedFamily
    {
        public int Id { get; }
        public string Name { get; }


        public OutdatedFamilyDto(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
