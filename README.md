# SKKU_SRPG_AI
성균관대학교 졸업작품(유니티) — SRPG(전술 RPG) 구조 위에 **AI 기반 대화/이벤트 선택 시스템**을 결합한 프로젝트

> 목표: 플레이 흐름(탐험/거점/전투) 속에서, 상황에 따라 **이벤트 텍스트와 선택지(Option A/B)**가 동적으로 구성되고
> 선택 결과가 플레이어 성향/관계/상태에 반영되는 인터랙티브 SRPG 경험을 구현

---

## 시작하기

### 요구 사항
- Unity 6.3 (`6000.3.8f1`) 이상
- OpenAI API Key (LLM 기능 사용 시)

### 실행 방법
1. 레포지토리 클론
   ```bash
   git clone https://github.com/hoeo159/SKKU_SRPG_AI.git
   ```
2. Unity Hub에서 프로젝트 열기
3. OpenAI API Key 설정 (아래 [API 설정](#api-설정) 참고)
4. `Assets/_Project/Scenes/Boot.unity` 씬을 열고 Play

---

## 주요 기능

- **씬 구성**
  - `Boot` → `Hub(거점)` → `Expedition(탐험)` 씬 구성
  - 씬 전환에도 상태가 유지되는 런타임 상태 관리 구조

- **런타임 상태 관리**
  - `GameStateSO` 템플릿을 런타임에 `Instantiate`하여 안전하게 상태 유지
  - `GameManager`는 `DontDestroyOnLoad` 싱글톤으로 전체 흐름 관리

- **AI/LLM 기반 대화 및 이벤트 결과 구조**
  - `TalkDirector`가 AI 응답을 받아 다음을 처리:
    - `reply` : 화면에 출력되는 대사/텍스트
    - `affinityDelta` : 호감도/관계 변화량
    - `memorySummary` : 요약 메모리(장기 반영용)
    - `tags` : 상황/성향 태그

- **EventCardSO 기반 이벤트 데이터**
  - ScriptableObject로 이벤트 카드/데이터 관리
  - 화면 중앙 출력 + Option A/B 선택 + Custom 확장

- **전투 AI**
  - `EnemyUtilityAI` 기반 적 행동 결정
  - `BattleController`가 턴 순서/행동 처리 관리

---

## 프로젝트 구조
> 실제 폴더 구조는 변경될 수 있으며, 핵심 역할 기준으로 정리했습니다.

- **Core / State**
  - `GameStateSO` : 런타임 상태 템플릿
  - `GameManager` : 전역 상태/씬 전환 관리 (DontDestroyOnLoad)
  - `BootLoader` : 초기화 후 Hub 진입 흐름 고정
  - `ProfileCalculator` : 플레이어 성향/관계 수치 계산

- **Hub**
  - `HubDirectorRunner` : 거점 흐름 총괄
  - `HubEventGenerator` : 이벤트 카드 동적 생성
  - `HubEventBoard` : 이벤트 카드 목록 표시/관리

- **Expedition**
  - `ExpedController` : 탐험 씬 전체 흐름 관리
  - `GridManager` : 격자 맵 관리
  - `WorldTurnRunner` : 탐험 턴 진행
  - `NpcController` : NPC 행동 제어

- **Combat**
  - `BattleController` : 전투 턴/행동 처리
  - `EnemyUtilityAI` : 적 행동 결정 AI
  - `CombatUnit` : 전투 유닛 데이터/상태

- **AI**
  - `TalkDirector` : AI 응답 수신/규칙 적용/결과 반영
  - `PromptContextBuilder` : 상황 기반 프롬프트 구성
  - `OpenAIResponseClient` : 외부 모델 호출 및 응답 파싱

- **Event**
  - `EventCardSO` : 이벤트 카드 데이터
  - `EventDirector` : 이벤트 트리거/표시/흐름 제어
  - `EventApplier` : 이벤트 선택 결과 게임 상태에 반영

- **UI**
  - `HubUI` : 거점 UI 및 이동 버튼/상태 표시
  - `HubEventUI` : 거점 이벤트 카드 UI
  - `ExpeditionReturnUI` : 탐험 복귀 UI / 테스트 인터페이스

---

## API 설정

본 프로젝트는 LLM을 위한 외부 API를 사용합니다.
`OpenAIResponseClient.cs`에서 아래 환경 변수 또는 Inspector 필드를 통해 설정합니다.

| 항목 | 설명 |
|---|---|
| `OPENAI_API_KEY` | OpenAI API 인증 키 |
| Model | 사용할 모델 ID (예: `gpt-4o`) |

> API Key를 코드에 직접 하드코딩하지 마세요. 환경 변수 또는 별도 설정 파일을 사용하세요.

---

## Credits
- Developer: hoeo159 (Minho Baek)
- Project: SKKU Graduation Project (Unity SRPG + AI)

---

## License
MIT License
