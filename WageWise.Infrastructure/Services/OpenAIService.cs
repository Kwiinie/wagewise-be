using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WageWise.Application.Interfaces.Services;
using WageWise.Domain.Models;
using Newtonsoft.Json;


namespace WageWise.Infrastructure.Services
{
    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        public AIService(IConfiguration config)
        {
            _apiKey = config["Gemini:ApiKey"] ?? throw new Exception("Missing Gemini API key.");
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://generativelanguage.googleapis.com/")
            };
        }

        public async Task<CVAnalysisResult?> AnalyzeCVAsync(string textContent, string province, string district)
        {
            var prompt = $$"""
                         You are a professional CV analysis assistant.

                         Analyze the following CV text and extract **structured information** in JSON format with **detailed reasoning**.

                         ---

                        ### Extract:

                        1. **positionLevel**: One of:
                            - Intern, Fresher, Junior, Middle, Senior, Lead, Manager, Director

                        2. **jobCategory**: Categorize the job title into:
                            - Software Engineering, Artificial Intelligence, Data Analysis, Information Assurance, Network Engineering, Content Creator, UI/UX Design, Marketing, HR, Sales, etc.

                        3. **field**: The major field of the CV, such as:
                            - IT, Business, Finance, Graphic Design, Education, Media, Healthcare, etc.

                        4. **location**: Extract and format as: **{{province}}, {{district}}**

                        5. **estimatedSalary**: Estimate a realistic **monthly salary in VND**. Make an informed guess using:
                            - position level
                            - job category + field
                            - **location: {{province}}, {{district}}**
                            - CV content: skills, certifications, project depth, company type
                            - current salary benchmarks in that specific province or major nearby cities in Vietnam

                        6. **salaryReason**: Write a detailed reason using the actual CV content.
                            - Mention specific **skills** (e.g., Python, React, SQL)
                            - Mention relevant **projects or achievements**
                            - Mention **certifications**, degrees, or internships
                            - Adjust the range based on **{{province}}, {{district}}** and job level
                            - Provide a real-world context for the salary (e.g., “Junior React developers in Hồ Chí Minh earn 15–20M”) **ANSWER IN VIETNAMESE**

                        ---

                        Return **only** a valid JSON in this exact format:

                        ```json
                        {
                         "positionLevel": "...",
                         "jobCategory": "...",
                         "field": "...",
                         "location": "...",
                         "estimatedSalary": 00000000,
                         "salaryReason": "..."
                         }

                        CV Text:
                        {{textContent}}

             """;

            var body = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[] { new { text = prompt } }
                }
            }
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}", UriKind.Relative),
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Gemini Error: " + error);
                return null;
            }

            var resultText = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Gemini Raw Response:");
            Console.WriteLine(resultText);
            dynamic json = JsonConvert.DeserializeObject(resultText)!;
            string modelOutput = json.candidates[0].content.parts[0].text;
            modelOutput = modelOutput.Replace("```json", "").Replace("```", "").Trim();

            try
            {
                return JsonConvert.DeserializeObject<CVAnalysisResult>(modelOutput);
            }
            catch
            {
                Console.WriteLine("Failed to parse Gemini JSON: " + modelOutput);
                return null;
            }
        }


        public async Task<(bool isCV, string reason)> IsLikelyCVAsync(string textContent)
        {
            var prompt = $$"""
    Bạn là một hệ thống phân loại tài liệu.

    Hãy xác định xem đoạn văn bản sau **có phải là CV xin việc hay không**.

    Trả lời **bằng đúng JSON** theo định dạng:

    Reason trả lời bằng tiếng Việt

    ```json
    {
      "isCV": true,
      "reason": "..."
    }
    ```

    ---
    Văn bản:
    {{textContent}}
    """;

            var body = new
            {
                contents = new[]
                {
            new
            {
                parts = new[] { new { text = prompt } }
            }
        }
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}", UriKind.Relative),
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Gemini error: " + await response.Content.ReadAsStringAsync());
                return (false, "Lỗi khi gửi yêu cầu đến Gemini");
            }

            var resultText = await response.Content.ReadAsStringAsync();
            dynamic json = JsonConvert.DeserializeObject(resultText)!;
            string modelOutput = json.candidates[0].content.parts[0].text;
            modelOutput = modelOutput.Replace("```json", "").Replace("```", "").Trim();

            try
            {
                dynamic result = JsonConvert.DeserializeObject(modelOutput)!;
                return ((bool)result.isCV, (string)result.reason);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi parse JSON Gemini: " + modelOutput);
                return (false, "Không thể phân tích phản hồi từ AI");
            }
        }

        /*public OpenAIService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["OpenAI:ApiKey"] ?? throw new ArgumentNullException("Missing OpenAI API Key.");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<CVAnalysisResult?> AnalyzeCVAsync(string textContent)
        {
            var prompt = $$"""
Analyze the following CV text and extract:
- Job Title
- Job Category (e.g., IT, Marketing, Sales)
- Job Location (Vietnam-based if available)
- Estimated monthly salary in VND

Format the result as valid JSON:
{
  "jobTitle": "...",
  "jobCategory": "...",
  "location": "...",
  "estimatedSalary": 0
}

CV Text:
{{textContent}}
""";

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                new { role = "system", content = "You are a CV parsing assistant." },
                new { role = "user", content = prompt }
            }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine("OpenAI Error: " + err);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            // Extract the assistant reply content from response
            using var doc = JsonDocument.Parse(json);
            var message = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(message))
                return null;

            try
            {
                var result = JsonSerializer.Deserialize<CVAnalysisResult>(message, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch
            {
                Console.WriteLine("Failed to parse AI response: " + message);
                return null;
            }
        }*/
    }
}
