using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GHPC;
using GHPC.AI;
using GHPC.Camera;
using GHPC.Equipment;
using GHPC.Equipment.Optics;
using GHPC.Player;
using GHPC.State;
using GHPC.Vehicle;
using GHPC.Weapons;
using GHPC.Weaponry;
using MelonLoader;
using NWH.VehiclePhysics;
using UnityEngine;

namespace SuperM60
{
    public static class M60A1
    {
        static readonly string[] gas_valid_ammo = { "M900A1 APFSDS-T", "M393A3 HEP-T" };
        static readonly string[] vic_names = { "M60A1 RISE (Passive)", "M60A1 RISE (Passive) '77", "M60A1 AOS", "M60A1" };

        static MelonPreferences_Entry<bool> useM900A2, betterLoader, betterCommander, betterGunner;
        static MelonPreferences_Entry<int> firstAmmoCount, secondAmmoCount, thirdAmmoCount;

        public static void Config(MelonPreferences_Category cfg)
        {
            useM900A2 = cfg.CreateEntry<bool>("M60A1 M900A2", false);
            useM900A2.Description = "Slightly better penetration, and better ERA defeat";

            firstAmmoCount = cfg.CreateEntry<int>("M60A1 M900 Round Count", 30);
            firstAmmoCount.Description = "How many rounds per type each upgraded M60A1 should carry. Maximum of 63 rounds total.";
            secondAmmoCount = cfg.CreateEntry<int>("M60A1 M456A3 Round Count", 20);
            thirdAmmoCount = cfg.CreateEntry<int>("M60A1 M393A3 Round Count", 13);

            betterLoader = cfg.CreateEntry<bool>("M60A1 Better Loader", false);
            betterLoader.Description = "M60A1 Crew Proficiency";
            betterCommander = cfg.CreateEntry<bool>("M60A1 Better Commander", false);
            betterGunner = cfg.CreateEntry<bool>("M60A1 Better Gunner", false);
        }

