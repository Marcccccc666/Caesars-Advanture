using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class Level2BossRoomController : BossRoomController
{
    [SerializeField, ChineseListLabel("前置普通房间")] private NormalRoomController[] prerequisiteRooms;
    [Header("通关出口")]
    [SerializeField, ChineseLabel("下一关对象")] private GameObject nextLevelEntrance;

    [Header("Boss 显现镜头")]
    [SerializeField, ChineseLabel("Boss 显现相机")] private CinemachineCamera bossRevealCamera;
    [SerializeField, ChineseLabel("镜头聚焦点")] private Transform bossRevealFocusPoint;
    [SerializeField, Min(0f), ChineseLabel("镜头移动时长")] private float cameraMoveDuration = 1f;
    [SerializeField, Min(0f), ChineseLabel("镜头停留时长")] private float cameraHoldDuration = 1f;
    [SerializeField, ChineseLabel("镜头期间暂停游戏")] private bool pauseGameDuringReveal = true;
    [SerializeField, ChineseLabel("镜头期间禁用玩家输入")] private bool disablePlayerInputDuringReveal = true;

    [Header("中部墙体")]
    [SerializeField, ChineseListLabel("Boss 显现时隐藏的墙体")] private GameObject[] wallsToHide;
    [SerializeField, ChineseLabel("隐藏后失活墙体对象")] private bool deactivateWallsAfterHidden = true;

    private readonly HashSet<NormalRoomController> subscribedRooms = new();
    private bool allPrerequisiteRoomsCleared = false;
    private bool bossUnlocked = false;
    private bool bossRevealTriggered = false;
    private bool wallsHidden = false;
    private bool revealPauseApplied = false;
    private bool revealPlayerControlLockApplied = false;
    private bool revealVisualSequenceApplied = false;
    private bool revealCameraBindingOverridden = false;
    private Transform cachedRevealCameraFollow;
    private Transform cachedRevealCameraLookAt;

    private CameraManager cameraManager => CameraManager.Instance;
    private GameManager gameManager => GameManager.Instance;

    private void Start()
    {
        SetBossVisible(false);
        SetNextLevelEntranceVisible(false);
        RefreshPrerequisiteState();
    }

    protected override void OnBossDie()
    {
        base.OnBossDie();
        SetNextLevelEntranceVisible(true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SubscribePrerequisiteRooms();
    }

    protected override void OnDisable()
    {
        RestoreRevealRuntimeState();
        RestoreBossRevealCameraBinding();
        UnsubscribePrerequisiteRooms();
        base.OnDisable();
    }

    protected override bool CanStartBattleOnPlayerEnter()
    {
        return bossUnlocked;
    }

    protected override void OnBlockedPlayerEnter()
    {
        TryRevealBoss();
    }

    private void SubscribePrerequisiteRooms()
    {
        subscribedRooms.Clear();

        for (int i = 0; i < prerequisiteRooms.Length; i++)
        {
            NormalRoomController room = prerequisiteRooms[i];
            if (room == null || !subscribedRooms.Add(room))
                continue;

            room.RoomClearedAction += OnPrerequisiteRoomCleared;
        }
    }

    private void UnsubscribePrerequisiteRooms()
    {
        foreach (NormalRoomController room in subscribedRooms)
        {
            if (room == null)
                continue;

            room.RoomClearedAction -= OnPrerequisiteRoomCleared;
        }

        subscribedRooms.Clear();
    }

    private void OnPrerequisiteRoomCleared(BattleRoomController _)
    {
        RefreshPrerequisiteState();
    }

    private void RefreshPrerequisiteState()
    {
        bool hasValidRoom = false;
        bool isAllCleared = true;

        for (int i = 0; i < prerequisiteRooms.Length; i++)
        {
            NormalRoomController room = prerequisiteRooms[i];
            if (room == null)
                continue;

            hasValidRoom = true;

            if (!room.IsRoomCleared)
            {
                isAllCleared = false;
                break;
            }
        }

        allPrerequisiteRoomsCleared = !hasValidRoom || isAllCleared;
    }

    private void TryRevealBoss()
    {
        if (bossRevealTriggered || bossUnlocked || !allPrerequisiteRoomsCleared)
            return;

        bossRevealTriggered = true;
        ApplyRevealRuntimeState();
        PlayBossRevealCamera();
    }

    private void PlayBossRevealCamera()
    {
        if (bossRevealCamera == null || cameraManager == null)
        {
            ApplyBossRevealVisualSequence();
            OnBossRevealSequenceFinished();
            return;
        }

        OverrideBossRevealCameraBinding();
        cameraManager.PlayTemporaryCamera(bossRevealCamera, cameraMoveDuration, cameraHoldDuration, false, ApplyBossRevealVisualSequence, OnBossRevealSequenceFinished);
    }

    private void OnBossRevealSequenceFinished()
    {
        RestoreBossRevealCameraBinding();
        RestoreRevealRuntimeState();

        bossUnlocked = true;

        if (HasPendingPlayerEnter)
        {
            StartBattleRoomFight();
        }
    }

    private void ApplyBossRevealVisualSequence()
    {
        if (revealVisualSequenceApplied)
            return;

        revealVisualSequenceApplied = true;
        HideConfiguredWalls();
        SetBossVisible(true);
    }

    private void ApplyRevealRuntimeState()
    {
        if (gameManager == null)
            return;

        if (pauseGameDuringReveal && !gameManager.IsGamePaused)
        {
            gameManager.SetGamePaused(true);
            revealPauseApplied = true;
        }

        if (disablePlayerInputDuringReveal)
        {
            gameManager.SetPlayerControlLocked(true);
            revealPlayerControlLockApplied = true;
        }
    }

    private void RestoreRevealRuntimeState()
    {
        if (gameManager == null)
            return;

        if (revealPlayerControlLockApplied)
        {
            gameManager.SetPlayerControlLocked(false);
            revealPlayerControlLockApplied = false;
        }

        if (revealPauseApplied)
        {
            gameManager.SetGamePaused(false);
            revealPauseApplied = false;
        }
    }

    private void HideConfiguredWalls()
    {
        if (wallsHidden)
            return;

        wallsHidden = true;

        for (int i = 0; i < wallsToHide.Length; i++)
        {
            GameObject wall = wallsToHide[i];
            if (wall == null)
                continue;

            Renderer[] renderers = wall.GetComponentsInChildren<Renderer>(true);
            for (int j = 0; j < renderers.Length; j++)
            {
                renderers[j].enabled = false;
            }

            Collider2D[] colliders2D = wall.GetComponentsInChildren<Collider2D>(true);
            for (int j = 0; j < colliders2D.Length; j++)
            {
                colliders2D[j].enabled = false;
            }

            Collider[] colliders3D = wall.GetComponentsInChildren<Collider>(true);
            for (int j = 0; j < colliders3D.Length; j++)
            {
                colliders3D[j].enabled = false;
            }

            if (deactivateWallsAfterHidden)
            {
                wall.SetActive(false);
            }
        }
    }

    private void SetNextLevelEntranceVisible(bool visible)
    {
        if (nextLevelEntrance == null)
            return;

        nextLevelEntrance.SetActive(visible);
    }

    private void OverrideBossRevealCameraBinding()
    {
        if (bossRevealCamera == null || bossRevealFocusPoint == null || revealCameraBindingOverridden)
            return;

        cachedRevealCameraFollow = bossRevealCamera.Follow;
        cachedRevealCameraLookAt = bossRevealCamera.LookAt;
        bossRevealCamera.Follow = bossRevealFocusPoint;
        bossRevealCamera.LookAt = bossRevealFocusPoint;
        revealCameraBindingOverridden = true;
    }

    private void RestoreBossRevealCameraBinding()
    {
        if (!revealCameraBindingOverridden || bossRevealCamera == null)
            return;

        bossRevealCamera.Follow = cachedRevealCameraFollow;
        bossRevealCamera.LookAt = cachedRevealCameraLookAt;
        cachedRevealCameraFollow = null;
        cachedRevealCameraLookAt = null;
        revealCameraBindingOverridden = false;
    }
}