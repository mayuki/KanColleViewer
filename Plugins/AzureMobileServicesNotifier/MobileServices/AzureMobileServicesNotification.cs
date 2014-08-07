using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.WindowsAzure.MobileServices;

namespace Misuzilla.KanColleViewer.Plugins.AzureMobileServicesNotifier.MobileServices
{
    public class AzureMobileServicesNotification
    {
        private const String ApplicationUrl = "";
        private const String AppKey = "";

        private MobileServiceClient _client;
        private ISubject<Unit> _pushQueue = new Subject<Unit>();

        /// <summary>
        /// ログインする必要があるかどうかを返します。
        /// </summary>
        public Boolean IsLoginRequired
        {
            get { return _client == null; }
        }

        public AzureMobileServicesNotification()
        {
            _pushQueue
                .Sample(TimeSpan.FromSeconds(10))
                .Subscribe(_ => Push().FireAndForget());
        }

        /// <summary>
        /// Azure Mobile Servicesに通知の情報を追加します。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <param name="description"></param>
        /// <param name="completionTime"></param>
        /// <returns></returns>
        public async Task Upsert(AzureMobileNotificationType type, int id, string description, DateTimeOffset? completionTime)
        {
            if (_client == null)
            {
                return;
            }
            await _client.InvokeApiAsync<AzureMobileNotificationItem>("NotificationItem/" + id,
                HttpMethod.Post,
                new Dictionary<String, String>()
                {
                    { "type", ((Int32)type).ToString() },
                    { "description", description },
                    { "completionTimeUtc", completionTime.HasValue ? completionTime.Value.ToUniversalTime().ToString("r") : "" },
                }
            );

            _pushQueue.OnNext(Unit.Default);
        }

        /// <summary>
        /// Azure Mobile Servicesから通知情報を削除します。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task Delete(AzureMobileNotificationType type, int id)
        {
            if (_client == null)
            {
                return;
            }
            await _client.InvokeApiAsync("NotificationItem/" + id,
                HttpMethod.Delete,
                new Dictionary<String, String>()
                {
                    { "type", ((Int32)type).ToString() },
                }
            );

            _pushQueue.OnNext(Unit.Default);
        }

        /// <summary>
        /// 保存されているデータからクライアントを生成ます。
        /// </summary>
        /// <returns></returns>
        public async Task InitializeClientFromSavedData()
        {
            // TODO: 仮
            await InitializeClientWithAuthenticate();
        }

        /// <summary>
        /// 認証を開いて認証情報を永続化し、クライアントを生成します。
        /// </summary>
        /// <returns></returns>
        public async Task InitializeClientWithAuthenticate()
        {
            var client = new MobileServiceClient(ApplicationUrl, AppKey);
            var auth = new MobileServiceBrowserAuthentication(client, "google");

            var user = await auth.LoginAsync();

            _client = client;
        }

        /// <summary>
        /// プッシュ通知をクライアントに送信します
        /// </summary>
        /// <returns></returns>
        public async Task Push()
        {
            await _client.InvokeApiAsync("Push",
                HttpMethod.Post,
                new Dictionary<String, String>()
            );
        }
    }
}
