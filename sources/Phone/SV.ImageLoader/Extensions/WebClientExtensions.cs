
namespace SV.ImageLoader.Extensions
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public static class WebClientExtensions
    {
        public static Task<Byte[]> DownloadDataTaskAsync(this WebClient client, Uri address)
        {
            var completerionSource = new TaskCompletionSource<byte[]>();
            var token = new object();

            OpenReadCompletedEventHandler handler = null;
            handler = (sender, args) =>
            {
                if (args.UserState != token)
                {
                    return;
                }

                if (args.Error != null)
                {
                    completerionSource.SetException(args.Error);
                }
                else if (args.Cancelled)
                {
                    completerionSource.SetCanceled();
                }
                else
                {                    
                    completerionSource.SetResult(args.Result.ToArray());
                }

                client.OpenReadCompleted -= handler;
            };

            client.OpenReadCompleted += handler;
            client.OpenReadAsync(address, token);

            return completerionSource.Task;
        }

    }
}
