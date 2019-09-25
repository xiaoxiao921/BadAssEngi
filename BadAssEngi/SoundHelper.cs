using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AssetPlus;

namespace BadAssEngi
{
    internal static class SoundHelper
    {
        private const string BankName = "BaeBank.sound";

        public const string Quack = "Quack";
        public const string TurretAlive = "Turret_Alive_Play_69";
        public const string TurretRTPCAttackSpeed = "Turret_Attack_Speed";
        public const string RailGunTurretShot = "Railgun_Turret_Shot_Play_69";
        public const string RailGunTurretTargeting = "Railgun_Turret_Targeting_Play_69";
        public const string MiniGunTurretShot = "Minigun_Turret_Shot_Play_69";
        public const string MainMenuSound = "Main_Menu_Play_69";
        public const string SeekerGrenadeFiring = "Seekers_Firing_Play_69";
        public const string SeekerGrenadeExplosion = "Seekers_Explosion_Play_69";
        public const string ClusterMineExplosion = "Cluster_Explosion_Play_69";
        public const string SatchelMineExplosion = "Satchel_Explosion_Play_69";

        public static void AddSoundBank()
        {
            var soundbank = LoadEmbeddedResource(BankName);
            if (soundbank != null)
            {
                SoundBanks.Add(soundbank);
            }
            else
            {
                UnityEngine.Debug.LogError("SoundBank Fetching Failed");
            }
        }

        private static byte[] LoadEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            resourceName = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(resourceName));

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new BinaryReader(stream ?? throw new InvalidOperationException()))
            {
                return reader.ReadBytes(Convert.ToInt32(stream.Length.ToString()));
            }

        }
    }
}
