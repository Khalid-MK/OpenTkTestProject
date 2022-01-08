using System;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace OpenTkTestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            using var window = new Game();

            window.Run();
        }
    }
}
