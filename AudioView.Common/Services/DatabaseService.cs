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
using Reading = AudioView.Common.Data.Reading;

namespace AudioView.Common.Services
{
    public class DatabaseService : IDatabaseService
    {
        public async Task<IList<Project>> SearchProjects(string name, DateTime? leftTime, DateTime? rightTime)
        {
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

                return (await request.Select(x => new
                {
                    Project = x,
                    Readings = x.Readings.Count
                })
                .OrderByDescending(x=>x.Project.Created)
                .ToListAsync().ConfigureAwait(false))
                .Select(x=>x.Project.ToInternal(x.Readings)).ToList();
            }
            return projects;
        }

        public async Task<IList<Reading>> GetReading(Guid projectId)
        {
            IList<Reading> readings = new List<Reading>();
            using (var audioViewEntities = new AudioViewEntities())
            {
                var results = (await audioViewEntities.Readings
                                .Where(x => x.Project == projectId)
                                .OrderByDescending(x=>x.Time)
                                .ToListAsync().ConfigureAwait(false))
                                .Select(x=>x.ToInternal());
                foreach (var reading in results)
                {
                    readings.Add(reading);
                }
            }
            return readings;
        }

        public async Task DeleteProject(Guid projectId)
        {
            using (var audioViewEntities = new AudioViewEntities())
            {
                var project = await audioViewEntities.Projects.Where(x => x.Id == projectId).FirstOrDefaultAsync().ConfigureAwait(false);
                if (project == null)
                    return;
                audioViewEntities.Projects.Remove(project);
                audioViewEntities.SaveChanges();
            }
        }

        public async Task DeleteReading(Guid readingId)
        {
            using (var audioViewEntities = new AudioViewEntities())
            {
                var reading = await audioViewEntities.Readings.Where(x => x.Id == readingId).FirstOrDefaultAsync().ConfigureAwait(false);
                if (reading == null)
                    return;
                audioViewEntities.Readings.Remove(reading);
                audioViewEntities.SaveChanges();
            }
        }

        public async Task<Project> GetProject(Guid id)
        {
            using (var audioViewEntities = new AudioViewEntities())
            {
                var project = await audioViewEntities.Projects.Where(x => x.Id == id).Select(x => new
                {
                    Project = x,
                    Readings = x.Readings.Count
                }).FirstOrDefaultAsync().ConfigureAwait(false);

                if (project == null)
                    return null;

                return project.Project.ToInternal(project.Readings);
            }
        }
    }
}
