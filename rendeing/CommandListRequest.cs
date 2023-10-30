using graphics;

namespace rendering;

public record CommandListRequest(int Count, CommandListRequest[]? ChildRequests = null) {
    public int GetTotalCommandListsRequired() {
        var total = Count;

        if (ChildRequests == null) {
            return total;
        }

        var totalChildRequests = 0;

        foreach (var childRequests in ChildRequests) {
            totalChildRequests += childRequests.GetTotalCommandListsRequired();
        }

        total *= totalChildRequests;

        return total;
    }

    public ReadOnlySpan<CommandList> Slice(ReadOnlySpan<CommandList> mainSpan, int index, int? childIndex = null) {
        if (index < 0 || index >= Count) {
            throw new ArgumentOutOfRangeException(nameof(index), "Index out of range.");
        }

        var chunkSize = mainSpan.Length / Count;
        var remainder = mainSpan.Length % Count;
        var start = index * chunkSize + Math.Min(index, remainder);
        var length = chunkSize + (index < remainder ? 1 : 0);
        var slice = mainSpan.Slice(start, length);

        if (!childIndex.HasValue) {
            return slice;
        }

        if (ChildRequests == null || childIndex.Value < 0 || childIndex.Value >= ChildRequests.Length) {
            throw new InvalidOperationException("Invalid child index or no child requests.");
        }

        var childRequest = ChildRequests[childIndex.Value];
        var totalCommandsBeforeChild =
            ChildRequests.Take(childIndex.Value).Sum(cr => cr.GetTotalCommandListsRequired());
        var childStart = totalCommandsBeforeChild;
        var childLength = childRequest.GetTotalCommandListsRequired();

        return slice.Slice(childStart, childLength);
    }
}