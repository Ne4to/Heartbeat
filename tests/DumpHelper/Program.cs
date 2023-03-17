using System.Diagnostics;

namespace DumpHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            Program program = new();
            program.Run();
        }

        private void Run()
        {
            var testsDir = GetRootTestsPath();
            var dumpsDir = Directory.CreateDirectory(Path.Combine(testsDir.FullName, "dumps"));

            foreach (var childDir in testsDir.EnumerateDirectories("*.IntegrationTest", SearchOption.TopDirectoryOnly))
            {
                foreach (var projectFile in childDir.EnumerateFiles("*.csproj", SearchOption.TopDirectoryOnly))
                {
                    BuildProject(projectFile);
                    var testProcess = RunProject(projectFile);
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    MakeDump(testProcess, dumpsDir);
                }

                Console.WriteLine(childDir);
            }
        }

        private void BuildProject(FileInfo projectFile)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "dotnet";
            //startInfo.ArgumentList.Add("--info");
            startInfo.ArgumentList.Add("build");
            startInfo.ArgumentList.Add("-c");
            startInfo.ArgumentList.Add("Release");
            startInfo.ArgumentList.Add(projectFile.FullName);

            var process = Process.Start(startInfo);
            process.WaitForExit();
        }

        private Process RunProject(FileInfo projectFile)
        {
            var dir = Path.Combine(projectFile.DirectoryName, @"bin\Release\net7.0");
            var exeFile = Path.ChangeExtension(projectFile.Name, "exe");

            ProcessStartInfo startInfo = new()
            {
                FileName = Path.Combine(dir, exeFile)
            };

            var process = Process.Start(startInfo);
            return process;
        }

        private void MakeDump(Process testProcess, DirectoryInfo dumpsDir)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"procdump.exe";
            startInfo.ArgumentList.Add("-ma");
            startInfo.ArgumentList.Add(testProcess.Id.ToString());
            startInfo.ArgumentList.Add(Path.Combine(dumpsDir.FullName, "AsyncStask.dmp"));

            var process = Process.Start(startInfo);
            process.WaitForExit();
        }

        private DirectoryInfo GetRootTestsPath()
        {
            var path = Directory.GetCurrentDirectory();
            var currentDir = new DirectoryInfo(path);

            while (currentDir.Name != "tests")
            {
                currentDir = currentDir.Parent;
            }

            return currentDir;
        }
    }
}
