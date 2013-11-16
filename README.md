## Universal Image Loader

An universal instrument for asynchronous image loading.

The next platforms are supported:
* .NET 4.5
* WinRT
* Windows Phone 8

### Features

* The library is fully asynchronous
* caching loaded images in memory, file system, isolated storage
* modular structure which allows you to constact image loading component to fit your needs
* wide opportunities to customize and extend functionality

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

### Concept

The library consist of independet ImageLoaders which can load images from different sources. Currently, the next loaders are implemented:

* WebImageLoader
* FileSystemImageLoader
* IsolatedStorageImageLoader
* MemoryImageLoader
* StaticImageLoader

All this loaders are fully configurable. It is possible to configure generic settings as well as some specific setting. For example, for WebImageLoader you can specify how many images to load concurrently, for FileSystemImageLoader you can specify the folder where to store images, etc. Also, if a loader is cache based loader (i.e. it stores images somewhere), you can specify cache cleanup strategy which will decide which images to delete to free requested space. You can implement your own cache cleanup strategy buy overriding <code>CacheCleanupStrategy</code>. Of course, you can also implement your own ImageLoader by overriding <code>BaseImageLoader</code>.

Separately, all this loaders are useless. The power of this library is that you can chain these loaders into one big, flexible and powerfull loader. 

[PICTURE]

When you request some specific image of specific size from first loader in chain, it will try to load this image from it's own storage, and here, several situations are possible:

1. Image not found. The loader will request the image from fallback loader in chain. 
2. Image is found but has smaller size than requested. The loader will return this image immediatly (it is better to display something than nothing) and in parallel will request image of requested size from fallback loader in chain.
3. Image is found but has larger size than requested. The loader will resize it to reqeusted size and will return it. No need to bother next loaders in chain.

When fallback loader returns image, the loader saves it into its own storage and returns it. So, as you can see, when you request image from loader, you may receive several images but with diffrent size (from small size to requested). It was made to provide any result as fast as possible. Instead of blank image we can display same image with smaller size from IsolatedStorage (of course if it exists there), while original image is loading from the Internet.

### Usage

To be continued...

## Copyright

Copyright (c) 2013 Sergii Vashchyshchuk
