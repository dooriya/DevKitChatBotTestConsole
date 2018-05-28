using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CognitiveServicesAuthorization;
using CognitiveServicesTTS;

namespace DevKitChatBotTestConsole
{
    public class CognitiveSpeechTTSTest
    {
        public async Task RunAsync()
        {
            string subscriptionKey = ConfigurationManager.AppSettings.Get("CognitiveSubscriptionKey");
            if (string.IsNullOrEmpty(subscriptionKey))
            {
                Console.WriteLine("Error: missing configuration value for key 'CognitiveSubscriptionKey' in app settings.");
                return;
            }

            var authorizationProvider = new CognitiveServicesAuthorizationProvider(subscriptionKey);
            string accessToken;

            try
            {
                accessToken = await authorizationProvider.GetAuthorizationTokenAsync();
                Console.WriteLine("Token: {0}\n", accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed authentication.");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine("Starting TTSSample request code execution.");
            // Synthesis endpoint for old Bing Speech API: https://speech.platform.bing.com/synthesize
            // For new unified SpeechService API: https://westus.tts.speech.microsoft.com/cognitiveservices/v1
            // Note: new unified SpeechService API synthesis endpoint is per region
            string requestUri = "https://speech.platform.bing.com/synthesize";  //"https://westus.tts.speech.microsoft.com/cognitiveservices/v1";
            var cortana = new Synthesize();

            //cortana.OnAudioAvailable += PlayAudio;
            //cortana.OnError += ErrorHandler;

            var totalBytes = await cortana.Speak(CancellationToken.None, new Synthesize.InputOptions()
            {
                RequestUri = new Uri(requestUri),
                // Text to be spoken.
                Text = "Hello, how are you doing today?",
                VoiceType = Gender.Female,
                // Refer to the documentation for complete list of supported locales.
                Locale = "en-US",
                // You can also customize the output voice. Refer to the documentation to view the different
                // voices that the TTS service can output.
                // VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, Jessa24KRUS)",
                //VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, Guy24KRUS)",
                VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)",

                // Service can return audio in different output format.
                OutputFormat = AudioOutputFormat.Riff16Khz16BitMonoPcm,
                AuthorizationToken = "Bearer " + accessToken,
            });

            Utility.PlayAudio(new MemoryStream(totalBytes));
        }

        /// <summary>
        /// This method is called once the audio returned from the service.
        /// It will then attempt to play that audio file.
        /// Note that the playback will fail if the output audio format is not pcm encoded.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="GenericEventArgs{Stream}"/> instance containing the event data.</param>
        private static void PlayAudio(object sender, GenericEventArgs<Stream> args)
        {
            Console.WriteLine(args.EventData);

            // For SoundPlayer to be able to play the wav file, it has to be encoded in PCM.
            // Use output audio format AudioOutputFormat.Riff16Khz16BitMonoPcm to do that.
            SoundPlayer player = new SoundPlayer(args.EventData);
            player.PlaySync();
            args.EventData.Dispose();
        }


        private static void ErrorHandler(object sender, GenericEventArgs<Exception> e)
        {
            Console.WriteLine("Unable to complete the TTS request: [{0}]", e.ToString());
        }
    }
}
