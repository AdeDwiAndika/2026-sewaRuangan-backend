using System.Text.Json.Serialization;

namespace SewaRuangan.API.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        Admin = 1,
        Mahasiswa = 2,
        Dosen = 3,
        Staff = 4
    }
}