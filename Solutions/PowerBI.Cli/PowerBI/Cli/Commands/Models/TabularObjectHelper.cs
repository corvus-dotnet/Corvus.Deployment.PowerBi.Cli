// Contents derived from https://github.com/TabularEditor/TabularEditor/blob/885ff9f84f6497b7958a402854d959d747e0e0a2/TOMWrapper/TOMWrapper/TabularDeployer.cs

namespace PowerBI.Cli.Commands.Models
{
    using System;
    using System.Text.RegularExpressions;
    using TOM = Microsoft.AnalysisServices.Tabular;

    public static class TabularObjectHelper
    {
        private static string GetObjectPathTableObject(TOM.NamedMetadataObject obj)
        {
            var name = obj.Name;
            if (name.Contains(".")) name = "[" + name + "]";

            if (obj.Parent != null)
                return obj.Parent.GetObjectPath() + "." + name;
            else
                return name;
        }

        public static string GetObjectPath(this TOM.MetadataObject obj)
        {
            switch (obj.ObjectType)
            {
                case TOM.ObjectType.Model:
                    return "Model";
                case TOM.ObjectType.Measure:
                case TOM.ObjectType.Table:
                case TOM.ObjectType.Column:
                case TOM.ObjectType.Hierarchy:
                    return GetObjectPathTableObject(obj as TOM.NamedMetadataObject);
                case TOM.ObjectType.Level:
                    var level = obj as TOM.Level;
                    return GetObjectPathTableObject(level.Hierarchy) + "." + level.Name;
                case TOM.ObjectType.KPI:
                    return GetObjectPathTableObject((obj as TOM.KPI).Measure) + ".KPI";
                case TOM.ObjectType.Variation:
                    return GetObjectPathTableObject((obj as TOM.Variation).Column) + ".Variations." + QuotePath((obj as TOM.Variation).Name);
                case TOM.ObjectType.Relationship:
                case TOM.ObjectType.DataSource:
                case TOM.ObjectType.Role:
                case TOM.ObjectType.Expression:
                case TOM.ObjectType.Perspective:
                case TOM.ObjectType.Culture:
                    return obj.ObjectType.ToString() + "." + QuotePath((obj as TOM.NamedMetadataObject).Name);
                case TOM.ObjectType.Partition:
                    return "TablePartition." + QuotePath((obj as TOM.Partition).Table?.Name ?? "") + "." + QuotePath((obj as TOM.Partition).Name);
                case TOM.ObjectType.RoleMembership:
                    var mrm = obj as TOM.ModelRoleMember;
                    return mrm.Role.GetObjectPath() + "." + mrm.Name;
                case TOM.ObjectType.CalculationGroup:
                    var cg = obj as TOM.CalculationGroup;
                    return cg.Table.GetObjectPath() + ".CalculationGroup";
                case TOM.ObjectType.TablePermission:
                    var tp = obj as TOM.TablePermission;
                    return tp.Role.GetObjectPath() + "." + tp.Table.Name;
                case TOM.ObjectType.CalculationItem:
                    var ci = obj as TOM.CalculationItem;
                    return ci.CalculationGroup.GetObjectPath() + ".CalculationGroup." + ci.Name;
                case TOM.ObjectType.AlternateOf:
                    var ao = obj as TOM.AlternateOf;
                    return ao.Column.GetObjectPath() + ".AlternateOf";
                default:
                    throw new NotSupportedException($"Cannot create reference for object of type {obj.ObjectType}.");
            }

        }

        private static string QuotePath(string name)
        {
            return name.Contains(".") ? $"[{name}]" : name;
        }

        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }

        public static string Pluralize(this string str)
        {
            if (str.EndsWith("y")) return str.Substring(0, str.Length - 1) + "ies";
            else if (!str.EndsWith("data")) return str + "s";
            else return str;
        }

        public static string GetTypeName(this ObjectType objType, bool plural = false)
        {
            if (objType == ObjectType.Culture) return "Translation" + (plural ? "s" : "");

            var result = objType.ToString().SplitCamelCase();
            return plural ? result.Pluralize() : result;
        }

        public static string GetTypeName(this Type type, bool plural = false)
        {
            var n = type.Name.SplitCamelCase();
            return plural ? n.Pluralize() : n;
        }
    }

}
