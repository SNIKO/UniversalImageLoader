
namespace SV.ImageLoader
{
    using SV.ImageLoader.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Reactive.Linq;

    /// <summary>
    ///     Loads images from Internet.
    /// </summary>
    public class WebImageLoader : BaseImageLoader
    {
        #region Fields

        private readonly DownloadManager downloadManager = new DownloadManager();

        #endregion

        #region Methods

        /// <summary>
        ///     Requests loading the image of <paramref name="size"/> located on <paramref name="uri"/>. 
        /// </summary>
        /// <param name="uri">
        ///     An uri of the image to load.
        /// </param>
        /// <param name="size">
        ///     The desired size of the image.
        /// </param>
        /// <returns>
        ///     Returns an observable sequence which contains an image(s) of <paramref name="size"/> loaded using <paramref name="uri"/>.
        /// </returns>
        /// <remarks>
        ///     Override this method to provide concrete logic of loading the image per each <see cref="BaseImageLoader"/>.
        /// </remarks>
        protected override IObservable<ImageInfo> WhenLoadedInternal(Uri uri, Size size)
        {
            var result = Observable.Create<ImageInfo>(observer =>
                {
                    var imageLoadContext = new ImageLoadContext(uri, size, observer);

                    this.downloadManager.EnqeueReqeust(imageLoadContext);

                    return () => this.downloadManager.DequeueRequest(imageLoadContext);
                });

            return result;
        }

        #endregion

        #region Types

        private class DownloadManager
        {
            #region Constants

            private const int MaxConcurrentRequests = 10;

            #endregion

            #region Fields

            private readonly LinkedList<ImageLoadContext> pendingRequests = new LinkedList<ImageLoadContext>();
            private int requestsCounter;

            #endregion

            public void EnqeueReqeust(ImageLoadContext request)
            {
                lock (pendingRequests)
                {
                    if (requestsCounter < MaxConcurrentRequests)
                    {
                        requestsCounter++;

                        this.ProceedReqeustAsync(request);
                    }
                    else
                    {
                        this.pendingRequests.AddLast(request);
                    }
                }
            }

            public bool DequeueRequest(ImageLoadContext request)
            {
                lock (pendingRequests)
                {
                    return this.pendingRequests.Remove(request);
                }
            }

            private async void ProceedReqeustAsync(ImageLoadContext request)
            {
                try
                {
                    var imageResponse = await WebRequest.Create(request.Uri).GetResponseAsync();
                    var imageData = await imageResponse.GetResponseStream().ToArrayAsync();
                    var resizedImageInfo = await imageData.ResizeAsync(request.Size, true);

                    var result = new ImageInfo
                        {
                            Data = resizedImageInfo.Data,
                            Size = resizedImageInfo.Size,
                            Uri = request.Uri,
                            IsFinal = true
                        };

                    request.Observer.OnNext(result);
                    request.Observer.OnCompleted();
                }
                catch (WebException ex)
                {
                    request.Observer.OnError(new ImageLoaderException(string.Format("An error occurred when loading the image on '{0}'", request.Uri), ex));
                }
                finally
                {
                    lock (this.pendingRequests)
                    {
                        if (this.pendingRequests.Any())
                        {
                            var newRequest = this.pendingRequests.First();
                            this.pendingRequests.RemoveFirst();

                            this.ProceedReqeustAsync(newRequest);
                        }
                        else
                        {
                            this.requestsCounter--;
                        }
                    }
                }
            }
        }

        private class ImageLoadContext
        {
            public ImageLoadContext(Uri uri, Size size, IObserver<ImageInfo> observer)
            {
                this.Uri = uri;
                this.Size = size;
                this.Observer = observer;
            }

            public IObserver<ImageInfo> Observer { get; private set; }

            public Uri Uri { get; private set; }

            public Size Size { get; private set; }
        }

        #endregion
    }
}
