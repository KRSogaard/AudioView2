using System;
using System.Threading.Tasks;
using AudioView.Common.Data;

namespace AudioView.Common.Engine
{
    public interface IMeterListener
    {
        Task OnMinor(DateTime time, DateTime starTime, ReadingData data);
        Task OnMajor(DateTime time, DateTime starTime, ReadingData data);
        Task OnSecond(DateTime time, DateTime starTime, ReadingData data, ReadingData minorData, ReadingData majorData);
        Task NextMinor(DateTime time);
        Task NextMajor(DateTime time);
    }
}