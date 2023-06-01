using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace JsonFlatFileDataStore.Test
{
    public class MockedFileSystemTest
    {
        [Fact]
        public async Task WriteToFileTest()
        {
            var filepath = @"c:\data.json";

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filepath, new MockFileData("{}") },
            });

            var store = new DataStore(fileSystem, "c:\\data.json");

            var collection = store.GetCollection<PrivateOwner>("PrivateOwner");
            Assert.Equal(0, collection.Count);

            await collection.InsertOneAsync(new PrivateOwner { 
                FirstName = "Jimmy", 
                OwnerLongTestProperty = "UT" });
            Assert.Equal(1, collection.Count);

            var json = fileSystem.File.ReadAllText(filepath);

            Assert.Contains("privateOwner", json);
            Assert.Contains("ownerLongTestProperty", json);

        }
    }
}
