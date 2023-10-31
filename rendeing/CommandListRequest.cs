using graphics;
using Reactive.Bindings;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace rendering;

public record CommandListRequest : IDisposable {
    public readonly ReactiveProperty<int> Count;
    public readonly ReactiveProperty<CommandListRequest[]?> ChildRequests;
    public readonly ReadOnlyReactiveProperty<int> TotalCount;

    public CommandListRequest(int count, CommandListRequest[]? childRequests = null) {
        Count = new(initialValue: count, raiseEventScheduler: ImmediateScheduler.Instance);
        ChildRequests = new(initialValue: childRequests, raiseEventScheduler: ImmediateScheduler.Instance);

        TotalCount = Observable
            .CombineLatest(
                Count,
                ChildRequests,
                (count, children) => (count, children))
            .SelectMany(tuple => {
                var (count, children) = tuple;

                if (children == null || children.Length == 0) {
                    return Observable.Return(count);
                }

                var childObservables = children.Select(child => child.TotalCount).ToArray();

                return Observable.CombineLatest(childObservables)
                    .Select(totalCounts => totalCounts.Sum())
                    .Select(childTotal => count * childTotal);
            })
            .ToReadOnlyReactiveProperty(eventScheduler: ImmediateScheduler.Instance);
    }

    public ReadOnlySpan<CommandList> Slice(ReadOnlySpan<CommandList> mainSpan, int index, int? childIndex = null) {
        var count = Count.Value;

        if (index < 0 || index >= count) {
            throw new ArgumentOutOfRangeException(nameof(index), "Index out of range.");
        }

        var chunkSize = mainSpan.Length / count;
        var remainder = mainSpan.Length % count;
        var start = index * chunkSize + Math.Min(index, remainder);
        var length = chunkSize + (index < remainder ? 1 : 0);
        var slice = mainSpan.Slice(start, length);

        if (!childIndex.HasValue) {
            return slice;
        }

        var childRequests = ChildRequests.Value;

        if (childRequests == null || childIndex.Value < 0 || childIndex.Value >= childRequests.Length) {
            throw new InvalidOperationException("Invalid child index or no child requests.");
        }

        var childRequest = childRequests[childIndex.Value];
        var totalCommandsBeforeChild =
            childRequests.Take(childIndex.Value).Sum(cr => cr.TotalCount.Value);
        var childStart = totalCommandsBeforeChild;
        var childLength = childRequest.TotalCount.Value;

        return slice.Slice(childStart, childLength);
    }

    public void Dispose() {
        Count.Dispose();
        ChildRequests.Dispose();
        TotalCount.Dispose();
    }
}