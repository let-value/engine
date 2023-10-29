using graphics;

namespace rendering;

public record CommandListRequest(int Count, CommandListRequest[]? ChildRequests = null) {
    public int GetTotalCommandListsRequired() {
        var total = Count;

        if (ChildRequests == null) {
            return total;
        }

        int totalChildRequests = 0;

        foreach (var childRequests in ChildRequests) {
            totalChildRequests += childRequests.GetTotalCommandListsRequired();
        }

        total *= totalChildRequests;

        return total;
    }

    public ReadOnlySpan<CommandList> Slice(int index, ReadOnlySpan<CommandList> mainSpan) {
        if (index < 0 || index >= Count) {
            throw new ArgumentOutOfRangeException(nameof(index), "Index out of range.");
        }

        var chunkSize = mainSpan.Length / Count;
        return mainSpan.Slice(index * chunkSize, chunkSize);
    }

    public ReadOnlySpan<CommandList> SliceChild(int childIndex, ReadOnlySpan<CommandList> mainSpan) {
        if (ChildRequests == null || childIndex >= ChildRequests.Length) {
            throw new InvalidOperationException("Invalid child index or no child requests.");
        }

        var start = 0;
        for (var i = 0; i <= childIndex; i++) {
            var size = ChildRequests[i].GetTotalCommandListsRequired();
            if (i == childIndex) {
                return mainSpan.Slice(start, size);
            }

            start += size;
        }

        throw new InvalidOperationException("Should not reach here.");
    }
}