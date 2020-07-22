using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmgucvDemo
{
    public class Luknja
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public float Area { get; set; }

        public Luknja(int width, int height, float area)
        {
            Width = width;
            Height = height;
            Area = area;
        }
    }
}
