using System.Threading.Tasks;

namespace AudioView.Common.Engine
{
    public interface IMeterReader
    {
        Task<ReadingData> GetSecondReading();
        Task<ReadingData> GetMinorReading();
        Task<ReadingData> GetMajorReading();
    }
}