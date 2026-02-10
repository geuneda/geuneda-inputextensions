using System;
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
    /// - <see cref="PointerControls.PointerActions.point"/>에서 <see cref="InputAction.passThrough"/>가
    ///   활성화되어 있습니다. 이는 하나의 액션을 통해 임의의 여러 포인터 입력을 받기 위함입니다.
    ///   패스스루 없이는 액션의 기본 충돌 해결이 작동하여 한 번에 하나의
    ///   컴포지트 바인딩만 통과시킵니다.
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

        private bool m_Dragging;
        private PointerControls m_Controls;

        // 디버깅에 유용하며, 특히 터치 시뮬레이션이 켜져 있을 때 사용합니다.
        [SerializeField] private bool m_UseMouse;
        [SerializeField] private bool m_UsePen;
        [SerializeField] private bool m_UseTouch;

        protected virtual void Awake()
        {
            m_Controls = new PointerControls();

            m_Controls.pointer.point.performed += OnAction;
            // 다양한 입력에 바인딩되어 있으므로 실제로 취소될 가능성은 낮지만,
            // 전체가 리셋되는 경우를 대비하여 연결해 둡니다.
            m_Controls.pointer.point.canceled += OnAction;

            SyncBindingMask();
        }

        protected virtual void OnEnable()
        {
            m_Controls?.Enable();
        }

        protected virtual void OnDisable()
        {
            m_Controls?.Disable();
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

            if (drag.Contact && !m_Dragging)
            {
                Pressed?.Invoke(drag, context.time);
                m_Dragging = true;
            }
            else if (drag.Contact && m_Dragging)
            {
                Dragged?.Invoke(drag, context.time);
            }
            else
            {
                Released?.Invoke(drag, context.time);
                m_Dragging = false;
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
