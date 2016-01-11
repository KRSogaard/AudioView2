using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudioView.Common.Data;

namespace AudioView.Common.Services
{
    public interface IDatabaseService
    {
        Task<IList<Project>> SearchProjects(string name, DateTime? leftTime, DateTime? rightTime);
    }
}