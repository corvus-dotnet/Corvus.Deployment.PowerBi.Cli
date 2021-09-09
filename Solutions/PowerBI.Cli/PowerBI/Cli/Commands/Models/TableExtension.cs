// Contents derived from https://github.com/TabularEditor/TabularEditor/blob/885ff9f84f6497b7958a402854d959d747e0e0a2/TOMWrapper/TOMWrapper/Table.cs

namespace PowerBI.Cli.Commands.Models
{
    using System.Linq;
    using TOM = Microsoft.AnalysisServices.Tabular;

    internal static class TableExtension
    {
        public static TOM.PartitionSourceType GetSourceType(this TOM.Table table)
        {
            return table.Partitions.FirstOrDefault()?.SourceType ?? TOM.PartitionSourceType.None;
        }
        public static bool IsCalculatedOrCalculationGroup(this TOM.Table table)
        {
            var sourceType = table.GetSourceType();
            return sourceType == TOM.PartitionSourceType.Calculated || sourceType == TOM.PartitionSourceType.CalculationGroup;
        }
        public static bool IsCalculated(this TOM.Table table)
        {
            var sourceType = table.GetSourceType();
            return sourceType == TOM.PartitionSourceType.Calculated;
        }
    }
}