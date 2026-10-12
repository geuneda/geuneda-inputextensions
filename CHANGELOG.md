# 변경 이력
이 패키지에 대한 모든 주요 변경 사항이 이 파일에 기록됩니다.

이 형식은 [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)를 기반으로 하며,
이 프로젝트는 [Semantic Versioning](http://semver.org/spec/v2.0.0.html)을 따릅니다.

## [1.0.2] - 2026-10-12

**수정**:
- `PointerInputManager` 가 누름 상태를 포인터 하나의 플래그로 관리해 멀티터치에서 두 번째 포인터의 첫 접촉이 `Dragged` 로 오거나 `Released` 가 누락되던 문제 수정. 이제 `InputId` 별로 관리하고, 비활성화되면 눌림 상태를 비웁니다
- `point` 액션 타입을 원래 설계대로 `PassThrough` 로 변경해 여러 포인터(멀티터치, 마우스와 터치)의 입력을 동시에 받도록 수정(`.inputactions` 와 생성 코드의 내장 JSON 을 함께 변경)
- `point` 액션에 초기 상태 검사(`initialStateCheck`)를 켜서 활성화할 때 이미 눌린 포인터도 `Pressed` 로 받도록 수정. 이 기능은 Input System 1.1 이상에서 동작하며, 선언된 최소 버전(1.0.0-preview.4)처럼 그 미만에서는 활성화 뒤 첫 입력 변화부터 받습니다
- 비활성화, 액션 리셋(canceled), 장치 분리 때는 이벤트 없이 눌림 상태만 비웁니다. canceled 값은 기본값이라 정상 해제로 처리하지 않습니다(이전에는 콜백 안에서 비활성화하면 기본값 좌표의 `Released` 가 발행될 수 있었음)
- `GestureController` 가 해제를 받지 못하고 남은 제스처가 있을 때 다음 `Pressed` 에서 중복 키 예외 없이 새 제스처로 교체하도록 수정

## [1.0.1] - 2026-10-12

**수정**:
- `GestureController` 의 탭/스와이프 판정에서 지속 시간을 `StartTime - EndTime` 으로 계산해 `maxTapDuration`, `maxSwipeDuration` 이 반영되지 않던 문제 수정
- `PointerInputManager` 가 누르지 않은 상태에서도 `Contact == false` 값마다 `Released` 를 발행하던 문제 수정. 이제 누름 상태일 때만 발행합니다
- `GestureController.Pressed` 의 불필요한 `new` 한정자 제거(CS0109 경고)
- 런타임 어셈블리가 사용하는 uGUI(`UnityEngine.UI`)를 asmdef 참조와 `package.json` 의존성(`com.unity.ugui` 1.0.0, 최소 Unity 2019.3 에서도 해석되는 버전)에 추가
- 자동 생성 코드 헤더의 옛 패키지 경로와 `PointerInputManager` 주석의 `point` 액션 타입 설명을 실제와 맞춤
- `LICENSE.md` 추가

**버전 정규화**:
- `package.json` 은 `0.1.0-preview.4` 였지만 이 CHANGELOG 의 최신 항목과 기존 `v1.0.0` 태그가 1.0.0 이라, 그 다음 버전인 1.0.1 로 맞췄습니다.

## [1.0.0] - 2020-02-07

- Input System 1.0.0-preview.4 버전으로 업데이트

## [0.1.0] - 2020-01-06

- 패키지 배포를 위한 최초 제출
