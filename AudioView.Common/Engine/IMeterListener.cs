using System;
using System.Threading.Tasks;

namespace AudioView.Common.Engine
{
    public interface IMeterListener
    {
        Task OnMinor(DateTime time, ReadingData data);
        Task OnMajor(DateTime time, ReadingData data);
        Task OnSecond(DateTime time, ReadingData data);
        Task NextMinor(DateTime time);
        Task NextMajor(DateTime time);
    }
}