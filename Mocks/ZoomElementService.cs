using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UzlePlugins.Contracts;

namespace Mocks
{
    public class ZoomElementService : IZoomEntity
    {
        public void Zoom(int id)
        {
            Debug.WriteLine(
                $"Zoomed to element {id}");
        }
    }
}