        public static void Update()
        {
            if (SuperM60Mod.gameManager == null) return;
            CameraSlot cam = SuperM60Mod.camManager._currentCamSlot;
            if (cam == null) return;
            if (cam.name != "Aux sight M105D" && cam.name != "Aux sight (GAS)") return;

            AmmoType current_ammo = SuperM60Mod.playerManager.CurrentPlayerWeapon.FCS.CurrentAmmoType;
            if (!System.Array.Exists(gas_valid_ammo, n => n == current_ammo.Name)) return;

            GameObject reticle = cam.transform.GetChild(0).gameObject;
            if (!reticle.activeSelf) reticle.SetActive(true);
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (Vehicle vic in SuperM60Mod.vics)
            {
                if (vic == null) continue;
                if (!System.Array.Exists(vic_names, n => n == vic.FriendlyName)) continue;
                if (vic.gameObject.GetComponent<AlreadyConverted>() != null) continue;

                vic.gameObject.AddComponent<AlreadyConverted>();
                GameObject vic_go = vic.gameObject;

                // Variable armor
                foreach (GameObject armour in GameObject.FindGameObjectsWithTag("Penetrable"))
                {
                    if (armour == null) continue;
                    VariableArmor va = armour.GetComponent<VariableArmor>();
                    if (va == null || va.Unit == null || va.Unit.FriendlyName != "M60A1 RISE (Passive)") continue;
                    if (va.Name == "turret") va._armorType = Armour.armor_codex_composite_turret;
                    if (va.Name == "hull") va._armorType = Armour.armor_codex_composite_hull;
                }

                // Uniform armor
                foreach (GameObject armour in GameObject.FindGameObjectsWithTag("Penetrable"))
                {
                    if (armour == null) continue;
                    UniformArmor ua = armour.GetComponent<UniformArmor>();
                    if (ua == null || ua.Unit == null || ua.Unit.FriendlyName != "M60A1 RISE (Passive)") continue;
                    ApplyUniformArmor(ua);
                }

                string originalName = vic.FriendlyName;
                vic._friendlyName = "M60A1A4";

                WeaponsManager weaponsManager = vic.GetComponent<WeaponsManager>();
                WeaponSystem mainGun = weaponsManager.Weapons[0].Weapon;
                WeaponSystemInfo coaxGunInfo = weaponsManager.Weapons[1];
                WeaponSystem coaxGun = coaxGunInfo.Weapon;
                WeaponSystem roofGun = weaponsManager.Weapons[1].Weapon;

                LoadoutManager loadoutManager = vic.GetComponent<LoadoutManager>();
                AmmoClipCodexScriptable apClip = useM900A2.Value ? AMMO_105mm.clip_codex_m900a2 : AMMO_105mm.clip_codex_m900a1;
                AmmoType.AmmoClip apClipType = useM900A2.Value ? AMMO_105mm.clip_m900a2 : AMMO_105mm.clip_m900a1;

                loadoutManager.LoadedAmmoList.AmmoClips = new AmmoClipCodexScriptable[] { apClip, AMMO_105mm.clip_codex_m456a3, AMMO_105mm.clip_codex_m393a3 };
                loadoutManager.TotalAmmoCounts = new int[] { firstAmmoCount.Value, secondAmmoCount.Value, thirdAmmoCount.Value };
                loadoutManager._totalAmmoTypes = 3;

                AmmoType.AmmoClip[] ammo_clip_types = { apClipType, AMMO_105mm.clip_m456a3, AMMO_105mm.clip_m393a3 };
                for (int i = 0; i < loadoutManager.RackLoadouts.Length; i++)
                {
                    loadoutManager.RackLoadouts[i].Rack.ClipTypes = ammo_clip_types;
                    Util.EmptyRack(loadoutManager.RackLoadouts[i].Rack);
                }

                loadoutManager.SpawnCurrentLoadout();
                mainGun.Feed.AmmoTypeInBreech = null;
                mainGun.Feed.Start();
                loadoutManager.RegisterAllBallistics();

                coaxGunInfo.Name = "M2HB RWS";
                coaxGun.SetCycleTime(0.1f);
                coaxGun.BaseDeviationAngle = 0.025f;
                coaxGun.Feed.AmmoTypeInBreech = null;
                coaxGun.Feed._totalCycleTime = 0.1f;
                coaxGun.Feed.ReadyRack.ClipTypes[0] = AMMO_105mm.clip_m8api;
                coaxGun.Feed.ReadyRack.Awake();
                coaxGun.Feed.Start();

                // Optics
                var gpsTransform = vic_go.transform.Find("M60A3TTS_rig/hull/turret/Turret Scripts/Sights/GPS/");
                var gasTransform = vic_go.transform.Find("M60A3TTS_rig/hull/turret/main gun mantlet/Gun Scripts/Aux sight M105D/");
                var roofOptic = vic_go.transform.Find("M60A3TTS_rig/hull/turret/cupola/cupola mantlet/M85 gunsight/");

                if (gpsTransform != null)
                {
                    gpsTransform.GetComponent<UsableOptic>().RotateAzimuth = true;
                    var gpsOptic = gpsTransform.GetComponent<CameraSlot>();
                    gpsOptic.DefaultFov = 4.5f;
                    gpsOptic.OtherFovs = new float[] { 2, 1 };
                    gpsOptic.BaseBlur = 0;
                    gpsOptic.VibrationBlurScale = 0;
                    gpsOptic.VibrationShakeMultiplier = 0.2f;
                }

                if (gasTransform != null)
                {
                    var gasOptic = gasTransform.GetComponent<CameraSlot>();
                    gasOptic.DefaultFov = 6.5f;
                    gasOptic.OtherFovs = new float[] { 4.5f, 2.5f, 1.5f };
                    gasOptic.VibrationBlurScale = 0.1f;
                    gasOptic.VibrationShakeMultiplier = 0.2f;
                }

                mainGun.FCS.ComputerNeedsPower = false;

                // Drivetrain
                VehicleController vicVC = vic_go.GetComponent<VehicleController>();
                NwhChassis vicNwhC = vic_go.GetComponent<NwhChassis>();

                vicVC.engine.maxPower = 935f;
                vicVC.engine.maxRPM = 4500f;
                vicVC.engine.maxRpmChange = 3000f;
                vicVC.brakes.maxTorque = 121920;
                vicNwhC._maxForwardSpeed = 22f;
                vicNwhC._maxReverseSpeed = 8f;

                vicVC.transmission.forwardGears = new List<float> { 6.28f, 4.81f, 2.98f, 1.76f, 1.36f, 1.16f };
                vicVC.transmission.reverseGears = new List<float> { -2.76f, -8.28f };
                vicVC.transmission.gears = new List<float> { -2.76f, -8.28f, 0f, 6.28f, 4.81f, 2.98f, 1.76f, 1.36f, 1.16f };
                vicVC.transmission.gearMultiplier = 10.16f;
                vicVC.transmission.shiftDuration = 0.01f;
                vicVC.transmission.shiftDurationRandomness = 0f;
                vicVC.transmission.shiftPointRandomness = 0.05f;

                for (int i = 0; i < 12; i++)
                {
                    vicVC.wheels[i].wheelController.damper.maxForce = 6500;
                    vicVC.wheels[i].wheelController.damper.unitBumpForce = 6500;
                    vicVC.wheels[i].wheelController.damper.unitReboundForce = 9000;
                    vicVC.wheels[i].wheelController.spring.length = 0.32f;
                    vicVC.wheels[i].wheelController.spring.maxForce = 240000;
                    vicVC.wheels[i].wheelController.spring.maxLength = 0.52f;
                    vicVC.wheels[i].wheelController.fFriction.forceCoefficient = 1.25f;
                    vicVC.wheels[i].wheelController.fFriction.slipCoefficient = 1f;
                    vicVC.wheels[i].wheelController.sFriction.forceCoefficient = 0.85f;
                    vicVC.wheels[i].wheelController.sFriction.slipCoefficient = 1f;
                }

                // Stabilizers — only base M60A1 lacks one; [2]=Gun Scripts, [3]=Turret Scripts
                if (originalName == "M60A1")
                {
                    vic.AimablePlatforms[2].Stabilized = true;
                    vic.AimablePlatforms[2]._stabActive = true;
                    vic.AimablePlatforms[2].StabilizerActive = true;
                    vic.AimablePlatforms[2]._stabMode = StabilizationMode.WorldPoint;
                    vic.AimablePlatforms[2].SpeedPowered = 40;
                    vic.AimablePlatforms[2].SpeedUnpowered = 40;
                    vic.AimablePlatforms[3].Stabilized = true;
                    vic.AimablePlatforms[3]._stabActive = true;
                    vic.AimablePlatforms[3].StabilizerActive = true;
                    vic.AimablePlatforms[3]._stabMode = StabilizationMode.WorldPoint;
                    vic.AimablePlatforms[3].SpeedPowered = 60;
                    vic.AimablePlatforms[3].SpeedUnpowered = 60;
                    mainGun.FCS.StabsActive = true;
                    mainGun.FCS.CurrentStabMode = StabilizationMode.WorldPoint;
                }

                // Roof gun
                roofGun.FCS.SuperelevateWeapon = true;
                roofGun.FCS.SuperleadWeapon = true;
                roofGun.FCS.LaserAim = LaserAimMode.ImpactPoint;
                roofGun.FCS.MaxLaserRange = 4000;
                roofGun.FCS.DefaultRange = 600;
                roofGun.FCS.RegisteredRangeLimits = new Vector2(100, 4000);
                roofGun.FCS.MarkRangeInvalidBelow = 100;

                if (roofOptic != null)
                {
                    var roofUO = roofOptic.GetComponent<UsableOptic>();
                    var roofCS = roofOptic.GetComponent<CameraSlot>();
                    roofUO.Alignment = OpticAlignment.FcsRange;
                    roofUO.ForceHorizontalReticleAlign = true;
                    roofUO.RotateElevation = true;
                    roofUO.RotateAzimuth = true;
                    roofCS.DefaultFov = 25f;
                    roofCS.OtherFovs = new float[] { 16.5f, 6.5f, 3.472f, 1.204f };
                    roofCS.VibrationBlurScale = 0;
                    roofCS.VibrationShakeMultiplier = 0;
                }

                if (betterLoader.Value)
                {
                    mainGun.Feed._totalReloadTime = 4.75f;
                    mainGun.Feed.SlowReloadMultiplier = 4f;
                    for (int i = 0; i < 3; i++) { loadoutManager.RackLoadouts[i].Rack._retrievalDelaySeconds = 3; loadoutManager.RackLoadouts[i].Rack._storageDelaySeconds = 1.5f; }
                    for (int i = 3; i < 5; i++) { loadoutManager.RackLoadouts[i].Rack._retrievalDelaySeconds = 4; loadoutManager.RackLoadouts[i].Rack._storageDelaySeconds = 1.5f; }
                }

                UnitAI vicUAI = vic.GetComponentInChildren<UnitAI>();

                if (betterCommander.Value)
                {
                    vicUAI.SpotTimeMaxDistance *= 1.35f;
                    vicUAI.TargetSensor._spotTimeMax *= 0.65f;
                    vicUAI.TargetSensor._spotTimeMaxDistance *= 1.35f;
                    vicUAI.TargetSensor._spotTimeMin *= 0.65f;
                }

                if (betterGunner.Value)
                {
                    vicUAI.combatSpeedLimit = 25;
                    vicUAI.firingSpeedLimit = 20;
                    vicUAI.AccuracyModifiers.Angle.MaxDistance *= 1.35f;
                    vicUAI.AccuracyModifiers.Angle.MaxRadius *= 0.65f;
                    vicUAI.AccuracyModifiers.Angle.MinRadius *= 0.65f;
                    vicUAI.AccuracyModifiers.Angle.IncreaseAccuracyPerShot = false;
                }
            }

            yield break;
        }

