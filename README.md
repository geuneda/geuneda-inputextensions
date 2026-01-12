# Geuneda Input Extensions

Unity의 새로운 Input System 패키지를 위한 드래그 확장 기능을 제공하는 패키지입니다.

## 개요

이 패키지는 Unity의 새로운 Input System을 확장하여 터치 및 드래그 입력을 보다 쉽게 처리할 수 있게 해줍니다.

## 요구 사항

- Unity 2019.3 이상
- Unity Input System 패키지 (`com.unity.inputsystem`)

## 설치 방법

### Unity Package Manager를 통한 설치

1. Unity 에디터에서 `Window` > `Package Manager`를 엽니다.
2. 좌측 상단의 `+` 버튼을 클릭하고 `Add package from git URL...`을 선택합니다.
3. 다음 URL을 입력합니다:
   ```
   https://github.com/geuneda/geuneda-inputextensions.git
   ```
4. `Add` 버튼을 클릭합니다.

### manifest.json을 통한 설치

프로젝트의 `Packages/manifest.json` 파일에 다음을 추가합니다:

```json
{
  "dependencies": {
    "com.geuneda.inputextensions": "https://github.com/geuneda/geuneda-inputextensions.git"
  }
}
```

## 네임스페이스

```csharp
using Geuneda.InputExtensions;
```

## 라이선스

MIT License
