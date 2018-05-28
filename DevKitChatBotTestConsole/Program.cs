namespace DevKitChatBotTestConsole
{
    using System;
    using System.Linq;
    using NAudio.Wave;

    public class Program
    {
        static void Main(string[] args)
        {
            //Utility.SaveBinaryToWavFile(@"C:\IoT\Voice\audioBytes.txt", @"C:\IoT\Voice\devkitAudio.pcm");

            new CognitiveSpeechSTTTest().RunAsync(@"C:\IoT\Voice\devkitAudio.pcm", "en-US").Wait();
            //new CognitiveSpeechTTSTest().RunAsync().Wait();
            //new DirectLineClientTest().RunAsync().Wait();

            //new DemoBotAppTest().RunAsync().Wait();

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}
