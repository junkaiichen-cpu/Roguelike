using Godot;
using System;

public static class MetaProgressionSave
{
    private const string SavePath = "user://faith_fight_meta.json";

    public static MetaProgressionState Load()
    {
        MetaProgressionState state = new();
        if (!FileAccess.FileExists(SavePath)) return state;

        using FileAccess file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
        Variant parsed = Json.ParseString(file.GetAsText());
        if (parsed.VariantType != Variant.Type.Dictionary) return state;

        Godot.Collections.Dictionary data = parsed.AsGodotDictionary();
        state.AddFaith(data.ContainsKey("faith_currency") ? Mathf.Max(0, data["faith_currency"].AsInt32()) : 0);

        if (data.ContainsKey("permanent_upgrades")
            && data["permanent_upgrades"].VariantType == Variant.Type.Dictionary)
        {
            Godot.Collections.Dictionary upgrades = data["permanent_upgrades"].AsGodotDictionary();
            foreach (Variant key in upgrades.Keys)
            {
                state.SetUpgradeLevel(key.AsString(), Mathf.Max(0, upgrades[key].AsInt32()));
            }
        }

        if (data.ContainsKey("unlocks") && data["unlocks"].VariantType == Variant.Type.Array)
        {
            Godot.Collections.Array unlocks = data["unlocks"].AsGodotArray();
            foreach (Variant unlock in unlocks) state.Unlock(unlock.AsString());
        }

        return state;
    }

    public static bool Save(MetaProgressionState state)
    {
        if (state == null) return false;

        Godot.Collections.Dictionary upgrades = new();
        foreach (var pair in state.PermanentUpgrades) upgrades[pair.Key] = pair.Value;
        Godot.Collections.Array unlocks = new();
        foreach (string unlock in state.Unlocks) unlocks.Add(unlock);

        Godot.Collections.Dictionary data = new()
        {
            ["save_version"] = MetaProgressionState.CurrentSaveVersion,
            ["faith_currency"] = state.FaithCurrency,
            ["permanent_upgrades"] = upgrades,
            ["unlocks"] = unlocks,
        };

        using FileAccess file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        if (file == null) return false;
        file.StoreString(Json.Stringify(data));
        return true;
    }
}
