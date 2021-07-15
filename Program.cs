using System;

namespace ShaderTest
{
    public static class Program
    {
        static void Main()
        {
			using (var game = new ShaderTestGame())
				game.Run();
        }
    }
}
