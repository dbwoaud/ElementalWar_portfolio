# ElementalWar_portfolio
# **🕹️ [프로젝트] 엘리멘탈 워 (Elemental War)**

> **"냥코 대전쟁 게임과 전쟁 시대 플레시 게임에서 영감을 얻어, 속성 상성 시스템과 실시간 멀티플레이를 결합한 2D 사이드뷰 타워 디펜스 게임입니다."**

---

## **📌 프로젝트 개요**

- **개발 인원**: 1인 개발 (기획, 프로그래밍, 리소스 관리)
- **개발 기간**: 2026.04 ~ 2026.X05
- **기술 스택**: Unity, C#, Photon PUN2, Photon Chat, PlayFab, ScriptableObject
- **핵심 컨셉**: 풍/림/화/산/무 5속성 상성 시스템을 기반으로 한 실시간 1v1 멀티플레이 타워 디펜스
- **주요 성과**: 씬 전반에 걸친 MVC 아키텍처 적용, 다수의 GoF 디자인 패턴 실전 구현, 네트워크 동기화 기반 멀티플레이 구현

---

## **🛠️ 시스템 아키텍처 & 디자인 패턴**

컴퓨터공학과 전공자로서 **유지보수성, 확장성, 가독성**을 고려한 설계를 지향했습니다.

---

### **1. 씬 구조의 MVC 패턴 — `BaseSceneController` / `BaseUIManager` / `NetworkManager`**

모든 씬(MainMenu, Lobby, Room, UnitSetting, Game)에 일관된 **MVC 아키텍처**를 적용했습니다.

- **Controller (Model)**: `BaseSceneController<T>`를 상속한 각 씬의 매니저(`MainMenuManager`, `LobbyManager`, `UnitSettingManager`, `GameManager`)가 게임 로직과 상태를 담당합니다.
- **View**: `BaseUIManager<T>`를 상속한 UI 매니저(`MainMenuUIManager`, `LobbyUIManager`, `UnitSettingUIManager`, `GameUIManager`)가 화면 출력만 담당하며, 직접적인 로직 처리 없이 이벤트를 발행하여 Controller에 상태를 전달합니다.
- **Model(Network)**: `MainMenuNetworkManager`,`LobbyNetworkManager`, `RoomNetworkManager`, `UnitSettingNetworkManager`, `GameNetworkManager` 등 씬별 네트워크 매니저가 Photon PUN2 콜백을 수신하고, 직접적인 로직 처리 없이 이벤트를 발행하여 Controller에 상태를 전달합니다.

이 구조를 통해 UI, 게임 로직, 네트워크 로직의 책임을 완전히 분리했으며, `BaseSceneController`와 `BaseUIManager`의 추상 메서드(`SetUIManager`, `SetNetworkManager`, `InitUIElements`, `BindUIEvents`)를 통해 **Template Method 패턴**을 병행 적용하여 각 씬의 초기화 흐름을 강제하고 코드 누락을 방지했습니다.


---

### **2. UI 계층의 Composite 패턴 — `UIPanel` / `Container` / `Item`**

UI 코드 전반에 **Composite 패턴**을 적용하여 단일 컴포넌트와 복합 컨테이너를 동일한 인터페이스로 다룰 수 있도록 설계했습니다.

```
UIManager (최상위 조합자)
     ├─ LoginPanel, GameResultPanel, GameLoadingPanel … (Leaf)
     └─ Container (Composite — 자식 Item들을 포함)
          ├─ DeckSlotContainer
          │    └─ DeckSlotItem × N
          ├─ UnitSlotContainer
          │    └─ UnitSlotItem × N
          └─ PlayerSlotContainer
               └─ PlayerSlotItem × N
```

- **`UIPanel`**: `Show()`, `Hide()`, `HideImmediate()`, `InitializeListener()`, `UnregisterListener()`, `ResetUI()` 를 정의하는 공통 기반 클래스로, 모든 패널은 이를 상속하여 생명주기를 통일합니다.
- **`BasePopupPanel`**: `UIPanel`을 상속하여 게임 내의 여러 팝업 패널을 담당하는 계층입니다.
- **Container 클래스들**: 내부의 Item들을 배열로 보유하고, 이벤트를 집계하여 상위로 버블링합니다. UIManager는 Container의 세부 Item 수나 종류를 알 필요 없이 단일 인터페이스로 조작합니다.

