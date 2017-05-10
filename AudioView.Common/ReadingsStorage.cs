using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioView.Common.Data;

namespace AudioView.Common
{
    public class ReadingsStorage
    {
        private TimeSpan maxAge;
        private LinkedList<Tuple<DateTime, ReadingData>> history;

        public ReadingsStorage(TimeSpan maxAge)
        {
            this.maxAge = maxAge;
            history = new LinkedList<Tuple<DateTime, ReadingData>>();
        }


        public void Add(DateTime time, ReadingData data)
        {
            history.AddLast(new Tuple<DateTime, ReadingData>(time, data));

            if (history.Count > 0 && history.First.Value.Item1 < time - maxAge)
            {
                history.RemoveFirst();
            }
        }

        public Tuple<DateTime, ReadingData> GetLatests()
        {
            if (history.Count == 0)
            {
                return null;
            }
            return history.Last.Value;
        }

        public void Each(Action<Tuple<DateTime, ReadingData>> action)
        {
            foreach (var r in history)
            {
                action(r);
            }
        }

        public LinkedList<ReadingData> GetSince(DateTime since)
        {
            LinkedList<ReadingData> datas = new LinkedList<ReadingData>();
            if (history.Count == 0)
            {
                return datas;
            }

            LinkedListNode<Tuple<DateTime, ReadingData>> current = history.Last;
            while (current != null)
            {
                if (current.Value.Item1 < since)
                {
                    break;
                }

                datas.AddLast(current.Value.Item2);

                current = current.Previous;
            }

            return datas;
        }
    }
}
