using MelonLoader;
using UnityEngine;
using Il2CppMonsterBox.Runtime.Gameplay.Arcade.Level;
using Il2CppMonsterBox.Runtime.Gameplay.Arcade.Calamity.Kraken;
using Il2CppMonsterBox.Runtime.Gameplay.Arcade.Minigames;
using Il2CppMonsterBox.Runtime.Gameplay.RailShooter;
using Il2CppMonsterBox.Runtime.Gameplay.SecurityOffice.Sanity;
using Il2CppMonsterBox.Runtime.Gameplay.SecurityOffice.Microgames;
using Il2CppMonsterBox.Runtime.Gameplay.Interactables;
using Il2CppMonsterBox.Runtime.Gameplay.SecurityOffice.Character;
using Il2CppMonsterBox.Runtime.Gameplay.SecurityOffice.Enums;
using Il2CppMonsterBox.Runtime.Gameplay.SecurityOffice.Interactables;
using Il2CppMonsterBox.Runtime.Gameplay.Level.Scriptables;
using Il2CppMonsterBox.Runtime.Gameplay.Level;
using Il2CppMonsterBox.Runtime.Gameplay.Room;
using Il2CppMonsterBox.Runtime.Gameplay.Room.Microgames;
using Il2CppMonsterBox.Runtime.Gameplay.Character;
using Il2CppMonsterBox.Runtime.Gameplay.Character.Extensions;
using Il2CppMonsterBox.Runtime.Gameplay.Customer;
using Il2CppMonsterBox.Runtime.Gameplay.Nightclub.Customer;
using Il2CppMonsterBox.Runtime.Gameplay.Nightclub.Character;
using Il2CppMonsterBox.Runtime.Gameplay.Nightclub.Enums;
using Il2CppMonsterBox.Runtime.Gameplay.Nightclub.Interactables;
using Il2CppMonsterBox.Runtime.Gameplay.Nightclub.Microgames.Jailbreak;
using GameLevel = Il2CppMonsterBox.Runtime.Gameplay.Enums.Level;
using Il2CppMonsterBox.Runtime.Title;
using Il2CppMonsterBox.Runtime.Title.Scriptables;
using Il2CppMonsterBox.Core.Media;
using Il2CppMonsterBox.Core.Saving;
using Il2CppMonsterBox.Core.Saving.Data;
using Il2CppMonsterBox.Core.Enums;
using Il2CppMonsterBox.Runtime.Gallery;
using Il2CppMonsterBox.Runtime.Gameplay.Player;
using HarmonyLib;

[assembly: MelonInfo(typeof(InHeatMenu.MenuMod), "IN HEAT / Clover Mod Menu", "1.0.0", "Poklut")]
[assembly: MelonGame("MonsterBox", "IN HEAT")]

namespace InHeatMenu;

public sealed class MenuMod : MelonMod
{
    internal static bool MediaOverride;
    internal static bool PlushieOverride;
    internal static bool GalleryOverride;
    internal static bool DatingOverride;
    private Rect _window = new(30f, 80f, 410f, 520f);
    private bool _visible = true;
    private int _activeTab = 1;
    private bool _infiniteArcadeEnergy;
    private bool _infiniteArcadePsych;
    private bool _antiCorruption;
    private bool _autoRemoveTentacles;
    private bool _autoMashMisty;
    private bool _infiniteSanity;
    private bool _autoFixGasLeaks;
    private bool _autoHelpCleaningRobot;
    private bool _autoDisarmVentBunny;
    private bool _autoFixPower;
    private bool _autoDoors;
    private bool _autoMashSammyLookAway;
    private bool _freezeEvolvedAri;
    private bool _autoCleanRooms;
    private bool _autoFixNightclubFuseBoxes;
    private bool _autoAssignNightclubCustomers;
    private bool _autoCloseNightclubDoors;
    private bool _autoDefuseNightclubBombs;
    private bool _autoMicrowaveMaddie;
    private bool _microwaveTriggeredForVisit;
    private bool _unlockedPlushies;
    private bool _unlockedMedia;
    private bool _unlockedGallery;
    private bool _unlockedDating;
    private bool _maxedAffection;
    private bool _fpsUnlocker;
    private bool _showFps = true;
    private bool _showModeLevel = true;
    private bool _showNightClock = true;
    private bool _showSurvivalRemaining = true;
    private int _originalTargetFrameRate;
    private int _originalVSync;
    private float _smoothedFps;
    private LevelManagerBase _levelManager;
    private LevelTimeManager _levelTimeManager;
    private float _leftDoorHoldUntil;
    private float _rightDoorHoldUntil;
    private ArcadeResourceManager _arcadeResources;
    private SecurityOfficeSanityManager _sanityManager;
    private PlayerStateMachine _playerStateMachine;
    private bool _sammyCloseActive;
    private bool _sammyPulseDown;
    private int _sammyPulseIndex;
    private float _sammyNextPulse;
    private float _nextLookup;
    private readonly HashSet<int> _removedTentacles = new();
    private readonly HashSet<int> _mashedMistyGames = new();
    private readonly HashSet<int> _resolvedValves = new();
    private readonly HashSet<int> _resolvedRobots = new();
    private readonly HashSet<int> _resolvedPowerPanels = new();
    private readonly HashSet<int> _pausedEvolvedAri = new();
    private readonly Dictionary<int, Vector3> _evolvedAriPositions = new();
    private readonly Dictionary<int, Quaternion> _evolvedAriRotations = new();
    private readonly HashSet<int> _fixedNightclubFuseBoxes = new();
    private readonly HashSet<int> _cleanedNightclubRooms = new();
    private readonly HashSet<int> _fixedBrokenNightclubRooms = new();
    private readonly HashSet<int> _defusedNightclubBombs = new();
    private GUIStyle _titleStyle;
    private GUIStyle _statusStyle;
    private GUIStyle _panelStyle;
    private GUIStyle _tabStyle;
    private GUIStyle _activeTabStyle;
    private GUIStyle _toggleOnStyle;
    private GUIStyle _toggleOffStyle;
    private GUIStyle _toggleLabelStyle;
    private GUIStyle _fpsStyle;
    private GUIStyle _hudCenterStyle;

