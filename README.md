# ElementalWar_portfolio
# **🕹️ [프로젝트] 풍림화산 전쟁(Elemental War)**

https://github.com/user-attachments/assets/cb65efce-49bf-4e47-ad4b-02efa7e1b1dd

> **"냥코 대전쟁과 전쟁 시대, 팔라독에서 영감을 얻어, 속성 상성 시스템과 실시간 멀티플레이를 결합한 2D 사이드뷰 타워 디펜스 게임입니다."**

---

## **📌 프로젝트 개요**

- **개발 인원**: 1인 개발 (기획, 프로그래밍, 리소스 관리)
- **개발 기간**: 2026.04 ~ 2026.07
- **기술 스택**: Unity, C#, Photon PUN2, Photon Chat, PlayFab
- **핵심 컨셉**: 풍/림/화/산/무 5속성 상성 시스템을 기반으로 한 실시간 1v1 멀티플레이 타워 디펜스
- **주요 성과**
  1. 5개 씬 전체에 **MVC 아키텍처**를 일관 적용해 UI, 게임 로직, 네트워크의 책임을 분리
  2. Composite, State, Adapter, Strategy, Registry 등 **GoF 패턴을 문제 상황에 맞춰 선택 적용**
  3. 탐색 주기 분리와 소유 유닛 한정 Tick으로 **유닛 800명 환경에서 프레임 타임 51.6% 단축**, 논할당 물리 질의로 **GC 수집 횟수 94.7% 감소** (계측 기반)
  4. Photon PUN2의 **RPC, CustomProperties, Prefab Pool**을 조합해 별도 전용 서버 없이 1v1 실시간 대전 구현

---

## **🛠️ 시스템 아키텍처**

컴퓨터공학과 전공자로서 **유지보수성, 확장성, 가독성**을 고려한 설계를 지향했고, 코드 작성 시 **SOLID 원칙 준수**를 철저히 했습니다.

---

### **1. 씬 구조의 MVC 패턴: `BaseSceneController` / `BaseUIManager` / `NetworkManager`**

모든 씬(MainMenu, Lobby, Room, UnitSetting, Game)에 일관된 **MVC 아키텍처**를 적용했습니다. 이 구조를 통해 UI, 게임 로직, 네트워크의 책임을 완전히 분리했으며, `BaseSceneController`와 `BaseUIManager`의 추상 메서드를 통해 **Template Method 패턴**을 적용하여 각 씬의 초기화 흐름을 강제하고 코드 누락을 방지했습니다.

**패턴 사용 이유**: UI, 게임 로직, 네트워크가 한 클래스에 뭉쳐 있으면 씬이 늘어날 때마다 수정 범위가 많아지기 때문에, 세 책임을 분리함으로써 각기 다른 책임의 이유로 발생하는 코드 변경을 최소화했습니다.

- **Model(Network)**: 각 씬의 네트워크 매니저(`MainMenuNetworkManager`, `LobbyNetworkManager`, `RoomNetworkManager`, `UnitSettingNetworkManager`, `GameNetworkManager`)가 Photon PUN2 콜백을 수신하고, 직접적인 로직 처리 없이 이벤트를 발행하여 Controller에 상태를 전달합니다.
- **View**: `BaseUIManager<T>`를 상속한 각 씬의 UI 매니저(`MainMenuUIManager`, `LobbyUIManager`, `RoomUIManager`, `UnitSettingUIManager`, `GameUIManager`)가 화면 출력만 담당하며, 직접적인 로직 처리 없이 이벤트를 발행하여 Controller에 상태를 전달합니다.
- **Controller**: `BaseSceneController<T>`를 상속한 각 씬의 매니저(`MainMenuManager`, `LobbyManager`, `RoomManager`, `UnitSettingManager`, `GameManager`)가 게임 로직과 상태를 담당합니다.

https://github.com/dbwoaud/ElementalWar_portfolio/blob/2ffeb1072c5449a83ce88f6e002821254c725c4a/Scripts/Common/Abstractions/Base%20Classes/Base%20Scene%20Controller.cs#L3-L45
https://github.com/dbwoaud/ElementalWar_portfolio/blob/2ffeb1072c5449a83ce88f6e002821254c725c4a/Scripts/Common/Abstractions/Base%20Classes/Base%20UI%20Manager.cs#L4-L43
- [🔗 **LobbyNetworkManager.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Lobby/Network/Lobby%20Network%20Manager.cs)
- [🔗 **BaseUIManager를 상속한 LobbyUIManager.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Lobby/UI/Lobby%20UI%20Manager.cs)
- [🔗 **BaseSceneController를 상속한 LobbyManager.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Lobby/Controller/Lobby%20Manager.cs)

---

### **2. UI 계층의 Composite 패턴 — `UIPanel` / `Container` / `Item`**

UI 코드 전반에 Composite 패턴을 적용하여 단일 컴포넌트와 복합 컨테이너를 동일한 인터페이스로 다룰 수 있도록 설계했습니다. 이를 통해 UI 구조가 복잡해지더라도 관리 포인트를 최소화했습니다.

**패턴 사용 이유**: 패널, 컨테이너, 아이템을 각각 다르게 다루면 UI가 복잡해질수록 관리도 복잡해집니다. 동일 인터페이스로 통일해, 슬롯의 개수와 관계없이 최상위 Manager 한 곳에서 일괄 제어하도록 만들었습니다.

