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
                MinorInterval = p.MinorInterval,
                MajorInterval = p.MajorInterval,
                DBLimit = p.DBLimit,
                Name = p.Name,
                Created = p.Created,
                Readings = readings
            };
        }
    }
}