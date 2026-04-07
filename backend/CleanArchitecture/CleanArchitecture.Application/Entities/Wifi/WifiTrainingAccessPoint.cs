namespace CleanArchitecture.Core.Entities.Wifi
{
    public class WifiTrainingAccessPoint : AuditableBaseEntity
    {
        public int WifiTrainingSampleId { get; set; }
        public WifiTrainingSample Sample { get; set; }

        public string Bssid { get; set; }
        public int Rssi { get; set; }
    }
}