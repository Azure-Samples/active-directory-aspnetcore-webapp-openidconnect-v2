// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Identity;
using Common;
using Microsoft.Identity.Lab.Api;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using TC = Common.TestConstants;

namespace B2CUiTest
{
    public class B2CUiTest : IClassFixture<InstallPlaywrightBrowserFixture>
    {
        private const string KeyvaultEmailName = "IdWeb-B2C-user";
        private const string KeyvaultPasswordName = "IdWeb-B2C-password";
        private const string KeyvaultClientSecretName = "IdWeb-B2C-Client-ClientSecret";
        private const string NameOfUser = "unknown";
        private const uint ProcessStartupRetryNum = 3;
        private const string SampleSolutionFileName = "4-2-B2C-Secured-API.sln";
        private const uint TodoListClientPort = 5000;
        private const uint TodoListServicePort = 44332;
        private const string TraceClassName = "B2C";

        private readonly LocatorAssertionsToBeVisibleOptions _assertVisibleOptions = new() { Timeout = 25000 };
        private readonly string _sampleClientAppPath;
        private readonly string _samplePath = Path.Join("4-WebApp-your-API", "4-2-B2C");
        private readonly string _sampleServiceAppPath;
        private readonly Uri _keyvaultUri = new("https://webappsapistests.vault.azure.net");
        private readonly ITestOutputHelper _output;
        private readonly string _testAssemblyLocation = typeof(B2CUiTest).Assembly.Location;

        public B2CUiTest(ITestOutputHelper output)
        {
            _output = output;
            _sampleClientAppPath = Path.Join(_samplePath, TC.s_todoListClientPath);
            _sampleServiceAppPath = Path.Join(_samplePath, TC.s_todoListServicePath);
        }

        [Fact]
        [SupportedOSPlatform("windows")]
        public async Task LocalApp_ValidEmailPasswordCreds_LoginLogout()
        {
            // Web app and api environmental variable setup.
            Dictionary<string, Process>? processes = null;
            DefaultAzureCredential azureCred = new();
            string clientSecret = await UiTestHelpers.GetValueFromKeyvaultWitDefaultCreds(_keyvaultUri, KeyvaultClientSecretName, azureCred);
            var serviceEnvVars = new Dictionary<string, string>
            {
                {"ASPNETCORE_ENVIRONMENT", "Development" },
                {TC.KestrelEndpointEnvVar, TC.HttpStarColon + TodoListServicePort}
            };
            var clientEnvVars = new Dictionary<string, string>
            {
                {"ASPNETCORE_ENVIRONMENT", "Development"},
                {"AzureAdB2C__ClientSecret", clientSecret},
                {TC.KestrelEndpointEnvVar, TC.HttpsStarColon + TodoListClientPort}
            };

            // Get email and password from keyvault.
            string email = await UiTestHelpers.GetValueFromKeyvaultWitDefaultCreds(_keyvaultUri, KeyvaultEmailName, azureCred);
            string password = await UiTestHelpers.GetValueFromKeyvaultWitDefaultCreds(_keyvaultUri, KeyvaultPasswordName, azureCred);

            // Playwright setup. To see browser UI, set 'Headless = false'.
            const string TraceFileName = TraceClassName + "_TodoAppFunctionsCorrectly";
            using IPlaywright playwright = await Playwright.CreateAsync();
            IBrowser browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            IBrowserContext context = await browser.NewContextAsync(new BrowserNewContextOptions { IgnoreHTTPSErrors = true });
            await context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });

            try
            {
                UiTestHelpers.BuildSampleUsingSampleAppsettings(_testAssemblyLocation, _samplePath, SampleSolutionFileName);

                // Start the web app and api processes.
                // The delay before starting client prevents transient devbox issue where the client fails to load the first time after rebuilding.
                var clientProcessOptions = new ProcessStartOptions(_testAssemblyLocation, _sampleClientAppPath, TC.s_todoListClientExe, clientEnvVars); // probs need to add client specific path
                var serviceProcessOptions = new ProcessStartOptions(_testAssemblyLocation, _sampleServiceAppPath, TC.s_todoListServiceExe, serviceEnvVars);

                UiTestHelpers.StartAndVerifyProcessesAreRunning([serviceProcessOptions, clientProcessOptions], out processes, ProcessStartupRetryNum);

                // Navigate to web app the retry logic ensures the web app has time to start up to establish a connection.
                IPage page = await context.NewPageAsync();
                uint InitialConnectionRetryCount = 5;
                while (InitialConnectionRetryCount > 0)
                {
                    try
                    {
                        await page.GotoAsync(TC.LocalhostUrl + TodoListClientPort);
                        break;
                    }
                    catch (PlaywrightException)
                    {
                        await Task.Delay(1000);
                        InitialConnectionRetryCount--;
                        if (InitialConnectionRetryCount == 0) { throw; }
                    }
                }
                LabResponse labResponse = await LabUserHelper.GetB2CLocalAccountAsync();

                // Initial sign in
                _output.WriteLine("Starting web app sign-in flow.");
                ILocator emailEntryBox = page.GetByPlaceholder("Email Address");
                await emailEntryBox.FillAsync(email);
                await emailEntryBox.PressAsync("Tab");
                await page.GetByPlaceholder("Password").FillAsync(password);
                await page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();
                await Assertions.Expect(page.GetByText("TodoList")).ToBeVisibleAsync(_assertVisibleOptions);
                await Assertions.Expect(page.GetByText(NameOfUser)).ToBeVisibleAsync(_assertVisibleOptions);
                _output.WriteLine("Web app sign-in flow successful.");

                // Sign out
                _output.WriteLine("Starting web app sign-out flow.");
                await page.GetByRole(AriaRole.Link, new() { Name = "Sign out" }).ClickAsync();
                _output.WriteLine("Signing out ...");
                await Assertions.Expect(page.GetByText("You have successfully signed out.")).ToBeVisibleAsync(_assertVisibleOptions);
                await Assertions.Expect(page.GetByText(NameOfUser)).ToBeHiddenAsync();
                _output.WriteLine("Web app sign out successful.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"the UI automation failed: {ex} output: {ex.Message}.");
            }
            finally
            {
                // End all processes.
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