using Microsoft.CodeCoverage.Core.Reports.Coverage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace VTParseSharp_MSTest
{
    [TestClass]
    public sealed class Test1
    {
        private static readonly string Root = Path.GetFullPath("test_files");
        private const int ProcessTimeoutMs = 30000;

        public TestContext TestContext { get; set; } = null!;

        public static IEnumerable<object[]> Files()
        {
            if (!Directory.Exists(Root))
                yield break;

            foreach (var file in Directory.EnumerateFiles(Root, "*.*", SearchOption.TopDirectoryOnly))
                yield return new object[] { file };
        }

        private void TestExecutable(string executable, string testFilePath, List<string> output)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = "--codes-only",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var process = Process.Start(startInfo);
            Assert.IsNotNull(process, "Failed to start " + executable);

            // Read output asynchronously to avoid deadlocks
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data is not null)
                {
                    TestContext.WriteLine($"[{executable}] {e.Data}");
                    output.Add(e.Data);
                }
            };
            process.BeginOutputReadLine();

            // Pipe file contents to stdin
            var fileBytes = File.ReadAllBytes(testFilePath);
            process.StandardInput.BaseStream.Write(fileBytes, 0, fileBytes.Length);
            process.StandardInput.Close();

            if (!process.WaitForExit(ProcessTimeoutMs))
            {
                process.Kill();
                Assert.Fail($"Process {executable} timed out after {ProcessTimeoutMs}ms");
            }

            // Ensure all async output is flushed
            process.WaitForExit();
            TestContext.WriteLine($"[{executable}] Process exited with code {process.ExitCode}");
        }

        [TestMethod]
        [DynamicData(nameof(Files))]
        public void TestFile(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            Assert.IsGreaterThan(0, bytes.Length, $"Empty file: {filePath}");

            // Get expected text by running vtparse_test.exe on the file.
            // vtparse_test.exe expects input from stdin, and needs the --codes-only flag.
            var expectedOutput = new List<string>();
            TestExecutable("vtparse_test.exe", filePath, expectedOutput);
            var foundOutput = new List<string>();
            TestExecutable("VTParseSharp_Test.exe", filePath, foundOutput);

            // Find first difference
            var maxLines = Math.Max(expectedOutput.Count, foundOutput.Count);
            for (var i = 0; i < maxLines; i++)
            {
                var expected = i < expectedOutput.Count ? expectedOutput[i] : "<missing>";
                var found = i < foundOutput.Count ? foundOutput[i] : "<missing>";
                if (expected != found)
                {
                    Assert.Fail($"Mismatch at line {i + 1} in {filePath}:\nExpected: {expected}\nFound:    {found}");
                }
            }
        }
    }
}
