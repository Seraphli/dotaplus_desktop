using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace dotaplus_desktop
{
    public class PosIndex
    {
        int index = -1;
        int max;
        List<List<int>> heroNum;
        int c, row, col;

        public PosIndex(JObject cfg)
        {
            heroNum = cfg["num"].ToObject<List<List<int>>>();
            int sum = 0;
            foreach (List<int> rows in heroNum)
                foreach (int col in rows)
                    sum += col;
            max = sum;
            c = row = 0;
            col = -1;
        }

        public bool Increment()
        {
            index += 1;
            if (index >= max)
            {
                return false;
            }
            col += 1;
            if (col >= heroNum[c][row])
            {
                col = 0;
                row += 1;
                if (row >= 2)
                {
                    row = 0;
                    c += 1;
                }
            }
            return true;
        }

        public List<int> GetPos()
        {
            List<int> pos = new List<int>() { c, row, col };
            return pos;
        }
    }

    public static class GenericCopier<T>
    {
        public static T DeepCopy(object objectToCopy)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, objectToCopy);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }

    public class Util
    {
        public static string GetFromResources(string resourceName)
        {
            Assembly assem = Assembly.GetExecutingAssembly();
            using (Stream stream = assem.GetManifestResourceStream(assem.GetName().Name + '.' + resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static Bitmap GetImageFromResources(string resourceName)
        {
            Assembly assem = Assembly.GetExecutingAssembly();
            Bitmap image;
            using (Stream stream = assem.GetManifestResourceStream(assem.GetName().Name + '.' + resourceName))
            {
                image = new Bitmap(stream);
            }
            return image;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }

    public class ServerConfig
    {
        public string config_str;
        public List<string> roles;
    }
}
