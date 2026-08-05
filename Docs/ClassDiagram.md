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
| 10 | [성 & 대포 & 에너지](#10-성--대포--에너지) | 전투 자원과 승패 판정 |
| 11 | [이벤트 & RPC 흐름](#11-이벤트--rpc-흐름) | 발행/구독 및 네트워크 전파 경로 |

---

## 0. 전체 구조 개요

세부 클래스는 생략하고 **시스템 단위의 의존 방향**만 표현했습니다.

```mermaid
flowchart TD
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
        D3["HeroEditor · FantazyMonster · Spine"]
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

    subgraph BATTLE["🏰 전투 자원"]
        G1["Castle · Cannonball"]
        G2["EnergyManager"]
    end

    subgraph COMMON["🔧 공통 서비스"]
        H1["SoundManager · PopupPanelUIManager"]
        H2["UnitRegistry · InputGate"]
    end

    A1 --> A2
    A2 --> B2
    A3 --> B2
    B1 --> B2
    B2 --> C1
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
    G1 --> C1
    G2 --> B2
    C1 -. 등록 .-> H2
    B2 --> H1

    style UNIT fill:#e8f4ff,stroke:#4a90d9
    style NET fill:#ffe8e8,stroke:#d95a5a
    style CORE fill:#fff6e5,stroke:#e0a030
```

---

## 1. Core 공통 기반

모든 씬이 공유하는 기반 계층입니다. **Template Method 패턴**으로 초기화, 구독, 해제 순서를 강제해, 씬이 늘어나도 생명주기 처리 누락이 발생하지 않도록 했습니다.

```mermaid
classDiagram
    class Singleton_T {
        <<Abstract MonoBehaviour>>
        -static T _instance
        +static T instance$
        +static bool HasInstance$
        #virtual Awake()
        #virtual OnDestroy()
    }

    class BaseSceneController_T {
        <<Abstract · Template Method>>
        #virtual Start()
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
        #abstract InitUIElements()
        #virtual BindUIEvents()
        #abstract BindButtonEvent()
        #abstract BindPanelEvent()
        #virtual UnbindUIEvents()
        #PlayButtonSound()
    }

    class UIPanel {
        <<Abstract MonoBehaviour>>
        #Transform contentTransform
        #CanvasGroup canvasGroup
        #abstract InitializeListener()
        #virtual UnregisterListener()
        #abstract ResetUI()
        +virtual Show()
        +virtual Hide()
        +virtual HideImmediate()
    }

    class BasePopupPanel {
        <<Abstract>>
        #Text messageText
        +virtual SetMessage(message)
        #virtual ShowPopup(message)
    }

    class InputGate {
        <<Static>>
        -static int cachedFrame
        -static bool cachedResult
        +static bool IsBlocked$
        -static IsTextInputFocused() bool
    }
    note for InputGate "프레임 단위 캐싱으로 중복 EventSystem 조회 제거"

    Singleton_T <|-- BaseSceneController_T
    Singleton_T <|-- BaseUIManager_T
    UIPanel <|-- BasePopupPanel
    BaseUIManager_T ..> UIPanel : 패널 생명주기 제어
```

---

## 2. 씬 MVC 구조

5개 씬(MainMenu, Lobby, Room, UnitSetting, Game) 모두 동일한 3계층 구조를 따릅니다.
**Network는 Photon 콜백만 이벤트로 승격**하고, **UI는 화면 출력만** 담당하며, **로직과 상태는 Controller에만** 존재합니다.

```mermaid
flowchart LR
    subgraph V["View (BaseUIManager 상속)"]
        V1["MainMenuUIManager"]
        V2["LobbyUIManager"]
        V3["RoomUIManager"]
        V4["UnitSettingUIManager"]
        V5["GameUIManager"]
    end

    subgraph C["Controller (BaseSceneController 상속)"]
        C1["MainMenuManager"]
        C2["LobbyManager"]
        C3["RoomManager"]
        C4["UnitSettingManager"]
        C5["GameManager"]
    end

    subgraph M["Model / Network (PUN2 콜백 수신)"]
        M1["MainMenuNetworkManager<br/>PlayFabAuthManager"]
        M2["LobbyNetworkManager"]
        M3["RoomNetworkManager"]
        M4["UnitSettingNetworkManager"]
        M5["GameNetworkManager"]
    end

    V1 -. 이벤트 .-> C1
    V2 -. 이벤트 .-> C2
    V3 -. 이벤트 .-> C3
    V4 -. 이벤트 .-> C4
    V5 -. 이벤트 .-> C5

    M1 -. 이벤트 .-> C1
    M2 -. 이벤트 .-> C2
    M3 -. 이벤트 .-> C3
    M4 -. 이벤트 .-> C4
    M5 -. 이벤트 .-> C5

    C1 --> V1
    C2 --> V2
    C3 --> V3
    C4 --> V4
    C5 --> V5

    C1 --> M1
    C2 --> M2
    C3 --> M3
    C4 --> M4
    C5 --> M5

    style C fill:#e8f4ff,stroke:#4a90d9
    style M fill:#ffe8e8,stroke:#d95a5a
```

게임 씬 Controller를 예로 든 실제 구조입니다.

```mermaid
classDiagram
    class GameManager {
        <<BaseSceneController>>
        -GameUIManager gameUIManager
        -GameNetworkManager gameNetworkManager
        -MapManager mapManager
        -EnergyManager energyManager
        -DeckModel deckModel
        -GameStateModel gameState
        -UnitDatabase unitDatabase
        -Transform myUnitSpawnPoint
        #InitializeState()
        -RegisterAllUnitToNetworkPool()
        -LoadMyDeckFromNetwork()
        -HandleUnitSpawnRequest(slotIndex, stat)
        -HandleHotkeyUnitSpawn(slotIndex)
        -SpawnUnit(stat)
        -HandleCastleDestroyed(lost, pos)
        -GameEndSequence(won) IEnumerator
        -PauseAllGameSystems()
    }

    class GameNetworkManager {
        <<MonoBehaviourPunCallbacks>>
        +event Action~Player~ OnOpponentLeftRoom
        +event Action~int~ OnMapIndexReceived
        +event Action OnLeftRoomSuccess
        +OnPlayerLeftRoom(other)
        +OnRoomPropertiesUpdate(props)
        +GetMyDeckNames() string[]
    }

    class GameStateModel {
        <<순수 C#>>
        +bool IsGameOver
        +bool LocalPlayerWon
        +event Action~bool~ OnGameOver
        +DeclareGameOver(won)
        +Reset()
    }

    class MapManager {
        <<BaseSceneController>>
        -MapSpawner mapSpawner
        -CastleSpawner castleSpawner
        +Collider2D GroundCollider
        +event Action~MapData~ OnMapSetupCompleted
        +event Action~float~ OnLoadProgress
        +SetupGameEnvironment(mapIndex)
    }

    GameManager --> GameNetworkManager
    GameManager --> GameStateModel
    GameManager --> MapManager
```

---

## 3. 유닛 시스템 (Component)

`Unit`은 로직을 직접 구현하지 않고 **기능별 컴포넌트에 위임하는 퍼사드**입니다.
`[RequireComponent]`로 필수 컴포넌트 누락을 컴파일 타임에 막고, `[DisallowMultipleComponent]`로 중복 부착을 차단합니다.

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
        +float MaxHP
        +float CurrentHP
        +bool IsAlive
        +bool IsTargetable
        +OnPhotonInstantiate(info)
        +ChangeState(next, isSync)
        +HasValidTarget() bool
        +AcquirePrimaryTarget() Collider2D
        +ApplyDamageFromAttack(origin)
        +PlayAttackAnimation() float
        -HandleDied()
        -HandleKnockbackHPCrossed()
        +RPC_SyncAnimation(stateTypeInt)
        +RPC_TakeDamage(damage)
    }
    note for Unit "로직 없음 — 컴포넌트에 위임만 IsTargetable = 살아있고 피격 상태가 아닐 것"

    class UnitStats {
        <<MonoBehaviour · IDamagable>>
        -UnitStat baseStat
        -float maxHP / currentHP
        -float attackDamage / attackRange / aoeRadius
        -bool hasTriggeredHalfHPHit
        -bool hasTriggeredQuarterHPHit
        +event Action~float~ OnDamageTaken
        +event Action OnDied
        +event Action OnHpThresholdCrossed
        +InitializeFromBaseStat()
        +ApplyDamage(damage)
        -TryTriggerKnockback()
        +CalculateDamageAgainst(defenderElement) float
    }
    note for UnitStats "HP 절반 및 4분의 1 지점 통과 시 OnHpThresholdCrossed 발행 → 넉백"

    class UnitMovement {
        <<MonoBehaviour>>
        -Rigidbody2D rb
        -Collider2D unitCollider
        -float knockbackPower / knockbackDuration
        +float DirectionMultiplier
        +SetDirection(multiplier)
        +ResetForReuse()
        +MoveForward()
        +Stop()
        +ApplyKnockback()
        +DisableAllPhysics()
    }

    class UnitCombat {
        <<MonoBehaviour>>
        -Collider2D[] scanBuffer
        -Collider2D[] aoeBuffer
        -List~Collider2D~ reusableTargets
        +int TargetLayerMask
        +AcquirePrimaryTarget() Collider2D
        +IsTargetInRange(target) bool
        +ApplyDamageFromAttack(epicenter)
        -ComputeAttackBox(bounds) Bounds
        -FindEnemyInBox(center, size) Collider2D
        -FindAllEnemiesInAoeRadius(epicenter, results)
    }
    note for UnitCombat "논할당 물리 질의 버퍼 재사용으로 GC 제거"

    class UnitStateMachine {
        <<MonoBehaviour>>
        -IUnitState currentState
        -Dictionary~UnitStateType,IUnitState~ stateDictionary
        +event Action~IUnitState~ OnStateChanged
        +StartFromIdle()
        +Tick()
        +ChangeState(next, isSync)
        +TryGetStateByType(type, out state) bool
    }

    class UnitNetworkSync {
        <<MonoBehaviour · RequireComponent PhotonView>>
        +PhotonView PhotonView
        +bool IsOwnedByLocalPlayer
        +int OwnLayerMask
        +int TargetLayerMask
        +ConfigureNetworkRole()
        -SetDirection()
        -SetLayer()
        +BroadcastAttackAnimation()
        -BroadcastStateChange(next)
        +ScheduleDestruction(delay)
    }

    class UnitRegistry {
        <<Static>>
        -static HashSet~Unit~ activeUnits
        +static IReadOnlyCollection~Unit~ ActiveUnits$
        +static Register(unit)$
        +static Unregister(unit)$
        +static Clear()$
        +static CopyTo(buffer)$
    }
    note for UnitRegistry "FindObjectsOfType 대체 등록/해제 O(1)"

    Unit *-- UnitStats
    Unit *-- UnitMovement
    Unit *-- UnitCombat
    Unit *-- UnitStateMachine
    Unit *-- UnitNetworkSync
    Unit ..> UnitRegistry : OnEnable/OnDisable 등록
    UnitStateMachine ..> UnitNetworkSync : OnStateChanged 구독
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
        -const float ScanInterval
        -float lastScanTime
    }
    note for UnitStateIdle "0.1초 주기 스캔 타겟 유무로 Attack/Move 분기"

    class UnitStateMove {
        <<Serializable>>
        -const float ScanInterval
        -float lastScanTime
    }

    class UnitStateAttack {
        <<Serializable>>
        -AttackPhase phase
        -float phaseTimer
        -float currentAnimDuration
        -Collider2D currentTarget
        -TickWaitingFirst(unit)
        -BeginAttackCycle(unit)
        -TickAttacking(unit)
        -TryApplyDamage(unit)
        -TickInterval(unit)
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
    Dead --> [*] : ScheduleDestruction → 풀 반환
```

---

## 5. 애니메이터 계층 (Adapter)

리소스 한계로 **내부 API가 전혀 다른 3종의 서드파티 유닛 에셋**을 함께 사용해야 했습니다.
공통 인터페이스로 변환해, 유닛 시스템이 에셋 종류를 전혀 알지 못하도록 격리했습니다.

```mermaid
classDiagram
    class IUnitAnimator {
        <<Interface · Target>>
        +PlayIdle()
        +PlayMove()
        +PlayAttack() float
        +PlayHit()
        +PlayDead()
        +SetDirection(facesLeft)
        +StartFadeOut(duration)
        +ResetForReuse()
    }

    class BaseUnitAnimator {
        <<Abstract>>
        #virtual Awake()
        #abstract PlayIdle()
        #abstract PlayMove()
        #abstract PlayAttack() float
    }
    note for BaseUnitAnimator "피격 플래시 · 페이드 아웃 등 공통 로직 구현 에셋별 차이는 추상 메서드로 위임" 

    class HeroEditorAdapter {
        <<Concrete>>
        -Dictionary originalMaterials
        -CacheRenderers()
        -RestoreOriginalMaterials()
    }
    note for HeroEditorAdapter "SpriteMask · 커스텀 머티리얼 충돌을 어댑터 내부에 완전 캡슐화"

    class FantazyMonsterAdapter {
        <<Concrete>>
        Fantazy Monster API 변환
    }

    class SpineMonsterAdapter {
        <<Concrete>>
        Spine SkeletonAnimation 변환
    }

    IUnitAnimator <|.. BaseUnitAnimator
    BaseUnitAnimator <|-- HeroEditorAdapter
    BaseUnitAnimator <|-- FantazyMonsterAdapter
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
        -Dictionary~string,GameObject~ prefabDict
        -Dictionary objectPool
        #override Awake()
        +RegisterNetworkPrefab(prefab)
        +Instantiate(prefabId, pos, rot) GameObject
        +Destroy(obj)
        -ReuseFromPool(pool, pos, rot) GameObject
        -CreateNewInstance(prefab, pos, rot) GameObject
        -ReturnToPool(obj)
    }
    note for NetworkPoolManager "PhotonNetwork.PrefabPool을 직접 교체 Instantiate/Destroy → 큐 재사용"

    class UnitNetworkSync {
        <<MonoBehaviour>>
        +PhotonView PhotonView
        +bool IsOwnedByLocalPlayer
        +ConfigureNetworkRole()
        +BroadcastAttackAnimation()
        +ScheduleDestruction(delay)
    }

    class PhotonNetwork {
        <<PUN2 SDK>>
        +IPunPrefabPool PrefabPool$
        +Instantiate(prefabName, pos, rot)$
        +Destroy(obj)$
        +LoadLevel(scene)$
    }

    NetworkPoolManager ..> PhotonNetwork : PrefabPool 구현체 등록
    UnitNetworkSync ..> PhotonNetwork : RPC / Destroy
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
    GM->>EN: TryConsumeEnergy(spawnCost)
    EN-->>GM: true
    GM->>PN: Instantiate(prefabName, spawnPoint)
    PN->>Pool: Instantiate(prefabId, ...)
    Pool-->>PN: 풀에서 재사용 or 신규 생성
    PN->>U: OnPhotonInstantiate()
    U->>U: 스탯 초기화, 방향/레이어 설정, Idle 진입
    Note over U: 소유 클라이언트만 상태 머신 Tick
    U->>U: 상태 변경 시 RPC_SyncAnimation(Others)
    U->>PN: 사망 후 ScheduleDestruction → Destroy
    PN->>Pool: Destroy(obj) → SetActive(false) 및 큐 반환
