using System;
using System.Text.RegularExpressions;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.Services
{
    public static class AzureAISearchIndexValidator
    {
        private static readonly Regex ValidIndexNameRegex = new Regex("^[a-z0-9]([a-z0-9_-]*[a-z0-9])?$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
        private static readonly Regex ConsecutiveSpecialCharsRegex = new Regex("[_-]{2,}", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        public static ValidationResult ValidateIndexName(string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Index name is required."
                };
            }

            if (indexName.Length < 2 || indexName.Length > 128)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Index name must be between 2 and 128 characters long."
                };
            }

            if (indexName != indexName.ToLowerInvariant())
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Index name must be lowercase."
                };
            }

            if (!ValidIndexNameRegex.IsMatch(indexName))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Index name can only contain lowercase letters, numbers, dashes (-), and underscores (_). It must start and end with a letter or number."
                };
            }

            if (ConsecutiveSpecialCharsRegex.IsMatch(indexName))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Index name cannot contain consecutive dashes or underscores."
                };
            }

            if (indexName.Contains("..") || indexName.Contains("//"))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Index name cannot contain dots or slashes."
                };
            }

            return new ValidationResult { IsValid = true };
        }

        public static ValidationResult ValidateSearchServiceUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Search service URL is required."
                };
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Search service URL is not a valid URL."
                };
            }

            if (uri.Scheme != "https")
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Search service URL must use HTTPS."
                };
            }

            if (!uri.Host.EndsWith(".search.windows.net"))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Search service URL must be an Azure AI Search service endpoint (*.search.windows.net)."
                };
            }

            return new ValidationResult { IsValid = true };
        }

        public static ValidationResult ValidateApiKey(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "API key is required."
                };
            }

            if (apiKey.Length < 32)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "API key appears to be invalid (too short)."
                };
            }

            return new ValidationResult { IsValid = true };
        }

        public static ValidationResult ValidateJsonPayload(string jsonPayload)
        {
            if (string.IsNullOrWhiteSpace(jsonPayload))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "JSON payload is required."
                };
            }

            try
            {
                System.Text.Json.JsonDocument.Parse(jsonPayload);
                return new ValidationResult { IsValid = true };
            }
            catch (System.Text.Json.JsonException ex)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Invalid JSON format: {ex.Message}"
                };
            }
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }
}