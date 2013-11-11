
namespace SV.ImageLoader.Test.Extensions
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class CollectionExtensions
    {
        public static void CheckContainsSameItemsAs<T>(this IEnumerable<T> collection, IEnumerable<T> expectedCollection)
        {
            Assert.AreEqual(expectedCollection.Count(), collection.Count(), "Items count");

            var notFoundItems = expectedCollection.ToList().Where(i => collection.Contains(i) == false).ToList();
            var foundButNotExpectedItems = collection.ToList().Where(i => expectedCollection.Contains(i) == false).ToList();

            var messageBuilder = new StringBuilder();

            if (notFoundItems.Any())
            {
                messageBuilder.AppendLine("The following items were not found:");
                messageBuilder.AppendLine(string.Join(Environment.NewLine, notFoundItems));
            }

            if (foundButNotExpectedItems.Any())
            {
                messageBuilder.AppendLine();
                messageBuilder.AppendLine("The following items were found but not expected:");
                messageBuilder.AppendLine(string.Join(Environment.NewLine, foundButNotExpectedItems));
            }

            var message = messageBuilder.ToString();
            if (string.IsNullOrEmpty(message) == false)
            {
                Assert.Fail(message);
            }
        }
    }
}
