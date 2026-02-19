using GHPC.Equipment;
using GHPC.Weapons;
using MelonLoader;
using UnityEngine;

namespace SuperM60
{
    public class Armour
    {
        public static ArmorType armor_castarmorsteel_vnl;

        public static ArmorType armor_composite_turret;
        public static ArmorCodexScriptable armor_codex_composite_turret;

        public static ArmorType armor_composite_hull;
        public static ArmorCodexScriptable armor_codex_composite_hull;

        public static void Init()
        {
            if (armor_composite_turret != null) return;

            foreach (ArmorCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(ArmorCodexScriptable)))
            {
                if (s.ArmorType.Name == "cast armor steel") { armor_castarmorsteel_vnl = s.ArmorType; break; }
            }

            armor_composite_turret = new ArmorType();
            Util.ShallowCopy(armor_composite_turret, armor_castarmorsteel_vnl);
            armor_composite_turret.RhaeMultiplierCe = 1.8f;
            armor_composite_turret.RhaeMultiplierKe = 1.6f;
            armor_composite_turret.Name = "m60 composite turret";

            armor_codex_composite_turret = ScriptableObject.CreateInstance<ArmorCodexScriptable>();
            armor_codex_composite_turret.name = "m60 composite turret codex";
            armor_codex_composite_turret.ArmorType = armor_composite_turret;

            armor_composite_hull = new ArmorType();
            Util.ShallowCopy(armor_composite_hull, armor_castarmorsteel_vnl);
            armor_composite_hull.RhaeMultiplierCe = 1.8f;
            armor_composite_hull.RhaeMultiplierKe = 1.6f;
            armor_composite_hull.Name = "m60 composite hull";

            armor_codex_composite_hull = ScriptableObject.CreateInstance<ArmorCodexScriptable>();
            armor_codex_composite_hull.name = "m60 composite hull codex";
            armor_codex_composite_hull.ArmorType = armor_composite_hull;
        }
    }
}