```
GameUIManager (최상위 조합자)
     ├─ GameLoadingPanel, GameStartPanel, GameResultPanel (Leaf 패널, UIPanel 상속)
     └─ GameUnitSlotContainer (Composite: 자식 Item들을 포함)
          └─ GameUnitSlotItem × 10 (Leaf 아이템)               
```

- `GameUIManager`: 게임 씬 UI 시스템의 최상위 클래스로, 모든 패널의 생명주기를 총괄하고, Container를 관리합니다.
- `UIPanel`: 모든 패널의 공통 기능을 정의하는 기반 클래스로, 생명주기를 통일하여 모든 패널을 동일하게 제어할 수 있습니다.
- `Container`: 여러 개의 Item을 보유하며 이벤트를 집계하는 클래스로, 상위 매니저는 Container 내부의 구체적인 아이템 개수나 종류를 몰라도 단일 인터페이스를 통해 전체 아이템을 갱신하거나 조작할 수 있습니다.
- `GameUnitSlotItem`: 실제 UI를 구성하는 최소 단위 클래스로, 한 Item의 관리를 담당하며, 여기서 발생하는 모든 상호작용은 이벤트를 통해 부모 컨테이너로 전달되어 객체 간의 결합도를 낮췄습니다.

https://github.com/dbwoaud/ElementalWar_portfolio/blob/2ffeb1072c5449a83ce88f6e002821254c725c4a/Scripts/Game/UI/Game%20UI%20Manager.cs#L5-L15
https://github.com/dbwoaud/ElementalWar_portfolio/blob/2ffeb1072c5449a83ce88f6e002821254c725c4a/Scripts/Common/Abstractions/Base%20Classes/UIPanel.cs#L1-L129

- [🔗 **GameUIManager.cs 전체 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/UI/Game%20UI%20Manager.cs)
- [🔗 **UIPanel을 상속한 GameResultPanel.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/UI/Game%20Result%20Panel.cs)
- [🔗 **GameUnitSlotContainer.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/UI/Game%20Unit%20Slot%20Container.cs)
- [🔗 **GameUnitSlotItem.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/UI/Game%20Unit%20Slot%20Item.cs)

---

### **3. Unit에 적용된 다중 디자인 패턴**

유닛 시스템은 여러 GoF 패턴의 조합으로 설계되어 있습니다.

**3-1. Component 패턴: `Unit` + `UnitStats` / `UnitMovement` / `UnitCombat` / `UnitStateMachine` / `UnitNetworkSync` / `IUnitAnimator`**

`Unit` 클래스는 직접 로직을 구현하지 않으며, 기능별로 분리된 컴포넌트들에 책임을 위임하고 외부에 프로퍼티로 노출하는 **퍼사드** 역할만 수행합니다. `[RequireComponent]` attribute를 통해 필수 컴포넌트 누락을 컴파일 타임에 방지하고, `[DisallowMultipleComponent]` attribute를 통해 중복 컴포넌트를 허용하지 않게 했습니다.

**패턴 사용 이유**: Unit 하나에 로직을 모두 넣으면 거대한 God Class가 되어 유지보수가 어려워집니다. 기능별 컴포넌트로 책임을 위임하고 Unit은 퍼사드 역할만 맡겨, 각 기능을 독립적으로 수정과 테스트를 할 수 있게 했습니다.

https://github.com/dbwoaud/ElementalWar_portfolio/blob/2ffeb1072c5449a83ce88f6e002821254c725c4a/Scripts/Game/Units/Unit.cs#L4-L54

- [🔗 **Unit.cs 전체 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/Units/Unit.cs)  
- [🔗 **UnitStats.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/Units/Components/Unit%20Stats.cs)  
- [🔗 **UnitMovement.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/Units/Components/Unit%20Movement.cs)  
- [🔗 **UnitCombat.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/Units/Components/Unit%20Combat.cs)  
- [🔗 **UnitStateMachine.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/Units/Components/Unit%20State%20Machine.cs)  
- [🔗 **UnitNetworkSync.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/Units/Components/Unit%20Network%20Sync.cs)  
- [🔗 **IUnitAnimator.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/Units/Animators/IUnitAnimator.cs)  


**3-2. State 패턴: `IUnitState` / `UnitStateMachine`**

유닛의 행동(Idle, Move, Attack, Hit, Dead)을 `IUnitState` 인터페이스로 추상화하고, `UnitStateMachine`이 상태 전이를 관리합니다. 새로운 상태 추가 시 기존 코드를 수정할 필요가 없습니다.
`UnitStateMachine`은 `Dictionary<UnitStateType, IUnitState>`로 상태를 관리하여 열거형으로 O(1) 조회가 가능하도록 했으며, 상태 전이 시 `OnStateChanged` 이벤트를 발행하여 네트워크 동기화와 느슨하게 결합됩니다.

**패턴 사용 이유**: 유닛 행동을 if/switch로 처리하면 상태가 추가될 때마다 분기문 전체를 수정해야 합니다. 상태를 독립 클래스로 분리하여 새 상태 추가 시 기존 코드 수정 없이 클래스만 추가하면 되도록 했습니다.

