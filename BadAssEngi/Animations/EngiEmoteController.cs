using System.Collections.Generic;
using BadAssEngi.Assets;
using BadAssEngi.Networking;
using EntityStates;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace BadAssEngi.Animations
{
    internal static class EngiEmoteController
    {
        internal static bool IsEmoting;
        internal static GameObject EmoteWindow;
        internal static GameObject EmoteButton;

        internal static readonly Dictionary<NetworkInstanceId, GameObject> EngiNetIdToTempGO = new Dictionary<NetworkInstanceId, GameObject>();
        internal static readonly Dictionary<NetworkInstanceId, uint> EngiNetIdToSoundEvent = new Dictionary<NetworkInstanceId, uint>();
        internal static int NumberOfEmotePlaying;

        internal static void PlayCustomEngiAnim(int index)
        {
            if (index >= BaeAssets.EngiAnimations.Count)
            {
                Debug.LogWarning(
                    "Index out of range for the Custom Engi Animation, if it was through the console command, make sure you use one of the index from the command bae_list_animation");
                return;
            }

            var body = CameraRigController.readOnlyInstancesList[0].viewer.GetCurrentBody();

            if (body.characterMotor.isGrounded)
            {
                var stateMachine = body.gameObject.GetComponent<EntityStateMachine>();
                if (stateMachine.CanInterruptState(InterruptPriority.Skill))
                {
                    if (IsEmoting)
                    {
                        new StopAnimMsg {EngiNetId = body.GetComponent<NetworkIdentity>().netId}.Send(NetworkDestination.Clients);
                    }

                    new AnimMsg
                    {
                        AnimId = index,
                        EngiNetId = body.GetComponent<NetworkIdentity>().netId
                    }.Send(NetworkDestination.Clients);

                    IsEmoting = true;
                }
            }
        }

        internal static void OnClickEmoteButton()
        {
            RoR2.Util.PlaySound("Play_UI_menuClick", RoR2Application.instance.gameObject);

            if (EmoteWindow)
            {
                if (EmoteWindow.activeSelf)
                {
                    Object.Destroy(EmoteWindow);
                    Object.Destroy(EmoteButton);
                }
                else
                {
                    EmoteWindow.SetActive(true);
                    EmoteWindow.transform.GetChild(0).gameObject.SetActive(true);
                }  
            }
        }

        private const string BaeAnimationCmdUsage = "Enter the animation index number as argument, use bae_list_animation for all the animations indexes. Exemple Usage : bae_animation 4";
        [ConCommand(commandName = "bae_animation", flags = ConVarFlags.None, helpText = BaeAnimationCmdUsage)]
        private static void CCPlayEngiAnimation(ConCommandArgs args)
        {
            if (args.Count == 1)
            {
                if (int.TryParse(args[0], out var index))
                {
                    if (Run.instance && EmoteButton && EmoteButton)
                    {
                        PlayCustomEngiAnim(index);
                    }
                    else
                    {
                        Debug.Log("Be in a run, while playing Engi to use that command.");
                    }
                }
                else
                {
                    Debug.Log("Couldn't parse correctly the animation index. " + BaeAnimationCmdUsage);
                }
            }
            else
            {
                Debug.Log("Wrong number of arguments. " + BaeAnimationCmdUsage);
            }
        }
        
        [ConCommand(commandName = "bae_list_animation", flags = ConVarFlags.None, helpText = "List all possible animations to use for Bad Ass Engineer")]
        private static void CCEngiAnimationList(ConCommandArgs args)
        {
            for (var i = 0; i < BaeAssets.EngiAnimations.Count; i++)
            {
                Debug.Log($"Index {i} : {BaeAssets.EngiAnimations[i]}");
            }
        }
    }
}