---

### **3. Unit에 적용된 다중 디자인 패턴**

유닛 시스템은 여러 GoF 패턴의 조합으로 설계되어 있습니다.

#### **3-1. Component 패턴 — `Unit` + `UnitStats` / `UnitMovement` / `UnitCombat` / `UnitStateMachine` / `UnitNetworkSync`**

`Unit` 클래스는 직접 로직을 구현하지 않습니다. 기능별로 분리된 컴포넌트들에 책임을 위임하고 외부에 프로퍼티로 노출하는 **퍼사드(Facade)** 역할만 수행합니다. `[RequireComponent]` attribute를 통해 필수 컴포넌트 누락을 컴파일 타임에 방지했습니다.

#### **3-2. State 패턴 — `IUnitState` / `UnitStateMachine`**

유닛의 행동(Idle, Move, Attack, Hit, Dead)을 `IUnitState` 인터페이스로 추상화하고, `UnitStateMachine`이 상태 전이를 관리합니다. 새로운 상태 추가 시 기존 코드를 수정할 필요가 없습니다.

`UnitStateMachine`은 `Dictionary<UnitStateType, IUnitState>`로 상태를 관리하여 열거형으로 O(1) 조회가 가능하며, 상태 전이 시 `OnStateChanged` 이벤트를 발행하여 네트워크 동기화와 느슨하게 결합됩니다.

#### **3-3. Observer 패턴 — 상태 전이 네트워크 동기화**

`UnitStateMachine`의 `OnStateChanged` 이벤트를 `UnitNetworkSync`가 구독함으로써, 상태기계는 네트워크 존재를 알 필요 없이, 상태 변경만 발행하면 동기화가 자동으로 처리됩니다.

#### **3-4. Registry 패턴 — `UnitRegistry`**

`HashSet<Unit>` 기반의 정적 레지스트리를 통해 씬 내 모든 활성 유닛을 O(1)로 등록/해제하고, `IReadOnlyCollection`으로 외부 읽기 전용 노출하여 데이터 무결성을 보장합니다.

---

### **4. 어댑터 패턴 — `IUnitAnimator` / `BaseUnitAnimator` / 에셋별 Adapter**
 
리소스 한계로 인해 서로 다른 구조를 가진 세 가지 유닛 에셋을 동시에 사용해야 했습니다. 각 에셋은 애니메이션 재생 방식과 내부 API가 완전히 달랐기 때문에, 유닛 시스템이 에셋 종류를 직접 알게 되면 코드가 에셋에 강하게 결합되는 문제가 발생합니다.
 
이를 해결하기 위해 **어댑터 패턴**을 적용했습니다.
 
- **Target Interface (`IUnitAnimator`)**: `PlayIdle()`, `PlayMove()`, `PlayAttack()`, `PlayHit()`, `PlayDead()`, `ResetForReuse()` 등 유닛 시스템이 요구하는 공통 인터페이스를 정의합니다.
- **Base Adapter (`BaseUnitAnimator`)**: 피격 플래시, 페이드 아웃 코루틴 등 에셋 종류에 관계없이 공통으로 사용되는 로직을 구현하고, 에셋별 차이점은 추상 메서드(`OnPlayDeadInternal`, `OnResetForReuseInternal`, `ApplyFlashColor` 등)로 위임합니다.
- **Concrete Adapter**: 각 에셋의 실제 API를 공통 인터페이스로 변환합니다.
  - `HeroEditorAdapter` → `Character.SetState(CharacterState)` 호출
  - `FantasyMonsterAdapter` → `Monster.SetState(MonsterState)` 호출
  - `SpineMonsterAdapter` → `SkeletonAnimation.AnimationState.SetAnimation()` 호출
```
IUnitAnimator (Target)
    └─ BaseUnitAnimator (공통 로직 추상 기반)
            ├─ HeroEditorAdapter   (HeroEditor Character API → IUnitAnimator)
            ├─ FantasyMonsterAdapter (Fantasy Monster API → IUnitAnimator)
            └─ SpineMonsterAdapter  (Spine SkeletonAnimation API → IUnitAnimator)
```
 
