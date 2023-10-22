using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace rendering.loop;

public delegate void GameLoopHandler(double deltaTime);

public class GameLoop(IOptionsMonitor<GameLoopOptions> optionsMonitor) : IDisposable {
    private readonly CancellationTokenSource cancellationTokenSource = new();

    private readonly Stopwatch stopwatch = new();

    private Thread? thread;
    private float? updateTime = GetUpdateTime(optionsMonitor.CurrentValue);

    private IDisposable? optionsListener => optionsMonitor.OnChange(OnOptionsChanged);

    public void Dispose() {
        cancellationTokenSource.Cancel();
        optionsListener?.Dispose();
    }

    public event GameLoopHandler OnUpdate = null!;

    private static float? GetUpdateTime(GameLoopOptions options) {
        return 1f / options.UpdateRate * 1000;
    }

    private void OnOptionsChanged(GameLoopOptions options) {
        updateTime = GetUpdateTime(options);
    }

    public void Start() {
        thread ??= new(() => {
            while (!cancellationTokenSource.IsCancellationRequested) {
                Run();
            }
        });


        thread.Start();
    }

    public void Stop() {
        thread?.Join();
    }

    public void Run() {
        stopwatch.Stop();
        var deltaTime = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        OnUpdate.Invoke(deltaTime);

        if (!updateTime.HasValue) return;

        stopwatch.Stop();
        var elapsedTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Start();
        var delayTime = updateTime - elapsedTime;

        if (delayTime > 0) {
            Thread.Sleep((int)delayTime);
        }
    }
}