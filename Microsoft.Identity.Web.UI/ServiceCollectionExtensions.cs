// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Identity.Web.UI
{
    public static class ServiceCollectionExtensions
    {
        public static IMvcBuilder AddMicrosoftIdentityUI(this IMvcBuilder builder)
        {
            builder.ConfigureApplicationPartManager(apm =>
            {
                apm.FeatureProviders.Add(new MicrosoftIdentityAccountControllerFeatureProvider());
            });

            return builder;
        }
    }
}
