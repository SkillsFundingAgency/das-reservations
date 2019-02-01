using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Reservations.Infrastructure.AzureConfigurationProvider;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.Configuration
{
    public class WhenParsingConfigurationFromStorage
    {
        private StorageConfigParser _storageConfigParser;

        [SetUp]
        public void Arrange()
        {
            _storageConfigParser = new StorageConfigParser();
        }

        [Test]
        public void Then_The_Data_Is_Not_Added_To_The_Dictionary_If_Not_Valid()
        {
            //Arrange
            var configItem = new ConfigurationItem { Data = "{" };

            //Act Assert
            Assert.Throws<JsonReaderException>(()=>_storageConfigParser.ParseConfig(configItem));

            
        }


        [Test]
        public void Then_The_Data_Is_Not_Added_To_The_Dictionary_It_Does_Not_Exist()
        {
            //Arrange
            var configItem = new ConfigurationItem { Data = "{}" };

            //Act
            var actual = _storageConfigParser.ParseConfig(configItem);

            //Assert
            Assert.IsNotNull(actual);
        }

        [Test]
        public void Then_The_Properties_Are_Correctly_Added_To_The_Dictionary_For_Simple_Data_Structures()
        {
            //Arrange
            var configItem = new ConfigurationItem { Data = "{\"Configuration\":{\"Item1\":\"Value1\"}}" };

            //Act
            var actual = _storageConfigParser.ParseConfig(configItem);

            //Assert
            Assert.IsNotNull(actual);
            Assert.IsNotEmpty(actual);
            Assert.Contains(new KeyValuePair<string,string>("Configuration:Item1", "Value1"), actual);
        }

        [Test]
        public void Then_Complex_Configuration_Structures_Are_Added_To_The_Dictionary()
        {
            //Arrange
            var configItem = new ConfigurationItem { Data = "{\"Configuration\":{\"Item1\":\"Value1\",\"Item2\":\"Value2\"}, \"Configuration2\":{\"Item3\":\"Value3\"}}" };

            //Act
            var actual = _storageConfigParser.ParseConfig(configItem);

            //Assert
            Assert.IsNotNull(actual);
            Assert.IsNotEmpty(actual);
            Assert.AreEqual(3,actual.Count);
            Assert.Contains(new KeyValuePair<string, string>("Configuration:Item1", "Value1"), actual);
            Assert.Contains(new KeyValuePair<string, string>("Configuration:Item2", "Value2"), actual);
            Assert.Contains(new KeyValuePair<string, string>("Configuration2:Item3", "Value3"), actual);
        }
    }
}
