using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudioView.Common.Data;

namespace AudioView.Common.Services
{
    public interface IDatabaseService
    {
        Task<IList<Project>> SearchProjects(string name, string number, DateTime? leftTime, DateTime? rightTime);
        Task<IList<Reading>> GetReading(Guid projectId);
        Task DeleteProject(Guid ProjectId);
        Task DeleteReading(Guid readingId);
        Task<Project> GetProject(Guid id);
    }
}