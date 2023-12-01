using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace rendering.loop;

public delegate void GameLoopHandler(double deltaTime);

public class GameLoop(IOptionsMonitor<GameLoopOptions> optionsMonitor) : IDisposable {
    private readonly CancellationTokenSource CancellationTokenSource = new();

    private readonly Stopwatch Stopwatch = new();

    private Thread? Thread;
    private float? UpdateTime = GetUpdateTime(optionsMonitor.CurrentValue);

    private IDisposable? OptionsListener => optionsMonitor.OnChange(OnOptionsChanged);

    public void Dispose() {
        CancellationTokenSource.Cancel();
        OptionsListener?.Dispose();
    }

    public event GameLoopHandler OnUpdate = null!;

    private static float? GetUpdateTime(GameLoopOptions options) {
        return 1f / options.UpdateRate * 1000;
    }

    private void OnOptionsChanged(GameLoopOptions options) {
        UpdateTime = GetUpdateTime(options);
    }

    public void Start() {
        Thread ??= new(() => {
            while (!CancellationTokenSource.IsCancellationRequested) Run();
        });


        Thread.Start();
    }

    public void Stop() {
        Thread?.Join();
    }

    public void Run() {
        Stopwatch.Stop();
        var deltaTime = Stopwatch.ElapsedMilliseconds;

        Stopwatch.Restart();
        OnUpdate.Invoke(deltaTime);

        if (!UpdateTime.HasValue) {
            return;
        }

        Stopwatch.Stop();
        var elapsedTime = Stopwatch.ElapsedMilliseconds;
        Stopwatch.Start();
        var delayTime = UpdateTime - elapsedTime;

        if (delayTime > 0) {
            Thread.Sleep((int)delayTime);
        }
    }
}