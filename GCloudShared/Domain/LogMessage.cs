using System;
namespace GCloudShared.Domain
{
    public class LogMessage : BasePersistable
    {
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
