# Geuneda Input Extensions

Unity Input System 위에서 포인터(마우스, 펜, 터치) 드래그, 스와이프/탭 제스처, 게임패드 입력을 이벤트와 프로퍼티로 받을 수 있게 해 주는 패키지입니다.

## 설치

Unity 에디터에서 `Window > Package Manager` > `+` > `Add package from git URL...` 을 선택하고 아래 URL 을 입력합니다.

```
https://github.com/geuneda/geuneda-inputextensions.git
```

또는 프로젝트의 `Packages/manifest.json` 에 직접 추가합니다.

```json
{
  "dependencies": {
    "com.geuneda.inputextensions": "https://github.com/geuneda/geuneda-inputextensions.git"
  }
}
```

특정 버전에 고정하려면 URL 끝에 `#<태그>` 를 붙입니다. 사용할 수 있는 태그는 GitHub 레포에서 확인하세요.

## 빠른 시작

1. 씬의 GameObject 에 `PointerInputManager` 를 추가하고, 인스펙터에서 사용할 장치(`Use Mouse`, `Use Pen`, `Use Touch`)를 켭니다. 세 값의 기본값은 모두 꺼짐입니다.
2. 같은 GameObject(또는 다른 GameObject)에 `GestureController` 를 추가하고 `Input Manager` 필드에 1번의 `PointerInputManager` 를 연결합니다.
3. 아래 스크립트를 추가하고 `Gesture Controller` 필드를 연결합니다.

```csharp
using Input;
using UnityEngine;

public class GestureLogger : MonoBehaviour
{
    [SerializeField] private GestureController gestureController;

    private void OnEnable()
    {
        gestureController.Swiped += OnSwiped;
        gestureController.Tapped += OnTapped;
    }

    private void OnDisable()
    {
        gestureController.Swiped -= OnSwiped;
        gestureController.Tapped -= OnTapped;
    }

    private void OnSwiped(SwipeInput swipe)
    {
        Debug.Log($"Swipe {swipe.SwipeDirection}, {swipe.SwipeVelocity} px/s");
    }

    private void OnTapped(TapInput tap)
    {
        Debug.Log($"Tap at {tap.ReleasePosition}");
    }
}
```

## 핵심 개념과 기능

네임스페이스는 `Input`(런타임 컴포넌트와 데이터 타입)과 `Input.Controls`(`.inputactions` 에서 자동 생성된 래퍼)입니다. 어셈블리 이름은 `Geuneda.InputExtensions` 입니다.

```
PointerControls  ->  PointerInputManager  ->  GestureController  ->  SwipeInput / TapInput
GamepadControls  ->  GamepadInputManager
```

| 타입 | 종류 | 역할 |
|------|------|------|
| `PointerInputManager` | MonoBehaviour | `PointerControls` 의 입력을 읽어 `Pressed`, `Dragged`, `Released` 이벤트(`Action<PointerInput, double>`)를 발행합니다. |
| `GestureController` | MonoBehaviour | `PointerInputManager` 이벤트를 받아 `Pressed`, `PotentiallySwiped`, `Swiped`(`Action<SwipeInput>`), `Tapped`(`Action<TapInput>`)를 발행합니다. |
| `GamepadInputManager` | MonoBehaviour | `AnalogueValue`(`Vector2`), `PrimaryButtonValue`, `SecondaryButtonValue`(`bool`)를 프로퍼티로 노출합니다. 매 프레임 폴링해서 씁니다. |
| `PointerInput` | struct | 포인터 한 번의 값. `Contact`, `InputId`, `Position` 과 장치에 따라 값이 없을 수 있는 `Tilt`, `Pressure`, `Radius`, `Twist`(nullable). |
| `SwipeInput` | struct | 스와이프 결과. 시작/이전/끝 위치, `SwipeDirection`, `SwipeVelocity`, `TravelDistance`, `SwipeDuration`, `SwipeSameness`. |
| `TapInput` | struct | 탭 결과. `PressPosition`, `ReleasePosition`, `TapDuration`, `TapDrift`, `TimeStamp`. |
| `PointerInputComposite` | InputBindingComposite | 여러 컨트롤을 하나의 `PointerInput` 값으로 합치는 컴포지트 바인딩. 에디터와 런타임에서 자동 등록됩니다. |

