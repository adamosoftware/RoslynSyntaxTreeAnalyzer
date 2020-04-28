﻿using JsonSettings;
using MarkdownViewer.App.Extensions;
using MarkdownViewer.App.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Storage.Blob.Protocol;
using RoslynDoc.Library.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MarkdownViewer.App.Pages
{
    public class MarkdownModel : PageModel
    {
        private readonly IWebHostEnvironment _hosting;
		private readonly BlobStorage _blobStorage;

        public MarkdownModel(IWebHostEnvironment hosting, BlobStorage blobStorage, CSharpMarkdownHelper csmd)
        {
            _hosting = hosting;
			_blobStorage = blobStorage;
            CSMarkdown = csmd;
        }

        public CSharpMarkdownHelper CSMarkdown { get; }

        public IEnumerable<ClassInfo> Classes { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Solution { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Namespace { get; set; }

        [BindProperty(SupportsGet = true)]
        public string AssemblyName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ClassName { get; set; }

		public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            context.HttpContext.Response.ContentType = "text/plain";
            return base.OnPageHandlerExecutionAsync(context, next);            
        }

        public async Task OnGetAsync()
        {
			var metadata = (!string.IsNullOrEmpty(Solution)) ?
				await _blobStorage.GetAsync<SolutionInfo>(User.Email(), Solution) : 
				GetSolutionMetadata();
			
			Classes = (!string.IsNullOrEmpty(Namespace)) ? metadata.Classes.Where(ci => ci.Namespace.Equals(Namespace)) : metadata.Classes;

			CSMarkdown.OnlinePath = metadata.SourceFileBase();
		}

		private SolutionInfo GetSolutionMetadata()
        {
            string fileName = Path.Combine(_hosting.WebRootPath, "data", "SolutionMetadata.json");
            return JsonFile.Load<SolutionInfo>(fileName);
        }
    }
}