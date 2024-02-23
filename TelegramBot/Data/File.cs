using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TelegramBot.Data;

[PrimaryKey("Id")]
public class File
{
    public long Id { get; set; }
    public long AccessHash { get; set; }
    public byte[] FileReference { get; set; } = null!;
    [MaxLength(32)] public string? Type { get; set; } = null;
    [MaxLength(32)] public string Category { get; set; } = null!;
    [MaxLength(32)] public string LogsName { get; set; } = null!;
}