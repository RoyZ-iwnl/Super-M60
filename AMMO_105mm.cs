using System.Collections.Generic;
using System.Linq;
using GHPC.Effects;
using GHPC.Weapons;
using GHPC.Weaponry;
using MelonLoader;
using UnityEngine;

namespace SuperM60
{
    public class AMMO_105mm : Module
    {
        public static AmmoClipCodexScriptable clip_codex_m900a1;
        public static AmmoType.AmmoClip clip_m900a1;
        public static AmmoType ammo_m900a1;
        public static AmmoCodexScriptable codex_m900a1;

        public static AmmoClipCodexScriptable clip_codex_m900a2;
        public static AmmoType.AmmoClip clip_m900a2;
        public static AmmoType ammo_m900a2;
        public static AmmoCodexScriptable codex_m900a2;

        public static AmmoClipCodexScriptable clip_codex_m393a3;
        public static AmmoType.AmmoClip clip_m393a3;
        public static AmmoType ammo_m393a3;
        public static AmmoCodexScriptable codex_m393a3;

        public static AmmoClipCodexScriptable clip_codex_m456a3;
        public static AmmoType.AmmoClip clip_m456a3;
        public static AmmoType ammo_m456a3;
        public static AmmoCodexScriptable codex_m456a3;

        public static AmmoClipCodexScriptable clip_codex_m8api;
        public static AmmoType.AmmoClip clip_m8api;
        public static AmmoType ammo_m8api;
        public static AmmoCodexScriptable codex_m8api;

        public static AmmoClipCodexScriptable clip_codex_m2apt;
        public static AmmoType.AmmoClip clip_m2apt;
        public static AmmoType ammo_m2apt;
        public static AmmoCodexScriptable codex_m2apt;

        public static MelonPreferences_Entry<int> hepFragments;
        private static bool assets_loaded = false;

        public static void Config(MelonPreferences_Category cfg)
        {
            hepFragments = cfg.CreateEntry<int>("HEP Fragments", 600);
            hepFragments.Description = "How many fragments are generated when the below round explodes. NOTE: Higher number, means higher performance hit. Be careful in using higher number.";
        }

        public override void LoadStaticAssets()
        {
            string[] target_vehicles = new string[] {
                "M60A1RISEP", 
                "M60A1RISEP77", 
                "M60A1", 
                "M60A1AOS", 
                "M60A3"
            };

            foreach (string name in target_vehicles)
            {
                AssetUtil.LoadVanillaVehicle(name);
            }
        }

        public static void LoadAssets()
        {
            if (assets_loaded) return;
            assets_loaded = true;

            AmmoType ammo_m833 = null, ammo_m456 = null, ammo_m8vnl = null;

            foreach (AmmoCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoCodexScriptable)))
            {
                if (s.AmmoType.Name == "M833 APFSDS-T") ammo_m833 = s.AmmoType;
                if (s.AmmoType.Name == "M456 HEAT-FS-T") ammo_m456 = s.AmmoType;
                if (s.AmmoType.Name == "M8 API") ammo_m8vnl = s.AmmoType;
            }

            var era_optimizations_m456a3 = new List<AmmoType.ArmorOptimization>();
            var era_optimizations_m900a2 = new List<AmmoType.ArmorOptimization>();

            string[] era_names = { "kontakt-1 armour", "kontakt-5 armour", "ARAT-1 Armor Codex", "BRAT-M3 Armor Codex", "BRAT-M5 Armor Codex" };

            foreach (ArmorCodexScriptable s in Resources.FindObjectsOfTypeAll<ArmorCodexScriptable>())
            {
                if (era_names.Contains(s.name))
                {
                    AmmoType.ArmorOptimization opt_m456a3 = new AmmoType.ArmorOptimization();
                    opt_m456a3.Armor = s; opt_m456a3.RhaRatio = 0.25f;
                    era_optimizations_m456a3.Add(opt_m456a3);

                    AmmoType.ArmorOptimization opt_m900a2 = new AmmoType.ArmorOptimization();
                    opt_m900a2.Armor = s; opt_m900a2.RhaRatio = 0.35f;
                    era_optimizations_m900a2.Add(opt_m900a2);
                }
                if (era_optimizations_m456a3.Count == era_names.Length) break;
            }

            // M900A1
            ammo_m900a1 = new AmmoType();
            Util.ShallowCopy(ammo_m900a1, ammo_m833);
            ammo_m900a1.CachedIndex = -1;
            ammo_m900a1.Name = "M900A1 APFSDS-T";
            ammo_m900a1.RhaPenetration = 550f;
            ammo_m900a1.MuzzleVelocity = 1505f;
            ammo_m900a1.Mass = 4.2f;
            ammo_m900a1.SpallMultiplier = 1.35f;
            ammo_m900a1.MinSpallRha = 3f;
            ammo_m900a1.MaxSpallRha = 15f;
            ammo_m900a1.Coeff = 0.07f;
            ammo_m900a1.CertainRicochetAngle = 9;

