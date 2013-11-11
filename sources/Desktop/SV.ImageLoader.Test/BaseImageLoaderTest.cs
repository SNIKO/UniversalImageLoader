
namespace SV.ImageLoader.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SV.ImageLoader.Extensions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;

    [TestClass]
    public abstract class BaseImageLoaderTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenLoaded_UriIsNull_ThrowException()
        {
            var loaderToTest = this.GetImageLoaderInstance();
            loaderToTest.WhenLoaded(null, new Size(10, 10));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenLoaded_SizeIsEmpty_ThrowException()
        {
            var loaderToTest = this.GetImageLoaderInstance();
            loaderToTest.WhenLoaded(new Uri("http://www.mclaren.com"), default(Size));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenLoaded_SizeIsInvalid_ThrowException()
        {
            var loaderToTest = this.GetImageLoaderInstance();
            loaderToTest.WhenLoaded(new Uri("http://www.mclaren.com"), new Size(0, 0));
        }

        protected abstract BaseImageLoader GetImageLoaderInstance();

        #region Mocks

        /// <summary>
        ///     The mock object for <see cref="BaseImageLoader"/>.
        /// </summary>
        internal class ImageLoaderMock : BaseImageLoader
        {
            private readonly List<Expectation> expectations = new List<Expectation>();

            /// <summary>
            ///     Setups an expectation for a <see cref="BaseImageLoader.WhenLoaded"/> method call.
            /// </summary>
            /// <param name="uri">
            ///     The expected uri in the call.
            /// </param>
            /// <param name="desiredSize">
            ///     The expected image desired size in the call.
            /// </param>
            /// <param name="returnSize">
            ///     The size of image to return as result.
            /// </param>
            /// <param name="times">
            ///     The number of times the <see cref="BaseImageLoader.WhenLoaded"/> with <paramref name="uri"/> and <paramref name="desiredSize"/> is expected to be called.
            /// </param>
            public void Setup(string uri, Size desiredSize, Size returnSize, int times)
            {
                var imageData = GetImageData(returnSize);

                expectations.Add(new Expectation
                    {
                        Uri = new Uri(uri),
                        Size = desiredSize,
                        Result = new ImageInfo(new Uri(uri), returnSize, imageData, true),
                        ExpectedTimes = times
                    });
            }

            /// <summary>
            ///     Checks whether all expectation configured via <see cref="Setup"/> are called of expected number times. Otherwise, throws assert.
            /// </summary>
            public void Verify()
            {
                for (var i = 0; i < expectations.Count; i++)
                {
                    Assert.AreEqual(expectations[i].ExpectedTimes, expectations[i].ActualTimes, string.Format("WhenLoaded('{0}', {1}) times called", expectations[i].Uri, expectations[i].Size));
                }
            }

            /// <summary>
            ///     Clears all expectation.
            /// </summary>
            public void Reset()
            {
                this.expectations.Clear();
            }

            protected override IObservable<ImageInfo> WhenLoadedInternal(Uri uri, Size size)
            {
                var sequence = Observable.Create<ImageInfo>(observer =>
                {
                    var expectation = this.expectations.FirstOrDefault(e => e.Size == size && e.Uri == uri);
                    if (expectation != null)
                    {
                        expectation.ActualTimes++;
                        observer.OnNext(expectation.Result);
                    }

                    observer.OnCompleted();
                    return () => { };
                });

                return sequence;
            }

            private byte[] GetImageData(Size imageSize)
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var fileStream = new FileStream("..\\..\\TestData\\image.jpg", FileMode.Open))
                    {
                        fileStream.CopyTo(memoryStream);
                    }

                    return memoryStream.ToArray().ResizeAsync(imageSize, false).GetAwaiter().GetResult().Data;
                }
            }

            private class Expectation
            {
                public Uri Uri { get; set; }

                public Size Size { get; set; }

                public ImageInfo Result { get; set; }

                public int ExpectedTimes { get; set; }

                public int ActualTimes { get; set; }
            }
        }

        #endregion
    }
}
