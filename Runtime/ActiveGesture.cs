using UnityEngine;

namespace Input
{
    /// <summary>
    /// 주어진 입력에 대해 진행 중인 잠재적 제스처입니다.
    /// </summary>
    internal sealed class ActiveGesture
    {
        /// <summary>
        /// 이 제스처를 생성한 입력 ID입니다.
        /// </summary>
        public int InputId;

        /// <summary>
        /// 이 잠재적 제스처가 시작된 시간입니다.
        /// </summary>
        public readonly double StartTime;

        /// <summary>
        /// 이 잠재적 제스처가 종료된 시간입니다.
        /// </summary>
        public double EndTime;

        /// <summary>
        /// 이 제스처가 시작된 위치입니다.
        /// </summary>
        public readonly Vector2 StartPosition;

        /// <summary>
        /// 마지막 샘플 시점에서 이 제스처의 위치입니다.
        /// </summary>
        public Vector2 PreviousPosition;

        /// <summary>
        /// 이 제스처가 종료된 위치입니다.
        /// </summary>
        public Vector2 EndPosition;

        /// <summary>
        /// 이 제스처에 대한 샘플 수입니다.
        /// </summary>
        public int Samples;

        /// <summary>
        /// 스와이프 방향의 일관성입니다. 직선에 가까울수록 1에 수렴합니다.
        /// </summary>
        /// <remarks>
        /// 모든 선분(정규화)의 내적을 시작점에서 스와이프 끝까지의 정규화된
        /// 벡터에 대해 계산한 평균값입니다.
        /// </remarks>
        public float SwipeDirectionSameness;

        /// <summary>
        /// 이 제스처의 화면 단위 총 이동 거리입니다. 항상 <see cref="StartPosition"/>과
        /// <see cref="EndPosition"/> 사이의 거리 이상이며, 직선이 아닌 제스처의 경우
        /// 더 길어질 수 있습니다.
        /// </summary>
        public float TravelDistance;

        /// <summary>
        /// 모든 정규화된 이동 벡터의 누적 합입니다.
        /// </summary>
        private Vector2 accumulatedNormalized;

        /// <summary>
        /// 새로운 잠재적 제스처를 생성합니다.
        /// </summary>
        /// <param name="inputId">이 제스처의 입력 ID입니다.</param>
        /// <param name="startPosition">제스처의 시작 위치입니다.</param>
        /// <param name="startTime">제스처가 시작된 시간입니다.</param>
        public ActiveGesture(int inputId, Vector2 startPosition, double startTime)
        {
            InputId = inputId;
            EndTime = StartTime = startTime;
            EndPosition = StartPosition = startPosition;
            Samples = 1;
            SwipeDirectionSameness = 1;
            accumulatedNormalized = Vector2.zero;
        }

        /// <summary>
        /// 이 제스처에 새로운 위치를 제출합니다.
        /// </summary>
        /// <param name="position">새 샘플의 위치입니다.</param>
        /// <param name="time">새 샘플의 시간입니다.</param>
        public void SubmitPoint(Vector2 position, double time)
        {
            Vector2 toNewPosition = position - EndPosition;
            float distanceMoved = toNewPosition.magnitude;

            // 새로운 종료 시간을 설정합니다
            EndTime = time;

            if (Mathf.Approximately(distanceMoved, 0))
            {
                // 이전 위치와 동일한 지점은 건너뜁니다
                return;
            }

            // 정규화
            toNewPosition /= distanceMoved;

            Samples++;
            Vector2 toNewEndPosition = (position - StartPosition).normalized;

            // 새로운 종료 위치와 이전 위치를 설정합니다
            PreviousPosition = EndPosition;
            EndPosition = position;

            accumulatedNormalized += toNewPosition;

            SwipeDirectionSameness = Vector2.Dot(toNewEndPosition, accumulatedNormalized / (Samples - 1));

            TravelDistance += distanceMoved;
        }
    }
}
