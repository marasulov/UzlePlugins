using System;
using System.Collections.Generic;
using System.Text;

namespace UzlePlugins.Contracts
{
    public interface ISettingsReader
    {
        List<string> GetFamilyNames();
    }
}
