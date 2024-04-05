using System.Collections.Generic;

namespace UzlePlugins.Contracts
{
    public interface ISettingsReader<T>
    {
        T Read(string filename);
    }
}