    public override void OnInitializeMelon()
    {
        _originalTargetFrameRate = Application.targetFrameRate;
        _originalVSync = QualitySettings.vSyncCount;
        HarmonyInstance.PatchAll();
        LoggerInstance.Msg("Loaded. Press F1 to toggle the menu.");
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            _visible = !_visible;
        if (Input.GetKeyDown(KeyCode.F2))
        {
            _autoMashSammyLookAway = false;
            ReleaseSammyInput();
        }

        if (Time.unscaledTime >= _nextLookup)
        {
            _nextLookup = Time.unscaledTime + 0.1f;
            if (_infiniteArcadeEnergy || _infiniteArcadePsych || _antiCorruption)
                _arcadeResources = UnityEngine.Object.FindObjectOfType<ArcadeResourceManager>();
            if (_infiniteSanity)
                _sanityManager = UnityEngine.Object.FindObjectOfType<SecurityOfficeSanityManager>();
            if (_autoRemoveTentacles)
                RemoveActiveTentacles();
            if (_autoMashMisty)
                MashActiveMistyVentGames();
            if (_autoFixGasLeaks || _autoHelpCleaningRobot || _autoDisarmVentBunny || _autoFixPower || _autoDoors || _autoMashSammyLookAway || _autoMicrowaveMaddie)
                RunSecurityOfficeAutomation();
            UpdateEvolvedAriFreeze();
            if (_autoCleanRooms || _autoFixNightclubFuseBoxes || _autoAssignNightclubCustomers || _autoCloseNightclubDoors || _autoDefuseNightclubBombs)
                RunNightclubAutomation();
            if (_showModeLevel || _showNightClock || _showSurvivalRemaining)
            {
                _levelManager = UnityEngine.Object.FindObjectOfType<LevelManagerBase>();
                _levelTimeManager = UnityEngine.Object.FindObjectOfType<LevelTimeManager>();
            }
        }

        if (_infiniteArcadeEnergy && _arcadeResources != null)
            _arcadeResources.SetEnergy(100f);
        if (_infiniteArcadePsych && _arcadeResources != null)
            _arcadeResources.SetPsych(100f);
        if (_antiCorruption && _arcadeResources != null)
            _arcadeResources.SetCorruption(0f);
        if (_infiniteSanity && _sanityManager != null)
            _sanityManager.SetSanity(100f);

        _smoothedFps = Mathf.Lerp(_smoothedFps, 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f), 0.08f);
        if (_fpsUnlocker)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
        }
    }

    public override void OnLateUpdate()
    {
        ApplyEvolvedAriTransformFreeze();
        if (!_autoMashSammyLookAway || !_sammyCloseActive)
        {
            ReleaseSammyInput();
            return;
        }

        if (_playerStateMachine == null)
            _playerStateMachine = UnityEngine.Object.FindObjectOfType<PlayerStateMachine>();
        if (_playerStateMachine == null || Time.unscaledTime < _sammyNextPulse)
            return;

        if (_sammyPulseDown)
        {
            _playerStateMachine.CameraInput = Vector2.zero;
            _sammyPulseDown = false;
            _sammyNextPulse = Time.unscaledTime + 0.006f;
            return;
        }

        Vector2[] sequence =
        {
            new(0f, 1f),   // W
            new(-1f, 0f),  // A
            new(0f, -1f),  // S
            new(1f, 0f)    // D
        };
        _playerStateMachine.CameraInput = sequence[_sammyPulseIndex++ & 3];
        _sammyPulseDown = true;
        _sammyNextPulse = Time.unscaledTime + 0.025f;
    }

    private void ReleaseSammyInput()
    {
        if (_sammyPulseDown && _playerStateMachine != null)
            _playerStateMachine.CameraInput = Vector2.zero;
        _sammyPulseDown = false;
        _sammyPulseIndex = 0;
    }

    public override void OnGUI()
    {
        EnsureStyles();
        if (_showFps)
            GUI.Label(new Rect(Screen.width - 145f, 10f, 130f, 30f), $"{_smoothedFps:0} FPS", _fpsStyle);
        if (_showModeLevel && _levelManager != null && LevelManagerBase.LevelConfig != null)
            GUI.Label(new Rect(Screen.width - 285f, 38f, 270f, 26f), GetModeLevelText(), _fpsStyle);
        if (_showNightClock && _levelTimeManager != null)
            GUI.Label(new Rect((Screen.width - 300f) * 0.5f, 8f, 300f, 28f), GetNightClockText(), _hudCenterStyle);
        if (_showSurvivalRemaining && _levelTimeManager != null)
            GUI.Label(new Rect((Screen.width - 300f) * 0.5f, 34f, 300f, 26f), GetRemainingText(), _hudCenterStyle);
        if (!_visible)
            return;

        GUI.backgroundColor = new Color(0.12f, 0.08f, 0.16f, 0.98f);
        GUI.color = Color.white;
        GUILayout.BeginArea(_window, "IN HEAT / CLOVER MOD MENU made by Poklut", GUI.skin.window);
        DrawMenu();
        GUILayout.EndArea();
    }

    private void DrawMenu()
    {
        GUILayout.Space(6f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("SECURITY OFFICE", _activeTab == 1 ? _activeTabStyle : _tabStyle)) _activeTab = 1;
        if (GUILayout.Button("NIGHTCLUB", _activeTab == 2 ? _activeTabStyle : _tabStyle)) _activeTab = 2;
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("DATING SIM", _activeTab == 4 ? _activeTabStyle : _tabStyle)) _activeTab = 4;
        if (GUILayout.Button("MISC", _activeTab == 3 ? _activeTabStyle : _tabStyle)) _activeTab = 3;
        if (GUILayout.Button("ARCADE", _activeTab == 0 ? _activeTabStyle : _tabStyle)) _activeTab = 0;
        GUILayout.EndHorizontal();

        GUILayout.Space(10f);
        if (_activeTab == 0) DrawArcadeTab();
        else if (_activeTab == 1) DrawSecurityOfficeTab();
        else if (_activeTab == 2) DrawNightclubTab();
        else if (_activeTab == 3) DrawMiscTab();
        else DrawDatingTab();

        GUILayout.FlexibleSpace();
        GUILayout.Label("F1 menu  |  F2 emergency input release", _statusStyle);
    }

    private void DrawArcadeTab()
    {
        GUILayout.Label("ARCADE", _titleStyle);
        _infiniteArcadeEnergy = ToggleRow("Infinite energy", _infiniteArcadeEnergy);
        _infiniteArcadePsych = ToggleRow("Infinite psych", _infiniteArcadePsych);
        _antiCorruption = ToggleRow("Anti-corruption", _antiCorruption);
        _autoRemoveTentacles = ToggleRow("Auto-remove Kraken tentacles", _autoRemoveTentacles);
        _autoMashMisty = ToggleRow("Auto-mash Misty vent attack", _autoMashMisty);
        GUILayout.Space(10f);
        string state = _arcadeResources != null
            ? $"Energy {_arcadeResources.Energy:0.0}  |  Psych {_arcadeResources.Psych:0.0}  |  Corruption {_arcadeResources.Corruption:0.0}"
            : "Waiting for an Arcade level";
        GUILayout.Label(state, _statusStyle);
    }

    private void RemoveActiveTentacles()
    {
        var tentacles = UnityEngine.Object.FindObjectsOfType<KrakenCalamityTentacle>();
        if (tentacles.Length == 0)
        {
            _removedTentacles.Clear();
            return;
        }

        foreach (var tentacle in tentacles)
        {
            if (tentacle == null || !_removedTentacles.Add(tentacle.GetInstanceID()))
                continue;
            tentacle.OnTentacleDestroyed();
        }
    }

    private void MashActiveMistyVentGames()
    {
        var games = UnityEngine.Object.FindObjectsOfType<MistyVentAttackMinigame>();
        if (games.Length == 0)
        {
            _mashedMistyGames.Clear();
            return;
        }

        foreach (var game in games)
        {
            if (game == null || !_mashedMistyGames.Add(game.GetInstanceID()))
                continue;
            game.OnButtonMashWin();
        }
    }

    private bool ToggleRow(string label, bool value)
    {
        GUILayout.BeginHorizontal();
        bool clicked = GUILayout.Button(value ? "✓" : "", value ? _toggleOnStyle : _toggleOffStyle,
            GUILayout.Width(28f), GUILayout.Height(27f));
        if (GUILayout.Button(label, _toggleLabelStyle, GUILayout.Height(27f)))
            clicked = true;
        GUILayout.EndHorizontal();
        return clicked ? !value : value;
    }

    private void DrawSecurityOfficeTab()
    {
        GUILayout.Label("SECURITY OFFICE", _titleStyle);
        _infiniteSanity = ToggleRow("Infinite sanity", _infiniteSanity);
        _autoFixGasLeaks = ToggleRow("Auto-fix gas leaks", _autoFixGasLeaks);
        _autoHelpCleaningRobot = ToggleRow("Auto-help cleaning robot", _autoHelpCleaningRobot);
        _autoDisarmVentBunny = ToggleRow("Auto-remove exploding vent bunny", _autoDisarmVentBunny);
        _autoFixPower = ToggleRow("Auto-fix electrical panel", _autoFixPower);
        _autoDoors = ToggleRow("Auto-close threatened doors", _autoDoors);
        _autoMashSammyLookAway = ToggleRow("Auto-mash Sammy look-away", _autoMashSammyLookAway);
        _autoMicrowaveMaddie = ToggleRow("Auto-use microwave for Maddie", _autoMicrowaveMaddie);
        GUILayout.Space(12f);
        GUILayout.Label("BOSS LEVEL", _titleStyle);
        _freezeEvolvedAri = ToggleRow("Freeze evolved Ari", _freezeEvolvedAri);
        GUILayout.Space(10f);
        GUILayout.Label("Next: Sammy look-away and Maddie microwave", _statusStyle);
        GUILayout.Label(_sanityManager != null ? "Security Office detected" : "Waiting for Security Office", _statusStyle);
    }

    private void RunSecurityOfficeAutomation()
    {
        var manager = UnityEngine.Object.FindObjectOfType<SecurityOfficeMicrogameManager>();

        if (_autoFixGasLeaks)
        {
            var valves = UnityEngine.Object.FindObjectsOfType<ValveTurnMicrogame>();
            foreach (var valve in valves)
            {
                if (valve == null) continue;
                int id = valve.GetInstanceID();
                if (valve.IsActive && _resolvedValves.Add(id))
                {
                    valve.EndMicrogame(true);
                    if (manager != null) manager.UpdateStateLight();
                }
                else if (!valve.IsActive) _resolvedValves.Remove(id);
            }
        }

        if (_autoHelpCleaningRobot)
        {
            var robots = UnityEngine.Object.FindObjectsOfType<CleaningRobotMicrogame>();
            foreach (var robot in robots)
            {
                if (robot == null) continue;
                int id = robot.GetInstanceID();
                if (robot.IsActive && _resolvedRobots.Add(id))
                {
                    robot.EndMicrogame(true);
                    if (manager != null) manager.UpdateStateLight();
                }
                else if (!robot.IsActive) _resolvedRobots.Remove(id);
            }
        }

        if (_autoFixPower)
        {
            var panels = UnityEngine.Object.FindObjectsOfType<PowerPanelMicrogame>();
            foreach (var panel in panels)
            {
                if (panel == null) continue;
                int id = panel.GetInstanceID();
                if (panel.IsActive && _resolvedPowerPanels.Add(id))
                {
                    panel.EndMicrogame(true);
                    if (manager != null) manager.UpdateStateLight();
                }
                else if (!panel.IsActive) _resolvedPowerPanels.Remove(id);
            }
        }

        if (_autoDisarmVentBunny)
        {
            var bombs = UnityEngine.Object.FindObjectsOfType<PlushieBomb>();
            foreach (var bomb in bombs)
                if (bomb != null && bomb.BombActive)
                    bomb.Disarm();
        }

        if (_autoDoors)
        {
            bool leftThreat = false;
            bool rightThreat = false;
            var characters = UnityEngine.Object.FindObjectsOfType<SecurityOfficeCharacterController>();
            foreach (var character in characters)
            {
                if (character == null) continue;
                if (character.Character == Character.Ari || character.Character == Character.AriSO)
                    continue;
                leftThreat |= character.CurrentPosition == (int)SecurityOfficePositions.HallwayLeft ||
                              character.CurrentPosition == (int)SecurityOfficePositions.WindowLeft;
                rightThreat |= character.CurrentPosition == (int)SecurityOfficePositions.HallwayRight ||
                               character.CurrentPosition == (int)SecurityOfficePositions.WindowRight;
            }

            if (leftThreat) _leftDoorHoldUntil = Time.unscaledTime + 3f;
            if (rightThreat) _rightDoorHoldUntil = Time.unscaledTime + 3f;
            bool keepLeftClosed = Time.unscaledTime < _leftDoorHoldUntil;
            bool keepRightClosed = Time.unscaledTime < _rightDoorHoldUntil;

            var doors = UnityEngine.Object.FindObjectsOfType<SecurityOfficeSecurityDoor>();
            foreach (var door in doors)
            {
                if (door == null) continue;
                if (door.doorPosition == SecurityOfficeDoorPosition.Left) door.SetOpenState(!keepLeftClosed);
                else if (door.doorPosition == SecurityOfficeDoorPosition.Right) door.SetOpenState(!keepRightClosed);
            }
        }

        _sammyCloseActive = false;
        if (_autoMashSammyLookAway)
        {
            var characters = UnityEngine.Object.FindObjectsOfType<SecurityOfficeCharacterController>();
            foreach (var character in characters)
            {
                if (character == null) continue;
                bool isSammy = character.Character == Character.Sammy || character.Character == Character.SammySO;
                _sammyCloseActive |= isSammy && character.CurrentPosition == (int)SecurityOfficePositions.SecurityRoom;
            }
        }

        if (_autoMicrowaveMaddie)
        {
            bool maddieInOffice = false;
            var characters = UnityEngine.Object.FindObjectsOfType<SecurityOfficeCharacterController>();
            foreach (var character in characters)
            {
                if (character == null) continue;
                bool isMaddie = character.Character == Character.Maddie || character.Character == Character.MaddieSO;
                maddieInOffice |= isMaddie && character.CurrentPosition == (int)SecurityOfficePositions.SecurityRoom;
            }

            if (!maddieInOffice)
                _microwaveTriggeredForVisit = false;
            else if (!_microwaveTriggeredForVisit)
            {
                var microwave = UnityEngine.Object.FindObjectOfType<Microwave>();
                if (microwave != null && microwave.CanActivate)
                {
                    microwave.OnInteract();
                    _microwaveTriggeredForVisit = true;
                }
            }
        }
        else _microwaveTriggeredForVisit = false;

    }

    private void UpdateEvolvedAriFreeze()
    {
        var characters = UnityEngine.Object.FindObjectsOfType<SecurityOfficeCharacterController>();
        foreach (var character in characters)
        {
            if (character == null || (character.Character != Character.AriBM && character.Character != Character.AriA))
                continue;
            int id = character.GetInstanceID();
            if (_freezeEvolvedAri && _pausedEvolvedAri.Add(id))
            {
                _evolvedAriPositions[id] = character.transform.position;
                _evolvedAriRotations[id] = character.transform.rotation;
                character.PauseAI(true);
            }
            else if (!_freezeEvolvedAri && _pausedEvolvedAri.Remove(id))
            {
                character.PauseAI(false);
                _evolvedAriPositions.Remove(id);
                _evolvedAriRotations.Remove(id);
            }
        }
    }

    private void ApplyEvolvedAriTransformFreeze()
    {
        if (!_freezeEvolvedAri || _pausedEvolvedAri.Count == 0)
            return;
        var characters = UnityEngine.Object.FindObjectsOfType<SecurityOfficeCharacterController>();
        foreach (var character in characters)
        {
            if (character == null) continue;
            int id = character.GetInstanceID();
            if (!_pausedEvolvedAri.Contains(id)) continue;
            if (_evolvedAriPositions.TryGetValue(id, out var position)) character.transform.position = position;
            if (_evolvedAriRotations.TryGetValue(id, out var rotation)) character.transform.rotation = rotation;
        }
    }

    private void DrawNightclubTab()
    {
        GUILayout.Label("NIGHTCLUB", _titleStyle);
        _autoAssignNightclubCustomers = ToggleRow("Auto-assign customers and girls", _autoAssignNightclubCustomers);
        _autoCloseNightclubDoors = ToggleRow("Auto-close threatened office doors", _autoCloseNightclubDoors);
        _autoCleanRooms = ToggleRow("Auto-clean dirty rooms", _autoCleanRooms);
        _autoFixNightclubFuseBoxes = ToggleRow("Auto-fix fuse boxes", _autoFixNightclubFuseBoxes);
        GUILayout.Space(10f);
        GUILayout.Label("BOSS LEVEL", _titleStyle);
        _autoDefuseNightclubBombs = ToggleRow("Auto-defuse bombs", _autoDefuseNightclubBombs);
        GUILayout.Space(8f);
        GUILayout.Label("Uses each customer's girl and room restrictions.", _statusStyle);
    }

    private void RunNightclubAutomation()
    {
        if (_autoAssignNightclubCustomers)
            AssignNightclubCustomers();
        if (_autoCloseNightclubDoors)
            ProtectNightclubOffice();
        if (_autoDefuseNightclubBombs)
            DefuseNightclubBombs();

        if (_autoCleanRooms)
        {
            var rooms = UnityEngine.Object.FindObjectsOfType<RoomController>();
            foreach (var room in rooms)
            {
                if (room == null) continue;
                int id = room.GetInstanceID();
                if (room.IsDirty && _cleanedNightclubRooms.Add(id))
                {
                    room._cleanTime = 0f;
                    room.CleanRoom(false);
                }
                else if (!room.IsDirty)
                    _cleanedNightclubRooms.Remove(id);
            }
        }

        if (_autoFixNightclubFuseBoxes)
        {
            var rooms = UnityEngine.Object.FindObjectsOfType<RoomController>();
            foreach (var room in rooms)
            {
                if (room == null || room.roomFuseBox == null) continue;
                int id = room.GetInstanceID();
                if (room.IsBroken && _fixedBrokenNightclubRooms.Add(id))
                {
                    if (room.roomFuseBox.IsActive) room.roomFuseBox.EndMicrogame(true);
                    room.OnPowerFixed();
                    room.UpdateRoomState();
                    room.UpdateStateLight();
                }
                else if (!room.IsBroken)
                    _fixedBrokenNightclubRooms.Remove(id);
            }

            var fuseBoxes = UnityEngine.Object.FindObjectsOfType<RoomFuseBoxMicrogame>();
            foreach (var fuseBox in fuseBoxes)
            {
                if (fuseBox == null) continue;
                int id = fuseBox.GetInstanceID();
                if (fuseBox.IsActive && _fixedNightclubFuseBoxes.Add(id))
                    fuseBox.EndMicrogame(true);
                else if (!fuseBox.IsActive)
                    _fixedNightclubFuseBoxes.Remove(id);
            }
        }
    }

    private void DefuseNightclubBombs()
    {
        foreach (var bomb in UnityEngine.Object.FindObjectsOfType<JailbreakMicrogame>())
        {
            if (bomb == null) continue;
            int id = bomb.GetInstanceID();
            if (bomb.IsActive && _defusedNightclubBombs.Add(id))
                bomb.EndMicrogame(true);
            else if (!bomb.IsActive)
                _defusedNightclubBombs.Remove(id);
        }
    }

    private void AssignNightclubCustomers()
    {
        var rooms = UnityEngine.Object.FindObjectsOfType<RoomController>();
        var customers = UnityEngine.Object.FindObjectsOfType<NightclubCustomerController>();
        var characters = UnityEngine.Object.FindObjectsOfType<CharacterControllerBase>();

        var orderedCustomers = customers
            .Where(customer => customer != null && customer.IsActive && !customer.IsBusy && customer.AssignedRoom == null)
            .OrderBy(customer => CountCompatibleGirls(customer, characters, rooms))
            .ThenBy(customer => customer.PatiencePercentage)
            .ToArray();

        foreach (var customer in orderedCustomers)
        {
            RoomController bestRoom = null;
            CharacterControllerBase bestCharacter = null;
            CharacterRoomExtension bestExtension = null;
            float bestScore = float.MinValue;

            foreach (var room in rooms)
            {
                if (room == null || !room.IsReady || room.CharacterAssigned || room.CustomerAssigned ||
                    room.IsDirty || room.IsBroken || room.IsDestroyed)
                    continue;

                foreach (var character in characters)
                {
                    if (character == null || !customer.AllowAssignment(character.Character, room.Position))
                        continue;

                    CharacterRoomExtension extension = null;
                    foreach (var candidate in character._extensions)
                    {
                        extension = candidate.TryCast<CharacterRoomExtension>();
                        if (extension != null) break;
                    }
                    if (extension == null || !extension.CanAssign || extension.IsAssigned || extension.IsBusy)
                        continue;

                    // Prefer exact requests, but spread work toward the least excited girl.
                    float score = -extension.Excitement * 10f;
                    if (customer.CharacterPreference == character.Character) score += 4;
                    if (customer.RoomPreference == room.Position) score += 2;
                    if (score <= bestScore) continue;
                    bestScore = score;
                    bestRoom = room;
                    bestCharacter = character;
                    bestExtension = extension;
                }
            }

            if (bestRoom == null || bestCharacter == null || bestExtension == null)
                continue;

            bestRoom.TryAssignCharacter(bestExtension);
            bestRoom.TryAssignCustomer(customer);
        }
    }

    private static int CountCompatibleGirls(NightclubCustomerController customer,
        CharacterControllerBase[] characters, RoomController[] rooms)
    {
        int count = 0;
        foreach (var character in characters)
        {
            if (character == null) continue;
            bool allowed = false;
            foreach (var room in rooms)
            {
                if (room != null && customer.AllowAssignment(character.Character, room.Position))
                {
                    allowed = true;
                    break;
                }
            }
            if (allowed) count++;
        }
        return count;
    }

    private void ProtectNightclubOffice()
    {
        bool threatLeft = false;
        bool threatRight = false;
        foreach (var character in UnityEngine.Object.FindObjectsOfType<NightclubCharacterController>())
        {
            if (character == null || !character.IsExcited) continue;
            var position = (NightclubPositions)character.CurrentPosition;
            threatLeft |= position == NightclubPositions.OfficeDoorLeft || position == NightclubPositions.OfficeLeftNext;
            threatRight |= position == NightclubPositions.OfficeDoorRight || position == NightclubPositions.OfficeRightNext;
        }

        foreach (var door in UnityEngine.Object.FindObjectsOfType<NightclubSecurityDoor>())
        {
            if (door == null) continue;
            bool threatened = door.doorPosition == NightclubDoorPosition.Left ? threatLeft : threatRight;
            if (door.DoorOpen == threatened)
                door.SetOpenState(!threatened);
        }
    }

    private void DrawMiscTab()
    {
        GUILayout.Label("EXTRA", _titleStyle);
        _unlockedMedia = ToggleRow("Unlock all media", _unlockedMedia);
        _unlockedPlushies = ToggleRow("Unlock all plushies", _unlockedPlushies);
        MediaOverride = _unlockedMedia;
        PlushieOverride = _unlockedPlushies;
        GUILayout.Space(12f);
        GUILayout.Label("GALLERY", _titleStyle);
        _unlockedGallery = ToggleRow("Unlock all animations and characters", _unlockedGallery);
        GalleryOverride = _unlockedGallery;
        GUILayout.Space(12f);
        GUILayout.Label("PERFORMANCE", _titleStyle);
        bool previousUnlocker = _fpsUnlocker;
        _fpsUnlocker = ToggleRow("Unlock FPS", _fpsUnlocker);
        _showFps = ToggleRow("Show FPS counter", _showFps);
        _showModeLevel = ToggleRow("Show game mode and level", _showModeLevel);
        _showNightClock = ToggleRow("Show night clock", _showNightClock);
        _showSurvivalRemaining = ToggleRow("Show survival time remaining", _showSurvivalRemaining);
        if (previousUnlocker && !_fpsUnlocker)
        {
            Application.targetFrameRate = _originalTargetFrameRate;
            QualitySettings.vSyncCount = _originalVSync;
        }
        GUILayout.Space(12f);
        GUILayout.Label("UNLOCK / LOCK NIGHTS", _titleStyle);
        DrawProgressButtons("Security Office", GameLevel.SecurityOffice);
        DrawProgressButtons("Nightclub", GameLevel.Nightclub);
        DrawProgressButtons("Arcade", GameLevel.Arcade);
        GUILayout.Space(8f);
        GUILayout.Label("Reopen the relevant game menu after changing unlocks.", _statusStyle);
    }

    private void DrawProgressButtons(string label, GameLevel mode)
    {
        GUILayout.Label(label, _toggleLabelStyle);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Unlock all nights / boss", GUILayout.Height(28f)))
            SetModeProgress(mode, true);
        if (GUILayout.Button("Lock to first night", GUILayout.Height(28f)))
            SetModeProgress(mode, false);
        GUILayout.EndHorizontal();
    }

    private void SetModeProgress(GameLevel mode, bool unlockAll)
    {
        var save = SaveManager.Save;
        if (save == null) return;
        var levels = Resources.FindObjectsOfTypeAll<LevelConfigBase>()
            .Where(level => level != null && level.Level == mode && !string.IsNullOrWhiteSpace(level.GUID))
            .GroupBy(level => level.GUID)
            .Select(group => group.First())
            .OrderBy(level => level.levelNumber)
            .ToArray();
        if (levels.Length == 0) return;

        foreach (var level in levels)
        {
            while (save.UnlockedLevels.Contains(level.GUID)) save.UnlockedLevels.Remove(level.GUID);
            while (save.CompletedLevels.Contains(level.GUID)) save.CompletedLevels.Remove(level.GUID);
            foreach (var flag in level.progressFlags)
                while (save.ProgressFlags.Contains(flag)) save.ProgressFlags.Remove(flag);
        }

        if (unlockAll)
        {
            foreach (var level in levels)
                if (!save.UnlockedLevels.Contains(level.GUID)) save.UnlockedLevels.Add(level.GUID);
        }
        else
        {
            var first = levels.FirstOrDefault(level => level.levelNumber <= 1) ?? levels[0];
            if (!save.UnlockedLevels.Contains(first.GUID)) save.UnlockedLevels.Add(first.GUID);
        }

        SaveManager.SaveSaveState();
        LoggerInstance.Msg($"{mode}: {(unlockAll ? "all nights unlocked" : "locked to first night")}.");
    }

    private string GetModeLevelText()
    {
        string mode = LevelManagerBase.Level.ToString().Replace("SecurityOffice", "Security Office");
        var config = LevelManagerBase.LevelConfig;
        string level = config.useSpecialLevelName && !string.IsNullOrWhiteSpace(config.specialLevelName)
            ? config.specialLevelName
            : $"Night {config.levelNumber}";
        return $"{mode}  •  {level}";
    }

    private string GetNightClockText()
    {
        int hour = _levelTimeManager.CurrentHour;
        int minute = 0;
        if (_levelTimeManager.TotalHours > 0 && _levelTimeManager.levelDuration > 0)
        {
            float secondsPerHour = (float)_levelTimeManager.levelDuration / _levelTimeManager.TotalHours;
            minute = Mathf.Clamp((int)((_levelTimeManager.elapsedTime % secondsPerHour) / secondsPerHour * 60f), 0, 59);
        }
        return $"NIGHT TIME  {hour:00}:{minute:00}";
    }

    private string GetRemainingText()
    {
        float remaining = Mathf.Max(0f, _levelTimeManager.levelDuration - _levelTimeManager.elapsedTime);
        int minutes = (int)remaining / 60;
        int seconds = (int)remaining % 60;
        return $"SURVIVE FOR  {minutes:00}:{seconds:00}";
    }

    private void DrawDatingTab()
    {
        GUILayout.Label("DATING SIM", _titleStyle);
        _unlockedDating = ToggleRow("Unlock all girls", _unlockedDating);
        DatingOverride = _unlockedDating;
        _maxedAffection = ActionRow("Maximum affection", _maxedAffection, MaxDatingAffection);
        GUILayout.Space(8f);
        GUILayout.Label("Open Dating once before using maximum affection.", _statusStyle);
    }

    private bool ActionRow(string label, bool completed, System.Action action)
    {
        GUILayout.BeginHorizontal();
        bool clicked = GUILayout.Button(completed ? "✓" : "", completed ? _toggleOnStyle : _toggleOffStyle,
            GUILayout.Width(28f), GUILayout.Height(27f));
        if (GUILayout.Button(label, _toggleLabelStyle, GUILayout.Height(27f))) clicked = true;
        GUILayout.EndHorizontal();
        if (!clicked || completed) return completed;

        try
        {
            action();
            return true;
        }
        catch (Exception exception)
        {
            LoggerInstance.Error($"{label} failed: {exception}");
            return false;
        }
    }

    private static void MaxDatingAffection()
    {
        foreach (var titleCharacter in Resources.FindObjectsOfTypeAll<TitleCharacter>())
        {
            if (titleCharacter == null || titleCharacter.DatingData == null || titleCharacter.DatingConfig == null)
                continue;
            int rankCount = titleCharacter.DatingConfig.DatingRanks.Count;
            if (rankCount < 1) continue;
            titleCharacter.DatingData.currentRank = rankCount;
            titleCharacter.DatingData.currentXP = titleCharacter.DatingConfig.DatingRanks[rankCount - 1].requiredXP;
            titleCharacter.DatingData.unlockedPreference1 = true;
            titleCharacter.DatingData.unlockedPreference2 = true;
            titleCharacter.DatingData.unlockedPreference3 = true;
        }
        SaveManager.SaveSaveState();
    }

    private void EnsureStyles()
    {
        if (_titleStyle != null)
            return;

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 0.38f, 0.72f) }
        };
        _statusStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            normal = { textColor = new Color(0.78f, 0.78f, 0.84f) }
        };
        _panelStyle = new GUIStyle(GUI.skin.window)
        {
            padding = new RectOffset(14, 14, 26, 12),
            normal = { background = SolidTexture(new Color(0.045f, 0.03f, 0.065f, 0.84f)), textColor = Color.white }
        };
        _tabStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 11,
            fixedHeight = 28f,
            normal = { background = SolidTexture(new Color(0.12f, 0.09f, 0.15f, 0.72f)), textColor = new Color(0.72f, 0.68f, 0.76f) }
        };
        _activeTabStyle = new GUIStyle(_tabStyle)
        {
            normal = { background = SolidTexture(new Color(0.75f, 0.16f, 0.45f, 0.9f)), textColor = Color.white }
        };
        _toggleOffStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter,
            normal = { background = SolidTexture(new Color(0.16f, 0.14f, 0.19f, 0.9f)), textColor = Color.white }
        };
        _toggleOnStyle = new GUIStyle(_toggleOffStyle)
        {
            normal = { background = SolidTexture(new Color(0.20f, 0.80f, 0.54f, 0.96f)), textColor = Color.white }
        };
        _toggleLabelStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 12,
            padding = new RectOffset(9, 6, 3, 3),
            normal = { background = SolidTexture(new Color(0.10f, 0.075f, 0.125f, 0.74f)), textColor = new Color(0.94f, 0.92f, 0.96f) }
        };
        _fpsStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleRight,
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.25f, 1f, 0.62f) }
        };
        _hudCenterStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
    }

    private static Texture2D SolidTexture(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}

