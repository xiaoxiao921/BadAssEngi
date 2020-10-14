using BadAssEngi.Skills.Secondary.ClusterMine;
using BadAssEngi.Skills.Secondary.SatchelMine.MineStates.ArmingStateMachine;
using BadAssEngi.Skills.Secondary.SatchelMine.MineStates.MainStateMachine;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine.Networking;

namespace BadAssEngi.Networking
{
    public struct DetonateMsg : INetMessage
    {
        internal NetworkInstanceId SenderUserNetId;

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(SenderUserNetId);
        }

        public void Deserialize(NetworkReader reader)
        {
            SenderUserNetId = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            // Executed by server
            foreach (var networkUser in NetworkUser.readOnlyInstancesList)
            {
                if (SenderUserNetId != networkUser.netId) continue;
                var deployableInfos =
                    networkUser.master.deployablesList;

                if (deployableInfos != null && deployableInfos.Count >= 1)
                {
                    foreach (var deployableInfo in deployableInfos)
                    {
                        if (deployableInfo.slot == DeployableSlot.EngiMine &&
                            !deployableInfo.deployable.GetComponent<RecursiveMine>())
                        {
                            EntityStateMachine
                                .FindByCustomName(deployableInfo.deployable.gameObject, "Arming")
                                .SetNextState(new MineArmingFullSatchel());
                            EntityStateMachine
                                .FindByCustomName(deployableInfo.deployable.gameObject, "Main")
                                .SetNextState(new DetonateSatchel());
                        }
                    }
                }
            }
        }
    }
}