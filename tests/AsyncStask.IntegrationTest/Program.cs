namespace AsyncStask.IntegrationTest
{
    class Program
    {
        private static readonly Random _random = new();

        static async Task Main(string[] args)
        {
            await MethodA();
        }

        private static async Task MethodA()
        {
            var a = _random.Next();
            await MethodB();
            Console.WriteLine(a);
        }

        private static async Task MethodB()
        {
            var b = _random.Next();
            await MethodC();
            Console.WriteLine(b);
        }

        private static async Task MethodC()
        {
            var c = _random.Next();
            await MethodD();
            Console.WriteLine(c);
        }

        private static async Task MethodD()
        {
            var d = _random.Next();
            await Task.Delay(TimeSpan.FromSeconds(5D));
            Console.WriteLine(d);
        }
    }
}
