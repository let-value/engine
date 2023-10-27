namespace core;

public class TemporalStateBuffer<T> {
    private readonly Tuple<T, DateTime>[] Buffer;
    private readonly int Capacity;
    private int Count;
    private int CurrentPosition;

    public TemporalStateBuffer(int capacity) {
        Capacity = capacity;
        Buffer = new Tuple<T, DateTime>[capacity];
        CurrentPosition = 0;
        Count = 0;
    }

    public void Push(T item) {
        var localPosition = Interlocked.Increment(ref CurrentPosition) - 1;
        Buffer[localPosition % Capacity] = new(item, DateTime.UtcNow);

        Interlocked.Increment(ref Count);
        if (Count > Capacity) {
            Interlocked.Exchange(ref Count, Capacity);
        }
    }

    public T GetLatest() {
        var localPosition = Volatile.Read(ref CurrentPosition);
        if (Count == 0) {
            throw new InvalidOperationException("Buffer is empty");
        }

        var latestPosition = (localPosition - 1 + Capacity) % Capacity;
        return Buffer[latestPosition].Item1;
    }

    public bool IsInterpolationNeeded(double requesterTickRate) {
        if (Count < 2) return false;

        var timeDifference = DateTime.UtcNow
                             - Buffer[(Volatile.Read(ref CurrentPosition) - 1 + Capacity) % Capacity].Item2;
        var timeDifferenceInSeconds = timeDifference.TotalSeconds;

        return timeDifferenceInSeconds > 1.0 / requesterTickRate;
    }

    public IEnumerable<Tuple<T, DateTime>> GetAllStates() {
        var localCount = Volatile.Read(ref Count);
        var localPosition = Volatile.Read(ref CurrentPosition);

        for (var i = 0; i < localCount; i++) {
            var position = (localPosition - i - 1 + Capacity) % Capacity;
            yield return Buffer[position];
        }
    }
}