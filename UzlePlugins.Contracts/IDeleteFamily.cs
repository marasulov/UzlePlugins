using UzlePlugins.Contracts.DTOs;

namespace UzlePlugins.Contracts;

public interface IDeleteFamily
{
    public void Delete(OutdatedFamilyDto dto);
}