```

---

## 7. 데이터 계층

로직과 데이터를 분리해, **유닛 추가 시 코드 수정 없이 에셋 파일만 생성**하면 시스템에 반영되도록 했습니다(OCP).

```mermaid
classDiagram
    class UnitStat {
        <<ScriptableObject>>
        +GameObject unitPrefab
        +Sprite unitIcon
        +string unitName
        +ElementType elementType
        +int spawnCost
        +float spawnCoolTime
        +float maxHP / attackDamage
        +float firstAttackDelay / attackInterval
        +float attackRange / moveSpeed / aoeRadius
        +CalculateDamage(attacker, defender, base) float
    }
    note for UnitStat "상성 배율을 데이터 레벨에 캡슐화 전투 로직은 상성표를 모름"

    class UnitDatabase {
        <<ScriptableObject>>
        -List~UnitStat~ units
        -Dictionary~string,UnitStat~ nameData
        -Dictionary elementData
        +IReadOnlyList~UnitStat~ All
        -BuildData()
        +FindByName(name) UnitStat
        +FindByElement(type) IReadOnlyList
    }
    note for UnitDatabase "이름 조회 및 속성 필터링 O(1) OnEnable/OnValidate 시 캐시 재구축"

    class ElementType {
        <<enum>>
        Void
        Wind
        Forest
        Fire
        Mountain
    }

    class DeckModel {
        <<순수 C#>>
        -UnitStat[] myDeck
        +int Capacity
        +event Action~int,UnitStat~ OnSlotChanged
        +GetUnit(index) UnitStat
        +SetUnit(index, stat)
        +RemoveUnit(index)
        +SwapUnits(from, to)
        +IsFull() bool
        +GetUnitNames() string[]
        +GetSnapshot() UnitStat[]
    }

    class EnergyLevelStat {
        <<Serializable struct>>
        +float energyPerSecond
        +float maxEnergy
        +int upgradeCost
    }

    UnitDatabase o-- UnitStat
    UnitStat --> ElementType
    DeckModel o-- UnitStat
