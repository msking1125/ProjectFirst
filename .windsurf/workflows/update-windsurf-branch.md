---
description: Windsurf 브랜치 업데이트 워크플로우 - main 브랜치 기반으로 windsurf 브랜치 유지
---

# Windsurf 브랜치 업데이트 워크플로우

이 워크플로우는 항상 `main` 브랜치의 최신 데이터를 기반으로 `windsurf` 브랜치를 업데이트하여 사용하는 방법을 설명합니다.

## 1. main 브랜치 최신화

```bash
git checkout main
git pull origin main
```

## 2. windsurf 브랜치로 전환 및 main 내용 병합

```bash
git checkout windsurf
git merge main
```

또는 rebase 방식:

```bash
git checkout windsurf
git rebase main
```

## 3. 충돌 해결 (필요시)

병합 충돌이 발생하면 수동으로 해결 후:

```bash
git add .
git rebase --continue
```

## 4. 변경사항 푸시

```bash
git push origin windsurf
```

// turbo

## 빠른 업데이트 (한 줄 명령어)

```bash
git checkout windsurf && git pull origin main && git push origin windsurf
```

## 주의사항

- **반드시 main 브랜치를 기반으로 업데이트**
- windsurf 브랜치에서 직접 작업한 내용이 있다면 먼저 commit/push
- 충돌 발생 시 신중하게 해결
