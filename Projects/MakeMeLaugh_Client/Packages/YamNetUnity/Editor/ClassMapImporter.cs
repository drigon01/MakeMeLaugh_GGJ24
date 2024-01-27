using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace YamNetUnity.Editor
{
    [ScriptedImporter(1, ".classmap.csv")]
    public class ClassMapImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var classes = new Dictionary<int, string>();
            
            using (var reader = new StreamReader(ctx.assetPath))
            {
                reader.ReadLine(); // Discard the first line.

                var regex = new Regex("^(?<index>\\d+),(?<mid>[^,]+),(?<display_name>.*)$");
                while (reader.ReadLine() is { } line)
                {
                    if (string.IsNullOrWhiteSpace(line)) 
                        continue;
                    
                    var match = regex.Match(line);
                    if (!match.Success)
                        continue;

                    int classId = int.Parse(match.Groups["index"].Value);
                    string className = match.Groups["display_name"].Value.Trim('"');
                    classes[classId] = className;
                }
            }
            
            var classMap = ScriptableObject.CreateInstance<ClassMap>();
            var so = new SerializedObject(classMap);
            var sp = so.FindProperty("classNames");
            int maxClassId = classes.Keys.Max();
            sp.arraySize = maxClassId + 1;
            foreach (var kvp in classes)
            {
                sp.GetArrayElementAtIndex(kvp.Key).stringValue = kvp.Value;
            }
            so.ApplyModifiedProperties();
            
            ctx.AddObjectToAsset("classMap", classMap);
        }
    }
}