using UnityEngine;

namespace Input
{
    /// <summary>
    /// 스와이프 입력 정보를 담는 간단한 객체입니다.
    /// </summary>
    public struct SwipeInput
    {
        /// <summary>
        /// 이 스와이프를 수행한 입력의 ID입니다.
        /// </summary>
        public readonly int InputId;

        /// <summary>
        /// 스와이프가 시작된 위치입니다.
        /// </summary>
        public readonly Vector2 StartPosition;

        /// <summary>
        /// 이 스와이프의 이전 위치입니다.
        /// </summary>
        public readonly Vector2 PreviousPosition;

        /// <summary>
        /// 스와이프의 종료 위치입니다.
        /// </summary>
        public readonly Vector2 EndPosition;

        /// <summary>
        /// 스와이프의 평균 정규화 방향입니다.
        /// <c>(EndPosition - StartPosition).normalized</c>와 동일합니다.
        /// </summary>
        public readonly Vector2 SwipeDirection;

        /// <summary>
        /// 스와이프의 평균 속도입니다 (화면 단위/초).
        /// </summary>
        public readonly float SwipeVelocity;

        /// <summary>
        /// 스와이프가 화면 단위로 이동한 거리입니다. 항상 <see cref="StartPosition"/>과
        /// <see cref="EndPosition"/> 사이의 거리 이상이며, 직선이 아닌 경우 더 길어집니다.
        /// </summary>
        public readonly float TravelDistance;

        /// <summary>
        /// 스와이프의 지속 시간(초)입니다.
        /// </summary>
        public readonly double SwipeDuration;

        /// <summary>
        /// 이 스와이프의 방향 일관성을 나타내는 정규화된 측정값입니다.
        /// </summary>
        public readonly float SwipeSameness;

        /// <summary>
        /// 주어진 제스처로부터 새로운 스와이프 입력을 생성합니다.
        /// </summary>
        internal SwipeInput(ActiveGesture gesture) : this()
        {
            InputId = gesture.InputId;
            StartPosition = gesture.StartPosition;
            PreviousPosition = gesture.PreviousPosition;
            EndPosition = gesture.EndPosition;
            SwipeDirection = (EndPosition - StartPosition).normalized;
            SwipeDuration = gesture.EndTime - gesture.StartTime;
            TravelDistance = gesture.TravelDistance;
            SwipeSameness = gesture.SwipeDirectionSameness;

            if (SwipeDuration > 0.0f)
            {
                SwipeVelocity = (float)(TravelDistance / SwipeDuration);
            }
        }
    }
}
