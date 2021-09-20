// <copyright file="IVellumConsole.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Deployment.PowerBi.Cli
{
    using System.CommandLine;
    using Spectre.Console;

    public interface ICompositeConsole : IConsole, IAnsiConsole
    {
    }
}