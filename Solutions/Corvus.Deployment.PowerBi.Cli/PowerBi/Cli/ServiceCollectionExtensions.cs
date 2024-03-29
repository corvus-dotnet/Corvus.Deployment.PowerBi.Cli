﻿// <copyright file="ServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Deployment.PowerBi.Cli
{
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds application wide services.
        /// </summary>
        /// <param name="serviceCollection">Application's ServiceCollection.</param>
        public static void AddCommonServices(this IServiceCollection serviceCollection)
        {
        }
    }
}