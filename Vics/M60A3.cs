using System.Collections;
using System.Collections.Generic;
using GHPC;
using GHPC.AI;
using GHPC.Camera;
using GHPC.Equipment;
using GHPC.Equipment.Optics;
using GHPC.State;
using GHPC.Vehicle;
using GHPC.Weapons;
using GHPC.Weaponry;
using MelonLoader;
using NWH.VehiclePhysics;
using UnityEngine;

namespace SuperM60
{
    public static class M60A3
    {
        static readonly string[] gas_valid_ammo = { "M900A1 APFSDS-T", "M393A3 HEP-T" };

        static MelonPreferences_Entry<bool> useM900A2, betterLoader, betterCommander, betterGunner;
        static MelonPreferences_Entry<int> firstAmmoCount, secondAmmoCount, thirdAmmoCount;

        public static void Config(MelonPreferences_Category cfg)
        {
            useM900A2 = cfg.CreateEntry<bool>("M60A3 M900A2", false);
            useM900A2.Description = "Slightly better penetration, and better ERA defeat";

            firstAmmoCount = cfg.CreateEntry<int>("M60A3 M900 Round Count", 30);
            firstAmmoCount.Description = "How many rounds per type each upgraded M60A3 should carry. Maximum of 63 rounds total.";
            secondAmmoCount = cfg.CreateEntry<int>("M60A3 M456A3 Round Count", 20);
            thirdAmmoCount = cfg.CreateEntry<int>("M60A3 M393A3 Round Count", 13);

            betterLoader = cfg.CreateEntry<bool>("M60A3 Better Loader", false);
            betterLoader.Description = "M60A3 Crew Proficiency";
            betterCommander = cfg.CreateEntry<bool>("M60A3 Better Commander", false);
            betterGunner = cfg.CreateEntry<bool>("M60A3 Better Gunner", false);
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
                if (vic.FriendlyName != "M60A3 TTS") continue;
                if (vic.gameObject.GetComponent<AlreadyConverted>() != null) continue;

                vic.gameObject.AddComponent<AlreadyConverted>();
                GameObject vic_go = vic.gameObject;

                // Variable armor
                foreach (GameObject armour in GameObject.FindGameObjectsWithTag("Penetrable"))
                {
                    if (armour == null) continue;
                    VariableArmor va = armour.GetComponent<VariableArmor>();
                    if (va == null || va.Unit == null || va.Unit.FriendlyName != "M60A3 TTS") continue;
                    if (va.Name == "turret") va._armorType = Armour.armor_codex_composite_turret;
                    if (va.Name == "hull") va._armorType = Armour.armor_codex_composite_hull;
                }

                // Uniform armor
                foreach (GameObject armour in GameObject.FindGameObjectsWithTag("Penetrable"))
                {
                    if (armour == null) continue;
                    UniformArmor ua = armour.GetComponent<UniformArmor>();
                    if (ua == null || ua.Unit == null || ua.Unit.FriendlyName != "M60A3 TTS") continue;
                    ApplyUniformArmor(ua);
                }

                vic._friendlyName = "M60A4";

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
                var flirTransform = vic_go.transform.Find("M60A3TTS_rig/hull/turret/Turret Scripts/Sights/FLIR/");
                var gasOptic = vic_go.transform.Find("M60A3TTS_rig/hull/turret/main gun mantlet/Gun Scripts/Aux sight M105D/").GetComponent<CameraSlot>();
                var roofOptic = vic_go.transform.Find("M60A3TTS_rig/hull/turret/cupola/cupola mantlet/M85 gunsight/");

                gpsTransform.GetComponent<UsableOptic>().RotateAzimuth = true;
                flirTransform.GetComponent<UsableOptic>().RotateAzimuth = true;

                List<float> fovs = new List<float>();
                for (float f = 14.5f; f >= 1f; f -= 1.5f) fovs.Add(f);

                var gpsCS = gpsTransform.GetComponent<CameraSlot>();
                gpsCS.DefaultFov = 16;
                gpsCS.OtherFovs = fovs.ToArray();
                gpsCS.BaseBlur = 0;
                gpsCS.VibrationBlurScale = 0;
                gpsCS.VibrationShakeMultiplier = 0.2f;

                var flirCS = flirTransform.GetComponent<CameraSlot>();
                flirCS.DefaultFov = 16;
                flirCS.OtherFovs = fovs.ToArray();
                flirCS.BaseBlur = 0;
                flirCS.VibrationBlurScale = 0;
                flirCS.VibrationShakeMultiplier = 0.2f;
                GameObject.Destroy(flirTransform.Find("Canvas Scanlines").gameObject);

                gasOptic.DefaultFov = 6.5f;
                gasOptic.OtherFovs = new float[] { 4.5f, 2.5f, 1.5f };
                gasOptic.VibrationBlurScale = 0.1f;
                gasOptic.VibrationShakeMultiplier = 0.2f;

                mainGun.FCS.ComputerNeedsPower = false;
                mainGun.FCS.CurrentStabMode = StabilizationMode.WorldPoint;

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

                // Stabilizers
                vic.AimablePlatforms[0].SpeedPowered = 60;
                vic.AimablePlatforms[0].SpeedUnpowered = 60;
                vic.AimablePlatforms[0]._stabActive = true;
                vic.AimablePlatforms[0].StabilizerActive = true;
                vic.AimablePlatforms[0]._stabMode = StabilizationMode.Vector;
                vic.AimablePlatforms[1].SpeedPowered = 40;
                vic.AimablePlatforms[1].SpeedUnpowered = 40;
                vic.AimablePlatforms[1]._stabActive = true;
                vic.AimablePlatforms[1].StabilizerActive = true;
                vic.AimablePlatforms[1]._stabMode = StabilizationMode.Vector;
                vic.AimablePlatforms[1].LocalEulerLimits = new Vector2(-17, 65);
                vic.AimablePlatforms[2].SpeedPowered = 40;
                vic.AimablePlatforms[2].SpeedUnpowered = 20;
                vic.AimablePlatforms[2]._stabMode = StabilizationMode.WorldPoint;
                vic.AimablePlatforms[3].SpeedPowered = 40;
                vic.AimablePlatforms[3].SpeedUnpowered = 20;
                vic.AimablePlatforms[3]._stabMode = StabilizationMode.WorldPoint;

                // Roof gun
                var roofCS = roofOptic.GetComponent<CameraSlot>();
                var roofUO = roofOptic.GetComponent<UsableOptic>();

                roofGun.FCS._fixParallaxForVectorMode = true;
                roofGun.FCS.SuperelevateWeapon = true;
                roofGun.FCS.SuperleadWeapon = true;
                roofGun.FCS.LaserAim = LaserAimMode.ImpactPoint;
                roofGun.FCS.MaxLaserRange = 4000;
                roofGun.FCS.DefaultRange = 600;
                roofGun.FCS.RegisteredRangeLimits = new Vector2(100, 4000);
                roofGun.FCS.MarkRangeInvalidBelow = 100;
                roofGun.FCS.CurrentStabMode = StabilizationMode.Vector;
                roofGun.FCS.StabsActive = true;

                roofUO.Alignment = OpticAlignment.FcsRange;
                roofUO.ForceHorizontalReticleAlign = true;
                roofUO.RotateElevation = true;
                roofUO.RotateAzimuth = true;

                roofCS.DefaultFov = 25f;
                roofCS.OtherFovs = new float[] { 16.5f, 6.5f, 3.472f, 1.204f };
                roofCS.VibrationBlurScale = 0;
                roofCS.VibrationShakeMultiplier = 0;

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
                    vicUAI.SpotTimeMaxDistance = 3500;
                    vicUAI.TargetSensor._spotTimeMax = 3;
                    vicUAI.TargetSensor._spotTimeMaxDistance = 500;
                    vicUAI.TargetSensor._spotTimeMaxVelocity = 7f;
                    vicUAI.TargetSensor._spotTimeMin = 1;
                    vicUAI.TargetSensor._spotTimeMinDistance = 50;
                    vicUAI.TargetSensor._trackedTargetCooldown = 1.5f;
                    vicUAI.CommanderAI._identifyTargetDurationRange = new Vector2(1.5f, 2.5f);
                    vicUAI.CommanderAI._sweepCommsCheckDuration = 4;
                }

