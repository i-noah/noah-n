﻿using GH_IO.Serialization;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Runtime;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NoahiRhino.Utils
{
    public static class IO
    {
        // Convert.FromBase64String(x)
        public static byte[] SerializeGrasshopperData(GH_Structure<IGH_Goo> tree, string name = "default", bool isEmpty = false)
        {
            GH_LooseChunk ghLooseChunk = new GH_LooseChunk("Noah Data");
            ghLooseChunk.SetGuid("OriginId", Guid.NewGuid());

            GH_IWriter chunk = ghLooseChunk.CreateChunk("Block", 0);
            chunk.SetString("Name", name);
            chunk.SetBoolean("Empty", isEmpty);
            if (!isEmpty)
            {
                if (!tree.Write(chunk.CreateChunk("Data")))
                    throw new Exception(string.Format("There was a problem writing the {0} data.", name));
            }

            return ghLooseChunk.Serialize_Binary();
        }

        public static GH_Structure<IGH_Goo> DeserializeGrasshopperData(byte[] array)
        {
            GH_LooseChunk val = new GH_LooseChunk("Noah Data");
            val.Deserialize_Binary(array);
            if (val.ItemCount == 0)
            {
                return null;
            }

            GH_Structure<IGH_Goo> gH_Structure = new GH_Structure<IGH_Goo>();
            GH_IReader val2 = val.FindChunk("Block", 0);

            bool boolean = val2.GetBoolean("Empty");

            if (boolean) return null;

            GH_IReader val3 = val2.FindChunk("Data");
            if (val3 == null)
            {
                return null;
            }
            else if (!gH_Structure.Read(val3))
            {
                return null;
            }

            return gH_Structure;
        }

        public static string EncodeCommonObjectToBase64(CommonObject src)
        {
            if (null == src)
                return null;

            byte[] rc = null;
            try
            {
                var formatter = new BinaryFormatter();
                using (var stream = new MemoryStream())
                {
                    formatter.Serialize(stream, src);
                    rc = stream.ToArray();
                }
            }
            catch
            {
                //Debug.WriteLine(e.Message);
            }

            if (rc == null) throw new Exception("转换失败");

            return Convert.ToBase64String(rc);
        }

        public static CommonObject DecodeCommonObjectFromBase64(string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            if (null == bytes || 0 == bytes.Length)
                return null;

            CommonObject rc = null;
            try
            {
                using (var stream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Seek(0, SeekOrigin.Begin);
                    if (formatter.Deserialize(stream) is CommonObject obj)
                        rc = obj;
                }
            }
            catch
            {
                //Debug.WriteLine(e.Message);
            }

            return rc;
        }
    }
}
