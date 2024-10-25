// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Common;
using Microsoft.Identity.Lab.Api;
using Microsoft.Playwright;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using Process = System.Diagnostics.Process;
using TC = Common.TestConstants;


namespace GraphUserTokenCacheTest
{
    public class GraphUserTokenCacheTest
    {
        private const uint ClientPort = 44321;
        private const uint NumProcessRetries = 3;
        private const string SampleSlnFileName = "2-2-TokenCache.sln";
        private const string SignOutPageUriPath = @"/MicrosoftIdentity/Account/SignedOut";
        private const string SqlDbName = "MY_TOKEN_CACHE_DATABASE";
        private const string SqlServerConnectionString = "Server=(localdb)\\mssqllocaldb;Integrated Security=true";
        private const string SqlTableName = "TokenCache";
        private const string TraceFileClassName = "GraphUserTokenCacheTest";
        private readonly LocatorAssertionsToBeVisibleOptions _assertVisibleOptions = new() { Timeout = 25000 };
        private readonly ITestOutputHelper _output;
        private readonly string _sampleAppPath = "2-WebApp-graph-user" + Path.DirectorySeparatorChar + "2-2-TokenCache" + Path.DirectorySeparatorChar.ToString();
        private readonly string _testAppsettingsPath = "UiTests" + Path.DirectorySeparatorChar + "GraphUserTokenCache" + Path.DirectorySeparatorChar.ToString() + TC.AppSetttingsDotJson;
        private readonly string _testAssemblyLocation = typeof(GraphUserTokenCacheTest).Assembly.Location;

        public GraphUserTokenCacheTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task ChallengeUser_MicrosoftIdFlow_LocalApp_ValidEmailPasswordCreds_LoginLogoutAsync()
        {
            // Setup web app and api environmental variables.
            var clientEnvVars = new Dictionary<string, string>
            {
                {"ASPNETCORE_ENVIRONMENT", "Development"},
                {TC.KestrelEndpointEnvVar, TC.HttpsStarColon + ClientPort}
            };

            Dictionary<string, Process>? processes = null;

            // Arrange Playwright setup, to see the browser UI set Headless = false.
            const string TraceFileName = TraceFileClassName + "_LoginLogout";
            using IPlaywright playwright = await Playwright.CreateAsync();
            IBrowser browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            IBrowserContext context = await browser.NewContextAsync(new BrowserNewContextOptions { IgnoreHTTPSErrors = true });
            await context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });
            IPage page = await context.NewPageAsync();
            string uriWithPort = TC.LocalhostUrl + ClientPort;

            try
            {
                // Make sure database and table for cache exist, if not they will be created.
                UiTestHelpers.EnsureDatabaseAndTokenCacheTableExist(SqlServerConnectionString, SqlDbName, SqlTableName, _output);

                // Build the sample app with correct appsettings file.
                UiTestHelpers.BuildSampleUsingTestAppsettings(_testAssemblyLocation, _sampleAppPath, _testAppsettingsPath, SampleSlnFileName);

                // Start the web app and api processes.
                // The delay before starting client prevents transient devbox issue where the client fails to load the first time after rebuilding
                var clientProcessOptions = new ProcessStartOptions(_testAssemblyLocation, _sampleAppPath, TC.s_oidcWebAppExe, clientEnvVars);

                bool areProcessesRunning = UiTestHelpers.StartAndVerifyProcessesAreRunning([clientProcessOptions], out processes, NumProcessRetries);

                if (!areProcessesRunning)
                {
                    _output.WriteLine($"Process not started after {NumProcessRetries} attempts.");
                    StringBuilder runningProcesses = new();
                    foreach (var process in processes)
                    {
#pragma warning disable CA1305 // Specify IFormatProvider
                        runningProcesses.AppendLine($"Is {process.Key} running: {UiTestHelpers.ProcessIsAlive(process.Value)}");
#pragma warning restore CA1305 // Specify IFormatProvider
                    }
                    Assert.Fail(TC.WebAppCrashedString + " " + runningProcesses.ToString());
                }

                LabResponse labResponse = await LabUserHelper.GetSpecificUserAsync(TC.MsidLab4User);

                // Initial sign in
                _output.WriteLine("Starting web app sign-in flow.");
                string email = labResponse.User.Upn;
                await UiTestHelpers.NavigateToWebApp(uriWithPort, page);
                await UiTestHelpers.FirstLogin_MicrosoftIdFlow_ValidEmailPassword(page, email, labResponse.User.GetOrFetchPassword());
                await Assertions.Expect(page.GetByText("Integrating Azure AD V2")).ToBeVisibleAsync(_assertVisibleOptions);
                await Assertions.Expect(page.GetByText(email)).ToBeVisibleAsync(_assertVisibleOptions);
                _output.WriteLine("Web app sign-in flow successful.");

                // Sign out
                _output.WriteLine("Starting web app sign-out flow.");
                await page.GetByRole(AriaRole.Link, new() { Name = "Sign out" }).ClickAsync();
                await UiTestHelpers.PerformSignOut_MicrosoftIdFlow(page, email, TC.LocalhostUrl + ClientPort + SignOutPageUriPath, _output);
                _output.WriteLine("Web app sign out successful.");
            }
            catch (Exception ex)
            {
                // Adding guid in case of multiple test runs. This will allow screenshots to be matched to their appropriate test runs.
                var guid = Guid.NewGuid().ToString();
                try
                {
                    if (page != null)
                    {
                        await page.ScreenshotAsync(new PageScreenshotOptions() { Path = $"ChallengeUser_MicrosoftIdFlow_LocalApp_ValidEmailPasswordCreds_TodoAppFunctionsCorrectlyScreenshotFail{guid}.png", FullPage = true });
                    }
                }
                catch
                {
                    _output.WriteLine("No Screenshot.");
                }

                string runningProcesses = UiTestHelpers.GetRunningProcessAsString(processes);
                Assert.Fail($"the UI automation failed: {ex} output: {ex.Message}.\n{runningProcesses}\nTest run: {guid}");
            }
            finally
            {
                // Make sure all processes and their children are stopped.
                UiTestHelpers.EndProcesses(processes);

                // Stop tracing and export it into a zip archive.
                string path = UiTestHelpers.GetTracePath(_testAssemblyLocation, TraceFileName);
                await context.Tracing.StopAsync(new() { Path = path });
                _output.WriteLine($"Trace data for {TraceFileName} recorded to {path}.");

                // Close the browser and stop Playwright.
                await browser.CloseAsync();
                playwright.Dispose();
            }
        }
    }
}