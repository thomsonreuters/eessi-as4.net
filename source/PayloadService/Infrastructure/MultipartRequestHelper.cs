﻿using System;
using System.IO;
using Microsoft.Net.Http.Headers;

namespace Eu.EDelivery.AS4.PayloadService.Infrastructure
{
    public static class MultipartRequestHelper
    {
        
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary);
            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException(
                    $"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary;
        }

        public static bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                   && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }
       
        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {            
            return contentDisposition != null
                   && contentDisposition.DispositionType.Equals("form-data")
                   && (!string.IsNullOrEmpty(contentDisposition.FileName)
                       || !string.IsNullOrEmpty(contentDisposition.FileNameStar));
        }
    }
}