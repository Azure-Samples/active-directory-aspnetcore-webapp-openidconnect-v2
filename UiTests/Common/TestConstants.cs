// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class TestConstants
    {
        public const string AppSetttingsDotJson = "appsettings.json";
        public const string ClientFilePrefix = "client_";
        public const string EmailText = "Email";
        public const string Headless = "headless";
        public const string HeaderText = "Header";
        public const string HttpStarColon = "http://*:";
        public const string HttpsStarColon = "https://*:";
        public const string KestrelEndpointEnvVar = "Kestrel:Endpoints:Http:Url";
        public const string LocalhostUrl = @"https://localhost:";
        public const string MsidLab3User = "fIDLAB@MSIDLAB3.com";
        public const string MsidLab4User = "idlab@msidlab4.onmicrosoft.com";
        public const string PasswordText = "Password";
        public const string ServerFilePrefix = "server_";
        public const string TodoTitle1 = "Testing create todo item";
        public const string TodoTitle2 = "Testing edit todo item";
        public const string WebAppCrashedString = $"The web app process has exited prematurely.";

        public static readonly string s_oidcWebAppExe = Path.DirectorySeparatorChar + "WebApp-OpenIDConnect-DotNet.exe";
        public static readonly string s_oidcWebAppPath = Path.DirectorySeparatorChar + "WebApp-OpenIDConnect";
        public static readonly string s_todoListClientExe = Path.DirectorySeparatorChar + "TodoListClient.exe";
        public static readonly string s_todoListClientPath = Path.DirectorySeparatorChar + "Client";
        public static readonly string s_todoListServiceExe = Path.DirectorySeparatorChar + "TodoListService.exe";
        public static readonly string s_todoListServicePath = Path.DirectorySeparatorChar + "TodoListService";
    }
}
