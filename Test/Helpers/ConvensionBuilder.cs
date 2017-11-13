// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Helpers
{
    public static class ConvensionBuilder
    {
        private static IServiceProvider CreateContextServices(this DbContext context)
            => ((IInfrastructure<IServiceProvider>)context).Instance;

        public static ModelBuilder CreateConventionBuilder(this DbContext context)
        {
            var contextServices = CreateContextServices(context);

            var conventionSetBuilder = contextServices.GetRequiredService<IConventionSetBuilder>();
            var conventionSet = contextServices.GetRequiredService<ICoreConventionSetBuilder>().CreateConventionSet();
            conventionSet = conventionSetBuilder == null
                ? conventionSet
                : conventionSetBuilder.AddConventions(conventionSet);
            return new ModelBuilder(conventionSet);
        }
    }
}