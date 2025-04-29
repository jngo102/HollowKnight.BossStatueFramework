using Modding;
using Modding.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BossStatueFramework;

internal class BossStatueFramework : Mod, ILocalSettings<LocalSettings> {
    internal static BossStatueFramework Instance { get; private set; }

    internal static readonly List<IBossStatueMod> BossStatueMods = [];

    internal static LocalSettings LocalSettings = new();

    public BossStatueFramework() : base("Boss Statue Framework") { }

    public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

    internal static int ModCount => BossStatueMods.Count;

    public override void Initialize() {
        Instance = this;

        GetBossMods();

        InitMod();
    }

    internal void InitMod() {
        ModHooks.AfterSavegameLoadHook += AfterSaveGameLoad;
        ModHooks.NewGameHook += AddComponent;
        ModHooks.GetPlayerVariableHook += Instance.GetPlayerVariable;
        ModHooks.SetPlayerVariableHook += Instance.SetPlayerVariable;
    }

    private void AfterSaveGameLoad(SaveGameData data) => AddComponent();

    private object GetPlayerVariable(Type type, string key, object orig) {
        return LocalSettings.Completions.TryGetValue(key, out var completion) ? completion : orig;
    }

    private object SetPlayerVariable(Type type, string key, object obj) {
        if (LocalSettings.Completions.ContainsKey(key)) {
            LocalSettings.Completions[key] = (BossStatue.Completion)obj;
        }
        return obj;
    }

    private void AddComponent() {
        GameManager.instance.gameObject.GetOrAddComponent<WorkshopRemodeler>();
    }

    private void GetBossMods() {
        BossStatueMods.Clear();
        foreach (var mod in ModHooks.GetAllMods()) {
            if (mod.GetType() == GetType()) continue;
            if (mod is IBossStatueMod bossMod) {
                Log("Found boss mod: " + bossMod.GetType().Name);
                BossStatueMods.Add(bossMod);
                if (!LocalSettings.Completions.ContainsKey(bossMod.PlayerData)) {
                    LocalSettings.Completions[bossMod.PlayerData] = new BossStatue.Completion();
                }
            }

        }
    }
    public void OnLoadLocal(LocalSettings settings) => LocalSettings = settings;

    public LocalSettings OnSaveLocal() => LocalSettings;
}