```

### 속성 상성 관계

```mermaid
flowchart LR
    W["🌪️ 풍<br/>Wind"] -->|"×1.5"| M["⛰️ 산<br/>Mountain"]
    M -->|"×1.5"| F["🔥 화<br/>Fire"]
    F -->|"×1.5"| Fo["🌲 림<br/>Forest"]
    Fo -->|"×1.5"| W
    V["⬛ 무 (Void)<br/>상성 없음 · ×1.0"]

    style W fill:#e8f4ff,stroke:#4a90d9
    style Fo fill:#eaffea,stroke:#4caf50
    style F fill:#ffe8e8,stroke:#d95a5a
    style M fill:#fff6e5,stroke:#e0a030
    style V fill:#eeeeee,stroke:#888888
```

> 역방향은 `×0.75`가 적용되어, 상성 우위와 열세가 한 함수 안에서 함께 계산됩니다.

---

## 8. UI 계층 (Composite)

단일 컴포넌트와 복합 컨테이너를 동일한 방식으로 다뤄, 슬롯 개수와 무관하게 **최상위 Manager 한 곳에서 일괄 제어**합니다.

```mermaid
classDiagram
    class GameUIManager {
        <<BaseUIManager · 최상위 조합자>>
        -GameUnitSlotContainer slotContainer
        +event Action~int,UnitStat~ OnUnitSlotClicked
        +ShowGameLoadingPanel()
        +UpdateLoadingProgress(normalized)
        +ShowGameStartPanel(p1, p2)
        +ShowGameResultPanel(won, nickname)
        +UpdateDeckSlotsUI(index, stat)
        +StartSlotCoolTime(index)
        +RefreshSlotsEnergyState(energy)
    }

    class GameUnitSlotContainer {
        <<Composite>>
        -GameUnitSlotItem[] gameSlots
        +event Action~int,UnitStat~ OnUnitSlotClicked
        +InitializeSlots()
        +ShowUnitSlot(index, stat)
        +StartSlotCoolTime(index)
        +RefreshSlotsEnergyState(energy)
    }

    class GameUnitSlotItem {
        <<Leaf · IPointerClickHandler>>
        -UnitStat assignedUnit
        -SlotState currentState
        -Coroutine coolTimeCoroutine
        +bool IsActive
        +bool IsInCoolTime
        +event Action~int,UnitStat~ OnUnitSlotClicked
        +SetupSlot(index)
        +UpdateUI(stat)
        +StartCoolTime()
        +EvaluateEnergyState(energy)
    }

    class SlotState {
        <<enum>>
        Active
        Inactive
        CoolTime
    }

    class UIPanel {
        <<Abstract>>
        +Show()
        +Hide()
    }
    class GameLoadingPanel
    class GameStartPanel
    class GameResultPanel

    UIPanel <|-- GameLoadingPanel
    UIPanel <|-- GameStartPanel
    UIPanel <|-- GameResultPanel

    GameUIManager o-- GameUnitSlotContainer
    GameUIManager o-- GameLoadingPanel
    GameUIManager o-- GameStartPanel
    GameUIManager o-- GameResultPanel
    GameUnitSlotContainer o-- GameUnitSlotItem
    GameUnitSlotItem --> SlotState
