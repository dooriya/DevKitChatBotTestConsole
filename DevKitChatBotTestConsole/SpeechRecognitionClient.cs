namespace DemoBotApp
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using CognitiveServicesAuthorization;
    using Microsoft.Bing.Speech;
    using Newtonsoft.Json;

    public class SpeechRecognitionClient : IDisposable
    {
        private string cognitiveSubscriptionKey;
        private HttpClient httpClient;

        private bool disposed;
        private readonly string serviceUrl = "https://speech.platform.bing.com/speech/recognition/conversation/cognitiveservices/v1?language=en-US&format=detailed";

        public SpeechRecognitionClient(string subscriptionKey)
        {
            if (string.IsNullOrEmpty(subscriptionKey))
            {
                throw new ArgumentNullException(subscriptionKey);
            }

            this.cognitiveSubscriptionKey = subscriptionKey;
            this.httpClient = new HttpClient();
        }

        ~SpeechRecognitionClient()
        {
            Dispose(false);
        }

        public async Task<string> ConvertSpeechToTextAsync(Stream contentStream)
        {
            CognitiveServicesAuthorizationProvider tokenProvider = new CognitiveServicesAuthorizationProvider(this.cognitiveSubscriptionKey);
            string token = await tokenProvider.GetAuthorizationTokenAsync();

            this.httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            
            this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json;text/xml");
            this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", cognitiveSubscriptionKey);
            this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-type", @"audio/wav; codec=""audio/pcm""; samplerate=8000");
            this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Host", "speech.platform.bing.com");
            this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Transfer-Encoding", "chunked");
            this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Expect", "100-continue");
            

            using (var binaryContent = new StreamContent(contentStream))
            {
                var response = await this.httpClient.PostAsync(serviceUrl, binaryContent);
                var responseString = await response.Content.ReadAsStringAsync();

                try
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseString);
                    if (result.RecognitionStatus == RecognitionStatus.Success)
                    {
                        return result.NBest[0].Lexical;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (JsonReaderException ex)
                {
                    throw new InvalidDataException(responseString, ex);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.httpClient != null)
                {
                    this.httpClient.Dispose();
                }
            }

            this.disposed = true;
        }
    }
}
