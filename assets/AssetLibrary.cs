namespace assets;

public class AssetLibrary(AssetFactory factory) {
    private readonly Dictionary<object, Asset> Assets = new();

    public Asset Load(object key, string path) {
        if (Assets.TryGetValue(key, out var asset)) {
            return asset;
        }

        var newAsset = factory.Create(path);
        Assets.Add(key, newAsset);

        return newAsset;
    }
}