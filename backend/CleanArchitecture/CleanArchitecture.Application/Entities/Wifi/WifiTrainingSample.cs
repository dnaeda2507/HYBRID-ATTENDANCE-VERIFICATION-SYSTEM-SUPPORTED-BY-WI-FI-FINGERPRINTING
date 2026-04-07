using System;
using System.Collections.Generic;

namespace CleanArchitecture.Core.Entities.Wifi
{
    public class WifiTrainingSample : AuditableBaseEntity
    {
        public int ClassroomId { get; set; }
        public Classroom Classroom { get; set; }

        public DateTime CollectedAtUtc { get; set; }
        public ICollection<WifiTrainingAccessPoint> AccessPoints { get; set; } = new List<WifiTrainingAccessPoint>();
    }
}