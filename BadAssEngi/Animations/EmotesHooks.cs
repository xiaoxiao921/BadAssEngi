using System;
using BadAssEngi.Assets;
using BadAssEngi.AssetsScripts;
using BadAssEngi.Util;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using Rewired;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable PossibleNullReferenceException

namespace BadAssEngi.Animations
{
    internal static class EmotesHooks
    {
        internal static void Init()
        {
            On.RoR2.CameraRigController.Update += AddUIOnCameraRig;

            On.RoR2.PlayerCharacterMasterController.CanSendBodyInput += FixBodyInputLossWhenHoveringEmoteButton;
            IL.RoR2.CameraRigController.Update += FixCameraWorkLossWhenHoveringEmoteButton;

            SceneManager.sceneLoaded += ShouldDisableUIAndResetEmotes;

            IL.RoR2.MusicController.LateUpdate += EmotesDisableGameMusic;
        }

        private static void AddUIOnCameraRig(On.RoR2.CameraRigController.orig_Update orig, CameraRigController self)
        {
            orig(self);
            if (!self || !self.hud)
            {
                Object.Destroy(EngiEmoteController.EmoteWindow);
                Object.Destroy(EngiEmoteController.EmoteButton);
                Object.Destroy(BaeAssets.PauseMenuPrefab);
                Object.Destroy(BaeAssets.MainMenuButtonPrefab);
                return;
            }
                
            if (EngiEmoteController.EmoteButton && EngiEmoteController.EmoteWindow)
                return;

            

            var isLocal = false;
            foreach (var localUser in LocalUserManager.readOnlyLocalUsersList)
            {
                if (self.hud.localUserViewer != null && 
                    localUser == self.hud.localUserViewer)
                    isLocal = true;
            }
            if (!isLocal)
                return;

            if (self.hud.localUserViewer == null || 
                !self.hud.localUserViewer.cachedBody || 
                self.hud.localUserViewer.cachedBody.bodyIndex != BadAssEngi.EngiBodyIndex)
            {
                if (EngiEmoteController.EmoteWindow)
                    Object.Destroy(EngiEmoteController.EmoteWindow);

                if (EngiEmoteController.EmoteButton)
                    Object.Destroy(EngiEmoteController.EmoteButton);

                return;
            }

            var parent =
                self.hud.transform.Find(
                    "MainContainer/MainUIArea/BottomRightCluster/Scaler");

            if (!parent)
                return;

            var inventoryClusterTransform = parent.transform.Find("InventoryCluster");
            if (!inventoryClusterTransform)
                return;

            var inventoryCluster = inventoryClusterTransform.gameObject;

            var canvas = RoR2Application.instance.mainCanvas;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.scaleFactor = 1.0f;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;


            EngiEmoteController.EmoteWindow = Object.Instantiate(BaeAssets.PrefabEmoteWindow);

            EngiEmoteController.EmoteWindow.transform.SetParent(canvas.transform);
            var rect = EngiEmoteController.EmoteWindow.transform as RectTransform;
            rect.SetSpreadAnchor();
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.one;
            rect.localScale = Vector3.one;

            var closeButton = EngiEmoteController.EmoteWindow.transform.GetChild(0).GetChild(6).gameObject.GetComponent<Button>();
            closeButton.interactable = true;
            closeButton.onClick = new Button.ButtonClickedEvent();
            closeButton.onClick.AddListener(() =>
            {
                Object.Destroy(EngiEmoteController.EmoteWindow);
            });

            var firstButton = EngiEmoteController.EmoteWindow.transform.Find("EmoteWindow/Form/ScrollView/Viewport/Content/Emote (1)");
            var buttonParent = firstButton.parent;

            var firstButtonComponent = firstButton.GetComponent<Button>();
            firstButtonComponent.onClick = new Button.ButtonClickedEvent();
            firstButtonComponent.onClick.AddListener(() => EngiEmoteController.PlayCustomEngiAnim(0));

            var textButton = firstButton.GetComponentInChildren<Text>();
            textButton.text = BaeAssets.EngiAnimations[0];

            for (var i = 1; i < BaeAssets.EngiAnimations.Count; i++)
            {
                var emoteName = BaeAssets.EngiAnimations[i];

                var newButton = Object.Instantiate(firstButton.gameObject);
                newButton.transform.SetParent(buttonParent, false);

                newButton.name = $"Emote ({emoteName})";

                var buttonComponent = newButton.GetComponent<Button>();

                buttonComponent.onClick = new Button.ButtonClickedEvent();
                var currentIndex = i;
                buttonComponent.onClick.AddListener(() => EngiEmoteController.PlayCustomEngiAnim(currentIndex));

                textButton = newButton.GetComponentInChildren<Text>();
                textButton.text = emoteName;
            }

            if (!EngiEmoteController.EmoteButton)
            {
                if (BaeAssets.MainMenuButtonPrefab == null || !BaeAssets.MainMenuButtonPrefab)
                {
                    BaeAssets.InitMainMenuButtonPrefab();
                    Object.Destroy(BaeAssets.PauseMenuPrefab);
                }

                EngiEmoteController.EmoteButton = Object.Instantiate(BaeAssets.MainMenuButtonPrefab, inventoryCluster.transform.parent);
                EngiEmoteController.EmoteButton.name = "DirectorUIMenuButton";
                EngiEmoteController.EmoteButton.transform.localPosition = parent.transform.Find("Skill1Root").localPosition -
                                                                          new Vector3(Configuration.EmoteButtonUIPosX.Value,
                                                                              Configuration.EmoteButtonUIPosY.Value);

                var rectTransform = EngiEmoteController.EmoteButton.transform as RectTransform;
                rectTransform.sizeDelta = new Vector2(160f, 62f);
            }

            var hgButton = EngiEmoteController.EmoteButton.GetComponent<HGButton>();

            BadAssEngi.Instance.StartCoroutine(CoroutineUtil.DelayedMethod(1, () =>
            {
                hgButton.onClick = new Button.ButtonClickedEvent();
                hgButton.onClick.AddListener(EngiEmoteController.OnClickEmoteButton);
            }));
        }

