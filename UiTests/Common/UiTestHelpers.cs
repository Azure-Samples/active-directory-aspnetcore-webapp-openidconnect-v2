// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Playwright;
using System.Diagnostics;
using System.Management;
using System.Runtime.Versioning;
using System.Text;
using Xunit.Abstractions;

namespace Common
{
    public static class UiTestHelpers
    {
        /// <summary>
        /// Navigates to a web page with retry logic to ensure establish a connection in case a web app needs more startup time.
        /// </summary>
        /// <param name="uri">The uri to navigate to</param>
        /// <param name="page">A page in a playwright browser</param>
        /// <returns></returns>
        public static async Task NavigateToWebApp(string uri, IPage page)
        {
            uint InitialConnectionRetryCount = 5;
            while (InitialConnectionRetryCount > 0)
            {
                try
                {
                    await page.GotoAsync(uri);
                    break;
                }
                catch (PlaywrightException)
                {
                    await Task.Delay(1000);
                    InitialConnectionRetryCount--;
                    if (InitialConnectionRetryCount == 0)
                    { throw; }
                }
            }
        }

        /// <summary>
        /// Login flow for the first time in a given browsing session.
        /// </summary>
        /// <param name="page">Playwright Page object the web app is accessed from</param>
        /// <param name="email">email of the user to sign in</param>
        /// <param name="password">password for sign in</param>
        /// <param name="output">Used to communicate output to the test's Standard Output</param>
        /// <param name="staySignedIn">Whether to select "stay signed in" on login</param>
        public static async Task FirstLogin_MicrosoftIdFlow_ValidEmailPassword(IPage page, string email, string password, ITestOutputHelper? output = null, bool staySignedIn = false)
        {
            string staySignedInText = staySignedIn ? "Yes" : "No";
            await EnterEmailAsync(page, email, output);
            await EnterPasswordAsync(page, password, output);
            await StaySignedIn_MicrosoftIdFlow(page, staySignedInText, output);
        }

        /// <summary>
        /// Login flow for anytime after the first time in a given browsing session.
        /// </summary>
        /// <param name="page">Playwright Page object the web app is accessed from</param>
        /// <param name="email">email of the user to sign in</param>
        /// <param name="password">password for sign in</param>
        /// <param name="output">Used to communicate output to the test's Standard Output</param>
        /// <param name="staySignedIn">Whether to select "stay signed in" on login</param>
        public static async Task SuccessiveLogin_MicrosoftIdFlow_ValidEmailPassword(IPage page, string email, string password, ITestOutputHelper? output = null, bool staySignedIn = false)
        {
            string staySignedInText = staySignedIn ? "Yes" : "No";

            WriteLine(output, $"Logging in again in this browsing session... selecting user via email: {email}.");
            await SelectKnownAccountByEmail_MicrosoftIdFlow(page, email);
            await EnterPasswordAsync(page, password, output);
            await StaySignedIn_MicrosoftIdFlow(page, staySignedInText, output);
        }

        public static async Task EnterEmailAsync(IPage page, string email, ITestOutputHelper? output = null)
        {
            WriteLine(output, $"Logging in ... Entering and submitting user name: {email}.");
            ILocator emailInputLocator = page.GetByPlaceholder(TestConstants.EmailText);
            await FillEntryBox(emailInputLocator, email);
        }

        /// <summary>
        /// Signs the current user out of the web app.
        /// </summary>
        /// <param name="page">Playwright Page object the web app is accessed from</param>
        /// <param name="email">email of the user to sign out</param>
        /// <param name="signOutPageUrl">The url for the page arrived at once successfully signed out</param>
        public static async Task PerformSignOut_MicrosoftIdFlow(IPage page, string email, string signOutPageUrl, ITestOutputHelper? output = null)
        {
            WriteLine(output, "Signing out ...");
            await SelectKnownAccountByEmail_MicrosoftIdFlow(page, email.ToLowerInvariant());
            await page.WaitForURLAsync(signOutPageUrl);
            WriteLine(output, "Sign out page successfully reached.");
        }

