﻿using Microsoft.Extensions.Configuration;
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

        public async Task<CVAnalysisResult?> AnalyzeCVAsync(string textContent, string province, string? district)

        {

            string locationFormatted = string.IsNullOrWhiteSpace(district)
        ? province
        : $"{province}, {district}";

            var prompt = $$"""
                            You are a professional CV analysis assistant.

                            Analyze the following CV text and extract **structured information** in JSON format with **detailed reasoning**.

                        --- 

                        ### Extract:

                        1. **positionLevel**: Determine the candidate’s level of experience or seniority.
                        Choose the most appropriate from:
                        - Intern, Fresher, Junior, Middle, Senior, Lead, Manager, Director, VP, C-level

                        2. **jobCategory**: Categorize the job title into one of (or more specific if possible):
                        - Software Engineering, Frontend Development, Backend Development, Mobile App Development, DevOps, Data Analysis, Data Engineering, Machine Learning, Artificial Intelligence, Cybersecurity, Blockchain, Network Engineering, Business Analyst, Marketing, Content Creator, Copywriter, Product Design, UI/UX Design, Human Resources, Project Management, Game Development, QA/QC, Sales, E-commerce, SEO Specialist, Cloud Engineer, etc.

                        3. **field**: The CV's academic or industry field, such as:
                        - Information Technology, Computer Science, Artificial Intelligence, Event, Cybersecurity, Data Science, Multimedia, Graphic Design, Communication, Journalism, Business Administration, Marketing, Finance, Banking, Accounting, Human Resources, Education, Law, Construction, Architecture, Healthcare, Civil Engineering, Economics, etc.

                        4. **location**: Return exactly as: "{{locationFormatted}}"

                        5. **estimatedSalary**: Estimate a realistic monthly salary in VND, considering:
                        - Proven skills
                        - Real-world experience
                        - Certifications or projects
                        - Job level and job category
                        - Market demand in {{locationFormatted}}

                        6. **salaryReason**: Write in **Vietnamese** as **one clear paragraph**, based on actual CV content.
                        - Explain why can have that estimated Salary. 
                        - Mention exact skills (e.g., ReactJS, Python, AWS)
                        - Certifications, international degrees or courses
                        - Emphasize key projects and achievements
                        - Major projects or responsibilities
                        - Number of years of experience or impact
                        - Justify the salary logically based on {{locationFormatted}}, job category, and skill strength
                        - Include soft skills or unique value if relevant
                        - Don’t use vague language – base everything on actual CV content
                
                        7. **improvementSuggestions**: Write in **Vietnamese** as **one clear paragraph**. 
                        - Point out unclear or missing job titles, lack of KPIs/results, outdated skills
                        - Mention missing certifications (e.g., Google, AWS, Coursera)
                        - Suggest modern skills to add (e.g., Next.js, TypeScript, Figma, Docker, ChatGPT)
                        - Identify structural or formatting issues (lack of clear sections or inconsistent styling)
                        - Point out if no GitHub/portfolio/LinkedIn is provided
                        - Mention if the CV is not tailored to any specific role or lacks focus

                        --- 

                        Return **only** valid JSON in this exact format:

                    ```json
                    {
                    "positionLevel": "...",
                    "jobCategory": "...",
                    "field": "...",
                    "location": "{{locationFormatted}}",
                    "estimatedSalary": 00000000,
                    "salaryReason": "..."
                    "improvementSuggestions": "..."
                    }
                    ```

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
