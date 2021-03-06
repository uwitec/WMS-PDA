﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace ZC.HttpUtils
{
    public class HttpHelp
    {
        static AuthenticateResultModel Token;

        static string BaseUrl
        {
            get { return IniFilesHelp.Ini.GetParam("BaseUrl", "http://127.0.0.1"); }
        }

        public static bool Login(string uid, string pwd)
        {
            try
            {
                var res = RequsetAbp<AuthenticateResultModel>(RequsetMethod.POST, "/api/TokenAuth/Authenticate", new AuthenticateModel() { UserNameOrEmailAddress = uid, Password = pwd, RememberClient = true }, new List<HttpHeader>() { new HttpHeader() { Key = "Abp.TenantId", Value = IniFilesHelp.Ini.GetParam("TenantId", "") } }, true);

                if (res.Success)
                {
                    Token = res.Result;
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static List<Tenant> GetTenantByKeyName(string keyName)
        {
            try
            {
                return RequsetAbp<List<Tenant>>(RequsetMethod.GET, "/api/services/app/Account/GetTenantByKeyName?keyName=" + keyName, null, null, true).Result;
            }
            catch
            {
                return null;
            }

        }

        public static T Post<T>(string url, object data) where T : class, new()
        {
            return Requset<T>(RequsetMethod.POST, url, data, null, false);
        }

        public static ReresultDto<T> PostAbp<T>(string url, object data) where T : class, new()
        {
            return RequsetAbp<T>(RequsetMethod.POST, url, data, null, false);
        }


        public static T Get<T>(string url, object data) where T : class, new()
        {
            return Requset<T>(RequsetMethod.GET, url, data, null, false);
        }

        public static ReresultDto<T> GetAbp<T>(string url, object data) where T : class, new()
        {
            return RequsetAbp<T>(RequsetMethod.GET, url, data, null, false);
        }

        public static ReresultDto<T> RequsetAbp<T>(RequsetMethod method, string url, object data, List<HttpHeader> headers, bool noToken) where T : class, new()
        {
            return Requset<ReresultDto<T>>(method, url, data, headers, noToken);
        }

        public static T Requset<T>(RequsetMethod method, string url, object data, List<HttpHeader> headers, bool noToken) where T : class, new()
        {

            Encoding encoding = Encoding.UTF8;

            // 如果直接精确到地址不用加服务器基础地址
            if (url.ToLower().Contains("https://") || url.Contains("http://"))
            {

            }
            else
            {
                url = BaseUrl + url;
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = method.ToString();

            // 添加token
            if (!noToken)
            {
                request.Headers.Add("Authorization", "Bearer " + Token.AccessToken);
            }

            // 添加自定义头
            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // 统一这一种格式数据
            request.ContentType = "application/json;charset=utf-8";

            // 有数据正文，发送数据正文
            if (data != null)
            {
                var jsonData = Common.SerializeJSON(data);
                byte[] requsetData;
                requsetData = Encoding.UTF8.GetBytes(jsonData);
                request.ContentLength = requsetData.Length;

                using (Stream reStream = request.GetRequestStream())
                {
                    reStream.Write(requsetData, 0, requsetData.Length);
                }
            }

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
            }


            using (response)
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    var resSrt = reader.ReadToEnd();
                    return Common.DeserializeJSON<T>(resSrt);
                }
            }
        }



        public static void PostAsync<T>(string url, object data, Action<T> onResponse) where T : class, new()
        {
            RequsetAsync<T>(RequsetMethod.POST, url, data, null, false, onResponse);
        }

        public static void PostAbpAsync<T>(string url, object data, Action<ReresultDto<T>> onResponse) where T : class, new()
        {
            RequsetAbpAsync<T>(RequsetMethod.POST, url, data, null, false, onResponse);
        }


        public static void GetAsync<T>(string url, object data, Action<T> onResponse) where T : class, new()
        {
            RequsetAsync<T>(RequsetMethod.GET, url, data, null, false, onResponse);
        }

        public static void GetAbpAsync<T>(string url, object data, Action<ReresultDto<T>> onResponse) where T : class, new()
        {
            RequsetAbpAsync<T>(RequsetMethod.GET, url, data, null, false, onResponse);
        }

        public static void RequsetAbpAsync<T>(RequsetMethod method, string url, object data, List<HttpHeader> headers, bool noToken, Action<ReresultDto<T>> onResponse) where T : class, new()
        {
            RequsetAsync<ReresultDto<T>>(method, url, data, headers, noToken, onResponse);
        }

        public static void RequsetAsync<T>(RequsetMethod method, string url, object data, List<HttpHeader> headers, bool noToken, Action<T> onResponse) where T : class, new()
        {

            try
            {

                Encoding encoding = Encoding.UTF8;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BaseUrl + url);

                // 如果直接精确到地址不用加服务器基础地址
                if (url.ToLower().Contains("https://") || url.Contains("http://"))
                {
                    request = (HttpWebRequest)WebRequest.Create(url);
                }

                request.Method = method.ToString();

                // 添加token
                if (!noToken)
                {
                    request.Headers.Add("Authorization", "Bearer " + Token.AccessToken);
                }

                // 添加自定义头
                if (headers != null && headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                // 统一这一种格式数据
                request.ContentType = "application/json;charset=utf-8";




                request.BeginGetRequestStream(new AsyncCallback((result) =>
                {
                    HttpWebRequest req = (HttpWebRequest)result.AsyncState;

                    // 有数据正文，发送数据正文
                    if (data != null)
                    {
                        var jsonData = Common.SerializeJSON(data);
                        Stream postStream = req.EndGetRequestStream(result);
                        byte[] requsetData;
                        requsetData = Encoding.UTF8.GetBytes(jsonData);
                        postStream.Write(requsetData, 0, requsetData.Length);
                        postStream.Close();
                    }

                    req.BeginGetResponse(new AsyncCallback(responseResult =>
                    {
                        HttpWebRequest req1 = (HttpWebRequest)responseResult.AsyncState;

                        if (responseResult.IsCompleted)
                        {
                            var webResponse = req1.EndGetResponse(responseResult);
                            using (var stream = webResponse.GetResponseStream())
                            {
                                using (var read = new StreamReader(stream))
                                {
                                    if (onResponse != null)
                                    {
                                        onResponse.Invoke(Common.DeserializeJSON<T>(read.ReadToEnd()));
                                    }
                                }
                            }
                        }
                    }), req);
                }), request);

            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }
    }


    public enum RequsetMethod
    {
        GET = 0, POST, DELETE, PUT
    }

    public class ReresultDto<T>
    {
        public T Result { get; set; }

        public string TargetUrl { get; set; }

        public bool Success { get; set; }

        public ErrorInfo Error { get; set; }

        public bool UnAuthorizedRequest { get; set; }

        public bool __abp { get; set; }

    }

    public class ErrorInfo
    {
        /// <summary>
        /// Error code.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Error details.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Validation errors if exists.
        /// </summary>
        public ValidationErrorInfo[] ValidationErrors { get; set; }
    }


    public class ValidationErrorInfo
    {
        /// <summary>
        /// Validation error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Relate invalid members (fields/properties).
        /// </summary>
        public string[] Members { get; set; }
    }

    public class Tenant
    {
        public string Name { get; set; }

        public string TenantId { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class AuthenticateResultModel
    {
        public string AccessToken { get; set; }
        public string EncryptedAccessToken { get; set; }
        public int ExpireInSeconds { get; set; }
        public string UserId { get; set; }
    }

    public class AuthenticateModel
    {
        public string UserNameOrEmailAddress { get; set; }
        public string Password { get; set; }
        public bool RememberClient { get; set; }
    }

    public class HttpHeader
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class PagedResultDto<T>
    {
        public int TotalCount { get; set; }
        public List<T> Items { get; set; }
    }

    public class PagedResultRequestMESDto
    {
        public int SkipCount { get; set; }
        public int MaxResultCount { get; set; }


        public List<RequestMESDto> RequestMESDtos { get; set; }

        public string SortName { get; set; }

        public bool Desc { get; set; }

    }

    public class RequestMESDto
    {
        public string PropertyName { get; set; }

        public Operation Operation { get; set; }

        public object QueryValue { get; set; }

        public LinkOperation LinkOperation { get; set; }

        public List<RequestMESDto> RequestMESDtos { get; set; }
    }

    public enum LinkOperation
    {
        And = 0,
        Or
    }

    /// <summary>
    /// 操作符
    /// </summary>
    public enum Operation
    {
        Equal = 0,
        NotEqual,
        Contains,
        StartsWith,
        EndsWith,
        NotContains,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        Empry,
        NotEmpty,
        Null,
        NotNull,
        RegEx
    }

}

