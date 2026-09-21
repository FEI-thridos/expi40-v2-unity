using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Assets.Editor
{
    public class LinkXmlGenerator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private const string _sourceFolder = "Assets/Plugins/Expi40";
        private const string _outputPath = "Assets/Plugins/Expi40/link.xml";

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android ||
                report.summary.platform == BuildTarget.WSAPlayer)
            {
                GenerateLinkXml();
            }
        }

        private static void GenerateLinkXml()
        {
            if (!Directory.Exists(_sourceFolder))
            {
                Debug.LogWarning($"Folder not found: {_sourceFolder}");
                return;
            }

            var dllFiles = Directory.GetFiles(_sourceFolder, "*.dll", SearchOption.AllDirectories);

            if (dllFiles.Length == 0)
            {
                Debug.LogWarning("No DLLs found under Expi40.");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("<linker>");

            foreach (var dllPath in dllFiles)
            {
                string assemblyName = Path.GetFileNameWithoutExtension(dllPath);

#if !UNITY_STANDALONE_WIN || !UNITY_EXPI40_SERVER
                if (assemblyName == "Expi40.OpcUaClient")
                {
                    continue;
                }
#endif

                sb.AppendLine($"  <assembly fullname=\"{assemblyName}\" preserve=\"all\" />");
            }

            sb.AppendLine("</linker>");

            File.WriteAllText(_outputPath, sb.ToString());
            AssetDatabase.Refresh();

            Debug.Log($"Generated link.xml with {dllFiles.Length} assemblies.");
        }
    }
}