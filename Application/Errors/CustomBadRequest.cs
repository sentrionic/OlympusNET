using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Application.Errors
{
    public class Error
    {
        public string Field { get; set; }
        public string Message { get; set; }
    }
    public class CustomBadRequest
    {
        public string Message { get; set; }
        [JsonPropertyName("errors")]
        public List<Error> Errors { get; } = new List<Error>();
        public CustomBadRequest(ActionContext context)
        {
            Message = "One or more validation errors occurred.";
            ConstructErrorMessages(context);
        }
        
        private void ConstructErrorMessages(ActionContext context)
        {
            foreach (var keyModelStatePair in context.ModelState)
            {
                var key = keyModelStatePair.Key;
                var errors = keyModelStatePair.Value.Errors;
                if (errors.Count <= 0) continue;
                if (errors.Count == 1)
                {
                    var errorMessage = GetErrorMessage(errors[0]);
                    Errors.Add(new Error
                    {
                        Field = char.ToLower(key[0]) + key.Substring(1),
                        Message = errorMessage 
                    });
                }
                else
                {
                    var errorMessages = new string[errors.Count];
                    for (var i = 0; i < errors.Count; i++)
                    {
                        errorMessages[i] = GetErrorMessage(errors[i]);
                    }
                    Errors.Add(new Error
                    {
                        Field = key,
                        Message = errorMessages.ToString() 
                    });
                }
            }
        }
        string GetErrorMessage(ModelError error)
        {
            return string.IsNullOrEmpty(error.ErrorMessage) ?
                "The input was not valid." :
            error.ErrorMessage;
        }
    }
}