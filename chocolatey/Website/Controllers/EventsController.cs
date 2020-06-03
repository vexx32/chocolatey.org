// Copyright 2011 - Present RealDimensions Software, LLC, the original
// authors/contributors from ChocolateyGallery
// at https://github.com/chocolatey/chocolatey.org,
// and the authors/contributors of NuGetGallery
// at https://github.com/NuGet/NuGetGallery
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.UI;
using Markdig;
using NuGetGallery.MvcOverrides;

namespace NuGetGallery.Controllers
{
    public class EventsController : Controller
    {
        private readonly IFileSystemService _fileSystem;
        public IConfiguration Configuration { get; set; }
        public MarkdownPipeline MarkdownPipeline { get; set; }

        public EventsController(IFileSystemService fileSystem, IConfiguration configuration)
        {
            _fileSystem = fileSystem;
            Configuration = configuration;

            MarkdownPipeline = new MarkdownPipelineBuilder()
                 .UseSoftlineBreakAsHardlineBreak()
                 .UseAutoLinks()
                 .UseGridTables()
                 .UsePipeTables()
                 .UseAutoIdentifiers()
                 .UseEmphasisExtras()
                 .UseNoFollowLinks()
                 .UseCustomContainers()
                 .UseBootstrap()
                 .UseEmojiAndSmiley()
                 .UseCustomContainers()
                 .UseGenericAttributes()
                 .Build();
        }

        [HttpGet, OutputCache(VaryByParam = "*", Location = OutputCacheLocation.Any, Duration = 7200)]
        public ActionResult Events(string eventName)
        {
            eventName = eventName.Replace("-", "");
            var posts = GetPostsByUpcomingEventDate();

            if (posts.Count() == 0)
            {
                return RedirectToRoute(RouteName.Resources, new { resourceType = "home" });
            }

            return View("~/Views/Events/{0}.cshtml".format_with(eventName), posts);
        }

        private IEnumerable<EventViewModel> GetPostsByUpcomingEventDate()
        {
            IList<EventViewModel> posts = new List<EventViewModel>();

            var postsDirectory = Server.MapPath("~/Views/Events/Files/");
            var postFiles = Directory.GetFiles(postsDirectory, "*.md", SearchOption.TopDirectoryOnly);
            foreach (var postFile in postFiles)
            {
                posts.Add(GetPost(postFile));
            }

            return posts.Where(p => p.IsArchived.Equals("false")).OrderByDescending(p => p.EventDate);
        }

        private EventViewModel GetPost(string filePath, string eventName = null)
        {
            var model = new EventViewModel();
            if (_fileSystem.FileExists(filePath))
            {
                var contents = string.Empty;
                using (var fileStream = System.IO.File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    contents = streamReader.ReadToEnd();
                }

                model.IsArchived = GetPostMetadataValue("IsArchived", contents.ToLower());
                model.UrlPath = GetPostMetadataValue("Url", contents);
                if (string.IsNullOrEmpty(model.UrlPath)) model.UrlPath = GetUrl(filePath, eventName);
                model.Type = GetPostMetadataValue("Type", contents.ToLower());
                model.EventDate = DateTime.ParseExact(GetPostMetadataValue("EventDate", contents), "yyyyMMddTHH:mm:ss", CultureInfo.InvariantCulture);
                model.Time = GetPostMetadataValue("Time", contents);
                model.Duration = GetPostMetadataValue("Duration", contents);
                model.Title = GetPostMetadataValue("Title", contents);
                model.Tagline = GetPostMetadataValue("Tagline", contents);
                model.Speakers = GetPostMetadataValue("Speakers", contents).Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                model.Image = Markdown.ToHtml(GetPostMetadataValue("Image", contents), MarkdownPipeline);
                model.RegisterLink = GetPostMetadataValue("RegisterLink", contents);
                model.Tags = GetPostMetadataValue("Tags", contents).Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                model.Summary = GetPostMetadataValue("Summary", contents);
                model.Post = Markdown.ToHtml(contents.Remove(0, contents.IndexOf("---") + 3), MarkdownPipeline); 
            }

            return model;
        }

        private string GetUrl(string filePath, string eventName = null)
        {
            if (!string.IsNullOrWhiteSpace(eventName)) return eventName;
            if (string.IsNullOrWhiteSpace(filePath)) return filePath;

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var hyphenatedValue = new StringBuilder();

            Char previousChar = '^';
            foreach (var valueChar in fileName)
            {
                if (Char.IsUpper(valueChar) && hyphenatedValue.Length != 0)
                {
                    hyphenatedValue.Append("-");
                }

                if (Char.IsDigit(valueChar) && !Char.IsDigit(previousChar) && hyphenatedValue.Length != 0)
                {
                    hyphenatedValue.Append("-");
                }

                previousChar = valueChar;
                hyphenatedValue.Append(valueChar.to_string());
            }

            return hyphenatedValue.to_string().to_lower();
        }

        private string GetPostMetadataValue(string name, string contents)
        {
            var regex = new Regex(@"(?:^{0}\s*:\s*)([^\r\n]*)(?>\s*\r?$)".format_with(name), RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);

            var match = regex.Match(contents);
            return match.Groups[1].Value;
        }

    }
}