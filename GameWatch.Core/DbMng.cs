using System;
using System.IO;
using System.Threading;
using GameWatch.Core.GameRecords;
using Microsoft.Data.Sqlite;

namespace GameWatch.Core;

public static class DbMng
{
    public sealed class AutoGameDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PlayTimeSec { get; set; }
        public string? WindowTitle { get; set; }
        public string? FilePath { get; set; }
        public string? WindowRule { get; set; }
        public string? PathRule { get; set; }
    }
}