
namespace SV.ImageLoader.Extensions
{
    using System.Net;
    using System.Threading.Tasks;

    public static class WebRequestExtensions
    {
        public static Task<WebResponse> GetResponseAsync(this WebRequest request)
        {
            var completerionSource = new TaskCompletionSource<WebResponse>();

            request.BeginGetResponse(asyncResponse =>
                {
                    try
                    {
                        var r = (WebRequest)asyncResponse.AsyncState;
                        var response = r.EndGetResponse(asyncResponse);

                        completerionSource.TrySetResult(response);
                    }
                    catch (WebException ex)
                    {
                        completerionSource.TrySetException(ex);

                    }
                }, request);

            return completerionSource.Task;
        }
    }
}