기본 바인딩:

- `PointerControls`: 마우스와 펜을 묶은 컴포지트 1개, 터치 `touch0`~`touch4` 컴포지트 5개. 컨트롤 스킴은 `Mouse`, `Pen`, `Touch` 입니다.
- `GamepadControls`: `movement` = Left Stick, WASD / `button1Action` = Button South, Space, Z / `button2Action` = Button East, X.

`GestureController` 의 판정 기준(`maxTapDuration`, `maxTapDrift`, `maxSwipeDuration`, `minSwipeDistance`, `swipeDirectionSamenessThreshold`)은 인스펙터에서 조정합니다.

### 주의 사항

- 네임스페이스 `Input` 이 전역 네임스페이스에 있어서 `UnityEngine.Input` 클래스를 가립니다. 이 어셈블리를 참조하는 코드에서 레거시 입력 API 를 쓰려면 `UnityEngine.Input.GetKey(...)` 처럼 완전한 이름을 써야 합니다.
- `PointerInputManager` 는 누름 상태를 `PointerInput.InputId` 별로 관리합니다. 포인터마다 첫 접촉은 `Pressed`, 이후 이동은 `Dragged`, 해제는 `Released` 로 옵니다. `point` 액션이 PassThrough 라 여러 포인터(멀티터치, 마우스와 터치 동시)를 함께 받고, 초기 상태 검사가 켜져 있어 활성화할 때 이미 눌린 포인터도 `Pressed` 로 옵니다(Input System 1.1 이상. 선언된 최소 버전 1.0.0-preview.4 처럼 그 미만에서는 활성화 뒤 첫 입력 변화부터 받습니다). 컴포넌트가 비활성화되거나 액션이 리셋되거나 장치가 분리되면 이벤트 없이 그 포인터들의 눌림 상태만 비웁니다(입력 콜백 안에서 비활성화해도 `Released` 로 처리되지 않습니다). 그 뒤 같은 포인터의 접촉은 다시 `Pressed` 로 오므로, 직접 상태를 추적한다면 같은 `InputId` 의 `Pressed` 를 새 시작으로 처리하고 자신의 `OnDisable` 에서도 상태를 비우세요. `GestureController` 는 새 `Pressed` 때 관리자에서 더 이상 눌려 있지 않은 포인터의 남은 제스처를 정리합니다.
- `GestureController` 는 누른 시점부터 뗀 시점까지의 지속 시간이 `maxTapDuration` 이하일 때만 탭, `maxSwipeDuration` 이하일 때만 스와이프로 판정합니다. `minSwipeDistance <= maxTapDrift` 이면 한 동작이 `Swiped` 와 `Tapped` 를 모두 발행할 수 있습니다.
- `GestureController` 의 디버그용 `label` 을 연결하면 `Camera.main` 을 사용하므로 씬에 MainCamera 태그가 붙은 카메라가 있어야 합니다.
- `Input.Controls` 의 `PointerControls`, `GamepadControls` 는 자동 생성 코드이므로 직접 수정하지 말고 `.inputactions` 파일을 고친 뒤 다시 생성합니다.

## 요구 사항과 의존성

- Unity 2019.3 이상
- `com.unity.inputsystem` 1.0.0-preview.4 이상(`package.json` 선언 기준)
- Player Settings 의 `Active Input Handling` 에서 Input System 을 사용하도록 설정해야 합니다.
- `com.unity.ugui` 1.0.0 이상(`package.json` 선언 기준, Unity 6 에서는 2.0.0 으로 해석). 런타임 코드가 `UnityEngine.UI.Text`, `UnityEngine.EventSystems.PointerInputModule` 을 사용합니다.

변경 내역은 [CHANGELOG.md](CHANGELOG.md) 를 참고하세요.

## 라이선스

MIT 라이선스입니다. 자세한 내용은 [LICENSE.md](LICENSE.md) 를 참고하세요. 이 패키지에는 Unity Technologies 의 코드가 포함되어 있으며 해당 구성 요소는 Unity Companion Package License v1.0 을 따릅니다. 자세한 내용은 [Third Party Notices.md](Third%20Party%20Notices.md) 를 참고하세요.