        /// <summary>
        /// In the Microsoft Identity flow, the user is at certain stages presented with a list of accounts known in 
        /// the current browsing session to choose from. This method selects the account using the user's email.
        /// </summary>
        /// <param name="page">page for the playwright browser</param>
        /// <param name="email">user email address to select</param>
        private static async Task SelectKnownAccountByEmail_MicrosoftIdFlow(IPage page, string email)
        {
            await page.Locator($"[data-test-id=\"{email}\"]").ClickAsync();
        }

        /// <summary>
        /// The set of steps to take when given a password to enter and submit when logging in via Microsoft.
        /// </summary>
        /// <param name="page">The browser page instance.</param>
        /// <param name="password">The password for the account you're logging into.</param>
        /// <param name="staySignedInText">"Yes" or "No" to stay signed in for the given browsing session.</param>
        /// <param name="output">The writer for output to the test's console.</param>
        public static async Task EnterPasswordAsync(IPage page, string password, ITestOutputHelper? output = null)
        {
            // If using an account that has other non-password validation options, the below code should be uncommented
            /* WriteLine(output, "Selecting \"Password\" as authentication method"); 
            await page.GetByRole(AriaRole.Button, new() { Name = TestConstants.PasswordText }).ClickAsync();*/

            WriteLine(output, "Logging in ... entering and submitting password.");
            ILocator passwordInputLocator = page.GetByPlaceholder(TestConstants.PasswordText);
            await FillEntryBox(passwordInputLocator, password);
        }

        public static async Task StaySignedIn_MicrosoftIdFlow(IPage page, string staySignedInText, ITestOutputHelper? output = null)
        {
            WriteLine(output, $"Logging in ... Clicking {staySignedInText} on whether the browser should stay signed in.");
            await page.GetByRole(AriaRole.Button, new() { Name = staySignedInText }).ClickAsync();
        }

        public static async Task FillEntryBox(ILocator entryBox, string entryText)
        {
            await entryBox.ClickAsync();
            await entryBox.FillAsync(entryText);
            await entryBox.PressAsync("Enter");
        }
        private static void WriteLine(ITestOutputHelper? output, string message)
        {
            if (output != null)
            {
                output.WriteLine(message);
            }
            else
            {
                Trace.WriteLine(message);
            }
        }

        /// <summary>
        /// This starts the recording of playwright trace files. The corresponsing EndAndWritePlaywrightTrace method will also need to be used.
        /// This is not used anywhere by default and will need to be added to the code if desired.
        /// </summary>
        /// <param name="page">The page object whose context the trace will record.</param>
        public static async Task StartPlaywrightTrace(IPage page)
        {
            await page.Context.Tracing.StartAsync(new()
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });
        }

