﻿using Microsoft.AspNetCore.Mvc;

namespace MsHelper.Controllers
{
    [ApiController]
    [Route("wz")]
    public class MsWeb : ControllerBase
    {
        private readonly MsService _msService = MsService.GetInstance();

        [HttpGet("getList")]
        public ActionResult<R> GetList(string path)
        {
            return _msService.GetWzs(path);
        }

        [HttpGet("wzFile")]
        public ActionResult<R> GetWzFile(string path, string filename, short version, short mv)
        {
            return _msService.GetWzFile(path, filename, version, mv);
        }

        [HttpGet("wzObj")]
        public ActionResult<R> GetWzObject(string filename, string path)
        {
            return _msService.GetWzObject(filename, path);
        }

        [HttpGet("getImg")]
        public IActionResult GetImg(string id)
        {
            return _msService.GetPng(id);
        }
    }
}