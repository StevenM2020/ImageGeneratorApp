// Class:       ImageGeneration
// Author:      Steven Motz
// Date:        03/18/2024
// Description: This class contains functions for generating images. It contains functions for generating images, validating prompts, and improving prompts.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGeneratorApp
{
     internal static class ImageGeneration
     {
         private static int[] imageSizes = { 256, 512, 1024 };

         public static int GetImageSize(int i)
         {
             return imageSizes[i];
         }


         public static async Task<string> GenerateImage(string userPrompt, int imageSize)
        {

            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/images/generations");

                var openAIAPIKey = await Storage.GetSecureStorage("OpenAIKey");
                if (openAIAPIKey == null)
                {
                    // error
                    return "";
                }
                Debug.WriteLine("not null");
                // add the API key to the request
                request.Headers.Add("Authorization", $"Bearer {openAIAPIKey}");

                // create the message to send to the AI
                var message = new
                {
                    //model = "dall-e-3",
                    prompt = userPrompt,
                    n = 1,
                    size = imageSizes[imageSize] + "x" + imageSizes[imageSize],
                    response_format = "b64_json"
                };
                string contentString = JsonConvert.SerializeObject(message, Formatting.Indented);
                var content = new StringContent(contentString, Encoding.UTF8, "application/json");
                request.Content = content;

                // send the request
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // get the response
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(responseContent);
                // parse the response and display it
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                var base64Image = responseObject.data[0].b64_json.ToString();

                return base64Image;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex.Message);
                Console.WriteLine(ex.Message);
            }
            return "";
        }

         public static ImageSource Base64StringToImage(string base64Image)
         {
             return ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(base64Image)));
         }

         public static async Task<bool> ValidatePrompt(string prompt)
         {
             string systemContent = "You're checking if a text prompt complies with OpenAI's content policies or if it is copyrighted, sensitive, or inappropriate content. Insure that it does not infringe on copyright laws typically associated with named entities, brands, or characters. Reply 'true' if it's appropriate and 'false' if it's not.";
             string response = await GenerateText(prompt, systemContent);
             return response.ToLower().Contains("true");
        }

         public static async Task<string> ImprovePrompt(string prompt)
         {
             {
                 string systemContent = "Your task is to refine the given text prompt to make it more effective for generating a descriptive and clear image. Ensure the modified prompt is specific, vivid, and direct without changing the original intent. Avoid ambiguity and enhance creativity.";
                 string response = await GenerateText(prompt, systemContent);
                 return response;
             }
         }

         private static async Task<string> GenerateText(string userContent, string systemContent)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");

                // get the API key from the secure storage and check if it is null
                var openAIAPIKey = await Storage.GetSecureStorage("OpenAIKey");
                if (openAIAPIKey == null)
                {
                    return"";
                }

                request.Headers.Add("Authorization", $"Bearer {openAIAPIKey}");

                string model = "gpt-3.5-turbo";

                // create the message to send to the AI
                var message = new
                {
                    model = model,
                    messages = new[]
                    {
                    new { role = "system", content = systemContent },
                    new { role = "user", content = userContent }
                }
                };
                string contentString = JsonConvert.SerializeObject(message, Formatting.Indented);
                var content = new StringContent(contentString, Encoding.UTF8, "application/json");
                request.Content = content;

                // send the request
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // get the response
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                // parse the response and display it
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                var responseText = responseObject.choices[0].message.content.ToString();
                return responseText;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return "";
        }
    }
}