            codex_m900a1 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            codex_m900a1.AmmoType = ammo_m900a1; codex_m900a1.name = "ammo_m900a1";

            clip_m900a1 = new AmmoType.AmmoClip();
            clip_m900a1.Capacity = 1; clip_m900a1.Name = "M900A1 APFSDS-T";
            clip_m900a1.MinimalPattern = new AmmoCodexScriptable[] { codex_m900a1 };

            clip_codex_m900a1 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_m900a1.name = "clip_m900a1"; clip_codex_m900a1.ClipType = clip_m900a1;

            GameObject vis_m900a1 = GameObject.Instantiate(ammo_m833.VisualModel);
            vis_m900a1.name = "M900A1 visual";
            ammo_m900a1.VisualModel = vis_m900a1;
            vis_m900a1.GetComponent<AmmoStoredVisual>().AmmoType = ammo_m900a1;
            vis_m900a1.GetComponent<AmmoStoredVisual>().AmmoScriptable = codex_m900a1;

            // M900A2
            ammo_m900a2 = new AmmoType();
            Util.ShallowCopy(ammo_m900a2, ammo_m833);
            ammo_m900a2.CachedIndex = -1;
            ammo_m900a2.Name = "M900A2 APFSDS-T";
            ammo_m900a2.RhaPenetration = 580f;
            ammo_m900a2.MuzzleVelocity = 1600f;
            ammo_m900a2.Mass = 4.5f;
            ammo_m900a2.SpallMultiplier = 1.5f;
            ammo_m900a2.MinSpallRha = 3f;
            ammo_m900a2.MaxSpallRha = 24f;
            ammo_m900a2.CertainRicochetAngle = 9;
            ammo_m900a2.ArmorOptimizations = era_optimizations_m900a2.ToArray();

            codex_m900a2 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            codex_m900a2.AmmoType = ammo_m900a2; codex_m900a2.name = "ammo_m900a2";

            clip_m900a2 = new AmmoType.AmmoClip();
            clip_m900a2.Capacity = 1; clip_m900a2.Name = "M900A2 APFSDS-T";
            clip_m900a2.MinimalPattern = new AmmoCodexScriptable[] { codex_m900a2 };

            clip_codex_m900a2 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_m900a2.name = "clip_m900a2"; clip_codex_m900a2.ClipType = clip_m900a2;

            GameObject vis_m900a2 = GameObject.Instantiate(ammo_m833.VisualModel);
            vis_m900a2.name = "M900A2 visual";
            ammo_m900a2.VisualModel = vis_m900a2;
            vis_m900a2.GetComponent<AmmoStoredVisual>().AmmoType = ammo_m900a2;
            vis_m900a2.GetComponent<AmmoStoredVisual>().AmmoScriptable = codex_m900a2;

            // M456A3
            ammo_m456a3 = new AmmoType();
            Util.ShallowCopy(ammo_m456a3, ammo_m456);
            ammo_m456a3.CachedIndex = -1;
            ammo_m456a3.Name = "M456A3 HEAT-FS-T";
            ammo_m456a3.RhaPenetration = 450f;
            ammo_m456a3.MuzzleVelocity = 1174f;
            ammo_m456a3.Mass = 10.2f;
            ammo_m456a3.TntEquivalentKg = 3.29f;
            ammo_m456a3.CertainRicochetAngle = 3.0f;
            ammo_m456a3.MinSpallRha = 3f;
            ammo_m456a3.MaxSpallRha = 21f;
            ammo_m456a3.ShatterOnRicochet = false;
            ammo_m456a3.SpallMultiplier = 2;
            ammo_m456a3.DetonateSpallCount = 150;
            ammo_m456a3.ForcedSpallAngle = 0;
            ammo_m456a3.Coeff = 0.16f;
            ammo_m456a3.ArmorOptimizations = era_optimizations_m456a3.ToArray();

            codex_m456a3 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            codex_m456a3.AmmoType = ammo_m456a3; codex_m456a3.name = "ammo_m456a3";

            clip_m456a3 = new AmmoType.AmmoClip();
            clip_m456a3.Capacity = 1; clip_m456a3.Name = "M456A3 HEAT-FS-T";
            clip_m456a3.MinimalPattern = new AmmoCodexScriptable[] { codex_m456a3 };

            clip_codex_m456a3 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_m456a3.name = "clip_m456a3"; clip_codex_m456a3.ClipType = clip_m456a3;

            GameObject vis_m456a3 = GameObject.Instantiate(ammo_m456.VisualModel);
            vis_m456a3.name = "M456A3 visual";
            ammo_m456a3.VisualModel = vis_m456a3;
            vis_m456a3.GetComponent<AmmoStoredVisual>().AmmoType = ammo_m456a3;
            vis_m456a3.GetComponent<AmmoStoredVisual>().AmmoScriptable = codex_m456a3;

