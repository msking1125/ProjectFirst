# BattleReadyScene 생성 가이드

Unity 에디터에서 아래 단계를 따라 BattleReadyScene을 생성하세요.

## 1. 씬 생성

1. **Project 창**에서 `Assets/Project/Scenes/` 폴더로 이동
2. 마우스 오른쪽 클릭 → `Create` → `Scene`
3. 씬 이름을 `BattleReadyScene`으로 지정

## 2. 기본 오브젝트 설정

### 2.1 BattleReadyManager 설정
1. 빈 GameObject 생성 → 이름을 `BattleReadyManager`로 변경
2. `BattleReadyManager` 컴포넌트 추가:
   - `Add Component` 버튼 클릭
   - `Scripts/InGame/BattleReadyManager` 검색 및 추가

### 2.2 UI Document 설정
1. 빈 GameObject 생성 → 이름을 `UI`로 변경
2. `UI` GameObject에 `UIDocument` 컴포넌트 추가
3. `UIDocument`의 `Panel Settings` 설정:
   - 새 Panel Settings Asset 생성 (`Create` → `UI Toolkit` → `Panel Settings`)
   - 기본 설정 유지 (Scale Mode: Constant Pixel Size, Reference Resolution: 1920x1080)

### 2.3 UI 소스 설정
1. `UIDocument` 컴포넌트의 `Source Asset` 필드에 `BattleReadyView.uxml` 할당
   - `Assets/Project/UI/BattleReady/BattleReadyView.uxml` 파일 드래그

## 3. 데이터 참조 설정

### 3.1 BattleReadyManager Inspector 설정
`BattleReadyManager` 컴포넌트의 Inspector에서 아래 항목들을 설정:

#### Required References:
- **UI Document**: `UI` GameObject (UIDocument 컴포넌트가 있는)
- **Player Data**: `Assets/Project/Data/PlayerData.asset` (또는 해당 ScriptableObject)
- **Agent Table**: `Assets/Project/Data/AgentTable.asset` (또는 해당 ScriptableObject)
- **Stage Data**: `Assets/Project/Data/StageData.asset` (또는 해당 ScriptableObject)
- **Run Session**: `Assets/Project/Systems/RunSession.asset` (또는 해당 ScriptableObject)

#### Enhanced Systems (선택사항):
- **UI Animator**: `BattleReadyManager` GameObject에 `BattleReadyUIAnimator` 컴포넌트 추가 후 연결
- **Tooltip Manager**: `BattleReadyManager` GameObject에 `CharacterTooltipManager` 컴포넌트 추가 후 연결
- **Validation System**: `BattleReadyManager` GameObject에 `PartyValidationSystem` 컴포넌트 추가 후 연결

## 4. 추가 컴포넌트 설정

### 4.1 UI Animator (선택사항)
1. `BattleReadyManager` GameObject 선택
2. `Add Component` → `BattleReadyUIAnimator`
3. 필요시 Audio Source 추가 및 사운드 클립 할당

### 4.2 Tooltip Manager (선택사항)
1. `BattleReadyManager` GameObject 선택
2. `Add Component` → `CharacterTooltipManager`
3. Agent Table과 Player Data 참조 설정 (Inspector에서 자동 설정됨)

### 4.3 Validation System (선택사항)
1. `BattleReadyManager` GameObject 선택
2. `Add Component` → `PartyValidationSystem`
3. Agent Table, Stage Data, Player Data 참조 설정 (Inspector에서 자동 설정됨)

## 5. 씬 저장 및 Build Settings 추가

### 5.1 씬 저장
1. `Ctrl+S` (또는 `File` → `Save As`)로 씬 저장
2. 경로 확인: `Assets/Project/Scenes/BattleReadyScene.unity`

### 5.2 Build Settings에 추가
1. `File` → `Build Settings`
2. `Add Open Scenes` 버튼 클릭
3. 또는 `Add Open Scenes` 버튼 옆의 `+` 버튼으로 `BattleReadyScene` 직접 추가

## 6. 테스트

### 6.1 씬 테스트
1. Unity 에디터에서 `BattleReadyScene` 열기
2. Play 버튼으로 실행
3. UI가 정상적으로 표시되는지 확인
4. 콘솔에 에러가 없는지 확인

### 6.2 맵 챕터에서 연동 테스트
1. `MapChapterScene` 열기
2. 스테이지 선택 후 "전투 준비" 버튼 클릭
3. `BattleReadyScene`으로 정상적으로 전환되는지 확인

## 7. 문제 해결

### 일반적인 문제들:

**UI가 표시되지 않을 경우:**
- UIDocument의 Source Asset이 BattleReadyView.uxml로 설정되었는지 확인
- Panel Settings가 제대로 설정되었는지 확인
- Canvas의 Render Mode 설정 확인

**데이터 참조 오류:**
- Inspector에서 모든 ScriptableObject 참조가 할당되었는지 확인
- 해당 Asset 파일이 존재하는지 확인

**씬 전환 오류:**
- Build Settings에 BattleReadyScene이 추가되었는지 확인
- AsyncSceneLoader 인스턴스가 있는지 확인

**스타일이 적용되지 않을 경우:**
- BattleReadyStyles.uss 파일이 있는지 확인
- UXML 파일에 Style path가 올바르게 설정되었는지 확인

## 8. 최종 확인 리스트

- [ ] BattleReadyScene.unity 파일 생성됨
- [ ] BattleReadyManager GameObject 및 컴포넌트 추가됨
- [ ] UIDocument 설정 및 BattleReadyView.uxml 연동됨
- [ ] 모든 데이터 참조(PlayerData, AgentTable, StageData 등) 할당됨
- [ ] 선택적 시스템(UI Animator, Tooltip Manager, Validation) 추가됨
- [ ] Build Settings에 씬 추가됨
- [ ] 씬 테스트 완료 및 정상 작동 확인됨

이 가이드를 따라 모든 단계를 완료하면 완전한 배틀 준비 화면이 구현됩니다.