[HarmonyPatch(typeof(ItemData), "get_IsUnlocked")]
internal static class ItemUnlockedPatch
{
    private static void Postfix(ItemData __instance, ref bool __result)
    {
        if ((__instance.Type == ItemType.Plushie && MenuMod.PlushieOverride) ||
            (__instance.Type == ItemType.Media && MenuMod.MediaOverride))
            __result = true;
    }
}

[HarmonyPatch(typeof(ItemData), "get_IsAvailable")]
internal static class ItemAvailablePatch
{
    private static void Postfix(ItemData __instance, ref bool __result)
    {
        if ((__instance.Type == ItemType.Plushie && MenuMod.PlushieOverride) ||
            (__instance.Type == ItemType.Media && MenuMod.MediaOverride))
            __result = true;
    }
}

[HarmonyPatch(typeof(MediaElement), nameof(MediaElement.IsUnlocked))]
internal static class MediaUnlockedPatch
{
    private static void Postfix(ref bool __result)
    {
        if (MenuMod.MediaOverride) __result = true;
    }
}

[HarmonyPatch(typeof(GalleryManager), nameof(GalleryManager.IsLevelUnlocked))]
internal static class GalleryUnlockedPatch
{
    private static void Postfix(ref bool __result)
    {
        if (MenuMod.GalleryOverride) __result = true;
    }
}

[HarmonyPatch(typeof(CharacterDatingConfig), "get_IsCharacterUnlocked")]
internal static class DatingConfigUnlockedPatch
{
    private static void Postfix(ref bool __result)
    {
        if (MenuMod.DatingOverride) __result = true;
    }
}

[HarmonyPatch(typeof(TitleCharacter), "get_IsDatingUnlocked")]
internal static class DatingCharacterUnlockedPatch
{
    private static void Postfix(ref bool __result)
    {
        if (MenuMod.DatingOverride) __result = true;
    }
}