유닛의 `Unit` 클래스와 `UnitStateMachine`은 `IUnitAnimator` 인터페이스만 바라보기 때문에, 어떤 에셋을 사용하는 유닛이든 코드 수정 없이 동일하게 동작합니다. 새로운 에셋 추가 시에도 Adapter 클래스 하나만 추가하면 됩니다.

---

### **5. 싱글톤 & 매니저 패턴 — `Singleton<T>` & Bootstrap**

- `Singleton<T>` 베이스 클래스를 상속받아 `SoundManager`, `NetworkPoolManager`, `PopupPanelUIManager` 등 핵심 시스템의 단일성을 보장하고 전역 접근성을 확보했습니다.
- `GameBootstrap`에서 씬 진입 전 `DontDestroyOnLoad` 전역 매니저들을 일괄 생성하여 씬 이동 간 상태를 안전하게 유지합니다.

---

## **🚀 기술적 도전**

### **1. [멀티플레이] Photon PUN2 기반 실시간 네트워크 동기화**

- **네트워크 오브젝트 풀(`NetworkPoolManager`)**: `IPunPrefabPool`을 직접 구현하여 Photon의 `Instantiate` / `Destroy` 사이클을 커스텀 오브젝트 풀로 대체했습니다. 런타임 중 빈번하게 발생하는 유닛 생성/파괴를 풀링으로 처리하여 GC 부하를 최소화했습니다.
- **상태 동기화**: 유닛의 상태 전이(State Machine)와 공격 애니메이션을 `PhotonView.RPC`로 상대 클라이언트에 동기화하여, 양측 화면에서 일관된 시각적 표현을 보장합니다.
- **방 속성(Custom Properties)**: 덱 정보, 준비 상태, 맵 인덱스를 Photon `CustomProperties`에 저장하고 `OnRoomPropertiesUpdate` 콜백으로 처리하여, 별도 서버 없이도 두 클라이언트 간 게임 시작 조건을 안전하게 동기화했습니다.

### **2. [인증] PlayFab 기반 회원가입 / 로그인 시스템**

- **구현**: PlayFab SDK를 활용하여 이메일/비밀번호 기반 회원가입 및 로그인을 구현했습니다. 로그인 성공 시 계정 닉네임을 Photon 닉네임과 동기화하여 방 내에서 플레이어 정보를 표시합니다.
- **에러 처리**: `ErrorTranslator`를 통해 PlayFab 및 Photon 에러 코드를 사용자 친화적 메시지로 변환하고, 공통 팝업 UI(`PopupPanelUIManager`)를 통해 출력하여 에러 처리를 일관되게 관리했습니다.

### **3. [데이터 주도] ScriptableObject 기반 유닛 데이터 설계**

- `UnitStat`을 `ScriptableObject`로 정의하여 게임 로직과 데이터를 완전히 분리했습니다. 새로운 유닛 추가 시 코드 수정 없이 에셋 파일 생성만으로 시스템에 즉시 반영되는 **개방-폐쇄 원칙(OCP)**을 실천했습니다.
- `UnitDatabase`는 `ScriptableObject`에 `Dictionary<string, UnitStat>`와 `Dictionary<ElementType, List<UnitStat>>` 캐시를 구축하여, 이름 조회와 속성 필터링을 O(1) / O(1) 복잡도로 처리합니다.
- `UnitStat.CalculateDamage()`로 속성 상성 배율(풍>대지, 삼림>풍, 화염>삼림, 대지>화염)을 데이터 레벨에서 캡슐화하여, 전투 로직이 상성 테이블을 직접 알 필요가 없도록 설계했습니다.

### **4. [UX] 덱 편성 드래그 앤 드롭 시스템**

- Unity의 `IBeginDragHandler`, `IDragHandler`, `IEndDragHandler`, `IDropHandler` 인터페이스를 각 슬롯 Item(`DeckSlotItem`, `UnitSlotItem`)에 구현하여 직관적인 덱 편성 UX를 제공합니다.
- 드래그 중 고스트 이미지를 Canvas 최상단에 렌더링하고, 드롭 타겟 감지를 통해 유닛 슬롯→덱 슬롯 할당, 덱 슬롯 간 스왑을 구분 처리합니다.
- 이벤트는 Item → Container → UIManager → Controller로 버블링되어 실제 덱 데이터(`DeckModel`) 변경은 Controller 계층에서만 이루어집니다.

---

## **⚠️ 트러블 슈팅: HeroEditor 에셋 연동 이슈**
 
