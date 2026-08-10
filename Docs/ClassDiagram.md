# 📐 클래스 다이어그램: 풍림화산 전쟁 (Elemental War)

> 실제 구현 코드 기준 구조도입니다.
> 가독성을 위해 **시스템별로 다이어그램을 분리**했으며, 각 다이어그램에는 핵심 멤버만 표기했습니다.

**설계의 세 가지 축**

1. **씬 단위 MVC 분리**: 5개 씬 전체에서 `BaseSceneController` / `BaseUIManager` / `NetworkManager`가 각각 로직, 화면, 통신만 담당
2. **패턴 기반 확장**: Composite, State, Adapter, Strategy, Registry를 문제 상황별로 선택 적용해 기능 추가 시 기존 코드 수정 최소화(OCP)
3. **네트워크 경계의 국소화**: 게임 로직은 네트워크를 모르고, `UnitNetworkSync` · `NetworkPoolManager`만 Photon PUN2에 의존

### 📑 목차

| # | 다이어그램 | 내용 |
|---|---|---|
| 0 | [전체 구조 개요](#0-전체-구조-개요) | 시스템 간 의존 관계 맵 |
| 1 | [Core 공통 기반](#1-core-공통-기반) | `Singleton<T>`, `BaseSceneController<T>`, `BaseUIManager<T>`, `UIPanel` |
| 2 | [씬 MVC 구조](#2-씬-mvc-구조) | 5개 씬의 Controller, View, Network 3계층 |
| 3 | [유닛 시스템 (Component)](#3-유닛-시스템-component) | `Unit` 퍼사드 & 기능별 컴포넌트 |
| 4 | [유닛 상태 머신 (State)](#4-유닛-상태-머신-state) | `IUnitState` 5종 & 상태 전이 |
| 5 | [애니메이터 계층 (Adapter)](#5-애니메이터-계층-adapter) | 3종 서드파티 에셋 통합 |
| 6 | [네트워크 & 오브젝트 풀](#6-네트워크--오브젝트-풀) | `IPunPrefabPool` 및 RPC 동기화 |
| 7 | [데이터 계층](#7-데이터-계층) | `UnitStat`, `UnitDatabase`, `DeckModel` |
| 8 | [UI 계층 (Composite)](#8-ui-계층-composite) | Manager → Container → Item |
| 9 | [채팅 시스템 (Strategy)](#9-채팅-시스템-strategy) | `IChatTransport` 교체형 전송 |
| 10 | [맵 · 성 · 대포 · 에너지](#10-맵--성--대포--에너지) | 전투 자원과 승패 판정 |
| 11 | [프로파일링 계층](#11-프로파일링-계층-계측-빌드-전용) | `ENABLE_PROFILING` 전용 계측 인프라 |
| 12 | [이벤트 & RPC 흐름](#12-이벤트--rpc-흐름) | 발행/구독 및 네트워크 전파 경로 |

---

## 0. 전체 구조 개요

세부 클래스는 생략하고 **시스템 단위의 의존 방향**만 표현했습니다.

```mermaid
flowchart TD
    subgraph BOOT["🚀 부트스트랩"]
        Z1["GameBootstrap<br/>전역 매니저 생성 · DontDestroyOnLoad"]
    end

    subgraph CORE["🧩 Core 공통 기반"]
        A1["Singleton&lt;T&gt;"]
        A2["BaseSceneController&lt;T&gt;<br/>BaseUIManager&lt;T&gt;"]
        A3["UIPanel · BasePopupPanel"]
    end

    subgraph SCENE["🎬 씬 MVC (5개 씬)"]
        B1["MainMenu · Lobby · Room<br/>UnitSetting · Game"]
        B2["Controller / View / Network"]
    end

    subgraph UNIT["⚔️ 유닛 시스템"]
        C1["Unit (퍼사드)"]
        C2["UnitStats · UnitMovement<br/>UnitCombat · UnitStateMachine"]
        C3["IUnitState 5종"]
    end

    subgraph ANIM["🎞️ 애니메이터 어댑터"]
        D1["IUnitAnimator"]
        D2["BaseUnitAnimator"]
        D3["HeroEditor · FantasyMonster · Spine"]
    end

    subgraph NET["🌐 네트워크"]
        E1["UnitNetworkSync"]
        E2["NetworkPoolManager<br/>(IPunPrefabPool)"]
        E3["Photon PUN2 · Chat · PlayFab"]
    end

    subgraph DATA["📦 데이터"]
        F1["UnitStat · UnitDatabase"]
        F2["DeckModel · GameStateModel"]
    end

    subgraph BATTLE["🏰 맵 · 전투 자원"]
        G0["MapManager · MapSpawner"]
        G1["Castle · Cannonball"]
        G2["EnergyManager"]
    end

    subgraph COMMON["🔧 공통 서비스"]
        H1["SoundManager · PopupPanelUIManager"]
        H2["UnitRegistry · InputGate"]
    end

    subgraph PROF["📈 계측 (ENABLE_PROFILING)"]
        P1["ProfilingSwitches · ProfilingCounters"]
        P2["NetworkPerformanceLogger<br/>ProfilingScenarioRunner · FPSOverlay"]
    end

    Z1 --> H1
    A1 --> A2
    A2 --> B2
    A3 --> B2
    B1 --> B2
    B2 --> C1
    B2 --> G0
    B2 --> G2
    C1 --> C2
    C2 --> C3
    C1 --> D1
    D1 --> D2
    D2 --> D3
    C1 --> E1
    E1 --> E3
    E2 --> E3
    B2 --> E2
    F1 --> C2
    F1 --> B2
    F2 --> B2
    G0 --> G1
    G1 --> C1
    G2 --> B2
    C1 -. 등록 .-> H2
    B2 --> H1
    P1 -. 스위치 .-> C2
    P1 -. 스위치 .-> E2
    P2 -. 수집 .-> P1

    style UNIT fill:#e8f4ff,stroke:#4a90d9
    style NET fill:#ffe8e8,stroke:#d95a5a
    style CORE fill:#fff6e5,stroke:#e0a030
    style PROF fill:#eaffea,stroke:#4caf50
```

---

## 1. Core 공통 기반

모든 씬이 공유하는 기반 계층입니다. **Template Method 패턴**으로 초기화, 구독, 해제 순서를 강제해, 씬이 늘어나도 생명주기 처리 누락이 발생하지 않도록 했습니다.

```mermaid
classDiagram
    class GameBootstrap {
        <<MonoBehaviour>>
        -GameObject[] persistentManagerPrefabs
        -string firstSceneName
        -static bool isBootstrapped
        -static ResetStatics()$
        -Awake()
        -static SetRunInBackground()
        -SpawnPersistentManagers()
        -LoadFirstScene()
    }
    note for GameBootstrap "RuntimeInitializeOnLoadMethod로 정적 플래그 초기화 도메인 리로드 비활성화 대응"

    class Singleton_T {
        <<Abstract MonoBehaviour>>
        -static T instance
        +static T Instance$
        +static bool HasInstance$
        #virtual Awake()
        #virtual OnDestroy()
    }
    note for Singleton_T "파생 클래스는 base.Awake() 직후 Instance != this 가드로 중복 인스턴스의 초기화를 차단"

    class BaseSceneController_T {
        <<Abstract · Template Method>>
        #virtual Start()
        #override OnDestroy()
        #virtual SetCachedVariable()
        #virtual SubscribeEvents()
        #abstract SetUIManager()
        #abstract SetNetworkManager()
        #abstract PlayBGM()
        #abstract InitializeState()
        #virtual UnsubscribeAll()
        #abstract ResetUIManager()
        #abstract ResetNetworkManager()
    }
    note for BaseSceneController_T "Start()에서 캐싱 → 구독 → BGM → 초기화 순서를 고정해 씬별 누락 방지"

    class BaseUIManager_T {
        <<Abstract · Template Method>>
        #virtual Start()
        #override OnDestroy()
        #abstract InitUIElements()
        #virtual BindUIEvents()
        #abstract BindButtonEvent()
        #abstract BindPanelEvent()
        #virtual UnbindUIEvents()
        #virtual UnbindButtonEvent()
        #virtual UnbindPanelEvent()
        #PlayButtonSound()
    }

    class UIPanel {
        <<Abstract · RequireComponent CanvasGroup>>
        -float animationDuration
        -Vector3 hiddenScale
        -Coroutine animationCoroutine
        #Transform contentTransform
        #CanvasGroup canvasGroup
        -Transform ContentTransform
        #virtual Awake()
        #virtual Start()
        #virtual OnDestroy()
        #abstract RegisterListener()
        #virtual UnregisterListener()
        #abstract ResetUI()
        +virtual Show()
        +virtual Hide()
        +virtual HideImmediate()
        -Animate(targetAlpha, targetScale) IEnumerator
        -static EaseOut(t) float
    }
    note for UIPanel "Time.unscaledDeltaTime 기반 연출 게임 종료 timeScale=0 상태에서도 동작"

    class BasePopupPanel {
        <<Abstract>>
        #Text messageText
        #virtual ShowPopup(message)
        +virtual SetMessage(message)
    }

    class InputGate {
        <<Static>>
        -static int cachedFrame
        -static bool cachedResult
        +static bool IsBlocked$
        -static IsInputFieldFocused() bool
    }
    note for InputGate "프레임 단위 캐싱으로 중복 EventSystem 조회 제거"

    Singleton_T <|-- BaseSceneController_T
    Singleton_T <|-- BaseUIManager_T
    UIPanel <|-- BasePopupPanel
    BaseUIManager_T ..> UIPanel : 패널 생명주기 제어
    GameBootstrap ..> Singleton_T : 전역 매니저 프리팹 생성
```

---

## 2. 씬 MVC 구조

5개 씬(MainMenu, Lobby, Room, UnitSetting, Game) 모두 동일한 3계층 구조를 따릅니다.
**Network는 Photon 콜백만 이벤트로 승격**하고, **UI는 화면 출력만** 담당하며, **로직과 상태는 Controller에만** 존재합니다.

```mermaid
flowchart LR
    subgraph MODEL["Model — Photon 콜백 승격"]
        N1["MainMenuNetworkManager"]
        N2["LobbyNetworkManager"]
        N3["RoomNetworkManager"]
        N4["UnitSettingNetworkManager"]
        N5["GameNetworkManager"]
        N6["PlayFabAuthManager"]
    end

    subgraph VIEW["View — BaseUIManager&lt;T&gt;"]
        V1["MainMenuUIManager"]
        V2["LobbyUIManager"]
        V3["RoomUIManager"]
        V4["UnitSettingUIManager"]
        V5["GameUIManager"]
        V6["EnergyUIManager · CastleUIManager"]
    end

    subgraph CTRL["Controller — BaseSceneController&lt;T&gt;"]
        C1["MainMenuManager"]
        C2["LobbyManager"]
        C3["RoomManager"]
        C4["UnitSettingManager"]
        C5["GameManager"]
        C6["EnergyManager · CastleAttackManager<br/>MapManager"]
    end

    N1 -. event .-> C1
    N6 -. event .-> C1
    N2 -. event .-> C2
    N3 -. event .-> C3
    N4 -. event .-> C4
    N5 -. event .-> C5
    V1 -. event .-> C1
    V2 -. event .-> C2
    V3 -. event .-> C3
    V4 -. event .-> C4
    V5 -. event .-> C5
    V6 -. event .-> C6
    C1 --> V1
    C2 --> V2
    C3 --> V3
    C4 --> V4
    C5 --> V5
    C6 --> V6

    style MODEL fill:#ffe8e8,stroke:#d95a5a
    style VIEW fill:#e8f4ff,stroke:#4a90d9
    style CTRL fill:#fff6e5,stroke:#e0a030
```

> **`EnergyManager` · `CastleAttackManager` · `MapManager`는 씬 컨트롤러가 아닌 Game 씬의 서브시스템**입니다.
> 캐싱 → 구독 → 초기화 → 해제 생명주기를 동일하게 요구하므로 `BaseSceneController<T>`를 재사용했으며,
> 네트워크 콜백과 BGM이 불필요한 클래스는 해당 훅을 빈 구현으로 남겼습니다.

---

## 3. 유닛 시스템 (Component)

`Unit`은 로직을 직접 구현하지 않고 **기능별 컴포넌트에 위임하는 퍼사드**입니다.
`[RequireComponent]`로 필수 컴포넌트 누락을 막고, `[DisallowMultipleComponent]`로 중복 부착을 차단합니다.

```mermaid
classDiagram
    class Unit {
        <<MonoBehaviourPun · IDamagable · IPunInstantiateMagicCallback>>
        -UnitStats stats
        -UnitMovement movement
        -UnitCombat combat
        -UnitStateMachine stateMachine
        -UnitNetworkSync networkSync
        -IUnitAnimator animator
        -const int ZOffsetSlotCount
        -const float KillRewardEnergyRate
        +float MaxHP
        +float CurrentHP
        +bool IsAlive
        +bool IsTargetable
        -InitializeComponents()
        -Update()
        +OnPhotonInstantiate(info)
        -ResetTransform()
        -HandleDied()
        -HandleTriggerKnockback()
        +ChangeState(nextState, isSync)
        +CheckValidEnemy() bool
        +FindValidEnemy() Collider2D
        +IsAttackableEnemy(col) bool
        +ApplyDamage(origin)
        +ApplyMovement() / StopMovement()
        +ApplyKnockback() / DisableAllPhysics()
        +Despawn(delay)
        +PlayAttackAnimation() float
        +RPC_SyncAnimation(stateTypeInt)
        -PlayAnimationByType(type)
        +RPC_TakeDamage(damage)
    }
    note for Unit "로직 없음 — 컴포넌트에 위임만 IsTargetable = 살아있고 피격 상태가 아닐 것"

    class UnitStats {
        <<MonoBehaviour>>
        -UnitStat baseStat
        -float maxHP / currentHP
        -float attackDamage / attackRange / aoeRadius
        -float spawnCost
        -ElementType elementType
        -bool hasTriggeredHalfHPHit
        -bool hasTriggeredQuarterHPHit
        +bool IsAlive
        +event Action OnKnockBackRequested
        +event Action OnDied
        +InitializeUnitStat()
        -ResetKnockbackFlags()
        +ApplyDamage(damage)
        -TriggerKnockback()
        +CalculateDamage(targetElementType) float
    }
    note for UnitStats "HP 절반 및 4분의 1 지점을 처음 통과할 때만 OnKnockBackRequested 발행"

    class UnitMovement {
        <<MonoBehaviour · RequireComponent Rigidbody2D, Collider2D>>
        -Rigidbody2D rb
        -Collider2D unitCollider
        -float knockbackPower / knockbackDuration
        +Collider2D UnitCollider
        +float KnockbackDuration
        +float DirectionMultiplier
        +SetDirection(multiplier)
        +ResetForReuse()
        +ApplyMovement()
        +StopMovement()
        +ApplyKnockback()
        +SetPhysics(isOwnedByLocalPlayer)
        +DisableAllPhysics()
    }
    note for UnitMovement "소유 유닛만 Dynamic 상대 유닛은 Kinematic으로 물리 연산 제외"

    class UnitCombat {
        <<MonoBehaviour>>
        -const int ScanBufferSize
        -const int AoeBufferSize
        -Collider2D[] scanBuffer
        -Collider2D[] aoeBuffer
        -List~Collider2D~ currentTargetBuffer
        +int TargetLayerMask
        +CheckValidEnemy() bool
        +FindValidEnemy() Collider2D
        -FindOverlappingEnemy(bounds) Collider2D
        -FindFrontEnemy(bounds) Collider2D
        -FindEnemyInBox(center, size) Collider2D
        -IsAttackableEnemy(IDamagable) bool
        +IsAttackableEnemy(Collider2D) bool
        -CalculateAttackBox(bounds) Bounds
        +ApplyDamage(mainTargetCollider)
        -FindAllEnemiesInAoeRange(epicenter, results)
        -ApplyDamageToTargets(targets)
        -TryGetValidEnemy(col, out view, out target) bool
        -CalculateDamage(enemy) float
    }
    note for UnitCombat "논할당 물리 질의 버퍼 재사용으로 GC 제거"

    class UnitStateMachine {
        <<MonoBehaviour>>
        -UnitStateIdle stateIdle
        -UnitStateMove stateMove
        -UnitStateAttack stateAttack
        -UnitStateHit stateHit
        -UnitStateDead stateDead
        -UnitStateType currentStateType
        -IUnitState currentState
        -Dictionary~UnitStateType,IUnitState~ stateDictionary
        +IUnitState CurrentState
        +event Action~IUnitState~ OnStateChanged
        -InitializeDictionary()
        +InitializeState()
        +UpdateState()
        +ChangeState(nextState, isSync)
        +TryGetState(type, out state) bool
    }

    class UnitNetworkSync {
        <<MonoBehaviour · RequireComponent PhotonView>>
        -IUnitAnimator unitAnimator
        +PhotonView PhotonView
        +bool IsOwnedByLocalPlayer
        +int OwnLayerMask
        +int TargetLayerMask
        -HandleStateChange(nextState)
        -BroadcastStateRpc(type)
        +ResetForReuse()
        -SetDirection()
        -SetLayer()
        -SetPhysics()
        +BroadcastAttackAnimation()
        +Despawn(delay)
        -DespawnCoroutine(delay) IEnumerator
    }
    note for UnitNetworkSync "ResetForReuse가 방향·레이어·물리를 매 소환마다 재설정 → 풀 재사용 시 이전 소유자 설정 잔존 방지"

    class UnitRegistry {
        <<Static>>
        -static HashSet~Unit~ activeUnits
        +static IReadOnlyCollection~Unit~ ActiveUnits$
        -static ResetStatics()$
        +static Register(unit)$
        +static Unregister(unit)$
        +static Clear()$
        +static CopyTo(buffer)$
    }
    note for UnitRegistry "FindObjectsOfType 대체 등록/해제 O(1)"

    class UnitGizmosDrawer {
        <<MonoBehaviour · 에디터 시각화>>
        -OnDrawGizmos()
        -DrawColliderBounds(b)
        -DrawAttackRangeBox(b)
        -DrawAoeBlastRadius(b)
    }

    Unit *-- UnitStats
    Unit *-- UnitMovement
    Unit *-- UnitCombat
    Unit *-- UnitStateMachine
    Unit *-- UnitNetworkSync
    Unit ..> UnitRegistry : OnEnable/OnDisable 등록
    UnitStateMachine ..> UnitNetworkSync : OnStateChanged 구독
    UnitGizmosDrawer ..> UnitMovement
```

---

## 4. 유닛 상태 머신 (State)

행동을 `if` / `switch`가 아닌 **독립 클래스**로 분리했습니다. 상태를 추가할 때 기존 분기문을 수정할 필요가 없습니다.
각 상태는 `MonoBehaviour`가 아닌 `[Serializable]` 순수 클래스여서 인스펙터에서 내부 값을 그대로 관찰할 수 있습니다.

```mermaid
classDiagram
    class IUnitState {
        <<Interface>>
        +UnitStateType Type
        +EnterState(unit)
        +UpdateState(unit)
        +ExitState(unit)
    }

    class UnitStateIdle {
        <<Serializable>>
        -static float ScanInterval
        -float lastScanTime
    }
    note for UnitStateIdle "0.1초 주기 스캔 타겟 유무로 Attack/Move 분기"

    class UnitStateMove {
        <<Serializable>>
        -static float ScanInterval
        -float lastScanTime
    }

    class UnitStateAttack {
        <<Serializable>>
        -enum AttackPhase
        -AttackPhase phase
        -float currentAnimDuration
        -Collider2D currentTarget
        -float phaseTimer
        -HandleFirstAttackInterval(unit)
        -StartAttack(unit)
        -HandleAttacking(unit)
        -ApplyDamage(unit)
        -HandleAttackInterval(unit)
    }
    note for UnitStateAttack "WaitingFirst → Attacking → Interval 3단계 내부 페이즈"

    class UnitStateHit {
        <<Serializable>>
        -float hitTimer
    }

    class UnitStateDead {
        <<Serializable>>
        -const float FadeOutDuration
        -const float DestroyDelay
    }

    IUnitState <|.. UnitStateIdle
    IUnitState <|.. UnitStateMove
    IUnitState <|.. UnitStateAttack
    IUnitState <|.. UnitStateHit
    IUnitState <|.. UnitStateDead
```

### 상태 전이 흐름

```mermaid
stateDiagram-v2
    [*] --> Idle : OnPhotonInstantiate
    Idle --> Attack : 사거리 내 적 존재
    Idle --> Move : 적 없음
    Move --> Attack : 스캔 중 적 발견
    Attack --> Move : 타겟 소실
    Idle --> Hit : HP 절반/4분의1 통과
    Move --> Hit : HP 절반/4분의1 통과
    Attack --> Hit : HP 절반/4분의1 통과
    Hit --> Idle : 넉백 시간 경과
    Idle --> Dead : HP 0
    Move --> Dead : HP 0
    Attack --> Dead : HP 0
    Hit --> Dead : HP 0
    Dead --> [*] : Despawn → 풀 반환
```

---

## 5. 애니메이터 계층 (Adapter)

리소스 한계로 **내부 API가 전혀 다른 3종의 서드파티 유닛 에셋**을 함께 사용해야 했습니다.
공통 인터페이스로 변환해, 유닛 시스템이 에셋 종류를 전혀 알지 못하도록 격리했습니다.

```mermaid
classDiagram
    class IUnitAnimator {
        <<Interface · Target>>
        +ResetForReuse()
        +SetDirection(lookLeft)
        +PlayIdle()
        +PlayMove()
        +PlayAttack() float
        +PlayHit()
        +PlayDead()
        +StartFadeOut(duration)
    }

    class BaseUnitAnimator {
        <<Abstract>>
        #Color flashColor
        #float flashDuration
        -Dictionary~string,float~ animationLengthCache
        #Coroutine hitCoroutine / dieCoroutine
        #virtual Awake()
        +virtual ResetForReuse()
        +virtual PlayHit()
        +virtual PlayDead()
        +virtual StartFadeOut(duration)
        #virtual FadeOutCoroutine(duration) IEnumerator
        #abstract CacheRenderers()
        #abstract ApplyFlashColor(color)
        #abstract ApplyAlpha(alpha)
        #abstract RestoreOriginalColors()
        #virtual OnPlayKnockback()
        #virtual OnPlayDeadInternal()
        #virtual OnResetForReuseInternal()
        #GetAnimationClipDuration(animator, clipName) float
    }
    note for BaseUnitAnimator "피격 플래시 · 페이드 아웃 등 공통 로직 구현 클립 길이는 Dictionary로 캐싱"

    class HeroEditorAdapter {
        <<Concrete>>
        -Character characterScript
        -Dictionary originalColors
        -Dictionary originalMaterials
        -static Material defaultSpriteMaterial
        -ToggleSpriteMasks(isOn)
        -ApplyDefaultMaterial()
        -RestoreOriginalMaterials()
    }
    note for HeroEditorAdapter "SpriteMask · 커스텀 머티리얼 충돌을 어댑터 내부에 완전 캡슐화"

    class FantasyMonsterAdapter {
        <<Concrete>>
        -Monster monsterScript
        -Dictionary originalColors
    }

    class SpineMonsterAdapter {
        <<Concrete>>
        -SkeletonAnimation skeletonAnim
        -SetSpineAnim(animName, loop)
        -HasAnimation(animName) bool
    }

    IUnitAnimator <|.. BaseUnitAnimator
    BaseUnitAnimator <|-- HeroEditorAdapter
    BaseUnitAnimator <|-- FantasyMonsterAdapter
    BaseUnitAnimator <|-- SpineMonsterAdapter

    class Unit {
        <<MonoBehaviourPun>>
    }
    Unit ..> IUnitAnimator : 인터페이스만 참조
```

---

## 6. 네트워크 & 오브젝트 풀

Photon 의존성이 **`UnitNetworkSync`와 `NetworkPoolManager` 두 곳에만** 존재하도록 경계를 좁혔습니다.
상태 머신은 네트워크를 전혀 알지 못하고, 상태 변경 이벤트만 발행합니다.

```mermaid
classDiagram
    class NetworkPoolManager {
        <<Singleton · IPunPrefabPool>>
        -GameObject[] prefabArray
        -Dictionary~string,GameObject~ prefabDict
        -Dictionary~string,Queue~ objectPool
        -HashSet~GameObject~ pooledObjects
        #override Awake()
        #override OnDestroy()
        -RegisterPrefabs()
        +RegisterNetworkPrefab(prefab)
        +Instantiate(prefabId, pos, rot) GameObject
        +Destroy(obj)
        -GetPrefabFromPool(pool, pos, rot) GameObject
        -CreatePrefab(prefab, pos, rot) GameObject
        -ReturnToPool(obj)
    }
    note for NetworkPoolManager "OnDestroy에서 PhotonNetwork.PrefabPool을 DefaultPool로 원복 씬 전환 후 파괴된 참조 접근 방지"

    class UnitNetworkSync {
        <<MonoBehaviour>>
        +PhotonView PhotonView
        +bool IsOwnedByLocalPlayer
        +ResetForReuse()
        +BroadcastAttackAnimation()
        +Despawn(delay)
    }

    class GameNetworkManager {
        <<MonoBehaviourPunCallbacks>>
        -bool isMapIndexSet
        +event Action~Player~ OnOpponentLeftRoom
        +event Action~int~ OnMapIndexSet
        +event Action OnLeftRoomSuccess
        +event Action OnReturnToRoomRequested
        +override OnRoomPropertiesUpdate(props)
        -TryGetMapIndexCoroutine() IEnumerator
        -TryReadMapIndex(props, out index) bool
        +GetMyDeckNames() string[]
        +HandleReturnToRoomRequest()
    }
    note for GameNetworkManager "Start에서 1프레임 지연 후 맵 인덱스 조회 구독자 등록 순서 경쟁 회피"

    class PhotonNetwork {
        <<PUN2 SDK>>
        +IPunPrefabPool PrefabPool$
        +Instantiate(prefabName, pos, rot)$
        +Destroy(obj)$
        +LoadLevel(scene)$
    }

    NetworkPoolManager ..> PhotonNetwork : PrefabPool 구현체 등록
    UnitNetworkSync ..> PhotonNetwork : RPC / Destroy
    GameNetworkManager ..> PhotonNetwork : CustomProperties 콜백
```

### 유닛 생성, 동기화, 소멸 경로

```mermaid
sequenceDiagram
    participant UI as GameUnitSlotItem
    participant GM as GameManager
    participant EN as EnergyManager
    participant PN as PhotonNetwork
    participant Pool as NetworkPoolManager
    participant U as Unit (양측 클라이언트)

    UI->>GM: OnUnitSlotClicked(slotIndex, stat)
    GM->>EN: TryConsumeEnergy(SpawnCost)
    EN-->>GM: true
    GM->>PN: Instantiate(UnitPrefab.name, spawnPoint)
    PN->>Pool: Instantiate(prefabId, ...)
    Pool-->>PN: 풀 재사용 또는 신규 생성 (비활성 상태 반환)
    PN->>U: OnPhotonInstantiate()
    U->>U: InitializeUnitStat · ResetTransform · ResetForReuse ×3 · InitializeState
    Note over U: 소유 클라이언트만 상태 머신 Tick
    U->>U: 상태 변경 시 RPC_SyncAnimation(Others)
    U->>PN: 사망 후 Despawn → PhotonNetwork.Destroy
    PN->>Pool: Destroy(obj) → SetActive(false) 및 큐 반환
```

---

## 7. 데이터 계층

로직과 데이터를 분리해, **유닛 추가 시 코드 수정 없이 에셋 파일만 생성**하면 시스템에 반영되도록 했습니다(OCP).

```mermaid
classDiagram
    class UnitStat {
        <<ScriptableObject>>
        -GameObject unitPrefab
        -Sprite unitIcon
        -string unitName / unitDescription
        -ElementType elementType
        -int spawnCost
        -float spawnCoolTime
        -float maxHP / attackDamage / attackRange
        -float firstAttackDelay / attackInterval
        -float moveSpeed / aoeRadius
        -static float[,] ElementMultiplier$
        +GameObject UnitPrefab
        +Sprite UnitIcon
        +string UnitName / UnitDescription
        +ElementType ElementType
        +int SpawnCost
        +float SpawnCoolTime
        +float MaxHP / AttackDamage / AttackRange
        +float FirstAttackDelay / AttackInterval
        +float MoveSpeed / AoeRadius
        +static CalculateDamage(attacker, defender, baseDamage) float$
    }
    note for UnitStat "직렬화 필드는 private + 읽기 전용 프로퍼티 상성 배율은 5×5 정적 테이블 조회 O(1)"

    class UnitDatabase {
        <<ScriptableObject>>
        -List~UnitStat~ units
        -Dictionary~string,UnitStat~ nameData
        -Dictionary~ElementType,List~ elementData
        +IReadOnlyList~UnitStat~ Units
        -InitializeDatabase()
        +FindByName(unitName) UnitStat
        +FindByElement(type) IReadOnlyList
    }
    note for UnitDatabase "이름 조회 · 속성 필터링 모두 O(1) 속성별 목록은 소환 비용 오름차순 사전 정렬"

    class DeckModel {
        <<순수 C#>>
        -UnitStat[] deck
        +event Action~int,UnitStat~ OnDeckSlotStateChanged
        +GetUnit(index) UnitStat
        +SetUnit(index, stat)
        +RemoveUnit(index)
        +SwapUnits(from, to)
        -IsValidIndex(index) bool
        +IsFull() bool
        +FindUnitIndex(stat) int
        +GetUnitNames() string[]
    }

    class GameStateModel {
        <<순수 C#>>
        +bool IsGameOver
        +event Action~bool~ OnGameOver
        +DeclareGameOver(localPlayerWon)
    }
    note for GameStateModel "중복 선언 차단 성 파괴와 상대 탈주가 겹쳐도 결과는 한 번만 확정"

    class ElementType {
        <<enum>>
        Void · Wind · Forest · Fire · Mountain
    }

    UnitDatabase o-- UnitStat
    DeckModel o-- UnitStat
    UnitStat ..> ElementType
```

---

## 8. UI 계층 (Composite)

단일 아이템과 복합 컨테이너를 동일한 방식으로 다뤄, 슬롯 개수와 무관하게 최상위 Manager 한 곳에서 제어합니다.

```mermaid
classDiagram
    class GameUIManager {
        <<BaseUIManager>>
        -GameStartPanel gameStartPanel
        -GameResultPanel gameResultPanel
        -GameLoadingPanel gameLoadingPanel
        -GameUnitSlotContainer slotContainer
        +event Action OnReturnToRoomRequested
        +event Action OnReturnToLobbyRequested
        +event Action~int,UnitStat~ OnUnitSlotClicked
        +ShowGameStartPanel(p1, p2)
        +ShowGameResultPanel(won, name)
        +ShowGameLoadingPanel(message)
        +UpdateLoadingProgress(normalized)
        +SetGameUnitSlotsUI(index, stat)
        +StartSlotCoolTime(index)
        +UpdateSlotStateByEnergy(currentEnergy)
        +CheckUnitSpawnable(index) bool
    }

    class GameUnitSlotContainer {
        <<Composite>>
        -GameUnitSlotItem[] gameSlots
        +event Action~int,UnitStat~ OnUnitSlotClicked
        +InitializeSlots()
        +SetSlotsUI(index, stat)
        +StartSlotCoolTime(index)
        +UpdateSlotStateByEnergy(currentEnergy)
        +CheckUnitSpawnable(index) bool
    }

    class GameUnitSlotItem {
        <<Leaf · IPointerClickHandler>>
        -int slotIndex
        -UnitStat assignedUnit
        -SlotState currentState
        -Coroutine coolTimeCoroutine
        +bool IsActive / IsInCoolTime / IsSpawnable
        +SetupSlot(index)
        +SetSlotUI(stat)
        -ChangeState(newState)
        +StartCoolTime()
        -StartCoolTimeCoroutine() IEnumerator
        +UpdateSlotStateByEnergy(currentEnergy)
    }

    class UnitSettingUIManager {
        <<BaseUIManager>>
        -DeckSlotContainer deckSlotContainer
        -UnitSlotContainer unitSlotContainer
        -GameObject dragGhostObj
        +ShowDragGhost(UnitSlotItem)
        +ShowDragGhost(DeckSlotItem)
        +MoveDragGhost(eventData, canvas)
        +HideDragGhost()
    }

    class DeckSlotContainer {
        <<Composite>>
        +DeckSlotItem[] slotItems
        +InitializeSlotContainer()
        +UpdateDeckSlotUI(index, stat)
    }

    class DeckSlotItem {
        <<Leaf · IDropHandler · IBeginDragHandler>>
        +int slotIndex
        -UnitStat assignedUnit
        +UpdateUI(stat)
        +OnDrop(eventData)
    }

    class UnitSlotContainer {
        <<Composite · IDropHandler>>
        -UnitSlotItem[] unitSlots
        +InitializeSlotContainer()
        +UpdateUnitSlotList(elementUnits)
        +UpdateUnitSlotState(equipped, selected)
    }

    class UnitSlotItem {
        <<Leaf · IPointerClickHandler>>
        -UnitStat assignedUnit
        +Setup(stat)
        +SetEquippedState(isEquipped)
        +SetSelectedState(isSelected)
    }

    class PopupPanelUIManager {
        <<Singleton>>
        -Dictionary~PopupType,BasePopupPanel~ popupCache
        +ShowError(message, action)
        +ShowConfirm(message, action)
        +ShowSelection(message, onYes, onNo)
        +ShowWaiting(message, onCancel)
        +HideWaiting()
        -GetOrCreatePopup~T~(type) T
    }
    note for PopupPanelUIManager "팝업 프리팹을 최초 1회만 생성 후 캐싱 전역 싱글톤이라 모든 씬에서 재사용"

    GameUIManager o-- GameUnitSlotContainer
    GameUnitSlotContainer o-- GameUnitSlotItem
    UnitSettingUIManager o-- DeckSlotContainer
    UnitSettingUIManager o-- UnitSlotContainer
    DeckSlotContainer o-- DeckSlotItem
    UnitSlotContainer o-- UnitSlotItem
```

**이벤트 버블링 경로**

```
GameUnitSlotItem (클릭)
   → GameUnitSlotContainer (집계)
      → GameUIManager (View)
         → GameManager (Controller에서만 실제 소환 판정)
```

---

## 9. 채팅 시스템 (Strategy)

로비 채팅과 방 채팅은 **전송 방식이 완전히 다릅니다**(Photon Chat SDK vs. RPC 브로드캐스트).
전송을 인터페이스로 추상화해, `ChatController`는 구체 방식을 모른 채 씬에 따라 구현체만 교체됩니다.

```mermaid
classDiagram
    class ChatController {
        <<Context>>
        -MonoBehaviour transportComponent
        -ChatPanelUI viewComponent
        -IChatTransport transport
        -IChatView view
        -HandleSendMessageRequest(message)
        -HandlePlayerMessageReceived(sender, message)
        -HandleSystemMessageReceived(message)
    }

    class IChatTransport {
        <<Interface · Strategy>>
        +event Action~string,string~ OnPlayerMessageReceived
        +event Action~string~ OnSystemMessageReceived
        +Connect()
        +Disconnect()
        +Send(message)
    }

    class LobbyChatTransport {
        <<Concrete Strategy · IChatClientListener>>
        -string chatRegion
        -ChatClient chatClient
        +Connect()
        +Send(message)
        +OnConnected()
        +OnGetMessages(channel, senders, messages)
    }
    note for LobbyChatTransport "WebGL 빌드에서는 WebSocketSecure 프로토콜과 논스레드 송신으로 전환"

    class RoomChatTransport {
        <<Concrete Strategy · MonoBehaviourPunCallbacks>>
        -RoomNetworkManager roomNetworkManager
        +Connect()
        +Send(message)
        -RPC_ReceiveChat(sender, message)
        -HandlePlayerJoined(player)
        -HandlePlayerLeft(player)
    }

    class IChatView {
        <<Interface>>
        +event Action~string~ OnSendMessageRequest
        +AppendMessage(formattedMessage)
    }

    class ChatPanelUI {
        <<MonoBehaviour>>
        -InputField chatInputField
        -ScrollRect chatView
        -int maxMessageCount
        +AppendMessage(formattedMessage)
        -TrimOldMessages()
        -UpdateScrollPosition()
    }
    note for ChatPanelUI "메시지 상한 도달 시 오래된 항목부터 제거 장시간 로비 체류 시 무한 증가 방지"

    class ChatMessageFormatter {
        <<Static>>
        +GetFormattedPlayerMessage(sender, msg, isMine) string$
        +GetFormattedSystemMessage(msg) string$
    }

    IChatTransport <|.. LobbyChatTransport
    IChatTransport <|.. RoomChatTransport
    IChatView <|.. ChatPanelUI
    ChatController --> IChatTransport
    ChatController --> IChatView
    ChatController ..> ChatMessageFormatter
```

---

## 10. 맵 · 성 · 대포 · 에너지

맵 생성부터 승패 판정까지의 Game 씬 서브시스템입니다.

```mermaid
classDiagram
    class MapManager {
        <<BaseSceneController>>
        -MapSpawner mapSpawner
        -CastleSpawner castleSpawner
        -Collider2D cachedGroundCollider
        -bool isMapReady
        +Collider2D GroundCollider
        +event Action~MapData~ OnMapSetupCompleted
        +event Action~float~ OnLoadProgress
        +SetupGameMap(mapIndex)
        -SetupGameMapCoroutine(mapIndex) IEnumerator
        -SpawnPlayerCastle(mapData)
    }

    class MapSpawner {
        <<MonoBehaviour>>
        -List~MapData~ mapPrefabList
        +event Action~MapData~ OnMapSpawned
        +SpawnMap(mapIndex) MapData
        -CheckInValidIndex(mapIndex) bool
    }

    class MapData {
        <<MonoBehaviour>>
        +AudioClip MapBGM
        +Transform Player1CastlePoint
        +Transform Player2CastlePoint
        +PolygonCollider2D CameraBounds
        +BoxCollider2D GroundCollider
    }

    class CastleSpawner {
        <<MonoBehaviour>>
        -GameObject castlePrefab
        +SpawnCastle(spawnPoint)
    }

    class Castle {
        <<MonoBehaviourPun · IDamagable>>
        -float maxHP / currentHP
        -string lastHP
        -bool isDestroyed
        -Transform unitSpawnPoint / firePoint
        -float directionMultiplier / fireAngle
        +Transform UnitSpawnPoint
        +static event Action~bool,Vector3~ OnAnyCastleDestroyed$
        +OnPhotonInstantiate(info)
        -InitializeCastle()
        -SetHPText()
        +SetDirection(isRightCastle)
        -SetLayer()
        -RegisterCastleAttackManager()
        +FireCannon()
        +LaunchCannonBall(targetPosition)
        -RPC_CreateCannonball(pos, dir, force)
        +RPC_ShowExplosionEffect(hitPoint)
        +RPC_TakeDamage(damage)
        -HandleCastleDestruction()
        -RPC_MyCastleDestroyed()
    }
    note for Castle "lastHP 캐싱으로 동일 문자열 재할당 및 UI 리빌드 방지"

    class CastleAttackManager {
        <<BaseSceneController>>
        -Castle playerCastle
        -float coolTime / currentTimer
        -bool isReady / isRegistered / isStop
        +Castle PlayerCastle
        -UpdateCoolDownTimer()
        -CheckCannonFireInput()
        -HandleCannonFireRequest()
        -ResetFireState()
        +SetPlayerCastle(castle)
        +StopAttackSystem()
    }

    class Cannonball {
        <<MonoBehaviour>>
        -PhotonView castleView
        -float hpDamagePercent
        -bool hasDetonated
        -static List~Unit~ unitBuffer
        +Init(ownerCastleView)
        -OnTriggerEnter2D(collision)
        -ApplyDamageToUnit()
    }
    note for Cannonball "지면 충돌 시 폭발 UnitRegistry 스냅샷으로 대상 수집"

    class ExplosionEffectManager {
        <<Singleton>>
        -GameObject explosionPrefab
        -Queue~GameObject~ explosionPool
        -int initialPoolsize / maxPoolSize
        -float spacing / delay / effectLifeTime
        -bool hasCachedBounds
        +PlayChainExplosion(startPos)
        -ChainExplosionCoroutine(startPos) IEnumerator
        -SpawnExplosion(pos)
        -TryGetGroundBounds(out minX, out maxX) bool
        -ExpandExplosionsRoutine(...) IEnumerator
        -ReturnPrefabToPoolAfterDelay(effect, time) IEnumerator
    }

    class TrajectoryCalculator {
        <<Static>>
        +CalculateLaunchForce(from, to, angleDeg) Vector2$
    }

    class EnergyManager {
        <<BaseSceneController>>
        -EnergyLevelStat[] levelStats
        -float currentEnergy
        -int currentLevel
        -bool wasUpgradeable / isStop
        +float CurrentEnergy
        +event Action~float~ OnEnergyChanged
        -GenerateEnergy()
        -CalculateCurrentEnergy(stat)
        -CheckEnergyUpgradeInput()
        -HandleUpgradeRequest()
        -IsMaxLevel() bool
        -UpdateUpgradeButtonUI(forceUpdate)
        +TryConsumeEnergy(amount) bool
        +AddEnergy(amount)
        +StopEnergySystem()
    }
    note for EnergyManager "wasUpgradeable 캐싱으로 상태가 바뀔 때만 버튼 UI 갱신"

    class EnergyLevelStat {
        <<struct · Serializable>>
        +float energyGenerationRate
        +float maxEnergy
        +int upgradeCost
    }

    MapManager --> MapSpawner
    MapManager --> CastleSpawner
    MapSpawner ..> MapData
    CastleSpawner ..> Castle
    CastleAttackManager --> Castle
    Castle ..> TrajectoryCalculator
    Castle ..> Cannonball : RPC로 양측 생성
    Castle ..> ExplosionEffectManager
    Cannonball ..> UnitRegistry : 활성 유닛 조회
    EnergyManager o-- EnergyLevelStat
```

---

## 11. 프로파일링 계층 (계측 빌드 전용)

최적화 판단을 감이 아닌 계측으로 하기 위한 인프라입니다.
**`ENABLE_PROFILING` 심볼이 없으면 전 항목이 컴파일 단계에서 제거**되어, 배포 빌드에는 한 줄도 남지 않습니다.

```mermaid
classDiagram
    class ProfilingSwitches {
        <<MonoBehaviour>>
        +const float DefaultScanInterval
        +static bool UsePooling$
        +static bool UseNonAllocQueries$
        +static bool UseScanThrottle$
        +static bool TickOnlyOwnedUnits$
        +static float ScanInterval$
        +static string VariantName$
        -static ResetStatics()$
    }
    note for ProfilingSwitches "계측 빌드: 인스펙터로 항목별 on/off 릴리즈 빌드: 전부 const → 분기 소거"

    class ProfilingCounters {
        <<Static>>
        +static long RpcSent
        +static long RpcReceivedDamage
        +static long PhysicsQueries
        +static long UnitTicks
        +static CountRpcSent()$
        +static CountRpcReceivedDamage()$
        +static CountPhysicsQuery()$
        +static CountUnitTick()$
        +static ResetAll()$
    }
    note for ProfilingCounters "Conditional 특성으로 호출부까지 제거 릴리즈 빌드 오버헤드 0"

    class NetworkPerformanceLogger {
        <<MonoBehaviour>>
        -float sampleInterval
        -ProfilerRecorder gcAllocRecorder
        -List~string~ rows
        -StringBuilder lineBuilder
        -AccumulateFrame()
        -AppendRow()
        -ReadNetworkStats(...)
        -ResetIntervalAccumulators()
        +StartLogging(scenario)
        +StopLoggingAndExport() string
    }
    note for NetworkPerformanceLogger "프레임 지표 + Photon 트래픽 지표를 1초 단위로 CSV 기록"

    class ProfilingScenarioRunner {
        <<MonoBehaviour>>
        -string scenarioName / spawnUnitName
        -int totalUnits
        -float spawnInterval / warmupSeconds / durationSeconds
        -HandleStartSignal(seed)
        -RunScenario() IEnumerator
    }
    note for ProfilingScenarioRunner "RPC로 양측 클라이언트의 계측 시작 시점을 동기화"

    class FPSOverlay {
        <<MonoBehaviour>>
        -float smoothedMs
        -GUIStyle cachedStyle
    }

    ProfilingScenarioRunner --> NetworkPerformanceLogger
    NetworkPerformanceLogger ..> ProfilingCounters
    NetworkPerformanceLogger ..> ProfilingSwitches
```

---

## 12. 이벤트 & RPC 흐름

시스템 간 직접 참조를 최소화하기 위해 **C# 이벤트**로 내부를 연결하고, 클라이언트 간에는 **PUN2 RPC / CustomProperties**로 전파합니다.

```mermaid
flowchart TD
    subgraph LOCAL["로컬 이벤트 (C# event)"]
        E1["UnitStats.OnDied"] --> H1["Unit.HandleDied<br/>→ Dead 상태 + 에너지 회수"]
        E2["UnitStats.OnKnockBackRequested"] --> H2["Unit.HandleTriggerKnockback<br/>→ Hit 상태"]
        E3["UnitStateMachine.OnStateChanged"] --> H3["UnitNetworkSync.HandleStateChange"]
        E4["Castle.OnAnyCastleDestroyed<br/>(static)"] --> H4["GameManager.HandleGameResult"]
        E5["GameStateModel.OnGameOver"] --> H5["GameManager.HandleGameOverState"]
        E6["EnergyManager.OnEnergyChanged"] --> H6["GameUIManager.UpdateSlotStateByEnergy"]
        E7["GameUIManager.OnUnitSlotClicked"] --> H7["GameManager.HandleUnitSpawnRequest"]
        E8["MapManager.OnMapSetupCompleted"] --> H8["GameManager.HandleMapSetupCompleted"]
        E9["DeckHotkeyHandler.OnSlotHotkeyPressed"] --> H9["GameManager.HandleHotkeyUnitSpawn"]
    end

    subgraph NETWORK["네트워크 전파 (PUN2)"]
        R1["Unit.RPC_SyncAnimation<br/>RpcTarget.Others"]
        R2["Unit.RPC_TakeDamage<br/>RpcTarget.All"]
        R3["Castle.RPC_CreateCannonball<br/>RpcTarget.All"]
        R4["Castle.RPC_ShowExplosionEffect<br/>RpcTarget.All"]
        R5["Castle.RPC_MyCastleDestroyed<br/>RpcTarget.All"]
        P1["Room CustomProperties<br/>GameStart · MapIndex"]
        P2["Player CustomProperties<br/>DeckList · DeckReady"]
    end

    H3 --> R1
    UC["UnitCombat.ApplyDamageToTargets"] --> R2
    CB["Cannonball.ApplyDamageToUnit"] --> R2
    R2 --> E1
    R2 --> E2
    H4 --> E5
    R5 --> E4
    P1 --> H8
    P2 --> DL["GameManager.LoadDeckFromNetwork"]
    CA["CastleAttackManager.HandleCannonFireRequest"] --> R3
    CB --> R4

    style NETWORK fill:#ffe8e8,stroke:#d95a5a
    style LOCAL fill:#e8f4ff,stroke:#4a90d9
```

---

## 📋 설계 요약

| 계층 | 핵심 클래스 | 설계 의도 |
|---|---|---|
| **부트스트랩** | `GameBootstrap` | 전역 매니저를 한 곳에서 생성해 씬별 중복 배치 제거 |
| **Core** | `Singleton<T>` → `BaseSceneController<T>` · `BaseUIManager<T>` | Template Method로 초기화, 구독, 해제 순서를 강제해 씬 확장 시 누락 방지 |
| **씬 MVC** | 5개 씬 × (Controller · UIManager · NetworkManager) | UI, 로직, 네트워크의 변경 이유를 분리해 수정 범위 최소화(SRP) |
| **유닛** | `Unit` + `UnitStats` · `UnitMovement` · `UnitCombat` · `UnitStateMachine` · `UnitNetworkSync` | God class 대신 컴포넌트 위임 — `Unit`은 퍼사드로만 존재 |
| **상태** | `IUnitState` 5종 + `Dictionary` 기반 전이 | 상태 추가 시 분기문 수정 불필요(OCP) · 조회 *O(1)* |
| **애니메이션** | `IUnitAnimator` → `BaseUnitAnimator` → Adapter 3종 | 내부 API가 다른 서드파티 에셋을 공통 인터페이스로 흡수 |
| **네트워크** | `UnitNetworkSync` · `NetworkPoolManager`(`IPunPrefabPool`) | Photon 의존을 두 클래스로 국소화 및 생성/파괴를 풀 재사용으로 대체 |
| **데이터** | `UnitStat` · `UnitDatabase` · `DeckModel` · `GameStateModel` | ScriptableObject로 로직 및 데이터 분리, Dictionary 캐시로 조회 *O(1)* |
| **UI** | `GameUIManager` → `Container` → `Item` | Composite로 슬롯 개수와 무관하게 단일 진입점 제어 |
| **채팅** | `ChatController` + `IChatTransport` 구현 2종 | Strategy로 전송 방식만 교체(Context 코드 수정 없음) |
| **계측** | `ProfilingSwitches` · `ProfilingCounters` · `NetworkPerformanceLogger` | 런타임 스위치로 A/B 측정, 배포 빌드에서는 전량 스트립 |
| **공통 유틸** | `UnitRegistry`(HashSet) · `InputGate`(프레임 캐싱) | 선형 탐색과 중복 조회를 자료구조와 캐싱으로 제거 |

---

## ⚠️ 설계 한계 및 개선 방향

| 항목 | 현재 구조 | 상용 수준에서 필요한 것 |
|---|---|---|
| **권한 모델** | 클라이언트 권위 — 각 클라이언트가 계산한 피해량을 RPC로 전파 | 서버가 판정하고 클라이언트는 입력만 전송하는 서버 권위 구조 |
| **방 비밀번호** | `CustomRoomPropertiesForLobby`에 포함되어 클라이언트가 직접 비교 | Photon Custom Authentication 등 서버 측 검증 |
| **서브시스템 상속** | `EnergyManager` 등이 `BaseSceneController<T>`를 재사용하며 일부 훅이 빈 구현 | 생명주기 베이스와 씬 전용 베이스를 분리 |
