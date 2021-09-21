// Contents derived from https://github.com/TabularEditor/TabularEditor/blob/885ff9f84f6497b7958a402854d959d747e0e0a2/TOMWrapper/TOMWrapper\TabularObject.cs

namespace PowerBI.Cli.Commands.Models
{
    public enum ObjectType
    {
        // Special types needed by Tabular Editor (doesn't exist in the TOM):
        CalculationGroupTable = -7,
        CalculationItemCollection = -6,
        PartitionCollection = -4,
        KPIMeasure = -3,
        Group = -2,
        Folder = -1,

        // Default types:
        Null = 0,
        Model = 1,
        DataSource = 2,
        Table = 3,
        Column = 4,
        AttributeHierarchy = 5,
        Partition = 6,
        Relationship = 7,
        Measure = 8,
        Hierarchy = 9,
        Level = 10,
        Annotation = 11,
        KPI = 12,
        Culture = 13,
        ObjectTranslation = 14,
        LinguisticMetadata = 15,
        Perspective = 29,
        PerspectiveTable = 30,
        PerspectiveColumn = 31,
        PerspectiveHierarchy = 32,
        PerspectiveMeasure = 33,
        Role = 34,
        RoleMembership = 35,
        TablePermission = 36,
        Variation = 37,
        Expression = 41,
        ColumnPermission = 42,
        DetailRowsDefinition = 43,
        CalculationGroup = 46,
        CalculationItem = 47,
        AlternateOf = 48,
        Database = 1000
    }
}