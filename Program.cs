using System;

namespace ShaderSample
{
    public static class Program
    {
        static void Main()
        {
			using (var game = new ShaderGame())
				game.Run();
        }
    }
}