                if (betterGunner.Value)
                {
                    vicUAI.combatSpeedLimit = 25;
                    vicUAI.firingSpeedLimit = 20;
                    vicUAI.AccuracyModifiers.Angle.MaxDistance = 2500;
                    vicUAI.AccuracyModifiers.Angle.MaxRadius = 2.5f;
                    vicUAI.AccuracyModifiers.Angle.MinRadius = 1.5f;
                    vicUAI.AccuracyModifiers.Angle.IncreaseAccuracyPerShot = false;
                }
            }

            yield break;
        }

        static void ApplyUniformArmor(UniformArmor ua)
        {
            switch (ua.Name)
            {
                case "return roller":     ua.PrimaryHeatRha = ua.PrimarySabotRha = 50f; break;
                case "drive sprocket":    ua.PrimaryHeatRha = ua.PrimarySabotRha = 60f; break;
                case "driver hatch":      ua.PrimaryHeatRha = ua.PrimarySabotRha = 80f; break;
                case "engine deck":       ua.PrimaryHeatRha = ua.PrimarySabotRha = 80f; break;
                case "sponson storage":   ua.PrimaryHeatRha = ua.PrimarySabotRha = 25.4f; break;
                case "filter box":        ua.PrimaryHeatRha = ua.PrimarySabotRha = 25.4f; break;
                case "firewall":          ua.PrimaryHeatRha = ua.PrimarySabotRha = 38.1f; break;
                case "sponson":           ua.PrimaryHeatRha = ua.PrimarySabotRha = 12.7f; break;
                case "turret ring":       ua.PrimaryHeatRha = ua.PrimarySabotRha = 100f; break;
                case "driver's viewport": ua.PrimaryHeatRha = ua.PrimarySabotRha = 38.1f; break;
            }
        }

        public static void Init()
        {
            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Lowest);
        }
    }
}
