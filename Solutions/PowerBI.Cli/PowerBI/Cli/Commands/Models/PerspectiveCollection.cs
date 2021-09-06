// Contents orginall from https://github.com/TabularEditor/TabularEditor/blob/885ff9f84f6497b7958a402854d959d747e0e0a2/TOMWrapper/TOMWrapper/Perspective.cs

namespace PowerBI.Cli.Commands.Models
{
    using System.Collections.Generic;

    public partial class PerspectiveCollection
    {
        internal class SerializedPerspective
        {
            public string Name { get; set; }
    
            public string Description{ get; set; }
            
            public Dictionary<string, string> Annotations{ get; set; }
        }
    }
}