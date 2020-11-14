﻿﻿namespace MsHelper.Controllers
{
    public class R
    {
        public int Code { get; set; } = 400;

        public string Msg { get; set; } = "Error!";

        public object Data { get; set; }

        public R SetResult(int code, string msg, object data)
        {
            Code = code;
            Msg = msg;
            Data = data;
            return this;
        }

        public R SetResult(int code, string msg)
        {
            Code = code;
            Msg = msg;
            return this;
        }

        public static R Init() => new R();

        private R()
        {
        }
    }
}