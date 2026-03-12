---
description: Git 컨벤션 및 규칙 - 커밋 메시지 포맷, 작업 절차, AI 도구 사용 규칙
---

# Git 컨벤션 및 규칙

## 커밋자 정보 (필수)

```
[필수] 모든 Git 커밋은 반드시 아래 계정 정보로 수행합니다.
      AI 도구가 git commit을 실행할 때 이 정보를 사용해야 합니다.
```

| 항목 | 값 |
|------|-----|
| 이름 | 김민석 |
| 이메일 | msking1125@gmail.com |

**프로젝트 초기 설정 (최초 1회):**
```bash
git config user.name "김민석"
git config user.email "msking1125@gmail.com"
```

## 커밋 메시지 포맷

```
{커밋유형}: {제목}

- {변경 내용 1}
- {변경 내용 2}
```

## 커밋 유형 (영문, 첫 글자 대문자)

| 커밋 유형 | 의미 | 사용 예시 |
|-----------|------|-----------|
| `Feat` | 새로운 기능 추가 | `Feat: 로고 씬 FadeIn/FadeOut 구현` |
| `Fix` | 버그 수정 | `Fix: VFX Clone 누적 버그 수정` |
| `Docs` | 문서 수정 | `Docs: 로드맵 v0.4.0 업데이트` |
| `Style` | 코드 포맷팅 (기능 변경 없음) | `Style: SkillSystem 들여쓰기 정리` |
| `Refactor` | 코드 리팩토링 | `Refactor: BattleGameManager 클래스 분리` |
| `Test` | 테스트 코드 추가 | `Test: DamageCalculator 유닛 테스트` |
| `Chore` | 기타 수정 (.gitignore 등) | `Chore: Unity 메타 파일 정리` |
| `Design` | UI 디자인 변경 | `Design: 로비 탑바 레이아웃 변경` |
| `Comment` | 주석 추가/변경 | `Comment: Agent.cs XML 주석 추가` |
| `Rename` | 파일/폴더명 변경 | `Rename: LoginUI → LoginManager로 변경` |
| `Remove` | 파일 삭제 | `Remove: 미사용 테스트 에셋 삭제` |
| `!HOTFIX` | 긴급 버그 수정 | `!HOTFIX: 전투 중 크래시 수정` |

## 커밋 메시지 규칙

1. **제목은 한글로 작성** — 내용이 직관적으로 전달되도록
2. **제목 50자 이내** — 간결하게 핵심만
3. **제목 끝에 마침표(.) 금지**
4. **본문에는 "무엇을 & 왜" 설명** — "어떻게"보다 중요
5. **한 커밋에 한 가지 문제만** — 추적 가능하게 유지
6. **Phase 완료 시 태그 생성** — 예: `v0.4.0-phase2`

## 커밋 메시지 실전 예시

```bash
# 기능 추가
git commit -m "Feat: 로고 씬 FadeIn/FadeOut 구현

- LogoManager.cs 신규 생성
- Coroutine 기반 알파 보간 (DOTween 미사용)
- 로고 이미지 누락 시 안전하게 건너뜀"

# 버그 수정
git commit -m "Fix: VFX Clone 누적 버그 수정

- StopEmittingAndClear 호출 후 Play
- duration 계산하여 즉시 Destroy 예약
- Looping 여부와 관계없이 동일 로직 적용"

# Phase 완료 태그
git tag -a v0.4.0-phase2 -m "Phase 2 완료: 로고/로그인/닉네임/서버선택"
```

## AI 도구의 Git 사용 규칙

```
[필수] AI 도구가 자동으로 git commit을 수행할 때:
1. 반드시 위 커밋 메시지 포맷을 따를 것
2. 커밋자는 항상 김민석 / msking1125@gmail.com
3. 한 번에 여러 기능을 커밋하지 말 것 (기능별 분리 커밋)
4. 커밋 전 Unity 컴파일 에러가 없는지 확인할 것
5. .meta 파일이 누락되지 않았는지 확인할 것
```

## Windsurf 브랜치 업데이트 절차

1. **main 브랜치 최신화**
   ```bash
   git checkout main
   git pull origin main
   ```

2. **windsurf 브랜치로 전환 및 업데이트**
   ```bash
   git checkout windsurf
   git pull origin main
   ```

3. **변경사항 커밋 및 푸시**
   ```bash
   git add .
   git commit -m "커밋 메시지"
   git push origin windsurf
   ```

// turbo

## 빠른 업데이트 (한 줄 명령어)

```bash
git checkout windsurf && git pull origin main && git push origin windsurf
```
