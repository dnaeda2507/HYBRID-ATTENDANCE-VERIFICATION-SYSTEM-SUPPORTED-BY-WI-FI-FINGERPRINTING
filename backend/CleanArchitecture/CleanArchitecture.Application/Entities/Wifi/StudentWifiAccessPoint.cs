namespace CleanArchitecture.Core.Entities.Wifi
{
    public class StudentWifiAccessPoint : AuditableBaseEntity
    {
        public int StudentWifiScanId { get; set; }
        public StudentWifiScan Scan { get; set; }

        public string Bssid { get; set; }
        public int Rssi { get; set; }
    }
}