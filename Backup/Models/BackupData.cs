using System;
using System.Collections.Generic;
namespace Backup.Models
{
    public class BackupData
    {
        public string name { get; set; }
        public DateTime access { get; set; }
        public long lenght { get; set; }
        public bool dir { get; set; }
        public bool file { get; set; }
        public List<BackupData> child { get; set; }

        public BackupData(string name, DateTime access, bool dir, List<BackupData> child)
        {
            this.file = false;
            this.name = name;
            this.access = access;
            this.lenght = 0;
            this.dir = dir;
            this.child = child;
        }
        public BackupData(string name, DateTime access, long lenght, bool file)
        {
            this.dir = false;
            this.name = name;
            this.access = access;
            this.lenght = lenght;
            this.file = file;
            this.child = new List<BackupData>();
        }
    }
}