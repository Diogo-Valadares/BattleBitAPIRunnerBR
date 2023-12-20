public class Program
{
    public static void Main()
    {

    }

    public interface IDatabase {
        byte[] GetFile(string fileName);
        bool SaveFile(string fileName, byte[] file);
    }

    public interface IDrive
    {
        byte[] Download(string fileName);
        bool Upload(string fileName, byte[] file);
    }
    

    public class DriveService : IDrive
    {
        IDatabase Database { get; set; } 
        public byte[] Download(string fileName)
        {
            return Database.GetFile(fileName);
        }

        public bool Upload(string fileName, byte[] file)
        {
            return Database.SaveFile(fileName, file);
        }
    }

    public class CacheProxy : IDrive
    {
        public IDrive FileProvider { get; set; }

        public Dictionary<string, byte[]> fileCache;

        public CacheProxy (IDrive fileProvider)
        {
            FileProvider = fileProvider;
        }

        public byte[] Download(string fileName)
        {
            if (fileCache.ContainsKey(fileName))
            {
                return fileCache[fileName];
            }
            
            var file = FileProvider.Download(fileName);
            fileCache.Add(fileName, file);
            return file;
        }

        public bool Upload(string fileName, byte[] file)
        {
            if (fileCache.ContainsKey(fileName))
            {
                fileCache[fileName] = file;
            }
            return FileProvider.Upload(fileName, file);
        }
    }


}