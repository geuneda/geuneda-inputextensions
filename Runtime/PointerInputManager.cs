using System;
using System.Collections.Generic;
using Input.Controls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Input
{
    /// <summary>
    /// 주로 드래그 관련 컨트롤을 위해 펜, 마우스, 터치 입력을 해석하는 입력 관리자입니다.
    /// 압력, 기울기, 비틀림, 터치 반경을 드로잉 컴포넌트에 전달하여 처리합니다.
    /// </summary>
    /// <remarks>
    /// 컨트롤 설정에 대한 몇 가지 참고 사항:
    ///
    /// - 멀티터치를 지원하기 위해 터치는 `&lt;Pointer&gt;/position` 등을 사용하는 대신
    ///   마우스 및 펜과 별도로 분리되어 있습니다. <see cref="Touchscreen.position"/> 등에
    ///   바인딩하면 기본 터치만 올바르게 수신됩니다. 따라서 펜과 마우스의 바인딩을
    ///   터치와 별도로 설정합니다.
    /// - 마우스와 펜은 하나의 컴포지트로 묶여 있습니다. 이들은 서로 독립적으로 사용되지 않으므로
    ///   별도의 포인터 소스로 표현할 필요가 없다는 전제입니다. 하지만 마우스용
    ///   <see cref="PointerInputComposite"/>와 펜용을 각각 만들 수도 있습니다.
    /// - <see cref="PointerControls.PointerActions.point"/>의 액션 타입은 <see cref="InputActionType.PassThrough"/>입니다.
    ///   충돌 해결 없이 모든 컴포지트 바인딩(마우스/펜, 터치 0~4)의 변화가 그대로 들어오므로 여러 포인터를 동시에 받습니다.
    ///   초기 상태 검사가 켜져 있어 활성화할 때 이미 눌린 포인터도 <see cref="Pressed"/>로 들어옵니다.
    ///   누름 상태는 <see cref="PointerInput.InputId"/>별로 따로 관리합니다.
    /// - 비활성화, 액션 리셋, 장치 분리로 해제 입력을 받을 수 없게 되면 이벤트 없이 해당 포인터의 눌림 상태만 비웁니다.
    ///   그 뒤 같은 포인터의 접촉은 다시 <see cref="Pressed"/>로 오므로, 구독자는 같은 InputId 의 Pressed 를 새 시작으로 처리하고
    ///   자신이 비활성화될 때 자신의 상태도 비워야 합니다.
    /// </remarks>
    public class PointerInputManager : MonoBehaviour
    {
        /// <summary>
        /// 사용자가 화면을 눌렀을 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<PointerInput, double> Pressed;

        /// <summary>
        /// 사용자가 화면을 드래그할 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<PointerInput, double> Dragged;

        /// <summary>
        /// 사용자가 누르기를 해제했을 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<PointerInput, double> Released;


        // 눌려 있는 포인터의 InputId 와 그 장치. 포인터마다 Pressed/Dragged/Released 를 따로 판단한다
        private readonly Dictionary<int, InputDevice> m_PressedPointers = new Dictionary<int, InputDevice>();
        private PointerControls m_Controls;

        // 디버깅에 유용하며, 특히 터치 시뮬레이션이 켜져 있을 때 사용합니다.
        [SerializeField] private bool m_UseMouse;
        [SerializeField] private bool m_UsePen;
        [SerializeField] private bool m_UseTouch;

        protected virtual void Awake()
        {
            m_Controls = new PointerControls();

            m_Controls.pointer.point.performed += OnAction;
            // canceled 는 액션 전체가 리셋될 때 오고 값이 기본값이라 어느 포인터인지 알 수 없다.
            // 정상 해제로 판정하지 않고 눌림 상태만 비운다.
            m_Controls.pointer.point.canceled += OnActionCanceled;

            SyncBindingMask();
        }

        protected virtual void OnEnable()
        {
            m_Controls?.Enable();
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        protected virtual void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;

            // 비활성화 동안에는 해제 입력을 받지 못하므로 눌림 상태를 비운다. 입력 콜백 안에서 비활성화되면
            // Disable 이 canceled 를 바로 호출하는데, 상태를 먼저 비워 두면 그 경로에서도 아무 이벤트가 나가지 않는다.
            m_PressedPointers.Clear();
            m_Controls?.Disable();
        }

        private void OnActionCanceled(InputAction.CallbackContext context)
        {
            m_PressedPointers.Clear();
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change != InputDeviceChange.Removed && change != InputDeviceChange.Disconnected)
                return;

            // 분리된 장치의 포인터는 해제 입력이 오지 않으므로 눌림 상태에서 뺀다(드문 경로라 임시 리스트 할당을 허용)
            List<int> removedPointers = null;
            foreach (var pair in m_PressedPointers)
            {
                if (pair.Value == device)
                    (removedPointers ??= new List<int>()).Add(pair.Key);
            }

            if (removedPointers == null)
                return;

            foreach (var inputId in removedPointers)
                m_PressedPointers.Remove(inputId);
        }

        protected void OnAction(InputAction.CallbackContext context)
        {
            var control = context.control;
            var device = control.device;

            var isMouseInput = device is Mouse;
            var isPenInput = !isMouseInput && device is Pen;

            // 현재 포인터 값을 읽습니다.
            var drag = context.ReadValue<PointerInput>();
            if (isMouseInput)
                drag.InputId = PointerInputModule.kMouseLeftId;
            else if (isPenInput)
                drag.InputId = int.MinValue;

            var isPressed = m_PressedPointers.ContainsKey(drag.InputId);

            if (drag.Contact && !isPressed)
            {
                m_PressedPointers[drag.InputId] = device;
                Pressed?.Invoke(drag, context.time);
            }
            else if (drag.Contact)
            {
                Dragged?.Invoke(drag, context.time);
            }
            else if (isPressed)
            {
                // 그 포인터가 눌려 있을 때만 해제를 발행합니다. 누르지 않은 상태의 포인터 이동(마우스 호버 등)은 무시합니다.
                m_PressedPointers.Remove(drag.InputId);
                Released?.Invoke(drag, context.time);
            }
        }

        private void SyncBindingMask()
        {
            if (m_Controls == null)
                return;

            if (m_UseMouse && m_UsePen && m_UseTouch)
            {
                m_Controls.bindingMask = null;
                return;
            }

            m_Controls.bindingMask = InputBinding.MaskByGroups(m_UseMouse ? "Mouse" : null, m_UsePen ? "Pen" : null, m_UseTouch ? "Touch" : null);
        }

        private void OnValidate()
        {
            SyncBindingMask();
        }
    }
}
