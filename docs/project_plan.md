# Rolling Kimbap 프로젝트 작업 내역

## 완료된 작업

### **작업 이름:** Character.cs 충돌 로직 수정
- **설명:** Character.cs 파일에서 bodyCollider.enabled를 직접 제어하는 로직을 레이어 변경 로직으로 수정하여 충돌 문제를 해결했습니다.
- **완료 시간:** 2025년 7월 2일

### **작업 이름:** PlayerSettingsController.cs 수정
- **설명:** `PlayerSettingsController.cs`의 `LoadSettings` 함수에서 불필요한 `SaveSettings()` 호출을 제거했습니다.
- **완료 시간:** 2025년 7월 2일

### **작업 이름:** 설정 저장 및 로드 기능 테스트
- **설명:** BGM/SFX 음량, UI 반전 등의 설정을 변경한 후 앱을 재시작해도 해당 설정이 유지되는지 테스트했습니다.
- **완료 시간:** 2025년 7월 2일

### **작업 이름:** PlayerPrefs 설정 저장/로드 버그 수정
- **설명:** BGM, SFX 음량, UI 반전 등의 설정이 PlayerPrefs로 저장 및 로드될 때, 앱 재시작 시 설정이 유지되지 않는 버그가 있었습니다.
- **원인:** Managers에서 각 매니저의 `IBaseManager.Initialize()`를 실행할 때 `SaveManager`의 `Initialize()`는 `BaseManager.Initialize()`만 구현되어 있었고, `IBaseManager.Initialize(params object[] args)`가 아니어서 실행되지 않고 있었습니다. 이 때문에 `PlayerSettings.LoadSettings()`를 실행하는 부분이 없어 저장한 내용을 가져올 수 없었습니다.
- **설명:** SaveManager.Initialize()를 SaveManager.Initialize(param object[] args)로 변경하여 호출이 됨을 확인하고, 제대로 설정 값이 로드 되는 지 테스트했습니다.

### **작업 이름:** OrderUI 애니메이션 구현 (DOTween)
- **설명:** OrderUI 등장 및 기존 OrderUI 이동 애니메이션을 DOTween을 사용하여 구현했습니다.
- **완료된 세부 태스크:**
    - **OrderUI 스크립트 DOTween 참조 추가:** OrderUI를 관리하는 스크립트에 DOTween 참조를 추가했습니다.
    - **기존 OrderUI 하단 이동 애니메이션 구현 (LayoutGroup 연동):** `GenerateOrderUI()` 호출 시 `_verticalLayoutGroup`에 의해 다른 OrderUI들이 아래로 이동하는 것을 DOTween을 사용하여 부드럽게 애니메이션 처리했습니다.
    - **신규 OrderUI 등장 애니메이션 구현:** `isReversed` 값에 따라 신규 OrderUI가 좌측 또는 우측에서 화면 중앙으로 등장하는 애니메이션을 구현했습니다.
    - **신규 OrderUI 등장 애니메이션 호출 및 통합:** `OrdersUI.cs`의 `GenerateOrderUI()` 함수에서 기존 OrderUI들의 하단 이동 애니메이션이 완료된 후, 신규 OrderUI의 `PlayEnterAnimation()` 메서드를 호출하여 등장 애니메이션을 실행하도록 통합했습니다.
    - **애니메이션 완료 콜백 처리:** 기존 OrderUI 이동 및 신규 OrderUI 등장 애니메이션이 완료된 후 필요한 추가 로직을 처리할 수 있도록 콜백 함수를 연결했습니다.
    - **애니메이션 통합 및 테스트:** 기존 OrderUI 이동 애니메이션과 신규 OrderUI 등장 애니메이션을 통합하고, 다양한 시나리오에서 정상적으로 동작하는지 테스트했습니다.
    - **OrderUI _ingredientParent pivot 애니메이션 조정:** OrderUI의 _ingredientParent의 pivot을 조절하여 애니메이션이 자연스럽게 보이도록 수정했습니다.
    - **OrdersUI 기존 UI 하단 이동 애니메이션 제거:** OrdersUI 레이아웃 상 새로운 UI가 맨 아래에 등장하므로, 기존 OrderUI들의 하단 이동 애니메이션 로직을 제거했습니다.
    - **OrderUI 등장 애니메이션 Tween 관리 및 정지:** IsReverse 값이 중간에 변경될 경우, 활성화된 등장 애니메이션을 모두 정지하도록 activeEnterTween에 저장하고 관리했습니다.

