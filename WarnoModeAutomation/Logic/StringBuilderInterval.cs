using System.Text;
using System.Timers;

namespace WarnoModeAutomation.Logic
{
    internal class StringBuilderInterval
    {
        public delegate Task TextUpdatedDelegate(string data);
        public event TextUpdatedDelegate TextUpdated;

        private readonly StringBuilder _stringBuilder;
        private readonly object _lock = new();
        private int _lastLength = 0;
        private System.Timers.Timer Timer { get; set; }

        public StringBuilderInterval(TimeSpan textUpdateInterval, int textBuilderCapacity)
        {
            _stringBuilder = new(textBuilderCapacity);
            SetupTimer(textUpdateInterval);
        }

        public void AddLine(string data) 
        {
            lock (_lock)
            {
                _stringBuilder.AppendLine(Environment.NewLine);
                _stringBuilder.AppendLine(data);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _stringBuilder.Clear();
            }
            TextUpdated?.Invoke(_stringBuilder.ToString());
        }

        private void SetupTimer(TimeSpan interval)
        {
            Timer = new System.Timers.Timer(interval);
            Timer.Elapsed += OnTimedEvent;
            Timer.AutoReset = true;
            Timer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                if (_lastLength != _stringBuilder.Length)
                {
                    TextUpdated?.Invoke(_stringBuilder.ToString());
                    _lastLength = _stringBuilder.Length;
                }
            }
        }
    }
}
