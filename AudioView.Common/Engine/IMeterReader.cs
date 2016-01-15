using System.Threading.Tasks;
using AudioView.Common.Data;

namespace AudioView.Common.Engine
{
    public interface IMeterReader
    {
        Task<ReadingData> GetSecondReading();
        Task<ReadingData> GetMinorReading();
        Task<ReadingData> GetMajorReading();
    }
}