https://github.com/dbwoaud/ElementalWar_portfolio/blob/2ffeb1072c5449a83ce88f6e002821254c725c4a/Scripts/Game/Units/States/IUnitState.cs#L1-L8

- [🔗 **UnitStateIdle.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/Units/States/Unit%20State%20Idle.cs)  
- [🔗 **UnitStateMove.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/Units/States/Unit%20State%20Move.cs)  
- [🔗 **UnitStateAttack.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/Units/States/Unit%20State%20Attack.cs)  
- [🔗 **UnitStateHit.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/Units/States/Unit%20State%20Hit.cs)  
- [🔗 **UnitStateDead.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/Units/States/Unit%20State%20Dead.cs)  

**인스펙터의 `Current State Type`이 Idle → Move → Attack → Hit → Dead로 전이되는 모습**

https://github.com/user-attachments/assets/45c84802-b90f-41e6-9571-53c1abbe700c

**3-3. Observer 패턴: 상태 전이 네트워크 동기화**

`UnitStateMachine`의 `OnStateChanged` 이벤트를 `UnitNetworkSync`가 구독함으로써, 상태기계는 네트워크 존재를 알 필요 없이, 상태 변경만 발행하면 동기화가 자동으로 처리됩니다.

**패턴 사용 이유**: 상태 머신이 네트워크 동기화를 직접 호출하면 두 시스템이 강하게 결합되기 때문에 상태 변경을 이벤트로 발행만 하도록 했습니다. 이를 통해 상태 머신은 네트워크의 존재를 알 필요 없이 동기화가 가능합니다.

**3-4. Registry 패턴: `UnitRegistry`**

`HashSet<Unit>` 기반의 정적 레지스트리를 통해 씬 내 모든 활성 유닛을 O(1)로 등록/해제가 되도록 최적화를 수행하였으며, `IReadOnlyCollection`으로 외부 읽기 전용 노출하여 데이터 무결성을 보장합니다.

**패턴 사용 이유**: 씬의 모든 활성 유닛을 매번 FindObjectsOfType으로 찾으면 유닛이 많아질수록 성능이 급격히 떨어집니다. HashSet 레지스트리로 등록/해제를 O(1)로 처리해 대량 유닛 상황의 조회 비용을 없앴습니다.

- [🔗 **UnitRegistry.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Common/Singletons/Unit%20Registry.cs)

---

### **4. Adapter 패턴: `IUnitAnimator` / `BaseUnitAnimator` / 에셋별 Adapter**
 
리소스 한계로 인해 서로 다른 구조를 가진 세 가지 유닛 에셋을 동시에 사용해야 했습니다. 각 에셋은 애니메이션 재생 방식과 내부 API가 완전히 달랐기 때문에, 유닛 시스템이 에셋 종류를 직접 알게 되면 코드가 에셋에 강하게 결합되는 문제가 발생합니다.  
이를 해결하기 위해 **어댑터 패턴**을 적용했습니다.

**패턴 사용 이유**: 내부 API가 전혀 다른 세 종류 유닛 에셋을 그대로 쓰면 유닛 코드가 특정 에셋에 강하게 묶이기 때문에, 이를 공통 인터페이스로 변환하여 유닛 시스템이 에셋 종류를 몰라도 동작하고 새 에셋은 Adapter 하나만 추가하면 되도록 했습니다.

- **Target Interface (`IUnitAnimator`)**: `PlayIdle()`, `PlayMove()`, `PlayAttack()`, `PlayHit()`, `PlayDead()`, `ResetForReuse()` 등 유닛 시스템이 요구하는 공통 인터페이스를 정의합니다.
- **Base Adapter (`BaseUnitAnimator`)**: 피격 플래시, 페이드 아웃 코루틴 등 에셋 종류에 관계없이 공통으로 사용되는 로직을 구현하고, 에셋별 차이점은 추상 메서드로 위임합니다.
- **Concrete Adapter**: 각 에셋의 실제 API를 공통 인터페이스로 변환합니다.

```
IUnitAnimator (Target)
    └─ BaseUnitAnimator (공통 로직 추상 기반)
            ├─ HeroEditorAdapter   (HeroEditor Character API → IUnitAnimator)
            ├─ FantasyMonsterAdapter (Fantasy Monster API → IUnitAnimator)
            └─ SpineMonsterAdapter  (Spine SkeletonAnimation API → IUnitAnimator)
```

유닛의 `Unit` 클래스와 `UnitStateMachine`은 `IUnitAnimator` 인터페이스만 바라보기 때문에, 어떤 에셋을 사용하는 유닛이든 코드 수정 없이 동일하게 동작합니다. 새로운 에셋 추가 시에도 Adapter 클래스 하나만 추가하면 됩니다.  
- [🔗 **BaseUnitAnimator.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/2ffeb1072c5449a83ce88f6e002821254c725c4a/Scripts/Game/Units/Animators/Base%20Unit%20Animator.cs)  
- [🔗 **HeroEditorAdapter.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/2ffeb1072c5449a83ce88f6e002821254c725c4a/Scripts/Game/Units/Animators/Hero%20Editor%20Adapter.cs)  
- [🔗 **FantazyMonsterAdapter.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/2ffeb1072c5449a83ce88f6e002821254c725c4a/Scripts/Game/Units/Animators/Fantazy%20Monster%20Adapter.cs)  
- [🔗 **SpineMonsterAdapter.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/2ffeb1072c5449a83ce88f6e002821254c725c4a/Scripts/Game/Units/Animators/Spine%20Monster%20Adapter.cs)  

