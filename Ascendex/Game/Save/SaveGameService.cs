using System;
using System.Threading;

namespace Ascendex.Game.Save;

public sealed class SaveGameService : IDisposable
{
    private readonly SaveGameStore _store;
    private readonly SaveGameSettings _settings;
    private readonly object _saveLock = new();
    private GameSession? _session;
    private Func<int>? _getSelectedMainTab;
    private Timer? _autoSaveTimer;

    public SaveGameService(SaveGameStore? store = null, SaveGameSettings? settings = null)
    {
        _store = store ?? new SaveGameStore();
        _settings = settings ?? new SaveGameSettings();
    }

    public string SaveFilePath => _store.SaveFilePath;

    public TimeSpan AutoSaveInterval => _settings.AutoSaveInterval;

    public static SaveGameService CreateDefault() => new(new SaveGameStore(), new SaveGameSettings());

    /// <summary>Flush the bound session immediately (e.g. Android <c>OnPause</c>).</summary>
    public static void FlushActiveSave() => _activeInstance?.SaveNow();

    private static SaveGameService? _activeInstance;

    public (GameSession Session, int SelectedMainTab) LoadOrCreateNew(bool applyOfflineBankTime = true)
    {
        var data = _store.TryLoad();
        if (data is null)
        {
            return (GameSession.CreateNew(), 0);
        }

        var session = GameSession.CreateFromSave(data);
        if (applyOfflineBankTime)
        {
            session.ApplyOfflineBankTime(data.SavedAtUtc);
        }

        return (session, ClampTab(data.SelectedMainTab));
    }

    public void BindAutoSave(GameSession session, Func<int> getSelectedMainTab)
    {
        UnbindAutoSave();
        _session = session;
        _getSelectedMainTab = getSelectedMainTab;
        _activeInstance = this;

        var interval = _settings.AutoSaveInterval;
        _autoSaveTimer = new Timer(
            static state => ((SaveGameService)state!).SaveNow(),
            this,
            interval,
            interval);
    }

    public void SaveNow()
    {
        if (_session is null || _getSelectedMainTab is null)
        {
            return;
        }

        lock (_saveLock)
        {
            WriteSave();
        }
    }

    public void Dispose()
    {
        SaveNow();
        UnbindAutoSave();
    }

    private void UnbindAutoSave()
    {
        if (_activeInstance == this)
        {
            _activeInstance = null;
        }

        _autoSaveTimer?.Dispose();
        _autoSaveTimer = null;
        _session = null;
        _getSelectedMainTab = null;
    }

    private void WriteSave()
    {
        if (_session is null || _getSelectedMainTab is null)
        {
            return;
        }

        var data = SaveGameMapper.ToSaveData(_session.State, ClampTab(_getSelectedMainTab()));
        _store.Save(data);
    }

    private static int ClampTab(int tab) => Math.Clamp(tab, 0, 2);
}
