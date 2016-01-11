using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioView.Common.Data;
using AudioView.Common.DataAccess;
using Project = AudioView.Common.Data.Project;

namespace AudioView.Common.Services
{
    public class DatabaseService : IDatabaseService
    {
        public Task<IList<Project>> SearchProjects(string name, DateTime? leftTime, DateTime? rightTime)
        {
            return Task.Factory.StartNew(() => { 
                IList < Project > projects = new List<Project>();
                using (var audioViewEntities = new AudioViewEntities())
                {
                    var request = audioViewEntities.Projects.Where(x=>true);

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        request = request.Where(x => x.Name.ToLower().Contains(name.Trim().ToLower()));
                    }
                    if (leftTime != null)
                    {
                        request = request.Where(x => x.Created >= leftTime.Value);
                    }
                    if (rightTime != null)
                    {
                        request = request.Where(x => x.Created <= rightTime.Value);
                    }
                    request = request.Take(50);

                    var result = request.Select(x => new
                    {
                        Project = x,
                        Readings = x.Readings.Count
                    }) .ToListAsync().Result;
                    foreach (var project in result)
                    {
                        projects.Add(project.Project.ToInternal(project.Readings));
                    }
                }
                return projects;
            });
        }
    }
}