```

이벤트는 **Item → Container → UIManager → Controller** 순으로 버블링되어, 실제 데이터 변경은 Controller 계층에서만 일어납니다.

---

## 9. 채팅 시스템 (Strategy)

로비 채팅과 방 채팅은 **전송 방식이 완전히 다릅니다**(Photon Chat SDK vs. RPC 브로드캐스트).
전송을 인터페이스로 추상화해, `ChatController`는 구체 방식을 모른 채 씬에 따라 구현체만 교체됩니다.

```mermaid
classDiagram
    class ChatController {
        <<Context>>
        -IChatTransport transport
        -IChatView view
        -HandleSendMessageRequest(message)
        -HandleMessageReceived(sender, message)
        -HandleSystemMessage(message)
    }

    class IChatTransport {
        <<Interface · Strategy>>
        +event Action~string,string~ OnMessageReceived
        +event Action~string~ OnSystemMessage
        +Connect()
        +Disconnect()
        +Send(message)
    }

    class LobbyChatTransport {
        <<Concrete Strategy>>
        +Connect()
        +Send(message)
    }
    note for LobbyChatTransport "Photon Chat SDK 글로벌 로비 채널 구독" 

    class RoomChatTransport {
        <<Concrete Strategy>>
        +Connect()
        +Send(message)
    }
    note for RoomChatTransport "PhotonView.RPC(RpcTarget.All) 브로드캐스트 입장 및 퇴장 시스템 메시지 포함" 

    class IChatView {
        <<Interface>>
        +event Action~string~ OnSendMessageRequest
        +AppendMessage(formattedMessage)
    }

    class ChatPanelUI {
        <<MonoBehaviour>>
        +AppendMessage(formattedMessage)
    }

    class ChatMessageFormatter {
        <<Static>>
        +FormatPlayerMessage(sender, msg, isMine) string
        +FormatSystemMessage(msg) string
    }

    IChatTransport <|.. LobbyChatTransport
    IChatTransport <|.. RoomChatTransport
    IChatView <|.. ChatPanelUI
    ChatController --> IChatTransport
    ChatController --> IChatView
    ChatController ..> ChatMessageFormatter