        private static void ShouldDisableUIAndResetEmotes(Scene loadedScene, LoadSceneMode loadSceneMode)
        {
            if (EngiEmoteController.EmoteWindow)
            {
                Object.Destroy(EngiEmoteController.EmoteWindow);
            }
            if (EngiEmoteController.EmoteButton)
            {
                Object.Destroy(EngiEmoteController.EmoteButton);
            }

            EngiEmoteController.EngiNetIdToTempGO.Clear();

            foreach (var pair in EngiEmoteController.EngiNetIdToSoundEvent)
            {
                AkSoundEngine.StopPlayingID(pair.Value);
            }

            EngiEmoteController.EngiNetIdToSoundEvent.Clear();
            EngiEmoteController.NumberOfEmotePlaying = 0;
            EngiEmoteController.IsEmoting = false;
        }

        private static bool FixBodyInputLossWhenHoveringEmoteButton(
            On.RoR2.PlayerCharacterMasterController.orig_CanSendBodyInput orig, NetworkUser networkUser,
            out LocalUser localUser, out Player inputPlayer, out CameraRigController cameraRigController)
        {
            var res = orig(networkUser, out localUser, out inputPlayer, out cameraRigController);

            if (localUser?.eventSystem?.currentSelectedGameObject &&
                localUser?.eventSystem?.currentSelectedGameObject == EngiEmoteController.EmoteButton)
            {
                return inputPlayer != null && cameraRigController && cameraRigController.isControlAllowed;
            }

            return res;
        }

        private static void FixCameraWorkLossWhenHoveringEmoteButton(ILContext il)
        {
            var cursor = new ILCursor(il);
            var getViewerMethod = typeof(CameraRigController).GetMethodCached("get_viewer");
            var localUserField = typeof(NetworkUser).GetFieldCached("localUser");

            cursor.GotoNext(MoveType.After,
                i => i.MatchCallOrCallvirt(getViewerMethod),
                i => i.MatchLdfld(localUserField),
                i => i.MatchCallOrCallvirt<LocalUser>("get_isUIFocused")
            );
            cursor.Index--;
            cursor.Emit(OpCodes.Dup);
            cursor.Index++;
            cursor.EmitDelegate<Func<LocalUser, bool, bool>>((localUser, b) =>
                localUser.eventSystem.currentSelectedGameObject != EngiEmoteController.EmoteButton && b);
        }

        private static void EmotesDisableGameMusic(ILContext il)
        {
            var cursor = new ILCursor(il);

            cursor.GotoNext(i => i.MatchStloc(out _));
            cursor.EmitDelegate<Func<bool, bool>>(b =>
            {
                if (b)
                    return true;

                return EngiEmoteController.NumberOfEmotePlaying != 0;
            });
        }
    }
}
