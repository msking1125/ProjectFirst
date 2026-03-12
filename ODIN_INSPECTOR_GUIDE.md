# Odin Inspector 규약 (Codex Cloud 우선 적용)

이 문서는 Codex Cloud, Cursor, Claude 환경에서 공통으로 적용할 Odin Inspector 작성 규약입니다.

## 적용 우선순위
1. `CLAUDE.md`
2. `.cursorrules`
3. `ODIN_INSPECTOR_GUIDE.md`

상위 문서와 충돌 시 상위 문서 규칙을 우선합니다.

## 필수 규약
- Odin 속성은 **가독성 향상과 인스펙터 작업 효율**을 위한 최소 범위에서만 사용합니다.
- 런타임 동작을 Odin 전용 기능에 의존하지 않습니다.
- private 필드 노출 시 기존 프로젝트 규칙을 유지합니다.
  - 기본: `[SerializeField] private` + 프로퍼티
  - Odin 보조 노출이 필요할 때만 `[ShowInInspector]`를 사용합니다.
- 그룹 계열 속성(`TitleGroup`, `BoxGroup`, `FoldoutGroup`)은 한 클래스에서 일관된 패턴으로 통일합니다.
- 버튼 계열 속성(`Button`)은 디버그/개발 편의용 메서드에만 적용하고, 빌드 영향이 없도록 작성합니다.
- 유효성 계열 속성(`ValidateInput`, `MinValue`, `Required`)을 우선 사용해 데이터 오류를 사전 차단합니다.

## 권장 패턴
- 데이터 설정 클래스: `InfoBox` + `ValidateInput` + 범위 제한 속성 조합
- 디버그용 컴포넌트: `FoldoutGroup("Debug")` 하위에 `Button` 배치
- 팀 협업 시: 동일 목적 속성은 동일한 순서로 선언

## 금지/주의
- 과도한 인스펙터 커스터마이징으로 기본 Unity Inspector 흐름을 해치지 않습니다.
- 테스트가 필요한 핵심 로직을 인스펙터 버튼 호출 전용으로 숨기지 않습니다.
- Odin 미설치 환경에서도 컴파일 가능한 구조를 유지합니다.

## 체크리스트
- [ ] Soul Ark 코딩 규칙(네임스페이스/필드/주석/씬 전환) 준수
- [ ] Odin 속성 도입 이유가 명확함
- [ ] 디버그 도구와 런타임 로직이 분리됨
- [ ] 속성 사용 패턴이 팀 컨벤션과 일치함
