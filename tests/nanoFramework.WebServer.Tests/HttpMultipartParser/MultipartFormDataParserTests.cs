// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.Json;
using nanoFramework.TestFramework;
using nanoFramework.WebServer.HttpMultipartParser;
using System;
using System.IO;

namespace nanoFramework.WebServer.Tests
{
    [TestClass]
    public class MultipartFormDataParserTests
    {
        [TestMethod]
        public void FormWithParametersTest()
        {
            Stream stream = FormDataProvider.CreateFormWithParameters();

            MultipartFormDataParser parser = MultipartFormDataParser.Parse(stream);

            Assert.IsNotNull(parser);

            ParameterPart[] parameters = parser.Parameters;
            Assert.IsTrue(parameters.Length == 2);
            Assert.IsTrue(parameters[0].Name == "param1" && parameters[0].Data == "value1");
            Assert.IsTrue(parameters[1].Name == "param2" && parameters[1].Data == "value2");
        }

        [TestMethod]
        public void FormWithFileTest()
        {
            Person[] persons = FormDataProvider.CreatePersons();

            Stream stream = FormDataProvider.CreateFormWithFile(persons[0]);

            MultipartFormDataParser parser = MultipartFormDataParser.Parse(stream);

            Assert.IsNotNull(parser);

            ParameterPart[] parameters = parser.Parameters;
            Assert.IsTrue(parameters.Length == 0);

            FilePart[] files = parser.Files;
            Assert.IsTrue(files.Length == 1);
            ValidateFile(files[0], "somefile.json", persons[0].Name, persons[0].Age);
        }

        [TestMethod]
        public void FormWithMultipleFilesTest()
        {
            Person[] persons = FormDataProvider.CreatePersons();

            Stream stream = FormDataProvider.CreateFormWithFiles(persons);

            MultipartFormDataParser parser = MultipartFormDataParser.Parse(stream);

            Assert.IsNotNull(parser);

            ParameterPart[] parameters = parser.Parameters;
            Assert.IsTrue(parameters.Length == 0);

            FilePart[] files = parser.Files;
            Assert.IsTrue(files.Length == 2);
            ValidateFile(files[0], "first.json", persons[0].Name, persons[0].Age);
            ValidateFile(files[1], "second.json", persons[1].Name, persons[1].Age);
        }

        [TestMethod]
        public void FormWithEverythingTest()
        {
            Person[] persons = FormDataProvider.CreatePersons();

            Stream stream = FormDataProvider.CreateFormWithEverything(persons);

            MultipartFormDataParser parser = MultipartFormDataParser.Parse(stream);

            Assert.IsNotNull(parser);

            ParameterPart[] parameters = parser.Parameters;
            Assert.IsTrue(parameters.Length == 2);
            Assert.IsTrue(parameters[0].Name == "param1" && parameters[0].Data == "value1");
            Assert.IsTrue(parameters[1].Name == "param2" && parameters[1].Data == "value2");

            FilePart[] files = parser.Files;
            Assert.IsTrue(files.Length == 2);
            ValidateFile(files[0], "first.json", persons[0].Name, persons[0].Age);
            ValidateFile(files[1], "second.json", persons[1].Name, persons[1].Age);
        }

        [TestMethod]
        public void FormWithLargeFileTest()
        {
            string fileIn = FormDataProvider.CreateContent(4096);
            Stream stream = FormDataProvider.CreateFormWithFile(fileIn);

            MultipartFormDataParser parser = MultipartFormDataParser.Parse(stream);

            ParameterPart[] parameters = parser.Parameters;
            Assert.IsTrue(parameters.Length == 0);

            FilePart[] files = parser.Files;
            Assert.IsTrue(files.Length == 1);

            using var sr = new StreamReader(files[0].Data);
            string fileOut = sr.ReadToEnd();

            Assert.AreEqual(fileIn, fileOut);
        }

        [TestMethod]
        public void EmptyFormTest()
        {
            Stream stream = FormDataProvider.CreateEmptyForm();

            MultipartFormDataParser parser = MultipartFormDataParser.Parse(stream, ignoreInvalidParts: true);
            Assert.IsNotNull(parser);

            Assert.ThrowsException(typeof(MultipartFormDataParserException), () => MultipartFormDataParser.Parse(stream));
        }

        [TestMethod]
        public void InvalidFormTest()
        {
            Stream stream = FormDataProvider.CreateInvalidForm();

            MultipartFormDataParser parser = MultipartFormDataParser.Parse(stream, ignoreInvalidParts: true);
            Assert.IsNotNull(parser);

            Assert.ThrowsException(typeof(MultipartFormDataParserException), () => MultipartFormDataParser.Parse(stream));
        }

        private void ValidateFile(FilePart file, string filename, string personName, int personAge)
        {
            Assert.IsTrue(file.FileName == filename);
            StreamReader sr = new(file.Data);
            string content = sr.ReadToEnd();

            var person = JsonConvert.DeserializeObject(content, typeof(Person)) as Person;
            Assert.IsNotNull(person);
            Assert.IsTrue(person.Name == personName && person.Age == personAge);
        }
    }
}
