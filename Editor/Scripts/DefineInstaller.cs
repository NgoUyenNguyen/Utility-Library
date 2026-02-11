using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace NgoUyenNguyen.Editor
{
    [InitializeOnLoad]
    public static class DefineInstaller
    {
        static DefineInstaller()
        {
            // var existLT = HasType("LeanTween");
            // SetDefine("NGOUYENNGUYEN_LEANTWEEN", existLT);
        }
        
        public static bool HasType(string typeName) => 
            TypeCache.GetTypesDerivedFrom<object>().Any(type => type.Name == typeName);
        
        public static void SetDefine(string define, bool enable)
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(group);
            var definesString = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            
            var allDefines = definesString.Split(';')
                .Select(d => d.Trim())
                .Where(d => !string.IsNullOrEmpty(d))
                .ToList();

            var changed = false;

            if (enable)
            {
                if (!allDefines.Contains(define))
                {
                    allDefines.Add(define);
                    changed = true;
                }
            }
            else
            {
                if (allDefines.Contains(define))
                {
                    allDefines.Remove(define);
                    changed = true;
                }
            }

            if (!changed) return;
            
            var newDefines = string.Join(";", allDefines.ToArray());
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newDefines);
            Debug.Log($"<b>[DefineInstaller]</b> {(enable ? "Added" : "Removed")} define: <color=yellow>{define}</color>");
        }
    }
}