// <copyright file="ConvertOptions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Deployment.PowerBi.Cli.Commands.Models
{
    using System.IO;

    public class ConvertOptions
    {
        public ConvertOptions(FileInfo modelFilePath, FileInfo bimOutputFilePath)
        {
            this.ModelFilePath = Path.GetFullPath(modelFilePath.FullName);
            this.BimOutputFilePath = Path.GetFullPath(bimOutputFilePath.FullName);
        }

        public string ModelFilePath { get; }

        public string BimOutputFilePath { get; }
    }
}