using System;

namespace ShipGame
{
    static class Program
    {
        static void Main(string[] args)
        {
            using (ShipGame game = new ShipGame())
            {
                game.Run();
            }
        }
    }
}

