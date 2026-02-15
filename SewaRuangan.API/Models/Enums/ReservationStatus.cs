namespace SewaRuangan.API.Models.Enums
{
    public static class ReservationStatus
    {
        public const string Menunggu = "menunggu";
        public const string Disetujui = "disetujui";
        public const string Ditolak = "ditolak";
        public const string Dibatalkan = "dibatalkan";
        public const string Selesai = "selesai";

        public static string GetDisplayName(string status)
        {
            return status switch
            {
                Menunggu => "Menunggu Persetujuan",
                Disetujui => "Disetujui",
                Ditolak => "Ditolak",
                Dibatalkan => "Dibatalkan",
                Selesai => "Selesai",
                _ => status
            };
        }

        public static string GetBadgeColor(string status)
        {
            return status switch
            {
                Menunggu => "warning", // kuning
                Disetujui => "success", // hijau
                Ditolak => "danger", // merah
                Dibatalkan => "secondary", // abu-abu
                Selesai => "primary", // biru
                _ => "secondary"
            };
        }

        public static bool CanBeCancelled(string status)
        {
            return status == Menunggu || status == Disetujui;
        }

        public static bool CanBeApproved(string status)
        {
            return status == Menunggu;
        }

        public static bool CanBeRejected(string status)
        {
            return status == Menunggu;
        }

        public static bool CanBeCompleted(string status)
        {
            return status == Disetujui;
        }
    }
}