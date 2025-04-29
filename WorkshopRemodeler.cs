using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using USceneMgr = UnityEngine.SceneManagement.SceneManager;
using static BossStatue;
using System;
using Modding;
using UnityEngine.UI;

namespace BossStatueFramework;

internal class WorkshopRemodeler : MonoBehaviour {
    private const float StatueStartX = 0;
    private const float StatueSpacingX = 10;

    private static float LeftWallX => StatueStartX - StatueSpacingX * BossStatueFramework.ModCount;

    private static GameObject _darknessPrefab;

    private void Awake() {
        AddHooks();
    }

    private void OnDestroy() {
        USceneMgr.activeSceneChanged -= ModifyWorkshop;
        ClearILHooks();
    }

    private void AddHooks() {
        USceneMgr.activeSceneChanged += ModifyWorkshop;
    }

    private void ModifyWorkshop(Scene prevScene, Scene nextScene) {
        ClearILHooks();

        if (nextScene.name != "GG_Workshop") {
            return;
        }

        AddILHooks();
        CreateDarknessPrefab();
        AddFloor();
        AdjustLeftEdge();
        CreateStatues();
        ModifyBackground();
        ModifyRoof();
    }

    private const int SpriteLength = 64;
    
    private void CreateDarknessPrefab() {
        var tex = new Texture2D(SpriteLength, SpriteLength);
        for (var i = 0; i < tex.width; i++) {
            for (var j = 0; j < tex.height; j++) {
                tex.SetPixel(i, j, Color.black);
            }
        }
        tex.Apply();
        var floorSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, SpriteLength);
        _darknessPrefab = new GameObject("Darkness Prefab");
        _darknessPrefab.AddComponent<SpriteRenderer>().sprite = floorSprite;
    }

    private void AdjustLeftEdge() {
        // Move left wall
        foreach (var wall in FindObjectsOfType<Transform>().Where(go => go.name.Contains("GG_gold_wall") && go.transform.position.x < 10 && go.transform.position.y > 30)) {
            wall.SetPositionX(LeftWallX);
        }

        foreach (var rend in FindObjectsOfType<SpriteRenderer>().Where(rend => rend.name.Contains("gg_bush") && rend.transform.position.x < 20)) {
            rend.gameObject.SetActive(false);
        }

        // Expand left edge of camera lock area
        foreach (var area in FindObjectsOfType<CameraLockArea>().Where(area => area.cameraXMin <= 0)) {
            area.cameraXMin = LeftWallX + 14;
        }

        static IEnumerator ModifyWorkshopRoutine() {
            yield return new WaitUntil(() => FindObjectsOfType<GameObject>().
                FirstOrDefault(go => go.name.Contains("SceneBorder") && go.transform.position.x < 0) != null);

            var sceneBorder = FindObjectsOfType<GameObject>().FirstOrDefault(go => go.name.Contains("SceneBorder") && go.transform.position.x < 0);
            sceneBorder.SetActive(false);

            yield return new WaitUntil(() => GameObject.Find("Chunk 1 0") != null);

            var chunk = GameObject.Find("Chunk 1 0");
            var chunkRenderer = chunk.GetComponent<MeshRenderer>();
            chunkRenderer.enabled = false;
            var col = chunk.GetComponent<EdgeCollider2D>();
            var points = col.points;
            points[4].x = LeftWallX;
            points[5].x = LeftWallX;
            points[28].x = LeftWallX;
            points[29].x = LeftWallX;
            col.points = points;

            yield return new WaitUntil(() => GameObject.Find("World Edge v2") != null);

            GameObject.Find("World Edge v2").SetActive(false);

            yield return new WaitUntil(() => GameObject.Find("side_pillar_left") != null);
            // Remove left pillar collider
            GameObject.Find("side_pillar_left").SetActive(false);
        }

        GameManager.instance.StartCoroutine(ModifyWorkshopRoutine());
    }

    private void AddFloor() {
        var floorPrefab = FindObjectsOfType<GameObject>().FirstOrDefault(go => go.name.Contains("GG_scenery_0013_8") && go.transform.position.y > 30);
        if (floorPrefab == null) {
            Debug.LogError("Failed to find floor prefab!");
            return;
        }

        var floorJointPrefab = FindObjectsOfType<GameObject>().FirstOrDefault(go => go.name.Contains("GG_scene_arena_extra_0000_3_short") && go.transform.position.y > 30 && go.transform.GetScaleY() > 0);
        if (floorJointPrefab == null) {
            Debug.LogError("Failed to find floor joint prefab!");
            return;
        }

        var floorSpacing = floorPrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        
        var floorX = StatueStartX;
        do {
            Instantiate(floorPrefab, new Vector3(floorX, floorPrefab.transform.position.y, floorPrefab.transform.position.z), Quaternion.identity);
            floorX -= floorSpacing;
        } while (floorX > LeftWallX);
        floorX = StatueStartX;
        do {
            Instantiate(floorJointPrefab, new Vector3(floorX + floorSpacing / 2, floorJointPrefab.transform.position.y, floorJointPrefab.transform.position.z), Quaternion.identity);
            floorX -= floorSpacing;
        } while (floorX > LeftWallX);

        var floorBG = Instantiate(_darknessPrefab, new Vector3(LeftWallX, 25, 0), Quaternion.identity);
        floorBG.name = "Floor Background";
        var floorTrans = floorBG.transform;
        floorTrans.SetScaleX(16 - LeftWallX);
        floorTrans.SetScaleY(10);
        floorTrans.SetPosition2D(LeftWallX, 25);

        // Re-add platform darkness
        var platBG = Instantiate(_darknessPrefab, new Vector3(22, 25, 0), Quaternion.identity);
        platBG.name = "Platform Background";
        var platTrans = platBG.transform;
        platTrans.SetScaleX(20);
        platTrans.SetScaleY(10);
        platTrans.SetPosition2D(22, 25);

        // Re-add roof darkness
        var roofRBG = Instantiate(_darknessPrefab, new Vector3(6.75f, 42, 0), Quaternion.identity);
        roofRBG.name = "Roof R Background";
        var roofRTrans = roofRBG.transform;
        roofRTrans.SetRotationZ(14.36f);
        roofRTrans.SetScaleX(20);
        roofRTrans.SetScaleY(10);
    }

    private static void ModifyMinBoundX(ILContext il) {
        var cursor = new ILCursor(il).Goto(0);
        while (true) {
            if (cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcR4(14.6f))) {
                cursor.Remove();
                cursor.Emit(OpCodes.Ldc_R4, LeftWallX + 14);
                cursor.GotoNext();
            } else {
                break;
            }
        }
    }

    private static void AddILHooks() {
        IL.CameraController.IsAtSceneBounds += ModifyMinBoundX;
        IL.CameraController.IsTouchingSides += ModifyMinBoundX;
        IL.CameraController.IsAtHorizontalSceneBounds += ModifyMinBoundX;
        IL.CameraController.KeepWithinSceneBounds_Vector2 += ModifyMinBoundX;
        IL.CameraController.KeepWithinSceneBounds_Vector3 += ModifyMinBoundX;
        IL.CameraController.KeepWithinLockBounds += ModifyMinBoundX;
        IL.CameraController.LateUpdate += ModifyMinBoundX;
        IL.CameraController.LockToArea += ModifyMinBoundX;
        IL.CameraLockArea.ValidateBounds += ModifyMinBoundX;
    }

    internal static void ClearILHooks() {
        IL.CameraController.IsAtSceneBounds -= ModifyMinBoundX;
        IL.CameraController.IsTouchingSides -= ModifyMinBoundX;
        IL.CameraController.IsAtHorizontalSceneBounds -= ModifyMinBoundX;
        IL.CameraController.KeepWithinSceneBounds_Vector2 -= ModifyMinBoundX;
        IL.CameraController.KeepWithinSceneBounds_Vector3 -= ModifyMinBoundX;
        IL.CameraController.KeepWithinLockBounds -= ModifyMinBoundX;
        IL.CameraController.LateUpdate -= ModifyMinBoundX;
        IL.CameraController.LockToArea -= ModifyMinBoundX;
        IL.CameraLockArea.ValidateBounds -= ModifyMinBoundX;
    }

    private void ForceStopAnimPlayback(Animator anim) {
        anim.StopPlayback();
        anim.enabled = false;
    }

    private const string StatueNamePrefix = "GG_Statue_";
    private const string PlinthName = "GG_statues_0001_plinth_02";
    private const string PlinthPath = $"/Base/{PlinthName}";
    private void CreateStatues() {
        var statuePrefab = GameObject.Find($"{StatueNamePrefix}Mage_Knight");

        var smallPlinthSprite = GameObject.Find($"{StatueNamePrefix}Grimm{PlinthPath}").GetComponent<SpriteRenderer>().sprite;
        var midPlinthSprite = GameObject.Find($"{StatueNamePrefix}Defender{PlinthPath}").GetComponent<SpriteRenderer>().sprite;
        var longPlinthSprite = GameObject.Find($"{StatueNamePrefix}Nosk{PlinthPath}").GetComponent<SpriteRenderer>().sprite;

        var bossSummaryBoard = FindObjectOfType<BossSummaryBoard>();

        for (var modIdx = 0; modIdx < BossStatueFramework.ModCount; modIdx++) {
            var bossMod = BossStatueFramework.BossStatueMods[modIdx];
            var statue = Instantiate(statuePrefab, new Vector2(StatueStartX - StatueSpacingX * modIdx, statuePrefab.transform.GetPositionY()), Quaternion.identity);
            var bossStatue = statue.GetComponent<BossStatue>();

            bossSummaryBoard.bossStatues.Add(bossStatue);

            var statueBase = statue.transform.Find("Base");
            var plinth = statueBase.Find(PlinthName);
            var plinthRenderer = plinth.GetComponent<SpriteRenderer>();
            switch (bossMod.PlinthType) {
                case PlinthType.Small:
                    plinthRenderer.sprite = smallPlinthSprite;
                    break;
                case PlinthType.Medium:
                    plinthRenderer.sprite = midPlinthSprite;
                    break;
                case PlinthType.Long:
                    plinthRenderer.sprite = longPlinthSprite;
                    break;
            }
            plinth.localScale = new Vector3(1, plinth.localScale.y, plinth.localScale.z);
            var plinthBounds = plinthRenderer.bounds;

            var bossScene = ScriptableObject.CreateInstance<BossScene>();
            bossScene.sceneName = bossMod.SceneName;
            bossStatue.bossScene = bossScene;
            bossStatue.statueStatePD = bossMod.PlayerData;
            var statueState = bossStatue.StatueState;

            var bossDetails = new BossUIDetails {
                nameKey = bossMod.NameKey,
                nameSheet = bossMod.NameKey,
                descriptionKey = bossMod.DescriptionKey,
                descriptionSheet = bossMod.DescriptionKey,
            };
            bossStatue.bossDetails = bossDetails;

            var statueDisplay = bossStatue.statueDisplay;
            var statueSpriteHeight = bossMod.Sprite.bounds.size.y * bossMod.SpriteScale;
            
            statueDisplay.transform.localPosition = new Vector3(statueDisplay.transform.localPosition.x, statueSpriteHeight / 4, statueDisplay.transform.localPosition.z);
            statueDisplay.SetActive(true);
            var statueSprite = statueDisplay.transform.Find("GG_statues_0006_5");
            var displayRenderer = statueSprite.GetComponent<SpriteRenderer>();
            displayRenderer.enabled = true;
            displayRenderer.sprite = bossMod.Sprite;
            displayRenderer.transform.localScale = Vector3.one * bossMod.SpriteScale;

            switch (bossMod.AltType) {
                case AltSwitchType.Lever:
                    var altLever = statue.transform.Find("alt_lever");
                    altLever.SetPositionX(plinthBounds.max.x - 1);
                    var lever = altLever.GetComponentInChildren<BossStatueLever>(true);
                    lever.SetOwner(bossStatue);
                    altLever.gameObject.SetActive(true);
                    break;
                case AltSwitchType.Dream:
                    var dreamSwitch = statue.transform.Find("dream_version_switch");
                    dreamSwitch.SetPositionX(plinthBounds.max.x - 2);
                    dreamSwitch.Find("Statue Pt").SetPositionX(statue.transform.position.x);
                    dreamSwitch.Find("lit_pieces/Base Glow").SetPositionX(statue.transform.position.x);
                    dreamSwitch.gameObject.SetActive(true);
                    break;
            }

            if (bossMod.AltType != AltSwitchType.None) {
                var dreamBossScene = ScriptableObject.CreateInstance<BossScene>();
                dreamBossScene.sceneName = bossMod.AltSceneName;
                bossStatue.dreamBossScene = dreamBossScene;
                bossStatue.dreamStatueStatePD = bossMod.AltPlayerData;

                var altBossDetails = new BossUIDetails {
                    nameKey = bossMod.AltNameKey,
                    nameSheet = bossMod.AltNameKey,
                    descriptionKey = bossMod.AltDescriptionKey,
                    descriptionSheet = bossMod.AltDescriptionKey,
                };
                bossStatue.dreamBossDetails = altBossDetails;

                var altDisplay = bossStatue.statueDisplayAlt;
                var altStatueSpriteHeight = bossMod.AltSprite.bounds.size.y * bossMod.AltSpriteScale;

                altDisplay.transform.localPosition = new Vector3(altDisplay.transform.localPosition.x, altStatueSpriteHeight / 4, altDisplay.transform.localPosition.z);
                altDisplay.SetActive(true);
                var altSpriteObj = new GameObject("Statue");
                altSpriteObj.transform.SetParent(altDisplay.transform);
                altSpriteObj.transform.localPosition = statueSprite.transform.localPosition;
                var altDisplayRenderer = altSpriteObj.AddComponent<SpriteRenderer>();
                altDisplayRenderer.enabled = true;
                altDisplayRenderer.sprite = bossMod.AltSprite;
                altDisplayRenderer.transform.localScale = Vector3.one * bossMod.AltSpriteScale;

                On.BossStatue.SetDreamVersion += (orig, self, value, useAltStatue, doAnim) => {
                    if (self == bossStatue && doAnim) {
                        var current = value ? self.statueDisplay : self.statueDisplayAlt;
                        var next = value ? self.statueDisplayAlt : self.statueDisplay;
                        var currentAnim = current.GetComponent<Animator>();
                        var nextAnim = next.GetComponent<Animator>();
                        var nextStatueHeight = value ? altStatueSpriteHeight : statueSpriteHeight;

                        var forceStopAnimPlaybackName = nameof(ForceStopAnimPlayback);
                        var easeType = iTween.EaseType.easeInOutBack;

                        iTween.Stop(current);
                        var downHash = new Hashtable();
                        downHash.Add("y", next.transform.parent.position.y - 5);
                        downHash.Add("time", 0.5f);
                        downHash.Add("delay", self.swapWaitTime + self.shakeTime);
                        downHash.Add("easetype", easeType);
                        downHash.Add("onstart", forceStopAnimPlaybackName);
                        downHash.Add("onstarttarget", gameObject);
                        downHash.Add("onstartparams", currentAnim);
                        downHash.Add("onupdate", forceStopAnimPlaybackName);
                        downHash.Add("onupdatetarget", gameObject);
                        downHash.Add("onupdateparams", currentAnim);
                        iTween.MoveTo(current, downHash);

                        iTween.Stop(next);
                        var upHash = new Hashtable();
                        upHash.Add("y", nextStatueHeight / 4 + next.transform.parent.position.y);
                        upHash.Add("time", 0.5f);
                        upHash.Add("easetype", easeType);
                        upHash.Add("onstart", forceStopAnimPlaybackName);
                        upHash.Add("onstarttarget", gameObject);
                        upHash.Add("onstartparams", nextAnim);
                        upHash.Add("onupdate", forceStopAnimPlaybackName);
                        upHash.Add("onupdatetarget", gameObject);
                        upHash.Add("onupdateparams", nextAnim);
                        iTween.MoveTo(next, upHash);
                    }

                    orig(self, value, useAltStatue, doAnim);
                };
            }

            bossStatue.SetPlaquesVisible(statueState.isUnlocked && statueState.hasBeenSeen);
            var inspect = statue.transform.Find("Inspect").gameObject;
            var marker = inspect.transform.Find("Prompt Marker");
            marker.localPosition = Vector3.up * statueSpriteHeight;
            inspect.SetActive(true);

            statue.transform.Find("Spotlight").gameObject.SetActive(true);

            statue.SetActive(true);
        }

        bossSummaryBoard.Invoke("Start", 0);
        var grid = bossSummaryBoard.bossSummaryUI.GetComponentInChildren<GridLayoutGroup>(true);
        grid.cellSize = new Vector2(320, grid.cellSize.y);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
    }

    private void ModifyBackground() {
        foreach (var wall in FindObjectsOfType<GameObject>().Where(go => go.name.Contains("GG_gold_wall") && go.transform.position.x > 10 && go.transform.position.x < 20)) {
            var wallClone = Instantiate(wall, wall.transform.position, Quaternion.identity);
            var wallTrans = wallClone.transform;
            wallTrans.SetScaleX(-1);
            wallTrans.position += Vector3.left * 2 * (wall.transform.position.x - 10.7f);
        }
    }

    private const float ArchStartX = 6.75f;
    private const float ArchY = 42.2792f;
    private const float ArchZ = -0.1511f;
    private const float ArchJointLowerToArchRDist = 1.75f;
    private const float ArchRToArchJointHigherDist = 2.5f;
    private const float ArchJointHigherToArchLDist = 2.85f;
    private const float ArchJointHigherY = 42.5f;
    private const float ArchJointLowerY = 41.5f;
    private const float ArchJointZ = -0.1781f;
    private const float ArchEndOffsetX = 2.2f;
    private void ModifyRoof() {
        var roofDarkness = Instantiate(_darknessPrefab, new Vector2(7, 43), Quaternion.identity);
        roofDarkness.transform.localScale = new Vector3(-100, 20, 1);

        var archPrefab = GameObject.Find("GG_scenery_0001_1");
        var archJointPrefab = GameObject.Find("GG_scenery_0000_2");

        var posX = ArchStartX;
        do {    
            var archJointLower = Instantiate(archJointPrefab, new Vector3(posX, ArchJointLowerY, ArchJointZ), Quaternion.identity);
            posX -= ArchJointLowerToArchRDist;
            var archR = Instantiate(archPrefab, new Vector3(posX, ArchY, ArchZ), Quaternion.identity);
            archR.transform.SetScaleX(-1);
            posX -= ArchRToArchJointHigherDist;
            var archJointHigher = Instantiate(archJointPrefab, new Vector3(posX, ArchJointHigherY, ArchJointZ), Quaternion.identity);
            posX -= ArchJointHigherToArchLDist;
            var archL = Instantiate(archPrefab, new Vector3(posX, ArchY, ArchZ), Quaternion.identity);
            posX -= ArchEndOffsetX;
        } while (posX > LeftWallX);
        
        static IEnumerator RaiseRoofRoutine() {
            yield return new WaitUntil(() => GameObject.Find("Roof Collider (3)") != null);

            var roofCol = GameObject.Find("Roof Collider (3)").GetComponent<PolygonCollider2D>();
            var points = roofCol.points;
            points[21].y = 10;
            points[22].y = 10;
            roofCol.points = points;
        }

        GameManager.instance.StartCoroutine(RaiseRoofRoutine());
    }
}