---

### **5. 채팅 시스템의 Strategy 패턴: `IChatTransport` / `LobbyChatTransport` / `RoomChatTransport`**
  
로비와 방이라는 서로 다른 네트워크 환경에서 채팅을 동작시키기 위해 **Strategy 패턴**을 적용했습니다. 전송 방식을 `IChatTransport` 인터페이스로 추상화하여, `ChatController`가 구체적인 전송 방식을 알지 못하더라도 동일하게 동작하도록 설계했습니다.

**패턴 사용 이유**: 로비 채팅(Photon Chat)과 방 채팅(RPC 브로드캐스트)은 전송 방식이 완전히 다르기 때문에, 전송 방식을 인터페이스로 추상화하여, `ChatController`가 구체 방식을 몰라도 동일하게 동작하고 씬에 따라 구현체만 교체하면 되도록 했습니다.

- **`LobbyChatTransport`**: Photon Chat SDK를 사용해 글로벌 로비 채널에 접속합니다. 모든 로비 유저가 동일한 채널을 구독하여 메시지를 주고받습니다.
- **`RoomChatTransport`** : 별도의 Chat 서버 연결 없이 `PhotonView.RPC(RpcTarget.All)`로 방 내 모든 플레이어에게 메시지를 브로드캐스트합니다. `RoomNetworkManager`의 입/퇴장 이벤트를 구독하여 시스템 메시지도 함께 처리합니다.
- **`ChatController`**: `IChatTransport`(전송)와 `IChatView`(UI) 두 인터페이스만 바라봅니다. 씬에 어떤 Transport 구현체가 연결되든 코드 수정 없이 동일하게 동작하며, 메시지 수신 시 `ChatMessageFormatter`를 통해 발신자와 시스템 메시지를 색상 태그로 포맷하여 UI에 전달합니다.

```
ChatController (Context)
├─ IChatTransport (Strategy)
│ ├─ LobbyChatTransport → Photon Chat SDK (글로벌 채널 구독)
│ └─ RoomChatTransport → PhotonView.RPC (방 내 브로드캐스트)
└─ IChatView
└─ ChatPanelUI → ScrollRect 기반 채팅창 UI
```

https://github.com/dbwoaud/ElementalWar_portfolio/blob/2ffeb1072c5449a83ce88f6e002821254c725c4a/Scripts/Common/Chat/IChatTransport.cs#L1-L13
https://github.com/dbwoaud/ElementalWar_portfolio/blob/2ffeb1072c5449a83ce88f6e002821254c725c4a/Scripts/Common/Chat/IChatView.cs#L1-L8

- [🔗 **LobbyChatTransport.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Lobby/Chat/Lobby%20Chat%20Transport.cs)  
- [🔗 **RoomChatTransport.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Room/Chat/Room%20Chat%20Transport.cs)
- [🔗 **ChatPanelUI.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Common/Chat/Chat%20Panel%20UI.cs)  
- [🔗 **ChatController.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Common/Chat/Chat%20Controller.cs)

---

## **🚀 기술적 도전**

### **1. [멀티플레이] Photon PUN2 기반 실시간 네트워크 동기화**

- **네트워크 오브젝트 풀(`NetworkPoolManager`)**: `IPunPrefabPool`을 직접 구현하여 Photon의 `Instantiate` / `Destroy` 사이클을 커스텀 오브젝트 풀로 대체했습니다. 런타임 중 빈번하게 발생하는 유닛 생성/파괴를 풀링으로 처리하여 GC 부하를 최소화했습니다.
https://github.com/dbwoaud/ElementalWar_portfolio/blob/2ffeb1072c5449a83ce88f6e002821254c725c4a/Scripts/Game/Network/Network%20Pool%20Manager.cs#L43-L79
- [🔗 **NetworkPoolManager.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/Network/Network%20Pool%20Manager.cs)

- **상태 동기화**: 유닛의 상태 전이(State Machine)와 공격 애니메이션을 `PhotonView.RPC`로 상대 클라이언트에 동기화하여, 양측 화면에서 일관된 시각적 표현을 보장합니다.
https://github.com/dbwoaud/ElementalWar_portfolio/blob/2ffeb1072c5449a83ce88f6e002821254c725c4a/Scripts/Game/Units/Components/Unit%20Network%20Sync.cs#L89-L101

- **방 속성(Custom Properties)**: 덱 정보, 준비 상태, 맵 인덱스를 Photon `CustomProperties`에 저장하고, `OnRoomPropertiesUpdate` 콜백으로 처리하여 별도 서버 없이도 두 클라이언트 간 게임 시작 조건을 안전하게 동기화했습니다.
https://github.com/dbwoaud/ElementalWar_portfolio/blob/2ffeb1072c5449a83ce88f6e002821254c725c4a/Scripts/Lobby/Network/Lobby%20Network%20Manager.cs#L48-L86

