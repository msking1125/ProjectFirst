# 오딘 인스펙터(odin inspector) 사용 규약 for Soul Ark

## 목적
이 문서는 Soul Ark 프로젝트의 Unity 개발에서 Odin Inspector를 최대한 활용하기 위한 표준 패턴과 규칙을 정의합니다.
AI 코드 생성 시 이 규칙을 따라야 합니다.

---

## 1. 기본 구조

### 1.1 컨디셔널 컴파일 (필수)
모든 Odin 관련 코드는 `#if ODIN_INSPECTOR` 전처리기로 감싸야 합니다.
이는 Odin Inspector가 없는 환경에서도 코드가 동작하도록 보장합니다.

```csharp
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

public class ExampleClass : MonoBehaviour
{
#if ODIN_INSPECTOR
    [BoxGroup("Group Name")]
    [LabelText("Display Name")]
#endif
    public int field;
}
```

### 1.2 네임스페이스
모든 데이터 클래스는 `ProjectFirst.Data` 네임스페이스를 사용합니다.
에디터 툴은 `ProjectFirst.Editor` 네임스페이스를 사용합니다.

---

## 2. 데이터 테이블 패턴

### 2.1 Table SO (ScriptableObject)
```csharp
#if ODIN_INSPECTOR
[CreateAssetMenu(menuName = "Soul Ark/Table Name")]
#else
[CreateAssetMenu(menuName = "Game/Table Name")]
#endif
public class XxxTable : ScriptableObject
{
#if ODIN_INSPECTOR
    [Title("데이터 목록", TitleAlignment = TitleAlignments.Centered)]
    [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
    [Searchable]
#endif
    public List<XxxRow> rows = new();

#if ODIN_INSPECTOR
    [Button("인덱스 재구축", ButtonSizes.Medium)]
    [GUIColor(0.3f, 0.8f, 0.3f)]
#endif
    private void RebuildIndex() { }
}
```

### 2.2 Row 클래스
```csharp
[Serializable]
#if ODIN_INSPECTOR
[HideLabel]
#endif
public class XxxRow
{
#if ODIN_INSPECTOR
    [HorizontalGroup("Group", 0.5f)]
    [BoxGroup("Group/ID")]
    [LabelText("ID")]
#endif
    public int id;

#if ODIN_INSPECTOR
    [HorizontalGroup("Group", 0.5f)]
    [BoxGroup("Group/Name")]
    [LabelText("이름")]
#endif
    public string name;
}
```

---

## 3. 핵심 Odin 어트리뷰트

### 3.1 그룹화 (Layout)

| 어트리뷰트 | 설명 | 사용 예시 |
|-----------|------|----------|
| `[BoxGroup("Name")]` | 박스로 그룹화 | `[BoxGroup("전투")]` |
| `[HorizontalGroup("Name", 0.5f)]` | 수평 그룹, 너비 비율 | `[HorizontalGroup("기본", 0.5f)]` |
| `[VerticalGroup("Name")]` | 수직 그룹 | `[VerticalGroup("스탯")]` |
| `[TabGroup("Tabs", "Tab1")]` | 탭 그룹 | `[TabGroup("설정", "기본")]` |
| `[FoldoutGroup("Name")]` | 폴더블 그룹 | `[FoldoutGroup("고급")]` |

**중첩 규칙:**
```csharp
[HorizontalGroup("A", 0.5f)]
[BoxGroup("A/Left")]
public int left;

[HorizontalGroup("A", 0.5f)]
[BoxGroup("A/Right")]
public int right;
```

### 3.2 시각화 (Visualization)

| 어트리뷰트 | 설명 | 사용 예시 |
|-----------|------|----------|
| `[TableList]` | 리스트를 테이블 형태로 | `[TableList(ShowIndexLabels = true)]` |
| `[ListDrawerSettings]` | 리스트 설정 | `[ListDrawerSettings(Expanded = true)]` |
| `[PreviewField(60)]` | 프리뷰 이미지 | `[PreviewField(60, ObjectFieldAlignment.Left)]` |
| `[ProgressBar(min, max)]` | 프로그레스 바 | `[ProgressBar(0, 1000)]` |
| `[EnumToggleButtons]` | 토글 버튼 형태 enum | `[EnumToggleButtons]` |
| `[InlineProperty]` | 인라인 속성 표시 | `[InlineProperty]` |

### 3.3 조건부 표시

| 어트리뷰트 | 설명 | 사용 예시 |
|-----------|------|----------|
| `[ShowIf("Condition")]` | 조건 만족 시 표시 | `[ShowIf("IsAdvanced")]` |
| `[HideIf("Condition")]` | 조건 만족 시 숨김 | `[HideIf("IsSimple")]` |
| `[EnableIf("Condition")]` | 조건 만족 시 활성화 | `[EnableIf("CanEdit")]` |
| `[DisableIf("Condition")]` | 조건 만족 시 비활성화 | `[DisableIf("IsLocked")]` |

