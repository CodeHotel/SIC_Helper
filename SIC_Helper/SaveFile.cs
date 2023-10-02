using DrawNet_WPF.Converters;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SIC_Helper
{
    [Serializable]
    public class SaveFile
    {
        public string String1 { get; set; }
        public string String2 { get; set; }
        public string String3 { get; set; }
        public string String4 { get; set; }
        public string String5 { get; set; }
        public string String6 { get; set; }
        public string String7 { get; set; }
        public string String8 { get; set; }
        public Vector recPos { get; set; }
        public Vector recSize { get; set; }
        public Vector windowPos { get; set; }

        public static void Serialize(string filePath, SaveFile saveFile)
        {
            using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, saveFile);
            }
        }

        public static SaveFile Deserialize(string filePath)
        {
            using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IFormatter formatter = new BinaryFormatter();
                return (SaveFile)formatter.Deserialize(stream);
            }
        }

        public void SaveUserData()
        {
            string filePath = "UserData"; // specify the file name
            using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, this); // serialize the current instance
            }
        }
    }
}
