using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioView.Common;
using AudioView.Common.Data;
using Prism.Mvvm;
using V = System.Tuple<System.DateTime, double>;

namespace AudioView.ViewModels
{
    public class GraphViewModel : BindableBase
    {
        private string displayValue;
        private bool displayBars;
        private bool displayLine;
        private TimeSpan span;
        private ReadingsStorage lineHistory;
        private ReadingsStorage barHistory;
        private ObservableCollection<V> barValues;
        private ObservableCollection<V> lineValues;
        private int sizeToClearLine = 0;
        private int sizeToClearBars = 0;
        private double limitOffset;

        public GraphViewModel(string displayValue, ReadingsStorage lineHistory, ReadingsStorage barHistory,
            TimeSpan span,
            bool displayLine = true,
            bool displayBars = true)
        {
            this.displayValue = displayValue;
            this.lineHistory = lineHistory;
            this.barHistory = barHistory;
            this.displayBars = displayBars;
            this.displayLine = displayLine;
            this.span = span;

            barValues = new ObservableCollection<V>();
            lineValues = new ObservableCollection<V>();
        }

        public string DisplayValue => displayValue;

        public ObservableCollection<V> BarValues
        {
            get { return barValues; }
            set { SetProperty(ref barValues, value); }
        }
        public ObservableCollection<V> LineValues
        {
            get { return lineValues; }
            set { SetProperty(ref lineValues, value); }
        }
        public double LimitOffset
        {
            get { return limitOffset; }
            set { SetProperty(ref limitOffset, value); }
        }

        public void OnLineReading()
        {
            if (!displayLine || lineHistory == null)
                return;
            AddReading(lineHistory, lineValues);

            if (sizeToClearLine == 0 && lineValues[0].Item1 < DateTime.Now.Subtract(span))
            {
                // Add 3 to give a little margin
                sizeToClearLine = (lineValues.Count + 3) * 2;
            }
            if (sizeToClearLine != 0 && lineValues.Count >= sizeToClearLine)
            {
                var newList = new List<V>();
                for (int i = sizeToClearLine / 2; i < lineValues.Count; i++)
                {
                    newList.Add(lineValues[i]);
                }
                lineValues.Clear();
                lineValues.AddRange(newList);
            }
        }

        public void OnBarReading()
        {
            if (!displayBars || barValues == null)
                return;
            AddReading(barHistory, barValues);

            if (sizeToClearBars == 0 && barValues[0].Item1 < DateTime.Now.Subtract(span))
            {
                // Add 3 to give a little margin
                sizeToClearBars = (barValues.Count + 4) * 2;
            }
            if (sizeToClearBars != 0 && barValues.Count >= sizeToClearBars)
            {
                var newList = new List<V>();
                for (int i = sizeToClearBars / 2; i < barValues.Count; i++)
                {
                    newList.Add(barValues[i]);
                }
                barValues.Clear();
                barValues.AddRange(newList);
            }
        }

        private void AddReading(ReadingsStorage storage, ObservableCollection<V> list)
        {
            var latest = storage.GetLatests();
            list.Add(new V(latest.Item1, latest.Item2.GetValue(displayValue)));
        }

        public void ChangeDisplayItem(string displayItem, double limitOffset)
        {
            this.displayValue = displayItem;
            LimitOffset = limitOffset;

            if (displayBars)
            {
                ChangeDisplayItem(barHistory, barValues);
            }
            if (displayLine)
            {
                ChangeDisplayItem(lineHistory, lineValues);
            }
        }

        private void ChangeDisplayItem(ReadingsStorage storage, ObservableCollection<V> list)
        {
            List<V> newReadings = new List<V>();
            storage.Each(r =>
            {
                newReadings.Add(new V(r.Item1, r.Item2.GetValue(displayValue)));
            });
            list.Clear();
            list.AddRange(newReadings);
        }
    }
}