        /// <summary>
        /// Starts a process from an executable, sets its working directory, and redirects its output to the test's output.
        /// </summary>
        /// <param name="testAssemblyLocation">The path to the test's directory.</param>
        /// <param name="appLocation">The path to the processes directory.</param>
        /// <param name="executableName">The name of the executable that launches the process.</param>
        /// <param name="portNumber">The port for the process to listen on.</param>
        /// <param name="isHttp">If the launch URL is http or https. Default is https.</param>
        /// <returns>The started process.</returns>
        public static Process StartProcessLocally(string testAssemblyLocation, string appLocation, string executableName, Dictionary<string, string>? environmentVariables = null)
        {
            string applicationWorkingDirectory = GetApplicationWorkingDirectory(testAssemblyLocation, appLocation);
            ProcessStartInfo processStartInfo = new ProcessStartInfo(applicationWorkingDirectory + executableName)
            {
                WorkingDirectory = applicationWorkingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            if (environmentVariables != null)
            {
                foreach (var kvp in environmentVariables)
                {
                    processStartInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
                }
            }

            Process? process = Process.Start(processStartInfo);

            if (process == null)
            {
                throw new Exception($"Could not start process {executableName}");
            }
            else
            {
                return process;
            }
        }

        /// <summary>
        /// Builds the path to the process's directory
        /// </summary>
        /// <param name="testAssemblyLocation">The path to the test's directory</param>
        /// <param name="appLocation">The path to the processes directory</param>
        /// <returns>The path to the directory for the given app</returns>
        private static string GetApplicationWorkingDirectory(string testAssemblyLocation, string appLocation)
        {
            string testedAppLocation = Path.GetDirectoryName(testAssemblyLocation)!;
            // e.g. microsoft-identity-web\tests\E2E Tests\WebAppUiTests\bin\Debug\net6.0
            string[] segments = testedAppLocation.Split(Path.DirectorySeparatorChar);
            int numberSegments = segments.Length;
            int startLastSegments = numberSegments - 3;
            int endFirstSegments = startLastSegments - 2;
            return Path.Combine(
                Path.Combine(segments.Take(endFirstSegments).ToArray()),
                appLocation,
                Path.Combine(segments.Skip(startLastSegments).ToArray())
            );
        }

        /// <summary>
        /// Builds the path to the process's directory
        /// </summary>
        /// <param name="testAssemblyLocation">The path to the test's directory</param>
        /// <param name="appLocation">The path to the processes directory</param>
        /// <returns>The path to the directory for the given app</returns>
        private static string GetAppsettingsDirectory(string testAssemblyLocation, string appLocation)
        {
            string testedAppLocation = Path.GetDirectoryName(testAssemblyLocation)!;
            // e.g. microsoft-identity-web\tests\E2E Tests\WebAppUiTests\bin\Debug\net6.0
            string[] segments = testedAppLocation.Split(Path.DirectorySeparatorChar);
            int numberSegments = segments.Length;
            int startLastSegments = numberSegments - 3;
            int endFirstSegments = startLastSegments - 2;
            return Path.Combine(
                Path.Combine(segments.Take(endFirstSegments).ToArray()),
                appLocation
            );
        }

        /// <summary>
        /// Creates absolute path for Playwright trace file
        /// </summary>
        /// <param name="testAssemblyLocation">The path the test is being run from</param>
        /// <param name="traceName">The name for the zip file containing the trace</param>
        /// <returns>An absolute path to a Playwright Trace zip folder</returns>
        public static string GetTracePath(string testAssemblyLocation, string traceName)
        {
            const string traceParentFolder = "E2E Tests";
            const string traceFolder = "PlaywrightTraces";
            const string zipExtension = ".zip";
            const int netVersionNumberLength = 3;

            int parentFolderIndex = testAssemblyLocation.IndexOf(traceParentFolder, StringComparison.InvariantCulture);
            string substring = testAssemblyLocation[..(parentFolderIndex + traceParentFolder.Length)];
            string netVersion = "_net" + Environment.Version.ToString()[..netVersionNumberLength];

            // e.g. [absolute path to repo root]\tests\E2E Tests\PlaywrightTraces\[traceName]_net[versionNum].zip
            return Path.Combine(
                substring,
                traceFolder,
                traceName + netVersion + zipExtension
            );
        }

        public static void EndProcesses(Dictionary<string, Process>? processes)
        {
            Queue<Process> processQueue = new();
            if (processes != null)
            {
                foreach (var process in processes)
                {
                    processQueue.Enqueue(process.Value);
                }
            }
            KillProcessTrees(processQueue);
        }

        /// <summary>
        /// Kills the processes in the queue and all of their children
        /// </summary>
        /// <param name="processQueue">queue of parent processes</param>
        public static void KillProcessTrees(Queue<Process> processQueue)
        {
#if WINDOWS
            Process currentProcess;
            while (processQueue.Count > 0)
            {
                currentProcess = processQueue.Dequeue();
                if (currentProcess == null)
                    continue;

                foreach (Process child in GetChildProcesses(currentProcess))
                {
                    processQueue.Enqueue(child);
                }
                currentProcess.Kill();
                currentProcess.Close();
            }
#else
            while (processQueue.Count > 0)
            {
                Process p = processQueue.Dequeue();
                p.Kill();
                p.WaitForExit();
            }
#endif
        }

        /// <summary>
        /// Gets the child processes of a process on Windows
        /// </summary>
        /// <param name="process">The parent process</param>
        /// <returns>A list of child processes</returns>
        [SupportedOSPlatform("windows")]
        public static IList<Process> GetChildProcesses(this Process process)
        {
            ManagementObjectSearcher processSearch = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={process.Id}");
            IList<Process> processList = processSearch.Get()
                .Cast<ManagementObject>()
                .Select(mo =>
                    Process.GetProcessById(Convert.ToInt32(mo["ProcessID"], System.Globalization.CultureInfo.InvariantCulture)))
                .ToList();
            processSearch.Dispose();
            return processList;
        }

        /// <summary>
        /// Checks if all processes in a list are alive
        /// </summary>
        /// <param name="processes">List of processes to check</param>
        /// <returns>True if all are alive else false</returns>
        public static bool ProcessesAreAlive(List<Process> processes)
        {
            return processes.All(ProcessIsAlive);
        }

        /// <summary>
        /// Checks if a process is alive
        /// </summary>
        /// <param name="process">Process to check</param>
        /// <returns>True if alive false if not</returns>
        public static bool ProcessIsAlive(Process process)
        {
            return !process.HasExited;
        }

        /// <summary>
        /// Installs the chromium browser for Playwright enabling it to run even if no browser otherwise exists in the test environment
        /// </summary>
        /// <exception cref="Exception">Thrown if playwright is unable to install the browsers</exception>
        public static void InstallPlaywrightBrowser()
        {
            var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
            if (exitCode != 0)
            {
                throw new Exception($"Playwright exited with code {exitCode}");
            }
        }

        /// <summary>
        /// Requests a secret from keyvault using the default azure credentials
        /// </summary>
        /// <param name="keyvaultUri">The URI including path to the secret directory in keyvault</param>
        /// <param name="keyvaultSecretName">The name of the secret</param>
        /// <returns>The value of the secret from key vault</returns>
        /// <exception cref="ArgumentNullException">Throws if no secret name is provided</exception>
        internal static async Task<string> GetValueFromKeyvaultWitDefaultCreds(Uri keyvaultUri, string keyvaultSecretName, TokenCredential creds)
        {
            if (string.IsNullOrEmpty(keyvaultSecretName))
            {
                throw new ArgumentNullException(nameof(keyvaultSecretName));
            }
            SecretClient client = new(keyvaultUri, creds);
            return (await client.GetSecretAsync(keyvaultSecretName)).Value.Value;
        }

        public static bool StartAndVerifyProcessesAreRunning(List<ProcessStartOptions> processDataEntries, out Dictionary<string, Process> processes, uint numRetries)
        {
            processes = new Dictionary<string, Process>();

            //Start Processes
            foreach (ProcessStartOptions processDataEntry in processDataEntries)
            {
                var process = UiTestHelpers.StartProcessLocally(
                                                processDataEntry.TestAssemblyLocation,
                                                processDataEntry.AppLocation,
                                                processDataEntry.ExecutableName,
                                                processDataEntry.EnvironmentVariables);

                processes.Add(processDataEntry.ExecutableName, process);

                // Gives the current process time to start up before the next process is run
                Thread.Sleep(2000);
            }

            //Verify that processes are running
            for (int i = 0; i < numRetries; i++)
            {
                if (!UiTestHelpers.ProcessesAreAlive(processes.Values.ToList())) { RestartProcesses(processes, processDataEntries); }
                else { break; }
            }

            if (!UiTestHelpers.ProcessesAreAlive(processes.Values.ToList()))
            {
                return false;
            }

            return true;
        }

        private static void RestartProcesses(Dictionary<string, Process> processes, List<ProcessStartOptions> processDataEntries)
        {
            //attempt to restart failed processes
            foreach (KeyValuePair<string, Process> processEntry in processes)
            {
                if (!ProcessIsAlive(processEntry.Value))
                {
                    var processDataEntry = processDataEntries.Where(x => x.ExecutableName == processEntry.Key).Single();
                    var process = StartProcessLocally(
                                                    processDataEntry.TestAssemblyLocation,
                                                    processDataEntry.AppLocation,
                                                    processDataEntry.ExecutableName,
                                                    processDataEntry.EnvironmentVariables);
                    Thread.Sleep(5000);

                    //Update process in collection
                    processes[processEntry.Key] = process;
                }
            }
        }

        /// <summary>
        /// Returns a string representation of the running processes
        /// </summary>
        /// <param name="processes">Dict of running processes</param>
        /// <returns>A string of all processes from the give dict</returns>
        public static string GetRunningProcessAsString(Dictionary<string, Process>? processes)
        {
            StringBuilder runningProcesses = new StringBuilder();
            if (processes != null)
            {
                foreach (var process in processes)
                {
#pragma warning disable CA1305 // Specify IFormatProvider
                    runningProcesses.AppendLine($"Is {process.Key} running: {UiTestHelpers.ProcessIsAlive(process.Value)}");
#pragma warning restore CA1305 // Specify IFormatProvider
                }
            }
            return runningProcesses.ToString();
        }

        /// <summary>
        /// Takes two paths to existing files and swaps their names and locations effectively swapping the contents of the files.
        /// </summary>
        /// <param name="path1">The path of the first file to swap</param>
        /// <param name="path2">The path of the file to swap it with</param>
        private static void SwapFiles(string path1, string path2)
        {
            // Read the contents of both files
            string file1Contents = File.ReadAllText(path1);
            string file2Contents = File.ReadAllText(path2);

            // Write the contents of file2 to file1
            File.WriteAllText(path1, file2Contents);

            // Write the contents of file1 to file2
            File.WriteAllText(path2, file1Contents);

            Console.WriteLine("File contents swapped successfully.");
        }

        /// <summary>
        /// Builds the solution at the given path.
        /// </summary>
        /// <param name="solutionPath">Absolute path to the sln file to be built</param>
        private static void BuildSolution(string solutionPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build {solutionPath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }

            Console.WriteLine("Solution rebuild initiated.");
        }

        /// <summary>
        /// Replaces existing appsettings.json file with a test appsettings file, builds the solution, and then swaps the files back.
        /// </summary>
        /// <param name="testAssemblyLocation">Absolute path to the current test's working directory</param>
        /// <param name="sampleRelPath">Relative path to the sample app to build starting at the repo's root, does not include appsettings filename</param>
        /// <param name="testAppsettingsRelPath">Relative path to the test appsettings file starting at the repo's root, includes appsettings filename</param>
        /// <param name="solutionFileName">Filename for the sln file to build</param>
        public static void BuildSampleWithTestAppsettings(
            string testAssemblyLocation, 
            string sampleRelPath, 
            string testAppsettingsRelPath,
            string solutionFileName
            )
        {
            string appsettingsDirectory = GetAppsettingsDirectory(testAssemblyLocation, sampleRelPath);
            string appsettingsAbsPath = Path.Combine(appsettingsDirectory, TestConstants.AppSetttingsDotJson);
            string testAppsettingsAbsPath = GetAppsettingsDirectory(testAssemblyLocation, testAppsettingsRelPath);

            SwapFiles(appsettingsAbsPath, testAppsettingsAbsPath);

            try { BuildSolution(appsettingsDirectory + solutionFileName); }
            catch (Exception) { throw; }
            finally { SwapFiles(appsettingsAbsPath, testAppsettingsAbsPath); }
        }
    }

    /// <summary>
    /// Fixture class that installs Playwright browser once per xunit test class that implements it
    /// </summary>
    public class InstallPlaywrightBrowserFixture : IDisposable
    {
        public InstallPlaywrightBrowserFixture()
        {
            UiTestHelpers.InstallPlaywrightBrowser();
        }
        public void Dispose()
        {
        }
    }

    public class ProcessStartOptions
    {
        public string TestAssemblyLocation { get; }

        public string AppLocation { get; }

        public string ExecutableName { get; }

        public Dictionary<string, string>? EnvironmentVariables { get; }

        public ProcessStartOptions(
            string testAssemblyLocation,
            string appLocation,
            string executableName,
            Dictionary<string, string>? environmentVariables = null)
        {
            TestAssemblyLocation = testAssemblyLocation;
            AppLocation = appLocation;
            ExecutableName = executableName;
            EnvironmentVariables = environmentVariables;
        }
    }
}

