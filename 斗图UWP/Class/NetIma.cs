using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace 斗图UWP
{
    class NetIma
    {
        public async Task<StorageFile> getIma(string uri)
        {
            List<Byte> allbytes = new List<byte>();
            using (var response = await HttpWebRequest.Create(uri).GetResponseAsync())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    byte[] buffer = new byte[4000];
                    int bytesRead = 0;
                    while ((bytesRead = await responseStream.ReadAsync(buffer, 0, 4000)) > 0)
                    {
                        allbytes.AddRange(buffer.Take(bytesRead));
                    }
                }
            }
            var local = ApplicationData.Current.LocalFolder;
            var folder = await local.CreateFolderAsync("斗图", CreationCollisionOption.OpenIfExists);
            var file = await folder.CreateFileAsync("temp" + ".png", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, allbytes.ToArray());
            return file;
        }
    }
}
