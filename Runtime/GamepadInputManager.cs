using Input.Controls;
using UnityEngine;

namespace Input
{
    /// <summary>
    /// 컨트롤러 스타일 입력(아날로그 스틱과 버튼)을 위한 입력 관리자입니다.
    /// </summary>
    public class GamepadInputManager : MonoBehaviour
    {
        // 게임패드 입력 액션 맵입니다.
        private GamepadControls rollingControls;

        /// <summary>
        /// 아날로그 스틱의 현재 상태를 가져옵니다.
        /// </summary>
        public Vector2 AnalogueValue { get; private set; }

        /// <summary>
        /// 기본 버튼의 상태를 가져옵니다.
        /// </summary>
        public bool PrimaryButtonValue { get; private set; }

        /// <summary>
        /// 보조 버튼의 상태를 가져옵니다.
        /// </summary>
        public bool SecondaryButtonValue { get; private set; }

        protected virtual void Awake()
        {
            rollingControls = new GamepadControls();

            rollingControls.gameplay.movement.performed += context => AnalogueValue = context.ReadValue<Vector2>();
            rollingControls.gameplay.movement.canceled += context => AnalogueValue = Vector2.zero;

            rollingControls.gameplay.button1Action.performed +=
                context => PrimaryButtonValue = context.ReadValue<float>() > 0.5f;
            rollingControls.gameplay.button1Action.canceled +=
                context => PrimaryButtonValue = false;

            rollingControls.gameplay.button2Action.performed +=
                context => SecondaryButtonValue = context.ReadValue<float>() > 0.5f;
            rollingControls.gameplay.button2Action.canceled +=
                context => SecondaryButtonValue = false;
        }

        protected virtual void OnEnable()
        {
            rollingControls?.Enable();
        }

        protected virtual void OnDisable()
        {
            rollingControls?.Disable();
        }
    }
}
