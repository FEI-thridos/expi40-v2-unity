using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Assets.Editor
{
    public static class AssetBundlesGenerator
    {
        [MenuItem("Assets/Generate Asset Bundles")]
        static void GenerateAssetBundles()
        {
            // Clear AssetBundles directory if it exists
            string assetBundlesDirectory = "Assets/Models/AssetBundles";
            if (Directory.Exists(assetBundlesDirectory))
            {
                Directory.Delete(assetBundlesDirectory, true);
            }

            // List of supported build targets
            List<BuildTarget> buildTargets = new()
        {
            BuildTarget.StandaloneWindows64,
            BuildTarget.Android,
            BuildTarget.WSAPlayer
        };

            // Build asset bundles for each target
            foreach (BuildTarget buildTarget in buildTargets)
            {
                string outputPath = $"{assetBundlesDirectory}/{buildTarget}";
                Directory.CreateDirectory(outputPath);
                BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, buildTarget);
            }
        }
    }
}