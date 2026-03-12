# Soul Ark - 오딘 인스펙터 강화 작업 완료 보고서

**작업 일자:** 2026-03-12  
**작업자:** 김민석 / msking1125@gmail.com  
**브랜치/태그:** N/A (로컬 작업)

---

## 완료된 작업

### 1. 데이터 클래스 Odin 인스펙터 강화

| 파일 | 변경 내용 |
|------|----------|
| `CombatStats.cs` | `BoxGroup`, `ProgressBar`, `InlineProperty`, 네임스페이스 `ProjectFirst.Data` |
| `AgentRow.cs` | `HorizontalGroup`, `BoxGroup`, `EnumToggleButtons`, `PreviewField`, `InlineProperty`, CombatStats 직접 사용 |
| `AgentTable.cs` | `Title`, `TableList`, `Searchable`, `Button`, `GUIColor`, 네임스페이스 |
| `SkillRow.cs` | `HorizontalGroup`, `BoxGroup`, `ShowIf`, `EnumToggleButtons`, `PreviewField`, `AssetsOnly`, `OnValueChanged`, `MultiLineProperty`, `PropertyRange`, `SuffixLabel` |
| `SkillTable.cs` | `Title`, `TableList`, `Searchable`, `Button`, 네임스페이스 |
| `MonsterRow.cs` | `HorizontalGroup`, `BoxGroup`, `EnumToggleButtons`, `PreviewField`, `AssetsOnly`, `SuffixLabel`, CombatStats 직접 사용, 네임스페이스 |

### 2. 생성된 신규 파일

| 파일 | 설명 |
|------|------|
| `SoulArkDataManagerWindow.cs` | Odin Editor Window 기반 데이터 테이블 관리 툴. 탭 UI, 인덱스 재구축, 변경사항 저장, 밸런스 분석 기능 포함 |
| `ODIN_INSPECTOR_GUIDE.md` | AI 학습용 오딘 인스펙터 가이드. 11개 섹션으로 구성된 패턴 문서 |

### 3. 적용된 핵심 Odin 패턴

#### 그룹화
- `BoxGroup("그룹명")` - 박스로 섹션 구분
- `HorizontalGroup("그룹", 0.5f)` - 수평 배치 (비율 조정)
- 중첩 그룹: `[HorizontalGroup("A")] + [BoxGroup("A/Left")]`

#### 시각화
- `TableList(ShowIndexLabels = true, AlwaysExpanded = true)` - 테이블 형태 리스트
- `PreviewField(60, ObjectFieldAlignment.Left)` - 아이콘/프리팹 미리보기
- `ProgressBar(min, max, ColorGetter = "MethodName")` - 스탯 시각화
- `EnumToggleButtons` - 속성/타입 토글 UI

#### 조건부 표시
- `ShowIf("IsSingleTarget")` - 스킬 타입별 설정 노출
- `OnValueChanged("MethodName")` - 값 변경 시 콜백
- `EnableIf("@CanExecute")` - 인라인 조건

#### 메타데이터
- `LabelText("표시명")` - 한글 레이블
- `SuffixLabel("단위", true)` - 단위 표시 (sec, m, x, %)
- `AssetsOnly` - Assets 폴더 리소스 제한
- `Searchable` - 인스펙터 내 검색

### 4. 컨디셔널 컴파일 패턴

모든 Odin 코드는 `#if ODIN_INSPECTOR`로 감싸져 있어 Odin 미설치 환경에서도 컴파일 가능:

```csharp
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

#if ODIN_INSPECTOR
[BoxGroup("스탯")]
[ProgressBar(0, 1000)]
#endif
public float hp;
```

### 5. Editor Window 기능

**Soul Ark/Data Manager** 메뉴에서 실행:
- 탭 기반 UI (에이전트/몬스터/스킬/스테이지/밸런스)
- `InlineEditor`로 테이블 직접 편집
- `EnumToggleButtons`로 탭 전환
- 버튼: 모든 테이블 로드 / 인덱스 재구축 / 변경사항 저장
- 밸런스 탭: 평균 스탯 분석 (ProgressBar 표시)

---

## 검증 필요사항

1. **컴파일 검증** - Unity에서 스크립트 오류 여부 확인
2. **Odin 인스펙터 UI** - 데이터 에셋 선택 시 Odin UI 정상 표시 확인
3. **Editor Window** - Soul Ark/Data Manager 메뉴 실행 및 기능 확인
4. **기존 데이터 마이그레이션** - `MonsterRow`의 필드 변경(`hp`→`stats.hp`)으로 인해 기존 에셋 업데이트 필요

---

## 변경된 데이터 구조 (Breaking Change)

### MonsterRow
```csharp
// 변경 전
public float hp;
public float atk;
public float def;
public float critChance;
public float critMultiplier;

// 변경 후
public CombatStats stats;  // 위 필드들이 stats 내부로 이동
```

### AgentRow
```csharp
// 변경 전
public float hp;
public float atk;
...

// 변경 후
public CombatStats stats;
```

**주의:** 기존 `.asset` 파일은 수동으로 마이그레이션하거나, Unity의 `FormerlySerializedAs`로 자동 마이그레이션 구현 필요.

---

## Git 커밋 제안

```
Feat: 오딘 인스펙터 데이터 클래스 강화 및 에디터 툴 추가

- CombatStats, AgentRow/Table, SkillRow/Table, MonsterRow에 Odin 속성 적용
- BoxGroup, HorizontalGroup, TableList, ShowIf, EnumToggleButtons 등 적용
- SoulArkDataManagerWindow.cs 추가 (Odin Editor Window)
- ODIN_INSPECTOR_GUIDE.md 추가 (AI 학습용 패턴 문서)
- ProjectFirst.Data 네임스페이스 적용
- 컨디셔널 컴파일 (#if ODIN_INSPECTOR)로 Odin 의존성 분리
```

---

## 다음 작업 후보

1. **나머지 테이블 Odin 적용** - Stage, Chapter, Wave, Dialogue 등
2. **PlayerData Odin 적용** - 통화/재화, 진행도, 설정값 시각화
3. **AI 학습 문서 확장** - 실제 사용 예시 추가, 패턴 템플릿
4. **Odin Editor Window 확장** - CSV 임포트 통합, 밸런스 시뮬레이션
5. **DOTween 제거** - Plugins/Demigiant 폴더 제거 (프로젝트 규칙상 미사용)
