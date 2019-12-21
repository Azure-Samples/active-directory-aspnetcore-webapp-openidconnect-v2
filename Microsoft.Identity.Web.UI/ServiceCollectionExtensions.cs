using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

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
