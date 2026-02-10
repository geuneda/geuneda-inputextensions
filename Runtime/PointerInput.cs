using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

namespace Input
{
    /// <summary>
    /// 드래그 입력 정보를 담는 간단한 객체입니다.
    /// </summary>
    public struct PointerInput
    {
        public bool Contact;

        /// <summary>
        /// 입력 유형의 ID입니다.
        /// </summary>
        public int InputId;

        /// <summary>
        /// 드로우 입력의 위치입니다.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// 드로우 입력 펜의 기울기입니다.
        /// </summary>
        public Vector2? Tilt;

        /// <summary>
        /// 드로우 입력의 압력입니다.
        /// </summary>
        public float? Pressure;

        /// <summary>
        /// 드로우 입력의 반경입니다.
        /// </summary>
        public Vector2? Radius;

        /// <summary>
        /// 드로우 입력의 비틀림입니다.
        /// </summary>
        public float? Twist;
    }

    // PointerInputManager에서는 PointerInput에 필요한 각 입력에 대해 별도의 액션을 생성합니다.
    // 여기서는 컴포지트를 사용하여 모든 입력을 단일 값으로 제공하는 대안을 보여줍니다.
    // 장단점이 있으며, 가장 큰 장점은 모든 컨트롤이 함께 작동하여 하나의 입력 값을 전달한다는 것입니다.
    //
    // 참고: PointerControls에서 마우스와 펜은 터치와 별도로 바인딩됩니다. 멀티터치가 필요 없다면
    //       `<Pointer>/position` 등으로 바인딩할 수 있습니다. 하지만 각 터치를 별도의
    //       PointerInput 소스로 제공하려면 여러 개의 PointerInputComposite가 필요합니다.
    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    #endif
    public class PointerInputComposite : InputBindingComposite<PointerInput>
    {
        [InputControl(layout = "Button")]
        public int contact;

        [InputControl(layout = "Vector2")]
        public int position;

        [InputControl(layout = "Vector2")]
        public int tilt;

        [InputControl(layout = "Vector2")]
        public int radius;

        [InputControl(layout = "Axis")]
        public int pressure;

        [InputControl(layout = "Axis")]
        public int twist;

        [InputControl(layout = "Integer")]
        public int inputId;

        public override PointerInput ReadValue(ref InputBindingCompositeContext context)
        {
            var contact = context.ReadValueAsButton(this.contact);
            var pointerId = context.ReadValue<int>(inputId);
            var pressure = context.ReadValue<float>(this.pressure);
            var radius = context.ReadValue<Vector2, Vector2MagnitudeComparer>(this.radius);
            var tilt = context.ReadValue<Vector2, Vector2MagnitudeComparer>(this.tilt);
            var position = context.ReadValue<Vector2, Vector2MagnitudeComparer>(this.position);
            var twist = context.ReadValue<float>(this.twist);

            return new PointerInput
            {
                Contact = contact,
                InputId = pointerId,
                Position = position,
                Tilt = tilt != default ? tilt : (Vector2?)null,
                Pressure = pressure > 0 ? pressure : (float?)null,
                Radius = radius.sqrMagnitude > 0 ? radius : (Vector2?)null,
                Twist = twist > 0 ? twist : (float?)null,
            };
        }

        #if UNITY_EDITOR
        static PointerInputComposite()
        {
            Register();
        }

        #endif

        [RuntimeInitializeOnLoadMethod]
        private static void Register()
        {
            InputSystem.RegisterBindingComposite<PointerInputComposite>();
        }
    }
}
