using MelonLoader;
using UnityEngine;

namespace StellarDrive_Tech;

public class AssetManager
{
    private readonly Dictionary<string, UnityEngine.Object> _assets;

    public class AssetNotFoundException : Exception
    {
        public AssetNotFoundException() { }
        public AssetNotFoundException(string message) : base(message) { }
        public AssetNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public AssetManager(string assetBundlesRoot)
    {
        _assets = [];
        foreach (string assetBundlePath in Directory.GetFiles(assetBundlesRoot))
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
            if (assetBundle is null)
            {
                // May not be an asset bundle so just leave a warning
                Melon<Core>.Logger.Warning($"failed to load asset bundle from file {Path.GetFileName(assetBundlePath)}");
                continue;
            }

            string assetBundleName = Path.GetFileNameWithoutExtension(assetBundlePath);
            Melon<Core>.Logger.Msg($"Loading asset bundle {assetBundleName}...");
            foreach (UnityEngine.Object asset in assetBundle.LoadAllAssets())
            {
                string assetPath = $"{assetBundleName}/{asset.name}";
                _assets.Add(assetPath, asset);
                Melon<Core>.Logger.Msg($"Loaded asset {assetPath}");
            }
        }
    }

    public T InstantiateAsset<T>(string assetPath) where T : UnityEngine.Object
    {
        if (!_assets.ContainsKey(assetPath))
        {
            throw new AssetNotFoundException($"asset {assetPath} not found");
        }

        return UnityEngine.Object.Instantiate(_assets[assetPath]) as T;
    }
}
