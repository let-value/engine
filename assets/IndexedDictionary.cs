namespace assets;

public class IndexedDictionary<TKey, TValue>(int? capacity = null) where TKey : notnull {
    private readonly List<TValue> List = capacity.HasValue ? new(capacity.Value) : new();
    private readonly Dictionary<TKey, int> KeyToIndexMap = capacity.HasValue ? new(capacity.Value) : new();

    public TValue this[int index] => List[index];
    public TValue this[TKey key] => List[KeyToIndexMap[key]];
    public int Count => List.Count;

    public void Add(TKey key, TValue value) {
        KeyToIndexMap.Add(key, List.Count);
        List.Add(value);
    }

    public bool TryGetValue(TKey key, out TValue value) {
        if (KeyToIndexMap.TryGetValue(key, out var index)) {
            value = List[index];
            return true;
        }

        value = default!;
        return false;
    }
}