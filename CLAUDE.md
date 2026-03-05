# CLAUDE.md — ProjectFirst

## Project Overview

**ProjectFirst** is a Unity 3D Idle Defence game where enemy waves are defeated automatically by the player's units/towers. The player focuses on skill timing, tower placement, and upgrades rather than direct combat.

- **Engine**: Unity 2022 LTS+ with URP (Universal Render Pipeline)
- **Language**: C#
- **Data layer**: ScriptableObject-based tables (no external DB)

---

## Scenes

| Scene | Purpose |
|---|---|
| `Bootstrap` | Entry point; initializes services and loads the next scene |
| `Title` | Main menu / title screen |
| `Battle_Test` | Primary combat scene for testing and gameplay |

---

## Folder Structure

```
Assets/Project/
  Scripts/
    Agent/       # Player-controlled agent logic (Agent.cs, buffs, skills, timeline)
    Bootstrap/   # App startup (BootstrapManager.cs)
    Core/        # Shared systems: health, damage, scene loading, session, game manager
    Data/        # ScriptableObject row/table definitions (no MonoBehaviour)
    Editor/      # Unity editor tooling
    Enemy/       # Enemy AI, pooling, spawning, wave management
    Events/      # Event channel SOs (VoidEventChannelSO, etc.)
    Systems/     # Cross-cutting systems: BGM, damage calc, skill system, run session
    UI/          # All UI controllers and views
  Data/          # ScriptableObject asset instances
  Prefabs/
  Scenes/
  Art/
  Fonts/
  Materials/
  Sound/
```

---

## Key Systems

### Combat
- `BattleGameManager` — orchestrates battle start/end, win/loss conditions
- `WaveManager` — drives wave progression using `WaveTable` / `WaveRow` data
- `EnemySpawner` — spawns enemies from `EnemyPool` at set intervals
- `EnemyPool` — object pool; use `Return()` instead of `Destroy()` on death
- `EnemyManager` — tracks `activeEnemies`; enemies register/deregister here
- `EnemyController` — per-enemy movement, attack, death FSM
- `DamageCalculator` — centralised damage formula (handles `ElementType`, `MonsterGrade`)

### Agent (Player Unit)
- `Agent` — player unit base; references `AgentData` / `AgentStatsTable`
- `AgentBuffSystem` — applies/removes timed buffs
- `SkillSystem` — activates skills defined in `SkillTable` / `SkillRow`
- `SkillEffectTrigger` — links timeline signals to skill effects

### Data Tables (ScriptableObjects)
| Table | Row | Description |
|---|---|---|
| `AgentTable` | `AgentRow` | Agent base stats |
| `AgentStatsTable` | `AgentStatsRow` | Per-level stat curves |
| `MonsterTable` | `MonsterRow` | Enemy HP, speed, reward, grade |
| `WaveTable` | `WaveRow` | Wave composition and spawn intervals |
| `SkillTable` | `SkillRow` | Skill definitions and effect types |
| `HitEffectTable` | — | VFX/SFX mapping for hit effects |

### UI
- `BattleHUD` — in-battle overlay (wave counter, status)
- `StatusHudView` — agent HP/stats display
- `SkillBarController` / `SkillButtonSlot` — skill cooldown buttons
- `SkillSelectPanelController` — pre-battle skill selection
- `DamageText` — pooled floating damage numbers
- `ResultPanelManager` — win/loss result screen
- `ArkBaseHpBarView` — boss/base HP bar

### Events
- `VoidEventChannelSO` — zero-argument ScriptableObject event bus
- Persistent event assets: `StartEvent`, `QuitEvent`, `SettingEvent`

---

## Package Stack (확정 목록 — 구현 시 우선 활용)

### Third-Party
| 패키지 | 버전 | 활용 시스템 |
|---|---|---|
| **UniTask** (Cysharp) | 2.5.10 Git | AsyncSceneLoader, AgentBuffSystem, BGM 페이드 등 비동기 처리 |
| **Motion Blur** (OccaSoftware) | 3.2.1 Custom | URP Post-processing 카메라 연출 |

### Unity Packages (주요)
| 패키지 | 버전 | 활용 시스템 |
|---|---|---|
| **Addressables** | 1.22.3 | 에셋 비동기 로드 (프리팹, 사운드, VFX) |
| **AI Navigation** | 1.1.7 | 적 이동 경로(NavMesh) |
| **Burst** | 1.8.21 | 연산 집약 로직 성능 최적화 |
| **Cinemachine** | 2.10.6 | 카메라 연출 (스킬 컷씬, 결과 화면) |
| **Mathematics** | 1.2.6 | Burst 연동 고성능 수학 연산 |
| **Newtonsoft Json** | 3.2.2 | 세이브 데이터 직렬화 |
| **Post Processing** | 3.4.0 | 화면 이펙트 (Bloom, Vignette 등) |
| **TextMeshPro** | 3.0.9 | DamageText, BattleHUD, UI 전반 |
| **Timeline** | 1.7.7 | SkillEffectTrigger, 스킬 컷씬 |
| **Universal RP** | 14.0.12 | 렌더 파이프라인 (URP) |
| **Shader Graph** | 14.0.12 | 커스텀 셰이더 (적 사망 이펙트 등) |
| **Settings Manager** | 2.1.1 | 게임 설정 저장/불러오기 |

### 패키지 활용 원칙
- **비동기 처리**: 코루틴 신규 작성 금지 → `UniTask` + `async/await` 사용
- **씬 전환**: `AsyncSceneLoader` 유지, 내부는 UniTask로 전환 가능
- **카메라**: Cinemachine Virtual Camera 기반으로 연출 구성
- **적 이동**: AI Navigation(NavMesh)으로 경로 계산
- **에셋 로드**: 런타임 동적 로드는 Addressables 사용
- **UI 텍스트**: 반드시 TextMeshPro 사용 (Legacy Text 금지)
- **수학/최적화**: 벡터/행렬 연산은 `Unity.Mathematics` + Burst 고려

---

## Coding Conventions

- **Namespace**: none enforced project-wide; follow existing file conventions
- **Data vs Logic**: keep all tunable numbers in ScriptableObject tables; scripts read from them, never hard-code values
- **Object pooling**: always pool enemies via `EnemyPool`; call `pool.Return(enemy)` on death, never `Destroy`
- **Scene management**: use `AsyncSceneLoader` for all scene transitions
- **Events**: prefer `VoidEventChannelSO` for decoupled cross-system communication
- **Element/Grade enums**: use `ElementType` and `MonsterGrade` for damage and classification logic

---

## Enemy Pooling — Test Checklist

Use this when validating the enemy pool under load (300+ spawns):

1. Add `EnemyPool` component to a GameObject in `Battle_Test` scene
2. Assign `Enemy01` prefab to `EnemyPool.enemyPrefab`; set `initialCapacity = 60–100`, `allowExpand = true`
3. Connect `EnemySpawner.enemyPool`; assign `arkTarget` and `spawnPoints`
4. Set `EnemySpawner.spawnInterval = 0.03–0.05` for stress testing
5. Enter Play mode — confirm no console errors
6. Verify `Enemy(Clone)` count stabilises (reuse) rather than growing linearly
7. Confirm death triggers `Return()` not `Destroy()`
8. Check `EnemyManager.activeEnemies` registers/deregisters correctly
9. After 300+ cumulative spawns, confirm move/attack/death loop remains stable