### **1. 문제 상황 — 죽음 애니메이션 재생 시 그래픽 깨짐**
 
- **상황**: HeroEditor 유닛이 죽음 애니메이션(`PlayDead`)과 함께 페이드 아웃 코루틴을 실행할 때, 캐릭터의 일부 스프라이트가 검게 찌그러지거나 마스크 경계가 뭉개지는 그래픽 깨짐 현상이 발생했습니다.
- **원인 분석**: HeroEditor `Character`는 내부적으로 장비 레이어링을 위해 `SpriteMask`와 커스텀 머티리얼을 사용합니다. 페이드 아웃 중 알파값을 직접 조작하면 커스텀 머티리얼의 렌더링 순서와 마스크 처리가 충돌하여 시각적 아티팩트가 발생했습니다.
### **2. 해결 방법**
 
페이드 아웃 진입 시점에 모든 `SpriteRenderer`의 머티리얼을 HeroEditor 커스텀 머티리얼에서 표준 `Sprites/Default` 머티리얼로 교체하는 방식으로 해결했습니다. `HeroEditorAdapter`의 `CacheRenderers()`에서 원본 머티리얼을 `Dictionary<SpriteRenderer, Material>`에 미리 저장해 두고, 페이드 아웃 종료 후 `RestoreOriginalMaterials()`로 복구하여 재사용 시에도 원본 상태가 유지되도록 했습니다.
  
---
 
### **3. 문제 상황 — 오브젝트 풀 재사용 시 유닛이 누운 채로 이동하는 현상**
 
- **상황**: 유닛이 죽어 오브젝트 풀로 반환된 후 재생성(Reuse)되면, 죽음 애니메이션 도중 HeroEditor가 변경한 `body` 오브젝트의 `transform.rotation` 값이 초기화되지 않아 유닛이 누운 자세로 이동하는 현상이 발생했습니다.
- **코드 기반 해결 시도 (실패)**: `ResetForReuse()` 내에서 `body` Transform을 찾아 `Quaternion.identity`로 강제 리셋하는 코드를 작성했습니다. 그러나 HeroEditor의 `SetState(CharacterState.Idle)` 호출이 내부적으로 다시 rotation을 덮어쓰거나, Animator 갱신 타이밍 문제로 인해 코드 수정만으로는 안정적으로 초기화되지 않았습니다.
### **4. 해결 방법 — 애니메이션 클립 직접 수정**
 
코드 수정이 아닌 **애니메이션 클립 에셋 자체를 수정**하는 방향으로 전환했습니다. Unity Animation 에디터에서 HeroEditor의 Idle 애니메이션 클립 첫 번째 프레임에 `body`의 `transform.rotation`을 `(0, 0, 0, 1)`(Quaternion.identity)로 고정하는 키프레임을 직접 삽입했습니다.
 
이로 인해 재사용 후 Idle 상태가 되는 순간 첫 프레임에서 rotation이 강제로 정상화되어, 어떤 죽음 애니메이션 상태에서 풀로 반환되더라도 재사용 시 올바른 자세를 보장할 수 있게 되었습니다.
 
### **5. 결과 및 배운 점**
 
- **기술적 시야 확장**: 코드로 해결하려는 관성에서 벗어나, 문제가 애니메이션 데이터에서 비롯된 경우 클립을 직접 수정하는 것이 더 근본적인 해결책이 될 수 있음을 배웠습니다.
- **에셋 연동의 복잡성**: 서드파티 에셋은 내부 구현이 블랙박스에 가깝기 때문에, 단순히 API를 호출하는 것 이상으로 렌더링 파이프라인과 애니메이션 시스템의 동작 원리를 이해해야 한다는 점을 체감했습니다.
- **어댑터 패턴의 가치**: 이 과정에서 HeroEditor의 특수한 처리(머티리얼 교체, SpriteMask 토글)를 `HeroEditorAdapter` 내부에 완전히 캡슐화할 수 있었으며, 유닛 시스템은 이를 전혀 인지하지 않아도 정상 동작하는 설계의 이점을 실감했습니다.
---

## **🔗 참조**

- **Notion**: [[엘리멘탈 워 Notion 링크]]
- **YouTube**: [[기술 데모 영상 링크]]
- **GammPlay** [[엘리멘탈 워 게임 플레이링크]]
