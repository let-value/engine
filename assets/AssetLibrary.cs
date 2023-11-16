using assets.assets;
using Assimp;

namespace assets;

public class AssetLibrary(ModelFactory modelFactory) {
    private readonly Dictionary<object, ModelAsset> Models = new();

    public ModelAsset LoadModel(object key, string path, PostProcessSteps flags = default) {
        if (Models.TryGetValue(key, out var asset)) {
            return asset;
        }

        var newAsset = modelFactory.Create(key, path, flags);
        Models.Add(key, newAsset);

        return newAsset;
    }
}