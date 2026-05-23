using UnityEngine;

namespace MusicGame.Editor
{
    public struct ChartTimelineLayout
    {
        public Rect rect;
        public float rulerHeight;
        public float scrollTime;
        public float secondsVisible;
        public int laneCount;

        public float LaneAreaHeight => rect.height - rulerHeight;
        public float LaneHeight => laneCount > 0 ? LaneAreaHeight / laneCount : 0f;

        public float TimeToX(float time)
        {
            float t = (time - scrollTime) / secondsVisible;
            return rect.x + t * rect.width;
        }

        public float XToTime(float x)
        {
            float t = (x - rect.x) / rect.width;
            return scrollTime + t * secondsVisible;
        }

        public int YToLane(float y)
        {
            float localY = y - (rect.y + rulerHeight);
            if (LaneHeight <= 0f) return 0;
            int lane = Mathf.FloorToInt(localY / LaneHeight);
            return Mathf.Clamp(lane, 0, Mathf.Max(0, laneCount - 1));
        }

        public Rect GetLaneRect(int lane)
        {
            float y = rect.y + rulerHeight + lane * LaneHeight;
            return new Rect(rect.x, y, rect.width, LaneHeight);
        }

        public Rect GetNoteRect(float time, int lane, float durationSeconds = 0.08f)
        {
            float x = TimeToX(time);
            float w = Mathf.Max(6f, durationSeconds / secondsVisible * rect.width);
            var laneRect = GetLaneRect(lane);
            return new Rect(x - w * 0.5f, laneRect.y + 2f, w, laneRect.height - 4f);
        }

        public bool Contains(Vector2 guiPos) => rect.Contains(guiPos);
    }
}
