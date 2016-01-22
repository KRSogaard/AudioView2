using System;
using System.Threading.Tasks;
using AudioView.Common.Data;

namespace AudioView.Common.Engine
{
    public delegate void ConnectionStatusUpdateDeligate(bool connected);

    public interface IMeterReader
    {
        Task<ReadingData> GetSecondReading();
        Task<ReadingData> GetMinorReading();
        Task<ReadingData> GetMajorReading();

        void SetMinorInterval(TimeSpan interval);
        void SetMajorInterval(TimeSpan interval);

        void SetEngine(AudioViewEngine engine);
        bool IsTriggerMode();
        event ConnectionStatusUpdateDeligate ConnectionStatusEvent;
        Task Close();
    }
}