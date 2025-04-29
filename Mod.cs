using Modding;
using Modding.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BossStatueFramework;

internal class BossStatueFramework() : Mod("Boss Statue Framework"), ILocalSettings<LocalSettings>
{
    internal static readonly List<IBossStatueMod> BossStatueMods = [];

    private static LocalSettings _localSettings = new();

    public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

    internal static int ModCount => BossStatueMods.Count;

    public override void Initialize() {
        GetBossMods();
        InitMod();
    }

    private void InitMod() {
        ModHooks.AfterSavegameLoadHook += AfterSaveGameLoad;
        ModHooks.NewGameHook += AddComponent;
        ModHooks.GetPlayerVariableHook += GetPlayerVariable;
        ModHooks.SetPlayerVariableHook += SetPlayerVariable;
    }

    private void AfterSaveGameLoad(SaveGameData data) => AddComponent();

    private object GetPlayerVariable(Type type, string key, object orig) {
        return _localSettings.Completions.TryGetValue(key, out var completion) ? completion : orig;
    }

    private object SetPlayerVariable(Type type, string key, object obj) {
        if (_localSettings.Completions.ContainsKey(key)) {
            _localSettings.Completions[key] = (BossStatue.Completion)obj;
        }
        return obj;
    }

    private void AddComponent() {
        if (ModCount <= 0) return;
        GameManager.instance.gameObject.GetOrAddComponent<WorkshopRemodeler>();
    }

    private void GetBossMods() {
        BossStatueMods.Clear();
        foreach (var mod in ModHooks.GetAllMods()) {
            if (mod.GetType() == GetType()) continue;
            if (mod is IBossStatueMod bossMod) {
                Log("Found boss mod: " + bossMod.GetType().Name);
                BossStatueMods.Add(bossMod);
                if (!_localSettings.Completions.ContainsKey(bossMod.PlayerData)) {
                    _localSettings.Completions[bossMod.PlayerData] = new BossStatue.Completion();
                }
            }

        }
    }
    public void OnLoadLocal(LocalSettings settings) => _localSettings = settings;

    public LocalSettings OnSaveLocal() => _localSettings;
}
