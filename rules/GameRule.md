# Adventurer 프로젝트 코딩 규칙

이 문서는 Adventurer Unity 프로젝트의 C# 스크립트 작성 시 따라야 할 코딩 규칙과 컨벤션을 정의합니다. 모든 개발자는 이 규칙을 준수하여 코드의 일관성과 가독성을 유지합니다.

---

## 1. 네이밍 규칙

### 1.1 변수 (Variables)

- **기본 규칙**: **CamelCase** 사용
- **멤버 변수**: CamelCase로 작성
- **지역 변수**: CamelCase로 작성
- **상수**: PascalCase 사용 (C# 컨벤션)

#### Boolean 타입 변수

- **접두어 사용**: `is`, `has`, `can`, `should` 등의 접두어를 사용
- **동사 우선**: 가능하면 동사 의미를 앞에 배치

```csharp
// ❌ BAD
bool player;
bool dead;
bool jump;

// ✅ GOOD
bool isPlayer;
bool isDead;
bool canJump;
bool hasWeapon;
bool shouldAttack;
```

#### 일반 변수 예시

```csharp
// ❌ BAD
int playerhealth;
string playername;
float movespeed;

// ✅ GOOD
int playerHealth;
string playerName;
float moveSpeed;
Vector3 currentPosition;
GameObject targetEnemy;
```

### 1.2 함수/메서드 (Functions/Methods)

- **기본 규칙**: **PascalCase** 사용
- **동사로 시작**: 메서드 이름은 항상 동사로 시작
- **Unity 콜백 예외**: Unity의 기본 콜백 메서드(`Update`, `Start`, `Awake`, `OnEnable` 등)는 예외

```csharp
// ❌ BAD
void playerMove();
void health();
void damage(int amount);

// ✅ GOOD
void MovePlayer();
void CalculateHealth();
void ApplyDamage(int amount);
void GetPlayerPosition();
bool IsPlayerAlive();
```

#### Boolean 반환 메서드

```csharp
// ❌ BAD
bool check();
bool player();

// ✅ GOOD
bool IsPlayerAlive();
bool HasWeapon();
bool CanJump();
bool ShouldAttack();
```

### 1.3 클래스/구조체/인터페이스 (Classes/Structs/Interfaces)

- **기본 규칙**: **PascalCase** 사용
- **명확한 역할**: 클래스 이름은 역할이 명확하게 드러나야 함

```csharp
// ❌ BAD
class player;
class manager;
class data;

// ✅ GOOD
class PlayerController;
class GameManager;
class PlayerData;
interface IAttackable;
enum PlayerState;
```

#### Unity 컴포넌트 네이밍 권장 패턴

- `SomethingController`: 입력 및 제어 담당
- `SomethingManager`: 시스템 관리 담당
- `SomethingView`: UI 표시 담당
- `SomethingData`: 데이터 저장 담당
- `SomethingHandler`: 이벤트 처리 담당

```csharp
// ✅ GOOD
class PlayerController : MonoBehaviour { }
class UIManager : MonoBehaviour { }
class HealthBarView : MonoBehaviour { }
class PlayerData : ScriptableObject { }
class DamageHandler : MonoBehaviour { }
```

---

## 2. 폴더 및 파일 구조

### 2.1 파일 이름 규칙

- **클래스명과 파일명 일치**: C# 스크립트 파일명은 반드시 클래스명과 동일해야 함
- **파일 확장자**: `.cs` 사용

```csharp
// 파일: PlayerController.cs
public class PlayerController : MonoBehaviour { }

// 파일: GameManager.cs
public class GameManager : MonoBehaviour { }
```

### 2.2 Unity 프로젝트 폴더 구조

기본 Unity 폴더 구조를 따릅니다:

```
Assets/
├── Scripts/           # 모든 C# 스크립트
│   ├── Characters/    # 캐릭터 관련 스크립트
│   ├── UI/            # UI 관련 스크립트
│   ├── Systems/       # 게임 시스템 스크립트
│   ├── Managers/      # 매니저 클래스
│   └── Data/          # 데이터 클래스, ScriptableObject
├── Scenes/            # 씬 파일
├── Prefabs/           # 프리팹
├── Materials/         # 머티리얼
├── Textures/          # 텍스처
├── Animations/        # 애니메이션
└── Audio/             # 오디오 파일
```

### 2.3 스크립트 폴더 구조 권장사항

- **기능별 분류**: 관련된 스크립트는 같은 폴더에 배치
- **계층 구조**: 큰 시스템은 하위 폴더로 세분화

```
Scripts/
├── Characters/
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   └── PlayerData.cs
│   └── Enemy/
│       ├── EnemyController.cs
│       └── EnemyAI.cs
├── UI/
│   ├── HealthBar.cs
│   └── MenuManager.cs
└── Systems/
    ├── InventorySystem.cs
    └── QuestSystem.cs
```

---

## 3. 주석 및 문서화

### 3.1 주석 스타일

- **언어 선택**: 한글 또는 영문 사용 가능하나, **한 파일 내에서는 일관성 유지**
- **요약 주석**: 복잡한 메서드나 외부 시스템과 연결된 코드에는 주석 추가 권장

```csharp
// ❌ BAD
void DoSomething() {
    // 뭔가 함
    int x = 10; // 변수
}

// ✅ GOOD
/// <summary>
/// 플레이어의 체력을 회복시킵니다.
/// </summary>
/// <param name="amount">회복할 체력량</param>
void RestoreHealth(int amount) {
    currentHealth += amount;
    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
}
```

### 3.2 TODO 및 FIXME

- **형식**: `// TODO: 설명` 또는 `// FIXME: 설명`
- **담당자/날짜**: 가능하면 담당자나 날짜를 함께 기록

```csharp
// ✅ GOOD
// TODO: 플레이어 이동 속도 밸런스 조정 필요 (2026-01-25)
// FIXME: 메모리 누수 가능성 확인 필요
// TODO(김개발): 인벤토리 UI 개선 예정
```

### 3.3 주석 작성 가이드라인

- **명확성**: 코드만 봐도 알 수 있는 내용은 주석 생략
- **이유 설명**: "무엇을" 하는지보다 "왜" 그렇게 하는지 설명
- **복잡한 로직**: 알고리즘이나 비즈니스 로직이 복잡할 때는 주석 추가

```csharp
// ❌ BAD
// 플레이어 이동
transform.position += moveDirection * speed * Time.deltaTime;

// ✅ GOOD
// 프레임 독립적인 이동을 위해 Time.deltaTime 사용
transform.position += moveDirection * speed * Time.deltaTime;
```

---

## 4. Unity 특화 규칙

### 4.1 MonoBehaviour 사용

- **1 파일 1 클래스**: 하나의 파일에는 하나의 클래스만 정의
- **책임 분리**: MonoBehaviour는 가능하면 단일 책임만 가지도록 설계

```csharp
// ❌ BAD
// PlayerController.cs
public class PlayerController : MonoBehaviour {
    // 이동, 공격, 인벤토리, UI 모두 처리
}

// ✅ GOOD
// PlayerController.cs
public class PlayerController : MonoBehaviour {
    // 이동만 처리
}

// PlayerCombat.cs
public class PlayerCombat : MonoBehaviour {
    // 공격만 처리
}
```

### 4.2 Unity 콜백 메서드

- **최소한의 로직**: `Awake`, `Start`, `Update` 등에는 최소한의 로직만 배치
- **초기화 분리**: 복잡한 초기화는 별도 메서드로 분리

```csharp
// ❌ BAD
void Start() {
    // 100줄의 초기화 코드
    LoadAllData();
    SetupUI();
    InitializeSystems();
    // ...
}

// ✅ GOOD
void Start() {
    InitializePlayer();
}

void InitializePlayer() {
    LoadPlayerData();
    SetupPlayerUI();
    InitializePlayerSystems();
}
```

### 4.3 Update 메서드 최적화

- **필요한 경우만 사용**: 매 프레임 호출이 꼭 필요한 로직만 `Update`에 배치
- **대안 활용**: 이벤트, 코루틴, Invoke 등을 활용하여 Update 사용 최소화

```csharp
// ❌ BAD
void Update() {
    // 매 프레임 체력 체크 (불필요)
    if (currentHealth <= 0) {
        Die();
    }
}

// ✅ GOOD
// 체력이 변경될 때만 체크
void OnHealthChanged(int newHealth) {
    if (newHealth <= 0) {
        Die();
    }
}

// 또는 코루틴 사용
IEnumerator CheckHealthPeriodically() {
    while (true) {
        if (currentHealth <= 0) {
            Die();
        }
        yield return new WaitForSeconds(0.5f);
    }
}
```

### 4.4 ScriptableObject 활용

- **데이터 분리**: 게임 밸런스, 설정값, 스탯 등은 하드코딩 대신 ScriptableObject 사용
- **재사용성**: 여러 오브젝트에서 공유되는 데이터는 ScriptableObject로 관리

```csharp
// ❌ BAD
public class Enemy : MonoBehaviour {
    public float health = 100f;  // 하드코딩
    public float damage = 10f;
    public float speed = 5f;
}

// ✅ GOOD
// EnemyData.cs (ScriptableObject)
[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/EnemyData")]
public class EnemyData : ScriptableObject {
    public float health;
    public float damage;
    public float speed;
}

// Enemy.cs
public class Enemy : MonoBehaviour {
    public EnemyData data;  // ScriptableObject 참조
}
```

### 4.5 GetComponent 최적화

- **캐싱**: `GetComponent`는 가능하면 `Awake`나 `Start`에서 한 번만 호출하여 캐싱

```csharp
// ❌ BAD
void Update() {
    Rigidbody rb = GetComponent<Rigidbody>();  // 매 프레임 호출
    rb.AddForce(Vector3.up);
}

// ✅ GOOD
Rigidbody rb;

void Awake() {
    rb = GetComponent<Rigidbody>();  // 한 번만 호출
}

void Update() {
    rb.AddForce(Vector3.up);
}
```

---

## 5. 코드 예시

### 5.1 완전한 예시: PlayerController

```csharp
// ✅ GOOD
using UnityEngine;

public class PlayerController : MonoBehaviour {
    // 멤버 변수: CamelCase
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    private Rigidbody rb;
    private bool isGrounded;
    private bool canJump;
    
    // Unity 콜백
    void Awake() {
        rb = GetComponent<Rigidbody>();
    }
    
    void Start() {
        InitializePlayer();
    }
    
    void Update() {
        HandleInput();
        CheckGrounded();
    }
    
    // 메서드: PascalCase, 동사로 시작
    void InitializePlayer() {
        // 초기화 로직
    }
    
    void HandleInput() {
        float horizontal = Input.GetAxis("Horizontal");
        Move(horizontal);
        
        if (Input.GetButtonDown("Jump") && canJump) {
            Jump();
        }
    }
    
    void Move(float direction) {
        Vector3 moveDirection = new Vector3(direction, 0f, 0f);
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
    
    void Jump() {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    
    // Boolean 반환 메서드
    bool CheckGrounded() {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.1f);
        canJump = isGrounded;
        return isGrounded;
    }
}
```

---

## 6. 규칙 유지 관리

이 규칙 문서는 프로젝트 진행에 따라 변경될 수 있습니다. 규칙을 추가하거나 수정할 때는:

1. **팀 합의**: 변경 사항을 팀원들과 논의
2. **문서 업데이트**: `GameRule.md` 파일을 즉시 업데이트
3. **기존 코드 리팩토링**: 가능하면 기존 코드도 새 규칙에 맞게 수정

규칙에 대한 질문이나 제안이 있으면 팀 리더나 프로젝트 관리자에게 문의하세요.

---

**마지막 업데이트**: 2026-01-25
