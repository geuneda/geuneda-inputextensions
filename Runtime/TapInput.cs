using UnityEngine;

namespace Input
{
    /// <summary>
    /// 탭 입력 정보를 담는 간단한 객체입니다.
    /// </summary>
    public struct TapInput
    {
        /// <summary>
        /// 탭이 시작된 위치입니다.
        /// </summary>
        public readonly Vector2 PressPosition;

        /// <summary>
        /// 탭이 해제된 위치입니다.
        /// </summary>
        public readonly Vector2 ReleasePosition;

        /// <summary>
        /// 탭이 유지된 시간입니다.
        /// </summary>
        public readonly double TapDuration;

        /// <summary>
        /// 탭의 총 이탈 거리(화면 단위)입니다.
        /// </summary>
        public readonly float TapDrift;

        /// <summary>
        /// 탭의 타임스탬프입니다.
        /// </summary>
        public readonly double TimeStamp;

        internal TapInput(ActiveGesture gesture) : this()
        {
            PressPosition = gesture.StartPosition;
            ReleasePosition = gesture.EndPosition;
            TapDuration = gesture.EndTime - gesture.StartTime;
            TapDrift = gesture.TravelDistance;
            TimeStamp = gesture.EndTime;
        }
    }
}
