using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Input
{
    /// <summary>
    /// <see cref="PointerInputManager"/>로부터 포인터 입력을 받아
    /// 방향성 스와이프와 탭을 감지하는 컨트롤러입니다.
    /// </summary>
    public class GestureController : MonoBehaviour
    {
        [SerializeField]
        private PointerInputManager inputManager;

        // 탭으로 간주되기 위한 누르기의 최대 지속 시간입니다.
        [SerializeField]
        private float maxTapDuration = 0.2f;

        // 탭으로 간주되기 위해 원래 위치에서 벗어날 수 있는 최대 거리(화면 단위)입니다.
        [SerializeField]
        private float maxTapDrift = 5.0f;

        // 유효한 스와이프로 간주되기 위한 최대 지속 시간입니다.
        [SerializeField]
        private float maxSwipeDuration = 0.5f;

        // 스와이프로 간주되기 위해 이동해야 하는 최소 거리(화면 단위)입니다.
        // 이 값이 maxTapDrift보다 작거나 같으면 사용자 동작이
        // 스와이프와 탭 모두로 반환될 수 있습니다.
        [SerializeField]
        private float minSwipeDistance = 10.0f;

        // 스와이프로 간주되기 위해 필요한 방향 일관성 임계값입니다.
        [SerializeField]
        private float swipeDirectionSamenessThreshold = 0.6f;

        [Header("Debug"), SerializeField]
        private Text label;

        // 입력 ID를 활성 제스처 추적 객체에 매핑합니다.
        private readonly Dictionary<int, ActiveGesture> activeGestures = new Dictionary<int, ActiveGesture>();

        /// <summary>
        /// 사용자가 화면을 눌렀을 때 발생하는 이벤트입니다.
        /// </summary>
        public new event Action<SwipeInput> Pressed;

        /// <summary>
        /// 잠재적 스와이프 제스처의 모든 움직임(프레임당 여러 번 가능)마다 발생하는 이벤트입니다.
        /// </summary>
        public event Action<SwipeInput> PotentiallySwiped;

        /// <summary>
        /// 사용자가 스와이프 제스처를 수행했을 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<SwipeInput> Swiped;

        /// <summary>
        /// 사용자가 탭 제스처를 수행하고 손을 뗄 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<TapInput> Tapped;

        protected virtual void Awake()
        {
            inputManager.Pressed += OnPressed;
            inputManager.Dragged += OnDragged;
            inputManager.Released += OnReleased;
        }

        /// <summary>
        /// 주어진 활성 제스처가 유효한 스와이프인지 확인합니다.
        /// </summary>
        private bool IsValidSwipe(ref ActiveGesture gesture)
        {
            return gesture.TravelDistance >= minSwipeDistance &&
                (gesture.StartTime - gesture.EndTime) <= maxSwipeDuration &&
                gesture.SwipeDirectionSameness >= swipeDirectionSamenessThreshold;
        }

        /// <summary>
        /// 주어진 활성 제스처가 유효한 탭인지 확인합니다.
        /// </summary>
        private bool IsValidTap(ref ActiveGesture gesture)
        {
            return gesture.TravelDistance <= maxTapDrift &&
                (gesture.StartTime - gesture.EndTime) <= maxTapDuration;
        }

        private void OnPressed(PointerInput input, double time)
        {
            Debug.Assert(!activeGestures.ContainsKey(input.InputId));

            var newGesture = new ActiveGesture(input.InputId, input.Position, time);
            activeGestures.Add(input.InputId, newGesture);

            DebugInfo(newGesture);

            Pressed?.Invoke(new SwipeInput(newGesture));
        }

        private void OnDragged(PointerInput input, double time)
        {
            if (!activeGestures.TryGetValue(input.InputId, out var existingGesture))
            {
                // UI에 의해 캡처되었거나 입력이 유실되었을 수 있습니다
                return;
            }

            existingGesture.SubmitPoint(input.Position, time);

            if (IsValidSwipe(ref existingGesture))
            {
                PotentiallySwiped?.Invoke(new SwipeInput(existingGesture));
            }

            DebugInfo(existingGesture);
        }

        private void OnReleased(PointerInput input, double time)
        {
            if (!activeGestures.TryGetValue(input.InputId, out var existingGesture))
            {
                // UI에 의해 캡처되었거나 입력이 유실되었을 수 있습니다
                return;
            }

            activeGestures.Remove(input.InputId);
            existingGesture.SubmitPoint(input.Position, time);

            if (IsValidSwipe(ref existingGesture))
            {
                Swiped?.Invoke(new SwipeInput(existingGesture));
            }

            if (IsValidTap(ref existingGesture))
            {
                Tapped?.Invoke(new TapInput(existingGesture));
            }

            DebugInfo(existingGesture);
        }

        private void DebugInfo(ActiveGesture gesture)
        {
            if (label == null) return;

            var builder = new StringBuilder();

            builder.AppendFormat("ID: {0}", gesture.InputId);
            builder.AppendLine();
            builder.AppendFormat("Start Position: {0}", gesture.StartPosition);
            builder.AppendLine();
            builder.AppendFormat("Position: {0}", gesture.EndPosition);
            builder.AppendLine();
            builder.AppendFormat("Duration: {0}", gesture.EndTime - gesture.StartTime);
            builder.AppendLine();
            builder.AppendFormat("Sameness: {0}", gesture.SwipeDirectionSameness);
            builder.AppendLine();
            builder.AppendFormat("Travel distance: {0}", gesture.TravelDistance);
            builder.AppendLine();
            builder.AppendFormat("Samples: {0}", gesture.Samples);
            builder.AppendLine();
            builder.AppendFormat("Realtime since startup: {0}", Time.realtimeSinceStartup);
            builder.AppendLine();
            builder.AppendFormat("Starting Timestamp: {0}", gesture.StartTime);
            builder.AppendLine();
            builder.AppendFormat("Ending Timestamp: {0}", gesture.EndTime);
            builder.AppendLine();

            label.text = builder.ToString();

            var worldStart = Camera.main.ScreenToWorldPoint(gesture.StartPosition);
            var worldEnd = Camera.main.ScreenToWorldPoint(gesture.EndPosition);

            worldStart.z += 5;
            worldEnd.z += 5;
        }
    }
}
