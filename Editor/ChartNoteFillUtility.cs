using System.Collections.Generic;
using MusicGame.Chart;
using UnityEngine;

namespace MusicGame.Editor
{
    public enum ChartFillLanePattern
    {
        SingleLane,
        RotateLanes,
        AllLanesEachStep
    }

    public static class ChartNoteFillUtility
    {
        public static int FillRange(
            SongChart chart,
            float startTime,
            float endTime,
            float stepSeconds,
            int singleLane,
            ChartFillLanePattern pattern)
        {
            if (chart == null || chart.notes == null || stepSeconds <= 0f) return 0;

            startTime = Mathf.Max(0f, startTime);
            endTime = Mathf.Max(startTime, endTime);

            float grid = stepSeconds * 0.25f;
            int added = 0;
            int alt = 0;

            for (float t = startTime; t <= endTime + stepSeconds * 0.001f; t += stepSeconds)
            {
                switch (pattern)
                {
                    case ChartFillLanePattern.SingleLane:
                        if (TryAdd(chart, singleLane, t, grid)) added++;
                        break;
                    case ChartFillLanePattern.RotateLanes:
                        if (TryAdd(chart, alt % chart.laneCount, t, grid)) added++;
                        alt++;
                        break;
                    case ChartFillLanePattern.AllLanesEachStep:
                        for (int lane = 0; lane < chart.laneCount; lane++)
                        {
                            if (TryAdd(chart, lane, t, grid)) added++;
                        }
                        break;
                }
            }

            chart.SortNotesByTime();
            return added;
        }

        static bool TryAdd(SongChart chart, int lane, float time, float threshold)
        {
            lane = Mathf.Clamp(lane, 0, chart.laneCount - 1);
            foreach (var n in chart.notes)
            {
                if (n.lane == lane && Mathf.Abs(n.time - time) < threshold)
                    return false;
            }

            chart.notes.Add(new NoteEventData(lane, time, NoteType.Tap, 0f));
            return true;
        }
    }
}