- **설계 한계 및 개선 방향 (권한 모델)**: 본 프로젝트는 학습을 목적으로 한 클라이언트 권위 구조로, 각 클라이언트가 계산한 결과를 `PhotonView.RPC`로 전파합니다. 이 방식은 구현이 간결하지만 클라이언트가 보낸 값을 서버가 검증하지 않아 메모리 조작 등 치팅에 취약합니다. 상용 서비스 수준에서는 서버가 모든 판정을 수행하고 클라이언트는 입력만 전송하는 서버 권위 구조로 전환하여 데미지 및 상태 판정을 서버에서 검증해야 함을 인지하고 있습니다.

### **2. [인증] PlayFab 기반 회원가입 / 로그인 시스템**

- **구현**: PlayFab SDK를 활용하여 이메일/비밀번호 기반 회원가입 및 로그인을 구현했습니다. 로그인 성공 시 계정 닉네임을 Photon 닉네임과 동기화하여 방 내에서 플레이어 정보를 표시합니다.  
- [🔗 **PlayFabAuthManager.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/MainMenu/Network/PlayFab%20Auth%20Manager.cs)
  
- **에러 처리**: `ErrorTranslator`를 통해 PlayFab 및 Photon 에러 코드를 사용자 친화적 메시지로 변환하고, 공통 팝업 UI(`PopupPanelUIManager`)를 통해 출력하여 에러 처리를 일관되게 관리했습니다.
https://github.com/dbwoaud/ElementalWar_portfolio/blob/2ffeb1072c5449a83ce88f6e002821254c725c4a/Scripts/Common/Constants/Game%20Constants.cs#L180-L238
- [🔗 **PopupPanelUIManager.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Common/Singletons/Popup%20Panel%20UI%20Manager.cs)
  
### **3. [데이터 주도] ScriptableObject 기반 유닛 데이터 설계**

- `UnitStat`을 `ScriptableObject`로 정의하여 게임 로직과 데이터를 완전히 분리했습니다. 새로운 유닛 추가 시 코드 수정 없이 에셋 파일 생성만으로 시스템에 즉시 반영되는 **OCP**를 실천했습니다.
- `UnitDatabase`는 `ScriptableObject`에 `Dictionary<string, UnitStat>`와 `Dictionary<ElementType, List<UnitStat>>` 캐시를 구축하여, 이름 조회와 속성 필터링을 모두 O(1)로 처리했습니다.
- `UnitStat.CalculateDamage()`로 속성 상성 배율(풍>산, 림>풍, 화>림, 산>화)을 데이터 레벨에서 캡슐화하여, 전투 로직이 상성 테이블을 직접 알 필요가 없도록 설계했습니다.  
- [🔗 **UnitStat.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Common/Data/Unit%20Stat.cs)  
- [🔗 **UnitDatabase.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Common/Data/Unit%20Database.cs)

### **4. [UX] 덱 편성 드래그 앤 드롭 시스템**

- Unity의 `IBeginDragHandler`, `IDragHandler`, `IEndDragHandler`, `IDropHandler` 인터페이스를 각 슬롯 Item(`DeckSlotItem`, `UnitSlotItem`)에 구현하여 직관적인 덱 편성 UX를 제공합니다.
- 드래그 중 고스트 이미지를 Canvas 최상단에 렌더링하고, 드롭 타겟 감지를 통해 유닛 슬롯→덱 슬롯 할당, 덱 슬롯 간 스왑을 구분 처리합니다.
- 이벤트는 Item → Container → UIManager → Controller로 버블링되어 실제 덱 데이터(`DeckModel`) 변경은 Controller 계층에서만 이루어집니다.  
- [🔗 **DeckSlotItem.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Unit%20Setting/UI/Deck%20Slot%20Item.cs)  
- [🔗 **UnitSlotItem.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Unit%20Setting/UI/Unit%20Slot%20Item.cs)

---

## **📈 최적화 & 성능 계측**

