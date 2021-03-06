﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using CommonMark;
using Volo.Abp.DependencyInjection;
using Volo.Docs.Documents;
using Volo.Docs.HtmlConverting;
using Volo.Docs.Projects;
using Volo.Docs.Utils;

namespace Volo.Docs.Markdown
{
    public class MarkdownDocumentToHtmlConverter : IDocumentToHtmlConverter, ITransientDependency
    {
        public const string Type = "md";

        private const string MdLinkFormat = "[{0}](/documents/{1}/{2}{3}/{4})";
        private const string MarkdownLinkRegExp = @"\[(.*)\]\((.*\.md)\)";
        private const string AnchorLinkRegExp = @"<a[^>]+href=\""(.*?)\""[^>]*>(.*)?</a>";

        public virtual string Convert(ProjectDto project, DocumentWithDetailsDto document, string version)
        {
            if (document.Content.IsNullOrEmpty())
            {
                return document.Content;
            }

            var content = NormalizeLinks(
                document.Content,
                project.ShortName,
                version,
                document.LocalDirectory
            );

            return CommonMarkConverter.Convert(Encoding.UTF8.GetString(Encoding.Default.GetBytes(content)));
        }

        protected virtual string NormalizeLinks(
            string content,
            string projectShortName,
            string version,
            string documentLocalDirectory)
        {
            var normalized = Regex.Replace(content, MarkdownLinkRegExp, delegate (Match match)
            {
                var link = match.Groups[2].Value;
                if (UrlHelper.IsExternalLink(link))
                {
                    return match.Value;
                }

                var displayText = match.Groups[1].Value;

                var documentName = RemoveFileExtension(link);
                var documentLocalDirectoryNormalized = documentLocalDirectory.TrimStart('/').TrimEnd('/');
                if (!string.IsNullOrWhiteSpace(documentLocalDirectoryNormalized))
                {
                    documentLocalDirectoryNormalized = "/" + documentLocalDirectoryNormalized;
                }

                return string.Format(
                    MdLinkFormat,
                    displayText,
                    projectShortName,
                    version,
                    documentLocalDirectoryNormalized,
                    documentName
                );
            });

            normalized = Regex.Replace(normalized, AnchorLinkRegExp, delegate (Match match)
            {
                var link = match.Groups[1].Value;
                if (UrlHelper.IsExternalLink(link))
                {
                    return match.Value;
                }

                var displayText = match.Groups[2].Value;
                var documentName = RemoveFileExtension(link);
                var documentLocalDirectoryNormalized = documentLocalDirectory.TrimStart('/').TrimEnd('/');
                if (!string.IsNullOrWhiteSpace(documentLocalDirectoryNormalized))
                {
                    documentLocalDirectoryNormalized = "/" + documentLocalDirectoryNormalized;
                }

                return string.Format(
                    MdLinkFormat,
                    displayText,
                    projectShortName,
                    version,
                    documentLocalDirectoryNormalized,
                    documentName
                );
            });

            return normalized;
        }

        private static string RemoveFileExtension(string documentName)
        {
            if (documentName == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(documentName))
            {
                return documentName;
            }

            if (!documentName.EndsWith(Type, StringComparison.OrdinalIgnoreCase))
            {
                return documentName;
            }

            return documentName.Left(documentName.Length - Type.Length - 1);
        }
    }
}
