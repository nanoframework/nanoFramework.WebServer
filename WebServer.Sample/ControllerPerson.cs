using nanoFramework.WebServer;
using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;

namespace nanoFramework.WebServer.Sample
{
    public class Person
    {
        public string First { get; set; }
        public string Last { get; set; }
    }

    public class ControllerPerson
    {
        private static ArrayList _persons = new ArrayList();

        [Route("Person")]
        [Method("GET")]
        public void Get(WebServerEventArgs e)
        {
            var ret = "[";
            foreach (var person in _persons)
            {
                var per = (Person)person;
                ret += $"{{\"First\"=\"{per.First}\",\"Last\"=\"{per.Last}\"}},";
            }
            if (ret.Length > 1)
            {
                ret = ret.Substring(0, ret.Length - 1);
            }
            ret += "]";
            WebServer.OutPutStream(e.Response, $"HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=UTF-8\r\nContent-Length: {ret.Length}\r\nCache-Control: no-cache\r\nConnection: close\r\n\r\n");
            WebServer.OutPutStream(e.Response, ret);
        }

        [Route("Person/Add")]
        [Method("POST")]
        public void AddPost(WebServerEventArgs e)
        {
            // Get the param from the body
            string rawData = new string(Encoding.UTF8.GetChars(e.Content));
            rawData = $"?{rawData}";
            AddPerson(e.Response, rawData);
        }

        [Route("Person/Add")]
        [Method("GET")]
        public void AddGet(WebServerEventArgs e)
        {
            AddPerson(e.Response, e.RawURL);
        }

        private void AddPerson(Socket socket, string url)
        {
            var parameters = WebServer.DecodeParam(url);
            Person person = new Person();
            foreach (var param in parameters)
            {
                if (param.Name.ToLower() == "first")
                {
                    person.First = param.Value;
                }
                else if (param.Name.ToLower() == "last")
                {
                    person.Last = param.Value;
                }
            }
            if ((person.Last != string.Empty) && (person.First != string.Empty))
            {
                _persons.Add(person);
                WebServer.OutputHttpCode(socket, HttpCode.Accepted);
            }
            else
            {
                WebServer.OutputHttpCode(socket, HttpCode.BadRequest);
            }
        }
    }
}
