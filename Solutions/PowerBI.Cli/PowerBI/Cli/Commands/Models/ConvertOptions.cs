// <copyright file="ConvertOptions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace PowerBI.Cli.Commands.Models
{
    using System.IO;
    using NDepend.Path;
    
    public class ConvertOptions
    {
        public ConvertOptions(FileInfo modelFilePath, FileInfo bimOutputFilePath)
        {
            this.ModelFilePath = modelFilePath.FullName.ToAbsoluteFilePath();
            this.BimOutputFilePath = bimOutputFilePath.FullName.ToAbsoluteFilePath();
        }

        public IAbsoluteFilePath ModelFilePath { get; }

        public IAbsoluteFilePath BimOutputFilePath { get; }
    }
} 