using System;

namespace Azw.FacialOsc.Tracking
{
    public class AverageFps
    {
        private nint capacity = 4;
        public double averageFps { get; private set; }
        public double averageInterval { get; private set; }
        double[] intervalSecond;
        public nint count { get; private set; }
        nint tail = 0;

        public AverageFps()
        {
            intervalSecond = new double[capacity];
        }

        public TimeSpan Add(TimeSpan interval)
        {
            intervalSecond[tail] = interval.TotalSeconds;

            if (count < capacity) count++;
            tail++;
            if (capacity <= tail) tail -= capacity;

            var total = 0d;
            for (nint i = 0; i < count; i++)
            {
                total += intervalSecond[i];
            }

            averageInterval = total / count;
            averageFps = 1 / averageInterval;

            var newCapacity = CulcNewCapacity(averageFps);
            if (capacity != newCapacity)
            {
                var old = intervalSecond;
                intervalSecond = new double[newCapacity];
                Array.Copy(old, intervalSecond, Math.Min(capacity, newCapacity));
                capacity = newCapacity;
                if (capacity < count) count = capacity;
                if (count <= tail) tail = count;
                if (capacity <= tail) tail--;
            }

            return interval;
        }
        public TimeSpan Temporary(TimeSpan interval)
        {
            intervalSecond[tail] = interval.TotalSeconds;

            var tempCount = count;
            if (tempCount < capacity) tempCount++;

            var total = 0d;
            for (nint i = 0; i < tempCount; i++)
            {
                total += intervalSecond[i];
            }

            averageInterval = total / count;
            averageFps = 1 / averageInterval;

            return interval;
        }

        internal static nint CulcNewCapacity(double fps)
        {
            return fps switch
            {
                > 45 => 60,
                > 20 => 30,
                > 10 => 15,
                > 4 => 8,
                _ => 2,
            };
        }
    }
}
