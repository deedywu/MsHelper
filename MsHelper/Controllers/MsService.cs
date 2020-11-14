using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using MsHelper.MapleLib.WzLib;
using MsHelper.MapleLib.WzLib.WzProperties;
using Microsoft.AspNetCore.Mvc;
using MsHelper.MapleLib.WzLib.Util;

namespace MsHelper.Controllers
{
    public class MsService
    {
        private MsService()
        {
        }

        private static MsService _service;

        private static readonly object Locker = new object();

        public static MsService GetInstance()
        {
            lock (Locker)
                if (_service == null)
                    _service = new MsService();
            return _service;
        }

        private readonly Dictionary<string, WzFile> _wzFiles = new Dictionary<string, WzFile>();

        private readonly Dictionary<string, string> _guid = new Dictionary<string, string>();

        public R GetWzs(string path)
        {
            var r = R.Init();
            var list = new List<string>();
            if (!Directory.Exists(path)) return r.SetResult(400, "该路径不是文件夹!");
            var root = new DirectoryInfo(path);
            foreach (var keyValuePair in _wzFiles) keyValuePair.Value.Dispose();
            _wzFiles.Clear();
            var fileInfos = root.GetFiles();
            list.AddRange(from fileInfo in fileInfos where fileInfo.Name.EndsWith(".wz") select fileInfo.Name);
            return r.SetResult(200, "获取wz文件列表成功!", list);
        }

        public R GetWzFile(string path, string filename, short version, short mv)
        {
            var r = R.Init();
            WzFile wzFile;
            if (!_wzFiles.ContainsKey(filename))
            {
                var mapleVer = WzMapleVersion.Gms;
                switch (mv)
                {
                    case 1:
                        mapleVer = WzMapleVersion.Gms;
                        break;
                    case 2:
                        mapleVer = WzMapleVersion.Ems;
                        break;
                    case 3:
                        mapleVer = WzMapleVersion.Bms;
                        break;
                }

                wzFile = new WzFile(path + "/" + filename, version, mapleVer);
                try
                {
                    wzFile.ParseWzFile();
                    _wzFiles[filename] = wzFile;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return r.SetResult(500, e.Message);
                }
            }

            wzFile = _wzFiles[filename];
            var dir = ListSort(wzFile.WzDirectory.WzDirectories
                .Select(wzDirectoryWzDirectory => wzDirectoryWzDirectory.Name)
                .ToList());
            var img = ListSort(wzFile.WzDirectory.WzImages.Select(wzDirectoryWzImage => wzDirectoryWzImage.Name)
                .ToList());
            var map = new Dictionary<string, List<string>> {["dir"] = dir, ["img"] = img};
            return r.SetResult(200, $"获取{filename}文件成功!", map);
        }

