# SKKU_SRPG_AI
성균관대학교 졸업작품(유니티) — SRPG(전술 RPG) 구조 위에 **AI 기반 대화/이벤트 선택 시스템**을 결합한 프로젝트

> 목표: 플레이 흐름(탐험/거점/전투) 속에서, 상황에 따라 **이벤트 텍스트와 선택지(Option A/B)**가 동적으로 구성되고  
> 선택 결과가 플레이어 성향/관계/상태에 반영되는 인터랙티브 SRPG 경험을 구현

---

## Main Features

- **씬 구성**
  - `Boot` → `Hub(거점)` → `Expedition(탐험)` 씬 구성
  - 씬 전환에도 상태가 유지되는 런타임 상태 관리 구조

- **Runtime State Management**
  - `GameStateSO` 템플릿을 런타임에 `Instantiate`하여 안전하게 상태 유지
  - `GameManager`는 `DontDestroyOnLoad` 싱글톤으로 전체 흐름 관리

- **AI/LLM 기반 대화 및 이벤트 결과 구조(진행 중)**
  - `TalkDirector`가 AI 응답을 받아 다음을 처리:
    - `reply` : 화면에 출력되는 대사/텍스트
    - `affinityDelta` : 호감도/관계 변화량
    - `memorySummary` : 요약 메모리(장기 반영용)
    - `tags` : 상황/성향 태그

- **EventCardSO 기반 이벤트 데이터**
  - ScriptableObject로 이벤트 카드/데이터 관리
  - 화면 중앙 출력 + Option A/B 선택 + Custom 확장

---

## Project Structure (Core)
> 실제 폴더 구조는 변경될 수 있으며, 핵심 역할 기준으로 정리했습니다.

- **Core / State**
  - `GameStateSO` : 런타임 상태 템플릿
  - `GameManager` : 전역 상태/씬 전환 관리 (DontDestroyOnLoad)
  - `BootLoader` : 초기화 후 Hub 진입 흐름 고정

- **UI**
  - `HubUI` : 거점 UI 및 이동 버튼/상태 표시
  - `ExpeditionReturnUI` : 탐험 복귀 UI / 테스트 인터페이스

- **AI**
  - `TalkDirector` : AI 응답 수신/규칙 적용/결과 반영
  - `OpenAIResponseClient` : 외부 모델 호출 및 응답 파싱 (프로젝트 설정 필요)

- **Event**
  - `EventCardSO` : 이벤트 카드 데이터 관리
  - `EventSystem` : 이벤트 트리거/표시/선택 처리

---

## Version
1. Unity 버전: Unity 6.3 (6000.3.8f1)

---

## API
본 프로젝트는 LLM을 위한 외부 API를 사용할 수 있습니다.

---

## Credits
- Developer: hoeo159 (Minho Baek)
- Project: SKKU Graduation Project (Unity SRPG + AI)

---

## License
- Mit Licensew