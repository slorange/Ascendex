using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ascendex.Game.Save;

public sealed class SaveGameStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    private readonly string _saveFilePath;

    public SaveGameStore(string? saveFilePath = null)
    {
        _saveFilePath = saveFilePath ?? GetDefaultSaveFilePath();
    }

    public string SaveFilePath => _saveFilePath;

    public static string GetDefaultSaveFilePath()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, "Ascendex", "save.json");
    }

    public async Task SaveAsync(SaveGameData data, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_saveFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(data, JsonOptions);
        var tempPath = _saveFilePath + ".tmp";
        await File.WriteAllTextAsync(tempPath, json, cancellationToken).ConfigureAwait(false);
        File.Move(tempPath, _saveFilePath, overwrite: true);
    }

    public void Save(SaveGameData data)
    {
        var directory = Path.GetDirectoryName(_saveFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(data, JsonOptions);
        var tempPath = _saveFilePath + ".tmp";
        File.WriteAllText(tempPath, json);
        File.Move(tempPath, _saveFilePath, overwrite: true);
    }

    public async Task<SaveGameData?> TryLoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_saveFilePath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_saveFilePath, cancellationToken).ConfigureAwait(false);
            return Deserialize(json);
        }
        catch
        {
            return null;
        }
    }

    public SaveGameData? TryLoad()
    {
        if (!File.Exists(_saveFilePath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(_saveFilePath);
            return Deserialize(json);
        }
        catch
        {
            return null;
        }
    }

    private static SaveGameData? Deserialize(string json)
    {
        var data = JsonSerializer.Deserialize<SaveGameData>(json, JsonOptions);
        if (data is null || data.Version != SaveGameVersions.Current)
        {
            return null;
        }

        return data;
    }
}
