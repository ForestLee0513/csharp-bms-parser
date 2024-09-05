namespace BMS
{
    public class Layer
    {
        public readonly Event @event;
        public readonly Sequence[][] sequence;

        public Layer(Event @event, Sequence[][] sequence)
        {
            this.@event = @event;
            this.sequence = sequence;
        }

        public class Event
        {
            public readonly EventType type;
            public readonly int interval;

            public Event(EventType type, int interval)
            {
                this.type = type;
                this.interval = interval;
            }
        }

        public enum EventType
        {
            ALWAYS,
            PLAY,
            MISS
        }

        public class Sequence
        {
            public static readonly int END = int.MinValue;
            public readonly long time;
            public readonly int id;
            
            public Sequence(long time)
            {
                this.time = time;
                id = END;
            }

            public Sequence(long time, int id)
            {
                this.time = time;
                this.id = id;
            }

            public bool isEnd()
            {
                return id == END;
            }
        }
    }
}
