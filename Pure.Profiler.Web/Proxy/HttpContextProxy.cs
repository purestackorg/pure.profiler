//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Web;
//using System.Web.UI;

//namespace Pure.Profiler.Web.Proxy
//{
//    public class HttpContextProxy
//    {
//        //public static void SetHttpContextProxy()
//        //{
//        //    if (HttpContext.Current != null)
//        //    {
//        //        var response = HttpContext.Current.Response;
//        //        response.BufferOutput = true;
//        //        response.Output = new TextWriterProxy();
//        //        HttpContext.Current = new HttpContext(HttpContext.Current.Request, response);
//        //    }
//        //}

//        //protected override void Render(HtmlTextWriter writer)
//        //{


//        //    base.Render(writer);

//        //    //Response.Output其实就是一个HttpWriter,Response.OutputStream其实就是HttpResponseStream,Object.ReferenceEquals (Response.Output,(Response.OutputStream as HttpResponseStream)._writer)为true

//        //    BindingFlags bind = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.GetField;
//        //    //因为HttpWriter._charBuffer这个字符数组的长度是1024，所以推测，一旦某一段字符串超过1024就会创建一个IHttpResponseElement，然后加入到HttpWriter._buffers,HttpWriter._buffers的每一个元素都实现了IHttpResponseElement接口,具体类型可能是HttpResponseUnmanagedBufferElement，HttpSubstBlockResponseElement等类型
//        //    ArrayList arr = (ArrayList)Response.Output.GetType().GetField("_buffers", bind).GetValue(Response.Output);

//        //    Assembly systemWeb = Assembly.Load("System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
//        //    Type type = systemWeb.GetType("System.Web.IHttpResponseElement");
//        //    MethodInfo method = type.GetMethod("GetBytes");
//        //    StringBuilder sb = new StringBuilder(5000);
//        //    //遍历每一个buffer，获取buffer里存储的字符数组，然后转换为字符串
//        //    for (int i = 0; i < arr.Count; i++)
//        //    {
//        //        byte[] buffer = (byte[])method.Invoke(arr[i], null);
//        //        //使用当前编码得出已经存储到HttpWriter._buffers中的字符串
//        //        sb.Append(Response.ContentEncoding.GetString(buffer));
//        //    }
//        //    //获取HttpWriter的字符数组缓冲区
//        //    char[] charBuffer = (char[])Response.Output.GetType().GetField("_charBuffer", bind).GetValue(Response.Output);
//        //    int charBufferLength = (int)Response.Output.GetType().GetField("_charBufferLength", bind).GetValue(Response.Output);
//        //    int charBufferFree = (int)Response.Output.GetType().GetField("_charBufferFree", bind).GetValue(Response.Output);
//        //    //charBufferLength - charBufferFree 等于字符数组缓冲区已经使用的字符数量
//        //    for (int i = 0; i < charBufferLength - charBufferFree; i++)
//        //    {
//        //        sb.Append(charBuffer[i]);
//        //    }

//        //}

//        public static string GetResponseText(HttpResponse Response)
//        {
            

//            //Response.Output其实就是一个HttpWriter,Response.OutputStream其实就是HttpResponseStream,Object.ReferenceEquals (Response.Output,(Response.OutputStream as HttpResponseStream)._writer)为true

//            BindingFlags bind = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.GetField;
//            //因为HttpWriter._charBuffer这个字符数组的长度是1024，所以推测，一旦某一段字符串超过1024就会创建一个IHttpResponseElement，然后加入到HttpWriter._buffers,HttpWriter._buffers的每一个元素都实现了IHttpResponseElement接口,具体类型可能是HttpResponseUnmanagedBufferElement，HttpSubstBlockResponseElement等类型
//            ArrayList arr = (ArrayList)Response.Output.GetType().GetField("_buffers", bind).GetValue(Response.Output);

//            Assembly systemWeb = Assembly.Load("System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
//            Type type = systemWeb.GetType("System.Web.IHttpResponseElement");
//            MethodInfo method = type.GetMethod("GetBytes");
//            StringBuilder sb = new StringBuilder(5000);
//            //遍历每一个buffer，获取buffer里存储的字符数组，然后转换为字符串
//            for (int i = 0; i < arr.Count; i++)
//            {
//                byte[] buffer = (byte[])method.Invoke(arr[i], null);
//                //使用当前编码得出已经存储到HttpWriter._buffers中的字符串
//                sb.Append(Response.ContentEncoding.GetString(buffer));
//            }
//            //获取HttpWriter的字符数组缓冲区
//            char[] charBuffer = (char[])Response.Output.GetType().GetField("_charBuffer", bind).GetValue(Response.Output);
//            int charBufferLength = (int)Response.Output.GetType().GetField("_charBufferLength", bind).GetValue(Response.Output);
//            int charBufferFree = (int)Response.Output.GetType().GetField("_charBufferFree", bind).GetValue(Response.Output);
//            //charBufferLength - charBufferFree 等于字符数组缓冲区已经使用的字符数量
//            //for (int i = 0; i < charBufferLength - charBufferFree; i++)
//            //{
//            //    sb.Append(charBuffer[i]);
//            //}
//            if (charBuffer != null)
//            {
//                foreach (var item in charBuffer)
//                {
//                    sb.Append(item);
//                }
//            }
           


//            return sb.ToString();
//        }

//    }
//}