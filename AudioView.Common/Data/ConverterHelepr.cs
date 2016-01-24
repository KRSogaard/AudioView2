using System;
using System.Collections.Generic;

namespace AudioView.Common.Data
{
    public static class ConverterHelepr
    {
        public static Project ToInternal(this DataAccess.Project p, int readings)
        {
            return new Project()
            {
                Id = p.Id,
                MinorInterval = TimeSpan.FromTicks(p.MinorInterval),
                MajorInterval = TimeSpan.FromTicks(p.MajorInterval),
                MinorDBLimit = p.MinorDBLimit,
                MajorDBLimit = p.MajorDBLimit,
                Name = p.Name,
                Number = p.Number,
                Created = p.Created,
                Readings = readings
            };
        }

        public static Reading ToInternal(this DataAccess.Reading r)
        {
            return new Reading()
            {
                Id = r.Id,
                Time = r.Time,
                Major = r.Major == 1,
                LAeq = r.LAeq
            };
        }

        public static User ToInternal(this DataAccess.User u)
        {
            return new User()
            {
                Id = u.id,
                UserName = u.username
            };
        }

        public static DataAccess.User ToDatabase(this User u)
        {
            return new DataAccess.User()
            {
                id = u.Id,
                username = u.UserName
            };
        }
    }
}