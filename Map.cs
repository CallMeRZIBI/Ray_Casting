using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace RayCasting
{
    public class Map
    {
        public int[,] map;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public void LoadMap(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var JsonMap = JsonConvert.DeserializeObject<int[,]>(json);
                map = JsonMap;

                Width = map.GetLength(0);
                Height = map.GetLength(1);
            }catch(Exception ex)
            {
                throw new Exception("Problem occured while loading Map:\n" + ex.Message);
            }
        }

        public void SaveMap(string path)
        {
            try
            {
                var JsonMap = JsonConvert.SerializeObject(map);
                byte[] json = new UTF8Encoding(true).GetBytes(JsonMap);

                Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('/')));
                using (FileStream fs = File.Create(path))
                {
                    fs.Write(json);
                }
            }catch(Exception ex)
            {
                throw new Exception("Problem occured while saving Map:\n" + ex.Message);
            }
        }
    }
}