### **작업 이름:** 오더(Order) 남은 시간 시각화 기능 구현
- **설명:** 게임 내 오더의 남은 시간을 시각적으로 표시하는 기능을 구현했습니다.
- **완료된 세부 태스크:**
    - **UI 요소 추가 및 연결:** `OrderUI` 프리팹에 `LifeTimeImage` UI 요소를 추가하고, `OrderUI.cs` 스크립트에 `_lifeTimeImage` 필드를 연결했습니다.
    - **오더 생명주기 데이터 관리:** `OrderUI.cs`에 `_startPosX` 필드를 추가하여 오더의 시작 X 위치를 기록하고, `SetStartPosition` 메서드로 설정할 수 있도록 했습니다. `UpdateOrderLifeTime` 메서드를 구현하여 현재 플레이어의 이동 거리와 오더의 시작/종료 X 위치를 기반으로 `LifeTimeImage`의 `fillAmount`를 계산하고 업데이트하도록 했습니다.
    - **전체 오더 UI 업데이트 로직 구현:** `OrdersUI.cs`에 `_currentPosX` 필드를 추가하여 현재 플레이어의 X 좌표를 추적하고, `SetCurrentPosX` 메서드를 통해 모든 `OrderUI` 인스턴스의 `UpdateOrderLifeTime` 메서드를 호출하여 오더 생명주기 UI를 일괄 업데이트하도록 했습니다.
    - **게임 매니저와의 연동:** `InGameUI.cs`에서 `OrdersUI` 프로퍼티를 public으로 노출하여 `InGameUIManager`에서 접근할 수 있도록 했습니다. `InGameUIManager.cs`에서 `StatusManager`를 주입받아 플레이어의 이동 거리를 가져오고, `Tick()` 메서드에서 매 프레임마다 `InGameUI.OrdersUI.SetCurrentPosX()`를 호출하여 모든 오더의 생명주기 UI가 실시간으로 업데이트되도록 했습니다.

### **작업 이름:** 지형지물 이미지 변화 (리소스 추가)
- **설명:** `travelDistance`에 따라 게임 내 지형지물 이미지를 변화시키기 위한 리소스를 추가했습니다.
- **완료된 세부 태스크:**
    - 나무 2종 추가: 단풍 나무, 눈 덮인 나무
    - 지형 2종 추가: 툰드라 지형, 설원 지형
- **완료 시간:** 2025년 7월 2일

## 진행 중인 작업

(없음)

## 향후 계획

### **1. 서빙 성공/실패 시 애니메이션 이펙트 추가**
- **서빙 성공 시:**
    - 돈 획득 시 획득한 돈(메뉴 가격 및 팁) 텍스트 효과 출력
    - 만들어진 김밥 이미지를 해당하는 오더 UI로 이동시키는 효과
    - 서빙 성공한 오더 UI에서 돈 UI로 작은 금화 이미지 여러 개가 이동해 들어가는 효과
    - 돈이 늘어나면서 돈 텍스트가 커졌다가 작아지는 효과
- **서빙 실패 시:**
    - 조합된 김밥 이미지를 아래로 떨어뜨리는 효과

### **2. 사운드 시스템 개선**
- **효과음 추가:** 점프, 서빙, 장애물 충돌, 죽음, 서빙 성공, 서빙 실패 시 효과음 추가
- **배경음 추가:** 게임 플레이 중 배경 음악 추가
- **음향 설정 적용:** 게임 내 음향 설정(BGM/SFX 볼륨 조절 등) 기능 구현 및 적용

### **3. 카메라 연출 및 결과 페이지 구현**
- **서빙 성공 시 카메라 연출:** 서빙 성공 시 손님을 잠시 비추는 카메라 연출 추가
- **결과 페이지 구현:** 게임 종료 후 결과(점수, 획득 금액 등)를 표시하는 페이지 구현
