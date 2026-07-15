using System;
using System.Diagnostics;
using Ascendex.Game;
using Avalonia.Threading;

namespace Ascendex.ViewModels;

/// <summary>Advances elapsed game time on the UI thread while any bar is active.</summary>
public sealed class GameTickLoop : IDisposable
{
    private static readonly TimeSpan PresentationInterval = TimeSpan.FromMilliseconds(1000.0 / 30.0);

    private readonly GameSession _session;
    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _elapsed = new();
    private double _timeDisplayAccumulatorSeconds;
    private bool _isSuspended;

    public GameTickLoop(GameSession session)
    {
        _session = session;
        _timer = new DispatcherTimer
        {
            Interval = PresentationInterval,
        };
        _timer.Tick += OnTimerTick;
        _session.ActiveBarsChanged += SyncTimerState;
        SyncTimerState();
    }

    public event Action? TimeDisplayRefreshRequested;

    public event Action? PresentationAdvanced;

    public void Suspend()
    {
        _isSuspended = true;
        _timer.Stop();
        _elapsed.Reset();
    }

    public void Resume()
    {
        _isSuspended = false;
        SyncTimerState();
    }

    public void Dispose()
    {
        _timer.Stop();
        _elapsed.Stop();
        _timer.Tick -= OnTimerTick;
        _session.ActiveBarsChanged -= SyncTimerState;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        var realSeconds = _elapsed.Elapsed.TotalSeconds;
        _elapsed.Restart();
        if (realSeconds <= 0)
        {
            return;
        }

        _session.Advance(realSeconds);
        PresentationAdvanced?.Invoke();
        _timeDisplayAccumulatorSeconds += realSeconds;
        if (_timeDisplayAccumulatorSeconds >= 1)
        {
            _timeDisplayAccumulatorSeconds %= 1;
            TimeDisplayRefreshRequested?.Invoke();
        }
    }

    private void SyncTimerState()
    {
        if (!_isSuspended && _session.HasActiveBars())
        {
            _elapsed.Restart();
            _timer.Start();
            return;
        }

        _timer.Stop();
        _elapsed.Reset();
    }
}
