using System.Collections.Generic;

namespace UzlePlugins.Contracts
{
    public interface ISettingsReader
    {
        List<string> GetFamilyNames();
    }
}
