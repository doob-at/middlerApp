using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using middlerApp.Api.Attributes;
using middlerApp.Api.Helper;
using middlerApp.DataAccess;
using middlerApp.DataAccess.Entities.Models;
using middlerApp.SharedModels.Interfaces;

namespace middlerApp.Api.Controllers.Admin
{
    [ApiController]
    [Route("_api/variables")]
    [AdminController]
    [Authorize(Policy = "Admin")]
    public class VariablesController: Controller
    {
        public VariablesRepository VariablesStore { get; }

        public VariablesController(IVariablesRepository variablesStore)
        {
            VariablesStore = variablesStore as VariablesRepository;
        }


        [HttpGet("folders")]
        public IActionResult GetFolders()
        {
            return Ok(VariablesStore.GetFolderTree());
        }

        [HttpPost("{parent}/{name}")]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> StoreVariable(string parent, string name)
        {
            var sizeLimit = 5 * 1024 * 1024;

            if (Request.ContentLength > sizeLimit)
                return BadRequest($"Size Limit of '{sizeLimit / 1024 / 1024} MB' exceeded, you tried to send: {Request.ContentLength / 1024 / 1024} MB");

            if (MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {

                var bytes =  await ExecuteMultipartMessage(Request);
                var tnode = new TreeNode();
                tnode.Parent = parent?.Replace(".", "/");
                tnode.Name = name;
                tnode.Bytes = bytes;
                tnode.Extension = "bytes";

                await VariablesStore.CreateVariable(tnode);
                return Ok();
            }
            else
            {
                var bytes = await ExecuteSimpleStreaming(Request);
                var tnode = new TreeNode();
                tnode.Parent = parent?.Replace(".", "/");
                tnode.Name = name;
                tnode.Bytes = bytes;
                tnode.Extension = "bytes";

                await VariablesStore.CreateVariable(tnode);
                return Ok();
            }
        }

        private async Task<byte[]> ExecuteMultipartMessage(HttpRequest request)
        {

            if (!MultipartRequestHelper.IsMultipartContentType(request.ContentType))
            {
                throw new Exception($"Expected a multipart request, but got {request.ContentType}");
            }

            var formAccumulator = new KeyValueAccumulator();
            var _defaultFormOptions = new FormOptions();
            byte[] bytes = null;
            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(request.ContentType),
                _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, request.Body);

            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                ContentDispositionHeaderValue contentDisposition;
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        if(bytes != null)
                            continue;
                        
                        FileMultipartSection currentFile = section.AsFileSection();
                        var fileName = currentFile.FileName;
                        MemoryStream ms = new MemoryStream();
                        await section.Body.CopyToAsync(ms);
                        bytes = ms.ToArray();
                        //var filePath = Path.Combine(dirName, fileName);

                        //using (var targetStream = File.Create(filePath))
                        //{
                        //    await section.Body.CopyToAsync(targetStream).ConfigureAwait(false);
                        //}
                        //result.Add(new FileStreamInfo(filePath, fileName));

                        //await section.Body.CopyToAsync(targetStream);
                    }
                    else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {
                        // Content-Disposition: form-data; name="key"
                        //
                        // value

                        // Do not limit the key name length here because the 
                        // multipart headers length limit is already in effect.
                        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                        var encoding = section.GetEncoding();
                        using (var streamReader = new StreamReader(
                            section.Body,
                            encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true))
                        {
                            // The value length limit is enforced by MultipartBodyLengthLimit
                            var value = await streamReader.ReadToEndAsync();
                            if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                            {
                                value = string.Empty;
                            }
                            formAccumulator.Append(key.ToString(), value);

                            if (formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
                            {
                                throw new InvalidDataException($"Form key count limit {_defaultFormOptions.ValueCountLimit} exceeded.");
                            }
                        }
                    }
                }

                // Drains any remaining section body that has not been consumed and
                // reads the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }



            return bytes;
        }

        private async Task<byte[]> ExecuteSimpleStreaming(HttpRequest request)
        {

            MemoryStream ms = new MemoryStream();
            await request.Body.CopyToAsync(ms);
            return ms.ToArray();

        }
    }
}