        public R GetWzObject(string filename, string path)
        {
            var r = R.Init();
            if (!filename.EndsWith(".wz"))
            {
                var indexOf = filename.IndexOf(".wz", StringComparison.Ordinal);
                path = filename.Substring(indexOf + 4) + "/" + path;
                filename = filename.Substring(0, indexOf + 3);
            }

            var wzFile = _wzFiles[filename];
            var strings = path.Split("/");
            object wzObject = null;
            for (var i = 0; i < strings.Length; i++)
                wzObject = i == 0 ? wzFile[strings[i]] : ((WzObject) wzObject)?[strings[i]];
            if (wzObject == null) return r.SetResult(400, "获取错误!");
            var types = new Dictionary<string, object>();
            var map = new Dictionary<string, object>();
            if (wzObject is WzImage wzImage)
            {
                var wzImageWzProperties = wzImage.WzProperties;
                foreach (var wzImageWzProperty in wzImageWzProperties)
                {
                    var objects = new Dictionary<object, object>
                    {
                        ["value"] = wzImageWzProperty.WzValue, ["type"] = wzImageWzProperty.PropertyType.ToString()
                    };
                    types[wzImageWzProperty.Name] = objects;
                }

                map["property"] = types;
            }
            else if (wzObject is WzDirectory wzDirectory)
            {
                var dir = ListSort(wzDirectory.WzDirectories
                    .Select(wzDirectoryWzDirectory => wzDirectoryWzDirectory.Name)
                    .ToList());
                var img = ListSort(wzDirectory.WzImages.Select(wzDirectoryWzImage => wzDirectoryWzImage.Name)
                    .ToList());
                map["dir"] = dir;
                map["img"] = img;
            }
            else if (wzObject is WzSubProperty wzSubProperty)
            {
                var subProperty = new Dictionary<string, object>();
                foreach (var wzImageProperty in wzSubProperty.WzProperties)
                {
                    var dictionary = new Dictionary<object, object>();
                    if (wzImageProperty is WzSubProperty)
                    {
                        foreach (var imageProperty in wzImageProperty.WzProperties)
                        {
                            var objects = new Dictionary<object, object>();
                            if (imageProperty is WzSoundProperty soundProperty)
                            {
                                objects["value"] = Base64((soundProperty.GetBytes(false)));
                                objects["type"] = soundProperty.PropertyType.ToString();
                            }
                            else if (imageProperty is WzUolProperty uolProperty)
                            {
                                objects["value"] = uolProperty.Value;
                                objects["type"] = uolProperty.PropertyType.ToString();
                            }

                            else if (imageProperty is WzCanvasProperty canvasProperty)
                            {
                                var guid = Guid.NewGuid().ToString("N");
                                _guid[guid] = canvasProperty.FullPath;
                                objects["value"] = guid;
                                objects["width"] = canvasProperty.PngProperty.Width;
                                objects["height"] = canvasProperty.PngProperty.Height;
                                objects["type"] = canvasProperty.PropertyType.ToString();
                            }
                            else if (imageProperty is WzVectorProperty vectorProperty)
                            {
                                objects["value"] = ((WzVector2) vectorProperty.WzValue).GetV2();
                                objects["type"] = vectorProperty.PropertyType.ToString();
                            }
                            else
                            {
                                objects["value"] = imageProperty.WzValue;
                                objects["type"] = imageProperty.PropertyType.ToString();
                            }

                            dictionary[imageProperty.Name] = objects;
                        }

                        subProperty[wzImageProperty.Name] = dictionary;
                        dictionary["type"] = wzSubProperty.PropertyType.ToString();
                    }
                    else if (wzImageProperty is WzCanvasProperty canvasProperty)
                    {
                        var guid = Guid.NewGuid().ToString("N");
                        _guid[guid] = canvasProperty.FullPath;
                        var objects = new Dictionary<object, object>
                        {
                            ["value"] = guid,
                            ["width"] = canvasProperty.PngProperty.Width,
                            ["height"] = canvasProperty.PngProperty.Height,
                            ["type"] = wzImageProperty.PropertyType.ToString()
                        };
                        subProperty[wzImageProperty.Name] = objects;
                    }
                    else if (wzImageProperty is WzUolProperty uolProperty)
                    {
                        var objects = new Dictionary<object, object>
                        {
                            ["value"] = uolProperty.Value, ["type"] = wzImageProperty.PropertyType.ToString()
                        };
                        subProperty[wzImageProperty.Name] = objects;
                    }
                    else if (wzImageProperty is WzSoundProperty soundProperty)
                    {
                        var objects = new Dictionary<object, object>
                        {
                            ["value"] = Base64((soundProperty.GetBytes(false))),
                            ["type"] = soundProperty.PropertyType.ToString()
                        };
                        subProperty[wzImageProperty.Name] = objects;
                    }
                    else if (wzImageProperty is WzVectorProperty vectorProperty)
                    {
                        var objects = new Dictionary<object, object>
                        {
                            ["value"] = ((WzVector2) vectorProperty.WzValue).GetV2(),
                            ["type"] = vectorProperty.PropertyType.ToString()
                        };
                        subProperty[wzImageProperty.Name] = objects;
                    }
                    else
                    {
                        var objects = new Dictionary<object, object>
                        {
                            ["value"] = wzImageProperty.WzValue, ["type"] = wzImageProperty.PropertyType.ToString()
                        };
                        subProperty[wzImageProperty.Name] = objects;
                    }

                }

                map["property"] = subProperty;
            }
            else if (wzObject is WzCanvasProperty canvasProperty)
            {
                var subProperty = new Dictionary<string, object>();
                foreach (var wzImageProperty in canvasProperty.WzProperties)
                {
                    if (wzImageProperty is WzVectorProperty vectorProperty)
                    {
                        var objects = new Dictionary<object, object>
                        {
                            ["value"] = ((WzVector2) vectorProperty.WzValue).GetV2(),
                            ["type"] = vectorProperty.PropertyType.ToString()
                        };
                        subProperty[wzImageProperty.Name] = objects;
                    }
                    else
                    {
                        var objects = new Dictionary<object, object>
                        {
                            ["value"] = wzImageProperty.WzValue, ["type"] = wzImageProperty.PropertyType.ToString()
                        };
                        subProperty[wzImageProperty.Name] = objects;
                    }
                }

                map["property"] = subProperty;
            }

            return r.SetResult(200, $"获取{filename}文件中 {path} 内容成功!", map);
        }

        public IActionResult GetPng(string id)
        {
            var path = _guid[id];
            var indexOf = path.IndexOf(".wz", StringComparison.Ordinal);
            var filename = path.Substring(0, indexOf + 3);
            path = path.Substring(indexOf + 4);
            var strings = path.Split("\\");
            object wzObject = null;
            var wzFile = _wzFiles[filename];
            for (var i = 0; i < strings.Length; i++)
                wzObject = i == 0 ? wzFile[strings[i]] : ((WzObject) wzObject)?[strings[i]];
            if (!(wzObject is WzCanvasProperty wzCanvasProperty)) return null;
            var bmp = wzCanvasProperty.PngProperty.GetPng(false);
            var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            bmp.Dispose();
            return new FileContentResult(ms.GetBuffer(), "image/png");
        }

        private static string Base64(byte[] arr)
        {
            try
            {
                return Convert.ToBase64String(arr);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static List<string> ListSort(List<string> strList)
        {
            if (strList != null && strList.Count > 0)
                strList.Sort((x, y) => string.Compare(x, y, StringComparison.Ordinal)); //顺序
            return strList;
        }
    }
}