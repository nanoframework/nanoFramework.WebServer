// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Text;
using nanoFramework.Json;

namespace nanoFramework.WebServer.Tests
{
    internal static class FormDataProvider
    {
        public static Stream CreateFormWithParameters()
        {
            string content = @"------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""param1""

value1
------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""param2""

value2
------WebKitFormBoundarySZFRSm4A2LAZPpUu--";

            return new MemoryStream(Encoding.UTF8.GetBytes(content)) { Position = 0 };
        }

        public static Stream CreateFormWithFile(Person person)
        {
            string content = $@"------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""file""; filename=""somefile.json""
Content-Type: application/json

{JsonConvert.SerializeObject(person)}
------WebKitFormBoundarySZFRSm4A2LAZPpUu--";

            return new MemoryStream(Encoding.UTF8.GetBytes(content)) { Position = 0 };
        }

        public static Stream CreateFormWithFiles(Person[] persons)
        {
            string content = $@"------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""file""; filename=""first.json""
Content-Type: application/json

{JsonConvert.SerializeObject(persons[0])}
------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""file""; filename=""second.json""
Content-Type: application/json

{JsonConvert.SerializeObject(persons[1])}
------WebKitFormBoundarySZFRSm4A2LAZPpUu--";

            return new MemoryStream(Encoding.UTF8.GetBytes(content)) { Position = 0 };
        }

        public static Stream CreateFormWithEverything(Person[] persons)
        {
            string content = $@"------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""param1""

value1
------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""param2""

value2
------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""file""; filename=""first.json""
Content-Type: application/json

{JsonConvert.SerializeObject(persons[0])}
------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""file""; filename=""second.json""
Content-Type: application/json

{JsonConvert.SerializeObject(persons[1])}
------WebKitFormBoundarySZFRSm4A2LAZPpUu--";

            return new MemoryStream(Encoding.UTF8.GetBytes(content)) { Position = 0 };
        }

        public static string CreateContent(int size)
        {
            StringBuilder sb = new(size);

            while (sb.Length < size)
                sb.Append("HMLTncevuycfsoiS7cAHhiJq8CI2pTnHhJJb3MfwRB9qlK0VryH8AuJAQzhguP1Z");

            return sb.ToString();
        }

        public static Stream CreateFormWithFile(string file)
        {
            string content = @$"------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""file""; filename=""somefile.json""
Content-Type: application/json

{file}
------WebKitFormBoundarySZFRSm4A2LAZPpUu--";

            return new MemoryStream(Encoding.UTF8.GetBytes(content)) { Position = 0 };
        }

        public static Stream CreateEmptyForm()
        {
            string content = @"------WebKitFormBoundarySZFRSm4A2LAZPpUu



------WebKitFormBoundarySZFRSm4A2LAZPpUu--";

            return new MemoryStream(Encoding.UTF8.GetBytes(content)) { Position = 0 };
        }

        public static Stream CreateInvalidForm()
        {
            //missing the name parameter should fail
            string content = @"------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; invalid=""blah""

value1
------WebKitFormBoundarySZFRSm4A2LAZPpUu--";

            return new MemoryStream(Encoding.UTF8.GetBytes(content)) { Position = 0 };
        }

        public static Person[] CreatePersons()
        {
            return new Person[]
            {
                new()
                {
                    Name = "Chuck Norris",
                    Age = 999
                },
                new()
                {
                    Name = "Darth Vader",
                    Age = 9999
                }
            };
        }
    }

    internal class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
