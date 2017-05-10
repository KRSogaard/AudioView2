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
                Data = new ReadingData()
                {
                    LAeq = r.LAeq,
                    LAMax = r.LAMax,
                    LAMin = r.LAMin,
                    LZMax = r.LZMax,
                    LZMin = r.LZMin,
                    LAeqOctaveBandOneThird = new ReadingData.OctaveBandOneThird()
                    {
                        Hz6_3 = r.Hz6_3,
                        Hz8 = r.Hz8,
                        Hz10 = r.Hz10,
                        Hz12_5 = r.Hz12_5,
                        Hz16 = r.Hz16,
                        Hz20 = r.Hz20,
                        Hz25 = r.Hz25,
                        Hz31_5 = r.Hz31_5,
                        Hz40 = r.Hz40,
                        Hz50 = r.Hz50,
                        Hz63 = r.Hz63,
                        Hz80 = r.Hz80,
                        Hz100 = r.Hz100,
                        Hz125 = r.Hz125,
                        Hz160 = r.Hz160,
                        Hz200 = r.Hz200,
                        Hz250 = r.Hz250,
                        Hz315 = r.Hz315,
                        Hz400 = r.Hz400,
                        Hz500 = r.Hz500,
                        Hz630 = r.Hz630,
                        Hz800 = r.Hz800,
                        Hz1000 = r.Hz1000,
                        Hz1250 = r.Hz1250,
                        Hz1600 = r.Hz1600,
                        Hz2000 = r.Hz2000,
                        Hz2500 = r.Hz2500,
                        Hz3150 = r.Hz3150,
                        Hz4000 = r.Hz4000,
                        Hz5000 = r.Hz5000,
                        Hz6300 = r.Hz6300,
                        Hz8000 = r.Hz8000,
                        Hz10000 = r.Hz10000,
                        Hz12500 = r.Hz12500,
                        Hz16000 = r.Hz16000,
                        Hz20000 = r.Hz20000
                    }
                }
            };
        }

        public static Reading ToInternal(this Tuple<DateTime, ReadingData> r, bool major)
        {
            return new Reading()
            {
                Id = Guid.Empty,
                Time = r.Item1,
                Major = major,
                Data = r.Item2
            };
        }

        public static User ToInternal(this DataAccess.User u)
        {
            return new User()
            {
                Id = u.id,
                UserName = u.username,
                Expires = u.expires
            };
        }

        public static DataAccess.User ToDatabase(this User u)
        {
            return new DataAccess.User()
            {
                id = u.Id,
                username = u.UserName,
                expires = u.Expires
            };
        }
    }
}