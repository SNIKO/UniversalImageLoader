
namespace SV.ImageLoader
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;

    /// <summary>
    ///     The base class for all image loaders.
    /// </summary>
    /// <remarks>
    ///     Defines the base logic of loading images with support. All overriders just need to implement a concrete loading mechanism
    ///     from concrete source (web, filesystem, database, memory, etx.). The errors handling, fallback to other image loaders, combining results are 
    ///     covered by this class.
    /// </remarks>
    public abstract class BaseImageLoader
    {
        #region Fields

        private BaseImageLoader fallbackLoader;

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
        ///     The observable sequence may generate several instances of same image but with different size. For example,
        ///     there cam be a smaller sample of the requested image in the local cache, so this image will be returned immediatly while the image of requested 
        ///     size is loading. Once the image of the desired size is loaded, it will be pushed into the same observable sequence.
        /// </remarks>
        public IObservable<ImageInfo> WhenLoaded(Uri uri, Size size)
        {
            return this.WhenLoaded(uri, size, new Size(0, 0));
        }

        /// <summary>
        ///     Requests loading the image of <paramref name="size"/> located on <paramref name="uri"/>. 
        /// </summary>
        /// <param name="uri">
        ///     An uri of the image to load.
        /// </param>
        /// <param name="size">
        ///     The desired size of the image.
        /// </param>
        /// <param name="minSize">
        ///     Thi mimimum accetable size. 
        /// </param>
        /// <returns>
        ///     Returns an observable sequence which contains an image(s) of <paramref name="size"/> loaded using <paramref name="uri"/>.
        /// </returns>
        /// <remarks>
        ///     If the image cannot be loaded or loaded image has smaller size than requested, then it tries to load the image using fallback
        ///     <see cref="BaseImageLoader"/>. All intermediate images (the images of smaller size than requested), are pushed into observable sequence.
        /// </remarks>
        protected IObservable<ImageInfo> WhenLoaded(Uri uri, Size size, Size minSize)
        {
            var result = Observable.Create<ImageInfo>(observer =>
                {
                    ImageInfo image = null;
                    var compositeDisposable = new CompositeDisposable();

                    compositeDisposable.Add(this.WhenLoadedInternal(uri, size).Subscribe(
                        img =>
                        {
                            observer.OnNext(img);
                            image = img;
                        },
                        err =>
                        {
                            if (fallbackLoader != null)
                            {
                                compositeDisposable.Add(this.fallbackLoader.WhenLoaded(uri, size, minSize)
                                    .Do(this.OnFallbackImageLoaded)
                                    .Subscribe(observer));
                            }
                            else
                            {
                                observer.OnError(err);
                            }
                        },
                        () =>
                        {
                            if ((image == null || image.IsFinal == false) && fallbackLoader != null)
                            {
                                compositeDisposable.Add(this.fallbackLoader.WhenLoaded(uri, size, image == null ? minSize : image.Size)
                                    .Do(this.OnFallbackImageLoaded)
                                    .Subscribe(observer));
                            }
                            else
                            {
                                observer.OnCompleted();
                            }
                        }));

                    return () => { compositeDisposable.Dispose(); };
                });

            return result;
        }

        /// <summary>
        ///     Specifies the fallback <see cref="BaseImageLoader"/> that can be used if the image cannot be loaded from this <see cref="BaseImageLoader"/>. 
        /// </summary>
        /// <param name="fallbackImageLoader">
        ///     The fallback image loader to use.
        /// </param>
        /// <returns>
        ///     Returns instance of current <see cref="BaseImageLoader"/>.
        /// </returns>
        public BaseImageLoader AsFallbackUse(BaseImageLoader fallbackImageLoader)
        {
            if (fallbackImageLoader == null)
            {
                throw new ArgumentNullException("fallbackImageLoader");
            }

            this.fallbackLoader = fallbackImageLoader;

            return this;
        }

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
        protected abstract IObservable<ImageInfo> WhenLoadedInternal(Uri uri, Size size);

        /// <summary>
        ///     Notifies that the image is loaded from fallback <see cref="BaseImageLoader"/>.
        /// </summary>
        /// <param name="image">
        ///     The loaded image.
        /// </param>
        /// <remarks>
        ///     Use this method to save/cache the loaded image. 
        /// </remarks>
        protected virtual void OnFallbackImageLoaded(ImageInfo image)
        {
        }

        #endregion
    }
}
