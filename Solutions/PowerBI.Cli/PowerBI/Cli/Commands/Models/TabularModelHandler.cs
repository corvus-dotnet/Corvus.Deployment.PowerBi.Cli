// Contents derived from https://github.com/TabularEditor/TabularEditor/blob/885ff9f84f6497b7958a402854d959d747e0e0a2/TOMWrapper/TOMWrapper/TabularModelHandler.cs

namespace PowerBI.Cli.Commands.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TOM = Microsoft.AnalysisServices.Tabular;

    internal class TabularModelHandler
    {
        internal static List<Tuple<TOM.NamedMetadataObject, TOM.ObjectState>> GetObjectsNotReady(TOM.Database database)
        {
            var result = new List<Tuple<TOM.NamedMetadataObject, TOM.ObjectState>>();

            // Find partitions that are not in the "Ready" state:
            result.AddRange(
                    database.Model.Tables.SelectMany(t => t.Partitions).Where(p => p.State != TOM.ObjectState.Ready)
                    .Select(p => new Tuple<TOM.NamedMetadataObject, TOM.ObjectState>(p, p.State))
                    );

            // Find calculated columns that are not in the "Ready" state:
            result.AddRange(
                    database.Model.Tables.SelectMany(t => t.Columns.OfType<TOM.CalculatedColumn>()).Where(c => c.State != TOM.ObjectState.Ready)
                    .Select(c => new Tuple<TOM.NamedMetadataObject, TOM.ObjectState>(c, c.State))
                );

            // Find calculated columns that are not in the "Ready" state:
            result.AddRange(
                    database.Model.Tables.SelectMany(t => t.Columns.OfType<TOM.DataColumn>()).Where(c => c.State != TOM.ObjectState.Ready)
                    .Select(c => new Tuple<TOM.NamedMetadataObject, TOM.ObjectState>(c, c.State))
                );

            return result;
        }

        internal static List<Tuple<TOM.NamedMetadataObject, string>> CheckErrors(TOM.Database database)
        {
            var result = new List<Tuple<TOM.NamedMetadataObject, string>>();
            foreach (var t in database.Model.Tables)
            {
                result.AddRange(t.Measures.Where(m => !string.IsNullOrEmpty(m.ErrorMessage)).Select(m => new Tuple<TOM.NamedMetadataObject, string>(m, m.ErrorMessage)));
                if (database.CompatibilityLevel >= 1400) result.AddRange(t.Measures.Where(m => !string.IsNullOrEmpty(m.DetailRowsDefinition?.ErrorMessage)).Select(m => new Tuple<TOM.NamedMetadataObject, string>(m, "Detail rows expression: " + m.DetailRowsDefinition.ErrorMessage)));
                result.AddRange(t.Columns.Where(c => !string.IsNullOrEmpty(c.ErrorMessage)).Select(c => new Tuple<TOM.NamedMetadataObject, string>(c, c.ErrorMessage)));
                result.AddRange(t.Partitions.Where(p => !string.IsNullOrEmpty(p.ErrorMessage)).Select(p => new Tuple<TOM.NamedMetadataObject, string>(p, p.ErrorMessage)));
                if (database.CompatibilityLevel >= 1470 && t.CalculationGroup != null)
                {
                    result.AddRange(t.CalculationGroup.CalculationItems.Where(ci => !string.IsNullOrEmpty(ci.ErrorMessage)).Select(ci => new Tuple<TOM.NamedMetadataObject, string>(ci, ci.ErrorMessage)));
                    result.AddRange(t.CalculationGroup.CalculationItems.Where(ci => !string.IsNullOrEmpty(ci.FormatStringDefinition?.ErrorMessage)).Select(ci => new Tuple<TOM.NamedMetadataObject, string>(ci, "Format string expression: " + ci.FormatStringDefinition.ErrorMessage)));
                }
            }
            foreach (var r in database.Model.Roles)
            {
                result.AddRange(r.TablePermissions.Where(tp => !string.IsNullOrEmpty(tp.ErrorMessage)).Select(tp => new Tuple<TOM.NamedMetadataObject, string>(tp, tp.ErrorMessage)));
            }
            return result;
        }
    }
}