```

---

## 10. 성 & 대포 & 에너지

승패를 결정하는 `Castle`, 광역 견제 수단인 `Cannonball`, 유닛 소환 자원인 `EnergyManager`입니다.

```mermaid
classDiagram
    class Castle {
        <<MonoBehaviourPun · IDamagable>>
        -float maxHP / currentHP
        -bool isDestroyed
        -string lastHPText
        -Transform unitSpawnPoint
        +Transform UnitSpawnPoint
        +static event Action~bool,Vector3~ OnAnyCastleDestroyed$
        +event Action~bool,Vector3~ OnThisCastleDestroyed
        +OnPhotonInstantiate(info)
        -InitializeCastle()
        +SetDirection(isRightCastle)
        +FireCannon()
        +LaunchCannonBall(targetPos)
        +RPC_CreateCannonball(pos, dir, force)
        +RPC_ShowExplosionEffect(hitPoint)
        +RPC_TakeDamage(damage)
        -HandleCastleDestruction()
        +RPC_MyCastleDestroyed()
    }
    note for Castle "lastHPText 캐싱으로 동일 문자열 재할당 및 UI 리빌드 방지"

    class CastleAttackManager {
        <<BaseSceneController>>
        -Castle playerCastle
        -float coolTime / currentTimer
        -bool isReady / isRegistered
        +Castle PlayerCastle
        -UpdateCoolTimeTimer()
        -CheckSpaceBarInput()
        -HandleFireRequest()
        +SetPlayerCastle(castle)
        +Stop()
    }

    class Cannonball {
        <<MonoBehaviour>>
        -PhotonView castleView
        -float hpDamagePercent
        -bool hasDetonated
        -static List~Unit~ unitBuffer
        +Init(ownerCastleView)
        -OnTriggerEnter2D(collision)
        -ApplyNetworkDamage()
    }
    note for Cannonball "지면 충돌 시 폭발 UnitRegistry 스냅샷으로 대상 수집"

    class TrajectoryCalculator {
        <<Static>>
        +CalculateLaunchForce(from, to, angle) Vector2
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
        -HandleUpgradeRequest()
        -CheckUpgradeAvailability(force)
        +TryConsumeEnergy(amount) bool
        +AddEnergy(amount)
        +Stop()
    }

    Castle ..> TrajectoryCalculator
    Castle ..> Cannonball : RPC로 양측 생성
    CastleAttackManager --> Castle
    Cannonball ..> UnitRegistry : 활성 유닛 조회
    EnergyManager o-- EnergyLevelStat