            // M393A3
            ammo_m393a3 = new AmmoType();
            Util.ShallowCopy(ammo_m393a3, ammo_m456);
            ammo_m393a3.CachedIndex = -1;
            ammo_m393a3.Name = "M393A3 HEP-T";
            ammo_m393a3.RhaPenetration = 50f;
            ammo_m393a3.MuzzleVelocity = 750f;
            ammo_m393a3.Mass = 11.3f;
            ammo_m393a3.TntEquivalentKg = 5.26f;
            ammo_m393a3.CertainRicochetAngle = 5;
            ammo_m393a3.MinSpallRha = 3f;
            ammo_m393a3.MaxSpallRha = 60f;
            ammo_m393a3.Coeff = 0.16f;
            ammo_m393a3.Category = AmmoType.AmmoCategory.Explosive;
            ammo_m393a3.ShatterOnRicochet = false;
            ammo_m393a3.SpallMultiplier = 2;
            ammo_m393a3.DetonateSpallCount = hepFragments.Value;
            ammo_m393a3.ForcedSpallAngle = 0;
            ammo_m393a3.ShortName = AmmoType.AmmoShortName.He;
            ammo_m393a3.RhaToFuse = 15f;

            codex_m393a3 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            codex_m393a3.AmmoType = ammo_m393a3; codex_m393a3.name = "ammo_m393a3";

            clip_m393a3 = new AmmoType.AmmoClip();
            clip_m393a3.Capacity = 1; clip_m393a3.Name = "M393A3 HEP-T";
            clip_m393a3.MinimalPattern = new AmmoCodexScriptable[] { codex_m393a3 };

            clip_codex_m393a3 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_m393a3.name = "clip_m393a3"; clip_codex_m393a3.ClipType = clip_m393a3;

            GameObject vis_m393a3 = GameObject.Instantiate(ammo_m456.VisualModel);
            vis_m393a3.name = "M393A3 visual";
            ammo_m393a3.VisualModel = vis_m393a3;
            vis_m393a3.GetComponent<AmmoStoredVisual>().AmmoType = ammo_m393a3;
            vis_m393a3.GetComponent<AmmoStoredVisual>().AmmoScriptable = codex_m393a3;

            // M2 AP-T
            ammo_m2apt = new AmmoType();
            Util.ShallowCopy(ammo_m2apt, ammo_m8vnl);
            ammo_m2apt.CachedIndex = -1;
            ammo_m2apt.Name = "12.7x99mm M2 AP-T";
            ammo_m2apt.RhaPenetration = 29f;
            ammo_m2apt.MuzzleVelocity = 887;
            ammo_m2apt.CertainRicochetAngle = 15f;
            ammo_m2apt.MaxSpallRha = 8f;
            ammo_m2apt.MinSpallRha = 2f;
            ammo_m2apt.NutationPenaltyDistance = 0f;
            ammo_m2apt.SpallMultiplier = 10f;
            ammo_m2apt.UseTracer = true;

            codex_m2apt = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            codex_m2apt.AmmoType = ammo_m2apt; codex_m2apt.name = "ammo_m2apt";

            clip_m2apt = new AmmoType.AmmoClip();
            clip_m2apt.Capacity = 300; clip_m2apt.Name = "M2 AP-T";
            clip_m2apt.MinimalPattern = new AmmoCodexScriptable[] { codex_m2apt };

            clip_codex_m2apt = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_m2apt.name = "clip_m2apt"; clip_codex_m2apt.ClipType = clip_m2apt;

            // M8 AP-I
            ammo_m8api = new AmmoType();
            Util.ShallowCopy(ammo_m8api, ammo_m8vnl);
            ammo_m8api.CachedIndex = -1;
            ammo_m8api.Name = "12.7x99mm M8 AP-I";
            ammo_m8api.RhaPenetration = 29f;
            ammo_m8api.MuzzleVelocity = 887;
            ammo_m8api.CertainRicochetAngle = 15f;
            ammo_m8api.MaxSpallRha = 8f;
            ammo_m8api.MinSpallRha = 2f;
            ammo_m8api.NutationPenaltyDistance = 0f;
            ammo_m8api.SpallMultiplier = 20f;
            ammo_m8api.UseTracer = true;

            codex_m8api = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            codex_m8api.AmmoType = ammo_m8api; codex_m8api.name = "ammo_m8api";

            clip_m8api = new AmmoType.AmmoClip();
            clip_m8api.Capacity = 300; clip_m8api.Name = "M8 AP-I/T Mix";
            clip_m8api.MinimalPattern = new AmmoCodexScriptable[] { codex_m8api };

            clip_codex_m8api = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_m8api.name = "clip_m8api"; clip_codex_m8api.ClipType = clip_m8api;
        }

        public static void Init()
        {
            if (ammo_m393a3 != null) return;

            LoadAssets();

            Util.CacheAmmo(ammo_m900a1);
            Util.CacheAmmo(ammo_m900a2);
            Util.CacheAmmo(ammo_m456a3);
            Util.CacheAmmo(ammo_m393a3);
            Util.CacheAmmo(ammo_m2apt);
            Util.CacheAmmo(ammo_m8api);
        }
    }
}