        static void ApplyUniformArmor(UniformArmor ua)
        {
            switch (ua.Name)
            {
                case "return roller":    ua.PrimaryHeatRha = ua.PrimarySabotRha = 50f; break;
                case "drive sprocket":   ua.PrimaryHeatRha = ua.PrimarySabotRha = 60f; break;
                case "driver hatch":     ua.PrimaryHeatRha = ua.PrimarySabotRha = 80f; break;
                case "engine deck":      ua.PrimaryHeatRha = ua.PrimarySabotRha = 80f; break;
                case "sponson storage":  ua.PrimaryHeatRha = ua.PrimarySabotRha = 25.4f; break;
                case "filter box":       ua.PrimaryHeatRha = ua.PrimarySabotRha = 25.4f; break;
                case "firewall":         ua.PrimaryHeatRha = ua.PrimarySabotRha = 38.1f; break;
                case "sponson":          ua.PrimaryHeatRha = ua.PrimarySabotRha = 12.7f; break;
                case "turret ring":      ua.PrimaryHeatRha = ua.PrimarySabotRha = 100f; break;
                case "driver's viewport":ua.PrimaryHeatRha = ua.PrimarySabotRha = 38.1f; break;
            }
        }

        public static void Init()
        {
            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Lowest);
        }
    }
}
