using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace Misuzilla.KanColleViewer.Plugins.AzureMobileServicesNotifier.MobileServices
{
    /// <summary>
    /// ブラウザーでAzure Mobile Servicesの認証画面を開いて認証行うためのクラスです
    /// </summary>
    internal class MobileServiceBrowserAuthentication : MobileServiceAuthentication
    {
        public MobileServiceBrowserAuthentication(IMobileServiceClient client, string providerName)
            : base(client, providerName)
        { }

        protected override Task<string> LoginAsyncOverride()
        {
            var taskCompletionSource = new TaskCompletionSource<string>();

            var webBrowser = Activator.CreateInstance(Type.GetTypeFromProgID("InternetExplorer.Application")) as SHDocVw.InternetExplorer;
            webBrowser.OnQuit += () => { taskCompletionSource.TrySetCanceled(); };
            webBrowser.NavigateComplete2 += (object pDisp, ref object URL) =>
            {
                var result = URL as string;
                if (result.StartsWith(EndUri.ToString()))
                {
                    var m = Regex.Match(result, "token=([^&]+)");
                    if (m.Success)
                    {
                        taskCompletionSource.TrySetResult(
                            Uri.UnescapeDataString(m.Groups[1].Value));
                    }
                    else
                    {
                        taskCompletionSource.TrySetCanceled();
                    }
                }
            };
            webBrowser.Navigate(StartUri.ToString());
            webBrowser.Visible = true;

            taskCompletionSource.Task.ContinueWith(_ => webBrowser.Quit());

            return taskCompletionSource.Task;
        }
    }
}
