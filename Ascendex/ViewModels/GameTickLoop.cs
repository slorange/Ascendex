using System;
using Ascendex.Game;
using Avalonia.Threading;

namespace Ascendex.ViewModels;

/// <summary>Drives <see cref="GameSession.Tick"/> on the UI thread while any bar is active.</summary>
public sealed class GameTickLoop : IDisposable
{
    private readonly GameSession _session;
    private readonly DispatcherTimer _timer;

    public GameTickLoop(GameSession session)
    {
        _session = session;
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(GameBalance.Training.TickIntervalMilliseconds),
        };
        _timer.Tick += OnTimerTick;
        _session.ActiveBarsChanged += SyncTimerState;
        SyncTimerState();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
        _session.ActiveBarsChanged -= SyncTimerState;
    }

    private void OnTimerTick(object? sender, EventArgs e) => _session.Tick();

    private void SyncTimerState()
    {
        if (_session.HasActiveBars())
        {
            _timer.Start();
            return;
        }

        _timer.Stop();
    }
}
