# MindArk 프로젝트 컨텍스트 (필수 읽기)

나는 Unity 모바일 RPG "MindArk"를 개발 중입니다. 아래 규칙을 모든 코드 생성에 적용하세요.

## 필수 코딩 규칙
- 네임스페이스: `ProjectFirst.{폴더명}` (예: `ProjectFirst.Core`, `ProjectFirst.UI`)
- private 변수: `_camelCase` (언더스코어 접두사)
- public 변수 직접 노출 금지: `[SerializeField] private` + 프로퍼티 사용
- DOTween 금지: 순수 Coroutine + CanvasGroup.alpha 보간
- 씬 전환: `AsyncSceneLoader.LoadScene()` 만 사용
- 이벤트 통신: `VoidEventChannelSO` (ScriptableObject 이벤트 채널)
- UI: UI Toolkit (UXML/USS) 기반
- 한 파일 최대 400줄 (초과 시 분리)
- 모든 public 메서드에 `<summary>` XML 주석 필수

## 현재 완료된 핵심 파일 (참조 가능)
- `BattleGameManager.cs` (`Assets/Project/Scripts/Core/`) — 전투 흐름 총괄
- `Agent.cs` (`Assets/Project/Scripts/Agent/`) — 플레이어 캐릭터 전투 AI
- `Enemy.cs` / `EnemyPool.cs` (`Assets/Project/Scripts/Enemy/`) — 적 + 오브젝트 풀링
- `SkillSystem.cs` (`Assets/Project/Scripts/Systems/`) — 스킬 4종 이펙트
- `WaveManager.cs` (`Assets/Project/Scripts/Systems/`) — CSV 기반 웨이브
- `TitleManager.cs` (`Assets/Project/Scripts/Bootstrap/`) — 타이틀 씬
- `AsyncSceneLoader.cs` (`Assets/Project/Scripts/Bootstrap/`) — 씬 비동기 전환
- `VoidEventChannelSO.cs` — 이벤트 채널 SO

## 게임 핵심 스펙
- 속성 상성: `Passion > Intuition > Reason > Passion` (유리 1.5배 / 불리 0.7배)
- 캐릭터 9종 (3속성 × 3사거리)
- 재화: 스태미나 / 골드 / 젬
- 파티 최대 3명 편성
- 전투: 좌→우 이동 몬스터, 우측 기지 HP, 자동 공격 + 스킬 버튼

## Git 커밋 규칙
- 커밋자: `김민석 / msking1125@gmail.com` (반드시 이 계정으로 커밋)
- 포맷: `{유형}: {한글 제목}` (제목 50자 이내, 마침표 금지)
- 유형: `Feat` / `Fix` / `Docs` / `Style` / `Refactor` / `Test` / `Chore` / `Design` / `Rename` / `Remove` / `!HOTFIX`
- 본문: "무엇을 & 왜" 위주로 `-` 글머리표 사용
- 한 커밋에 한 가지 문제만, Phase 완료 시 태그 생성 (예: `v0.4.0-phase2`)
- `.meta` 파일 누락 금지

## 폴더 구조 기준
새 파일은 아래 구조에 맞춰 배치합니다.

```text
Assets/Project/
├── Scripts/
│   ├── Agent/
│   ├── Bootstrap/
│   ├── Core/
│   ├── Data/
│   ├── Editor/
│   ├── Enemy/
│   ├── InGame/
│   ├── OutGame/
│   │   └── UI/
│   ├── Systems/
│   └── UI/
├── Data/
│   ├── CSV/
│   └── Tables/
├── Resources/
├── Scenes/
└── UI/
```