**ShowIf 조건 메서드:**
```csharp
#if ODIN_INSPECTOR
private bool IsAdvanced => _mode == Mode.Advanced;
private bool CanEdit => !_isLocked;
#endif
```

### 3.4 버튼 및 액션

```csharp
#if ODIN_INSPECTOR
[Button("버튼명", ButtonSizes.Large)]
[GUIColor(0.3f, 0.8f, 0.3f)]  // RGB 0-1
[EnableIf("@CanExecute")]
#endif
private void ActionMethod() { }
```

### 3.5 레이블 및 메타데이터

| 어트리뷰트 | 설명 |
|-----------|------|
| `[LabelText("표시명")]` | 커스텀 레이블 |
| `[HideLabel]` | 레이블 숨김 |
| `[SuffixLabel("단위", true)]` | 접미사 (bold=true) |
| `[Title("제목", TitleAlignment = TitleAlignments.Centered)]` | 섹션 제목 |
| `[PropertyRange(min, max)]` | 값 범위 제한 |
| `[MinValue(0)]` / `[MaxValue(100)]` | 최소/최대값 |
| `[AssetsOnly]` | Assets 폴더 리소스만 |
| `[SceneObjectsOnly]` | 씬 오브젝트만 |
| `[MultiLineProperty(5)]` | 멀티라인 텍스트 |

---

## 4. 데이터 타입별 표준 패턴

### 4.1 CombatStats (전투 스탯)
```csharp
[Serializable]
#if ODIN_INSPECTOR
[InlineProperty]
#endif
public struct CombatStats
{
#if ODIN_INSPECTOR
    [BoxGroup("기본 스탯")]
    [ProgressBar(0, 10000, ColorGetter = "GetHpColor")]
#endif
    [Min(0f)] public float hp;

#if ODIN_INSPECTOR
    [BoxGroup("기본 스탯")]
    [ProgressBar(0, 500, ColorGetter = "GetAtkColor")]
#endif
    [Min(0f)] public float atk;

#if ODIN_INSPECTOR
    private static Color GetHpColor() => new Color(1f, 0.3f, 0.3f);
    private static Color GetAtkColor() => new Color(1f, 0.6f, 0.2f);
#endif
}
```

### 4.2 ElementType (속성 enum)
```csharp
#if ODIN_INSPECTOR
[EnumToggleButtons]
#endif
public ElementType element = ElementType.Reason;
```

### 4.3 Sprite/GameObject (리소스)
```csharp
#if ODIN_INSPECTOR
[PreviewField(60, ObjectFieldAlignment.Left)]
[AssetsOnly]
#endif
public Sprite icon;

#if ODIN_INSPECTOR
[PreviewField(80, ObjectFieldAlignment.Left)]
[AssetsOnly]
#endif
public GameObject prefab;
```

### 4.4 List<T> (테이블 목록)
```csharp
#if ODIN_INSPECTOR
[TableList(ShowIndexLabels = true, AlwaysExpanded = true, DrawScrollView = true)]
[Searchable]
#endif
public List<RowType> rows = new();
```

---

## 5. 에디터 윈도우 패턴

### 5.1 기본 구조
```csharp
#if ODIN_INSPECTOR
public class MyToolWindow : OdinEditorWindow
#else
public class MyToolWindow : EditorWindow
#endif
{
    [MenuItem("Soul Ark/Tool Name")]
    private static void OpenWindow()
    {
        var window = GetWindow<MyToolWindow>();
        window.titleContent = new GUIContent("Tool Name");
        window.minSize = new Vector2(1200, 700);
        window.Show();
    }

#if ODIN_INSPECTOR
    [EnumToggleButtons, HideLabel]
    public ToolTab CurrentTab = ToolTab.Tab1;

    [ShowIf("@CurrentTab == ToolTab.Tab1")]
    [BoxGroup("그룹")]
    public ContentType content;
#endif
}
```

### 5.2 Odin Editor Window 특징
- `OdinEditorWindow` 상속 시 Odin 인스펙터 UI 자동 적용
- `OnEnable()`에서 초기화
- `OdinEditorWindow`는 Odin Inspector 설치 시에만 사용 가능

---

## 6. 고급 패턴

### 6.1 OnValueChanged (값 변경 콜백)
```csharp
#if ODIN_INSPECTOR
[OnValueChanged("OnTypeChanged")]
[EnumToggleButtons]
#endif
public EffectType effectType = EffectType.All;

#if ODIN_INSPECTOR
private void OnTypeChanged()
{
    // 값 변경 시 실행할 로직
}
#endif
```

### 6.2 ValidateInput (입력 검증)
```csharp
#if ODIN_INSPECTOR
[ValidateInput("ValidateHealth", "HP는 0보다 커야 합니다.")]
#endif
public float health;

#if ODIN_INSPECTOR
private bool ValidateHealth(float value) => value > 0;
#endif
```

