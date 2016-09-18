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
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.UI;

namespace NuGetGallery.Controllers
{
    public class BlogController : Controller
    {
        private readonly IFileSystemService _fileSystem;

        public BlogController(IFileSystemService fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public ActionResult Index()
        {
            IList<MarkdownPostViewModel> posts = new List<MarkdownPostViewModel>();

            var postsDirectory = Server.MapPath("~/Views/Blog/");
            var postFiles = Directory.GetFiles(postsDirectory, "*.md", SearchOption.TopDirectoryOnly);
            foreach (var postFile in postFiles)
            {
                posts.Add(GetPost(postFile));
            }

            posts = posts.OrderByDescending(p => p.Published).ToList();

            return View("~/Views/Pages/MarkdownPostIndex.cshtml", posts);
        }

        [HttpGet, OutputCache(VaryByParam = "*", Location = OutputCacheLocation.Any, Duration = 7200)]
        public ActionResult Article(string articleName)
        {
            var articleNameNoHyphens = articleName.Replace("-", "");
            var filePath = Server.MapPath("~/Views/Blog/{0}.md".format_with(articleNameNoHyphens));

            if (_fileSystem.FileExists(filePath))
            {
                return View("~/Views/Pages/MarkdownPostArticle.cshtml", GetPost(filePath, articleName));
            }

            return RedirectToAction("PageNotFound", "Error");
        }

        private MarkdownPostViewModel GetPost(string filePath, string articleName = null)
        {
            var model = new MarkdownPostViewModel();
            if (_fileSystem.FileExists(filePath))
            {
                var contents = string.Empty;
                using (var fileStream = System.IO.File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    contents = streamReader.ReadToEnd();
                }

                model.UrlPath = GetUrl(filePath, articleName);
                model.Title = GetPostMetadataValue("Title", contents);
                model.Author = GetPostMetadataValue("Author", contents);
                model.Published = DateTime.ParseExact(GetPostMetadataValue("Published", contents), "yyyyMMdd", CultureInfo.InvariantCulture);
                model.Post = contents.Remove(0, contents.IndexOf("---") + 3);
            }

            return model;
        }

        private string GetUrl(string filePath, string articleName = null)
        {
            if (!string.IsNullOrWhiteSpace(articleName)) return articleName;
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
