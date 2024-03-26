using System.Diagnostics;
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