### 6.3 InfoBox (정보 표시)
```csharp
#if ODIN_INSPECTOR
[InfoBox("이 설정은 고급 사용자용입니다.", InfoMessageType.Warning)]
#endif
public bool advancedMode;
```

### 6.4 PropertyOrder (순서 지정)
```csharp
#if ODIN_INSPECTOR
[PropertyOrder(1)]
#endif
public int first;

#if ODIN_INSPECTOR
[PropertyOrder(2)]
#endif
public int second;
```

---

## 7. 색상 팔레트

### 7.1 GUIColor RGB 값
```csharp
// 성공/확인 - 녹색
[GUIColor(0.3f, 0.8f, 0.3f)]

// 경고/주의 - 주황
[GUIColor(1f, 0.6f, 0.2f)]

// 정보/동작 - 파랑
[GUIColor(0.2f, 0.6f, 1f)]

// 위험/삭제 - 빨강
[GUIColor(0.9f, 0.3f, 0.3f)]
```

### 7.2 ProgressBar 색상
```csharp
private static Color GetHpColor() => new Color(1f, 0.3f, 0.3f);    // HP - 빨강
private static Color GetAtkColor() => new Color(1f, 0.6f, 0.2f);    // ATK - 주황
private static Color GetDefColor() => new Color(0.3f, 0.6f, 1f);    // DEF - 파랑
private static Color GetBuffColor() => new Color(0.3f, 0.8f, 0.3f); // 버프 - 녹색
private static Color GetDebuffColor() => new Color(0.8f, 0.3f, 0.3f); // 디버프 - 빨강
```

---

## 8. 검색 및 필터링

### 8.1 Searchable 속성
```csharp
#if ODIN_INSPECTOR
[TableList(ShowIndexLabels = true)]
[Searchable]  // 인스펙터에서 검색 가능
#endif
public List<Row> rows = new();
```

### 8.2 ListDrawerSettings 페이징
```csharp
#if ODIN_INSPECTOR
[ListDrawerSettings(Expanded = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
#endif
public List<Row> rows = new();
```

---

## 9. Soul Ark 프로젝트 적용 예시

### 9.1 AgentRow (캐릭터 데이터)
```csharp
namespace ProjectFirst.Data
{
    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class AgentRow
    {
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/ID")]
        public int id;

        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/이름")]
        public string name;

        [BoxGroup("전투")]
        [InlineProperty]
        public CombatStats stats;

        [HorizontalGroup("속성", 0.5f)]
        [BoxGroup("속성/타입")]
        [EnumToggleButtons]
        public ElementType element;

        [HorizontalGroup("속성", 0.5f)]
        [BoxGroup("속성/아이콘")]
        [PreviewField(60, ObjectFieldAlignment.Left)]
        public Sprite portrait;
    }
}
```

### 9.2 SkillRow (스킬 데이터)
```csharp
namespace ProjectFirst.Data
{
    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class SkillRow
    {
        [HorizontalGroup("기본", 0.33f)]
        [BoxGroup("기본/ID")]
        public int id;

        [HorizontalGroup("기본", 0.33f)]
        [BoxGroup("기본/이름")]
        public string name;

        [HorizontalGroup("기본", 0.34f)]
        [BoxGroup("기본/속성")]
        [EnumToggleButtons]
        public ElementType element;

        [BoxGroup("효과")]
        [EnumToggleButtons]
        [OnValueChanged("OnEffectTypeChanged")]
        public SkillEffectType effectType;

        [BoxGroup("효과")]
        [ShowIf("IsSingleTarget")]
        public float singleTargetBonus;

#if ODIN_INSPECTOR
        private bool IsSingleTarget => effectType == SkillEffectType.SingleTarget;
        private void OnEffectTypeChanged() { }
#endif
    }
}
```

---

## 10. 체크리스트

데이터 클래스 생성 시 확인:
- [ ] `#if ODIN_INSPECTOR` 사용 여부
- [ ] `Sirenix.OdinInspector` using 문
- [ ] 네임스페이스 `ProjectFirst.Data`
- [ ] `[Serializable]` 추가
- [ ] `[HideLabel]` (Row 클래스)
- [ ] 그룹화 (`BoxGroup`, `HorizontalGroup`)
- [ ] 시각화 (`PreviewField`, `ProgressBar`, `EnumToggleButtons`)
- [ ] 조건부 (`ShowIf`, `EnableIf`)
- [ ] 버튼 (`Button`, `GUIColor`)

---

## 11. 참고 링크

- Odin Inspector 문서: https://odininspector.com/documentation
- Attributes Reference: https://odininspector.com/documentation/sirenix/odininspector
- Editor Windows: https://odininspector.com/documentation/sirenix/odininspector/editorwindows

---

**버전:** 1.0  
**생성일:** 2026-03-12  
**프로젝트:** Soul Ark  
**작성:** 김민석 / msking1125@gmail.com
