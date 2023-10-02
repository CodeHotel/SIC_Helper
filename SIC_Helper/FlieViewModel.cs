using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SIC_Helper
{
    [Serializable]
    public class FileItem
    {
        public string Filename { get; set; }
        public string Description { get; set; }
    }

    [Serializable]
    public class FileViewModel
    {
        public ObservableCollection<FileItem> FileItems { get; set; }

        public FileViewModel()
        {
            FileItems = new ObservableCollection<FileItem>();
        }

        public bool SaveToPath(string path)
        {
            if (!Directory.Exists(path))
                return false;

            string fullPath = Path.Combine(path, "SIC_HELPER.metadata");
            using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
            }
            return true;
        }


        public static FileViewModel LoadFromPath(string path)
        {
            string fullPath = Path.Combine(path, "SIC_HELPER.metadata");
            if (File.Exists(fullPath))
            {
                using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                {
                    var formatter = new BinaryFormatter();
                    return (FileViewModel)formatter.Deserialize(stream);
                }
            }
            else return new FileViewModel();
        }

        public void Verify(string path)
        {
            for (int i = FileItems.Count - 1; i >= 0; i--)
            {
                var fileItem = FileItems[i];
                string filePath = Path.Combine(path, fileItem.Filename);
                filePath += ".png";
                if (!File.Exists(filePath))
                {
                    FileItems.RemoveAt(i);
                }
            }
        }
    }
}
