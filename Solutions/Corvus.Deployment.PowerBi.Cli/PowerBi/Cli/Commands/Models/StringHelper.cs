// Contents derived from https://github.com/TabularEditor/TabularEditor/blob/885ff9f84f6497b7958a402854d959d747e0e0a2/TOMWrapper/TOMWrapper/TabularDeployer.cs

namespace Corvus.Deployment.PowerBi.Cli.Commands.Models
{
    using System;

    public static class StringHelper
    {
        public static bool EqualsI(this string str, string other)
        {
            if (str == null || other == null) return str == other;
            return str.Equals(other, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
