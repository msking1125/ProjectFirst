# MapChapter 씬 설정 가이드

## 1단계: 기본 Unity 씬 구조 설정

### 1.1 씬 기본 오브젝트 생성
```
📁 MapChapter.unity
├── 🎥 Main Camera
├── 🎨 Canvas
├── 📱 UIDocument
├── 🎮 MapChapterManager
└── 🔄 AsyncSceneLoader
```

### 1.2 Main Camera 설정
- **Position**: (0, 0, -10)
- **Rotation**: (0, 0, 0)
- **Projection**: Orthographic
- **Size**: 5
- **Background**: 검은색 (#1a1a1a)

### 1.3 Canvas 설정
- **Render Mode**: Screen Space - Overlay
- **Pixel Perfect**: 체크
- **Dynamic Pixels Per Unit**: 1

### 1.4 UIDocument 설정
- **Panel Settings**: 새로 생성
- **Source Asset**: MapView.uxml 연결
- **Style Sheets**: MapView.uss 연결

## 2단계: MapChapterManager 컴포넌트 설정

### 2.1 Inspector 연결 필드
```csharp
// 데이터 (Data)
Chapter Table: [ChapterTable ScriptableObject]
Stage Table: [StageTable ScriptableObject]  
Player Data: [PlayerData ScriptableObject]
Chapter Data: [ChapterData ScriptableObject]

// UI (UI)
UI Document: [UIDocument 컴포넌트]

// 이벤트 (Events)
On Stage Selected: [VoidEventChannelSO ScriptableObject - 선택적]
```

### 2.2 ScriptableObject 생성 필요
아래 ScriptableObject 에셋들을 Assets/Project/Data/ 폴더에 생성:
- `ChapterTable.asset` (챕터 데이터)
- `StageTable.asset` (스테이지 데이터)
- `PlayerData.asset` (플레이어 데이터)
- `ChapterData.asset` (시각 데이터)

## 3단계: AsyncSceneLoader 설정

### 3.1 AsyncSceneLoader 추가
- MapChapter 씬에 AsyncSceneLoader 컴포넌트 추가
- **Dont Destroy On Load**: 체크

### 3.2 Bootstrap 씬 연동
- Bootstrap 씬에도 AsyncSceneLoader가 있는지 확인
- 전역 싱글톤으로 동작하도록 설정

## 4단계: UI Toolkit 설정

### 4.1 UXML 연결
- UIDocument의 Source Asset에 `MapView.uxml` 연결
- Panel Settings에서 Resolution 참조 설정

### 4.2 USS 스타일 연결
- UIDocument의 Style Sheets에 `MapView.uss` 추가
- 스타일이 제대로 적용되는지 확인

## 5단계: 테스트 데이터 설정

### 5.1 ChapterTable 데이터 예시
```csharp
// Chapter 1
ID: 1
Name: "초원의 챕터"
Description: "첫 번째 모험을 시작하는 곳"
World Map X: 200
World Map Y: 150
Is Unlocked: true

// Chapter 2  
ID: 2
Name: "숲의 챕터"
Description: "신비로운 숲으로의 여정"
World Map X: 400
World Map Y: 200
Is Unlocked: false
```

### 5.2 StageTable 데이터 예시
```csharp
// Stage 1-1
ID: 101
Chapter ID: 1
Stage Number: 1
Name: "초원의 첫 만남"
Description: "쉬운 튜토리얼 스테이지"
Recommended Power: 100
Enemy Element: Reason
Stamina Cost: 5
Reward Gold: 100
Reward Exp: 50

// Stage 1-2
ID: 102
Chapter ID: 1
Stage Number: 2
Name: "초원의 도전"
Description: "조금 더 강한 적들"
Recommended Power: 150
Enemy Element: Fire
Stamina Cost: 6
Reward Gold: 150
Reward Exp: 75
```

### 5.3 ChapterData 시각 정보
```csharp
// Chapter 1
Chapter ID: 1
Chapter Name: "초원의 챕터"
Chapter Icon: [챕터 아이콘 스프라이트]
World Map Icon: [월드맵 아이콘 스프라이트]
World Map Position: (200, 150)
Clear Stars: 0
Is Unlocked: true
Stage IDs: [101, 102, 103]
```

## 6단계: 씬 전환 테스트

### 6.1 Build Settings 설정
1. File → Build Settings
2. Scenes In Build에 아래 씬들 추가:
   - Bootstrap.unity (인덱스 0)
   - Logo.unity
   - Title.unity
   - Login.unity
   - Lobby.unity
   - MapChapter.unity
   - BattleReady.unity

### 6.2 테스트 순서
1. Bootstrap 씬 시작
2. Logo → Title → Login → Lobby 순서로 진행
3. Lobby에서 "게임 시작" 버튼 클릭
4. MapChapter 씬으로 정상 전환 확인

## 7단계: 디버깅 및 확인사항

### 7.1 Console 로그 확인
- `[MapChapterManager]` 로그 확인
- 데이터 로딩 오류 확인
- UI 바인딩 오류 확인

### 7.2 Inspector 연결 상태 확인
- 모든 필드에 에셋이 연결되었는지 확인
- null 참조 오류가 없는지 확인

### 7.3 기능 테스트
- 월드맵 드래그 동작 확인
- 챕터 노드 클릭 동작 확인
- 스테이지 선택 및 정보 표시 확인
- 전투 준비 버튼 동작 확인

## 8단계: 최종 점검

### 8.1 필수 요구사항
- ✅ MapChapterManager 컴포넌트 연결
- ✅ UIDocument에 MapView.uxml 연결
- ✅ MapView.uss 스타일 적용
- ✅ 모든 데이터 테이블 연결
- ✅ AsyncSceneLoader 동작 확인

### 8.2 선택적 요구사항
- 🔄 OnStageSelected 이벤트 연결
- 🎨 배경 이미지 에셋 추가
- ⭐ 별점 시각 에셋 추가
- 🎭 캐릭터 SD 에셋 추가

## 문제 해결

### 자주 발생하는 문제
1. **UI가 보이지 않음**: UIDocument의 Panel Settings 확인
2. **스타일 적용 안됨**: USS 파일 연결 확인
3. **데이터 로딩 실패**: ScriptableObject 연결 확인
4. **씬 전환 실패**: AsyncSceneLoader 확인
5. **클릭 동작 안됨**: 이벤트 콜백 연결 확인

### 디버깅 팁
- Console 창에서 `[MapChapterManager]` 로그 필터링
- Inspector에서 null 참조 확인
- Unity의 UI Debugger로 UI 계층 구조 확인