```

---

## 11. 이벤트 및 RPC 흐름

시스템 간 직접 참조를 최소화하기 위해 **C# 이벤트**로 내부를 연결하고, 클라이언트 간에는 **PUN2 RPC / CustomProperties**로 전파합니다.

```mermaid
flowchart TD
    subgraph LOCAL["로컬 이벤트 (C# event)"]
        E1["UnitStats.OnDied"] --> H1["Unit.HandleDied<br/>→ Dead 상태 + 에너지 회수"]
        E2["UnitStats.OnHpThresholdCrossed"] --> H2["Unit.HandleKnockbackHPCrossed<br/>→ Hit 상태"]
        E3["UnitStateMachine.OnStateChanged"] --> H3["UnitNetworkSync.BroadcastStateChange"]
        E4["Castle.OnAnyCastleDestroyed<br/>(static)"] --> H4["GameManager.HandleCastleDestroyed"]
        E5["GameStateModel.OnGameOver"] --> H5["GameManager.GameEndSequence"]
        E6["EnergyManager.OnEnergyChanged"] --> H6["GameUIManager.RefreshSlotsEnergyState"]
        E7["GameUIManager.OnUnitSlotClicked"] --> H7["GameManager.HandleUnitSpawnRequest"]
        E8["MapManager.OnMapSetupCompleted"] --> H8["GameManager.HandleMapSetupCompleted"]
    end

    subgraph NETWORK["네트워크 전파 (PUN2)"]
        R1["Unit.RPC_SyncAnimation<br/>RpcTarget.Others"]
        R2["Unit.RPC_TakeDamage<br/>RpcTarget.All"]
        R3["Castle.RPC_CreateCannonball<br/>RpcTarget.All"]
        R4["Castle.RPC_MyCastleDestroyed<br/>RpcTarget.All"]
        P1["Room CustomProperties<br/>GameStart · MapIndex"]
        P2["Player CustomProperties<br/>DeckList"]
    end

    H3 --> R1
    H1 --> R1
    UC["UnitCombat.ApplyDamageToTargets"] --> R2
    R2 --> E1
    R2 --> E2
    H4 --> E5
    R4 --> E4
    P1 --> H8
    P2 --> DL["GameManager.LoadMyDeckFromNetwork"]

    style NETWORK fill:#ffe8e8,stroke:#d95a5a
    style LOCAL fill:#e8f4ff,stroke:#4a90d9
