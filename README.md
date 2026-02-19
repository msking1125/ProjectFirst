# ProjectFirst
Unity 3D Idle Defence Game

## 목표
3D 맵에서 적 웨이브를 자동으로 방어하고, 플레이어는 업그레이드/배치/스킬 타이밍에 집중하는 **Idle Defence** 게임을 제작합니다.

---

## 코어 게임 루프
1. 전투 시작 → 적 웨이브 자동 생성
2. 아군 타워/유닛이 자동 공격
3. 처치 보상(골드, 재료) 획득
4. 전투 중/전투 후 업그레이드
5. 더 강한 웨이브 도전
6. 특정 구간에서 Prestige(환생)로 장기 성장

---

## MVP(첫 플레이 가능 버전) 범위
- 단일 스테이지(1 맵)
- 적 타입 3종(근접, 원거리, 보스)
- 타워 3종(단일딜, 광역, 감속)
- 재화 1~2종(골드 + 선택적 코어)
- 업그레이드 트리(공격력, 공격속도, 사거리)
- 웨이브 시스템(일반 웨이브 + 보스 웨이브)
- 오프라인 보상(간단 버전: 최대 N시간)

---

## Unity 권장 기술 스택
- **Unity 2022 LTS 이상**
- **URP**(모바일/PC 확장성)
- **C# + ScriptableObject**(데이터 분리)
- **Addressables**(에셋 관리)
- **Cinemachine**(카메라 연출)
- **TextMeshPro**(UI)

---

## 시스템 설계(초안)

### 1) 전투 시스템
- `EnemySpawner`: 웨이브 테이블 기준 스폰
- `EnemyController`: 이동/공격/사망 처리
- `TowerController`: 타겟 탐색, 발사, 쿨타임
- `Projectile`: 충돌, 데미지, 이펙트

### 2) 경제/성장 시스템
- `CurrencyService`: 재화 증감, 저장
- `UpgradeService`: 업그레이드 레벨/비용 계산
- `PrestigeService`: 환생 보상 계산

### 3) 진행도 저장
- `SaveService`: 로컬 저장(JSON)
- 저장 항목: 재화, 업그레이드, 스테이지, 마지막 접속 시간

### 4) 오프라인 보상
- 마지막 종료 시간 기준 경과 시간 계산
- 분당 수익 × 경과 시간(상한 적용)

---

## 데이터 구조 예시
- `EnemyData`(SO): 체력, 속도, 보상, 프리팹
- `TowerData`(SO): 기본 공격력, 공속, 사거리, 탄환 타입
- `WaveData`(SO): 웨이브 번호, 적 구성, 스폰 간격
- `UpgradeData`(SO): 업그레이드 대상, 계수, 최대 레벨

---

## 개발 로드맵(4주 샘플)

### Week 1: 전투 기본기
- 맵/경로 세팅
- 적 스폰 + 이동
- 타워 자동 타겟팅/발사
- 전투 승패 처리

### Week 2: 경제/업그레이드
- 보상 지급
- 업그레이드 UI + 수치 반영
- 웨이브 난이도 스케일링

### Week 3: 메타 시스템
- 저장/불러오기
- 오프라인 보상
- 간단 환생 시스템

### Week 4: 폴리싱
- UI/UX 개선
- 밸런스 1차 조정
- 성능 최적화(오브젝트 풀링, Update 최소화)

---

## 폴더 구조 권장
```text
Assets/
  _Project/
    Scripts/
      Core/
      Combat/
      Economy/
      UI/
      Data/
    ScriptableObjects/
    Prefabs/
    Scenes/
    Art/
```

---

## 다음 액션(바로 시작)
1. Unity URP 3D 템플릿으로 프로젝트 생성
2. `Game`, `Battle`, `UI` 씬 3개 구성
3. Enemy/Tower 최소 프리팹 1개씩 제작
4. 웨이브 1개만 동작하는 프로토타입 완성
5. 플레이 테스트 후 DPS/HP 밸런스 1차 조정

원하시면 다음 단계로, 제가 **Unity C# 스크립트 뼈대(Spawner, Tower, Save, OfflineReward)** 를 바로 만들어드릴게요.

---

## Enemy Pooling 테스트 절차 (Unity Play Mode)

아래 절차로 **300마리 스폰** 상황에서 풀 재사용/안정성을 확인할 수 있습니다.

1. `Battle_Test` 씬(또는 실제 전투 씬)에 `EnemyPool` 컴포넌트를 가진 오브젝트를 추가합니다.
2. `EnemyPool.enemyPrefab`에 `Enemy01` 프리팹을 연결합니다.
3. `EnemyPool.initialCapacity`를 `60`~`100`으로 설정하고 `allowExpand=true`로 둡니다.
4. `EnemySpawner.enemyPool`에 위 `EnemyPool`을 연결하고, `arkTarget`/`spawnPoints`를 모두 지정합니다.
5. 빠른 부하 테스트를 위해 `EnemySpawner.spawnInterval = 0.03`~`0.05`로 설정합니다.
6. Play 실행 후 콘솔 에러가 없는지 확인합니다.
7. Hierarchy의 `Enemy(Clone)` 개수가 웨이브 진행 중 계속 선형 증가하지 않고, 일정 범위 안에서 재사용되는지 확인합니다.
8. 적이 사망할 때 `Destroy`가 아니라 `Return`으로 회수되어 다시 스폰되는지 확인합니다.
9. 전투 중 Agent 타겟팅이 정상인지(`EnemyManager.activeEnemies`가 등록/해제되는지) 확인합니다.
10. 300마리 이상 누적 스폰 후에도 이동/공격/사망 루프가 깨지지 않는지 최종 확인합니다.
