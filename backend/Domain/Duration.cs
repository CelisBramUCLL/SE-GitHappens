namespace Dotnet_test.Domain
{
    public readonly struct Duration
    {
        public int Minutes { get; }
        public int Seconds { get; }

        public Duration(int minutes, int seconds)
        {
            if (seconds >= 60)
            {
                Minutes = minutes + (seconds / 60);
                Seconds = seconds % 60;
            }
            else
            {
                Minutes = minutes;
                Seconds = seconds;
            }
        }

        public Duration(TimeSpan timeSpan)
        {
            Minutes = (int)timeSpan.TotalMinutes;
            Seconds = timeSpan.Seconds;
        }

        public TimeSpan ToTimeSpan() => new TimeSpan(0, Minutes, Seconds);

        public int TotalSeconds => (Minutes * 60) + Seconds;

        public override string ToString() => $"{Minutes}:{Seconds:D2}";

        public static Duration FromSeconds(int totalSeconds)
        {
            return new Duration(totalSeconds / 60, totalSeconds % 60);
        }

        public static Duration Parse(string durationString)
        {
            var parts = durationString.Split(':');
            if (
                parts.Length == 2
                && int.TryParse(parts[0], out int minutes)
                && int.TryParse(parts[1], out int seconds)
            )
            {
                return new Duration(minutes, seconds);
            }
            throw new FormatException("Duration must be in format 'mm:ss'");
        }

        // Operators
        public static bool operator ==(Duration left, Duration right) =>
            left.Minutes == right.Minutes && left.Seconds == right.Seconds;

        public static bool operator !=(Duration left, Duration right) => !(left == right);

        public static bool operator >(Duration left, Duration right) =>
            left.TotalSeconds > right.TotalSeconds;

        public static bool operator <(Duration left, Duration right) =>
            left.TotalSeconds < right.TotalSeconds;

        public override bool Equals(object? obj) => obj is Duration other && this == other;

        public override int GetHashCode() => HashCode.Combine(Minutes, Seconds);
    }
}