```

---

## 📋 설계 요약

| 계층 | 핵심 클래스 | 설계 의도 |
|---|---|---|
| **Core** | `Singleton<T>` → `BaseSceneController<T>` · `BaseUIManager<T>` | Template Method로 초기화, 구독, 해제 순서를 강제해 씬 확장 시 누락 방지 |
| **씬 MVC** | 5개 씬 × (Controller · UIManager · NetworkManager) | UI, 로직, 네트워크의 변경 이유를 분리해 수정 범위 최소화(SRP) |
| **유닛** | `Unit` + `UnitStats` · `UnitMovement` · `UnitCombat` · `UnitStateMachine` · `UnitNetworkSync` | God class 대신 컴포넌트 위임 — `Unit`은 퍼사드로만 존재 |
| **상태** | `IUnitState` 5종 + `Dictionary` 기반 전이 | 상태 추가 시 분기문 수정 불필요(OCP) · 조회 *O(1)* |
| **애니메이션** | `IUnitAnimator` → `BaseUnitAnimator` → Adapter 3종 | 내부 API가 다른 서드파티 에셋을 공통 인터페이스로 흡수 |
| **네트워크** | `UnitNetworkSync` · `NetworkPoolManager`(`IPunPrefabPool`) | Photon 의존을 두 클래스로 국소화 및 생성/파괴를 풀 재사용으로 대체 |
| **데이터** | `UnitStat` · `UnitDatabase` · `DeckModel` | ScriptableObject로 로직 및 데이터 분리, Dictionary 캐시로 조회 *O(1)* |
| **UI** | `GameUIManager` → `Container` → `Item` | Composite로 슬롯 개수와 무관하게 단일 진입점 제어 |
| **채팅** | `ChatController` + `IChatTransport` 구현 2종 | Strategy로 전송 방식만 교체(Context 코드 수정 없음) |
| **공통 유틸** | `UnitRegistry`(HashSet) · `InputGate`(프레임 캐싱) | 선형 탐색과 중복 조회를 자료구조와 캐싱으로 제거 |

> **설계 한계**: 본 프로젝트는 학습 목적의 **클라이언트 권위 구조**입니다. 각 클라이언트가 계산한 피해량을 RPC로 전파하므로 서버 검증이 없어 치팅에 취약합니다. 상용 수준에서는 서버가 판정을 수행하고 클라이언트는 입력만 전송하는 구조로 전환해야 함을 인지하고 있습니다.
