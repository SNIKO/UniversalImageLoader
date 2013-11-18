## Universal Image Loader

A universal instrument for parallel images loading in background.

The next platforms are supported:
* .NET 4.5
* WinRT
* Windows Phone 8

### Features

* the library is fully asynchronous;
* supports multiple level cache infrosrtucture;
* has modular structure which allows you to constact image loading component to fit your needs;
* provides wide opportunities to customize and extend functionality.

### Prerequisites

In order to build sources, be sure that next prerequisites are installed:

* .NET Framework 4.5
* Windows Phone 8 SDK
* Reactive Extensions v2.0.20823 and upper

### Installation

To start using the library, please follow next steps:

* Download sources 
<code>git clone git://github.com/SNIKO/UniversalImageLoader.git</code>

* Build <code>\sources\UniversalImageLoader.sln</code> solution;

* Run <code>RegisterBinaries.bat</code> script. It will register binaries into GAC to make them available in "Reference Manager";

* Add <code>SV.UniversalImageLoader.dll</code> assembly to your project and enjoy using it.

### Design

The library consists of independet image loaders which can load images from different sources. The next loaders are implemented:

* WebImageLoader - loads image from the Internet
* FileSystemImageLoader - loads image from file system (available only for win app)
* IsolatedStorageImageLoader - loads image from IsolatedStorage (available only for windows phone)
* MemoryImageLoader - loads image from memory
* StaticImageLoader - loads static image (the image should be specified during configuration)

All these loaders are fully configurable. You can configure generic settings as well as some specific setting. For example, for WebImageLoader you can specify how many images to load concurrently, for FileSystemImageLoader you can specify the folder where to store images, etc. Also, if a loader is a cache based loader (i.e. it stores images somewhere), you can specify a cache cleanup strategy which will decide which images to delete when the cache is full. You can implement your own cache cleanup strategy buy overriding <code>CacheCleanupStrategy</code>. You can also implement your own image loader by overriding <code>BaseImageLoader</code>.

Separately, all these loaders are useless. However, the library allows you to chain these loaders into one big, flexible and powerful loader:

![alt tag](https://raw.github.com/SNIKO/UniversalImageLoader/master/doc/concept.png)

When you request the first loader in the chain for a some image with concrete size, the loader will try to load this image from it's own storage, and depedning on whether the image exists, several situations are possible:

1. Image not found. The loader will request the image from fallback loader in chain. 
2. Image is found but has smaller size than requested. The loader will return this image immediatly and in parallel will request image of requested size from fallback loader in chain.
3. Image is found but has larger size than requested. The loader will resize it to requested size and will return it. No need to bother next loaders in chain.

When a fallback loader returns image to a current loader, the current loader saves it into its own storage and pushes it into result. As you can see, when you request image from loader, you may receive several instances of same image but with different size (from small size to requested size). Such approach allows you to receive the result as fast as possible - we can display same image with smaller size from IsolatedStorage (if it exists there), while original image is loading from the Internet.

### Usage

The most common image loader should have memory and file system cache. The following code demonstrates how to configure image loader in such way:

```csharp

var imageLoader =
    new MemoryImageLoader()
        .WithCacheSize(100.Items())
        .WithCacheCleanupStrategy(CacheCleanupStrategy.RarelyUsedItemsRemoveFirst)
        .AsFallbackUse(
            new IsolatedStorageImageLoader()
                .WithDirectory("ImagesCache")
                .WithCacheSize(100.Megabytes())
                .WithCacheCleanupStrategy(CacheCleanupStrategy.SmallInstancesOfSameImageRemoveFirst)
                .AsFallbackUse(
                    new WebImageLoader()));

```

Now lets assume you need to display some blank image while requested image is loading. It is easy to achieve this, all you need to do is to add StaticImageLoader at first position in the chain:

```csharp
var imageLoader =
    new StaticImageLoader()
        .WithImage(new Uri(@"Assets/BusyIndicator.png", UriKind.Relative))
        .AsFallbackUse(
            new MemoryImageLoader()
                .WithCacheSize(100.Items())
                .WithCacheCleanupStrategy(CacheCleanupStrategy.RarelyUsedItemsRemoveFirst)
                .AsFallbackUse(
                    new IsolatedStorageImageLoader()
                        .WithDirectory("ImagesCache")
                        .WithCacheSize(100.Megabytes())
                        .WithCacheCleanupStrategy(CacheCleanupStrategy.SmallInstancesOfSameImageRemoveFirst)
                        .AsFallbackUse(
                            new WebImageLoader())));
```

We can move the StaticImageLoader to the end of chain. It will display some default image if the original image is not exist or cannot be downloaded (for example due to Internet connection lost):

```csharp
var imageLoader =
    new MemoryImageLoader()
        .WithCacheSize(100.Items())
        .WithCacheCleanupStrategy(CacheCleanupStrategy.RarelyUsedItemsRemoveFirst)
        .AsFallbackUse(
            new IsolatedStorageImageLoader()
                .WithDirectory("ImagesCache")
                .WithCacheSize(100.Megabytes())
                .WithCacheCleanupStrategy(CacheCleanupStrategy.SmallInstancesOfSameImageRemoveFirst)
                .AsFallbackUse(
                    new WebImageLoader()
                        .AsFallbackUse(
                            new StaticImageLoader()
                                .WithImage(new Uri(@"Assets/default.png", UriKind.Relative)))));

```

You can use as many loaders as you want and combine them in any way you wish. You can implement your own image loaders by overriding the <code>BaseImageLoader</code> or <code>CacheImageLoader</code>.

We know how to configure loaders, now lets see how to load images using them. To load image, you should call <code>WhenLoaded</code> method which will return <code>IObservable<ImageResult></code> sequence. As soon as the image is loaded, it will be pushed into this observable sequence. All you need to do is to subscribe to this sequence and handle images pushed into it.

Remember, you can receive several instances of same image. For example, you requested image with size 100x100. But the loader found the same image but with size 50x50 in its memory cache. In this case you will receive 50x50 image immediatly and 100x100 image as soon as it is loaded.

Here is an example of image loading:

```csharp
var imageUri = "http://2.bp.blogspot.com/-IM--ZDFZ5-M/UTsWRJua23I/AAAAAAAAGsE/bJXnjTreTmg/s1600/amber-heard-hot.jpg";

imageLoader.WhenLoaded(new Uri(imageUri), new Size(1024, 768)).Subscribe(
    imageInfo =>
        {
            // Handle imageInfo.Data
        });
```

It is possible to cancel image load request. To do this, you need to dispose <code>IDisposable</code> object returned by <code>Subscribe</code>:

```csharp
var imageUri = "http://wallpaper95.com/w/78/hot-blonde-yacht-sailing-548-hd-1920x1080.jpg";

var imageLoadSubscription = imageLoader.WhenLoaded(new Uri(imageUri), new Size(1920, 1080)).Subscribe(
    imageInfo =>
    {
        // Handle imageInfo.Data
    });

...

imageLoadSubscription.Dispose();

```

## Copyright

Copyright (c) 2013 Sergii Vashchyshchuk
