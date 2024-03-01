using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UzlePlugins.Vm;

namespace UzlePlugins.Models
{
    public class OutdatedFamily : IOutdatedFamily
    {
        public int Id { get; }
        public string Name { get; }
        public string Point { get; }

        public OutdatedFamily(int id, string name, string point)
        {
            Id = id;
            Name = name;
            Point = point;
        }


    }
}
