namespace DevKitChatBotTestConsole
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Configuration;
    using System.Threading.Tasks;
    using CognitiveServicesAuthorization;
    using Microsoft.Bing.Speech;
    using System.IO;
    using NAudio.Wave;
    using DemoBotApp;

    public class CognitiveSpeechSTTTest
    {
        /// <summary>
        /// Short phrase mode URL
        /// </summary>
        private static readonly Uri ShortPhraseUrl = new Uri(@"wss://speech.platform.bing.com/api/service/recognition");

        /// <summary>
        /// The long dictation URL
        /// </summary>
        private static readonly Uri LongDictationUrl = new Uri(@"wss://speech.platform.bing.com/api/service/recognition/continuous");

        /// <summary>
        /// A completed task
        /// </summary>
        private static readonly Task CompletedTask = Task.FromResult(true);

        /// <summary>
        /// Cancellation token used to stop sending the audio.
        /// </summary>
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        /// <summary>
        /// Invoked when the speech client receives a phrase recognition result(s) from the server.
        /// </summary>
        /// <param name="args">The recognition result.</param>
        /// <returns>
        /// A task
        /// </returns>
        private Task OnRecognitionResult(RecognitionResult args)
        {
            var response = args;
            Console.WriteLine();

            Console.WriteLine("--- Phrase result received by OnRecognitionResult ---");

            // Print the recognition status.
            Console.WriteLine("***** Phrase Recognition Status = [{0}] ***", response.RecognitionStatus);
            if (response.Phrases != null)
            {
                foreach (var result in response.Phrases)
                {
                    // Print the recognition phrase display text.
                    Console.WriteLine("{0} (Confidence:{1})", result.DisplayText, result.Confidence);
                }
            }

            Console.WriteLine();
            return CompletedTask;
        }

        public async Task RunAsync(string audioFile, string locale)
        {
            string subscriptionKey = ConfigurationManager.AppSettings.Get("CognitiveSubscriptionKey");
            if (string.IsNullOrEmpty(subscriptionKey))
            {
                Console.WriteLine("Error: missing configuration value for key 'CognitiveSubscriptionKey' in app settings.");
                return;
            }

            using (SpeechRecognitionClient client = new SpeechRecognitionClient(subscriptionKey))
            {
                using (var audio = new FileStream(audioFile, FileMode.Open, FileAccess.Read))
                {
                    string speechText = await client.ConvertSpeechToTextAsync(audio);
                    Console.WriteLine($"You said: {speechText}");
                }
            }


            /*
            // create the preferences object
            var preferences = new Preferences(locale, ShortPhraseUrl, new CognitiveServicesAuthorizationProvider(subscriptionKey));

            // Create a a speech client
            using (var speechClient = new SpeechClient(preferences))
            {
                speechClient.SubscribeToRecognitionResult(this.OnRecognitionResult);

                // create an audio content and pass it a stream.
                using (var audio = new FileStream(audioFile, FileMode.Open, FileAccess.Read))
                {
                    var deviceMetadata = new DeviceMetadata(DeviceType.Near, DeviceFamily.Desktop, NetworkType.Wifi, OsName.Windows, "1607", "Dell", "T3600");
                    var applicationMetadata = new ApplicationMetadata("SampleApp", "1.0.0");
                    var requestMetadata = new RequestMetadata(Guid.NewGuid(), deviceMetadata, applicationMetadata, "SampleAppService");

                    await speechClient.RecognizeAsync(new SpeechInput(audio, requestMetadata), this.cts.Token).ConfigureAwait(false);
                }
            }
            */
        }
    }
}
