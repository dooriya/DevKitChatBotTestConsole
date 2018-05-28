using System;
namespace DevKitChatBotTestConsole
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Media;
    using System.Text;
    using System.Threading.Tasks;

    public static class Utility
    {
        public static void PlayAudio(Stream audioBinary)
        {
            // For SoundPlayer to be able to play the wav file, it has to be encoded in PCM.
            // Use output audio format AudioOutputFormat.Riff16Khz16BitMonoPcm to do that.
            SoundPlayer player = new SoundPlayer(audioBinary);
            player.PlaySync();
            audioBinary.Dispose();
        }

        public static void SaveBinaryToWavFile(string byteArrayDataFilePath, string outputWavFilePath)
        {
            List<byte> dataBytes = new List<byte>();

            string[] lines = File.ReadAllLines(byteArrayDataFilePath);
            foreach (string line in lines)
            {
                string[] items = line.Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                byte[] subItems = items.Select(i => Convert.ToByte(i)).ToArray();
                dataBytes.AddRange(subItems);
            }

            File.WriteAllBytes(outputWavFilePath, dataBytes.ToArray());
        }
    }
}
