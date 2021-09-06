// Contents originally from https://github.com/TabularEditor/TabularEditor/blob/master/TOMWrapper/TOMWrapper/Serialization/SplitModelSerializer.cs

namespace PowerBI.Cli.Commands.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using NDepend.Path;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class MultiFileModel
    {
        private const string AnnRelationships = "TabularEditor_Relationships";
        private const string AnnCultures = "TabularEditor_Cultures";
        private const string AnnPerspectives = "TabularEditor_Perspectives";
        private const string AnnInperspective = "TabularEditor_InPerspective";
        private const string AnnNames = "TabularEditor_TranslatedNames";
        private const string AnnDescriptions = "TabularEditor_TranslatedDescriptions";
        private const string AnnDisplayfolders = "TabularEditor_TranslatedDisplayFolders";

        public static string Load(IAbsoluteFilePath path)
        {
            var document = LoadJsonFile(path.ToString());
            var model = document["model"] as JObject;

            InArray(path.ParentDirectoryPath.ToString(), "dataSources", model);

            if (Directory.Exists(path.ParentDirectoryPath.ToString() + "\\tables"))
            {
                var tables = new JArray();
                foreach (var tablePath in Directory.GetDirectories(path.ParentDirectoryPath.ToString() + "\\tables"))
                {
                    var filesInTableFolder = Directory.GetFiles(tablePath, "*.json");
                    if (filesInTableFolder.Length != 1)
                    {
                        throw new FileNotFoundException($"Folder '{tablePath}' is expected to contain exactly one .json file.");}
                    
                    var tableFile = filesInTableFolder[0];

                    var table = LoadJsonFile(tableFile);
                    InArray(tablePath, "columns", table);
                    InArray(tablePath, "partitions", table);
                    InArray(tablePath, "measures", table);
                    InArray(tablePath, "hierarchies", table);
                    InArray(tablePath, "annotations", table);
                    InArray(tablePath, "calculationGroup.calculationItems", table);

                    tables.Add(table);
                }

                model?.Add("tables", tables);
            }

            InArray(path.ParentDirectoryPath.ToString(), "relationships", model);
            InArray(path.ParentDirectoryPath.ToString(), "cultures", model);
            InArray(path.ParentDirectoryPath.ToString(), "perspectives", model);
            InArray(path.ParentDirectoryPath.ToString(), "roles", model);

            ResolveAnnotations(model);

            return document.ToString();
        }

        private static void ResolveAnnotations(JObject model)
        {
            // Relationships:
            var relationships = new JArray();

            foreach (var table in model.Enum("tables"))
            {
                GetAnnotatedRelationships(table, relationships);
            }

            if (relationships.Count > 0)
            {
                model["relationships"] = relationships;
            }

            // Perspectives:
            var perspectivesJson = model.GetAnnotation(AnnPerspectives, true);
            if (perspectivesJson != null)
            {
                model["perspectives"] = ConvertPerspectivesJson(perspectivesJson);
            }

            // Cultures:
            var culturesJson = model.GetAnnotation(AnnCultures, true);
            if (culturesJson != null)
            {
                model["cultures"] = ConvertCulturesJson(culturesJson);
            }

            // Perspective memberships:
            foreach (var table in model.Enum("tables"))
            {
                ResolveTablePerspective(table, model);
            }

            // Translations:
            if (model["cultures"] != null)
            {
                ResolveTranslations(model);
            }
        }

        private static void ResolveTranslations(JObject model)
        {
            ApplyAllTranslations(model, c => GetOrCreateModelTranslation(model, c));

            foreach (var perspective in model.Enum("perspectives"))
            {
                ApplyAllTranslations(perspective,
                    c => GetOrCreatePerspectiveTranslation(model, c, (string)perspective["name"]));
            }

            foreach (var table in model.Enum("tables"))
            {
                var tableName = (string)table["name"];
                ApplyAllTranslations(table, c => GetOrCreateTableTranslation(model, c, tableName));

                foreach (var measure in table.Enum("measures"))
                {
                    ApplyAllTranslations(measure,
                        c => GetOrCreateMeasureTranslation(model, c, tableName, (string)measure["name"]));
                }

                foreach (var column in table.Enum("columns"))
                {
                    ApplyAllTranslations(column,
                        c => GetOrCreateColumnTranslation(model, c, tableName, (string)column["name"]));
                }

                foreach (var hierarchy in table.Enum("hierarchies"))
                {
                    var hierarchyName = (string)hierarchy["name"];
                    ApplyAllTranslations(hierarchy,
                        c => GetOrCreateHierarchyTranslation(model, c, tableName, hierarchyName));

                    foreach (var level in hierarchy.Enum("levels"))
                    {
                        ApplyAllTranslations(level,
                            c => GetOrCreateLevelTranslation(model, c, tableName, hierarchyName,
                                (string)level["name"]));
                    }
                }
            }
        }

        private static JObject GetOrCreateLevelTranslation(JObject model, string cultureName, string tableName,
            string hierarchyName, string levelName)
        {
            var hierarchyTran = GetOrCreateHierarchyTranslation(model, cultureName, tableName, hierarchyName);
            return hierarchyTran.GetOrCreateArrayObj("levels", levelName);
        }

        private static JObject GetOrCreateHierarchyTranslation(JObject model, string cultureName, string tableName,
            string hierarchyName)
        {
            var tableTran = GetOrCreateTableTranslation(model, cultureName, tableName);
            return tableTran.GetOrCreateArrayObj("hierarchies", hierarchyName);
        }

        private static JObject GetOrCreateColumnTranslation(JObject model, string cultureName, string tableName,
            string columnName)
        {
            var tableTran = GetOrCreateTableTranslation(model, cultureName, tableName);
            return tableTran.GetOrCreateArrayObj("columns", columnName);
        }

        private static JObject GetOrCreateMeasureTranslation(JObject model, string cultureName, string tableName,
            string measureName)
        {
            var tableTran = GetOrCreateTableTranslation(model, cultureName, tableName);
            return tableTran.GetOrCreateArrayObj("measures", measureName);
        }

        private static JObject GetOrCreateTableTranslation(JObject model, string cultureName, string tableName)
        {
            var modelTran = GetOrCreateModelTranslation(model, cultureName);
            return modelTran.GetOrCreateArrayObj("tables", tableName);
        }

        private static JObject GetOrCreatePerspectiveTranslation(JObject model, string cultureName,
            string perspectiveName)
        {
            var modelTran = GetOrCreateModelTranslation(model, cultureName);
            return modelTran.GetOrCreateArrayObj("perspectives", perspectiveName);
        }

        private static JObject GetOrCreateModelTranslation(JObject model, string cultureName)
        {
            var culture = GetOrCreateCulture(model, cultureName);
            var translations = culture["translations"] as JObject;

            if (translations == null)
            {
                translations = new JObject();
                culture["translations"] = translations;
            }

            var modelTran = translations["model"] as JObject;

            if (modelTran == null)
            {
                modelTran = new JObject();
                modelTran["name"] = model["name"] == null ? "Model" : (string)model["name"];
                translations["model"] = modelTran;
            }

            return modelTran;
        }

        private static JObject GetOrCreateCulture(JObject model, string cultureName)
        {
            return model.GetOrCreateArrayObj("cultures", cultureName);
        }

        private static JObject GetOrCreateArrayObj(this JObject baseObject, string arrayName, string objectName)
        {
            var array = baseObject.Sub(arrayName);
            var result = array.OfType<JObject>()
                .FirstOrDefault(j => j["name"] != null && (string)j["name"] == objectName);

            if (result == null)
            {
                result = new JObject();
                result["name"] = objectName;
                array.Add(result);
            }

            return result;
        }

        private static void ApplyAllTranslations(JObject translatableObject, Func<string, JObject> translation)
        {
            var translatedNamesJson = translatableObject.GetAnnotation(AnnNames, true);
            var translatedDescriptionsJson = translatableObject.GetAnnotation(AnnDescriptions, true);
            var translatedDisplayFoldersJson = translatableObject.GetAnnotation(AnnDisplayfolders, true);

            if (translatedNamesJson != null)
            {
                ApplyTranslations(translatedNamesJson, "translatedCaption", translation);
            }

            if (translatedDescriptionsJson != null)
            {
                ApplyTranslations(translatedDescriptionsJson, "translatedDescription", translation);
            }

            if (translatedDisplayFoldersJson != null)
            {
                ApplyTranslations(translatedDisplayFoldersJson, "translatedDisplayFolder", translation);
            }
        }

        private static void ApplyTranslations(string annotatedTranslationJson, string translatedProperty,
            Func<string, JObject> translation)
        {
            if (annotatedTranslationJson[0] == '[')
            {
                var jTranArr = JArray.Parse(annotatedTranslationJson);

                foreach (var item in jTranArr)
                {
                    translation((string)item["Key"])[translatedProperty] = (string)item["Value"];
                }
            }
            else
            {
                var jTran = JObject.Parse(annotatedTranslationJson);

                foreach (var prop in jTran.Properties())
                {
                    translation(prop.Name)[translatedProperty] = (string)prop.Value;
                }
            }
        }

        private static void ResolveTablePerspective(JObject table, JObject model)
        {
            var inPerspectives = StringToHashSet(table.GetAnnotation(AnnInperspective, true));

            if (inPerspectives.Count == 0)
            {
                return;
            }

            bool any = false;

            var perspectiveTableMap = new List<Tuple<string, JObject>>();

            foreach (var perspective in model.Enum("perspectives"))
            {
                var perspectiveName = (string)perspective["name"];

                if (inPerspectives.Contains(perspectiveName))
                {
                    var perspectiveTable = new JObject();
                    perspectiveTable["name"] = table["name"];
                    perspective.Sub("tables").Add(perspectiveTable);
                    perspectiveTableMap.Add(Tuple.Create(perspectiveName, perspectiveTable));

                    any = true;
                }
            }

            if (!any)
            {
                return; 
            }

            ResolveObjectPerspective(table, perspectiveTableMap, "measures");
            ResolveObjectPerspective(table, perspectiveTableMap, "columns");
            ResolveObjectPerspective(table, perspectiveTableMap, "hierarchies");
        }

        private static void ResolveObjectPerspective(JObject table, List<Tuple<string, JObject>> perspectiveTableMap,
            string collectionName)
        {
            foreach (var obj in table.Enum(collectionName))
            {
                var inPerspectives = StringToHashSet(obj.GetAnnotation(AnnInperspective, true));
                foreach (var p in perspectiveTableMap)
                {
                    if (inPerspectives.Contains(p.Item1))
                    {
                        var pObj = new JObject();
                        pObj["name"] = obj["name"];
                        p.Item2.Sub(collectionName).Add(pObj);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or creates the specified array
        /// </summary>
        /// <param name="baseObject"></param>
        /// <param name="arrayProperty"></param>
        /// <returns></returns>
        private static JArray Sub(this JObject baseObject, string arrayProperty)
        {
            if (!(baseObject[arrayProperty] is JArray array))
            {
                array = new JArray();
                baseObject.Add(arrayProperty, array);
            }

            return array;
        }

        private static HashSet<string> StringToHashSet(string jsonStringArray)
        {
            if (jsonStringArray == null)
            {
                return new HashSet<string>();
            }

            return new HashSet<string>(
                JsonConvert.DeserializeObject<IEnumerable<string>>(jsonStringArray) ?? Array.Empty<string>(),
                StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Converts an array of culture names into an equivalent TOM representation of the Culture
        /// </summary>
        /// <returns></returns>
        private static JArray ConvertCulturesJson(string culturesAnnotationJson)
        {
            var cultures = JsonConvert.DeserializeObject<IEnumerable<string>>(culturesAnnotationJson);
            var result = new JArray();

            foreach (var culture in cultures)
            {
                var cult = new JObject();
                cult["name"] = culture;
                cult["translations"] = new JObject();
                result.Add(cult);
            }

            return result;
        }

        /// <summary>
        /// Converts a string representing an array of <see cref="PerspectiveCollection.SerializedPerspective"/> objects into an equivalent TOM representation
        /// </summary>
        /// <returns></returns>
        private static JArray ConvertPerspectivesJson(string perspectivesAnnotationJson)
        {
            var perspectives =
                JsonConvert.DeserializeObject<PerspectiveCollection.SerializedPerspective[]>(
                    perspectivesAnnotationJson);
            var result = new JArray();

            foreach (var perspective in perspectives)
            {
                var obj = new JObject();
                obj["name"] = perspective.Name;
                obj["description"] = perspective.Description;

                if (perspective.Annotations.Count > 0)
                {
                    var anns = new JArray();

                    foreach (var kvp in perspective.Annotations)
                    {
                        var ann = new JObject();
                        ann["name"] = kvp.Key;
                        ann["value"] = kvp.Value;
                        anns.Add(ann);
                    }

                    obj["annotations"] = anns;
                }

                result.Add(obj);
            }

            return result;
        }

        private static void GetAnnotatedRelationships(JObject table, JArray relationships)
        {
            var relationshipJson = table.GetAnnotation(AnnRelationships, true);

            if (relationshipJson == null)
            {
                return;
            }

            var annotatedRelationships = JArray.Parse(relationshipJson);

            foreach (var relationship in annotatedRelationships)
            {
                relationships.Add(relationship);
            }
        }

        /// <summary>
        /// Enumerates all JObjects of the specified JArray (provided it exists)
        /// </summary>
        /// <param name="baseObject"></param>
        /// <param name="arrayProperty"></param>
        /// <returns></returns>
        private static IEnumerable<JObject> Enum(this JObject baseObject, string arrayProperty)
        {
            if (baseObject[arrayProperty] is JArray array)
            {
                return array.OfType<JObject>();
            }

            return Enumerable.Empty<JObject>();
        }

        private static string GetAnnotation(this JObject obj, string annotationName, bool removeIfFound = false)
        {
            if (!(obj["annotations"] is JArray annotations))
            {
                return null;
            }

            var annotation = annotations.OfType<JObject>().FirstOrDefault(j => (string)j["name"] == annotationName);

            if (annotation == null)
            {
                return null;
            }

            var value = annotation["value"];

            if (removeIfFound)
            {
                annotation.Remove();
            }

            if (value.Type == JTokenType.String)
            {
                return (string)value;
            }

            if (value.Type == JTokenType.Array)
            {
                return string.Join("\r\n", (value as JArray).Select(j => (string)j).ToArray());
            }

            throw new NotSupportedException();
        }

        private static void InArray(string path, string arrayPath, JObject baseObject)
        {
            var objPath = arrayPath.Split('.');
            var arrayName = objPath.Last();

            var array = new JArray();
            if (Directory.Exists(path + "\\" + arrayName))
            {
                foreach (var file in Directory.GetFiles(path + "\\" + arrayName, "*.json").OrderBy(n => n))
                {
                    array.Add(LoadJsonFile(file));
                }

                for (int i = 0; i < objPath.Length - 1; i++)
                {
                    baseObject = baseObject?[objPath[i]] as JObject;
                }

                if (baseObject?[arrayName] is JArray existingArray)
                {
                    existingArray.Merge(array);
                }
                else
                {
                    baseObject?.Add(arrayName, array);
                }
            }
        }

        private static JObject LoadJsonFile(string path)
        {
            try
            {
                return JObject.Parse(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to load Tabular Model (Compatibility Level 1200+) from {path}.\r\nError:\r\n{ex.GetType()} - {ex.Message}", ex);
            }
        }
    }
}