실시간 1v1 대전에서는 양측 유닛이 동시에 수백 명까지 늘어나므로, **매 프레임 반복되는 탐색 및 할당**과 **유닛 수에 비례해 증가하는 네트워크 트래픽**이 병목이 될 것으로 판단했습니다. 이를 검증하기 위해 [지하 10층](https://github.com/dbwoaud/Basement10_portfolio)의
`PerformanceLogger`를 확장한 **`NetworkPerformanceLogger`** 를 제작해, 프레임 지표와 네트워크 지표를 1초 단위로 함께 수집했습니다.

### **계측 방법**

```
Unity 6000.4.7f1 · IL2CPP · Development Build · VSync Off
1920×1080 / 동일 PC로 2인스턴스 실행(마스터, 게스트 동시 계측)
시나리오: 고정 간격 자동 소환 후 60초 유지 (1초 단위 샘플링, 59행)
```

각 최적화는 **런타임 스위치로 해당 항목만 비활성화한 빌드**를 별도로 만들어 before를 측정했으며, 각 수집 항목은 프레임 타임(평균, P95), GC 할당량과 수집 횟수, 물리 질의 횟수, 상태 머신 Tick 수, RPC 발행 수, 송신 바이트, RTT, 재전송 커맨드로 설정했습니다.

> **GC 할당은 "프레임당"이 아니라 "초당"으로 정규화했습니다.** 프레임당 수치는 FPS에 반비례하므로, FPS가 다른 두 실행을 프레임당 지표로 비교하면 개선되지 않은 항목이 개선된 것처럼 보입니다. 실제로 실험 1에서 프레임당 GC 할당은 −1.5%였지만 초당으로 환산하면 −0.6%로 사실상 차이가 없었습니다.

- [🔗 **NetworkPerformanceLogger.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Common/Profiling/NetworkPerformanceLogger.cs)

---

### **1. 탐색 주기 분리 & 소유 유닛 한정 Tick(유닛 수 확장성)**

- **스캔 스로틀링**: Idle, Move 상태의 적 탐색을 매 프레임이 아닌 **0.1초 간격**으로 수행
- **소유 유닛 한정 Tick**: `Unit.Update()`가 자신이 소유한 유닛에서만 상태 머신을 Tick하고, 상대 유닛은 RPC로 전달된 상태만 재생

단일 수치보다 **유닛 수에 따른 증가 곡선**이 중요하다고 판단해, 최대 동시 유닛 수를 200 / 400 / 800 명으로 나눠 3개 조합을 각각 측정했습니다.

**평균 프레임 타임 (ms)**
| 최대 유닛 수 | 미적용 | 스로틀만 | **둘 다 적용** | 개선율 |
|---|---|---|---|---|
| 200명 | 2.13 | 2.02 | **1.69** | **−20.7%** |
| 400명 | 7.80 | 6.80 | **3.78** | **−51.5%** |
| 800명 | 27.08 | 17.67 | **13.10** | **−51.6%** |

**유닛 1명당 프레임당 물리 질의 횟수**
| 최대 유닛 수 | 미적용 | 스로틀만 | **둘 다 적용** |
|---|---|---|---|
| 200명 | 1.334 | 0.896 | **0.488** |
| 400명 | 1.181 | 1.142 | **0.529** |
| 800명 | 0.602 | 0.439 | **0.337** |

**초선형 구간 완화**
| 조합 | 200명 | 800명 | 증가 배율 |
|---|---|---|---|
| 미적용 | 2.13 ms | 27.08 ms | **12.7배** |
| 둘 다 적용 | 1.69 ms | 13.10 ms | **7.8배** |

**미적용: 유닛 700명 구간**

https://github.com/user-attachments/assets/5eecc863-dddd-488d-9eba-9f7fe92e5e42

**둘 다 적용: 유닛 750명 구간**

https://github.com/user-attachments/assets/80ab5009-1f6e-45a8-9a64-63bfcb0e2272

**계측으로 확인한 것**: 스캔 스로틀링 단독 효과는 200명 구간에서 −5%에 그쳤습니다. 스로틀은 Idle, Move 상태만 대상이고 **Attack 상태의 사거리 확인은 매 프레임 그대로 실행**되기 때문입니다. 실제 이득의 대부분은 소유 유닛 한정 Tick에서 나온 것을 확인할 수 있습니다.

---

### **2. 비할당 물리 질의(전투 탐색의 GC 제거)**

적 탐색은 유닛마다 초당 여러 번 실행되므로, 배열을 새로 반환하는 `OverlapBoxAll` / `OverlapCircleAll` 대신 **사전 할당 버퍼와 `ContactFilter2D`를 받는 오버로드**를 사용했습니다.

| 대상 | 버퍼 | 목적 |
|---|---|---|
| 사거리 스캔 | `Collider2D[16]` | 근접, 원거리 타겟 탐색 |
| 광역 피해 수집 | `Collider2D[32]` | AOE 반경 내 적 수집 |
| 피해 적용 대상 | 재사용 `List<Collider2D>` | 프레임당 리스트 재할당 방지 |

최대 유닛 60명 규모, 60초 교전 기준입니다.

| 지표 | before (`~All` 오버로드) | after (버퍼 재사용) | 변화 |
|---|---|---|---|
| **초당 GC 할당 (마스터)** | 3,236 KB/s | **146 KB/s** | **−95.5%** |
| **초당 GC 할당 (게스트)** | 2,470 KB/s | **145 KB/s** | **−94.1%** |
| **GC 수집 횟수 (마스터)** | 114회 | **6회** | **−94.7%** |
| GC 수집 횟수 (게스트) | 86회 | **6회** | −93.0% |
| 평균 프레임 타임 (게스트) | 0.89 ms | 0.77 ms | −13.5% |
| P95 프레임 타임 (게스트) | 27.85 ms | 11.13 ms | −60.0% |

**미적용: GC Allocated In Frame 톱니가 지속적으로 누적**

https://github.com/user-attachments/assets/4301e411-8336-4339-a0b7-ea81230633ba

**적용: 할당이 거의 발생하지 않음**

https://github.com/user-attachments/assets/64f9dc95-73a5-427a-b09c-1b2d7844f963

유닛 60명이라는 가벼운 조건에서도 **60초간 GC 수집이 114회에서 6회로 감소**했습니다. 프레임 타임 개선폭이 크지 않은 것은 이 규모에서 CPU 여유가 충분하기 때문이며, 유닛 수가 늘어날수록 GC 스파이크로 나타났을 부하를 사전에 제거한 것으로 해석했습니다.

- [🔗 **UnitCombat.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/Units/Components/Unit%20Combat.cs)

---

### **3. 상태 동기화 RPC 지연 전송(측정 후 채택하지 않은 최적화)**

`UnitStateIdle`은 진입 즉시 스캔해 Move나 Attack으로 빠지는 **경유 상태**입니다. 1프레임만 머무는 전이까지 RPC로 전파하는 것이 낭비라고 판단해, Idle 전이를 50ms 지연시킨 뒤 그때도 여전히 Idle이면 전송하는 방식을 시험했습니다.

최대 유닛 200명 규모, 60초 교전 기준입니다.

| 지표 | baseline | 지연 전송 | 변화 |
|---|---|---|---|
| 초당 RPC 발행 | 63.5 | 56.0 | **−11.8%** |
| **재전송 커맨드** | 564회 | **68회** | **−87.9%** |
| **초당 송신 바이트** | 8,650 B/s | **11,759 B/s** | **+35.9%** |
| **RPC 1건당 바이트** | 136 B | **210 B** | **+54.4%** |
| 평균 RTT | 29.8 ms | 31.1 ms | +4.3% |

**RPC 수는 줄었는데 총 트래픽은 오히려 36% 늘었습니다.**

Photon은 SendRate(기본 초당 30회) 주기로 메시지를 묶어 전송합니다. Idle RPC를 50ms 지연시키면 원래 함께 묶여 나가던 배치를 놓치고 별도 패킷으로 전송되면서 패킷 헤더 비용이 추가됩니다. RPC 1건당 바이트가 136B에서 210B로 늘어난 것이 이를 뒷받침합니다.

재전송 커맨드가 88% 감소한 것은 명확한 이득이지만, 대역폭 손해가 더 크다고 판단해 이 최적화는 채택하지 않았습니다. 다만 baseline에서 60초간 재전송이 564회 발생한 사실 자체가 RPC 버스트로 신뢰성 큐가 포화되고 있다는 신호이므로, 지연 전송이 아닌 전송량 자체를 줄이는 방향을 후속 과제로 남겨두었습니다.

---

### **4. 네트워크 오브젝트 풀(가설이 기각된 항목)**

`IPunPrefabPool`을 직접 구현해 Photon의 생성 및 파괴를 큐 기반 재사용으로 대체했습니다. 유닛 사망이 잦은 게임이므로 생성 및 파괴 비용이 병목일 것으로 예상했습니다.

최대 유닛 1000명, 60초 교전 기준입니다.

| 지표 | before (풀링 미적용) | after (풀링 적용) | 변화 |
|---|---|---|---|
| 초당 GC 할당 (마스터) | 1,314 KB/s | 1,306 KB/s | −0.6% |
| GC 수집 횟수 (마스터) | 31회 | 30회 | −3.2% |
| 평균 프레임 타임 (마스터) | 18.44 ms | 18.24 ms | −1.1% |
| 평균 프레임 타임 (게스트) | 16.27 ms | 18.20 ms | **+11.9%** |

**유의미한 차이가 없었고, 마스터와 게스트의 방향도 반대로 나와 실행 간 편차 범위로 판단했습니다.**

원인은 명확했습니다. 60초 동안 1000명을 소환하는 동안 **사망은 182명(초당 3.1회)에 그쳤습니다.** `Instantiate` / `Destroy` 1회 비용이 수십 μs 수준이므로, 초당 3회로는 전체 프레임 예산에서 차지하는 비중이 애초에 무시할 수준이었습니다.

대신 이 계측에서 **유닛 430명 → 790명 구간에서 프레임 타임이 1.85 ms → 10.78 ms로 5.8배 증가**하는 초선형 구간을 발견했습니다. 병목은 유닛 생성이 아니라 **살아있는 유닛의 매 프레임 처리**에 있었고, 이에 따라 계측 대상을 탐색 주기와 상태 머신 Tick으로 재조정한 것이 위 1번 실험입니다.

풀 자체는 코드 복잡도가 낮고 유닛 회전율이 높은 상황에 대비한 안전장치로서 가치가 있다고 판단해 유지했습니다.

- [🔗 **NetworkPoolManager.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/Network/Network%20Pool%20Manager.cs)

---

### **5. 조회 자료구조 — 선형 탐색 제거**

설계 단계에서 적용한 항목으로, 복잡도 개선이 자명해 별도 before/after는 측정하지 않았습니다.

| 대상 | 기존 방식의 비용 | 적용한 자료구조 | 복잡도 |
|---|---|---|---|
| 씬 내 활성 유닛 조회 | `FindObjectsOfType` *O(N)* | `UnitRegistry`의 `HashSet<Unit>` | 등록/해제 *O(1)* |
| 상태 전이 | `switch` 분기 | `Dictionary<UnitStateType, IUnitState>` | 조회 *O(1)* |
| 유닛 이름 조회 | `List` 선형 탐색 | `Dictionary<string, UnitStat>` | *O(1)* |
| 속성별 유닛 필터링 | 매번 `Where` 순회 | `Dictionary<ElementType, List<UnitStat>>` 캐시 | *O(1)* |

성 체력 UI도 매 피격마다 문자열을 새로 만들지 않고, 직전 문자열과 동일하면 갱신을 건너뛰어 불필요한 문자열 할당과 UI 리빌드를 줄였습니다.

---

### **계측에서 배운 것**

**네 건 중 두 건은 예상이 빗나갔습니다.** 가장 확실할 것으로 본 오브젝트 풀은 차이가 없었고, RPC 지연 전송은 호출 수를 줄였는데 트래픽이 오히려 늘었습니다.
하지만 예상이 빗나간 실험은 다음 실험의 방향을 결정했습니다. 풀링 계측에서 관찰한 초선형 구간이 없었다면 탐색 주기와 Tick 범위를 의심하지 못했을 것이고, 51%의 프레임 타임 개선도 나오지 않았을 것입니다.

---

## **⚠️ 트러블슈팅: HeroEditor 에셋 연동 이슈**
 
### **이슈 1. 죽음 애니메이션 재생 시 그래픽 깨짐**

**문제 상황**

HeroEditor 유닛이 죽음 애니메이션과 함께 페이드 아웃 코루틴을 실행할 때, 캐릭터의 얼굴 스프라이트의 마스크 경계가 하얗게 뭉개지는 그래픽 깨짐 현상이 발생했습니다.

**원인 분석**
HeroEditor `Character`는 내부적으로 장비 레이어링을 위해 `SpriteMask`와 커스텀 머티리얼을 사용합니다. 페이드 아웃 중 알파값을 직접 조작하면 커스텀 머티리얼의 렌더링 순서와 마스크 처리가 충돌하여 시각적인 깨짐이 발생했습니다.

**해결 방법**

페이드 아웃 진입 시점에 모든 `SpriteRenderer`의 머티리얼을 HeroEditor 커스텀 머티리얼에서 표준 `Sprites/Default` 머티리얼로 교체하는 방식으로 해결했습니다. `HeroEditorAdapter`의 `CacheRenderers()`에서 원본 머티리얼을 `Dictionary<SpriteRenderer, Material>`에 미리 저장해 두고, 페이드 아웃 종료 후 `RestoreOriginalMaterials()`로 복구하여 재사용 시에도 원본 상태가 유지되도록 했습니다.  
- [🔗 **HeroEditorAdapter.cs 코드 보기**](https://github.com/dbwoaud/ElementalWar_portfolio/blob/main/Scripts/Game/Units/Animators/Hero%20Editor%20Adapter.cs)
  
---
 
### **이슈 2. 오브젝트 풀 재사용 시 유닛이 누운 채로 이동하는 현상**
 
**문제 상황** 

유닛이 죽어 오브젝트 풀로 반환된 후 재생성되면, 죽음 애니메이션 도중 HeroEditor가 변경한 `body` 오브젝트의 `transform.rotation` 값이 초기화되지 않아 유닛이 누운 자세로 이동하는 현상이 발생했습니다.

**코드 기반 해결 시도 (실패)**

`ResetForReuse()` 내에서 `body` Transform을 찾아 `Quaternion.identity`로 강제 리셋하는 코드를 작성했습니다. 그러나 Animator 갱신 타이밍 문제로 인해 코드 수정만으로는 안정적으로 초기화되지 않았습니다.

**해결 방법**
 
코드 수정이 아닌 **애니메이션 클립 에셋 자체를 수정**하는 방향으로 전환했습니다. Unity Animation 에디터에서 HeroEditor의 Idle 애니메이션 클립 첫 번째 프레임에 `body`의 `transform.rotation`을 `(0, 0, 0, 1)`로 고정하는 키프레임을 직접 삽입했습니다. 재사용 후 Idle 상태가 되는 순간 첫 프레임에서 rotation이 강제로 정상화되어, 어떤 죽음 애니메이션 상태에서 풀로 반환되더라도 재사용 시 올바른 자세를 보장하도록 했습니다.
 
### **결과 및 배운 점**
 
- **기술적 시야 확장**: 코드로 해결하려는 관성에서 벗어나, 문제가 애니메이션 데이터에서 비롯된 경우 클립을 직접 수정하는 것이 더 근본적인 해결책이 될 수 있음을 배웠습니다.
- **에셋 연동의 복잡성**: 서드파티 에셋은 내부 구현이 블랙박스에 가깝기 때문에, 단순히 API를 호출하는 것 이상으로 렌더링 파이프라인과 애니메이션 시스템의 동작 원리를 이해해야 한다는 점을 체감했습니다.
- **어댑터 패턴의 가치**: 이 과정에서 HeroEditor의 특수한 처리를 `HeroEditorAdapter` 내부에 완전히 캡슐화할 수 있었으며, 유닛 시스템은 이를 전혀 인지하지 않아도 정상 동작하는 설계의 이점을 실감했습니다.

---

## **📐 클래스 다이어그램**

전체 구조는 [**Docs/ClassDiagram.md**](Docs/ClassDiagram.md)에서 확인할 수 있습니다.

---

## **🔗 참조**

- **Notion**: [[엘리멘탈 워 Notion 링크]](https://pinnate-earthworm-118.notion.site/35cfaf7d496e801c870ece488cbb2c5c)
- **YouTube**: [[기술 데모 영상 링크]](https://www.youtube.com/watch?v=EoBB8FgUPCY)
- **GamePlay**: [[엘리멘탈 워 게임 플레이 링크]](https://play.unity.com/en/games/b00590fc-0f6b-461a-aa6a-c77404829354/7zkn66a87zmu7ikwioyghoyfgq)
