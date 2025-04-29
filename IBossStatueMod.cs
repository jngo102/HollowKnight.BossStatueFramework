using UnityEngine;

namespace BossStatueFramework;

/// <summary>
/// The width of the plinth for the statue base.
/// </summary>
public enum PlinthType {
    /// <summary>
    /// Narrow plinth, e.g. Soul Warrior.
    /// </summary>
    Small,
    /// <summary>
    /// Medium-width plinth, e.g. Dung Defender.
    /// </summary>
    Medium,
    /// <summary>
    /// Wide plinth, e.g. Pure Vessel.
    /// </summary>
    Long,
}

/// <summary>
/// The type of switch used to activate the boss statue's alternate.
/// </summary>
public enum AltSwitchType {
    /// <summary>
    /// No alternate boss statue.
    /// </summary>
    None,
    /// <summary>
    /// Dream nail toggle.
    /// </summary>
    Dream,
    /// <summary>
    /// Lever switch to strike.
    /// </summary>
    Lever,
}

/// <summary>
/// Defines a mod that uses the Boss Statue Framework.
/// </summary>
public interface IBossStatueMod {
    /// <summary>
    /// The language key to the localized name of the boss statue.
    /// </summary>
    public string NameKey { get; }

    /// <summary>
    /// The language key to the localized description of the boss statue.
    /// </summary>
    public string DescriptionKey { get; }

    /// <summary>
    /// The name of the Unity scene to change to when challenging the boss statue.
    /// </summary>
    public string SceneName { get; }

    /// <summary>
    /// The player data entry of the boss statue's state.
    /// </summary>
    public string PlayerData { get; }

    /// <summary>
    /// The type of plinth for the statue base; either Small, Medium, or Long.
    /// </summary>
    public PlinthType PlinthType { get; }

    /// <summary>
    /// The sprite to display above the boss statue base.
    /// </summary>
    public Sprite Sprite { get; }

    /// <summary>
    /// The scale factor of the boss statue's sprite.
    /// </summary>
    public float SpriteScale { get; }

    /// <summary>
    /// The type of switch that this boss statue has; set to <cref="AltSwitchType.None" /> for no alt.
    /// </summary>
    public AltSwitchType AltType { get; }

    /// <summary>
    /// The language key to the localized name of the alternative boss statue.
    /// </summary>
    public string AltNameKey { get; }

    /// <summary>A
    /// The language key to the localized description of the alternative boss statue.
    /// </summary>
    public string AltDescriptionKey { get; }

    /// <summary>
    /// The name of the Unity scene to change to when challenging the alternative boss statue.
    /// </summary>
    public string AltSceneName { get; }

    /// <summary>
    /// The player data entry of the alternative boss statue's state.
    /// </summary>
    public string AltPlayerData { get; }

    /// <summary>
    /// The sprite to display above the altnernate boss statue base.
    /// </summary>
    public Sprite AltSprite { get; }

    /// <summary>
    /// The scale factor of the alternate boss statue's sprite.
    /// </summary>
    public float AltSpriteScale { get; }
}
